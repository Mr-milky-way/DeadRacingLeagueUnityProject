using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class BlackboxRecordGizmo : MonoBehaviour
	{
		private TextAsset m_data;

		internal BlackboxRecord record;

		public int clipId;

		public float elapsed;

		public float speed = 1f;

		public bool playing;

		public Vector3 currentPosition;

		public Quaternion currentRotation;

		public Vector3 currentVelocity;

		public Vector4 currentInput;

		public Vector3 currentPID;

		public float[] currentRPM;

		public float currentTorque;

		public float[] currentThrust;

		public Vector3 currentDragFactors;

		public Vector3 currentDragForce;

		public bool hasPhysics;

		internal List<Vector3> positions;

		internal List<Vector3> segment0;

		internal List<Vector3> segment1;

		internal List<Vector4> gates;

		internal List<Vector4> resets;

		internal List<Vector4> collisions;

		internal List<ActionEventData> actions;

		internal bool parse_lock;

		internal Thread parse_thread;

		public TextAsset data
		{
			get
			{
				return m_data;
			}
			set
			{
				m_data = value;
				record = null;
				if (parse_thread != null)
				{
					parse_thread.Abort();
				}
				parse_thread = null;
				Parse();
			}
		}

		internal void SetClip(int p_id)
		{
			if (!data || record == null || record.clips.Count <= 0)
			{
				return;
			}
			int index = (clipId = Mathf.Clamp(p_id, 0, record.clips.Count - 1));
			playing = false;
			elapsed = 0f;
			BlackboxData blackboxData = record.clips[index];
			byte key = 1;
			positions = new List<Vector3>();
			segment0 = new List<Vector3>();
			segment1 = new List<Vector3>();
			List<BlackboxFrame> list;
			if (blackboxData.tracks.ContainsKey(key))
			{
				list = blackboxData.tracks[key];
				for (int i = 0; i < list.Count; i++)
				{
					BlackboxFrame blackboxFrame = list[i];
					float x = Reflection<object>.Get<float>(blackboxFrame.data, 0);
					float y = Reflection<object>.Get<float>(blackboxFrame.data, 1);
					float z = Reflection<object>.Get<float>(blackboxFrame.data, 2);
					positions.Add(new Vector3(x, y, z));
				}
				if (positions.Count > 4)
				{
					for (int j = 1; j < positions.Count; j += 2)
					{
						Vector3 item = positions[j - 1];
						Vector3 item2 = positions[j];
						segment0.Add(item);
						segment0.Add(item2);
						item = positions[j];
						item2 = ((j + 1 < positions.Count) ? positions[j + 1] : item);
						segment1.Add(item);
						segment1.Add(item2);
					}
				}
			}
			key = 32;
			gates = new List<Vector4>();
			collisions = new List<Vector4>();
			resets = new List<Vector4>();
			actions = new List<ActionEventData>();
			if (!blackboxData.tracks.ContainsKey(key))
			{
				return;
			}
			list = blackboxData.tracks[key];
			for (int k = 0; k < list.Count; k++)
			{
				BlackboxFrame blackboxFrame2 = list[k];
				ReplayEventType replayEventType = (ReplayEventType)Reflection<object>.Get<byte>(blackboxFrame2.data, 0);
				float x2 = Reflection<object>.Get<float>(blackboxFrame2.data, 1);
				float y2 = Reflection<object>.Get<float>(blackboxFrame2.data, 2);
				float z2 = Reflection<object>.Get<float>(blackboxFrame2.data, 3);
				Vector4 vector = new Vector4(x2, y2, z2, blackboxFrame2.time);
				object[] array = Reflection<object>.Get<object[]>(blackboxFrame2.data, 4);
				switch (replayEventType)
				{
				case ReplayEventType.Gate:
					gates.Add(vector);
					break;
				case ReplayEventType.Hit:
					collisions.Add(vector);
					break;
				case ReplayEventType.Reset:
					resets.Add(vector);
					break;
				case ReplayEventType.Action:
				{
					ActionEventData item3 = new ActionEventData
					{
						@event = vector,
						data = array
					};
					actions.Add(item3);
					break;
				}
				}
			}
		}

		internal void Seek(float p_time)
		{
			if ((bool)data && record != null && record.clips.Count > 0)
			{
				int index = Mathf.Clamp(clipId, 0, record.clips.Count - 1);
				BlackboxData blackboxData = record.clips[index];
				float max = blackboxData.elapsed;
				float p_time2 = Mathf.Clamp(p_time, 0f, max);
				byte key = 1;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time2, p_smooth: true);
					blackboxFrame.GetTransform(out currentPosition, out currentRotation);
				}
				key = 2;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time2, p_smooth: true);
					currentVelocity = blackboxFrame.GetVector3();
				}
				if (currentRPM == null)
				{
					currentRPM = new float[0];
				}
				key = 4;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time2, p_smooth: true);
					currentRPM = blackboxFrame.GetFloats();
				}
				key = 8;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time2, p_smooth: true);
					currentInput = blackboxFrame.GetVector4();
				}
				key = 16;
				if (blackboxData.tracks.ContainsKey(key))
				{
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time2, p_smooth: true);
					currentPID = blackboxFrame.GetVector3();
				}
				if (currentThrust == null)
				{
					currentThrust = new float[0];
				}
				key = 64;
				hasPhysics = false;
				if (blackboxData.tracks.ContainsKey(key))
				{
					hasPhysics = true;
					BlackboxFrame blackboxFrame = BlackboxData.Sample(blackboxData.tracks[key], p_time2, p_smooth: true);
					blackboxFrame.GetPhysics(out currentDragFactors, out currentDragForce, out currentThrust, out currentTorque);
				}
			}
		}

		internal void Parse()
		{
			if (!data || parse_lock || record != null)
			{
				return;
			}
			parse_lock = true;
			record = null;
			byte[] data_bytes = data.bytes;
			parse_thread = new Thread((ThreadStart)delegate
			{
				if ((bool)data)
				{
					record = null;
					try
					{
						record = Serialize.FromBytes<BlackboxRecord>(data_bytes);
					}
					catch (Exception ex)
					{
						Debug.LogError("BlackboxRecordGizmo> Parse Error\n" + ex.Message);
					}
					if (record != null)
					{
						record.ParseTracks();
						record.ClearFrames();
						SetClip(clipId);
					}
					parse_lock = false;
					parse_thread = null;
				}
			});
			parse_thread.Start();
		}
	}
}
