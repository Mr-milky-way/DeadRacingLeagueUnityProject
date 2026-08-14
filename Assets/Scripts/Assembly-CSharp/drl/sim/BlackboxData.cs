using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityExt.Core.IO;
using thelab.core;

namespace drl.sim
{
	[Serializable]
	public class BlackboxData
	{
		private static BlackboxFrame m_sample_tmp;

		[SerializableField]
		public string version;

		[NonSerialized]
		private SerializedData m_header;

		[SerializableField]
		internal string m_header_data;

		[SerializableField]
		public byte flags;

		[SerializableField]
		public BlackboxFrame[] frames;

		[SerializableField]
		public byte[] compressedFrames;

		[SerializableField]
		public bool compressed;

		[SerializableField]
		public int iterator;

		[SerializableField]
		public float elapsed;

		public Dictionary<byte, List<BlackboxFrame>> tracks;

		public SerializedData header
		{
			get
			{
				if (m_header != null)
				{
					return m_header;
				}
				if (string.IsNullOrEmpty(m_header_data))
				{
					m_header = new SerializedData();
					m_header_data = m_header.ToJson();
					return m_header;
				}
				return m_header = SerializedData.FromJson<SerializedData>(m_header_data);
			}
			set
			{
				m_header = ((value == null) ? new SerializedData() : value);
				m_header_data = m_header.ToJson();
			}
		}

		public float start
		{
			get
			{
				if (frames.Length == 0)
				{
					return 0f;
				}
				return frames[0].time;
			}
		}

		public float end
		{
			get
			{
				if (frames.Length != 0)
				{
					return frames[frames.Length - 1].time;
				}
				return 0f;
			}
		}

		public static int Seek(List<BlackboxFrame> p_frames, float p_time, int p_hint)
		{
			int num = p_frames.Count - 1;
			if (p_hint >= 0)
			{
				p_hint = Mathf.Clamp(p_hint, 0, num);
			}
			float time = p_frames[num].time;
			p_hint = (int)(Mathf.Clamp01((time <= 0f) ? 0f : (p_time / time)) * (float)num);
			BlackboxFrame blackboxFrame = ((p_hint < 0) ? p_frames[0] : p_frames[p_hint]);
			int num2 = ((p_hint >= 0) ? ((!(p_time <= blackboxFrame.time)) ? p_hint : 0) : 0);
			int num3 = ((p_hint < 0) ? num : ((p_time <= blackboxFrame.time) ? p_hint : num));
			int i = 0;
			int num4 = 0;
			for (; i < 1000000; i++)
			{
				num4 = (num2 + num3) / 2;
				blackboxFrame = p_frames[num4];
				num2 = ((p_time <= blackboxFrame.time) ? num2 : num4);
				num3 = ((p_time <= blackboxFrame.time) ? num4 : num3);
				if (num3 - num2 <= 1)
				{
					break;
				}
			}
			return Mathf.Clamp(num4, 0, num);
		}

		public static int Seek(List<BlackboxFrame> p_frames, float p_time)
		{
			return Seek(p_frames, p_time, -1);
		}

		public static BlackboxFrame[] SeekKeyFrames(List<BlackboxFrame> p_frames, float p_time, int p_hint)
		{
			if (p_frames.Count <= 0)
			{
				return new BlackboxFrame[0];
			}
			BlackboxFrame blackboxFrame = null;
			BlackboxFrame blackboxFrame2 = null;
			int num = Seek(p_frames, p_time, p_hint);
			blackboxFrame = p_frames[num];
			if (p_time < blackboxFrame.time)
			{
				num = Mathf.Clamp(num - 1, 0, p_frames.Count - 1);
			}
			if (p_time >= blackboxFrame.time)
			{
				num = Mathf.Clamp(num + 1, 0, p_frames.Count - 1);
			}
			blackboxFrame2 = p_frames[num];
			return new BlackboxFrame[2] { blackboxFrame, blackboxFrame2 };
		}

		public static BlackboxFrame[] SeekKeyFrames(List<BlackboxFrame> p_frames, float p_time)
		{
			return SeekKeyFrames(p_frames, p_time, -1);
		}

