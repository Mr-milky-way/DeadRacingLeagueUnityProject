using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using drl.backend;
using drl.core;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class OpponentModel : Model<DRLApp>
	{
		public enum Status
		{
			None = 0,
			ManifestSuccess = 1,
			Progress = 2,
			Complete = 3,
			NoResults = 4,
			ByPass = 5,
			Error = 6
		}

		public List<WebAsyncRequest> requests = new List<WebAsyncRequest>();

		public WebAsyncRequest dataRequest;

		public List<byte[]> replayFiles;

		public List<BlackboxRecord> replayOpponents;

		public List<ReplayFile> replayV2Opponents;

		public byte[] replayV2OpponentsBytes;

		public Status status;

		public float progress;

		public BlackboxRecord ghostRecords;

		public ReplayRecord ghostRecordsV2;

		private bool m_thread_kill;

		private Thread m_process_thread;

		private Action m_replay_step;

		private Activity m_replay_thread_complete;

		private OpponentRequest m_last_request;

		private readonly List<BlackboxRecord> _loadedReplays = new List<BlackboxRecord>();

		public readonly List<ReplayFile> _loadedReplaysV2 = new List<ReplayFile>();

		public void Load(OpponentModeType p_mode, DRLMap p_map, DRLMapTrack p_track, int p_count, int p_drone_class, bool p_drone_official, bool p_custom_physics, Action p_callback, string p_circuitId = null, int p_circuitDifficulty = -1)
		{
			ServiceModel service = base.app.model.service;
			m_last_request = new OpponentRequest
			{
				mode = p_mode,
				map = p_map,
				track = p_track,
				count = p_count,
				droneClass = p_drone_class,
				droneOfficial = p_drone_official,
				customPhysics = p_custom_physics,
				circuitId = p_circuitId,
				circuitDifficulty = p_circuitDifficulty
			};
			OpponentModeType opponentModeType = p_mode;
			if (opponentModeType != OpponentModeType.Off && (uint)(opponentModeType - 1) <= 5u)
			{
				dataRequest = service.GetReplayRivals(p_map, p_track, p_count, p_drone_class, p_drone_official, p_custom_physics, delegate(DRLLeaderboardRivalsResult p_list)
				{
					OnReplayManifest(p_list, p_mode, p_callback);
				}, -1, null, p_circuitId, p_circuitDifficulty);
			}
		}

		public void Refresh(Action<bool> p_callback)
		{
			OpponentRequest last_request = m_last_request;
			Debug.Log("OpponentModel> Refresh / " + ((last_request == null) ? "<null>" : $"mode[{last_request.mode}] count[{last_request.count}]"));
			if (last_request == null || last_request.mode == OpponentModeType.Off)
			{
				if (p_callback != null)
				{
					p_callback(obj: false);
				}
				return;
			}
			Action p_callback2 = delegate
			{
				switch (status)
				{
				case Status.Complete:
					p_callback(obj: true);
					break;
				case Status.Error:
					p_callback(obj: false);
					break;
				}
			};
			Load(last_request.mode, last_request.map, last_request.track, last_request.count, last_request.droneClass, last_request.droneOfficial, last_request.customPhysics, p_callback2, last_request.circuitId, last_request.circuitDifficulty);
		}

		public void Load(string p_replayId, OnboardingCampaignMode onboardingCampaignMode, Action p_callback)
		{
			OnOnboardingReplayManifest(p_replayId, p_callback);
		}

		public void Load(DRLLeaderboardRivalsResult p_list, OpponentModeType p_mode, Action p_callback)
		{
			if (base.validContext)
			{
				OnReplayManifest(p_list, p_mode, p_callback);
			}
		}

		private IEnumerator DownloadReplay(string p_replay_url, Action<bool, float, byte[]> p_progress)
		{
			using UnityWebRequest req = UnityWebRequest.Get(p_replay_url);
			UnityWebRequestAsyncOperation operation = req.SendWebRequest();
			while (!operation.isDone && !req.isNetworkError)
			{
				p_progress?.Invoke(arg1: false, req.downloadProgress, null);
				yield return null;
			}
			if (req.isNetworkError)
			{
				p_progress?.Invoke(arg1: false, -1f, null);
			}
			else
			{
				p_progress?.Invoke(arg1: true, 1f, req.downloadHandler.data);
			}
		}

		private async void StartReplayDownload(string p_replay_url, Action<float, byte[]> p_progress)
		{
			float p = 0f;
			Task<byte[]> res = DownloadReplayAsync(p_replay_url);
			while (!res.IsCompleted)
			{
				await Task.Delay(100);
				if (p < 0.95f)
				{
					p += 0.01f;
				}
				p_progress?.Invoke(p, null);
			}
			p_progress?.Invoke(1f, res.Result);
		}

		private Task<byte[]> DownloadReplayAsync(string p_replay_url)
		{
			using HttpClient httpClient = new HttpClient();
			return httpClient.GetByteArrayAsync(p_replay_url);
		}

		public void Load(string[] p_replays, int p_max_count, Action p_callback)
		{
			if (p_replays == null)
			{
				p_replays = new string[0];
			}
			p_max_count = Mathf.Max(p_max_count, 0);
			int num = 5;
			List<string> list = new List<string>(p_replays);
			while (list.Count > num)
			{
				list.RemoveAt(0);
			}
			p_replays = list.ToArray();
			SetStatus(p_callback, Status.ManifestSuccess, 0f);
			Debug.Log("OpponentModel> Load / List\n" + string.Join("\n", p_replays));
			List<string> obj = ((p_replays != null) ? p_replays.ToList() : new List<string>());
			obj.RemoveAll((string v) => string.IsNullOrEmpty(v));
			if (obj.Count <= 0)
			{
				SetStatus(p_callback, Status.NoResults, 0f);
				Cancel();
				return;
			}
			SetStatus(p_callback, Status.Progress, 0f);
			int replay_idx = 0;
			float replay_count = p_replays.Length;
			float replay_len = 0f;
			if (ReplayFile.EnableVersion2)
			{
				if (replayV2Opponents == null)
				{
					replayV2Opponents = new List<ReplayFile>();
				}
				replayV2Opponents.Clear();
				if (ghostRecordsV2 != null)
				{
					ghostRecordsV2.Destroy();
				}
				ghostRecordsV2 = null;
			}
			else
			{
				replayOpponents.Clear();
				ghostRecords = null;
			}
			DateTime t0 = DateTime.Now;
			DateTime gc_t0 = DateTime.Now;
			TimeSpan gc_dt = DateTime.Now - t0;
			Debug.Log("OpponentModel> Load / Replay Batch Start");
			DRLApp.LogMemStats("OpponentModel> Load / Batch Start", p_show_delta: false);
			TimeSpan dt;
			m_replay_step = delegate
			{
				if (m_thread_kill)
				{
					m_thread_kill = false;
				}
				else if (replay_idx >= p_replays.Length)
				{
					SetStatus(p_callback, Status.Progress, 1f);
					dt = DateTime.Now - t0;
					Debug.LogWarning(string.Format("OpponentModel> Load / Batch Complete - {0} files - {1}s - {2}mb", p_replays.Length, dt.TotalSeconds.ToString("0"), replay_len.ToString("0.0")));
					if (ReplayFile.EnableVersion2)
					{
						ghostRecordsV2 = new ReplayRecord();
						ghostRecordsV2.replays.AddRange(replayV2Opponents);
					}
					else
					{
						BlackboxRecord p_record = BlackboxRecord.Merge(replayOpponents);
						ghostRecords = p_record;
						TryAddLoadedReplay(p_record);
						replayOpponents.Clear();
					}
					Activity.RunOnce(delegate
					{
						DRLApp.LogMemStats("OpponentModel> Load / Batch Complete", p_show_delta: true);
					}, 4f);
					SetStatus(p_callback, Status.Complete, 1f);
					if (p_callback != null)
					{
						p_callback();
					}
				}
				else
				{
					DateTime d0 = DateTime.UtcNow;
					string replay_url = p_replays[replay_idx];
					if (string.IsNullOrEmpty(replay_url))
					{
						Debug.LogWarning($"OpponentModel> Load / Replay {replay_idx} is Invalid!");
						replay_idx++;
						if (m_replay_step != null)
						{
							m_replay_step();
						}
					}
					else
					{
						DRLApp.LogMemStats($"OpponentModel> Load / Replay {replay_idx} Load Start!", p_show_delta: true);
						WebAsyncRequest webAsyncRequest = null;
						webAsyncRequest = Web.Get(replay_url, delegate(byte[] p_data, float p_progress, WebAsyncRequest p_request)
						{
							float num2 = Mathf.Lerp(0f, 0.5f, p_progress);
							SetStatus(p_callback, Status.Progress, ((float)replay_idx + num2) / replay_count);
							if (!(p_progress < 1f))
							{
								if (requests.Contains(p_request))
								{
									requests.Remove(p_request);
								}
								if (p_data == null)
								{
									Debug.LogWarning($"OpponentModel> Load / Replay {replay_idx} is Null!\n{replay_url}");
									int num3 = replay_idx;
									replay_idx = num3 + 1;
									if (m_replay_step != null)
									{
										m_replay_step();
									}
								}
								else
								{
									float num4 = (float)p_data.Length / 1024f / 1024f;
									DRLApp.LogMemStats($"OpponentModel> Load / Replay {replay_idx} Load Complete - {num4}mb", p_show_delta: true);
									Debug.Log($"OpponentModel> Load / Replay {replay_idx} from URL {replay_url} download finished in [{(DateTime.UtcNow - d0).TotalSeconds}]s file size [{(float)p_data.Length / 1048576f}]MB");
									replay_len += num4;
									p_request.loader.Dispose();
									Activity time_loop = null;
									time_loop = Activity.Run((Func<bool>)delegate
									{
										if (time_loop == null)
										{
											return false;
										}
										float num5 = 0.5f + Mathf.Lerp(0f, 0.5f, Mathf.Clamp01(time_loop.elapsed / 4f));
										SetStatus(p_callback, Status.Progress, ((float)replay_idx + num5) / replay_count);
										return time_loop.elapsed < 4f;
									}, 0f, false);
									Thread thread = (m_process_thread = new Thread((ThreadStart)delegate
									{
										if (m_thread_kill)
										{
											m_thread_kill = false;
										}
										else
										{
											if (ReplayFile.EnableVersion2)
											{
												replayV2OpponentsBytes = p_data;
												ReplayFile replayFile = ReplayFile.FromBytes(p_data);
												TryAddLoadedReplayV2(replayFile);
												if (replayFile.duration != 0f)
												{
													replayV2Opponents.Add(replayFile);
												}
											}
											else
											{
												BlackboxRecord blackboxRecord = null;
												blackboxRecord = Serialize.FromBytes<BlackboxRecord>(p_data, p_unsafe: true);
												blackboxRecord.Decompress();
												blackboxRecord.Prune();
												replayOpponents.Add(blackboxRecord);
												TryAddLoadedReplay(blackboxRecord);
											}
											if (m_thread_kill)
											{
												m_thread_kill = false;
											}
											else
											{
												m_process_thread = null;
												m_replay_thread_complete = Activity.RunOnce(delegate
												{
													if (m_thread_kill)
													{
														m_thread_kill = false;
													}
													else
													{
														if (time_loop != null)
														{
															time_loop.Stop();
															time_loop = null;
														}
														SetStatus(p_callback, Status.Progress, ((float)replay_idx + 1f) / replay_count);
														DRLApp.LogMemStats($"OpponentModel> Load / Replay {replay_idx} Parse Complete", p_show_delta: true);
														m_replay_thread_complete = Activity.RunOnce(delegate
														{
															m_replay_thread_complete = null;
															gc_t0 = DateTime.Now;
															GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
															GC.Collect(1);
															GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.Default;
															gc_dt = DateTime.Now - gc_t0;
															DRLApp.LogMemStats($"GC LOH {gc_dt.TotalMilliseconds}ms", p_show_delta: true);
															if (m_thread_kill)
															{
																m_thread_kill = false;
															}
															else
															{
																replay_idx++;
																if (m_replay_step != null)
																{
																	m_replay_step();
																}
															}
														}, 1f);
													}
												});
											}
										}
									}));
									DRLApp.LogMemStats($"OpponentModel> Load / Replay {replay_idx} Parse Start", p_show_delta: true);
									thread.Priority = System.Threading.ThreadPriority.Highest;
									thread.Start();
								}
							}
						});
						requests.Add(webAsyncRequest);
					}
				}
			};
			m_replay_step();
		}

		public void ClearGhosts()
		{
			ghostRecords = null;
		}

		protected void SetStatus(Action p_callback, Status p_status, float p_progress)
		{
			status = p_status;
			progress = p_progress;
			p_callback?.Invoke();
		}

		protected void OnReplayManifest(DRLLeaderboardRivalsResult p_list, OpponentModeType p_mode, Action p_callback)
		{
			if (!base.validContext)
			{
				return;
			}
			if (p_list == null)
			{
				SetStatus(p_callback, Status.Error, 0f);
				Cancel();
				return;
			}
			int num = ((p_list.top != null) ? p_list.top.Length : 0);
			int num2 = ((p_list.rivals != null) ? p_list.rivals.Length : 0);
			Debug.Log($"OpponentModel> OnReplayManifest\n  Top: {num}\n  Rivals: {num2}");
			SetStatus(p_callback, Status.ManifestSuccess, 0f);
			string[] array = new string[0];
			int p_max_count = 0;
			switch (p_mode)
			{
			case OpponentModeType.Top5:
				array = p_list.GetTopReplays();
				p_max_count = 5;
				break;
			case OpponentModeType.Leader:
				array = p_list.GetTopReplays(p_include_player: true);
				p_max_count = 1;
				break;
			case OpponentModeType.Rival5:
				array = p_list.GetRivalReplays();
				p_max_count = 5;
				break;
			case OpponentModeType.Self:
				array = p_list.GetPastReplays();
				p_max_count = 1;
				break;
			case OpponentModeType.Random5:
			case OpponentModeType.Random50:
			{
				List<string> list = new List<string>();
				int num3 = ((p_list.rivals != null) ? p_list.rivals.Length : 0);
				array = p_list.GetReplays(p_list.top, p_player_only: false, p_list.player);
				if (num3 > 0 && p_mode == OpponentModeType.Random50)
				{
					array = new string[0];
				}
				list.AddRange(array);
				array = p_list.GetReplays(p_list.rivals, p_player_only: false, p_list.player);
				list.AddRange(array);
				list.Shuffle();
				int num4 = Mathf.Min(list.Count, 5);
				array = new string[num4];
				for (int i = 0; i < num4; i++)
				{
					array[i] = list[i];
				}
				p_max_count = 5;
				break;
			}
			}
			ForceResetLoadedReplays();
			RunOnce(2f, delegate
			{
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			});
			Load(array, p_max_count, p_callback);
		}

		private void OnOnboardingReplayManifest(string p_result, Action p_callback)
		{
			if (base.validContext)
			{
				if (p_result == null || p_result.Length == 0)
				{
					SetStatus(p_callback, Status.Error, 0f);
					Cancel();
					return;
				}
				Debug.Log("OpponentModel> OnReplayManifest\n" + p_result);
				SetStatus(p_callback, Status.ManifestSuccess, 0f);
				string[] p_replays = new string[1] { p_result };
				Load(p_replays, 1, p_callback);
			}
		}

		protected void AbortProcessingThreads()
		{
			if (m_process_thread != null)
			{
				m_thread_kill = true;
			}
		}

		public void Cancel()
		{
			if (dataRequest != null)
			{
				dataRequest.Cancel();
				dataRequest = null;
			}
			m_replay_step = null;
			if (m_replay_thread_complete != null)
			{
				m_replay_thread_complete.Stop();
				m_replay_thread_complete = null;
			}
			ghostRecords = null;
			progress = 0f;
			status = Status.None;
			AbortProcessingThreads();
			if (requests == null)
			{
				requests = new List<WebAsyncRequest>();
			}
			for (int i = 0; i < requests.Count; i++)
			{
				requests[i].Cancel();
			}
			requests.Clear();
			if (replayFiles == null)
			{
				replayFiles = new List<byte[]>();
			}
			replayFiles.Clear();
			if (replayOpponents == null)
			{
				replayOpponents = new List<BlackboxRecord>();
			}
			replayOpponents.Clear();
			if (replayV2Opponents == null)
			{
				replayV2Opponents = new List<ReplayFile>();
			}
			replayV2Opponents.Clear();
		}

		public void TryAddLoadedReplay(BlackboxRecord p_record)
		{
			if (!_loadedReplays.Contains(p_record))
			{
				_loadedReplays.Add(p_record);
			}
		}

		public void TryAddLoadedReplayV2(ReplayFile p_replay)
		{
			if (!_loadedReplaysV2.Contains(p_replay))
			{
				_loadedReplaysV2.Add(p_replay);
			}
		}

		public void ForceResetLoadedReplays()
		{
			if (ReplayFile.EnableVersion2)
			{
				foreach (ReplayFile item in _loadedReplaysV2)
				{
					item.Destroy();
				}
				return;
			}
			foreach (BlackboxRecord loadedReplay in _loadedReplays)
			{
				if (loadedReplay == null)
				{
					continue;
				}
				foreach (BlackboxData clip in loadedReplay.clips)
				{
					if (clip == null)
					{
						continue;
					}
					BlackboxFrame[] frames = clip.frames;
					for (int i = 0; i < frames.Length; i++)
					{
						frames[i].data = new object[0];
					}
					clip.frames = new BlackboxFrame[0];
					if (clip.tracks != null)
					{
						foreach (KeyValuePair<byte, List<BlackboxFrame>> track in clip.tracks)
						{
							foreach (BlackboxFrame item2 in track.Value)
							{
								item2.data = new object[0];
							}
							track.Value.Clear();
						}
					}
					clip.ClearTrackTable();
					clip.compressedFrames = new byte[0];
				}
				loadedReplay.ClearFrames();
				loadedReplay.Clear();
			}
			_loadedReplays.Clear();
		}
	}
}
