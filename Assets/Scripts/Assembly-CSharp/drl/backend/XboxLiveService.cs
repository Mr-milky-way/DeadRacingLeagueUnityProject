using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using thelab.core;

namespace drl.backend
{
	public class XboxLiveService : PlatformService
	{
		public class TextValidateJob
		{
			public string text;

			public bool result;

			public string key;

			public Action<bool, string> callback;

			public static int CacheSort(TextValidateJob a, TextValidateJob b)
			{
				return string.Compare(a.key, b.key);
			}

			public void Invoke()
			{
				if (callback != null)
				{
					callback(result, result ? text : "");
				}
			}
		}

		private List<TextValidateJob> m_tvj_cache_queue;

		private List<TextValidateJob> m_tvj_cache;

		private List<TextValidateJob> m_tvj_service_queue;

		private List<TextValidateJob> m_tvj_result_queue;

		private Activity m_tvj_service_loop;

		private Thread m_tvj_cache_loop;

		private bool m_tvj_cache_dirty;

		private int m_tvj_service_active_count;

		public string networkXSTSToken;

		private Activity m_login_timeout_timer;

		private int m_login_retry;

		private bool m_refresh_cbl_active;

		private Activity m_purchase_polling;

		public bool hasUser => false;

		public string userOnlineId => "";

		public bool hasNetworkToken
		{
			get
			{
				if (hasUser)
				{
					return !string.IsNullOrEmpty(networkXSTSToken);
				}
				return false;
			}
		}

		public override void Initialize()
		{
			if (base.ready)
			{
				Debug.Log("XboxLiveService> Already Initialized...");
				return;
			}
			countryISO = Localization.LanguageToCountryISO2(Application.systemLanguage);
			switch (Application.systemLanguage)
			{
			case SystemLanguage.Chinese:
			case SystemLanguage.ChineseSimplified:
			case SystemLanguage.ChineseTraditional:
				languageISO = "zh";
				break;
			case SystemLanguage.English:
				languageISO = "en-us";
				break;
			default:
				languageISO = "en-us";
				break;
			}
			Debug.Log($"XboxLiveService> Initialize / country[{countryISO}] language[{languageISO}] system-language[{Application.systemLanguage}]");
			m_tvj_cache_queue = new List<TextValidateJob>();
			m_tvj_service_queue = new List<TextValidateJob>();
			m_tvj_result_queue = new List<TextValidateJob>();
			m_tvj_cache = new List<TextValidateJob>();
			m_tvj_cache_dirty = false;
			m_tvj_service_active_count = 0;
			active = false;
		}

		public override void RefreshFriends(Action p_oncomplete = null)
		{
		}

		public override void RefreshFlags(Action p_oncomplete = null)
		{
			flags.Clear();
		}

		public override void CheckPlatformMultiplayerPrivilege(Action p_oncomplete)
		{
		}

		public override void CheckPlatformUGCPrivilege(Action p_oncomplete)
		{
		}

		public override void CheckPlatformCommunicationPrivilege(Action p_oncomplete)
		{
		}

		public override void IsUserCommunicationBlocked(string p_id, Action<bool> p_on_result)
		{
		}

