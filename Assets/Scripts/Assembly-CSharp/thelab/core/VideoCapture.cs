using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using vrecorder;

namespace thelab.core
{
	public class VideoCapture : MonoBehaviour
	{
		public enum CompressionMode
		{
			Off = 0,
			Low = 1,
			Medium = 2,
			High = 3
		}

		public enum StateType
		{
			None = 0,
			RecordStart = 1,
			RecordStop = 2,
			RecordStep = 3,
			RecordEnd = 4,
			GenerateStart = 5,
			GenerateEnd = 6,
			GenerateProgress = 7
		}

		public class FrameJob
		{
			public Texture2D frame;

			public NativeArray<byte> buffer;

			public VideoCapture process;

			public FrameJob(VideoCapture p_process, Texture2D p_tex)
			{
				frame = p_tex;
				process = p_process;
			}

			public void Create()
			{
				if ((bool)frame)
				{
					buffer = frame.GetRawTextureData<byte>();
				}
			}

			public bool Pipe()
			{
				_ = buffer;
				NativeArray<byte>.Copy(buffer, process.m_frame_buffer, buffer.Length);
				return process.recorder.Pipe(process.m_frame_buffer, buffer.Length, p_force: true);
			}

			public void Clear()
			{
				if ((bool)frame)
				{
					UnityEngine.Object.Destroy(frame, 1f / 30f);
				}
				frame = null;
			}
		}

		[SerializeField]
		private VideoRecorder m_recorder;

		public int maxFrameCount;

		public int currentFrame;

		public int processedFrame;

		public Action<VideoCapture, StateType> OnState;

		private List<FrameJob> m_job_buffer;

		private List<FrameJob> m_job_save;

		private List<FrameJob> m_job_garbage;

		internal byte[] m_frame_buffer;

		private bool m_record_lock;

		private bool m_is_recording;

		private Thread m_save_thread;

		public VideoRecorder recorder
		{
			get
			{
				if (m_recorder != null)
				{
					return m_recorder;
				}
				return m_recorder = new VideoRecorder();
			}
		}

		public bool isRecording
		{
			get
			{
				if (!m_record_lock)
				{
					return m_is_recording;
				}
				return true;
			}
		}

		public bool isGenerating => recorder.running;

		private bool hasSaveJobs
		{
			get
			{
				if (m_job_save != null)
				{
					return m_job_save.Count > 0;
				}
				return false;
			}
		}

		private bool hasBufferJobs
		{
			get
			{
				if (m_job_buffer != null)
				{
					return m_job_buffer.Count > 0;
				}
				return false;
			}
		}

		private bool hasJobs
		{
			get
			{
				if (!hasSaveJobs)
				{
					return hasBufferJobs;
				}
				return true;
			}
		}

		protected void Awake()
		{
			m_job_buffer = new List<FrameJob>();
			m_job_save = new List<FrameJob>();
			m_job_garbage = new List<FrameJob>();
			StartCoroutine(StartRecordLoop());
		}

		public void SetCropArea(Rect p_rect)
		{
			recorder.SetCrop((int)p_rect.x, (int)p_rect.y, (int)p_rect.width, (int)p_rect.height);
		}

		public void ResetCropArea()
		{
			recorder.SetCrop();
		}

		public void SetCompression(CompressionMode p_mode)
		{
			recorder.crf = 20;
			switch (p_mode)
			{
			case CompressionMode.Off:
				recorder.crf = 0;
				break;
			case CompressionMode.Low:
				recorder.crf = 10;
				break;
			case CompressionMode.Medium:
				recorder.crf = 20;
				break;
			case CompressionMode.High:
				recorder.crf = 30;
				break;
			}
		}

		public void SetCompression(int p_crf)
		{
			recorder.crf = Mathf.Clamp(p_crf, 0, 40);
		}

		public void Record(float p_duration, int p_framerate, int p_width, int p_height, string p_target_folder, string p_video_name, bool p_piped = false)
		{
			if (m_record_lock)
			{
				Debug.LogWarning("VideoCapture> Already Recording!");
				return;
			}
			if (m_frame_buffer == null)
			{
				m_frame_buffer = new byte[80000000];
			}
			maxFrameCount = Mathf.RoundToInt(p_duration * (float)p_framerate);
			currentFrame = 1;
			processedFrame = 0;
			int scaleFactor = GetScaleFactor(p_height);
			recorder.OnEvent = delegate(VideoRecorder fp, VideoRecorder.EventType ev)
			{
				Activity.RunOnce(delegate
				{
					OnFFmpegEvent(fp, ev);
				});
			};
			recorder.input.codecFlag = VideoRecorder.CodecModeFlag.Raw;
			recorder.input.width = Screen.width * scaleFactor;
			recorder.input.height = Screen.height * scaleFactor;
			recorder.input.piped = true;
			recorder.input.codecFlag = VideoRecorder.CodecModeFlag.Raw;
			recorder.fps = p_framerate;
			recorder.vflip = true;
			recorder.output.width = p_width;
			recorder.output.height = p_height;
			recorder.output.filename = p_video_name;
			recorder.output.path = p_target_folder;
			recorder.output.filename = p_video_name;
			recorder.output.path = p_target_folder;
			ClearBuffers();
			m_record_lock = true;
			recorder.Start();
		}

		public int GetScaleFactor(int p_height)
		{
			float f = ((Screen.height <= 0) ? 1f : ((float)p_height / (float)Screen.height));
			return Mathf.Max(1, Mathf.CeilToInt(f));
		}

		public int GetEstimatedFrameCount(float p_duration)
		{
			return Mathf.RoundToInt(p_duration * recorder.fps);
		}

