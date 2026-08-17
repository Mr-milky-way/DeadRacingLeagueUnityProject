using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using drl;
using drl.game;

namespace thelab.core
{
	public class OS
	{
		public class Process
		{
			public enum IO
			{
				None = 0,
				In = 1,
				Out = 2,
				InOut = 3,
				Error = 4,
				All = 7
			}

			public Thread thread;

			public System.Diagnostics.Process process;

			internal bool m_running;

			private BinaryWriter m_raw_input;

			public bool valid => process != null;

			public bool threaded => thread != null;

			public bool running
			{
				get
				{
					if (valid)
					{
						return m_running;
					}
					return false;
				}
			}

			public StreamReader output
			{
				get
				{
					if (process == null)
					{
						return null;
					}
					if (process.StartInfo == null)
					{
						return null;
					}
					if (!process.StartInfo.RedirectStandardOutput)
					{
						return null;
					}
					try
					{
						return process.StandardOutput;
					}
					catch (Exception ex)
					{
						ex.GetHashCode();
						return null;
					}
				}
			}

			public StreamWriter input
			{
				get
				{
					if (process == null)
					{
						return null;
					}
					if (process.StartInfo == null)
					{
						return null;
					}
					if (!process.StartInfo.RedirectStandardInput)
					{
						return null;
					}
					try
					{
						return process.StandardInput;
					}
					catch (Exception ex)
					{
						ex.GetHashCode();
						return null;
					}
				}
			}

			public BinaryWriter rawInput
			{
				get
				{
					if (m_raw_input != null)
					{
						return m_raw_input;
					}
					StreamWriter streamWriter = input;
					if (streamWriter == null)
					{
						return null;
					}
					if (streamWriter.BaseStream == null)
					{
						return null;
					}
					return m_raw_input = new BinaryWriter(streamWriter.BaseStream);
				}
			}

			public StreamReader error
			{
				get
				{
					if (process == null)
					{
						return null;
					}
					if (process.StartInfo == null)
					{
						return null;
					}
					if (!process.StartInfo.RedirectStandardError)
					{
						return null;
					}
					try
					{
						return process.StandardError;
					}
					catch (Exception ex)
					{
						ex.GetHashCode();
						return null;
					}
				}
			}
		}

		public class AppWindow
		{
			private IntPtr m_hwnd;

			public static AppWindow current => new AppWindow();

			public static AppWindow foreground => new AppWindow(OS.foreground);

			public virtual IntPtr Handle => m_hwnd;

			public AppWindow(IntPtr p_hwnd)
			{
				m_hwnd = p_hwnd;
			}

