using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using drl.network;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class TelemetryRelayController : Controller<DRLApp>
	{
		public string ip;

		public TelemetryRelayState state;

		public int port = 7654;

		public float flushInterval = 0.1f;

		public TcpClient client;

		public StreamWriter clientWriter;

		public StringBuilder clientBuffer;

		private Task m_cl_connect_tsk;

		private Task m_cl_write_tsk;

		private Task m_cl_flush_tsk;

		private float m_cl_flush_time;

		private int m_retry_count;

		private string m_last_standings;

		public string GetServerIP()
		{
			if (!string.IsNullOrEmpty(ip))
			{
				return ip;
			}
			return "127.0.0.1";
		}

		protected void Awake()
		{
			state = TelemetryRelayState.Idle;
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "game.count@complete":
				break;
			case "game.track.load@complete":
				state = TelemetryRelayState.ConnectStart;
				break;
			case "game.race.enabled":
				_ = base.app.model.storage.state.player.profile;
				Write("race-enabled", "");
				break;
			case "game.race.gate@step":
			{
				int num4 = (int)p_data[0];
				int num5 = (int)p_data[1];
				Drone p_drone2 = (Drone)p_data[3];
				float num6 = (float)p_data[4];
				GamePlayerData playerDataByDrone2 = base.app.model.game.GetPlayerDataByDrone(p_drone2);
				Write("gate-step", playerDataByDrone2.upperName, $"{num4},{num5},{num6}");
				break;
			}
			case "game.race.lap@change":
			{
				int num = (int)p_data[0];
				int num2 = (int)p_data[1];
				Drone p_drone = (Drone)p_data[2];
				float num3 = (float)p_data[3];
				GamePlayerData playerDataByDrone = base.app.model.game.GetPlayerDataByDrone(p_drone);
				Write("gate-lap", playerDataByDrone.upperName, $"{num},{num2},{num3}");
				break;
			}
			case "game.race.gate@complete":
			{
				Drone p_drone3 = (Drone)p_data[3];
				float num7 = (float)p_data[4];
				GamePlayerData playerDataByDrone3 = base.app.model.game.GetPlayerDataByDrone(p_drone3);
				Write("gate-complete", playerDataByDrone3.upperName, $"{num7}");
				break;
			}
			case "game.standings@update":
			{
				List<GamePlayerData> list = (List<GamePlayerData>)p_data[0];
				if (list.Count > 0)
				{
					string text = string.Join(",", list.ConvertAll<string>(PlayerDataToName));
					if (!(text == m_last_standings))
					{
						m_last_standings = text;
						Write("standings-update", text);
					}
				}
				break;
			}
			case "network.drone-damage.update":
			{
				NetworkRoom.DamageData damageData = (NetworkRoom.DamageData)p_data[1];
				NetworkActor networkActor = (NetworkActor)p_data[2];
				string p_event2 = (damageData.isCrash ? "drone-crash" : "damage-update");
				Write(p_event2, networkActor.ProfileName.ToUpper(), $"{damageData.bodyDamage},{damageData.prop0Damage},{damageData.prop1Damage},{damageData.prop2Damage},{damageData.prop3Damage}");
				break;
			}
			}
		}

		public void Write(string p_event, string p_player, string p_content)
		{
			if (clientBuffer == null)
			{
				return;
			}
			clientBuffer.Append(p_event);
			if (!string.IsNullOrEmpty(p_event))
			{
				clientBuffer.Append(";");
			}
			if (!string.IsNullOrEmpty(p_player))
			{
				clientBuffer.Append(p_player);
				if (!string.IsNullOrEmpty(p_content))
				{
					clientBuffer.Append(",");
				}
			}
			clientBuffer.Append(p_content);
			clientBuffer.AppendLine();
		}

		public void Write(string p_event, string p_content)
		{
			Write(p_event, "", p_content);
		}

		protected void Update()
		{
			TelemetryRelayState telemetryRelayState = state;
			if ((uint)(telemetryRelayState - 6) <= 2u && (client == null || !client.Connected))
			{
				Debug.LogWarning("TelemetryRelayController> Client is not connected!");
				state = TelemetryRelayState.Idle;
				Activity.RunOnce(delegate
				{
					Debug.Log("TelemetryRelayController> Connection Retry...");
					state = TelemetryRelayState.ConnectStart;
				}, 3f);
			}
			switch (state)
			{
			case TelemetryRelayState.ConnectStart:
			{
				string serverIP = GetServerIP();
				Debug.LogWarning($"TelemetryRelayController> Connecting [{serverIP}:{port}]");
				client = new TcpClient();
				m_cl_connect_tsk = client.ConnectAsync(serverIP, port);
				state = TelemetryRelayState.ConnectWait;
				break;
			}
			case TelemetryRelayState.ConnectWait:
			{
				Task cl_connect_tsk = m_cl_connect_tsk;
				if (cl_connect_tsk == null)
				{
					Debug.LogWarning("TelemetryRelayController> Connection Task is <null>");
					state = TelemetryRelayState.Idle;
				}
				else
				{
					if (!cl_connect_tsk.IsCompleted)
					{
						break;
					}
					if (cl_connect_tsk.IsFaulted || cl_connect_tsk.IsCanceled)
					{
						string text2 = ((cl_connect_tsk.Exception == null) ? "" : ("\n" + cl_connect_tsk.Exception.Message));
						Debug.LogWarning("TelemetryRelayController> Connection Task Failed!" + text2);
						state = TelemetryRelayState.Idle;
						Activity.RunOnce(delegate
						{
							Debug.Log("TelemetryRelayController> Connection Retry...");
							m_retry_count++;
							if (m_retry_count > 3)
							{
								Debug.Log("TelemetryRelayController> Retry Time out");
								state = TelemetryRelayState.Idle;
							}
							else
							{
								state = TelemetryRelayState.ConnectStart;
							}
						}, 2f);
					}
					else
					{
						state = TelemetryRelayState.ConnectSuccess;
					}
				}
				break;
			}
			case TelemetryRelayState.ConnectSuccess:
				Debug.Log("TelemetryRelayController> Connection Success!");
				state = TelemetryRelayState.Buffer;
				clientWriter = new StreamWriter(client.GetStream());
				clientBuffer = new StringBuilder(5120);
				m_cl_flush_time = 0f;
				break;
			case TelemetryRelayState.Buffer:
				m_cl_flush_time += Time.deltaTime;
				if (!(m_cl_flush_time < flushInterval))
				{
					m_cl_flush_time = 0f;
					if (clientBuffer.Length > 0)
					{
						m_cl_write_tsk = clientWriter.WriteAsync(clientBuffer.ToString());
						clientBuffer.Clear();
						state = TelemetryRelayState.WritePoll;
					}
				}
				break;
			case TelemetryRelayState.WritePoll:
			{
				Task cl_write_tsk = m_cl_write_tsk;
				if (cl_write_tsk == null)
				{
					Debug.LogWarning("TelemetryRelayController> Write Task is <null>");
					state = TelemetryRelayState.Buffer;
				}
				else if (cl_write_tsk.IsCompleted)
				{
					if (cl_write_tsk.IsFaulted || cl_write_tsk.IsCanceled)
					{
						string text3 = ((cl_write_tsk.Exception == null) ? "" : ("\n" + cl_write_tsk.Exception.Message));
						Debug.LogWarning("TelemetryRelayController> Write Task Failed!" + text3);
						state = TelemetryRelayState.Buffer;
					}
					else
					{
						m_cl_flush_tsk = clientWriter.FlushAsync();
						state = TelemetryRelayState.FlushPoll;
					}
				}
				break;
			}
			case TelemetryRelayState.FlushPoll:
			{
				Task cl_flush_tsk = m_cl_flush_tsk;
				if (cl_flush_tsk == null)
				{
					Debug.LogWarning("TelemetryRelayController> Flush Task is <null>");
					state = TelemetryRelayState.Buffer;
				}
				else if (cl_flush_tsk.IsCompleted)
				{
					if (cl_flush_tsk.IsFaulted || cl_flush_tsk.IsCanceled)
					{
						string text = ((cl_flush_tsk.Exception == null) ? "" : ("\n" + cl_flush_tsk.Exception.Message));
						Debug.LogWarning("TelemetryRelayController> Flush Task Failed!" + text);
						state = TelemetryRelayState.Buffer;
					}
					else
					{
						state = TelemetryRelayState.Buffer;
					}
				}
				break;
			}
			case TelemetryRelayState.Idle:
			case TelemetryRelayState.ConnectError:
			case TelemetryRelayState.ConnectRetry:
				break;
			}
		}

		private static string PlayerDataToName(GamePlayerData d)
		{
			return d.upperName;
		}

		protected void OnDestroy()
		{
			state = TelemetryRelayState.Idle;
			if (client != null)
			{
				client.Close();
				client.Dispose();
			}
			if (clientWriter != null)
			{
				clientWriter.Close();
				clientWriter.Dispose();
			}
		}
	}
}