		public override void TextValidate(string p_input, Action<bool, string> p_on_result, bool p_chatMessage = false)
		{
			if (string.IsNullOrEmpty(p_input))
			{
				base.TextValidate(p_input, p_on_result);
				return;
			}
			TextValidateJob textValidateJob = new TextValidateJob();
			textValidateJob.text = p_input;
			textValidateJob.key = "";
			textValidateJob.result = false;
			textValidateJob.callback = p_on_result;
			m_tvj_cache_queue.Add(textValidateJob);
			Debug.Log($"XboxLiveService> TextValidate / Cache Queue - cache-count[{m_tvj_cache_queue.Count}] service-count[{m_tvj_service_queue.Count}] \n[{p_input.Substring(0, Mathf.Min(p_input.Length, 10))}]");
			_ = m_tvj_cache_loop;
			if (m_tvj_cache_loop == null || m_tvj_cache_loop.ThreadState == ThreadState.Stopped)
			{
				m_tvj_cache_loop = new Thread((ThreadStart)delegate
				{
					while (m_tvj_cache_queue.Count > 0)
					{
						int millisecondsTimeout = 0;
						if (m_tvj_service_queue.Count > 0)
						{
							for (int i = 0; i < m_tvj_service_queue.Count; i++)
							{
								TextValidateJob textValidateJob2 = m_tvj_service_queue[i];
								if (textValidateJob2 != null)
								{
									string text = textValidateJob2.text.ToLower();
									for (int j = 0; j < m_tvj_cache.Count; j++)
									{
										TextValidateJob textValidateJob3 = m_tvj_cache[j];
										if (!(textValidateJob3.key != text))
										{
											textValidateJob2.result = textValidateJob3.result;
											m_tvj_result_queue.Add(textValidateJob2);
											m_tvj_service_queue[i] = null;
										}
									}
								}
							}
						}
						if (m_tvj_cache_queue.Count <= 0)
						{
							Thread.Sleep(millisecondsTimeout);
						}
						else
						{
							if (m_tvj_cache_dirty)
							{
								m_tvj_cache_dirty = false;
								while (m_tvj_cache.Count > 800)
								{
									m_tvj_cache.RemoveAt(0);
								}
								for (int k = 0; k < m_tvj_cache.Count; k++)
								{
									TextValidateJob textValidateJob4 = m_tvj_cache[k];
									if (string.IsNullOrEmpty(textValidateJob4.key))
									{
										textValidateJob4.key = textValidateJob4.text.ToLower();
										textValidateJob4.text = "";
										textValidateJob4.callback = null;
									}
								}
								m_tvj_cache.Sort(TextValidateJob.CacheSort);
								for (int l = 0; l < m_tvj_cache.Count; l++)
								{
									TextValidateJob textValidateJob5 = m_tvj_cache[l];
									for (int m = l + 1; m < m_tvj_cache.Count; m++)
									{
										TextValidateJob textValidateJob6 = m_tvj_cache[m];
										if (textValidateJob5.key == textValidateJob6.key)
										{
											m_tvj_cache.RemoveAt(m--);
										}
									}
								}
							}
							TextValidateJob textValidateJob2 = m_tvj_cache_queue[0];
							m_tvj_cache_queue.RemoveAt(0);
							int count = m_tvj_cache.Count;
							bool flag = false;
							string text = textValidateJob2.text.ToLower();
							for (int n = 0; n < count; n++)
							{
								TextValidateJob textValidateJob7 = m_tvj_cache[n];
								if (!(textValidateJob7.key != text))
								{
									textValidateJob2.result = textValidateJob7.result;
									m_tvj_result_queue.Add(textValidateJob2);
									flag = true;
									break;
								}
							}
							if (flag)
							{
								Thread.Sleep(millisecondsTimeout);
							}
							else
							{
								m_tvj_service_queue.Add(textValidateJob2);
								Thread.Sleep(millisecondsTimeout);
							}
						}
					}
				});
				m_tvj_cache_loop.Priority = System.Threading.ThreadPriority.Lowest;
				m_tvj_cache_loop.Start();
			}
			if (m_tvj_service_loop != null && m_tvj_service_loop.active)
			{
				return;
			}
			if (m_tvj_service_loop != null)
			{
				m_tvj_service_loop.Stop();
			}
			m_tvj_service_loop = null;
			float call_delay = 2f;
			float call_elapsed = 0f;
			m_tvj_service_loop = Activity.Run((Func<bool>)delegate
			{
				TextValidateJob textValidateJob2;
				while (m_tvj_result_queue.Count > 0)
				{
					textValidateJob2 = m_tvj_result_queue[0];
					m_tvj_result_queue.RemoveAt(0);
					textValidateJob2.Invoke();
				}
				if (m_tvj_service_queue.Count <= 0)
				{
					return true;
				}
				if (m_tvj_service_active_count <= 0)
				{
					call_elapsed = call_delay;
				}
				call_elapsed += Time.unscaledDeltaTime;
				if (call_elapsed < call_delay)
				{
					return true;
				}
				call_elapsed = 0f;
				textValidateJob2 = m_tvj_service_queue[0];
				m_tvj_service_queue.RemoveAt(0);
				if (textValidateJob2 == null)
				{
					return true;
				}
				VerifyTextValidateJob(textValidateJob2);
				return true;
			}, 0f, false);
		}

		protected void VerifyTextValidateJob(TextValidateJob p_job)
		{
			m_tvj_service_active_count++;
		}

		public void RefreshCommunicationBlockList()
		{
		}

		public override void UpdateAchievement(string p_id, float p_progress, Action p_oncomplete)
		{
		}

		public void GetAchievements(Action<IList> p_oncomplete)
		{
		}

