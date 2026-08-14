using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class MACameraToolControlPoint : MAGuide
	{
		public struct Sample
		{
			public CameraToolTrackingMode trackingMode;

			public float trackingDelay;

			public Vector3 position;

			public Quaternion rotation;

			public float fov;

			public Vector3 cameraOrbitAngle;

			public float cameraDistance;

			public Vector3 cameraOffset;

			public static Sample Lerp(Sample v0, Sample v1, float r)
			{
				return new Sample
				{
					trackingMode = ((r < 0.5f) ? v0.trackingMode : v1.trackingMode),
					trackingDelay = Mathf.LerpUnclamped(v0.trackingDelay, v1.trackingDelay, r),
					position = Vector3.LerpUnclamped(v0.position, v1.position, r),
					rotation = Quaternion.LerpUnclamped(v0.rotation, v1.rotation, r),
					fov = Mathf.LerpUnclamped(v0.fov, v1.fov, r),
					cameraOrbitAngle = Vector3.LerpUnclamped(v0.cameraOrbitAngle, v1.cameraOrbitAngle, r),
					cameraDistance = Mathf.LerpUnclamped(v0.cameraDistance, v1.cameraDistance, r),
					cameraOffset = Vector3.LerpUnclamped(v0.cameraOffset, v1.cameraOffset, r)
				};
			}

			public static float GetTargetRatio(Sample v0, Sample v1, Vector3 p_target, float p_offset = 0f)
			{
				Vector3 vector = v0.position;
				Vector3 rhs = v1.position - vector;
				float magnitude = rhs.magnitude;
				rhs *= ((magnitude <= 0.0001f) ? 0f : (1f / magnitude));
				return (Vector3.Dot(p_target - vector, rhs) + p_offset) * ((magnitude <= 0.0001f) ? 0f : (1f / magnitude));
			}

			public static Sample Lerp(Sample v0, Sample v1, Vector3 p_target)
			{
				float targetRatio = GetTargetRatio(v0, v1, p_target);
				return Lerp(v0, v1, Mathf.Clamp01(targetRatio));
			}

			public static Vector3 GetPositionOffset(Sample v0, Sample v1, float r)
			{
				Vector3 vector = Vector3.LerpUnclamped(v0.cameraOffset, v1.cameraOffset, r);
				Vector3 a = v0.position;
				Vector3 b = v1.position;
				Vector3 normalized = (v1.position - v0.position).normalized;
				Vector3 normalized2 = Vector3.Cross(normalized, Vector3.up).normalized;
				Vector3 normalized3 = Vector3.Cross(normalized, normalized2).normalized;
				return Vector3.LerpUnclamped(a, b, r) + normalized2 * vector.x + normalized3 * vector.y + normalized * vector.z;
			}

			public static TransformVector Evaluate(Sample p_tool_sample, Transform p_target, Vector3 p_pivot, TransformVector p_default)
			{
				TransformVector result = p_default;
				Sample sample = p_tool_sample;
				switch (sample.trackingMode)
				{
				case CameraToolTrackingMode.FPV:
				{
					Vector3 vector2 = p_target.rotation * Vector3.forward;
					_ = p_target.position + vector2 * 0.02f;
					result.Set(p_target.position, p_target.rotation);
					break;
				}
				case CameraToolTrackingMode.LookAt:
					result.Set(p_pivot, Quaternion.LookRotation(p_target.position - p_pivot, Vector3.up));
					break;
				case CameraToolTrackingMode.Orbit:
				{
					Vector3 up = Vector3.up;
					Vector3 right = Vector3.right;
					Vector3 vector = sample.rotation * Vector3.forward;
					Quaternion quaternion = Quaternion.AngleAxis(sample.cameraOrbitAngle.x, up);
					quaternion = Quaternion.identity * quaternion * Quaternion.AngleAxis(sample.cameraOrbitAngle.y, right);
					Vector3 p_position = p_target.position + vector * (0f - sample.cameraDistance);
					result.Set(p_position, quaternion);
					break;
				}
				case CameraToolTrackingMode.Static:
					result.Set(p_pivot, sample.rotation);
					break;
				}
				return result;
			}
		}

		[SerializeField]
		private float m_fov = 60f;

		[SerializeField]
		private CameraToolTrackingMode m_tracking_mode = CameraToolTrackingMode.Static;

		[SerializeField]
		private float m_tracking_delay;

		[SerializeField]
		private Vector3 m_camera_offset;

		[SerializeField]
		private float m_camera_distance;

		[SerializeField]
		private Vector2 m_camera_orbit_angle;

		[SerializeField]
		private MACameraTool m_tool;

		private Transform m_transform;

		public float fov
		{
			get
			{
				return m_fov;
			}
			set
			{
				m_fov = value;
				Write();
			}
		}

		public CameraToolTrackingMode trackingMode
		{
			get
			{
				return m_tracking_mode;
			}
			set
			{
				m_tracking_mode = value;
				Write();
			}
		}

		public float trackingDelay
		{
			get
			{
				return m_tracking_delay;
			}
			set
			{
				m_tracking_delay = value;
				Write();
			}
		}

		public Vector3 cameraOffset
		{
			get
			{
				return m_camera_offset;
			}
			set
			{
				m_camera_offset = value;
				Write();
			}
		}

		public float cameraDistance
		{
			get
			{
				return m_camera_distance;
			}
			set
			{
				m_camera_distance = value;
				Write();
			}
		}

		public Vector2 cameraOrbitAngle
		{
			get
			{
				return m_camera_orbit_angle;
			}
			set
			{
				m_camera_orbit_angle = value;
				Write();
			}
		}

		public MACameraTool tool
		{
			get
			{
				if (!m_tool)
				{
					return m_tool = Hierarchy.FindReverse<MACameraTool>(transform);
				}
				return m_tool;
			}
		}

		public new MDCameraToolControlPoint data
		{
			get
			{
				return base.data as MDCameraToolControlPoint;
			}
			set
			{
				base.data = value;
			}
		}

		public new Transform transform
		{
			get
			{
				if (!m_transform)
				{
					return m_transform = base.transform;
				}
				return m_transform;
			}
		}

		public override void Write()
		{
			base.Write();
			MDCameraToolControlPoint mDCameraToolControlPoint = data;
			if (mDCameraToolControlPoint != null)
			{
				mDCameraToolControlPoint.fov = fov;
				mDCameraToolControlPoint.trackingMode = trackingMode;
				mDCameraToolControlPoint.trackingDelay = trackingDelay;
				mDCameraToolControlPoint.cameraDistance = cameraDistance;
				mDCameraToolControlPoint.cameraOffset = cameraOffset;
				mDCameraToolControlPoint.cameraOrbitAngle = cameraOrbitAngle;
			}
		}

		public override void Read()
		{
			if (m_data is MDCameraToolControlPoint mDCameraToolControlPoint)
			{
				m_fov = mDCameraToolControlPoint.fov;
				m_tracking_mode = mDCameraToolControlPoint.trackingMode;
				m_tracking_delay = mDCameraToolControlPoint.trackingDelay;
				m_camera_distance = mDCameraToolControlPoint.cameraDistance;
				m_camera_offset = mDCameraToolControlPoint.cameraOffset;
				m_camera_orbit_angle = mDCameraToolControlPoint.cameraOrbitAngle;
			}
			base.Read();
		}

		protected override MDObject NewData()
		{
			return new MDCameraToolControlPoint();
		}

		protected void Start()
		{
			if ((bool)tool)
			{
				tool.RefreshHierarchy();
				tool.Refresh();
			}
		}

		public Sample GetSample()
		{
			return new Sample
			{
				trackingMode = trackingMode,
				position = transform.position,
				rotation = transform.localRotation,
				fov = fov,
				cameraOffset = cameraOffset,
				cameraOrbitAngle = cameraOrbitAngle,
				cameraDistance = cameraDistance,
				trackingDelay = trackingDelay
			};
		}

		public void SetSample(Sample v)
		{
			transform.position = v.position;
			transform.localRotation = v.rotation;
			fov = v.fov;
			cameraOffset = v.cameraOffset;
			cameraOrbitAngle = v.cameraOrbitAngle;
			cameraDistance = v.cameraDistance;
			trackingDelay = v.trackingDelay;
		}

		public override void OnEditorSelect()
		{
			base.OnEditorSelect();
			if ((bool)tool && (bool)tool.collider)
			{
				tool.SetColliderActiveAsync(p_flag: true);
			}
		}

		public override void OnEditorUnselect()
		{
			base.OnEditorUnselect();
			if ((bool)tool && (bool)tool.collider)
			{
				tool.SetColliderActiveAsync(p_flag: false);
			}
		}
	}
}
