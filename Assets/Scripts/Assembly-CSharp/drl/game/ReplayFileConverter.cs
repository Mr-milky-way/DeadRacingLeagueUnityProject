using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class ReplayFileConverter : ScriptableObject
	{
		public static bool IsDRLDevAPI = true;

		public static string AMAZON_ACCESS_KEY = "";

		public static string AMAZON_SECRET_KEY = "";

		public static string AMAZON_S3_BUCKET = "";

		public static string AMAZON_S3_REPLAYS_KEY = "";

		public static string AMAZON_S3_REPLAYS_V2_KEY = "";

		public static string AMAZON_S3_RACES_KEY = "";

		public static string AMAZON_S3_RACES_V2_KEY = "";

		public static string AMAZON_S3_ROOT = "";

		private static System.Random rnd = new System.Random();

		public List<ReplayConvertJob> jobs;

		public static string DRLAPIRoot
		{
			get
			{
				if (!IsDRLDevAPI)
				{
					return "https://api.drlgame.com/";
				}
				return "https://api-dev.drlgame.com/";
			}
		}

		public int jobsCompleteCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < jobs.Count; i++)
				{
					num += ((jobs[i].state != ReplayConvertJobState.Error) ? 1 : 0);
				}
				return num;
			}
		}

		public int jobsV1LengthKb
		{
			get
			{
				int num = 0;
				for (int i = 0; i < jobs.Count; i++)
				{
					num += ((jobs[i].state != ReplayConvertJobState.Error) ? jobs[i].replayV1LengthKb : 0);
				}
				return num;
			}
		}

		public int jobsV2LengthKb
		{
			get
			{
				int num = 0;
				for (int i = 0; i < jobs.Count; i++)
				{
					num += ((jobs[i].state != ReplayConvertJobState.Error) ? jobs[i].replayV2LengthKb : 0);
				}
				return num;
			}
		}

		public static ReplayFile ConvertToReplayV2(BlackboxData p_data)
		{
			if (p_data == null)
			{
				UnityEngine.Debug.LogWarning("ReplayFileConverter> BlackboxData is <null>!");
				return null;
			}
			p_data.ParseTracks();
			SerializedData header = p_data.header;
			string text = rnd.Next(0, 1000000).ToString("0000");
			ReplayFile replayFile = new ReplayFile();
			replayFile.Initialize(ReplayStream.GetReplayTempFilePath("", "$rc_file_" + text + "_"));
			replayFile.header.Initialize(ReplayStream.GetReplayTempFilePath("", "$rc_header_"));
			replayFile.AddSimulatorChannels(p_all: true);
			ReplayHeader header2 = replayFile.header;
			foreach (KeyValuePair<string, object> item in header)
			{
				string key = item.Key;
				string text2 = ((item.Value is string) ? ((string)item.Value) : null);
				if (item.Value is bool)
				{
					_ = (bool)item.Value;
				}
				if (item.Value is float)
				{
					_ = (float)item.Value;
				}
				if (item.Value is int)
				{
					_ = (int)item.Value;
				}
				_ = item.Value;
				switch (item.Key)
				{
				case "drone-rig":
				{
					DroneRigData droneRig = DroneRigData.FromJson(text2);
					header2.SetDroneRig(droneRig);
					break;
				}
				case "fc-profile":
				{
					FCProfileData fCProfile = Serialize.FromJson<FCProfileData>(text2);
					header2.SetFCProfile(fCProfile);
					break;
				}
				case "physics-tune":
				{
					DronePhysicsData physicsTune = DronePhysicsData.FromJson(text2);
					header2.SetPhysicsTune(physicsTune);
					break;
				}
				default:
					header2.data[key] = header[key];
					break;
				}
			}
			byte key2 = 32;
			if (p_data.tracks.ContainsKey(key2))
			{
				List<BlackboxFrame> list = p_data.tracks[key2];
				for (int i = 0; i < list.Count; i++)
				{
					BlackboxFrame blackboxFrame = list[i];
					ReplayEventType p_type = (ReplayEventType)Reflection<object>.Get<byte>(blackboxFrame.data, 0);
					float x = Reflection<object>.Get<float>(blackboxFrame.data, 1);
					float y = Reflection<object>.Get<float>(blackboxFrame.data, 2);
					float z = Reflection<object>.Get<float>(blackboxFrame.data, 3);
					Vector4 vector = new Vector4(x, y, z, blackboxFrame.time);
					object obj = Reflection<object>.Get(blackboxFrame.data, 4);
					object[] p_data2 = ((obj == null) ? null : ((obj is IList) ? ((object[])obj) : new object[1] { obj }));
					replayFile.PushEvent(p_type, blackboxFrame.time, vector, p_data2);
				}
			}
			float[] t = new float[4];
			foreach (KeyValuePair<byte, List<BlackboxFrame>> track in p_data.tracks)
			{
				DroneBlackboxDataFlag key3 = (DroneBlackboxDataFlag)track.Key;
				List<BlackboxFrame> list = track.Value;
				for (int j = 0; j < list.Count; j++)
				{
					BlackboxFrame blackboxFrame2 = list[j];
					Vector3 dk;
					switch (key3)
					{
					case DroneBlackboxDataFlag.Transform:
					{
						replayFile.WriteTime(blackboxFrame2.time);
						blackboxFrame2.GetTransform(out dk, out var r);
						replayFile.Write(ReplayChannelIds.DronePos, dk);
						replayFile.Write(ReplayChannelIds.DroneQuat, r);
						break;
					}
					case DroneBlackboxDataFlag.Velocity:
						dk = blackboxFrame2.GetVector3();
						replayFile.Write(ReplayChannelIds.DroneVel, dk);
						break;
					case DroneBlackboxDataFlag.RPM:
					{
						float[] floats = blackboxFrame2.GetFloats();
						replayFile.Write(ReplayChannelIds.Drone4RPM, floats);
						break;
					}
					case DroneBlackboxDataFlag.Input:
					{
						Vector4 vector2 = blackboxFrame2.GetVector4();
						replayFile.Write(ReplayChannelIds.Input, vector2);
						break;
					}
					case DroneBlackboxDataFlag.PIDControl:
						dk = blackboxFrame2.GetVector3();
						replayFile.Write(ReplayChannelIds.DronePID, dk);
						break;
					case DroneBlackboxDataFlag.Physics:
					{
						blackboxFrame2.GetPhysics(out dk, out var df, out t, out var to);
						replayFile.Write(ReplayChannelIds.DroneDrag, dk);
						replayFile.Write(ReplayChannelIds.DroneDragForce, df);
						replayFile.Write(ReplayChannelIds.Drone4Thrust, t);
						replayFile.Write("drone-torque", to);
						break;
					}
					}
				}
			}
			return replayFile;
		}

		public static byte[] ConvertReplayV2ToBytes(ReplayFile p_data)
		{
			p_data.Serialize();
			MemoryStream memoryStream = new MemoryStream();
			FileStream fileStream = new FileStream(p_data.file.Name, FileMode.Open);
			fileStream.CopyTo(memoryStream);
			fileStream.Close();
			memoryStream.Flush();
			memoryStream.Position = 0L;
			byte[] result = memoryStream.ToArray();
			memoryStream.Close();
			return result;
		}

		public void Clear()
		{
			if (jobs == null)
			{
				jobs = new List<ReplayConvertJob>();
			}
			for (int i = 0; i < jobs.Count; i++)
			{
				jobs[i].Clear();
			}
			jobs.Clear();
			if (ReplayConvertJob.thread_jobs != null)
			{
				lock (ReplayConvertJob.thread_jobs)
				{
					ReplayConvertJob.thread_jobs.Clear();
				}
			}
			if (ReplayConvertJob.thread_serialize != null)
			{
				lock (ReplayConvertJob.thread_serialize)
				{
					ReplayConvertJob.thread_serialize.Clear();
				}
			}
		}

		public void GetAmazonFileCount(string p_folder, Action<uint> p_on_complete)
		{
			string state = "collect";
			uint count = 0u;
			AmazonS3Client amzn = new AmazonS3Client(AMAZON_ACCESS_KEY, AMAZON_SECRET_KEY, RegionEndpoint.USEast1);
			Task<ListObjectsV2Response> req_task = null;
			ListObjectsV2Response res = null;
			Stopwatch req_timer = new Stopwatch();
			req_timer.Start();
			ListObjectsV2Request req;
			SetLoop(delegate
			{
				if (state != null)
				{
					switch (state)
					{
					case "collect":
						req = new ListObjectsV2Request
						{
							BucketName = AMAZON_S3_BUCKET,
							MaxKeys = 1000,
							Prefix = p_folder
						};
						if (res != null)
						{
							req.ContinuationToken = res.NextContinuationToken;
						}
						req_task = amzn.ListObjectsV2Async(req);
						state = "collect-wait";
						break;
					case "collect-wait":
					{
						if (req_task == null)
						{
							UnityEngine.Debug.LogError("ReplayFileConverter> GetAmazonFileCount / Response Task is <null>");
							return false;
						}
						try
						{
							if (!req_task.IsCompleted)
							{
								return true;
							}
							if (req_task.IsFaulted)
							{
								UnityEngine.Debug.LogError($"ReplayFileConverter> GetAmazonFileCount / Response Task Failed\n{req_task.Result}");
								return false;
							}
						}
						catch (Exception arg)
						{
							UnityEngine.Debug.LogError($"ReplayFileConverter> GetAmazonFileCount / Response Task Error\n{arg}");
							return false;
						}
						res = req_task.Result;
						List<S3Object> s3Objects = res.S3Objects;
						count += (uint)s3Objects.Count;
						for (int i = 0; i < s3Objects.Count; i++)
						{
							_ = s3Objects[i];
						}
						state = "collect";
						if (!res.IsTruncated)
						{
							state = "collect-completed";
						}
						break;
					}
					case "collect-completed":
					{
						float num = (float)req_timer.ElapsedMilliseconds / 1000f / 60f;
						UnityEngine.Debug.Log($"ReplayFileConverter> GetAmazonFileCount / Complete in [{num} min]");
						req_timer.Stop();
						if (p_on_complete != null)
						{
							p_on_complete(count);
						}
						return false;
					}
					}
				}
				return true;
			});
		}

		public void PutAmazonObject(Stream p_stream, string p_bucket, string p_folder, string p_key, Action<float, string> p_handler)
		{
			AmazonS3Client amzn = new AmazonS3Client(AMAZON_ACCESS_KEY, AMAZON_SECRET_KEY, RegionEndpoint.USEast1);
			PutObjectRequest req = null;
			Task<PutObjectResponse> req_task = null;
			float req_progress = 0f;
			string state = "create";
			SetLoop(delegate
			{
				if (state != null)
				{
					switch (state)
					{
					case "create":
					{
						string text = (p_folder.EndsWith("/") ? p_folder : (p_folder + "/"));
						req = new PutObjectRequest
						{
							BucketName = p_bucket,
							AutoCloseStream = false,
							AutoResetStreamPosition = true,
							Key = text + p_key,
							CannedACL = S3CannedACL.PublicRead
						};
						req.InputStream = p_stream;
						req.StreamTransferProgress = delegate(object p_sender, StreamTransferProgressArgs p_args)
						{
							req_progress = (float)p_args.PercentDone / 100f;
							state = "upload-progress";
						};
						req_task = amzn.PutObjectAsync(req);
						state = "upload";
						break;
					}
					case "upload-progress":
						if (p_handler != null)
						{
							p_handler(req_progress * 0.99f, "");
						}
						state = "upload";
						break;
					case "upload":
						if (req_task == null)
						{
							UnityEngine.Debug.LogError("ReplayFileConverter> PutAmazonObject / Response Task is <null>");
							if (p_handler != null)
							{
								p_handler(1f, "Task is <null>");
							}
							return false;
						}
						try
						{
							if (req_task.IsFaulted)
							{
								UnityEngine.Debug.LogError($"ReplayFileConverter> PutAmazonObject / Response Task Failed\n{req_task.Result}");
								if (p_handler != null)
								{
									p_handler(1f, "Task Failed");
								}
								return false;
							}
						}
						catch (Exception arg)
						{
							UnityEngine.Debug.LogError($"ReplayFileConverter> PutAmazonObject / Response Task Error\n{arg}");
							if (p_handler != null)
							{
								p_handler(1f, $"Task Error\n{arg}");
							}
							return false;
						}
						if (req_task.IsCompleted)
						{
							state = "upload-complete";
						}
						break;
					case "upload-complete":
						if (p_handler != null)
						{
							p_handler(1f, "");
						}
						return false;
					}
				}
				return true;
			});
		}

		public void ListAmazonObjects(string p_bucket, string p_folder, int p_page, int p_count, Action<List<S3Object>> p_callback)
		{
			AmazonS3Client amzn = new AmazonS3Client(AMAZON_ACCESS_KEY, AMAZON_SECRET_KEY, RegionEndpoint.USEast1);
			Task<ListObjectsV2Response> req_task = null;
			ListObjectsV2Response res = null;
			int page_idx = 0;
			string state = "fetch";
			ListObjectsV2Request req;
			SetLoop(delegate
			{
				if (state != null)
				{
					string text = state;
					if (!(text == "fetch"))
					{
						if (text == "fetch-wait")
						{
							if (req_task == null)
							{
								UnityEngine.Debug.LogError("ReplayFileConverter> ListAmazonObjects / Response Task is <null>");
								if (p_callback != null)
								{
									p_callback(null);
								}
								return false;
							}
							try
							{
								if (!req_task.IsCompleted)
								{
									return true;
								}
								if (req_task.IsFaulted)
								{
									UnityEngine.Debug.LogError($"ReplayFileConverter> ListAmazonObjects / Response Task Failed\n{req_task.Result}");
									if (p_callback != null)
									{
										p_callback(null);
									}
									return false;
								}
							}
							catch (Exception arg)
							{
								UnityEngine.Debug.LogError($"ReplayFileConverter> ListAmazonObjects / Response Task Error\n{arg}");
								if (p_callback != null)
								{
									p_callback(null);
								}
								return false;
							}
							res = req_task.Result;
							page_idx++;
							state = "fetch";
						}
					}
					else
					{
						if (page_idx > p_page)
						{
							List<S3Object> s3Objects = res.S3Objects;
							if (p_callback != null)
							{
								p_callback(s3Objects);
							}
							return false;
						}
						if (res != null && res.NextContinuationToken == null)
						{
							UnityEngine.Debug.LogWarning($"ReplayFileConverter> ListAmazonObjects / Page [{page_idx}] out of bounds!");
							if (p_callback != null)
							{
								p_callback(new List<S3Object>());
							}
							return false;
						}
						string prefix = (p_folder.EndsWith("/") ? p_folder : (p_folder + "/"));
						req = new ListObjectsV2Request
						{
							BucketName = p_bucket,
							MaxKeys = p_count,
							Prefix = prefix
						};
						if (res != null)
						{
							req.ContinuationToken = res.NextContinuationToken;
						}
						req_task = amzn.ListObjectsV2Async(req);
						state = "fetch-wait";
					}
				}
				return true;
			});
		}

		public void ConvertReplaysFromAmazonBucket(string p_bucket_from, string p_folder_from, string p_bucket_to, string p_folder_to, int p_file_count, int p_batch_count, int p_stride, int p_offset, Action<string, int> p_handler = null)
		{
			string state = "fetch-files";
			List<S3Object> batch_files = null;
			int batch_idx = 0;
			SetLoop(delegate
			{
				if (state != null)
				{
					switch (state)
					{
					case "fetch-files":
						if (p_handler != null)
						{
							p_handler(state, batch_idx);
						}
						if (p_batch_count > 0 && batch_idx >= p_batch_count)
						{
							UnityEngine.Debug.Log($"ReplayFileConverter> Conversion Completed / {p_batch_count} Batches - {p_batch_count * p_file_count / p_stride} Files");
							if (p_handler != null)
							{
								p_handler("conversion-completed", batch_idx - 1);
							}
							return false;
						}
						UnityEngine.Debug.Log("ReplayFileConverter> Fetching Files...");
						ListAmazonObjects(p_bucket_from, p_folder_from, batch_idx, p_file_count, delegate(List<S3Object> p_file_list)
						{
							UnityEngine.Debug.Log($"ReplayFileConverter> Fetch Completed / {p_file_list?.Count ?? 0} Files");
							batch_files = p_file_list;
							state = "fetch-files-completed";
						});
						state = "fetch-files-wait";
						break;
					case "fetch-files-completed":
					{
						if (batch_files == null)
						{
							UnityEngine.Debug.LogWarning($"ReplayFileConverter> Failed to fetch file list! Bucket[{p_bucket_from} -> {p_folder_from}] Batch[{batch_idx}/{p_batch_count}]");
							if (p_handler != null)
							{
								p_handler("error", batch_idx);
							}
							return false;
						}
						if (batch_files.Count <= 0)
						{
							UnityEngine.Debug.LogWarning("ReplayFileConverter> No files Found! Conversion Finished!");
							if (p_handler != null)
							{
								p_handler("conversion-completed", batch_idx);
							}
							return false;
						}
						int num3 = p_stride;
						int num4 = p_offset;
						Clear();
						for (int num5 = 0; num5 < batch_files.Count; num5++)
						{
							int num6 = num5 * num3 + num4;
							if (num6 >= batch_files.Count)
							{
								break;
							}
							S3Object s3Object = batch_files[num6];
							string srcURL = AMAZON_S3_ROOT + s3Object.Key;
							string text = s3Object.Key.Replace(p_folder_from, "");
							if (text.StartsWith("/"))
							{
								text = text.Substring(1);
							}
							ReplayConvertJob replayConvertJob = new ReplayConvertJob
							{
								converter = this,
								srcURL = srcURL,
								srcName = s3Object.ETag.Replace("\"", ""),
								useAmazonS3 = true,
								amazonBucket = p_bucket_to,
								amazonBucketFolder = p_folder_to,
								amazonFileKey = text
							};
							jobs.Add(replayConvertJob);
							replayConvertJob.Run();
						}
						if (p_handler != null)
						{
							p_handler(state, batch_idx);
						}
						state = "";
						Activity.RunOnce(delegate
						{
							UnityEngine.Debug.Log("ReplayFileConverter> Job Process Started!");
							state = "process-jobs";
							if (p_handler != null)
							{
								p_handler(state, batch_idx);
							}
						}, 2f);
						break;
					}
					case "process-jobs":
					{
						int num = 0;
						for (int num2 = 0; num2 < jobs.Count; num2++)
						{
							jobs[num2].Update();
							if (jobs[num2].completed)
							{
								num++;
							}
						}
						if (num < jobs.Count)
						{
							return true;
						}
						UnityEngine.Debug.Log("ReplayFileConverter> Job Process Completed!");
						state = "jobs-completed";
						if (p_handler != null)
						{
							p_handler(state, batch_idx);
						}
						batch_idx++;
						break;
					}
					case "jobs-completed":
						state = "";
						Activity.RunOnce(delegate
						{
							state = "fetch-files";
						}, 5f);
						break;
					}
				}
				return true;
			});
		}

		public string GetLeaderboardProviderURL(int p_page, int p_count, string p_map, string p_track, string p_custom_map)
		{
			string text = DRLAPIRoot + "leaderboards/";
			string[] value = new string[12]
			{
				$"page={p_page + 1}&",
				$"limit={p_count}&",
				"game-type=Race&",
				"score-type=TimeMin&",
				"diameter=7&",
				"map=" + p_map + "&",
				"track=" + p_track + "&",
				string.IsNullOrEmpty(p_custom_map) ? ("custom-map=" + p_custom_map + "&") : "",
				"is-custom-map=" + (!string.IsNullOrEmpty(p_custom_map)).ToString().ToLower() + "&",
				"drl-official=true&",
				"custom-physics=false&",
				"token=eyJzdGVhbUlkIjoiNzY1NjExOTgwMDQxOTY3MjIiLCJ4YnVpZCI6bnVsbCwicGxheXN0YXRpb25JZCI6bnVsbCwiZXBpY0lkIjpudWxsLCJ0aWNrZXQiOiIiLCJvcyI6IndpbiIsInZlcnNpb24iOiIzLjEyLjFhYzYucmxzLXdpbiJ9"
			};
			return text + "?" + string.Join("", value);
		}

		public void ConvertReplaysFromLeaderboards(int p_page, int p_count, string p_map, string p_track, string p_custom_map = "")
		{
			string leaderboardProviderURL = GetLeaderboardProviderURL(p_page, p_count, p_map, p_track, p_custom_map);
			UnityWebRequest req = UnityWebRequest.Get(leaderboardProviderURL);
			req.SendWebRequest();
			SetLoop(delegate
			{
				if (!req.isDone)
				{
					return true;
				}
				SerializedData serializedData = Serialize.FromJson<SerializedData>(req.downloadHandler.text);
				if (!serializedData.Get<bool>("success"))
				{
					UnityEngine.Debug.LogWarning("ReplayFileConverter> Failed to Load Replays!");
					return false;
				}
				SerializedData serializedData2 = ((JObject)serializedData["data"]).ToObject<SerializedData>();
				((JObject)serializedData2["pagging"]).ToObject<SerializedData>();
				List<SerializedData> list = ((JArray)serializedData2["leaderboard"]).ToObject<List<SerializedData>>();
				if (jobs == null)
				{
					jobs = new List<ReplayConvertJob>();
				}
				for (int i = 0; i < list.Count; i++)
				{
					SerializedData serializedData3 = list[i];
					ReplayConvertJob replayConvertJob = new ReplayConvertJob
					{
						converter = this,
						srcURL = serializedData3.Get<string>("replay-url")
					};
					jobs.Add(replayConvertJob);
					replayConvertJob.Run();
				}
				return false;
			});
		}

		public string GetDRLReplayConversionURL(int p_page, int p_count, string p_map, string p_track, string p_custom_map)
		{
			string text = DRLAPIRoot + "tools/replay-conversion/";
			List<string> list = new List<string>();
			list.Add($"page={p_page + 1}&");
			list.Add($"count={p_count}&");
			if (p_map == "onboarding")
			{
				text = text + p_map + "/";
			}
			else if (p_map == "circuits")
			{
				text = text + p_map + "/";
			}
			else
			{
				list.Add("map=" + p_map + "&");
				list.Add((!string.IsNullOrEmpty(p_track)) ? ("track=" + p_track + "&") : "");
				list.Add((!string.IsNullOrEmpty(p_custom_map)) ? ("custom-map=" + p_custom_map + "&") : "");
			}
			return text + "?" + string.Join("", list);
		}

		public void ListDRLReplayConversionFiles(int p_page, int p_count, string p_map, string p_track, string p_custom_map, Action<List<SerializedData>> p_on_complete)
		{
			string url = GetDRLReplayConversionURL(p_page, p_count, p_map, p_track, p_custom_map);
			UnityWebRequest req = UnityWebRequest.Get(url);
			req.SendWebRequest();
			SetLoop(delegate
			{
				if (!req.isDone)
				{
					return true;
				}
				new List<SerializedData>();
				if (req.responseCode != 200)
				{
					UnityEngine.Debug.LogWarning("ReplayFileConverter> ListDRLReplayConversionFiles / Failed to Load Replays\n  " + url);
					if (p_on_complete != null)
					{
						p_on_complete(null);
					}
					return false;
				}
				List<SerializedData> obj = ((JArray)Serialize.FromJson<SerializedData>(req.downloadHandler.text)["replays"]).ToObject<List<SerializedData>>();
				if (p_on_complete != null)
				{
					p_on_complete(obj);
				}
				return false;
			});
		}

		public void ConvertReplaysFromDRLConversionAPI(int p_page, int p_count, string p_map, string p_track, string p_custom_map, Action<string, int> p_handler = null)
		{
			string state = "fetch-files";
			List<SerializedData> batch_files = null;
			int batch_count = 10;
			int total_files = 0;
			SetLoop(delegate
			{
				if (state != null)
				{
					switch (state)
					{
					case "fetch-files":
						if (p_handler != null)
						{
							p_handler(state, p_page);
						}
						if (batch_count > 0 && p_page >= batch_count)
						{
							UnityEngine.Debug.Log($"ReplayFileConverter> Conversion Completed / {batch_count} Batches - {total_files} Files");
							if (p_handler != null)
							{
								p_handler("conversion-completed", p_page - 1);
							}
							return false;
						}
						UnityEngine.Debug.Log("ReplayFileConverter> Fetching Files...");
						ListDRLReplayConversionFiles(p_page, p_count, p_map, p_track, p_custom_map, delegate(List<SerializedData> p_file_list)
						{
							UnityEngine.Debug.Log($"ReplayFileConverter> Fetch Completed / {p_file_list?.Count ?? 0} Files");
							batch_files = p_file_list;
							state = "fetch-files-completed";
						});
						state = "fetch-files-wait";
						break;
					case "fetch-files-completed":
					{
						if (batch_files == null)
						{
							UnityEngine.Debug.LogWarning("ReplayFileConverter> Failed to fetch file list!");
							if (p_handler != null)
							{
								p_handler("error", p_page);
							}
							return false;
						}
						if (batch_files.Count <= 0)
						{
							UnityEngine.Debug.LogWarning("ReplayFileConverter> No files Found! Conversion Finished!");
							if (p_handler != null)
							{
								p_handler("conversion-completed", p_page);
							}
							return false;
						}
						total_files += batch_files.Count;
						Clear();
						for (int num3 = 0; num3 < batch_files.Count; num3++)
						{
							SerializedData serializedData = batch_files[num3];
							string srcName = serializedData.Get<string>("id");
							string srcURL = serializedData.Get<string>("url");
							string text = DRLAPIRoot + "tools/replay-conversion/";
							if (p_map == "circuits")
							{
								text += "circuits/";
							}
							ReplayConvertJob replayConvertJob = new ReplayConvertJob
							{
								converter = this,
								srcURL = srcURL,
								srcName = srcName,
								dstURL = text,
								useAmazonS3 = false
							};
							jobs.Add(replayConvertJob);
							replayConvertJob.Run();
						}
						if (p_handler != null)
						{
							p_handler(state, p_page);
						}
						state = "";
						Activity.RunOnce(delegate
						{
							UnityEngine.Debug.Log("ReplayFileConverter> Job Process Started!");
							state = "process-jobs";
							if (p_handler != null)
							{
								p_handler(state, p_page);
							}
						}, 2f);
						break;
					}
					case "process-jobs":
					{
						int num = 0;
						for (int num2 = 0; num2 < jobs.Count; num2++)
						{
							jobs[num2].Update();
							if (jobs[num2].completed)
							{
								num++;
							}
						}
						if (num < jobs.Count)
						{
							return true;
						}
						UnityEngine.Debug.Log("ReplayFileConverter> Job Process Completed!");
						state = "jobs-completed";
						if (p_handler != null)
						{
							p_handler(state, p_page);
						}
						p_page++;
						break;
					}
					case "jobs-completed":
						state = "";
						Activity.RunOnce(delegate
						{
							state = "fetch-files";
						}, 5f);
						break;
					}
				}
				return true;
			});
		}

		public string GetPublicReplayConversionURL(int p_page, int p_count)
		{
			return string.Concat(DRLAPIRoot + "tools/replay-conversion/public/", "?", string.Join("", new List<string>
			{
				$"page={p_page + 1}&",
				$"count={p_count}"
			}));
		}

		public void ListPublicReplayConversionFiles(int p_page, int p_count, Action<List<SerializedData>> p_on_complete)
		{
			string url = GetPublicReplayConversionURL(p_page, p_count);
			UnityWebRequest req = UnityWebRequest.Get(url);
			req.SendWebRequest();
			SetLoop(delegate
			{
				if (!req.isDone)
				{
					return true;
				}
				new List<SerializedData>();
				if (req.responseCode != 200)
				{
					UnityEngine.Debug.LogWarning($"ReplayFileConverter> ListPublicReplayConversionFiles / Failed to Load Replays | code {req.responseCode}\n  {url}");
					if (p_on_complete != null)
					{
						p_on_complete(null);
					}
					return false;
				}
				SerializedData serializedData = Serialize.FromJson<SerializedData>(req.downloadHandler.text);
				if (serializedData.Get<int>("count") < 0)
				{
					UnityEngine.Debug.LogWarning("ReplayFileConverter> ListPublicReplayConversionFiles / Failed to Load Replays | Negative Count\n  " + url);
					if (p_on_complete != null)
					{
						p_on_complete(null);
					}
					return false;
				}
				JArray jArray = (JArray)serializedData["replays"];
				List<SerializedData> obj = ((jArray == null) ? new List<SerializedData>() : jArray.ToObject<List<SerializedData>>());
				if (p_on_complete != null)
				{
					p_on_complete(obj);
				}
				return false;
			});
		}

		public void ConvertReplaysFromPublicConversionAPI(int p_page, int p_count, Action<string, int> p_handler = null)
		{
			string state = "fetch-files";
			List<SerializedData> batch_files = new List<SerializedData>();
			int batch_count = 0;
			int total_files = 0;
			SetLoop(delegate
			{
				if (state != null)
				{
					switch (state)
					{
					case "fetch-files":
						if (batch_files.Count > 0)
						{
							UnityEngine.Debug.Log($"ReplayFileConverter> Fetch Files / Available Files {batch_files.Count}");
							state = "fetch-files-completed";
						}
						else
						{
							if (p_handler != null)
							{
								p_handler(state, p_page);
							}
							if (batch_count > 0 && p_page >= batch_count)
							{
								UnityEngine.Debug.Log($"ReplayFileConverter> Conversion Completed / {batch_count} Batches - {total_files} Files");
								if (p_handler != null)
								{
									p_handler("conversion-completed", p_page - 1);
								}
								return false;
							}
							UnityEngine.Debug.Log("ReplayFileConverter> Fetching Files...");
							ListPublicReplayConversionFiles(p_page, p_count, delegate(List<SerializedData> p_file_list)
							{
								UnityEngine.Debug.Log($"ReplayFileConverter> Fetch Completed / {p_file_list?.Count ?? 0} Files");
								if (p_file_list != null)
								{
									batch_files.AddRange(p_file_list);
								}
								state = "fetch-files-completed";
							});
							state = "fetch-files-wait";
						}
						break;
					case "fetch-files-completed":
					{
						if (batch_files == null)
						{
							UnityEngine.Debug.LogWarning("ReplayFileConverter> Failed to fetch file list!");
							if (p_handler != null)
							{
								p_handler("error", p_page);
							}
							return false;
						}
						if (batch_files.Count <= 0)
						{
							UnityEngine.Debug.LogWarning("ReplayFileConverter> No files Found! Conversion Finished!");
							if (p_handler != null)
							{
								p_handler("conversion-completed", p_page);
							}
							return false;
						}
						List<SerializedData> list = new List<SerializedData>();
						int num3 = Mathf.Min(p_count, batch_files.Count);
						for (int num4 = 0; num4 < num3; num4++)
						{
							SerializedData item = batch_files[0];
							list.Add(item);
							batch_files.RemoveAt(0);
						}
						total_files += list.Count;
						Clear();
						for (int num5 = 0; num5 < list.Count; num5++)
						{
							SerializedData serializedData = list[num5];
							serializedData.Get<string>("id");
							string text = serializedData.Get<string>("url");
							string text2 = (text.Contains(AMAZON_S3_ROOT) ? text.Replace(AMAZON_S3_ROOT, "") : "");
							if (text2.StartsWith("replays"))
							{
								text2 = text2.Substring("replays".Length);
							}
							if (text2.StartsWith("/"))
							{
								text2 = text2.Substring(1);
							}
							ReplayConvertJob replayConvertJob = new ReplayConvertJob
							{
								converter = this,
								srcURL = text,
								srcName = text2,
								useAmazonS3 = true,
								amazonBucket = (string.IsNullOrEmpty(text2) ? "" : "drl-game-api"),
								amazonBucketFolder = "replays-v2",
								amazonFileKey = text2
							};
							jobs.Add(replayConvertJob);
							replayConvertJob.Run();
						}
						if (p_handler != null)
						{
							p_handler(state, p_page);
						}
						state = "";
						Activity.RunOnce(delegate
						{
							UnityEngine.Debug.Log("ReplayFileConverter> Job Process Started!");
							state = "process-jobs";
							if (p_handler != null)
							{
								p_handler(state, p_page);
							}
						}, 2f);
						break;
					}
					case "process-jobs":
					{
						int num = 0;
						for (int num2 = 0; num2 < jobs.Count; num2++)
						{
							jobs[num2].Update();
							if (jobs[num2].completed)
							{
								num++;
							}
						}
						if (num < jobs.Count)
						{
							return true;
						}
						UnityEngine.Debug.Log("ReplayFileConverter> Job Process Completed!");
						state = "jobs-completed";
						if (p_handler != null)
						{
							p_handler(state, p_page);
						}
						p_page++;
						break;
					}
					case "jobs-completed":
						state = "";
						Activity.RunOnce(delegate
						{
							state = "fetch-files";
						}, 5f);
						break;
					}
				}
				return true;
			});
		}

		public void SetLoop(Func<bool> p_loop)
		{
			if (p_loop != null && Application.isPlaying)
			{
				Activity.Run(() => p_loop());
			}
		}

		public static int ReplayToolsCLI(string p_mode, string p_input_file, string p_output_file)
		{
			string text = p_input_file;
			string text2 = p_output_file;
			if (string.IsNullOrEmpty(p_mode))
			{
				Log("  Error: Invalid Arguments");
				return 1;
			}
			string text3 = Environment.CurrentDirectory.Replace("\\", "/");
			if (!text3.EndsWith("/"))
			{
				text3 += "/";
			}
			text = text.Replace("\\", "/");
			if (text.StartsWith("/"))
			{
				text = text.Substring(1);
			}
			FileInfo fileInfo = new FileInfo(text.Contains(":") ? text : (text3 + text));
			if (!fileInfo.Exists)
			{
				Log("  Error: Invalid Input File\n" + text);
				return 1;
			}
			if (string.IsNullOrEmpty(text2))
			{
				switch (p_mode)
				{
				case "convert":
					text2 = text + ".rpl2";
					break;
				case "csv":
					text2 = text + ".csv";
					break;
				case "header":
					text2 = text + ".json";
					break;
				default:
					Log("  Error: Invalid Mode " + p_mode);
					return 2;
				}
			}
			text2 = text2.Replace("\\", "/");
			if (text2.StartsWith("/"))
			{
				text2 = text2.Substring(1);
			}
			string text4 = (text2.Contains(":") ? text2 : (text3 + text2));
			string text5 = "";
			if (text4.Contains("/"))
			{
				text5 = text4.Substring(0, text4.LastIndexOf("/"));
			}
			if (!string.IsNullOrEmpty(text5))
			{
				new DirectoryInfo(text5).Create();
			}
			BlackboxData replayV1FromFile = GetReplayV1FromFile(fileInfo.FullName);
			ReplayFile replayFile = ((replayV1FromFile == null) ? GetReplayV2FromFile(fileInfo.FullName) : null);
			switch (p_mode)
			{
			case "convert":
			{
				if (replayFile != null)
				{
					Log("  Input File is already V2 / Ignoring");
					break;
				}
				replayFile = ConvertToReplayV2(replayV1FromFile);
				byte[] bytes = ConvertReplayV2ToBytes(replayFile);
				File.WriteAllBytes(text4, bytes);
				replayFile.Destroy();
				break;
			}
			case "csv":
				if (replayV1FromFile == null && replayFile == null)
				{
					Log("  No Replay Files Provided / Ignoring");
					return 2;
				}
				if (replayV1FromFile != null)
				{
					File.WriteAllText(text4, replayV1FromFile.ToCSV());
				}
				if (replayFile != null)
				{
					FileStream fileStream2 = new FileStream(text4, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
					fileStream2.SetLength(0L);
					replayFile.ToCSV(fileStream2);
					fileStream2.Close();
					replayFile.Destroy();
				}
				break;
			case "header":
				if (replayV1FromFile == null && replayFile == null)
				{
					Log("  No Replay Files Provided / Ignoring");
					return 2;
				}
				if (replayV1FromFile != null)
				{
					File.WriteAllText(text4, replayV1FromFile.header.ToJson(p_indented: true));
				}
				if (replayFile != null)
				{
					FileStream fileStream = new FileStream(text4, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
					fileStream.SetLength(0L);
					replayFile.header.Serialize(fileStream);
					fileStream.Close();
					replayFile.Destroy();
				}
				break;
			}
			return 0;
		}

		public static void Log(string v)
		{
			Console.WriteLine(v);
		}

		public static BlackboxData GetReplayV1FromFile(string p_path)
		{
			BlackboxRecord blackboxRecord = null;
			byte[] p_data = File.ReadAllBytes(p_path);
			try
			{
				blackboxRecord = Serialize.FromBytes<BlackboxRecord>(p_data, p_unsafe: true);
				blackboxRecord.Decompress();
				blackboxRecord.Prune();
				blackboxRecord.ParseTracks();
			}
			catch (Exception)
			{
				return null;
			}
			if (blackboxRecord.clips.Count <= 0)
			{
				return null;
			}
			return blackboxRecord.clips[0];
		}

		public static ReplayFile GetReplayV2FromFile(string p_path)
		{
			byte[] p_data = File.ReadAllBytes(p_path);
			ReplayFile replayFile = null;
			try
			{
				return ReplayFile.FromBytes(p_data);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Replay V2 Parse Error\n  " + ex.Message);
				return null;
			}
		}
	}
}
