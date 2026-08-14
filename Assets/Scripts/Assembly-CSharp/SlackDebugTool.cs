using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using drl.backend;
using drl.game;
using thelab.core;
using thelab.mvc;

public class SlackDebugTool : Controller<DRLApp>
{
	private static ServiceModel m_service;

	private static bool m_skip_log_delete;

	[SerializeField]
	private bool workInEditor;

	[SerializeField]
	private int sessionExceptionsAllowed = 1;

	[SerializeField]
	private bool replaySwitchMode = true;

	public static bool launchDebugFlag;

	private int m_session_exceptions;

	private float m_log_exception_cooldown;

	private string m_log_path;

	private StreamWriter m_log_writer;

	private Thread m_log_write_thd;

	private List<string> m_log_buffer;

	private int m_log_buffer_count;

	private Stopwatch m_log_clock;

	private static bool m_activated;

	private float m_force_reset_cooldown;

	private float m_log_submission_cooldown;

	private bool m_log_submission_lock;

	private float m_replay_version_cooldown;

	private float m_log_activation_cooldown;

	private float m_log_activation_count;

	private float m_fpstracker_activation_cooldown;

	public ServiceModel service
	{
		get
		{
			if ((bool)m_service)
			{
				return m_service;
			}
			if ((bool)base.app && (bool)base.app.model)
			{
				m_service = base.app.model.service;
			}
			return m_service;
		}
	}

	public bool skipLogDelete
	{
		get
		{
			return m_skip_log_delete;
		}
		set
		{
			m_skip_log_delete = value;
		}
	}

	public void OnPersistency()
	{
		InitDebug();
	}

	[ContextMenu("Init")]
	protected void InitDebug()
	{
		bool flag = launchDebugFlag;
		if (UnityEngine.Debug.isDebugBuild)
		{
			flag = true;
		}
		UnityEngine.Debug.unityLogger.logEnabled = true;
		if (flag)
		{
			ActivateDebug(!skipLogDelete);
			skipLogDelete = false;
		}
		UnityEngine.Debug.Log($"SlackDebugTool> InitDebug / [{DRLVersion.value}] [{DateTime.Now}] allowed[{flag}]");
		if (workInEditor || !Application.isEditor)
		{
			UnityEngine.Debug.Log("SlackDebugTool> InitDebug / Listening");
			Application.logMessageReceivedThreaded -= OnLogMessage;
			Application.logMessageReceivedThreaded += OnLogMessage;
		}
	}

