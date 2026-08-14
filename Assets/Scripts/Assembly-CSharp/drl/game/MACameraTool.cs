using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class MACameraTool : MARenderer
	{
		[SerializeField]
		private SegmentStripeRenderer m_segment_renderer;

		private List<MACameraToolControlPoint> m_cache_cpl;

		private bool m_lock_gizmo_refresh;

		[SerializeField]
		private CameraToolMode m_mode;

		[SerializeField]
		private int m_index = 999;

		[SerializeField]
		private string m_easing_mode = "linear";

		private MACameraToolAnimation.Easing m_easing;

		public string cameraToolControlPointId = "DMA-fee8";

		public string cameraToolColliderId = "DMA-3faf";

		[SerializeField]
		private MACameraToolAnimation m_animation;

		[SerializeField]
		private MACameraToolCollider m_collider;

		private Activity m_collider_active_t;

		public TransformVector sample;

		private List<MACameraToolControlPoint> m_ctcp_cache;

		private Transform m_tcache;

		private bool m_tdirty;

		public SegmentStripeRenderer lineRenderer
		{
			get
			{
				if (!m_segment_renderer)
				{
					return m_segment_renderer = GetComponent<SegmentStripeRenderer>();
				}
				return m_segment_renderer;
			}
		}

		public CameraToolMode mode
		{
			get
			{
				if (m_mode != CameraToolMode.None)
				{
					return m_mode;
				}
				switch (Mathf.Clamp(GetControlPoints().Count, 0, 2))
				{
				case 0:
					m_mode = CameraToolMode.Invalid;
					break;
				case 1:
					m_mode = CameraToolMode.Single;
					break;
				case 2:
					m_mode = CameraToolMode.Wire;
					break;
				}
				return m_mode;
			}
		}

		public int index
		{
			get
			{
				return m_index;
			}
			set
			{
				m_index = value;
				Write();
			}
		}

		public string easingMode
		{
			get
			{
				return m_easing_mode;
			}
			set
			{
				m_easing_mode = value;
				Write();
			}
		}

		public MACameraToolAnimation.Easing easing
		{
			get
			{
				MACameraToolAnimation.Easing easing = m_easing;
				if (easing != null && easing.id != m_easing_mode)
				{
					easing = null;
				}
				if (easing == null)
				{
					easing = animation.Get(m_easing_mode);
				}
				if (easing == null)
				{
					easing = animation.Get("linear");
					m_easing_mode = "linear";
				}
				return easing;
			}
		}

		public MACameraToolAnimation animation
		{
			get
			{
				if (!m_animation)
				{
					return m_animation = GetComponent<MACameraToolAnimation>();
				}
				return m_animation;
			}
		}

		public MACameraToolCollider collider
		{
			get
			{
				if ((bool)m_collider)
				{
					return m_collider;
				}
				Transform transform = FetchValidCollider();
				if (!transform)
				{
					return null;
				}
				m_collider = transform.GetComponent<MACameraToolCollider>();
				return m_collider;
			}
		}

		public new MDCameraTool data
		{
			get
			{
				return base.data as MDCameraTool;
			}
			set
			{
				base.data = value;
			}
		}

		public List<MACameraToolControlPoint> GetControlPoints(bool p_cache = false)
		{
			if (m_cache_cpl != null && p_cache)
			{
				return m_cache_cpl;
			}
			if (m_cache_cpl == null)
			{
				m_cache_cpl = new List<MACameraToolControlPoint>();
			}
			m_cache_cpl = Hierarchy.FindAll<MACameraToolControlPoint>(base.transform);
			return m_cache_cpl;
		}

		public bool HasControlPoints()
		{
			return GetControlPoints().Count >= 1;
		}

		public void FitCollider()
		{
			MACameraToolCollider mACameraToolCollider = collider;
			if ((bool)mACameraToolCollider)
			{
				List<MACameraToolControlPoint> controlPoints = GetControlPoints();
				switch (controlPoints.Count)
				{
				case 1:
					mACameraToolCollider.transform.position = controlPoints[0].transform.position;
					mACameraToolCollider.transform.localScale = new Vector3(4f, 4f, 1f);
					mACameraToolCollider.transform.localRotation = Quaternion.LookRotation(controlPoints[0].transform.forward, Vector3.up);
					break;
				case 2:
				{
					Vector3 position = controlPoints[0].transform.position;
					Vector3 position2 = controlPoints[1].transform.position;
					mACameraToolCollider.transform.position = (position + position2) * 0.5f;
					mACameraToolCollider.transform.localScale = new Vector3(4f, 4f, 1f);
					mACameraToolCollider.transform.localRotation = Quaternion.LookRotation(position2 - position, Vector3.up);
					break;
				}
				}
			}
		}

		public void RefreshLineGizmo()
		{
			m_lock_gizmo_refresh = false;
			lineRenderer.Refresh();
		}

		public void RefreshLineGizmo(float p_delay)
		{
			if (!m_lock_gizmo_refresh)
			{
				m_lock_gizmo_refresh = true;
				Invoke("RefreshLineGizmo", p_delay);
			}
		}

		protected Transform FetchValidCollider()
		{
			int num = (this ? base.transform.childCount : 0);
			Transform transform = null;
			for (int i = 0; i < num; i++)
			{
				Transform child = base.transform.GetChild(i);
				if (!(child.name != "collider"))
				{
					if (!transform)
					{
						transform = child;
					}
					Vector3 vector = child.transform.localPosition;
					Vector3 vector2 = child.transform.localScale;
					if (!(vector.magnitude <= 0.05f) && !(Mathf.Abs(vector2.magnitude - 1f) <= 0.02f))
					{
						transform = child;
					}
				}
			}
			return transform;
		}

		public void ClearInvalidColliders()
		{
			Transform transform = FetchValidCollider();
			int num = (base.transform ? base.transform.childCount : 0);
			for (int i = 0; i < num; i++)
			{
				Transform child = base.transform.GetChild(i);
				if (!(child.name != "collider") && !(child == transform))
				{
					Object.Destroy(child.gameObject);
				}
			}
		}

		public void SetColliderActiveAsync(bool p_flag)
		{
			if (!collider)
			{
				return;
			}
			if (m_collider_active_t != null)
			{
				m_collider_active_t.Stop();
			}
			m_collider_active_t = Activity.RunOnce(delegate
			{
				if ((bool)collider && (bool)collider.gameObject)
				{
					collider.gameObject.SetActive(p_flag);
				}
			}, 0.05f);
		}

		public void SetColliderVisible(bool p_flag)
		{
		}

		protected void Start()
		{
			RefreshHierarchy();
			DelayRefresh();
		}

		public override void Write()
		{
			base.Write();
			MDCameraTool mDCameraTool = data;
			if (mDCameraTool != null)
			{
				mDCameraTool.index = index;
				mDCameraTool.easingMode = m_easing_mode;
			}
		}

		public override void Read()
		{
			if (m_data is MDCameraTool mDCameraTool)
			{
				m_easing_mode = mDCameraTool.easingMode;
				m_index = mDCameraTool.index;
			}
			base.Read();
		}

		protected override MDObject NewData()
		{
			return new MDCameraTool();
		}

		public int RefreshHierarchy()
		{
			List<MACameraToolControlPoint> controlPoints = GetControlPoints();
			for (int i = 0; i < controlPoints.Count; i++)
			{
				controlPoints[i].transform.SetSiblingIndex(i);
				switch (i)
				{
				case 0:
					controlPoints[i].name = "from";
					lineRenderer.from = controlPoints[i].transform;
					break;
				case 1:
					controlPoints[i].name = "to";
					lineRenderer.to = controlPoints[i].transform;
					break;
				}
			}
			return controlPoints.Count;
		}

		protected override void OnRefresh()
		{
			RefreshIfChange(p_force: true);
		}

		protected override void Awake()
		{
			base.Awake();
			Invoke("RefreshLineGizmo", 1f / 30f);
		}

		public void SetIngame(bool p_flag)
		{
			if ((bool)collider)
			{
				collider.gameObject.SetActive(p_flag);
				collider.boxRenderer.enabled = !p_flag;
				collider.boxRendererScaler.enabled = !p_flag;
			}
			lineRenderer.enabled = !p_flag;
			lineRenderer.renderer.enabled = !p_flag;
			List<MACameraToolControlPoint> controlPoints = GetControlPoints();
			for (int i = 0; i < controlPoints.Count; i++)
			{
				controlPoints[i].gameObject.SetActive(!p_flag);
			}
			base.gameObject.SetActive(value: true);
		}

		public float GetDistance(Vector3 p_position)
		{
			return Vector3.Distance(collider.collider.transform.position, p_position);
		}

		public float GetDistance(Transform p_target)
		{
			if (!p_target)
			{
				return 999999f;
			}
			return Vector3.Distance(collider.transform.position, p_target.position);
		}

		public CameraToolTrackingMode GetControlPointTrackingMode(int p_index)
		{
			List<MACameraToolControlPoint> controlPoints = GetControlPoints(p_cache: true);
			int num = ((p_index >= 0) ? ((p_index >= controlPoints.Count) ? (controlPoints.Count - 1) : p_index) : 0);
			MACameraToolControlPoint mACameraToolControlPoint = controlPoints[num];
			if (!mACameraToolControlPoint)
			{
				return CameraToolTrackingMode.None;
			}
			return mACameraToolControlPoint.trackingMode;
		}

		public float GetProjectionRatio(Vector3 p_point, float p_offset_z = 0f)
		{
			List<MACameraToolControlPoint> controlPoints = GetControlPoints(p_cache: true);
			MACameraToolControlPoint mACameraToolControlPoint = ((controlPoints.Count >= 1) ? controlPoints[0] : null);
			Vector3 rhs = ((controlPoints.Count >= 2) ? controlPoints[1] : mACameraToolControlPoint).transform.position - mACameraToolControlPoint.transform.position;
			float magnitude = rhs.magnitude;
			if (magnitude <= 0.001f)
			{
				return 0f;
			}
			rhs.Normalize();
			return (Vector3.Dot(p_point - mACameraToolControlPoint.transform.position, rhs) - p_offset_z) / magnitude;
		}

		public Vector3 GetControlPointPositionLerp(float p_ratio)
		{
			List<MACameraToolControlPoint> controlPoints = GetControlPoints(p_cache: true);
			MACameraToolControlPoint mACameraToolControlPoint = ((controlPoints.Count >= 1) ? controlPoints[0] : null);
			MACameraToolControlPoint obj = ((controlPoints.Count >= 2) ? controlPoints[1] : mACameraToolControlPoint);
			Vector3 position = mACameraToolControlPoint.transform.position;
			Vector3 position2 = obj.transform.position;
			Vector3 controlPointOffsetLerp = GetControlPointOffsetLerp(p_ratio);
			Vector3 normalized = (position2 - position).normalized;
			Vector3 up = Vector3.up;
			Vector3 vector = Vector3.Cross(up, normalized);
			up = Vector3.Cross(normalized, vector);
			return Vector3.LerpUnclamped(position, position2, p_ratio) + vector * controlPointOffsetLerp.x + up * controlPointOffsetLerp.y;
		}

		public Quaternion GetControlPointRotationLerp(float p_ratio)
		{
			List<MACameraToolControlPoint> controlPoints = GetControlPoints(p_cache: true);
			MACameraToolControlPoint mACameraToolControlPoint = ((controlPoints.Count >= 1) ? controlPoints[0] : null);
			MACameraToolControlPoint obj = ((controlPoints.Count >= 2) ? controlPoints[1] : mACameraToolControlPoint);
			Quaternion a = mACameraToolControlPoint.transform.localRotation;
			Quaternion b = obj.transform.localRotation;
			return Quaternion.SlerpUnclamped(a, b, p_ratio);
		}

		public Vector2 GetControlPointOrbitAngleLerp(float p_ratio)
		{
			List<MACameraToolControlPoint> controlPoints = GetControlPoints(p_cache: true);
			MACameraToolControlPoint mACameraToolControlPoint = ((controlPoints.Count >= 1) ? controlPoints[0] : null);
			MACameraToolControlPoint obj = ((controlPoints.Count >= 2) ? controlPoints[1] : mACameraToolControlPoint);
			Vector2 cameraOrbitAngle = mACameraToolControlPoint.cameraOrbitAngle;
			Vector2 cameraOrbitAngle2 = obj.cameraOrbitAngle;
			return Vector2.LerpUnclamped(cameraOrbitAngle, cameraOrbitAngle2, p_ratio);
		}

		public float GetCameraDistanceLerp(float p_ratio)
		{
			List<MACameraToolControlPoint> controlPoints = GetControlPoints(p_cache: true);
			MACameraToolControlPoint mACameraToolControlPoint = ((controlPoints.Count >= 1) ? controlPoints[0] : null);
			MACameraToolControlPoint mACameraToolControlPoint2 = ((controlPoints.Count >= 2) ? controlPoints[1] : mACameraToolControlPoint);
			return Mathf.LerpUnclamped(mACameraToolControlPoint.cameraDistance, mACameraToolControlPoint2.cameraDistance, p_ratio);
		}

		public Vector3 GetControlPointOffsetLerp(float p_ratio)
		{
			List<MACameraToolControlPoint> controlPoints = GetControlPoints(p_cache: true);
			MACameraToolControlPoint mACameraToolControlPoint = ((controlPoints.Count >= 1) ? controlPoints[0] : null);
			MACameraToolControlPoint obj = ((controlPoints.Count >= 2) ? controlPoints[1] : mACameraToolControlPoint);
			Vector3 cameraOffset = mACameraToolControlPoint.cameraOffset;
			Vector3 cameraOffset2 = obj.cameraOffset;
			return Vector3.LerpUnclamped(cameraOffset, cameraOffset2, p_ratio);
		}

		public float GetControlPointTrackingDelayLerp(float p_ratio)
		{
			List<MACameraToolControlPoint> controlPoints = GetControlPoints(p_cache: true);
			MACameraToolControlPoint mACameraToolControlPoint = ((controlPoints.Count >= 1) ? controlPoints[0] : null);
			MACameraToolControlPoint mACameraToolControlPoint2 = ((controlPoints.Count >= 2) ? controlPoints[1] : mACameraToolControlPoint);
			return Mathf.LerpUnclamped(mACameraToolControlPoint.trackingDelay, mACameraToolControlPoint2.trackingDelay, p_ratio);
		}

		public float GetControlPointFOVLerp(float p_ratio)
		{
			List<MACameraToolControlPoint> controlPoints = GetControlPoints(p_cache: true);
			MACameraToolControlPoint mACameraToolControlPoint = ((controlPoints.Count >= 1) ? controlPoints[0] : null);
			MACameraToolControlPoint mACameraToolControlPoint2 = ((controlPoints.Count >= 2) ? controlPoints[1] : mACameraToolControlPoint);
			return Mathf.LerpUnclamped(mACameraToolControlPoint.fov, mACameraToolControlPoint2.fov, p_ratio);
		}

		public TransformVector GetControlPointSampleLerp(Component p_target, Vector3 p_position, float p_ratio)
		{
			List<MACameraToolControlPoint> controlPoints = GetControlPoints(p_cache: true);
			MACameraToolControlPoint mACameraToolControlPoint = ((controlPoints.Count >= 1) ? controlPoints[0] : null);
			MACameraToolControlPoint mACameraToolControlPoint2 = ((controlPoints.Count >= 2) ? controlPoints[1] : mACameraToolControlPoint);
			bool flag = mode == CameraToolMode.Wire;
			Transform pivot = p_target.transform;
			bool flag2 = mACameraToolControlPoint.trackingMode == CameraToolTrackingMode.Orbit && mACameraToolControlPoint2.trackingMode == CameraToolTrackingMode.Orbit;
			if (!pivot)
			{
				flag2 = false;
			}
			if (flag2)
			{
				Vector2 controlPointOrbitAngleLerp = GetControlPointOrbitAngleLerp(p_ratio);
				float cameraDistanceLerp = GetCameraDistanceLerp(p_ratio);
				Quaternion controlPointRotationLerp = GetControlPointRotationLerp(p_ratio);
				return GetOrbitTrackingSample(pivot.position, controlPointRotationLerp, controlPointOrbitAngleLerp, cameraDistanceLerp);
			}
			Vector3 controlPointPositionLerp = GetControlPointPositionLerp(p_ratio);
			TransformVector transformVector = default(TransformVector);
			TransformVector b = default(TransformVector);
			switch ((!pivot) ? CameraToolTrackingMode.Static : mACameraToolControlPoint.trackingMode)
			{
			case CameraToolTrackingMode.FPV:
			{
				Drone drone = p_target as Drone;
				if ((bool)drone)
				{
					pivot = drone.body.frame.camera.pivot;
				}
				transformVector = GetFPVTrackingSample(pivot.position, pivot.rotation, pivot.forward);
				break;
			}
			case CameraToolTrackingMode.LookAt:
				transformVector = GetLookAtTrackingSample(controlPointPositionLerp, p_position, mACameraToolControlPoint.cameraDistance, Vector3.up);
				break;
			case CameraToolTrackingMode.Static:
				transformVector.Set(mACameraToolControlPoint.transform.position, mACameraToolControlPoint.transform.rotation);
				break;
			case CameraToolTrackingMode.Orbit:
				transformVector = GetOrbitTrackingSample(pivot.position, mACameraToolControlPoint.localRotation, mACameraToolControlPoint.cameraOrbitAngle, mACameraToolControlPoint.cameraDistance);
				break;
			}
			CameraToolTrackingMode cameraToolTrackingMode = ((!pivot) ? CameraToolTrackingMode.Static : mACameraToolControlPoint2.trackingMode);
			if (flag)
			{
				switch (cameraToolTrackingMode)
				{
				case CameraToolTrackingMode.FPV:
				{
					Drone drone2 = p_target as Drone;
					if ((bool)drone2)
					{
						pivot = drone2.body.frame.camera.pivot;
					}
					b = GetFPVTrackingSample(pivot.position, pivot.rotation, pivot.forward);
					break;
				}
				case CameraToolTrackingMode.LookAt:
					b = GetLookAtTrackingSample(controlPointPositionLerp, pivot.position, mACameraToolControlPoint2.cameraDistance, Vector3.up);
					break;
				case CameraToolTrackingMode.Static:
					b.Set(mACameraToolControlPoint2.transform.position, mACameraToolControlPoint2.transform.rotation);
					break;
				case CameraToolTrackingMode.Orbit:
					b = GetOrbitTrackingSample(pivot.position, mACameraToolControlPoint2.localRotation, mACameraToolControlPoint2.cameraOrbitAngle, mACameraToolControlPoint2.cameraDistance);
					break;
				}
			}
			if (flag)
			{
				return TransformVector.Lerp(transformVector, b, p_ratio);
			}
			return transformVector;
		}

		public TransformVector GetControlPointSampleLerp(Component p_target, float p_ratio)
		{
			return GetControlPointSampleLerp(p_target, p_target.transform.position, p_ratio);
		}

		public TransformVector GetLookAtTrackingSample(Vector3 p_position, Vector3 p_target, float p_distance, Vector3 p_up)
		{
			TransformVector result = default(TransformVector);
			Vector3 vector = p_target - p_position;
			vector.Normalize();
			Vector3 position = ((Mathf.Abs(p_distance) > 0.001f) ? (p_target - vector * p_distance) : p_position);
			result.position = position;
			result.rotation = Quaternion.LookRotation(vector, p_up);
			return result;
		}

		public TransformVector GetFPVTrackingSample(Vector3 p_position, Quaternion p_rotation, Vector3 p_forward)
		{
			TransformVector result = default(TransformVector);
			result.position = p_position + p_forward * 0.02f;
			result.rotation = p_rotation;
			return result;
		}

		public TransformVector GetOrbitTrackingSample(Vector3 p_position, Quaternion p_anchor, Vector2 p_angle, float p_distance)
		{
			TransformVector result = default(TransformVector);
			Vector3 up = Vector3.up;
			Vector3 right = Vector3.right;
			Quaternion quaternion = Quaternion.AngleAxis(p_angle.x, up);
			quaternion = p_anchor * quaternion * Quaternion.AngleAxis(p_angle.y, right);
			Vector3 vector = quaternion * Vector3.forward;
			Vector3 p_position2 = p_position + vector * (0f - p_distance);
			result.Set(p_position2, quaternion);
			return result;
		}

		public void Evaluate(Transform p_target, bool p_smooth, out MACameraToolControlPoint.Sample p_cps, out TransformVector p_sample)
		{
			if (m_ctcp_cache == null)
			{
				m_ctcp_cache = GetControlPoints();
			}
			MACameraToolControlPoint obj = ((m_ctcp_cache.Count >= 1) ? m_ctcp_cache[0] : null);
			MACameraToolControlPoint mACameraToolControlPoint = ((m_ctcp_cache.Count >= 2) ? m_ctcp_cache[1] : null);
			if (!obj && !mACameraToolControlPoint)
			{
				p_cps = default(MACameraToolControlPoint.Sample);
				p_sample = this.sample;
			}
			MACameraToolControlPoint.Sample sample = obj.GetSample();
			MACameraToolControlPoint.Sample sample2 = sample;
			Vector3 p_pivot = sample.position;
			CameraToolMode cameraToolMode = mode;
			if (cameraToolMode != CameraToolMode.Single && cameraToolMode == CameraToolMode.Wire)
			{
				MACameraToolControlPoint.Sample v = mACameraToolControlPoint.GetSample();
				float targetRatio = MACameraToolControlPoint.Sample.GetTargetRatio(sample, v, p_target.position);
				Vector3 vector = Vector3.Lerp(sample.cameraOffset, v.cameraOffset, targetRatio);
				targetRatio = MACameraToolControlPoint.Sample.GetTargetRatio(sample, v, p_target.position, vector.z);
				sample2 = MACameraToolControlPoint.Sample.Lerp(sample, v, Mathf.Clamp01(targetRatio));
				p_pivot = MACameraToolControlPoint.Sample.GetPositionOffset(sample, v, Mathf.Clamp01(targetRatio));
			}
			TransformVector b = MACameraToolControlPoint.Sample.Evaluate(sample2, p_target, p_pivot, this.sample);
			_ = sample2.trackingDelay;
			_ = 0f;
			_ = sample2.trackingMode;
			_ = 4;
			if (p_smooth)
			{
				this.sample = TransformVector.Lerp(this.sample, b, sample2.trackingDelay);
			}
			else
			{
				this.sample = b;
			}
			p_sample = this.sample;
			p_cps = sample2;
		}

		protected void LateUpdate()
		{
			if (base.enabled && m_tdirty)
			{
				RefreshLineGizmo();
				m_tdirty = false;
			}
		}

		protected void Update()
		{
			if (base.enabled)
			{
				RefreshIfChange();
			}
		}

		protected void OnTransformChildrenChanged()
		{
			if (base.enabled)
			{
				RefreshIfChange(p_force: true);
			}
		}

		private void RefreshIfChange(bool p_force = false)
		{
			if (!this || !base.gameObject)
			{
				return;
			}
			m_tcache = (m_tcache ? m_tcache : (m_tcache = base.transform));
			if (!m_tcache)
			{
				return;
			}
			bool flag = m_tcache.hasChanged;
			if (!flag)
			{
				for (int i = 0; i < m_tcache.childCount; i++)
				{
					Transform child = m_tcache.GetChild(i);
					if ((bool)child && child.hasChanged)
					{
						child.hasChanged = false;
						flag = true;
						break;
					}
				}
			}
			if (p_force || flag)
			{
				m_tcache.hasChanged = false;
				m_tdirty = true;
			}
		}
	}
}