		public static BlackboxFrame Lerp(BlackboxFrame f0, BlackboxFrame f1, float r)
		{
			if (f0 == null || f1 == null)
			{
				if (f0 != null)
				{
					return f0;
				}
				if (f1 != null)
				{
					return f1;
				}
				return null;
			}
			float num = f0?.time ?? 0f;
			float num2 = f1?.time ?? 0f;
			BlackboxFrame blackboxFrame = ((num < num2) ? f0 : f1);
			BlackboxFrame obj = ((num > num2) ? f0 : f1);
			f0 = blackboxFrame;
			f1 = obj;
			DroneBlackboxDataFlag droneBlackboxDataFlag = (DroneBlackboxDataFlag)(f0?.type ?? 0);
			if (m_sample_tmp != null && m_sample_tmp.data.Length < f0.data.Length)
			{
				m_sample_tmp = null;
			}
			BlackboxFrame blackboxFrame2 = ((m_sample_tmp != null) ? m_sample_tmp : (m_sample_tmp = new BlackboxFrame()));
			blackboxFrame2.Init();
			blackboxFrame2.type = (byte)droneBlackboxDataFlag;
			blackboxFrame2.time = Mathf.Lerp(num, num2, r);
			BlackboxFrame result = ((r <= 0.5f) ? f0 : f1);
			Quaternion r2;
			Quaternion r3;
			Vector3 p;
			Vector3 p2;
			switch (droneBlackboxDataFlag)
			{
			case DroneBlackboxDataFlag.Transform:
			{
				f0.GetTransform(out p, out r2);
				f1.GetTransform(out p2, out r3);
				Vector3 p_position = Vector3.Lerp(p, p2, r);
				Quaternion p_rotation = Quaternion.Lerp(r2, r3, r);
				blackboxFrame2.Set(blackboxFrame2.time, blackboxFrame2.type, p_position, p_rotation);
				break;
			}
			case DroneBlackboxDataFlag.TransformPart:
			{
				int i2 = 0;
				f0.GetTransformPart(out i2, out p, out r2);
				f1.GetTransformPart(out i2, out p2, out r3);
				Vector3 p_position = Vector3.Lerp(p, p2, r);
				Quaternion p_rotation = Quaternion.Lerp(r2, r3, r);
				blackboxFrame2.Set(blackboxFrame2.time, blackboxFrame2.type, i2, p_position, p_rotation);
				break;
			}
			case DroneBlackboxDataFlag.Velocity:
			{
				p = f0.GetVector3();
				p2 = f1.GetVector3();
				Vector3 p_position = Vector3.Lerp(p, p2, r);
				blackboxFrame2.Set(blackboxFrame2.time, blackboxFrame2.type, p_position);
				break;
			}
			case DroneBlackboxDataFlag.RPM:
			case DroneBlackboxDataFlag.Physics:
			{
				blackboxFrame2.data = new object[f0.data.Length];
				for (int i = 0; i < blackboxFrame2.data.Length; i++)
				{
					if (f0.data[i] != null && f1.data[i] != null)
					{
						float a = (float)f0.data[i];
						float b = (float)f1.data[i];
						blackboxFrame2.data[i] = Mathf.Lerp(a, b, r);
					}
				}
				break;
			}
			case DroneBlackboxDataFlag.PIDControl:
				blackboxFrame2.data = new object[f0.data.Length];
				Array.Copy(f0.data, blackboxFrame2.data, f0.data.Length);
				break;
			case DroneBlackboxDataFlag.Input:
			{
				Vector4 vector = f0.GetVector4();
				Vector4 vector2 = f1.GetVector4();
				Vector4 p_value = Vector4.Lerp(vector, vector2, r);
				blackboxFrame2.Set(blackboxFrame2.time, blackboxFrame2.type, p_value);
				break;
			}
			default:
				blackboxFrame2 = null;
				break;
			}
			if (blackboxFrame2 != null)
			{
				return blackboxFrame2;
			}
			return result;
		}

		public static BlackboxFrame TimeLerp(BlackboxFrame f0, BlackboxFrame f1, float t)
		{
			BlackboxFrame blackboxFrame = ((f0.time < f1.time) ? f0 : f1);
			BlackboxFrame obj = ((f0.time > f1.time) ? f0 : f1);
			f0 = blackboxFrame;
			f1 = obj;
			float num = f0.time - f1.time;
			float r = ((num <= 0f) ? 0f : Mathf.Clamp01((f0.time - t) / num));
			return Lerp(f0, f1, r);
		}