		public void Stop()
		{
			if (!m_record_lock)
			{
				Debug.LogWarning("VideoCapture> Not Recording!");
				return;
			}
			m_is_recording = false;
			m_record_lock = false;
			Time.captureFramerate = 0;
			recorder.Stop();
			if (OnState != null)
			{
				OnState(this, StateType.RecordStop);
			}
		}

		public ulong GetTempDiskSpaceRequirements(float p_duration, int p_width, int p_height, int p_fps)
		{
			ulong num = (ulong)(p_width * p_height * 4);
			return (ulong)(p_fps * Mathf.CeilToInt(p_duration)) * num;
		}

		public void Clear()
		{
			string path = recorder.output.path + "frames/";
			if (Directory.Exists(path))
			{
				string[] files = Directory.GetFiles(path, "*.raw", SearchOption.TopDirectoryOnly);
				for (int i = 0; i < files.Length; i++)
				{
					File.Delete(files[i]);
				}
				files = Directory.GetFiles(path, "*.png", SearchOption.TopDirectoryOnly);
				for (int j = 0; j < files.Length; j++)
				{
					File.Delete(files[j]);
				}
				files = Directory.GetFiles(path, "*.jpg", SearchOption.TopDirectoryOnly);
				for (int k = 0; k < files.Length; k++)
				{
					File.Delete(files[k]);
				}
			}
		}

		protected void OnFFmpegEvent(VideoRecorder p_fp, VideoRecorder.EventType p_event)
		{
			switch (p_event)
			{
			case VideoRecorder.EventType.Start:
			{
				m_is_recording = true;
				string text = string.Join(" ", recorder.arguments);
				Debug.Log("VideoCapture> Arguments\n" + text);
				Time.captureFramerate = Mathf.RoundToInt(recorder.fps);
				if (OnState != null)
				{
					OnState(this, StateType.RecordStart);
				}
				if (OnState != null)
				{
					OnState(this, StateType.GenerateStart);
				}
				break;
			}
			case VideoRecorder.EventType.Stop:
				Time.captureFramerate = 0;
				break;
			case VideoRecorder.EventType.Complete:
				Time.captureFramerate = 0;
				recorder.Kill();
				ClearBuffers();
				Debug.Log("VideoCapture> Log\n" + recorder.errorLog);
				if (OnState != null)
				{
					OnState(this, StateType.RecordEnd);
				}
				if (OnState != null)
				{
					OnState(this, StateType.GenerateEnd);
				}
				break;
			case VideoRecorder.EventType.Pipe:
				processedFrame = recorder.pipeCount;
				if (OnState != null)
				{
					OnState(this, StateType.GenerateProgress);
				}
				if (!hasJobs && !isRecording && recorder.isPipeEmpty && recorder.running)
				{
					recorder.Stop();
				}
				break;
			case VideoRecorder.EventType.Kill:
				break;
			}
		}

		protected IEnumerator StartRecordLoop()
		{
			while (true)
			{
				yield return new WaitForEndOfFrame();
				RecordLoop();
			}
		}

		protected void RecordLoop()
		{
			bool num = m_job_save.Count > 0;
			bool flag = m_job_buffer.Count > 0;
			bool flag2 = num || flag;
			if (m_is_recording)
			{
				if (currentFrame >= maxFrameCount)
				{
					if (!flag2)
					{
						Stop();
					}
				}
				else
				{
					if (!recorder.running)
					{
						return;
					}
					Texture2D p_tex = ScreenCapture.CaptureScreenshotAsTexture(GetScaleFactor(recorder.input.height));
					FrameJob item = new FrameJob(this, p_tex);
					m_job_buffer.Add(item);
					currentFrame++;
					if (currentFrame % 500 == 0)
					{
						GC.Collect();
					}
					if (OnState != null)
					{
						OnState(this, StateType.RecordStep);
					}
				}
			}
			ProcessBuffer();
			ProcessSave();
			for (int i = 0; i < 3; i++)
			{
				ProcessGargabage();
			}
		}

		protected void ProcessBuffer()
		{
			while (m_job_buffer.Count > 0)
			{
				FrameJob frameJob = m_job_buffer[0];
				frameJob.Create();
				m_job_save.Add(frameJob);
				m_job_buffer.RemoveAt(0);
			}
		}

		protected void ProcessSave()
		{
			if (m_save_thread != null || m_job_save.Count <= 0)
			{
				return;
			}
			m_save_thread = new Thread((ThreadStart)delegate
			{
				while (m_job_save.Count > 0)
				{
					FrameJob frameJob = m_job_save[0];
					m_job_save.RemoveAt(0);
					bool num = frameJob.Pipe();
					m_job_garbage.Add(frameJob);
					if (!num)
					{
						Debug.LogWarning("VideoCapture> Failed Pipe!");
					}
					Thread.Sleep(8);
				}
				m_save_thread = null;
			});
			m_save_thread.Priority = System.Threading.ThreadPriority.Highest;
			m_save_thread.Start();
		}

		protected void ProcessGargabage()
		{
			if (m_job_garbage.Count > 0)
			{
				m_job_garbage[0].Clear();
				m_job_garbage.RemoveAt(0);
			}
		}

		protected void ClearBuffers()
		{
			List<FrameJob> job_buffer = m_job_buffer;
			for (int i = 0; i < job_buffer.Count; i++)
			{
				job_buffer[i].Clear();
			}
			job_buffer.Clear();
			job_buffer = m_job_save;
			for (int j = 0; j < job_buffer.Count; j++)
			{
				job_buffer[j].Clear();
			}
			job_buffer.Clear();
			job_buffer = m_job_garbage;
			for (int k = 0; k < job_buffer.Count; k++)
			{
				job_buffer[k].Clear();
			}
			job_buffer.Clear();
		}
	}
}
