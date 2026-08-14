using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace drl.network
{
	public class NetworkRacer : MonoBehaviour, INetworkObject, IPunObservable
	{
		public struct Sample
		{
			public int id;

			public float time;

			public Vector3 position;

			public Quaternion rotation;

			public Vector3 velocity;

			public void ToArray(float[] a)
			{
				int num = 0;
				a[num++] = id;
				a[num++] = time;
				a[num++] = position.x;
				a[num++] = position.y;
				a[num++] = position.z;
				a[num++] = rotation.x;
				a[num++] = rotation.y;
				a[num++] = rotation.z;
				a[num++] = rotation.w;
				a[num++] = velocity.x;
				a[num++] = velocity.y;
				a[num++] = velocity.z;
			}

			public void FromArray(float[] a)
			{
				int num = 0;
				id = (int)a[num++];
				time = a[num++];
				position = new Vector3(a[num++], a[num++], a[num++]);
				rotation = new Quaternion(a[num++], a[num++], a[num++], a[num++]);
				velocity = new Vector3(a[num++], a[num++], a[num++]);
			}

			public override string ToString()
			{
				return string.Format("[{0}] {1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", id, time.ToString("0.00"), position.x.ToString("0.0"), position.y.ToString("0.0"), position.z.ToString("0.0"), rotation.x.ToString("0.0"), rotation.y.ToString("0.0"), rotation.z.ToString("0.0"), rotation.w.ToString("0"), velocity.x.ToString("0"), velocity.y.ToString("0"), velocity.z.ToString("0"));
			}
		}

		public static bool interpolationV2;

		[SerializeField]
		protected PhotonTransformViewRotationModel rotationModel = new PhotonTransformViewRotationModel();

		protected NetworkVector3Interpolator positionController;

		protected PhotonTransformViewRotationControl rotationController;

		protected readonly NetworkULongInterpolator inputAndRPMInterpolator = new NetworkULongInterpolator();

		protected PhotonView photonView;

		public Vector3 networkPosition;

		public Quaternion networkRotation;

		public Vector3 networkVelocity;

		public bool interpolate;

		public bool syncEnabled;

		public float syncEasing = 1E-06f;

		public float syncBias;

		public List<float[]> frames;

		public Sample[] samples;

		private int m_frame_index;

		private int m_frame_total;

		private int m_sample_index;

		private float m_sync_elapsed;

		private bool m_sync_active;

		private float[] m_frame_temp;

		private int m_sample_count;

		private float m_samples_duration;

		private float m_sync_deviation_factor;

		private StreamWriter m_logger;

		private GUIStyle m_lb_style;

		public int ID
		{
			get
			{
				if (Actor != null)
				{
					return Actor.ID;
				}
				return -1;
			}
		}

		public NetworkActor Actor { get; set; }

		public INetworkObservable Observed { get; private set; }

		protected NetworkRoom room { get; private set; }

		protected virtual void Awake()
		{
			photonView = GetComponent<PhotonView>() ?? base.gameObject.AddComponent<PhotonView>();
			photonView.synchronization = ViewSynchronization.Unreliable;
			photonView.ObservedComponents = new List<Component> { this };
			photonView.group = 1;
			Observed = GetComponent<INetworkObservable>();
			rotationModel.SynchronizeEnabled = true;
			rotationModel.InterpolateOption = PhotonTransformViewRotationModel.InterpolateOptions.Lerp;
			frames = new List<float[]>();
			for (int i = 0; i < 60; i++)
			{
				frames.Add(new float[12]);
			}
			m_frame_temp = new float[12];
			samples = new Sample[1800];
			positionController = new NetworkVector3Interpolator();
			rotationController = new PhotonTransformViewRotationControl(rotationModel);
			networkPosition = Vector3.zero;
			networkRotation = Quaternion.identity;
			syncEnabled = !interpolationV2;
			float b = PhotonNetwork.GetPing();
			syncBias = Mathf.Max(110f, b) * 0.001f * 2.5f;
			if (syncBias <= 0f)
			{
				syncBias = 0.25f;
			}
			syncEasing = syncBias * 0.3f;
			m_sync_deviation_factor = 1f;
		}

		public virtual void SetRacer(NetworkActor actor, NetworkRoom currentRoom)
		{
			Actor = actor;
			photonView.viewID = actor.ViewId;
			room = currentRoom;
			if (room.GameMode == NetworkRoom.GameType.Freestyle)
			{
				interpolate = true;
			}
		}

		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (Observed == null || !(Observed.NetworkTransform != null) || !Observed.CanSync)
			{
				return;
			}
			if (!interpolationV2)
			{
				positionController.OnPhotonSerializeView(Observed.NetworkTransform.localPosition, stream, info);
				rotationController.OnPhotonSerializeView(Observed.NetworkTransform.localRotation, stream, info);
			}
			inputAndRPMInterpolator.OnPhotonSerializeView(Observed.PackedInputAndRPM, stream, info);
			if (stream.isReading)
			{
				if (interpolationV2)
				{
					Sample sample = default(Sample);
					int num = (byte)stream.ReceiveNext();
					for (int i = 0; i < num; i++)
					{
						for (int j = 0; j < m_frame_temp.Length; j++)
						{
							m_frame_temp[j] = (float)stream.ReceiveNext();
						}
						sample.FromArray(m_frame_temp);
						m_samples_duration = Mathf.Max(m_samples_duration, sample.time);
						int num2 = ((m_sample_count > 0) ? ((m_sample_count - 1) % samples.Length) : 0);
						int num3 = m_sample_count % samples.Length;
						if (samples[num2].id < sample.id)
						{
							samples[num3] = sample;
							m_sample_count++;
						}
						else
						{
							Debug.LogWarning($"NetworkRacer> Sample received is before in time than latest sample [{m_sample_count}]");
						}
					}
				}
				else
				{
					syncEnabled = true;
					networkPosition = positionController.currentValue.Position;
					networkRotation = rotationController.GetNetworkRotation();
					networkVelocity = (Vector3)stream.ReceiveNext();
				}
			}
			else if (interpolationV2)
			{
				int frame_index = m_frame_index;
				m_frame_index = 0;
				stream.SendNext((byte)frame_index);
				Sample sample2 = default(Sample);
				for (int k = 0; k < frame_index; k++)
				{
					sample2.FromArray(frames[k]);
					for (int l = 0; l < frames[k].Length; l++)
					{
						stream.SendNext(frames[k][l]);
					}
				}
			}
			else
			{
				stream.SendNext(Observed.NetworkRigidbody.velocity);
			}
		}

		protected void Update()
		{
			if (room == null || !interpolationV2)
			{
				return;
			}
			switch (room.State)
			{
			case NetworkRoom.StateCode.GameLoading:
				m_sync_elapsed = 0f;
				m_sample_index = 0;
				m_frame_total = 0;
				m_sync_deviation_factor = 1f;
				m_sync_active = false;
				m_samples_duration = 0f;
				networkPosition = Vector3.zero;
				networkRotation = Quaternion.identity;
				break;
			case NetworkRoom.StateCode.GameRunning:
			{
				bool isLocal = Actor.IsLocal;
				float sync_elapsed = m_sync_elapsed;
				float num = Time.deltaTime;
				if (isLocal)
				{
					if (m_frame_index >= frames.Count)
					{
						m_frame_index--;
						float[] item = frames[0];
						frames.RemoveAt(0);
						frames.Add(item);
					}
					float[] a = frames[m_frame_index];
					m_frame_index++;
					Vector3 position = ((Observed == null) ? Vector3.zero : (Observed.NetworkTransform ? Observed.NetworkTransform.localPosition : Vector3.zero));
					Quaternion rotation = ((Observed == null) ? Quaternion.identity : (Observed.NetworkTransform ? Observed.NetworkTransform.localRotation : Quaternion.identity));
					Vector3 velocity = ((Observed == null) ? Vector3.zero : (Observed.NetworkRigidbody ? Observed.NetworkRigidbody.velocity : Vector3.zero));
					Sample sample = new Sample
					{
						id = m_frame_total,
						time = sync_elapsed,
						position = position,
						rotation = rotation,
						velocity = velocity
					};
					m_frame_total++;
					sample.ToArray(a);
				}
				else
				{
					int sample_count = m_sample_count;
					if (sample_count < 2)
					{
						break;
					}
					if (sample_count > 0 && !m_sync_active)
					{
						m_sync_active = true;
						m_sync_elapsed = Mathf.Max(m_samples_duration - 0.05f, 0f);
					}
					if (m_sync_elapsed >= m_samples_duration)
					{
						m_sync_elapsed = m_samples_duration;
					}
					sync_elapsed = m_sync_elapsed;
					for (int i = Mathf.Max(0, m_sample_index - 30); i < sample_count; i++)
					{
						int num2 = i;
						int num3 = i + 1;
						if (num3 >= sample_count)
						{
							num3 = sample_count - 1;
						}
						Sample sample2 = samples[num2 % samples.Length];
						Sample sample3 = samples[num3 % samples.Length];
						if (sync_elapsed >= sample2.time && sync_elapsed < sample3.time)
						{
							m_sample_index = i;
							break;
						}
					}
					float t = Mathf.Clamp01((m_samples_duration - m_sync_elapsed) / syncBias / 2f);
					float num4 = Mathf.Lerp(0.8f, 1.2f, t);
					num4 = Mathf.Floor(num4 * 5f) / 5f;
					m_sync_deviation_factor = Mathf.Lerp(m_sync_deviation_factor, num4, Time.deltaTime / 3f);
					num *= m_sync_deviation_factor;
					int sample_index = m_sample_index;
					int num5 = sample_index + 1;
					if (num5 >= sample_count)
					{
						num5 = sample_count - 1;
					}
					Sample sample4 = samples[sample_index % samples.Length];
					Sample sample5 = samples[num5 % samples.Length];
					float num6 = sample5.time - sample4.time;
					float num7 = Mathf.Max(sync_elapsed - sample4.time, 0f);
					float t2 = ((num6 <= 0f) ? 0f : Mathf.Clamp01(num7 / num6));
					networkPosition = Vector3.Lerp(sample4.position, sample5.position, t2);
					networkRotation = Quaternion.Lerp(sample4.rotation, sample5.rotation, t2);
					networkVelocity = Vector3.Lerp(sample4.velocity, sample5.velocity, t2);
					syncEnabled = true;
				}
				m_sync_elapsed += num;
				break;
			}
			case NetworkRoom.StateCode.GameWarmup:
			case NetworkRoom.StateCode.GameFinished:
				break;
			}
		}

		protected void OnGUI()
		{
			if (!interpolationV2 || room.State != NetworkRoom.StateCode.GameRunning || Actor.IsLocal)
			{
				return;
			}
			int num = -1;
			switch (base.name)
			{
			case "nt-0":
				num = 0;
				break;
			case "nt-1":
				num = 1;
				break;
			case "nt-2":
				num = 2;
				break;
			case "nt-3":
				num = 3;
				break;
			case "nt-4":
				num = 4;
				break;
			case "nt-5":
				num = 5;
				break;
			}
			if (num >= 0)
			{
				float num2 = m_samples_duration - m_sync_elapsed;
				GUI.color = Color.yellow;
				if (m_lb_style == null)
				{
					m_lb_style = new GUIStyle(GUI.skin.label);
				}
				m_lb_style.fontSize = 12;
				m_lb_style.fontStyle = FontStyle.Bold;
				string text = string.Format("name[{0}] t[{1} / {2}] dev[{3}] sample-index[{4} / {5}] sample-index-wrap[{6} / {7}]", base.name, m_sync_elapsed.ToString("0.00"), m_samples_duration.ToString("0.00"), num2.ToString("0.000"), m_sample_index, m_sample_count, m_sample_index % samples.Length, samples.Length);
				GUI.Label(new Rect(15f, 10f + (float)num * 13f, 600f, 20f), text);
			}
		}

		public virtual void CleanUp()
		{
			Object.Destroy(photonView);
			Object.Destroy(this);
		}

		protected void OnDestroy()
		{
			if (m_logger != null)
			{
				m_logger.BaseStream.Flush();
				m_logger.BaseStream.Close();
				m_logger.Dispose();
			}
		}
	}
}