			public AppWindow()
				: this(hwnd)
			{
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct OpenFileName
		{
			public int lstructSize;

			public IntPtr hwndOwner;

			public IntPtr hInstance;

			public IntPtr lpstrFilter;

			public IntPtr lpstrCustomFilter;

			public int lMaxCustomFilter;

			public int lFilterIndex;

			public IntPtr lpstrFile;

			public int lMaxFile;

			public IntPtr lpstrFileTitle;

			public int lMaxFileTitle;

			public string lpstrInitialDir;

			public string lpstrTitle;

			public int lFlags;

			public ushort nFileOffset;

			public ushort nFileExtension;

			public string lpstrDefExt;

			public IntPtr lCustData;

			public IntPtr lpfHook;

			public IntPtr lpTemplateName;
		}

		public struct KeyboardHookStruct
		{
			public int vkCode;

			private int scanCode;

			public int flags;

			private int time;

			private int dwExtraInfo;
		}

		public delegate IntPtr HookHandlerDelegate(int nCode, IntPtr wParam, ref KeyboardHookStruct lParam);

		public static string checksum;

		private static IntPtr m_hwnd = IntPtr.Zero;

		private static IntPtr m_foreground = IntPtr.Zero;

		public static string prefix
		{
			get
			{
				switch (UnityEngine.Application.platform)
				{
				case RuntimePlatform.WindowsPlayer:
				case RuntimePlatform.WindowsEditor:
					return "win";
				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.OSXPlayer:
					return "osx";
				case RuntimePlatform.LinuxPlayer:
				case RuntimePlatform.LinuxEditor:
					return "unix";
				case RuntimePlatform.XboxOne:
					return "xbox";
				case RuntimePlatform.PS4:
					return "ps4";
				default:
					return "none";
				}
			}
		}

		public static string context => UnityEngine.Application.platform switch
		{
			RuntimePlatform.WindowsEditor => "editor", 
			RuntimePlatform.WindowsPlayer => "standalone", 
			RuntimePlatform.OSXEditor => "editor", 
			RuntimePlatform.OSXPlayer => "standalone", 
			RuntimePlatform.LinuxEditor => "editor", 
			RuntimePlatform.LinuxPlayer => "standalone", 
			_ => "none", 
		};

		public static string root => string.Concat(UnityEngine.Application.dataPath + "/", "game/").Replace('\\', '/');

		private static IntPtr hwnd
		{
			get
			{
				if (m_hwnd != IntPtr.Zero)
				{
					return m_hwnd;
				}
				return m_hwnd = GetActiveWindow();
			}
		}

		private static IntPtr foreground
		{
			get
			{
				if (m_foreground != IntPtr.Zero)
				{
					return m_foreground;
				}
				return m_foreground = GetForegroundWindow();
			}
		}

		public static bool IsPS5 => false;

		public static List<string> args => new List<string>(Environment.GetCommandLineArgs());

		public static string GetPlatformByContext(string p_context = "")
		{
			if (p_context.Equals(""))
			{
				p_context = context;
			}
			switch (p_context.ToLower())
			{
			case "xb":
			case "xbs":
			case "xbx":
			case "xbox":
			case "xbss":
			case "xbsx":
				return "xbox";
			case "ps4":
			case "ps4base":
			case "ps4pro":
			case "playstation":
				return "playstation";
			case "standalone":
			case "editor":
			case "unix":
			case "win":
			case "osx":
				return "standalone";
			default:
				return "undefined";
			}
		}

		public static string GetPlatform()
		{
			return "Steam";
		}

		public static void RefreshChecksum(bool p_async = true, Action<string> p_callback = null)
		{
			if (false)
			{
				checksum = "b67b3721adc8d61e";
				p_callback?.Invoke(checksum);
				return;
			}
			string text = GetPlatform().ToLower();
			string text2 = "sharedassets0.assets";
			string checksumRoot = DRLPaths.checksumRoot;
			switch (text)
			{
			case "steam":
			case "epic":
			case "playstation":
			case "xbox":
				text2 = "sharedassets0.assets";
				break;
			}
			string text3 = checksumRoot + "/" + text2;
			if (p_async)
			{
				MD5Crypto.CalculateChecksumAsync(text3, delegate(string checksum_hash)
				{
					checksum = checksum_hash;
					if (string.IsNullOrEmpty(checksum))
					{
						checksum = "b67b3721adc8d61e";
						p_callback?.Invoke(checksum);
					}
					else
					{
						p_callback?.Invoke(checksum);
					}
				});
			}
			else
			{
				checksum = MD5Crypto.CalculateChecksum(text3);
				if (string.IsNullOrEmpty(checksum))
				{
					checksum = "b67b3721adc8d61e";
					p_callback?.Invoke(checksum);
				}
				else
				{
					p_callback?.Invoke(checksum);
				}
			}
		}

		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

		[DllImport("kernel32.dll")]
		private static extern uint GetCurrentThreadId();

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool BringWindowToTop(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool BringWindowToTop(HandleRef hWnd);

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

		[DllImport("user32.dll")]
		private static extern bool GetDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern IntPtr GetActiveWindow();

		[DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool GetOpenFileName([In][Out] ref OpenFileName ofn);

		[DllImport("comdlg32.dll", SetLastError = true)]
		public static extern int CommDlgExtendedError();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr SetWindowsHookEx(int idHook, HookHandlerDelegate lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref KeyboardHookStruct lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern short GetKeyState(int keyCode);

		private static void _Focus()
		{
			uint windowThreadProcessId = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
			uint currentThreadId = GetCurrentThreadId();
			if (windowThreadProcessId != currentThreadId)
			{
				AttachThreadInput(windowThreadProcessId, currentThreadId, fAttach: true);
				BringWindowToTop(hwnd);
				ShowWindow(hwnd, 5u);
				AttachThreadInput(windowThreadProcessId, currentThreadId, fAttach: false);
			}
			else
			{
				BringWindowToTop(hwnd);
				ShowWindow(hwnd, 5u);
			}
		}

		private static void _FileDialog(string p_title, bool p_multifile, Action<string[]> p_callback, string p_fitler = "*.*", string p_directory = "")
		{
			if (UnityEngine.Application.platform != RuntimePlatform.WindowsEditor && UnityEngine.Application.platform != RuntimePlatform.WindowsPlayer)
			{
				UnityEngine.Debug.LogWarning("OS> FileDialog is only supported on Windows.");
				p_callback?.Invoke(new string[0]);
				return;
			}

			const int maxFileChars = 65536;
			const int OFN_ALLOWMULTISELECT = 0x00000200;
			const int OFN_PATHMUSTEXIST = 0x00000800;
			const int OFN_FILEMUSTEXIST = 0x00001000;
			const int OFN_EXPLORER = 0x00080000;
			string filter = string.IsNullOrEmpty(p_fitler) || p_fitler == "*.*"
				? "All Files (*.*)\0*.*\0\0"
				: p_fitler.Replace('|', '\0') + "\0\0";
			IntPtr fileBuffer = Marshal.AllocHGlobal(maxFileChars * 2);
			IntPtr filterBuffer = Marshal.StringToHGlobalUni(filter);
			try
			{
				Marshal.WriteInt16(fileBuffer, 0);
				OpenFileName openFileName = new OpenFileName
				{
					lstructSize = Marshal.SizeOf(typeof(OpenFileName)),
					hwndOwner = hwnd,
					lpstrFilter = filterBuffer,
					lFilterIndex = 1,
					lpstrFile = fileBuffer,
					lMaxFile = maxFileChars,
					lpstrInitialDir = string.IsNullOrEmpty(p_directory) ? null : p_directory,
					lpstrTitle = string.IsNullOrEmpty(p_title) ? null : p_title,
					lFlags = OFN_EXPLORER | OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST | (p_multifile ? OFN_ALLOWMULTISELECT : 0)
				};

				if (!GetOpenFileName(ref openFileName))
				{
					int errorCode = CommDlgExtendedError();
					if (errorCode != 0)
					{
						UnityEngine.Debug.LogError("OS> FileDialog failed with native error " + errorCode);
					}
					p_callback?.Invoke(new string[0]);
					return;
				}

				List<string> selections = new List<string>();
				int offset = 0;
				while (offset < maxFileChars)
				{
					string selection = Marshal.PtrToStringUni(IntPtr.Add(fileBuffer, offset * 2));
					if (string.IsNullOrEmpty(selection))
					{
						break;
					}
					selections.Add(selection);
					offset += selection.Length + 1;
				}

				string[] files;
				if (selections.Count <= 1)
				{
					files = selections.ToArray();
				}
				else
				{
					files = new string[selections.Count - 1];
					for (int i = 1; i < selections.Count; i++)
					{
						files[i - 1] = Path.Combine(selections[0], selections[i]);
					}
				}
				UnityEngine.Debug.Log("OS> FileDialog File selection [" + string.Join(",", files) + "]");
				p_callback?.Invoke(files);
			}
			finally
			{
				Marshal.FreeHGlobal(fileBuffer);
				Marshal.FreeHGlobal(filterBuffer);
			}
		}

		private static Process _Run(string p_file, ProcessWindowStyle p_style = ProcessWindowStyle.Hidden, bool p_threaded = true, Process.IO p_io_mode = Process.IO.Out, string[] p_args = null, Action p_on_complete = null)
		{
			Process p = new Process();
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.WindowStyle = p_style;
			processStartInfo.CreateNoWindow = p_style == ProcessWindowStyle.Hidden;
			processStartInfo.FileName = p_file;
			processStartInfo.RedirectStandardOutput = (p_io_mode & Process.IO.Out) != 0;
			processStartInfo.RedirectStandardInput = (p_io_mode & Process.IO.In) != 0;
			processStartInfo.RedirectStandardError = (p_io_mode & Process.IO.Error) != 0;
			processStartInfo.UseShellExecute = p_io_mode == Process.IO.None;
			processStartInfo.Arguments = string.Join(" ", p_args);
			process.StartInfo = processStartInfo;
			p.process = process;
			ThreadStart threadStart = null;
			threadStart = delegate
			{
				try
				{
					process.Start();
					p.m_running = true;
					if (p_threaded)
					{
						process.WaitForExit();
						p.m_running = false;
						if (p_on_complete != null)
						{
							p_on_complete();
						}
					}
					else if (p_on_complete != null)
					{
						p_on_complete();
					}
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError(ex.Message);
				}
			};
			if (p_threaded)
			{
				p.thread = new Thread(threadStart);
				p.thread.Start();
			}
			else
			{
				threadStart();
			}
			return p;
		}

		public static void Focus()
		{
			_Focus();
		}

		public static void FileDialog(string p_title, bool p_multifile, Action<string[]> p_callback, string p_filter = "*.*", string p_directory = "")
		{
			_FileDialog(p_title, p_multifile, p_callback, p_filter, p_directory);
		}

		public static Process Run(string p_file, ProcessWindowStyle p_style = ProcessWindowStyle.Hidden, bool p_threaded = true, Process.IO p_io_mode = Process.IO.Out, string[] p_args = null, Action p_on_complete = null)
		{
			return _Run(p_file, p_style, p_threaded, p_io_mode, p_args, p_on_complete);
		}

		public static void PathAssert(string p_path)
		{
			p_path = p_path.Replace('\\', '/');
			string[] array = p_path.Split('/');
			if (array.Length == 0)
			{
				return;
			}
			string text = "";
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i];
				if (i < array.Length - 1)
				{
					if (i > 0)
					{
						text += "/";
					}
					text += text2;
					if (!(text2 == "..") && !(text2 == "") && !Directory.Exists(text))
					{
						Directory.CreateDirectory(text);
					}
				}
			}
		}

		public static string ProjectPath(string p_path)
		{
			p_path = p_path.Replace('\\', '/');
			return p_path.Substring(p_path.LastIndexOf("Assets"));
		}

		public static string FullPath(string p_path)
		{
			p_path = p_path.Replace('\\', '/');
			if (p_path.IndexOf("Assets/") == 0)
			{
				p_path = p_path.Replace("Assets/", "");
			}
			return UnityEngine.Application.dataPath + "/" + p_path;
		}

		public static void OpenFolder(string p_path)
		{
			System.Diagnostics.Process.Start(new ProcessStartInfo
			{
				FileName = p_path,
				UseShellExecute = true,
				Verb = "open"
			});
		}
	}
}