	protected void ActivateDebug(bool p_clear_log)
	{
		if (m_activated)
		{
			return;
		}
		m_activated = true;
		UnityEngine.Debug.unityLogger.logEnabled = true;
		m_log_path = DRLPaths.Storage.consoleLogFile;
		if (p_clear_log)
		{
			try
			{
				if (File.Exists(m_log_path))
				{
					File.Delete(m_log_path);
				}
			}
			catch (Exception)
			{
			}
		}
		FileStream stream = new FileStream(m_log_path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
		m_log_buffer = new List<string>();
		m_log_buffer_count = 0;
		m_log_writer = new StreamWriter(stream);
		AssertLogWriterThread();
		m_log_clock = new Stopwatch();
		m_log_clock.Start();
		UnityEngine.Debug.Log("SlackDebugTool> ActivateDebug / Custom Logging Active");
		Application.logMessageReceivedThreaded -= OnLogStorage;
		Application.logMessageReceivedThreaded += OnLogStorage;
		UnityEngine.Debug.Log($"SlackDebugTool> ActivateDebug / [{DRLVersion.value}] [{DateTime.Now}] clear[{p_clear_log}]");
	}

	private void AssertLogWriterThread()
	{
		bool flag = false;
		if (m_log_write_thd != null && (m_log_write_thd.IsAlive || m_log_write_thd.ThreadState == System.Threading.ThreadState.Running))
		{
			flag = true;
		}
		if (!flag)
		{
			m_log_write_thd = new Thread(LogThreadLoop);
			m_log_write_thd.Priority = System.Threading.ThreadPriority.Lowest;
			m_log_write_thd.Start();
		}
	}

	protected void LogThreadLoop()
	{
		FileInfo fileInfo = new FileInfo(m_log_path);
		long num = 31457280L;
		int num2 = 0;
		Task task = null;
		int num3 = 0;
		while (true)
		{
			StreamWriter log_writer = m_log_writer;
			if (log_writer == null || !log_writer.BaseStream.CanWrite)
			{
				break;
			}
			switch (num2)
			{
			case 0:
				fileInfo.Refresh();
				if (fileInfo.Length >= num)
				{
					return;
				}
				if (m_log_buffer_count > 0)
				{
					num2 = 1;
				}
				break;
			case 1:
			{
				if (m_log_buffer.Count <= 0)
				{
					num2 = 0;
					m_log_buffer_count = 0;
					break;
				}
				string value = m_log_buffer[0];
				m_log_buffer.RemoveAt(0);
				m_log_buffer_count--;
				task = log_writer.WriteLineAsync(value);
				num3 = 2;
				num2 = 10;
				break;
			}
			case 2:
				task = log_writer.FlushAsync();
				num3 = 0;
				num2 = 10;
				break;
			case 10:
				if (task == null)
				{
					num2 = 0;
				}
				else if (task.IsCompleted)
				{
					num2 = num3;
					task = null;
				}
				break;
			}
			Thread.Sleep(0);
		}
	}

	protected void OnLogStorage(string p_condition, string p_stack_trace, LogType p_type)
	{
		string text = "L";
		switch (p_type)
		{
		case LogType.Warning:
			text = "W";
			break;
		case LogType.Log:
			text = "L";
			break;
		case LogType.Error:
			text = "E";
			break;
		case LogType.Exception:
			text = "X";
			break;
		case LogType.Assert:
			text = "A";
			break;
		}
		float num = ((m_log_clock == null) ? 0 : m_log_clock.ElapsedMilliseconds);
		num /= 1000f;
		p_stack_trace = ((p_type != LogType.Error && p_type != LogType.Exception) ? "" : ("\n" + p_stack_trace));
		m_log_buffer.Add("[" + Format.SecondsToTime(num, 2, p_use_ms: true) + "][" + text + "] " + p_condition + p_stack_trace);
		m_log_buffer_count++;
		if (m_log_buffer_count >= 2 && UnityEngine.Debug.unityLogger.logEnabled)
		{
			AssertLogWriterThread();
		}
	}

	protected void OnLogMessage(string p_condition, string p_stack_trace, LogType p_type)
	{
		if (p_type == LogType.Exception && m_session_exceptions < sessionExceptionsAllowed)
		{
			m_session_exceptions++;
			Activity.RunOnce(delegate
			{
				ReportToSlack(p_condition, p_stack_trace);
			}, 1f / 60f);
		}
	}

	public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
	{
		if (Application.isEditor)
		{
			return;
		}
		switch (p_event)
		{
		case "service.login@error":
		case "service.state@error":
		case "service.state.game@error":
		{
			DRLServiceResult dRLServiceResult = (DRLServiceResult)p_data[0];
			if (dRLServiceResult == null)
			{
				ReportToSlack("DRLService> Backend Call Failure: event[" + p_event + "]", "Service Result was <null>");
			}
			else if ((dRLServiceResult.request == null || !dRLServiceResult.request.cancelled) && (dRLServiceResult.request == null || dRLServiceResult.request.code != 401))
			{
				string text = "";
				if (dRLServiceResult.data != null)
				{
					text = (dRLServiceResult.encoded ? Serialize.FromBase64<string>(dRLServiceResult.data as string) : (dRLServiceResult.data as string));
				}
				if (string.IsNullOrEmpty(text))
				{
					text = "No stack trace sent from backend";
				}
				if (string.IsNullOrEmpty(dRLServiceResult.message))
				{
					dRLServiceResult.message = "No error message sent from backend";
				}
				if (string.IsNullOrEmpty(dRLServiceResult.id))
				{
					dRLServiceResult.id = (service ? service.backend.playerId.ToString() : "<null>");
				}
				ReportToSlack("DRLService> Backend Call Failure: event[" + p_event + "]  player-id[" + dRLServiceResult.id + "] message[" + dRLServiceResult.message + "]", text);
			}
			break;
		}
		}
	}

	public void ReportToSlack(string p_message, string p_stack_trace)
	{
		string p_error_message = (string.IsNullOrEmpty(p_message) ? "<no log>" : p_message);
		string p_stack_trace2 = (string.IsNullOrEmpty(p_stack_trace) ? "<no stack trace>" : p_stack_trace);
		string slack_msg = CreateSlackMessage(p_error_message, p_stack_trace2);
		UploadImage(delegate(string p_image_url, string p_image_filename)
		{
			UploadConsoleLog(delegate(string p_log_url, string p_log_filename)
			{
				if (!string.IsNullOrEmpty(p_log_url))
				{
					string arg = DRLService.baseUri + "/logs?file=" + p_log_url;
					slack_msg += $"\n*Log:* <{p_log_url}|Download> | <{arg}|Open>\n";
				}
				if (!string.IsNullOrEmpty(p_image_url))
				{
					slack_msg += string.Format("\n*Screenshot:* <{0}|{1}>", p_image_url, p_image_filename + ".jpg");
				}
				slack_msg += "\n\n\n";
				PostToSlack(slack_msg);
			});
		});
	}

	private string CreateSlackMessage(string p_error_message, string p_stack_trace)
	{
		string full = DRLVersion.full;
		string text = Application.platform.ToString();
		string text2 = "<unknown>";
		string arg = "none";
		string text3 = "none";
		string arg2 = "unity-bot";
		string text4 = "0";
		string text5 = "0";
		string text6 = "";
		string arg3 = ":flag-un: ";
		bool flag = false;
		string text7 = "https://dashboard.drlgame.com/players/";
		switch (Application.platform)
		{
		case RuntimePlatform.WindowsEditor:
			text = "win-editor";
			break;
		case RuntimePlatform.OSXEditor:
			text = "osx-editor";
			break;
		case RuntimePlatform.OSXPlayer:
			text = "osx";
			break;
		case RuntimePlatform.WindowsPlayer:
			text = "win";
			break;
		case RuntimePlatform.XboxOne:
			text = "xbox";
			break;
		case RuntimePlatform.PS4:
			text = (OS.IsPS5 ? "ps5" : "ps4");
			break;
		}
		if (base.validContext)
		{
			try
			{
				PlayerStateModel player = base.app.model.storage.state.player;
				arg = player.profile.platformId;
				text3 = player.profile.playerId;
				arg2 = player.profile.username;
				arg3 = $":flag-{player.profile.countryISO.ToLower()}: ";
				text2 = player.profile.branchId;
				text4 = player.settings.graphics.resolution[0].ToString("0");
				text5 = player.settings.graphics.resolution[1].ToString("0");
				text6 = string.Format("@ Q[{0}] HS[{1}]", player.settings.graphics.quality, GraphicsStateModel.GetHardwareScore().ToString("0.00"));
				flag = true;
			}
			catch (Exception)
			{
			}
		}
		string arg4 = (Time.time / 60f / 60f).ToString("00");
		string arg5 = ((int)(Time.time / 60f) % 60).ToString("00");
		string arg6 = ((int)Time.time % 60).ToString("00");
		string text8 = "";
		text8 = $"https://steamcommunity.com/profiles/{arg}";
		string text9 = "";
		text9 += string.Format("{0}  *{1} {5} {2} {5} {3} {5} {4}*  {0}\n", ":beetle:", "DRLSim", full, text, text2, ":black_small_square:");
		text9 += $"*User:* {arg3} {arg2} <{text3}>\n";
		if (flag)
		{
			text9 += string.Format("><{0}|{1}> | <{2}|{3}>\n", text7 + text3, "Dashboard", text8, "Profile");
		}
		text9 += "*Info:*\n";
		text9 += $"```\n";
		text9 += $"{SystemInfo.deviceModel} / {SystemInfo.deviceType} / {text4}x{text5} {text6}\n";
		text9 += $"  os:  {SystemInfo.operatingSystem}\n";
		text9 += $"  cpu: {SystemInfo.processorType} / {SystemInfo.processorCount}C / {SystemInfo.processorFrequency}Mhz / {SystemInfo.systemMemorySize}RAM\n";
		text9 += $"  gpu: {SystemInfo.graphicsDeviceName} / {SystemInfo.graphicsMemorySize}VRAM\n";
		text9 += $"runtime: {arg4}:{arg5}:{arg6}\n";
		text9 += $"```\n";
		text9 += "*Error:*\n";
		text9 += $"```\n";
		text9 = text9 + p_error_message + "\n";
		text9 += $"```\n";
		text9 += "*Stack Trace:*\n";
		text9 += $"```\n";
		text9 += p_stack_trace;
		text9 += $"```\n";
		return EscapeSpecialCharacters(text9);
	}

	private void UploadImage(Action<string, string> p_on_finished)
	{
		StartCoroutine(TryUploadImage(p_on_finished));
	}

	private void UploadConsoleLog(Action<string, string> p_on_finished)
	{
		StartCoroutine(TryUploadConsoleLog(p_on_finished));
	}

	private IEnumerator TryUploadImage(Action<string, string> p_on_finished)
	{
		string text = (Time.time / 60f / 60f).ToString("00");
		string text2 = ((int)(Time.time / 60f) % 60).ToString("00");
		string text3 = ((int)Time.time % 60).ToString("00");
		string file_name = $"{SystemInfo.deviceName.ToLower()}-{text}{text2}{text3}-screenshot";
		bool flag = true;
		if (!service)
		{
			UnityEngine.Debug.LogWarning("SlackDebug> TryUploadImage / Service instance not available");
			flag = false;
		}
		if (flag)
		{
			yield return new WaitForEndOfFrame();
			byte[] p_data = ScreenCapture.CaptureScreenshotAsTexture().EncodeToJPG();
			service.StorageImage("slack-debug", p_data, delegate(string p_url)
			{
				p_on_finished(p_url, file_name);
			});
			yield return new WaitForEndOfFrame();
		}
		else
		{
			p_on_finished("", file_name);
		}
	}

	private IEnumerator TryUploadConsoleLog(Action<string, string> p_on_finished)
	{
		string text = (Time.time / 60f / 60f).ToString("00");
		string text2 = ((int)(Time.time / 60f) % 60).ToString("00");
		string text3 = ((int)Time.time % 60).ToString("00");
		string file_name = $"{SystemInfo.deviceName.ToLower()}-{text}{text2}{text3}-log";
		bool flag = File.Exists(DRLPaths.Tools.consoleLogFile);
		string text4 = ((m_log_writer == null) ? DRLPaths.Tools.consoleLogFile : DRLPaths.Storage.consoleLogFile);
		if (!File.Exists(text4))
		{
			text4 = (flag ? DRLPaths.Tools.consoleLogFile : "");
		}
		if (string.IsNullOrEmpty(text4))
		{
			UnityEngine.Debug.Log("SlackDebugTool> Log capture not supported / " + Application.platform);
			p_on_finished("", file_name);
			yield break;
		}
		UnityEngine.Debug.Log("SlackDebugTool> TryUploadConsoleLog / path[" + text4 + "]");
		bool flag2 = false;
		try
		{
			FileStream fileStream = new FileStream(text4, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			BinaryReader binaryReader = new BinaryReader(fileStream);
			byte[] p_data = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
			fileStream.Close();
			service.StorageLogs(p_data, delegate(string p_url)
			{
				p_on_finished(p_url, file_name);
			});
			flag2 = true;
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.Log("SlackDebugTool> TryUploadConsoleLog / Error\n" + ex.Message);
			p_on_finished("", file_name);
		}
		if (flag2)
		{
			yield return new WaitForEndOfFrame();
		}
	}

	private void PostToSlack(string p_report_json)
	{
		StopAllCoroutines();
		StartCoroutine(TryPostToSlack(p_report_json));
	}

	private IEnumerator TryPostToSlack(string p_report_json)
	{
		string uri = "https://hooks.slack.com/services/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";
		string v = "sim-exceptions-release";
		p_report_json.Contains("Cheat Warning");
		if (p_report_json.Contains("Photon Error"))
		{
			v = "sim-exceptions-photon";
		}
		string k = "icon_emoji";
		string v2 = ":beetle";
		if (base.validContext)
		{
			PlayerStateModel player = base.app.model.storage.state.player;
			k = "icon_url";
			v2 = player.profile.photoURL;
		}
		SerializedData serializedData = new SerializedData();
		serializedData.Set("channel", v);
		serializedData.Set("username", "unity-bot");
		serializedData.Set("text", p_report_json);
		serializedData.Set(k, v2);
		serializedData.ToJson();
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("payload", serializedData.ToJson());
		UnityWebRequest webRequest = UnityWebRequest.Post(uri, wWWForm);
		yield return webRequest.SendWebRequest();
		if (webRequest.isDone && webRequest.responseCode < 400 && !webRequest.isHttpError)
		{
			UnityEngine.Debug.Log($"[PostToSlack] {webRequest.url} returned {webRequest.downloadedBytes}bytes.");
			yield break;
		}
		string arg = (string.IsNullOrEmpty(webRequest.downloadHandler.text) ? webRequest.error : webRequest.downloadHandler.text);
		UnityEngine.Debug.LogError($"{webRequest.url}, failed with the error {arg}.");
	}

	private void Update()
	{
		if (UnityEngine.Debug.isDebugBuild)
		{
			UnityEngine.Debug.developerConsoleVisible = false;
		}
		bool flag = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
		if (Application.isEditor)
		{
			flag = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		}
		bool flag2 = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		bool key = Input.GetKey(KeyCode.F10);
		bool key2 = Input.GetKey(KeyCode.F12);
		bool key3 = Input.GetKey(KeyCode.L);
		bool key4 = Input.GetKey(KeyCode.F9);
		float num = 10f;
		bool key5 = Input.GetKey(KeyCode.JoystickButton7);
		bool key6 = Input.GetKey(KeyCode.JoystickButton4);
		bool key7 = Input.GetKey(KeyCode.JoystickButton5);
		key5 = false;
		key6 = false;
		key7 = false;
		num = 0.16f;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		flag3 = (flag && flag2 && key) || key5;
		flag4 = (!key6 && key7) || (flag && flag2 && key3);
		flag5 = (flag && flag2 && key4) || (key6 && !key7);
		flag6 = key2 || (key6 && key7);
		float deltaTime = Time.deltaTime;
		m_force_reset_cooldown = (flag6 ? (m_force_reset_cooldown + deltaTime) : 0f);
		m_replay_version_cooldown = (flag5 ? (m_replay_version_cooldown + deltaTime) : 0f);
		m_log_submission_cooldown = (flag3 ? (m_log_submission_cooldown + deltaTime) : 0f);
		m_log_activation_cooldown = (flag4 ? (m_log_activation_cooldown + deltaTime) : 0f);
		m_fpstracker_activation_cooldown = (flag5 ? (m_fpstracker_activation_cooldown + deltaTime) : 0f);
		m_log_exception_cooldown = ((m_session_exceptions > 1) ? (m_log_exception_cooldown + deltaTime) : 0f);
		if (m_force_reset_cooldown >= 10f)
		{
			m_force_reset_cooldown = 0f;
			if ((bool)base.app && (bool)base.app.controller && (bool)base.app.controller.plm)
			{
				base.app.controller.plm.ForceReset();
			}
		}
		if (!m_log_submission_lock && m_log_submission_cooldown >= num)
		{
			m_log_submission_cooldown = 0f;
			m_log_submission_lock = true;
			ReportToSlack("<Log Submission>", "");
			thelab.core.Timer.Set(this, "m_log_submission_lock", 3f, false);
		}
		if (m_log_exception_cooldown >= 10f)
		{
			m_log_exception_cooldown = 0f;
			m_session_exceptions = 0;
		}
		if (m_log_activation_cooldown >= 10f)
		{
			m_log_activation_cooldown = 0f;
			ActivateDebug(p_clear_log: true);
			if ((bool)base.app && (bool)base.app.view && (bool)base.app.view.ui && (bool)base.app.view.ui.header)
			{
				base.app.view.ui.header.SetDebug(p_flag: true);
			}
		}
		if (m_fpstracker_activation_cooldown >= 5f)
		{
			m_fpstracker_activation_cooldown = 0f;
			if ((bool)base.app && (bool)base.app.controller)
			{
				base.app.controller.fpsTrackerEnabled = !base.app.controller.fpsTrackerEnabled;
			}
		}
	}

	protected void OnDestroy()
	{
		try
		{
			if (m_log_write_thd != null && m_log_write_thd.IsAlive)
			{
				m_log_write_thd.Abort();
				m_log_write_thd = null;
			}
			if (m_log_clock != null)
			{
				m_log_clock.Stop();
				m_log_clock = null;
			}
			if (m_log_writer != null)
			{
				m_log_writer.FlushAsync();
				m_log_writer.Close();
				m_log_writer = null;
			}
		}
		catch (Exception)
		{
		}
		try
		{
			Application.logMessageReceivedThreaded -= OnLogStorage;
			Application.logMessageReceivedThreaded -= OnLogMessage;
		}
		catch (Exception)
		{
		}
	}

	[ContextMenu("Test Exception")]
	public void TestException()
	{
		throw new Exception("Test Exception");
	}

	[ContextMenu("Test Log")]
	public void TestLog()
	{
		ReportToSlack("<editor test log>", "");
	}

	[ContextMenu("Test Screenshot")]
	public void TestScreenshot()
	{
		UploadImage(delegate(string url, string imageName)
		{
			UnityEngine.Debug.Log("Test Screenshot uploaded at: " + url + " with filename: " + imageName);
		});
	}

	private string EscapeSpecialCharacters(string text)
	{
		return text.Replace("\b", "\\b").Replace("\f", "\\f").Replace("\t", "\\t")
			.Replace("\"", "\\\"")
			.Replace("\\", "\\\\");
	}
}