		public static BlackboxFrame Sample(BlackboxFrame f0, BlackboxFrame f1, float t, bool p_smooth)
		{
			BlackboxFrame blackboxFrame = ((f0.time < f1.time) ? f0 : f1);
			BlackboxFrame obj = ((f0.time > f1.time) ? f0 : f1);
			f0 = blackboxFrame;
			f1 = obj;
			float num = f1.time - f0.time;
			float num2 = ((num <= 0f) ? 0f : Mathf.Clamp01((t - f0.time) / num));
			if (!p_smooth)
			{
				if (!(num2 <= 0.5f))
				{
					return f1;
				}
				return f0;
			}
			return Lerp(f0, f1, num2);
		}

		public static BlackboxFrame Sample(List<BlackboxFrame> p_frames, float p_time, int p_hint, bool p_smooth)
		{
			BlackboxFrame[] array = SeekKeyFrames(p_frames, p_time, p_hint);
			if (array.Length == 0)
			{
				return new BlackboxFrame();
			}
			BlackboxFrame blackboxFrame = array[0];
			BlackboxFrame f = ((array.Length >= 2) ? array[1] : blackboxFrame);
			return Sample(blackboxFrame, f, p_time, p_smooth);
		}

		public static BlackboxFrame Sample(List<BlackboxFrame> p_frames, float p_time, bool p_smooth)
		{
			return Sample(p_frames, p_time, -1, p_smooth);
		}

		public string GetMapGUID()
		{
			return header.Get("map", "");
		}

		public string GetCustomMapGUID()
		{
			return header.Get("custom-map", "");
		}

		public bool IsCustomMap()
		{
			return header.Get("is-custom-map", d: false);
		}

		public bool HasCustomMapFlag()
		{
			return header.ContainsKey("is-custom-map");
		}

		public string GetTrackGUID()
		{
			return header.Get("track", "");
		}

		public float GetRaceTime()
		{
			return header.Get("race-time", -1f);
		}

		public bool GetPhysicsFlag()
		{
			return header.Get("custom-physics", d: false);
		}

		public void SetPhysicsFlag(bool p_flag)
		{
			SerializedData serializedData = header;
			serializedData.Set("custom-physics", p_flag);
			header = serializedData;
		}

		public BlackboxData()
		{
			version = "1";
		}

		public void Compress()
		{
			if (!compressed)
			{
				compressed = true;
				compressedFrames = Serialize.ToGzip(frames);
				frames = new BlackboxFrame[0];
			}
		}

		public void Decompress()
		{
			if (compressed)
			{
				compressed = false;
				frames = Serialize.FromGZip<BlackboxFrame[]>(compressedFrames);
				compressedFrames = null;
			}
		}

		public void Prune()
		{
			for (int i = 0; i < frames.Length; i++)
			{
				frames[i].Prune();
			}
		}

		public BlackboxData(float p_duration, int p_fps, byte p_flags)
			: this()
		{
			flags = p_flags;
			float num = Mathf.Max(p_duration, 0f);
			float num2 = Mathf.Max(p_fps, 0f);
			int num3 = Mathf.RoundToInt(num * num2);
			int num4 = 0;
			int num5 = 1;
			for (int i = 0; i < 32; i++)
			{
				if ((p_flags & num5) != 0)
				{
					num4++;
				}
				num5 <<= 1;
			}
			num3 = Mathf.Max(0, num4 * num3);
			frames = new BlackboxFrame[num3];
			for (int j = 0; j < frames.Length; j++)
			{
				frames[j] = new BlackboxFrame();
				frames[j].Init();
			}
			iterator = 0;
			elapsed = 0f;
		}

		public BlackboxData(float p_duration, int p_fps, Enum p_flags)
			: this(p_duration, p_fps, (byte)Reflection<object>.GetEnum(p_flags))
		{
		}

		public void Clear()
		{
			elapsed = 0f;
			iterator = 0;
		}

		public bool IsAllowed(byte p_flag)
		{
			return (flags & p_flag) != 0;
		}

		public bool IsAllowed(DroneBlackboxDataFlag p_flag)
		{
			return IsAllowed((byte)p_flag);
		}

