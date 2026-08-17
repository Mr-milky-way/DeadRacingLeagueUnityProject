using System;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class NetworkConnectivityController : Controller<DRLApp>
	{
		public float pingRateOnline = 10f;

		public float pingRateOffline = 20f;

		public float pingTimeout = 10f;

		private bool m_isOnline = true;

		private bool m_wasOnline = true;

		private bool m_running;

		private Activity m_pollActivity;

		private Activity m_pingTimeoutActivity;

		public bool connected;

		public void Init(string p_level)
		{
			if (string.IsNullOrEmpty(p_level) || !(p_level == "splash"))
			{
				connected = !DRLApp.offline && !DRLApp.forceOffline;
				Stop();
				SendPing();
				m_running = true;
			}
		}

		public void Stop()
		{
			if (m_pollActivity != null)
			{
				m_pollActivity.Stop();
				m_pollActivity = null;
			}
			if (m_pingTimeoutActivity != null)
			{
				m_pingTimeoutActivity.Stop();
				m_pingTimeoutActivity = null;
			}
		}

		private void SendPing()
		{
			Ping p = new Ping("1.1.1.1");
			WebAsyncRequest req = null;
			if (base.validContext)
			{
				req = base.app.model.service.ServerTime(null);
			}
			if (m_pingTimeoutActivity != null)
			{
				m_pingTimeoutActivity.Stop();
				m_pingTimeoutActivity = null;
			}
			float timer = 0f;
			m_pingTimeoutActivity = ((Component)this).ActivityRun((Func<bool>)delegate
			{
				bool flag = false;
				if (p.isDone)
				{
					flag = true;
				}
				if (req != null && req.completed && !req.hasError)
				{
					flag = true;
				}
				if (flag)
				{
					OnPingNetwork(success: true);
					return false;
				}
				timer += Time.deltaTime;
				if (timer >= pingTimeout)
				{
					OnPingNetwork(p.isDone);
					return false;
				}
				return true;
			}, 0f);
		}

		private void OnPingNetwork(bool success)
		{
			m_isOnline = success;
			connected = success;
			if (!m_isOnline && m_wasOnline)
			{
				CheckInternetConnectivity(delegate(bool p_success)
				{
					if (!DRLApp.isLoading)
					{
						if (!m_wasOnline && !p_success)
						{
							m_wasOnline = (m_isOnline = p_success);
						}
						else
						{
							m_wasOnline = (m_isOnline = p_success);
							if (!m_isOnline)
							{
								Notify("network.update.offline");
								DRLApp.offline = true;
								Debug.Log("<color=#00ffff>NetworkConnectivityController> User online:" + m_isOnline + "</color>");
							}
						}
					}
				});
			}
			if (m_pollActivity != null)
			{
				m_pollActivity.Stop();
				m_pollActivity = null;
			}
			m_pollActivity = this.ActivityRunOnce(delegate
			{
				SendPing();
			}, m_isOnline ? pingRateOnline : pingRateOffline);
		}

		public void CheckInternetConnectivity(Action<bool> p_callback, int p_retries = 5, float p_attemptRetryDelay = 3f)
		{
			PingStatusPage(delegate(bool p_success)
			{
				if (!p_success)
				{
					p_retries--;
					if (p_retries <= 0)
					{
						if (p_callback != null)
						{
							p_callback(p_success);
						}
					}
					else
					{
						this.TimerRunOnce(delegate
						{
							CheckInternetConnectivity(p_callback, p_retries);
						}, p_attemptRetryDelay);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_success);
				}
			});
		}

		private void PingStatusPage(Action<bool> p_callback)
		{
			WebAsyncRequest req = null;
			if (base.validContext)
			{
				req = base.app.model.service.ServerTime(null, 3);
			}
			if (req == null)
			{
				Debug.LogWarning("NetworkConnectivityController> PingStatusPage / Not a valid context - request not sent.");
				if (p_callback != null)
				{
					p_callback(obj: false);
				}
				return;
			}
			Debug.Log("NetworkConnectivityController> PingStatusPage / Checking Connection...");
			Activity.Run((Func<bool>)delegate
			{
				if (req.completed || req.cancelled)
				{
					bool flag = req.hasError || req.cancelled;
					Debug.Log($"NetworkConnectivityController> PingStatusPage / Check Complete - error[{flag}]");
					if (p_callback != null)
					{
						p_callback(!flag);
					}
					return false;
				}
				return true;
			}, 0f, false);
		}

		private void OnApplicationQuit()
		{
			Stop();
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
		}
	}
}
