using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class ReplayConvertJob
	{
		public static List<WaitCallback> thread_jobs = new List<WaitCallback>();

		public static List<WaitCallback> thread_serialize = new List<WaitCallback>();

		public static List<Thread> threads;

		public ReplayFileConverter converter;

		public ReplayConvertJobState state;

		public bool useAmazonS3;

		public string srcURL;

		public string dstURL;

		public string srcName;

		public string amazonBucket;

		public string amazonBucketFolder;

		public string amazonFileKey;

		public UnityWebRequest downloader;

		public UnityWebRequest uploader;

		protected float m_upload_progress;

		public BlackboxData replayV1;

		public ReplayFile replayV2;

		public ReplayHeader headerV2;

		public byte[] replayV1Data;

		public byte[] replayV2Data;

		public bool completed
		{
			get
			{
				if (state != ReplayConvertJobState.Complete)
				{
					return state == ReplayConvertJobState.Error;
				}
				return true;
			}
		}

		public float progress
		{
			get
			{
				float result = 0f;
				switch (state)
				{
				case ReplayConvertJobState.Init:
					result = 0.1f;
					break;
				case ReplayConvertJobState.Download:
					result = Mathf.Lerp(0.1f, 0.3f, requestProgress);
					break;
				case ReplayConvertJobState.DeserializeStart:
					result = 0.3f;
					break;
				case ReplayConvertJobState.Deserializing:
					result = 0.3f;
					break;
				case ReplayConvertJobState.ConvertStart:
					result = 0.6f;
					break;
				case ReplayConvertJobState.Converting:
					result = 0.6f;
					break;
				case ReplayConvertJobState.Upload:
					result = Mathf.Lerp(0.6f, 1f, requestProgress);
					break;
				case ReplayConvertJobState.Uploading:
					result = Mathf.Lerp(0.6f, 1f, requestProgress);
					break;
				case ReplayConvertJobState.Complete:
					result = 1f;
					break;
				case ReplayConvertJobState.Error:
					result = 1f;
					break;
				}
				return result;
			}
		}

		public float requestProgress
		{
			get
			{
				if (downloader != null)
				{
					return downloader.downloadProgress;
				}
				if (uploader != null)
				{
					return uploader.uploadProgress;
				}
				return m_upload_progress;
			}
		}

		public int replayV1LengthKb
		{
			get
			{
				if (replayV1Data != null)
				{
					return replayV1Data.Length / 1024;
				}
				return 0;
			}
		}

		public int replayV2LengthKb
		{
			get
			{
				if (replayV2Data != null)
				{
					return replayV2Data.Length / 1024;
				}
				return 0;
			}
		}

		public static void QueueJob(WaitCallback p_callback, bool p_is_serialize = false)
		{
			if (thread_jobs == null)
			{
				thread_jobs = new List<WaitCallback>();
			}
			if (threads == null)
			{
				int num = Mathf.Max(1, Environment.ProcessorCount - 6);
				Debug.Log($"ReplayFileConverter> Starting {num} threads!");
				threads = new List<Thread>();
				for (int i = 0; i < num; i++)
				{
					Thread thread = new Thread(delegate(object p_thread_id)
					{
						List<WaitCallback> list = (((int)p_thread_id == 0) ? thread_serialize : thread_jobs);
						while (true)
						{
							WaitCallback waitCallback = null;
							lock (list)
							{
								if (list.Count > 0)
								{
									waitCallback = list[0];
									list.RemoveAt(0);
								}
							}
							waitCallback?.Invoke(null);
							Thread.Sleep(0);
						}
					});
					thread.Priority = System.Threading.ThreadPriority.Highest;
					thread.Start(i);
					threads.Add(thread);
				}
			}
			if (p_is_serialize)
			{
				lock (thread_serialize)
				{
					thread_serialize.Add(p_callback);
					return;
				}
			}
			lock (thread_jobs)
			{
				thread_jobs.Add(p_callback);
			}
		}

		public void Clear()
		{
			state = ReplayConvertJobState.Idle;
			srcURL = "";
			UnityWebRequest unityWebRequest = downloader;
			if (unityWebRequest != null)
			{
				if (unityWebRequest.downloadHandler != null)
				{
					unityWebRequest.downloadHandler.Dispose();
				}
				if (unityWebRequest.uploadHandler != null)
				{
					unityWebRequest.uploadHandler.Dispose();
				}
				unityWebRequest.Dispose();
			}
			unityWebRequest = uploader;
			if (unityWebRequest != null)
			{
				if (unityWebRequest.downloadHandler != null)
				{
					unityWebRequest.downloadHandler.Dispose();
				}
				if (unityWebRequest.uploadHandler != null)
				{
					unityWebRequest.uploadHandler.Dispose();
				}
				unityWebRequest.Dispose();
			}
			if (replayV1 != null)
			{
				replayV1.Clear();
				replayV1.ClearTrackTable();
			}
			if (replayV2 != null)
			{
				replayV2.Destroy();
				replayV2 = null;
			}
			if (headerV2 != null)
			{
				headerV2.Clear();
				headerV2 = null;
			}
			replayV1 = null;
			replayV2 = null;
			replayV1Data = null;
			replayV2Data = null;
		}

		public void Run()
		{
			state = ReplayConvertJobState.Init;
		}

		public void Update()
		{
			switch (state)
			{
			case ReplayConvertJobState.Init:
			{
				if (string.IsNullOrEmpty(srcURL))
				{
					state = ReplayConvertJobState.Error;
					Debug.LogWarning("ReplayFileConverter> Job Error / Init - Empty URL");
					break;
				}
				UnityWebRequest unityWebRequest = UnityWebRequest.Get(srcURL);
				unityWebRequest.SendWebRequest();
				downloader = unityWebRequest;
				state = ReplayConvertJobState.Download;
				break;
			}
			case ReplayConvertJobState.Download:
				if (downloader == null)
				{
					state = ReplayConvertJobState.Error;
				}
				else if (downloader.isDone)
				{
					state = ReplayConvertJobState.DeserializeStart;
				}
				break;
			case ReplayConvertJobState.DeserializeStart:
			{
				if (downloader.responseCode != 200)
				{
					state = ReplayConvertJobState.Error;
					Debug.LogWarning($"ReplayFileConverter> Job Error / DeserializeStart - Download Failure | Code[{downloader.responseCode}]\n  {srcURL}");
					break;
				}
				byte[] d = downloader.downloadHandler.data;
				replayV1Data = d;
				_ = DRLPaths.DataPath;
				QueueJob(delegate
				{
					BlackboxRecord blackboxRecord = null;
					try
					{
						blackboxRecord = Serialize.FromBytes<BlackboxRecord>(d, p_unsafe: false);
						blackboxRecord.Decompress();
						blackboxRecord.Prune();
					}
					catch (Exception)
					{
						state = ReplayConvertJobState.Error;
						Debug.LogWarning("ReplayFileConverter> Job Error / DeserializeStart - V1 File Parse Failure\n" + srcURL);
						return;
					}
					if (blackboxRecord.clips.Count <= 0)
					{
						state = ReplayConvertJobState.Error;
					}
					else
					{
						replayV1 = blackboxRecord.clips[0];
						state = ReplayConvertJobState.ConvertStart;
					}
				});
				state = ReplayConvertJobState.Deserializing;
				break;
			}
			case ReplayConvertJobState.ConvertStart:
				if (replayV1 == null)
				{
					state = ReplayConvertJobState.Error;
					Debug.LogWarning("ReplayFileConverter> Job Error / ConvertStart - V1 File Invalid!");
					break;
				}
				state = ReplayConvertJobState.Converting;
				replayV2 = ReplayFileConverter.ConvertToReplayV2(replayV1);
				QueueJob(delegate
				{
					if (replayV2 == null)
					{
						state = ReplayConvertJobState.Error;
						Debug.LogWarning("ReplayFileConverter> Job Error / ConvertStart - V2 Conversion Failed!");
					}
					else
					{
						headerV2 = new ReplayHeader();
						headerV2.data.Merge(replayV2.header.data);
						byte[] array = ReplayFileConverter.ConvertReplayV2ToBytes(replayV2);
						replayV2.Destroy();
						replayV2Data = array;
						state = ReplayConvertJobState.Upload;
					}
				}, p_is_serialize: true);
				break;
			case ReplayConvertJobState.Upload:
				if (useAmazonS3)
				{
					if (string.IsNullOrEmpty(amazonBucket))
					{
						state = ReplayConvertJobState.Complete;
						break;
					}
					state = ReplayConvertJobState.Uploading;
					MemoryStream ms = new MemoryStream(replayV2Data);
					converter.PutAmazonObject(ms, amazonBucket, amazonBucketFolder, amazonFileKey, delegate(float p_progress, string p_error)
					{
						m_upload_progress = p_progress;
						if (!(p_progress < 1f))
						{
							ms.Close();
							if (!string.IsNullOrEmpty(p_error))
							{
								state = ReplayConvertJobState.Error;
								Debug.LogWarning("ReplayFileConverter> Job Error / Upload\n  " + p_error);
							}
							else
							{
								dstURL = ReplayFileConverter.AMAZON_S3_ROOT + amazonBucketFolder + "/" + amazonFileKey;
								Debug.Log("ReplayFileConverter> Job Completed [" + srcName + "] user[" + replayV1.header.Get("profile-name", "") + "] map[" + replayV1.GetMapGUID() + "] track[" + replayV1.GetTrackGUID() + "] custom-map[" + replayV1.GetCustomMapGUID() + "]\n  " + dstURL);
								state = ReplayConvertJobState.Complete;
							}
						}
					});
				}
				else
				{
					WWWForm wWWForm = new WWWForm();
					wWWForm.AddField("id", srcName);
					wWWForm.AddBinaryData("file", replayV2Data);
					UnityWebRequest unityWebRequest2 = UnityWebRequest.Post(dstURL, wWWForm);
					unityWebRequest2.SendWebRequest();
					uploader = unityWebRequest2;
					state = ReplayConvertJobState.Uploading;
				}
				break;
			case ReplayConvertJobState.Uploading:
				if (useAmazonS3)
				{
					break;
				}
				if (uploader == null)
				{
					state = ReplayConvertJobState.Error;
					Debug.LogWarning("ReplayFileConverter> Job Error / DRL API Uploader is <null>");
					break;
				}
				m_upload_progress = requestProgress;
				if (uploader.isDone)
				{
					Debug.Log("ReplayFileConverter> Job Completed [" + srcName + "] user[" + replayV1.header.Get("profile-name", "") + "] map[" + replayV1.GetMapGUID() + "] track[" + replayV1.GetTrackGUID() + "] custom-map[" + replayV1.GetCustomMapGUID() + "]\n  " + dstURL);
					dstURL = "";
					state = ReplayConvertJobState.Complete;
				}
				break;
			case ReplayConvertJobState.Idle:
			case ReplayConvertJobState.Deserializing:
			case ReplayConvertJobState.Converting:
			case ReplayConvertJobState.Complete:
			case ReplayConvertJobState.Error:
				break;
			}
		}
	}
}
