using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace vrecorder
{
	[Serializable]
	public class VideoRecorder
	{
		[Serializable]
		public class Settings
		{
			public CodecModeFlag codecFlag = CodecModeFlag.VP9;

			private bool m_input;

			public string filename;

			public string path;

			public int width = 1920;

			public int height = 1080;

			public bool input => m_input;

			public bool useGPU => false;

			public string extension => GetCodecFileExtension(codecFlag);

			public string fullpath
			{
				get
				{
					if (!piped)
					{
						return "\"" + path + filename + "." + extension + "\"";
					}
					return filename;
				}
			}

			public bool piped
			{
				get
				{
					return filename == "-";
				}
				set
				{
					filename = (value ? "-" : ((filename == "-") ? "" : filename));
				}
			}

			public string format
			{
				get
				{
					switch (codecFlag)
					{
					case CodecModeFlag.Gif:
					case CodecModeFlag.JPG:
					case CodecModeFlag.PNG:
					case CodecModeFlag.PPM:
					case CodecModeFlag.Tiff:
						if (!piped)
						{
							return "-f image2";
						}
						return "-f image2pipe";
					case CodecModeFlag.Raw:
						return "-f rawvideo";
					default:
						return "";
					}
				}
			}

			public string codec => "-vcodec " + GetCodecFlag(codecFlag);

			public string videoSize
			{
				get
				{
					if (codecFlag != CodecModeFlag.Raw)
					{
						return "";
					}
					return "-video_size " + width + ":" + height;
				}
			}

			public string pixelFormat => codecFlag switch
			{
				CodecModeFlag.Raw => "-pix_fmt rgba", 
				CodecModeFlag.Prores => "-pix_fmt yuv422p10le", 
				_ => "-pix_fmt yuv420p", 
			};

			public List<string> arguments
			{
				get
				{
					List<string> list = new List<string>();
					list.Add(format);
					list.Add(codec);
					list.Add(pixelFormat);
					if (input)
					{
						list.Add(videoSize);
					}
					if (useGPU)
					{
						list.Add("-hwaccel " + (input ? "nvdec" : "nvenc"));
					}
					list.Add((input ? "-i " : "") + fullpath);
					return list;
				}
			}

			public Settings(bool p_input)
			{
				m_input = p_input;
			}
		}

		public enum CodecModeFlag
		{
			Gif = 0,
			H261 = 1,
			H263 = 2,
			H263P = 3,
			H264 = 4,
			H264NV = 5,
			H265 = 6,
			H265NV = 7,
			JPG = 8,
			PNG = 9,
			PPM = 10,
			Prores = 11,
			Raw = 12,
			Tiff = 13,
			VP8 = 14,
			VP9 = 15
		}

		public enum EventType
		{
			Start = 0,
			Stop = 1,
			Complete = 2,
			Pipe = 3,
			Kill = 4,
			Error = 5
		}

		private static string[] m_codecs = new string[16]
		{
			"gif", "h261", "h263", "h263p", "libx264", "h264_nvenc", "libx265", "nvenc_hevc", "mjpeg", "png",
			"ppm", "prores_ks", "rawvideo", "tiff", "libvpx", "libvpx-vp9"
		};

		private static string[] m_codec_exts = new string[16]
		{
			"gif", "mp4", "mp4", "mp4", "mp4", "mp4", "mp4", "mp4", "jpg", "png",
			"ppm", "mov", "mp4", "tiff", "webm", "webm"
		};

		private static string[] m_codec_names = new string[16]
		{
			"GIF (Graphics Interchange Format)", "H.261", "H.263 / H.263-1996, H.263+ / H.263-1998 / H.263 version 2", "H.263+ / H.263-1998 / H.263 version 2", "H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10", "NVidia H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10", "H.265 / HEVC (High Efficiency Video Coding)", "NVidia H.265 / HEVC (High Efficiency Video Coding)", "JPG", "PNG (Portable Network Graphics)",
			"PPM (Portable PixelMap)", "Apple ProRes (iCodec Pro)", "Raw Video", "TIFF", "On2 VP8", "Google VP9"
		};

		public string path;

		public float fps = 30f;

		public Settings input = new Settings(p_input: true);

		public int crf = 18;

		public int[] crop;

		public bool vflip;

		public Settings output = new Settings(p_input: true);

		public Process process;

		public int pipeCount;

		public string errorLog;

		public Action<VideoRecorder, EventType> OnEvent;

		private Thread m_process_thread;

		private Thread m_pipe_thread;

		private List<byte[]> m_pipe_buffer;

		private bool m_pipe_started;

		public bool hasCrop
		{
			get
			{
				if (crop == null)
				{
					return false;
				}
				if (crop.Length < 4)
				{
					return false;
				}
				if (crop[0] <= 0 && crop[1] <= 0)
				{
					return false;
				}
				return true;
			}
		}

		public bool running { get; private set; }

		public int exitCode
		{
			get
			{
				int result = 0;
				if (!running)
				{
					return result;
				}
				try
				{
					result = process.ExitCode;
				}
				catch (Exception)
				{
				}
				return result;
			}
		}

		public StreamWriter standardInput
		{
			get
			{
				StreamWriter result = null;
				if (process == null)
				{
					return result;
				}
				try
				{
					result = process.StandardInput;
				}
				catch (Exception)
				{
				}
				return result;
			}
		}

		public bool isPipeEmpty
		{
			get
			{
				if (m_pipe_buffer != null)
				{
					return m_pipe_buffer.Count <= 0;
				}
				return true;
			}
		}

		public StreamReader standardError
		{
			get
			{
				StreamReader result = null;
				if (process == null)
				{
					return result;
				}
				try
				{
					result = process.StandardError;
				}
				catch (Exception)
				{
				}
				return result;
			}
		}

		public List<string> arguments
		{
			get
			{
				List<string> list = new List<string>();
				list.Add("-y");
				list.Add("-r " + fps);
				list.AddRange(input.arguments);
				List<string> list2 = new List<string>();
				if (vflip)
				{
					list2.Add("vflip");
				}
				list2.Add("scale=" + Rnd2(output.width) + ":" + Rnd2(output.height));
				if (hasCrop)
				{
					list2.Add("crop=" + Rnd2(crop[0]) + ":" + Rnd2(crop[1]) + ":" + Rnd2(crop[2]) + ":" + Rnd2(crop[3]));
				}
				list.Add("-vf \"" + string.Join(",", list2) + "\"");
				CodecModeFlag codecFlag = output.codecFlag;
				if ((uint)(codecFlag - 14) <= 1u)
				{
					list.Add("-quality realtime");
					list.Add("-row-mt 1");
					list.Add("-tile-columns 6");
					list.Add("-frame-parallel 1");
					list.Add("-threads 16");
					if (crf > 0)
					{
						int num = 3;
						if (crf >= 32)
						{
							num = 8;
						}
						else if (crf >= 28)
						{
							num = 7;
						}
						else if (crf >= 24)
						{
							num = 6;
						}
						else if (crf >= 20)
						{
							num = 5;
						}
						else if (crf >= 16)
						{
							num = 4;
						}
						else if (crf >= 12)
						{
							num = 3;
						}
						else if (crf >= 8)
						{
							num = 2;
						}
						else if (crf >= 4)
						{
							num = 1;
						}
						list.Add("-speed " + num);
						string text = "2M";
						if (output.height >= 2160)
						{
							text = ((fps > 30f) ? "50M" : "40M");
						}
						else if (output.height >= 1440)
						{
							text = ((fps > 30f) ? "24M" : "16M");
						}
						else if (output.height >= 1080)
						{
							text = ((fps > 30f) ? "12M" : "8M");
						}
						else if (output.height >= 720)
						{
							text = ((fps > 30f) ? "8M" : "5M");
						}
						else if (output.height >= 480)
						{
							text = ((fps > 30f) ? "4M" : "3M");
						}
						else if (output.height >= 360)
						{
							text = ((fps > 30f) ? "2M" : "1M");
						}
						list.Add("-b:v " + text);
						list.Add("-crf " + crf);
					}
					else
					{
						list.Add("-lossless 1");
					}
				}
				else
				{
					list.Add("-crf " + crf);
				}
				list.AddRange(output.arguments);
				return list;
			}
		}

		public static string GetCodecFlag(CodecModeFlag p_type)
		{
			return m_codecs[(int)p_type];
		}

		public static string GetCodecFileExtension(CodecModeFlag p_type)
		{
			return m_codec_exts[(int)p_type];
		}

		public static string GetCodecName(CodecModeFlag p_type)
		{
			return m_codec_names[(int)p_type];
		}

		public void SetCrop(int p_x = 0, int p_y = 0, int p_width = 0, int p_height = 0)
		{
			crop = new int[4] { p_width, p_height, p_x, p_y };
		}

		protected string GetErrorLog()
		{
			string result = "";
			try
			{
				result = ((standardError == null) ? "" : standardError.ReadToEnd());
			}
			catch (Exception)
			{
			}
			return result;
		}

		public VideoRecorder()
		{
			m_pipe_buffer = new List<byte[]>();
			input = new Settings(p_input: true);
			output = new Settings(p_input: false);
		}

		public void Start()
		{
			ApplyKill();
			m_pipe_started = true;
			running = true;
			pipeCount = 0;
			if (input.piped)
			{
				m_pipe_started = false;
				(m_pipe_thread = new Thread((ThreadStart)delegate
				{
					m_pipe_started = true;
					errorLog = "";
					while (true)
					{
						if (process != null)
						{
							if (!running)
							{
								break;
							}
							errorLog = GetErrorLog();
							List<byte[]> pipe_buffer = m_pipe_buffer;
							if (pipe_buffer != null && pipe_buffer.Count > 0)
							{
								WritePipe(pipe_buffer[0]);
								pipe_buffer.RemoveAt(0);
								if (OnEvent != null)
								{
									OnEvent(this, EventType.Pipe);
								}
								pipeCount++;
							}
						}
					}
					m_pipe_thread = null;
				})).Start();
			}
			(m_process_thread = new Thread((ThreadStart)delegate
			{
				while (!m_pipe_started)
				{
				}
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = path,
					Arguments = string.Join(" ", arguments),
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardInput = true,
					RedirectStandardOutput = false,
					RedirectStandardError = true
				};
				process = new Process();
				process.StartInfo = startInfo;
				process.Start();
				if (OnEvent != null)
				{
					OnEvent(this, EventType.Start);
				}
				process.WaitForExit();
				running = false;
				if (OnEvent != null)
				{
					OnEvent(this, EventType.Stop);
				}
				if (exitCode == 0 && OnEvent != null)
				{
					OnEvent(this, EventType.Complete);
				}
				m_process_thread = null;
				process = null;
			})).Start();
		}

		public bool Pipe(byte[] p_data, int p_length)
		{
			return Pipe(p_data, p_length, p_force: false);
		}

		public bool Pipe(byte[] p_data, bool p_force)
		{
			return Pipe(p_data, -1, p_force);
		}

		public bool Pipe(byte[] p_data)
		{
			return Pipe(p_data, -1, p_force: false);
		}

		public bool Pipe(byte[] p_data, int p_length, bool p_force)
		{
			if (!input.piped)
			{
				return false;
			}
			bool result = true;
			if (p_force)
			{
				result = WritePipe(p_data, p_length, p_async: true);
				if (OnEvent != null)
				{
					OnEvent(this, EventType.Pipe);
				}
				pipeCount++;
			}
			else
			{
				if (m_pipe_buffer == null)
				{
					m_pipe_buffer = new List<byte[]>();
				}
				m_pipe_buffer.Add(p_data);
			}
			return result;
		}

		private bool WritePipe(byte[] b, int l = -1, bool p_async = false)
		{
			if (b == null)
			{
				return false;
			}
			Stream stream = ((standardInput == null) ? null : standardInput.BaseStream);
			if (stream == null)
			{
				return false;
			}
			try
			{
				int count = ((l < 0) ? b.Length : l);
				if (p_async)
				{
					stream.WriteAsync(b, 0, count);
					stream.FlushAsync();
				}
				else
				{
					stream.Write(b, 0, count);
					stream.Flush();
				}
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		public void Stop()
		{
			if (process == null || !running)
			{
				return;
			}
			running = false;
			if (input.piped)
			{
				_ = standardInput;
				if (standardInput != null)
				{
					standardInput.Close();
				}
			}
			else
			{
				process.Kill();
			}
		}

		public void Kill()
		{
			if (process != null)
			{
				ApplyKill();
				if (OnEvent != null)
				{
					OnEvent(this, EventType.Kill);
				}
			}
		}

		protected void ApplyKill()
		{
			running = false;
			if (process != null)
			{
				try
				{
					process.Kill();
				}
				catch (Exception)
				{
				}
			}
			if (m_process_thread != null)
			{
				m_process_thread.Abort();
			}
			if (m_pipe_thread != null)
			{
				m_pipe_thread.Abort();
			}
			if (m_pipe_buffer != null)
			{
				m_pipe_buffer.Clear();
			}
			process = null;
			m_process_thread = null;
			m_pipe_thread = null;
			m_pipe_buffer = new List<byte[]>();
		}

		private int Rnd2(int v)
		{
			return (int)(Math.Round((float)v / 2f) * 2.0);
		}
	}
}