		public BlackboxFrame Push(byte p_type)
		{
			BlackboxFrame blackboxFrame;
			if (frames == null)
			{
				blackboxFrame = new BlackboxFrame();
				blackboxFrame.Init();
				return blackboxFrame;
			}
			if (iterator >= frames.Length)
			{
				blackboxFrame = new BlackboxFrame();
				blackboxFrame.Init();
				return blackboxFrame;
			}
			if (frames.Length == 0)
			{
				blackboxFrame = new BlackboxFrame();
				blackboxFrame.Init();
				return blackboxFrame;
			}
			blackboxFrame = frames[iterator];
			blackboxFrame.time = elapsed;
			blackboxFrame.type = p_type;
			frames[iterator] = blackboxFrame;
			iterator++;
			return blackboxFrame;
		}

		public BlackboxFrame Push(byte p_type, byte d)
		{
			BlackboxFrame blackboxFrame = Push(p_type);
			blackboxFrame.Set(blackboxFrame.time, p_type, d);
			return blackboxFrame;
		}

		public BlackboxFrame Push(byte p_type, Vector2 d)
		{
			BlackboxFrame blackboxFrame = Push(p_type);
			blackboxFrame.Set(blackboxFrame.time, p_type, d);
			return blackboxFrame;
		}

		public BlackboxFrame Push(byte p_type, Vector3 d)
		{
			BlackboxFrame blackboxFrame = Push(p_type);
			blackboxFrame.Set(blackboxFrame.time, p_type, d);
			return blackboxFrame;
		}

		public BlackboxFrame Push(byte p_type, Vector4 d)
		{
			BlackboxFrame blackboxFrame = Push(p_type);
			blackboxFrame.Set(blackboxFrame.time, p_type, d);
			return blackboxFrame;
		}

		public BlackboxFrame Push(byte p_type, Quaternion d)
		{
			BlackboxFrame blackboxFrame = Push(p_type);
			blackboxFrame.Set(blackboxFrame.time, p_type, d);
			return blackboxFrame;
		}

		public BlackboxFrame Push(byte p_type, SignalVector d)
		{
			BlackboxFrame blackboxFrame = Push(p_type);
			blackboxFrame.Set(blackboxFrame.time, p_type, d);
			return blackboxFrame;
		}

		public BlackboxFrame Push(byte p_type, Vector3 p, Quaternion r)
		{
			BlackboxFrame blackboxFrame = Push(p_type);
			blackboxFrame.Set(blackboxFrame.time, p_type, p, r);
			return blackboxFrame;
		}

		public BlackboxFrame Push(byte p_type, Transform d)
		{
			BlackboxFrame blackboxFrame = Push(p_type);
			blackboxFrame.Set(blackboxFrame.time, p_type, d);
			return blackboxFrame;
		}

		public BlackboxFrame Push(byte p_type, DroneFlightController d)
		{
			BlackboxFrame blackboxFrame = Push(p_type);
			blackboxFrame.Set(blackboxFrame.time, p_type, d);
			return blackboxFrame;
		}

		public BlackboxFrame Push(byte p_type, float[] d)
		{
			BlackboxFrame blackboxFrame = Push(p_type);
			blackboxFrame.Set(blackboxFrame.time, p_type, d);
			return blackboxFrame;
		}

		public BlackboxFrame Push(byte p_type, Vector3 p_dragK, Vector3 p_dragF, float[] p_thrust, float p_torque)
		{
			BlackboxFrame blackboxFrame = Push(p_type);
			blackboxFrame.Set(blackboxFrame.time, p_type, p_dragK, p_dragF, p_thrust, p_torque);
			return blackboxFrame;
		}

		public BlackboxFrame Push(byte p_type, int p_index, Vector3 p_position, Quaternion p_rotation)
		{
			BlackboxFrame blackboxFrame = Push(p_type);
			blackboxFrame.Set(blackboxFrame.time, p_type, p_index, p_position, p_rotation);
			return blackboxFrame;
		}

		public List<BlackboxFrame> GetFrames(byte p_flag)
		{
			List<BlackboxFrame> list = new List<BlackboxFrame>();
			if (p_flag <= 0)
			{
				return list;
			}
			for (int i = 0; i < frames.Length; i++)
			{
				BlackboxFrame blackboxFrame = frames[i];
				if (blackboxFrame == null)
				{
					Debug.LogWarning("DroneBlackbox> Layer[" + p_flag + "] Frame [" + i + "] is null");
					break;
				}
				if ((blackboxFrame.type & p_flag) != 0)
				{
					list.Add(blackboxFrame);
				}
			}
			return list;
		}

