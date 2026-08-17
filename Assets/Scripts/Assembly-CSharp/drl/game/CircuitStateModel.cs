using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class CircuitStateModel : Model<DRLApp>
	{
		public class CircuitsProgressData : SerializedData
		{
			private List<float> m_times;

			private List<string> m_replays;

			public string circuitId
			{
				get
				{
					return Get("circuit-id", "");
				}
				set
				{
					Set("circuit-id", value);
				}
			}

			public string circuitName
			{
				get
				{
					return Get("circuit-name", "");
				}
				set
				{
					Set("circuit-name", value);
				}
			}

			public int progress
			{
				get
				{
					return Get("circuit-progress", 0);
				}
				set
				{
					Set("circuit-progress", value);
				}
			}

			public int attempts
			{
				get
				{
					return Get("circuit-attempts", 0);
				}
				set
				{
					Set("circuit-attempts", value);
				}
			}

			public float time
			{
				get
				{
					if (times == null || times.Count == 0)
					{
						return 0f;
					}
					return times.Sum();
				}
			}

			public bool finished
			{
				get
				{
					return Get("has-finished", d: false);
				}
				set
				{
					Set("has-finished", value);
				}
			}

			public string timesData
			{
				get
				{
					return Get("circuit-times", "");
				}
				set
				{
					Set("circuit-times", value);
				}
			}

			public List<float> times
			{
				get
				{
					if (m_times == null)
					{
						m_times = new List<float>();
					}
					if (string.IsNullOrEmpty(timesData))
					{
						return m_times;
					}
					m_times = Serialize.FromJson<float[]>(timesData).ToList();
					return m_times;
				}
				set
				{
					m_times = value;
					if (m_times != null)
					{
						timesData = Serialize.ToJson(m_times.ToArray());
					}
				}
			}

			public string replaysData
			{
				get
				{
					return Get("replay-urls", "");
				}
				set
				{
					Set("replay-urls", value);
				}
			}

			public List<string> replays
			{
				get
				{
					if (m_replays == null)
					{
						m_replays = new List<string>();
					}
					if (string.IsNullOrEmpty(replaysData))
					{
						return m_replays;
					}
					m_replays = Serialize.FromJson<string[]>(replaysData).ToList();
					return m_replays;
				}
				set
				{
					m_replays = value;
					if (m_replays != null)
					{
						replaysData = Serialize.ToJson(m_replays.ToArray());
					}
				}
			}

			public bool drlOfficial
			{
				get
				{
					return Get("drl-official", d: true);
				}
				set
				{
					Set("drl-official", value);
				}
			}
		}

		public Dictionary<string, Texture2D> circuitThumbnailsCache = new Dictionary<string, Texture2D>();

		private DRLCircuitData[] m_circuits;

		private List<CircuitsProgressData> m_circuitsProgress;

		private bool m_inProgress;

		[HideInInspector]
		public int circuitTrackIndex;

		public PlayerStateModel parent => AssertParent<PlayerStateModel>("parent");

		public DataFlow data => parent.data;

		public CircuitsOpponentMode opponentMode
		{
			get
			{
				return (CircuitsOpponentMode)data.Get("circuits-opponent-mode", 0);
			}
			set
			{
				data.Set("circuits-opponent-mode", (int)value);
				Refresh();
			}
		}

		public CircuitsOpponentDifficulty opponentDifficulty
		{
			get
			{
				return (CircuitsOpponentDifficulty)data.Get("circuits-opponent-difficulty", 0);
			}
			set
			{
				data.Set("circuits-opponent-difficulty", (int)value);
				Refresh();
			}
		}

		public DRLCircuitData activeCircuit { get; set; }

		public DRLCircuitData[] circuits
		{
			get
			{
				if (m_circuits != null)
				{
					return m_circuits;
				}
				string p_data = circuitsData;
				m_circuits = Serialize.FromJson<DRLCircuitData[]>(p_data);
				return m_circuits;
			}
			private set
			{
				m_circuits = value;
				if (m_circuits != null)
				{
					circuitsData = Serialize.ToJson(m_circuits);
				}
			}
		}

		public string circuitsData
		{
			get
			{
				return data.Get("circuits-data", "");
			}
			set
			{
				data.Set("circuits-data", value);
				Refresh();
			}
		}

		public string circuitsProgressData
		{
			get
			{
				return data.Get("circuits-progress", "");
			}
			set
			{
				data.Set("circuits-progress", value);
				Refresh();
			}
		}

		public List<CircuitsProgressData> circuitProgress
		{
			get
			{
				if (m_circuitsProgress == null)
				{
					m_circuitsProgress = new List<CircuitsProgressData>();
				}
				CircuitsProgressData[] array = Serialize.FromJson<CircuitsProgressData[]>(circuitsProgressData);
				if (array != null)
				{
					m_circuitsProgress.Clear();
					m_circuitsProgress = array.ToList();
				}
				return m_circuitsProgress;
			}
			set
			{
				m_circuitsProgress = value;
				if (m_circuitsProgress != null)
				{
					string text = Serialize.ToJson(m_circuitsProgress.ToArray());
					circuitsProgressData = text;
				}
			}
		}

		public bool inProgress
		{
			get
			{
				if (m_inProgress && !base.app.inTournament)
				{
					return !base.app.inMultiplayer;
				}
				return false;
			}
			set
			{
				m_inProgress = value;
			}
		}

		public void RefreshCircuitData(Action p_on_complete = null)
		{
			if (!base.validContext || DRLApp.offline || DRLApp.forceOffline)
			{
				return;
			}
			base.app.model.service.GetCircuitsData(delegate(DRLCircuitData[] p_result)
			{
				if (p_result != null && p_result.Length != 0)
				{
					circuits = p_result;
					CacheCircuitThumbnails(p_result);
				}
				if (p_on_complete != null)
				{
					p_on_complete();
				}
			});
		}

		public void Refresh()
		{
			if ((bool)parent)
			{
				parent.Refresh();
			}
		}

		public void SetInProgress(DRLCircuitData p_data, int p_trackIdx)
		{
			if (p_data != null)
			{
				inProgress = true;
				activeCircuit = p_data;
				circuitTrackIndex = p_trackIdx;
			}
		}

		public void ClearInProgress()
		{
			inProgress = false;
			activeCircuit = null;
			circuitTrackIndex = 0;
		}

		public CircuitsProgressData GetCircuitProgress(string p_circuitId = "")
		{
			if (string.IsNullOrEmpty(p_circuitId) && activeCircuit == null)
			{
				return null;
			}
			if (string.IsNullOrEmpty(p_circuitId))
			{
				p_circuitId = activeCircuit.guid;
			}
			List<CircuitsProgressData> list = circuitProgress;
			if (list == null || list.Count == 0)
			{
				return null;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].circuitId == p_circuitId)
				{
					return list[i];
				}
			}
			return null;
		}

		public void ResetProgress(string p_circuitId)
		{
			if (string.IsNullOrEmpty(p_circuitId))
			{
				return;
			}
			CircuitsProgressData circuitsProgressData = GetCircuitProgress(p_circuitId);
			if (circuitsProgressData == null)
			{
				circuitsProgressData = new CircuitsProgressData
				{
					circuitId = p_circuitId
				};
				if (activeCircuit != null)
				{
					circuitsProgressData.circuitName = activeCircuit.name;
				}
			}
			circuitsProgressData.finished = false;
			circuitsProgressData.progress = 0;
			circuitsProgressData.times = new List<float>();
			circuitsProgressData.drlOfficial = true;
			SetCircuitProgress(circuitsProgressData, p_reset: true);
		}

		public void SetCircuitProgress(CircuitsProgressData p_progressData, bool p_reset = false)
		{
			if (p_progressData == null)
			{
				return;
			}
			List<CircuitsProgressData> list = new List<CircuitsProgressData>();
			List<CircuitsProgressData> list2 = circuitProgress;
			DRLCircuitData dRLCircuitData = circuits.First((DRLCircuitData o) => o.guid == p_progressData.circuitId);
			if (dRLCircuitData == null)
			{
				return;
			}
			bool flag = false;
			for (int num = 0; num < list2.Count; num++)
			{
				list.Add(list2[num]);
				if (list2[num].circuitId == p_progressData.circuitId)
				{
					if (p_progressData.progress >= dRLCircuitData.trackCount)
					{
						list[num].progress = dRLCircuitData.trackCount;
						list[num].finished = true;
						list[num].attempts++;
					}
					else
					{
						list[num].progress = p_progressData.progress;
						list[num].finished = false;
					}
					list[num].times = p_progressData.times;
					list[num].replays = p_progressData.replays;
					list[num].drlOfficial = p_reset || (list[num].drlOfficial && p_progressData.drlOfficial);
					flag = true;
				}
			}
			if (!flag)
			{
				list.Add(p_progressData);
			}
			circuitProgress = list;
		}

		public DRLCircuitData GetTryoutsCircuit()
		{
			DRLCircuitData result = null;
			for (int i = 0; i < circuits.Length; i++)
			{
				if (circuits[i] != null && circuits[i].ContainsTag(DRLCircuitData.Tag.tryouts))
				{
					result = circuits[i];
					break;
				}
			}
			return result;
		}

		public void SetCircuitReplay(string p_replayURL)
		{
			if (string.IsNullOrEmpty(p_replayURL))
			{
				return;
			}
			CircuitsProgressData circuitsProgressData = GetCircuitProgress();
			if (circuitsProgressData != null)
			{
				int num = circuitsProgressData.progress - 1;
				if (num < 0)
				{
					num = 0;
				}
				List<string> replays = circuitsProgressData.replays;
				if (replays.Count <= num)
				{
					replays.Add(p_replayURL);
				}
				else
				{
					replays[num] = p_replayURL;
				}
				circuitsProgressData.replays = replays;
				SetCircuitProgress(circuitsProgressData);
			}
		}

		private async void CacheCircuitThumbnails(DRLCircuitData[] p_circuits)
		{
			if (!DRLApp.offline)
			{
				circuitThumbnailsCache.Clear();
				for (int i = 0; i < p_circuits.Length; i++)
				{
					await DownloadAndStoreThumbnail(p_circuits[i].guid, p_circuits[i].imageURL);
				}
			}
		}

		private async Task DownloadAndStoreThumbnail(string p_circuitId, string p_url, Action<Texture2D> p_onComplete = null)
		{
			if (string.IsNullOrEmpty(p_circuitId) || string.IsNullOrEmpty(p_url))
			{
				Debug.LogWarning("CircuitStateModel> DownloadAndStoreThumbnail / guid[" + p_circuitId + "] url[" + p_url + "] result[Invalid Circuit or URL]");
				p_onComplete?.Invoke(null);
				return;
			}
			if (circuitThumbnailsCache.ContainsKey(p_circuitId))
			{
				Debug.Log("CircuitStateModel> DownloadAndStoreThumbnail / guid[" + p_circuitId + "] url[" + p_url + "] result[CACHED]");
				p_onComplete?.Invoke(circuitThumbnailsCache[p_circuitId]);
				return;
			}
			UnityWebRequest uwr = new UnityWebRequest(p_url);
			uwr.downloadHandler = new DownloadHandlerTexture();
			uwr.SendWebRequest();
			bool is_error = false;
			while (!uwr.isDone)
			{
				await Task.Delay(1);
			}
			UnityWebRequest.Result result = uwr.result;
			if ((uint)(result - 2) <= 2u)
			{
				is_error = true;
			}
			Debug.Log(string.Format("CircuitStateModel> DownloadAndStoreThumbnail / guid[{0}] url[{1}] result[{2}]{3}", p_circuitId, p_url, uwr.result, is_error ? ("\n" + uwr.error) : ""));
			Texture2D texture2D = (is_error ? null : ((DownloadHandlerTexture)uwr.downloadHandler).texture);
			if (texture2D == null)
			{
				p_onComplete?.Invoke(null);
				return;
			}
			circuitThumbnailsCache.Add(p_circuitId, texture2D);
			File.WriteAllBytes(DRLPaths.Storage.offlineCircuitsRoot + p_circuitId + ".png", texture2D.EncodeToPNG());
			uwr.downloadHandler.Dispose();
			uwr.Dispose();
		}

		public void GetCircuitThumbnail(DRLCircuitData p_circuit, Action<Texture2D> p_callback)
		{
			if (p_circuit == null || string.IsNullOrEmpty(p_circuit.guid) || string.IsNullOrEmpty(p_circuit.imageURL))
			{
				p_callback?.Invoke(null);
				return;
			}
			if (circuitThumbnailsCache.ContainsKey(p_circuit.guid))
			{
				p_callback?.Invoke(circuitThumbnailsCache[p_circuit.guid]);
				return;
			}
			base.app.model.storage.LoadImageLocally(DRLPaths.Storage.offlineCircuitsRoot + p_circuit.guid + ".png", 460, 630, delegate(Texture texture)
			{
				if (texture != null)
				{
					p_callback?.Invoke((Texture2D)texture);
				}
				else if (DRLApp.offline)
				{
					p_callback?.Invoke(null);
				}
				else
				{
					_ = DownloadAndStoreThumbnail(p_circuit.guid, p_circuit.imageURL, delegate(Texture2D texture2D)
					{
						p_callback?.Invoke(texture2D);
					});
				}
			});
		}
	}
}
