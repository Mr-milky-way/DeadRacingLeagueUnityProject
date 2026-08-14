using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIReplayConvertDashboard : MonoBehaviour
	{
		[Header("Jobs")]
		public ReplayFileConverter converter;

		public bool running;

		[Header("API Menu")]
		public RectTransform menuDRL;

		public RectTransform menuAmazon;

		public RectTransform menuPublic;

		public Button drlButtonTab;

		public Button amazonButtonTab;

		public Button publicButtonTab;

		[Header("DRL Menu")]
		public InputField drlEndpointField;

		public InputField drlPageIndexField;

		public InputField drlPageItemCountField;

		public InputField drlMapField;

		public InputField drlTrackField;

		public InputField drlCustomMapField;

		public Button drlRunButton;

		public Button drlCancelButton;

		[Header("Amazon Menu")]
		public InputField amazonBucketField;

		public InputField amazonFolderFromField;

		public InputField amazonFolderToField;

		public InputField amazonInstanceIdField;

		public InputField amazonInstanceCountField;

		public InputField amazonBatchCountField;

		public InputField amazonFileCountField;

		public Button amazonRunButton;

		public Button amazonCancelButton;

		[Header("Public Menu")]
		public InputField publicPageIndexField;

		public InputField publicPageItemCountField;

		public InputField publicInstanceIdField;

		public InputField publicInstanceCountField;

		public Button publicRunButton;

		public Button publicCancelButton;

		[Header("Dashboard")]
		public Text consoleField;

		public Text jobStatusField;

		public GridLayoutGroup jobsGridLayout;

		public UIReplayConvertJobItem jobItemTemplate;

		public List<UIReplayConvertJobItem> jobList;

		public string drlEndpoint
		{
			get
			{
				return drlEndpointField.text;
			}
			set
			{
				drlEndpointField.text = value;
			}
		}

		public int drlPageIndex
		{
			get
			{
				return GetFieldInt(drlPageIndexField, 0);
			}
			set
			{
				drlPageIndexField.text = value.ToString();
			}
		}

		public int drlPageItemCount
		{
			get
			{
				return GetFieldInt(drlPageItemCountField, 0);
			}
			set
			{
				drlPageItemCountField.text = value.ToString();
			}
		}

		public string drlMap
		{
			get
			{
				return drlMapField.text;
			}
			set
			{
				drlMapField.text = value;
			}
		}

		public string drlTrack
		{
			get
			{
				return drlTrackField.text;
			}
			set
			{
				drlTrackField.text = value;
			}
		}

		public string drlCustomMap
		{
			get
			{
				return drlCustomMapField.text;
			}
			set
			{
				drlCustomMapField.text = value;
			}
		}

		public string amazonBucket
		{
			get
			{
				return amazonBucketField.text;
			}
			set
			{
				amazonBucketField.text = value;
			}
		}

		public string amazonFolderFrom
		{
			get
			{
				return amazonFolderFromField.text;
			}
			set
			{
				amazonFolderFromField.text = value;
			}
		}

		public string amazonFolderTo
		{
			get
			{
				return amazonFolderToField.text;
			}
			set
			{
				amazonFolderToField.text = value;
			}
		}

		public int amazonInstanceId
		{
			get
			{
				return GetFieldInt(amazonInstanceIdField, 0);
			}
			set
			{
				amazonInstanceIdField.text = value.ToString();
			}
		}

		public int amazonInstanceCount
		{
			get
			{
				return GetFieldInt(amazonInstanceCountField, 1);
			}
			set
			{
				amazonInstanceCountField.text = value.ToString();
			}
		}

		public int amazonBatchCount
		{
			get
			{
				return GetFieldInt(amazonBatchCountField, 1);
			}
			set
			{
				amazonBatchCountField.text = value.ToString();
			}
		}

		public int amazonFileCount
		{
			get
			{
				return GetFieldInt(amazonFileCountField, 1);
			}
			set
			{
				amazonFileCountField.text = value.ToString();
			}
		}

		public int publicPageIndex
		{
			get
			{
				return GetFieldInt(publicPageIndexField, 0);
			}
			set
			{
				publicPageIndexField.text = value.ToString();
			}
		}

		public int publicPageItemCount
		{
			get
			{
				return GetFieldInt(publicPageItemCountField, 0);
			}
			set
			{
				publicPageItemCountField.text = value.ToString();
			}
		}

		public int publicInstanceId
		{
			get
			{
				return GetFieldInt(publicInstanceIdField, 0);
			}
			set
			{
				publicInstanceIdField.text = value.ToString();
			}
		}

		public int publicInstanceCount
		{
			get
			{
				return GetFieldInt(publicInstanceCountField, 1);
			}
			set
			{
				publicInstanceCountField.text = value.ToString();
			}
		}

		private static int GetFieldInt(InputField f, int d)
		{
			int result = d;
			if (!f)
			{
				return result;
			}
			int.TryParse(f.text, out result);
			return result;
		}

		protected void Start()
		{
			List<Component> list = new List<Component>
			{
				drlButtonTab, amazonButtonTab, publicButtonTab, drlEndpointField, drlPageIndexField, drlPageItemCountField, drlMapField, drlTrackField, drlCustomMapField, drlRunButton,
				drlCancelButton, amazonBucketField, amazonFolderFromField, amazonFolderToField, amazonInstanceIdField, amazonInstanceCountField, amazonBatchCountField, amazonFileCountField, amazonRunButton, amazonCancelButton,
				publicPageIndexField, publicPageItemCountField, publicInstanceIdField, publicInstanceCountField, publicRunButton, publicCancelButton
			};
			for (int i = 0; i < list.Count; i++)
			{
				Component it = list[i];
				if (it is InputField)
				{
					InputField inputField = (InputField)it;
					if ((bool)inputField)
					{
						inputField.onEndEdit.AddListener(delegate(string v)
						{
							OnFieldEvent(it.transform.parent.name, it, v);
						});
					}
				}
				if (!(it is Button))
				{
					continue;
				}
				Button button = (Button)it;
				if ((bool)button)
				{
					button.onClick.AddListener(delegate
					{
						OnFieldEvent(it.name, it);
					});
				}
			}
			amazonInstanceCount = 1;
			amazonFileCount = 30;
			amazonBatchCount = 1;
			drlPageIndex = 0;
			drlPageItemCount = 1000;
			drlMap = "MP-103";
			drlCustomMap = "CMP-5498a622a39bc3973543c4aa";
			SetMenu("drl");
			drlPageIndex = 0;
		}

		public void ClearJobs()
		{
			for (int i = 0; i < jobList.Count; i++)
			{
				UIReplayConvertJobItem uIReplayConvertJobItem = jobList[i];
				if ((bool)uIReplayConvertJobItem)
				{
					uIReplayConvertJobItem.Clear();
					UnityEngine.Object.Destroy(uIReplayConvertJobItem.gameObject);
				}
			}
			jobList.Clear();
		}

		public void SetJobs(List<ReplayConvertJob> p_list)
		{
			for (int i = 0; i < p_list.Count; i++)
			{
				ReplayConvertJob job = p_list[i];
				UIReplayConvertJobItem uIReplayConvertJobItem = UnityEngine.Object.Instantiate(jobItemTemplate);
				uIReplayConvertJobItem.Clear();
				uIReplayConvertJobItem.job = job;
				uIReplayConvertJobItem.name = i.ToString("00") ?? "";
				uIReplayConvertJobItem.transform.SetParent(jobsGridLayout.transform, worldPositionStays: false);
				jobList.Add(uIReplayConvertJobItem);
			}
		}

		public void OnFieldEvent(string p_name, Component p_target, params object[] p_data)
		{
			if (p_name == null)
			{
				return;
			}
			switch (p_name)
			{
			case "button-drl-cancel":
				break;
			case "button-amzn-cancel":
				break;
			case "tab-button-drl":
				SetMenu("drl");
				break;
			case "tab-button-amzn":
				SetMenu("amazon");
				break;
			case "tab-button-public":
				SetMenu("public");
				break;
			case "button-drl-run":
			{
				if (running)
				{
					break;
				}
				SetRunning(p_flag: true);
				string c_status3 = "Initializing...";
				int c_batch_idx3 = 0;
				float elapsed3 = 0f;
				List<float> file_process_rates3 = new List<float>();
				float k_processed_files_per_hour3 = 0f;
				int processed_files3 = 0;
				int processed_v1_mb3 = 0;
				int processed_v2_mb3 = 0;
				float processed_t2 = 0f;
				string jsf_s3 = "";
				Activity.Run((Func<bool>)delegate
				{
					if (!running)
					{
						return false;
					}
					int num = 0;
					elapsed3 += Time.deltaTime;
					TimeSpan timeSpan = new TimeSpan(0, 0, (int)elapsed3);
					string text = timeSpan.Days.ToString("0");
					string text2 = timeSpan.Hours.ToString("00");
					string text3 = timeSpan.Minutes.ToString("00");
					string text4 = timeSpan.Seconds.ToString("00");
					jsf_s3 = "";
					jsf_s3 = jsf_s3 + "Elapsed " + text + "D " + text2 + "h " + text3 + "m " + text4 + "s <color=#f00>|</color> ";
					jsf_s3 += string.Format("Batch {0}{1} <color=#f00>|</color> ", c_batch_idx3 + 1, (num > 0) ? $" of {num}" : "");
					if (k_processed_files_per_hour3 > 0f)
					{
						jsf_s3 += string.Format("Rate {0}K F/H - {1} Files <color=#f00>|</color> ", k_processed_files_per_hour3.ToString("0.0"), processed_files3);
					}
					jsf_s3 += $"v1 @ {processed_v1_mb3}mb <color=#f00>|</color> ";
					jsf_s3 += $"v2 @ {processed_v2_mb3}mb <color=#f00>|</color> ";
					float num2 = ((processed_v1_mb3 <= 0) ? 0f : Mathf.Clamp01(1f - (float)processed_v2_mb3 / (float)processed_v1_mb3));
					if (num2 > 0f)
					{
						jsf_s3 += $"Reduction {(int)(num2 * 100f)}% <color=#f00>|</color> ";
					}
					jsf_s3 += c_status3;
					jobStatusField.text = jsf_s3;
					return true;
				}, 0f, false);
				converter.ConvertReplaysFromDRLConversionAPI(drlPageIndex, drlPageItemCount, drlMap, drlTrack, drlCustomMap, delegate(string p_state, int p_batch_idx)
				{
					c_batch_idx3 = p_batch_idx;
					switch (p_state)
					{
					case "fetch-files":
						ClearJobs();
						c_status3 = "Loading Files...";
						break;
					case "fetch-files-completed":
						c_status3 = "Load Complete!";
						break;
					case "process-jobs":
						processed_t2 = elapsed3;
						c_status3 = "Processing Jobs!";
						SetJobs(converter.jobs);
						break;
					case "jobs-completed":
					{
						float num = elapsed3 - processed_t2;
						num = num / 60f / 60f;
						float item = (float)converter.jobsCompleteCount / 1000f / num;
						file_process_rates3.Add(item);
						float num2 = 0f;
						for (int num3 = 0; num3 < file_process_rates3.Count; num3++)
						{
							num2 += file_process_rates3[num3] / (float)file_process_rates3.Count;
						}
						processed_files3 += converter.jobs.Count;
						processed_v1_mb3 += converter.jobsV1LengthKb / 1024;
						processed_v2_mb3 += converter.jobsV2LengthKb / 1024;
						k_processed_files_per_hour3 = num2;
						if (!string.IsNullOrEmpty(jsf_s3))
						{
							Debug.Log("ReplayConvertDashboard> Jobs Status\n  " + jsf_s3);
						}
						c_status3 = "All Jobs Finished!";
						break;
					}
					case "error":
						c_status3 = "Error";
						break;
					case "conversion-completed":
						c_status3 = "Conversion Complete!";
						Activity.RunOnce(delegate
						{
							SetRunning(p_flag: false);
						}, 0.05f);
						break;
					}
				});
				break;
			}
			case "button-amzn-run":
			{
				if (running)
				{
					break;
				}
				SetRunning(p_flag: true);
				string c_status2 = "Initializing...";
				int c_batch_idx2 = 0;
				float elapsed2 = 0f;
				List<float> file_process_rates2 = new List<float>();
				float k_processed_files_per_hour2 = 0f;
				int processed_files2 = 0;
				int processed_v1_mb2 = 0;
				int processed_v2_mb2 = 0;
				float processed_t1 = 0f;
				string jsf_s2 = "";
				Activity.Run((Func<bool>)delegate
				{
					if (!running)
					{
						return false;
					}
					int num = amazonBatchCount;
					elapsed2 += Time.deltaTime;
					TimeSpan timeSpan = new TimeSpan(0, 0, (int)elapsed2);
					string text = timeSpan.Days.ToString("0");
					string text2 = timeSpan.Hours.ToString("00");
					string text3 = timeSpan.Minutes.ToString("00");
					string text4 = timeSpan.Seconds.ToString("00");
					jsf_s2 = "";
					jsf_s2 = jsf_s2 + "Elapsed " + text + "D " + text2 + "h " + text3 + "m " + text4 + "s <color=#f00>|</color> ";
					jsf_s2 += string.Format("Batch {0}{1} <color=#f00>|</color> ", c_batch_idx2 + 1, (num > 0) ? $" of {num}" : "");
					if (k_processed_files_per_hour2 > 0f)
					{
						jsf_s2 += string.Format("Rate {0}K F/H - {1} Files <color=#f00>|</color> ", k_processed_files_per_hour2.ToString("0.0"), processed_files2);
					}
					jsf_s2 += $"v1 @ {processed_v1_mb2}mb <color=#f00>|</color> ";
					jsf_s2 += $"v2 @ {processed_v2_mb2}mb <color=#f00>|</color> ";
					float num2 = ((processed_v1_mb2 <= 0) ? 0f : Mathf.Clamp01(1f - (float)processed_v2_mb2 / (float)processed_v1_mb2));
					if (num2 > 0f)
					{
						jsf_s2 += $"Reduction {(int)(num2 * 100f)}% <color=#f00>|</color> ";
					}
					jsf_s2 += c_status2;
					jobStatusField.text = jsf_s2;
					return true;
				}, 0f, false);
				converter.ConvertReplaysFromAmazonBucket(amazonBucket, amazonFolderFrom, amazonBucket, amazonFolderTo, amazonFileCount, amazonBatchCount, amazonInstanceCount, amazonInstanceId, delegate(string p_state, int p_batch_idx)
				{
					c_batch_idx2 = p_batch_idx;
					switch (p_state)
					{
					case "fetch-files":
						ClearJobs();
						c_status2 = "Loading Files...";
						break;
					case "fetch-files-completed":
						c_status2 = "Load Complete!";
						break;
					case "process-jobs":
						processed_t1 = elapsed2;
						c_status2 = "Processing Jobs!";
						SetJobs(converter.jobs);
						break;
					case "jobs-completed":
					{
						float num = elapsed2 - processed_t1;
						num = num / 60f / 60f;
						float item = (float)converter.jobsCompleteCount / 1000f / num;
						file_process_rates2.Add(item);
						float num2 = 0f;
						for (int num3 = 0; num3 < file_process_rates2.Count; num3++)
						{
							num2 += file_process_rates2[num3] / (float)file_process_rates2.Count;
						}
						processed_files2 += converter.jobs.Count;
						processed_v1_mb2 += converter.jobsV1LengthKb / 1024;
						processed_v2_mb2 += converter.jobsV2LengthKb / 1024;
						k_processed_files_per_hour2 = num2;
						if (!string.IsNullOrEmpty(jsf_s2))
						{
							Debug.Log("ReplayConvertDashboard> Jobs Status\n  " + jsf_s2);
						}
						c_status2 = "All Jobs Finished!";
						break;
					}
					case "error":
						c_status2 = "Error";
						break;
					case "conversion-completed":
						c_status2 = "Conversion Complete!";
						Activity.RunOnce(delegate
						{
							SetRunning(p_flag: false);
						}, 0.05f);
						break;
					}
				});
				break;
			}
			case "button-public-run":
			{
				if (running)
				{
					break;
				}
				SetRunning(p_flag: true);
				string c_status = "Initializing...";
				int c_batch_idx = 0;
				float elapsed = 0f;
				List<float> file_process_rates = new List<float>();
				float k_processed_files_per_hour = 0f;
				int processed_files = 0;
				int processed_v1_mb = 0;
				int processed_v2_mb = 0;
				float processed_t0 = 0f;
				string jsf_s = "";
				Activity.Run((Func<bool>)delegate
				{
					if (!running)
					{
						return false;
					}
					int num = 0;
					elapsed += Time.deltaTime;
					TimeSpan timeSpan = new TimeSpan(0, 0, (int)elapsed);
					string text = timeSpan.Days.ToString("0");
					string text2 = timeSpan.Hours.ToString("00");
					string text3 = timeSpan.Minutes.ToString("00");
					string text4 = timeSpan.Seconds.ToString("00");
					jsf_s = "";
					jsf_s = jsf_s + "Elapsed " + text + "D " + text2 + "h " + text3 + "m " + text4 + "s <color=#f00>|</color> ";
					jsf_s += string.Format("Batch {0}{1} <color=#f00>|</color> ", c_batch_idx + 1, (num > 0) ? $" of {num}" : "");
					if (k_processed_files_per_hour > 0f)
					{
						jsf_s += string.Format("Rate {0}K F/H - {1} Files <color=#f00>|</color> ", k_processed_files_per_hour.ToString("0.0"), processed_files);
					}
					jsf_s += $"v1 @ {processed_v1_mb}mb <color=#f00>|</color> ";
					jsf_s += $"v2 @ {processed_v2_mb}mb <color=#f00>|</color> ";
					float num2 = ((processed_v1_mb <= 0) ? 0f : Mathf.Clamp01(1f - (float)processed_v2_mb / (float)processed_v1_mb));
					if (num2 > 0f)
					{
						jsf_s += $"Reduction {(int)(num2 * 100f)}% <color=#f00>|</color> ";
					}
					jsf_s += c_status;
					jobStatusField.text = jsf_s;
					return true;
				}, 0f, false);
				converter.ConvertReplaysFromPublicConversionAPI(publicPageIndex, publicPageItemCount, delegate(string p_state, int p_batch_idx)
				{
					c_batch_idx = p_batch_idx;
					switch (p_state)
					{
					case "fetch-files":
						ClearJobs();
						c_status = "Loading Files...";
						break;
					case "fetch-files-completed":
						c_status = "Load Complete!";
						break;
					case "process-jobs":
						processed_t0 = elapsed;
						c_status = "Processing Jobs!";
						SetJobs(converter.jobs);
						break;
					case "jobs-completed":
					{
						float num = elapsed - processed_t0;
						num = num / 60f / 60f;
						float item = (float)converter.jobsCompleteCount / 1000f / num;
						file_process_rates.Add(item);
						float num2 = 0f;
						for (int num3 = 0; num3 < file_process_rates.Count; num3++)
						{
							num2 += file_process_rates[num3] / (float)file_process_rates.Count;
						}
						processed_files += converter.jobs.Count;
						processed_v1_mb += converter.jobsV1LengthKb / 1024;
						processed_v2_mb += converter.jobsV2LengthKb / 1024;
						k_processed_files_per_hour = num2;
						if (!string.IsNullOrEmpty(jsf_s))
						{
							Debug.Log("ReplayConvertDashboard> Jobs Status\n  " + jsf_s);
						}
						c_status = "All Jobs Finished!";
						break;
					}
					case "error":
						c_status = "Error";
						break;
					case "conversion-completed":
						c_status = "Conversion Complete!";
						Activity.RunOnce(delegate
						{
							SetRunning(p_flag: false);
						}, 0.05f);
						break;
					}
				});
				break;
			}
			}
		}

		public void SetRunning(bool p_flag)
		{
			running = p_flag;
			Button button = drlRunButton;
			button.transform.Find("field").GetComponent<Text>().text = (p_flag ? "Wait..." : "Run");
			button.transform.Find("spinner-loader-prop").gameObject.SetActive(p_flag);
			Button button2 = amazonRunButton;
			button2.transform.Find("field").GetComponent<Text>().text = (p_flag ? "Wait..." : "Run");
			button2.transform.Find("spinner-loader-prop").gameObject.SetActive(p_flag);
			Button button3 = publicRunButton;
			button3.transform.Find("field").GetComponent<Text>().text = (p_flag ? "Wait..." : "Run");
			button3.transform.Find("spinner-loader-prop").gameObject.SetActive(p_flag);
		}

		public void SetMenu(string p_menu)
		{
			menuDRL.gameObject.SetActive(value: false);
			menuAmazon.gameObject.SetActive(value: false);
			menuPublic.gameObject.SetActive(value: false);
			FadeComponent component = drlButtonTab.GetComponent<FadeComponent>();
			component.Fade(0.1f, 0.1f);
			component = amazonButtonTab.GetComponent<FadeComponent>();
			component.Fade(0.1f, 0.1f);
			component = publicButtonTab.GetComponent<FadeComponent>();
			component.Fade(0.1f, 0.1f);
			switch (p_menu)
			{
			case "drl":
				component = drlButtonTab.GetComponent<FadeComponent>();
				menuDRL.gameObject.SetActive(value: true);
				break;
			case "amazon":
				component = amazonButtonTab.GetComponent<FadeComponent>();
				menuAmazon.gameObject.SetActive(value: true);
				break;
			case "public":
				component = publicButtonTab.GetComponent<FadeComponent>();
				menuPublic.gameObject.SetActive(value: true);
				break;
			}
			component.FadeIn(0.1f);
		}
	}
}