		public float GetCrashTime()
		{
			List<BlackboxFrame> list = GetFrames(32);
			for (int i = 0; i < list.Count; i++)
			{
				BlackboxFrame blackboxFrame = list[i];
				if ((byte)blackboxFrame.data[0] == 3)
				{
					return blackboxFrame.time;
				}
			}
			return -1f;
		}

		public void ParseTracks()
		{
			tracks = new Dictionary<byte, List<BlackboxFrame>>();
			int num = 1;
			for (int i = 0; i < 32; i++)
			{
				List<BlackboxFrame> list = GetFrames((byte)num);
				if (list.Count > 0)
				{
					tracks[(byte)num] = list;
				}
				num <<= 1;
			}
		}

		public void ClearTrackTable()
		{
			if (tracks != null)
			{
				tracks.Clear();
			}
		}

		public void Trim()
		{
			BlackboxFrame[] array = new BlackboxFrame[(iterator >= 0) ? iterator : 0];
			int num = Mathf.Min(array.Length, (frames != null) ? frames.Length : 0);
			for (int i = 0; i < num; i++)
			{
				array[i] = frames[i];
			}
			frames = null;
			frames = array;
		}

		public void Update(float p_dt)
		{
			elapsed += p_dt;
		}

		public string ToCSV()
		{
			StringBuilder stringBuilder = new StringBuilder();
			SerializedData serializedData = header;
			NumberFormatInfo numberFormatInfo = new NumberFormatInfo
			{
				NumberDecimalSeparator = "."
			};
			string text = serializedData.Get("player-id", "").ToUpper();
			string text2 = serializedData.Get("profile-name", "").ToUpper();
			string text3 = serializedData.Get("profile-color", "ff0000");
			string text4 = ((ControllerStateType)serializedData.Get("controller-type", 2)/*cast due to .constrained prefix*/).ToString();
			float num = serializedData.Get("camera-tilt", 0f);
			float num2 = serializedData.Get("camera-fov", 0f);
			float num3 = serializedData.Get("race-time", 0f);
			float num4 = elapsed;
			stringBuilder.Append("#player-id,player-name,player-color,map-guid,track-guid,custom-map-guid,controller-type,camera-tilt,camera-fov,race-time");
			stringBuilder.Append("\n");
			stringBuilder.Append(text + ",");
			stringBuilder.Append(text2 + ",");
			stringBuilder.Append("0x" + text3 + ",");
			stringBuilder.Append(GetMapGUID() + ",");
			stringBuilder.Append(GetTrackGUID() + ",");
			stringBuilder.Append(GetCustomMapGUID() + ",");
			stringBuilder.Append(text4 + ",");
			stringBuilder.Append(num.ToString(numberFormatInfo) + ",");
			stringBuilder.Append(num2.ToString(numberFormatInfo) + ",");
			stringBuilder.Append(num3.ToString(numberFormatInfo) ?? "");
			stringBuilder.Append("\n");
			stringBuilder.Append("#event-index,type,time,pos-x,pos-y,pos-z,data-0,data-1,data-2,data-3");
			stringBuilder.Append("\n");
			List<BlackboxFrame> list = GetFrames(32);
			for (int i = 0; i < list.Count; i++)
			{
				BlackboxFrame blackboxFrame = list[i];
				object[] data = blackboxFrame.data;
				stringBuilder.Append($"{i},");
				string text5 = ((data.Length >= 1) ? $"{data[0]}" : "");
				stringBuilder.Append(text5 + ",");
				stringBuilder.Append(blackboxFrame.time.ToString(numberFormatInfo) + ",");
				object obj = ((data.Length >= 2) ? data[1] : null);
				text5 = ((obj == null) ? "" : ((obj is float num5) ? num5.ToString(numberFormatInfo) : obj.ToString()));
				stringBuilder.Append(text5 + ",");
				obj = ((data.Length >= 3) ? data[2] : null);
				text5 = ((obj == null) ? "" : ((obj is float num6) ? num6.ToString(numberFormatInfo) : obj.ToString()));
				stringBuilder.Append(text5 + ",");
				obj = ((data.Length >= 4) ? data[3] : null);
				text5 = ((obj == null) ? "" : ((obj is float num7) ? num7.ToString(numberFormatInfo) : obj.ToString()));
				stringBuilder.Append(text5 + ",");
				obj = ((data.Length >= 5) ? data[4] : null);
				text5 = ((obj == null) ? "" : ((obj is float num8) ? num8.ToString(numberFormatInfo) : obj.ToString()));
				stringBuilder.Append(text5 + ",");
				obj = ((data.Length >= 6) ? data[4] : null);
				text5 = ((obj == null) ? "" : ((obj is float num9) ? num9.ToString(numberFormatInfo) : obj.ToString()));
				stringBuilder.Append(text5 + ",");
				obj = ((data.Length >= 7) ? data[4] : null);
				text5 = ((obj == null) ? "" : ((obj is float num10) ? num10.ToString(numberFormatInfo) : obj.ToString()));
				stringBuilder.Append(text5 + ",");
				obj = ((data.Length >= 8) ? data[4] : null);
				text5 = ((obj == null) ? "" : ((obj is float num11) ? num11.ToString(numberFormatInfo) : obj.ToString()));
				stringBuilder.Append(text5 ?? "");
				if (i < list.Count - 1)
				{
					stringBuilder.Append("\n");
				}
			}
			stringBuilder.Append("\n");
			stringBuilder.Append("#sample-index,time-seconds,pos-x,pos-y,pos-z,rot-x,rot-y,rot-z,rot-w,vel-x,vel-y,vel-z,yaw,throttle,pitch,roll");
			stringBuilder.Append("\n");
			float num12 = 0.016f;
			float num13 = Mathf.Floor(num4 / num12) * num12;
			object[] array = new object[16];
			int num14 = 0;
			for (float num15 = 0f; num15 <= num13; num15 += num12)
			{
				int num16 = 0;
				Vector3 p = Vector3.zero;
				Vector3 vector = Vector3.zero;
				Quaternion r = Quaternion.identity;
				Quaternion quaternion = Quaternion.identity;
				Vector4 zero = Vector4.zero;
				Vector4 vector2 = Vector4.zero;
				byte key = 1;
				if (tracks.ContainsKey(key))
				{
					Sample(tracks[key], num15, p_smooth: true).GetTransform(out p, out r);
					quaternion = r;
				}
				key = 2;
				if (tracks.ContainsKey(key))
				{
					vector = Sample(tracks[key], num15, p_smooth: true).GetVector3();
				}
				key = 4;
				if (tracks.ContainsKey(key))
				{
					float[] floats = Sample(tracks[key], num15, p_smooth: true).GetFloats();
					zero[0] = ((floats.Length != 0) ? floats[0] : 0f);
					zero[1] = ((floats.Length > 1) ? floats[1] : floats[0]);
					zero[2] = ((floats.Length > 2) ? floats[2] : floats[0]);
					zero[3] = ((floats.Length > 3) ? floats[3] : floats[0]);
				}
				key = 8;
				if (tracks.ContainsKey(key))
				{
					vector2 = Sample(tracks[key], num15, p_smooth: true).GetVector4();
				}
				array[num16++] = num14++;
				array[num16++] = num15.ToString(numberFormatInfo);
				array[num16++] = p.x.ToString(numberFormatInfo);
				array[num16++] = p.y.ToString(numberFormatInfo);
				array[num16++] = p.z.ToString(numberFormatInfo);
				array[num16++] = quaternion.x.ToString(numberFormatInfo);
				array[num16++] = quaternion.y.ToString(numberFormatInfo);
				array[num16++] = quaternion.z.ToString(numberFormatInfo);
				array[num16++] = quaternion.w.ToString(numberFormatInfo);
				array[num16++] = vector.x.ToString(numberFormatInfo);
				array[num16++] = vector.y.ToString(numberFormatInfo);
				array[num16++] = vector.z.ToString(numberFormatInfo);
				array[num16++] = vector2.x.ToString(numberFormatInfo);
				array[num16++] = vector2.y.ToString(numberFormatInfo);
				array[num16++] = vector2.z.ToString(numberFormatInfo);
				array[num16++] = vector2.w.ToString(numberFormatInfo);
				stringBuilder.AppendLine(string.Join(",", array));
			}
			return stringBuilder.ToString();
		}
	}
}