		public void MultiplayerInviteValidate(string p_protocol_uri)
		{
			if (string.IsNullOrEmpty(p_protocol_uri))
			{
				return;
			}
			Dictionary<string, string> dictionary = ParseXboxActivationProtocol(new Uri(p_protocol_uri));
			Debug.Log("XboxLiveService> MultiplayerInviteValidate / " + p_protocol_uri.ToLower());
			string owner_id = (dictionary.ContainsKey("owner-xuid") ? dictionary["owner-xuid"] : "");
			string player_id = (dictionary.ContainsKey("player-xuid") ? dictionary["player-xuid"] : "");
			if (dictionary.ContainsKey("handle"))
			{
				_ = dictionary["handle"];
			}
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				Debug.Log("XboxLiveService>    " + item.Key + ": " + item.Value);
			}
			if (string.IsNullOrEmpty(owner_id))
			{
				return;
			}
			string text = owner_id;
			Debug.Log("XboxLiveService> MultiplayerInviteValidate / query[" + text + "]");
			MultiplayerSessionFindByOwner(text, delegate(object p_session_name)
			{
				string text2 = (string)p_session_name;
				Debug.Log("XboxLiveService> MultiplayerInviteValidate / session-name[" + (string.IsNullOrEmpty(text2) ? "<null>" : text2) + "]");
				string[] array = (string.IsNullOrEmpty(text2) ? "" : text2.ToString()).Split('_');
				if (array.Length != 0)
				{
					_ = array[0];
				}
				string room_region = ((array.Length <= 1) ? "" : array[1]);
				string room_id = ((array.Length <= 2) ? "" : array[2]);
				string crossplay_flag = (text2.Contains("xbl-xbox") ? "xbox" : "all");
				Debug.Log("XboxLiveService> MultiplayerInviteValidate / Checking Multiplayer Privilege!");
				CheckPlatformMultiplayerPrivilege(delegate
				{
					if (!ContainsFlag(PlatformServiceFlagType.XBoxMultiplayerAllowed))
					{
						Debug.Log("XboxLiveService> MultiplayerInviteValidate / Invite Declined - MP not Allowed");
					}
					else
					{
						AddInvite(owner_id, player_id, room_region, room_id, crossplay_flag);
					}
				});
			});
		}

		public Dictionary<string, string> ParseXboxActivationProtocol(Uri p_activation_uri)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (p_activation_uri == null)
			{
				return dictionary;
			}
			string text = p_activation_uri.ToString().ToLower();
			string[] array = text.Replace("://", "/").Replace("/?", "/").Split('/');
			if (array.Length <= 1)
			{
				Debug.LogWarning("XboxLiveService> ParseActivationProtocol / Bad URI - [" + text + "]");
				return dictionary;
			}
			string value = array[0];
			string text2 = ((array.Length >= 2) ? array[1] : "");
			string obj = ((array.Length >= 3) ? array[2] : "");
			dictionary["uri-protocol"] = value;
			dictionary["uri-path"] = text2;
			string[] array2 = obj.Split('&');
			for (int i = 0; i < array2.Length; i++)
			{
				string[] array3 = array2[i].Split('=');
				if (array3.Length == 0)
				{
					continue;
				}
				string text3 = array3[0].Trim();
				string value2 = (dictionary[text3] = ((array3.Length > 1) ? array3[1].Trim() : ""));
				switch (text2)
				{
				case "activityhandlejoin":
				case "invitehandleaccept":
					if (text3 == "invitedxuid")
					{
						dictionary["player-xuid"] = value2;
					}
					if (text3 == "senderxuid")
					{
						dictionary["owner-xuid"] = value2;
					}
					if (text3 == "joinerxuid")
					{
						dictionary["player-xuid"] = value2;
					}
					if (text3 == "joineexuid")
					{
						dictionary["owner-xuid"] = value2;
					}
					break;
				}
			}
			return dictionary;
		}

		public void MultiplayerSessionStart(string p_session_name, Action<object> p_on_complete)
		{
		}

		public void MultiplayerSessionJoin(string p_session_name, Action<object> p_on_complete)
		{
		}

		public void MultiplayerSessionLeave(string p_session_name, Action<object> p_on_complete = null)
		{
		}

		public void MultiplayerSessionList(string p_session_name)
		{
		}

		public void MultiplayerSessionFindByOwner(string p_xbuid, Action<object> p_on_complete)
		{
		}

		public void MultiplayerSessionFindByURI(string p_protocol_uri, Action<object> p_on_complete)
		{
		}

		public void MultiplayerSessionFindByHandle(string p_handle, Action<object> p_on_complete)
		{
		}

		public void MultiplayerSessionInviteFriends(string p_session_name, Action<object> p_on_complete)
		{
		}

		public void RefreshStore()
		{
		}

		public override void PurchaseProduct(string p_id, Action<bool, string> p_on_result)
		{
		}

		protected void OnDestroy()
		{
		}
	}
}
