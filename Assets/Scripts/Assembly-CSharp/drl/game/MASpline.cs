using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class MASpline : MARenderer
	{
		public static bool splineSnapSelectNext;

		[SerializeField]
		private SplineComponent m_spline;

		[SerializeField]
		private SplineRenderer m_spline_renderer;

		[SerializeField]
		internal SplineType m_spline_mode = SplineType.Catmull;

		public List<Material> splineCategoryStyles;

		private Dictionary<Transform, MASplineControlPoint> m_scp_cache;

		private Activity m_refresh_hierarchy_timer;

		private bool m_lock_spline_refresh;

		[SerializeField]
		private bool m_is_race_spline;

		[SerializeField]
		internal SplineCategory m_spline_category;

		[SerializeField]
		private bool m_is_loop;

		[SerializeField]
		private float m_spline_alpha = 1f;

		[SerializeField]
		private float m_spline_start_width = 0.2f;

		[SerializeField]
		private float m_spline_end_width = 0.2f;

		[SerializeField]
		private float m_spline_thickness = 0.2f;

		[SerializeField]
		private float m_spline_course_camera_speed = 4f;

		[SerializeField]
		private float m_spline_course_camera_fov = 60f;

		[SerializeField]
		private int m_spline_course_camera_index;

		public string splineControlPointId = "DMA-e4a3";

		private Transform m_tcache;

		private bool m_tdirty;

		public SplineComponent spline
		{
			get
			{
				if (!base.gameObject || !this)
				{
					return null;
				}
				if (!m_spline)
				{
					return m_spline = GetComponent<SplineComponent>();
				}
				return m_spline;
			}
		}

		public SplineRenderer splineRenderer
		{
			get
			{
				if (!m_spline_renderer)
				{
					if (!this)
					{
						return null;
					}
					return m_spline_renderer = GetComponent<SplineRenderer>();
				}
				return m_spline_renderer;
			}
		}

		public Material splineStyleMaterial
		{
			get
			{
				if (!splineRenderer)
				{
					return null;
				}
				if (!splineRenderer.renderer)
				{
					return null;
				}
				return splineRenderer.renderer.sharedMaterial;
			}
		}

		public SplineType splineMode
		{
			get
			{
				return m_spline_mode;
			}
			set
			{
				m_spline_mode = value;
				Write();
				DelayRefresh();
			}
		}

		public bool isRaceSpline
		{
			get
			{
				return m_is_race_spline;
			}
			set
			{
				m_is_race_spline = value;
				if (value)
				{
					m_spline_category = SplineCategory.RaceLine;
				}
				else if (m_spline_category == SplineCategory.RaceLine)
				{
					m_spline_category = SplineCategory.Visual;
				}
				Write();
				DelayRefresh();
			}
		}

		public SplineCategory splineCategory
		{
			get
			{
				if (m_is_race_spline)
				{
					return SplineCategory.RaceLine;
				}
				return m_spline_category;
			}
			set
			{
				m_spline_category = value;
				m_is_race_spline = value == SplineCategory.RaceLine;
				Write();
				DelayRefresh();
			}
		}

		public bool isLoop
		{
			get
			{
				return m_is_loop;
			}
			set
			{
				m_is_loop = value;
				Write();
				DelayRefresh();
			}
		}

		public float splineAlpha
		{
			get
			{
				return m_spline_alpha;
			}
			set
			{
				m_spline_alpha = value;
				Write();
				DelayRefresh();
			}
		}

		public float splineStartWidth
		{
			get
			{
				return m_spline_start_width;
			}
			set
			{
				m_spline_start_width = value;
				Write();
				DelayRefresh();
			}
		}

		public float splineEndWidth
		{
			get
			{
				return m_spline_end_width;
			}
			set
			{
				m_spline_end_width = value;
				Write();
				DelayRefresh();
			}
		}

		public float splineThickness
		{
			get
			{
				return m_spline_thickness;
			}
			set
			{
				m_spline_thickness = value;
				Write();
				DelayRefresh();
			}
		}

		public float splineCourseCameraSpeed
		{
			get
			{
				return m_spline_course_camera_speed;
			}
			set
			{
				m_spline_course_camera_speed = value;
				Write();
				DelayRefresh();
			}
		}

		public float splineCourseCameraFOV
		{
			get
			{
				return m_spline_course_camera_fov;
			}
			set
			{
				m_spline_course_camera_fov = value;
				Write();
				DelayRefresh();
			}
		}

		public int splineCourseCameraIndex
		{
			get
			{
				return m_spline_course_camera_index;
			}
			set
			{
				m_spline_course_camera_index = value;
				Write();
				DelayRefresh();
			}
		}

		public new MDSpline data
		{
			get
			{
				return base.data as MDSpline;
			}
			set
			{
				base.data = value;
			}
		}

		public List<MASplineControlPoint> GetControlPoints()
		{
			return Hierarchy.FindAll<MASplineControlPoint>(base.transform);
		}

		public MASplineControlPoint GetNextControlPoint(MASplineControlPoint p_node)
		{
			List<MASplineControlPoint> controlPoints = GetControlPoints();
			int num = controlPoints.IndexOf(p_node);
			if (num < 0)
			{
				return null;
			}
			num = (num + 1) % controlPoints.Count;
			return controlPoints[num];
		}

		public void RefreshSpline()
		{
			m_lock_spline_refresh = false;
			spline.Refresh();
			splineRenderer.Refresh();
		}

		public void RefreshHierarchy()
		{
			if (m_scp_cache == null)
			{
				m_scp_cache = new Dictionary<Transform, MASplineControlPoint>();
			}
			int childCount = base.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				MASplineControlPoint mASplineControlPoint2;
				if (!m_scp_cache.ContainsKey(child))
				{
					MASplineControlPoint mASplineControlPoint = (m_scp_cache[child] = child.GetComponent<MASplineControlPoint>());
					mASplineControlPoint2 = mASplineControlPoint;
				}
				else
				{
					mASplineControlPoint2 = m_scp_cache[child];
				}
				MASplineControlPoint mASplineControlPoint3 = mASplineControlPoint2;
				if ((bool)mASplineControlPoint3)
				{
					int siblingIndex = child.GetSiblingIndex();
					mASplineControlPoint3.index = siblingIndex;
				}
			}
		}

		public void AssertSiblingIndexes()
		{
			_ = base.transform.childCount;
			List<MASplineControlPoint> controlPoints = GetControlPoints();
			controlPoints.Sort((MASplineControlPoint a, MASplineControlPoint b) => (a.index >= b.index) ? 1 : (-1));
			for (int num = 0; num < controlPoints.Count; num++)
			{
				MASplineControlPoint mASplineControlPoint = controlPoints[num];
				mASplineControlPoint.transform.SetSiblingIndex(num);
				mASplineControlPoint.index = num;
			}
		}

		public void DelayedRefreshHierarchy()
		{
			if (m_refresh_hierarchy_timer != null)
			{
				m_refresh_hierarchy_timer.Stop();
			}
			m_refresh_hierarchy_timer = this.TimerRunOnce(RefreshHierarchy, 1f / 60f);
		}

		public void SetControlPointIndex(MASplineControlPoint p_target, int p_index, bool p_refresh = true)
		{
			Transform transform = base.transform;
			if (!p_target.transform.IsChildOf(transform))
			{
				return;
			}
			int childCount = base.transform.childCount;
			int siblingIndex = Mathf.Clamp(p_index, 0, childCount);
			p_target.transform.SetSiblingIndex(siblingIndex);
			if (!p_refresh)
			{
				return;
			}
			for (int i = 0; i < childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				MASplineControlPoint mASplineControlPoint2;
				if (!m_scp_cache.ContainsKey(child))
				{
					MASplineControlPoint mASplineControlPoint = (m_scp_cache[child] = child.GetComponent<MASplineControlPoint>());
					mASplineControlPoint2 = mASplineControlPoint;
				}
				else
				{
					mASplineControlPoint2 = m_scp_cache[child];
				}
				MASplineControlPoint mASplineControlPoint3 = mASplineControlPoint2;
				if ((bool)mASplineControlPoint3)
				{
					siblingIndex = mASplineControlPoint3.transform.GetSiblingIndex();
					mASplineControlPoint3.index = siblingIndex;
				}
			}
		}

		public void RefreshSpline(float p_delay)
		{
			if (!m_lock_spline_refresh)
			{
				m_lock_spline_refresh = true;
				Invoke("RefreshSpline", p_delay);
			}
		}

		public void OrientControlPoints()
		{
			int childCount = base.transform.childCount;
			if (childCount > 1)
			{
				Transform child;
				Transform child2;
				for (int i = 1; i < childCount; i++)
				{
					child = base.transform.GetChild(i - 1);
					child2 = base.transform.GetChild(i);
					child.LookAt(child2, Vector3.up);
				}
				child = base.transform.GetChild(childCount - 2);
				child2 = base.transform.GetChild(childCount - 1);
				child2.localRotation = child.localRotation;
			}
		}

		public override void Write()
		{
			base.Write();
			MDSpline mDSpline = data;
			if (mDSpline != null)
			{
				mDSpline.mode = m_spline_mode;
				mDSpline.isRaceSpline = m_is_race_spline;
				mDSpline.isLoop = m_is_loop;
				mDSpline.alpha = m_spline_alpha;
				mDSpline.startWidth = m_spline_start_width;
				mDSpline.endWidth = m_spline_end_width;
				mDSpline.thickness = m_spline_thickness;
				mDSpline.splineCategory = (m_is_race_spline ? SplineCategory.RaceLine : m_spline_category);
				if (m_spline_category == SplineCategory.CourseCamera)
				{
					mDSpline.courseCameraSpeed = m_spline_course_camera_speed;
					mDSpline.courseCameraFOV = m_spline_course_camera_fov;
					mDSpline.courseCameraIndex = m_spline_course_camera_index;
				}
			}
		}

		public override void Read()
		{
			if (m_data is MDSpline mDSpline)
			{
				m_spline_mode = mDSpline.mode;
				m_is_race_spline = mDSpline.isRaceSpline;
				m_is_loop = mDSpline.isLoop;
				m_spline_alpha = mDSpline.alpha;
				m_spline_start_width = mDSpline.startWidth;
				m_spline_end_width = mDSpline.endWidth;
				m_spline_thickness = mDSpline.thickness;
				m_spline_category = (m_is_race_spline ? SplineCategory.RaceLine : mDSpline.splineCategory);
				m_spline_course_camera_speed = mDSpline.courseCameraSpeed;
				m_spline_course_camera_fov = mDSpline.courseCameraFOV;
				m_spline_course_camera_index = mDSpline.courseCameraIndex;
			}
			base.Read();
		}

		protected override MDObject NewData()
		{
			return new MDSpline();
		}

		protected override void OnRefresh()
		{
			Color color = base.color0;
			Color color2 = base.color1;
			Color white = base.color2;
			Color cpc = Color.white;
			Material material = splineCategoryStyles[(int)splineCategory];
			Shader shader = material.shader;
			float p_start = m_spline_start_width;
			float p_end = m_spline_end_width;
			float p_thickness = m_spline_thickness;
			bool loop = m_is_loop;
			float alpha = splineAlpha;
			switch (splineCategory)
			{
			case SplineCategory.RaceLine:
				cpc = Color.red;
				color = (color2 = (white = Color.white));
				p_start = (p_end = (p_thickness = 0.2f));
				loop = false;
				alpha = 1f;
				break;
			case SplineCategory.CourseCamera:
				cpc = DRLColor.yellowDark;
				color = (color2 = (white = Color.white));
				p_start = (p_end = (p_thickness = 0.2f));
				loop = false;
				alpha = 1f;
				break;
			}
			Material material2 = splineStyleMaterial;
			if ((bool)material2)
			{
				material2.shader = shader;
				material2.CopyPropertiesFromMaterial(material);
			}
			Color[] p_colors = new Color[5] { color, color, color2, white, white };
			SplineRenderer splineRenderer = this.splineRenderer;
			if ((bool)splineRenderer)
			{
				splineRenderer.SetGradientColors(0, p_colors);
				splineRenderer.alpha = alpha;
				if ((bool)splineRenderer.renderer)
				{
					splineRenderer.renderer.loop = loop;
				}
				splineRenderer.SetWidth(p_start, p_end, p_thickness);
			}
			if ((bool)spline && spline.type != m_spline_mode)
			{
				spline.SetType(m_spline_mode);
			}
			Hierarchy.Traverse(base.transform, delegate(MASplineControlPoint it)
			{
				if ((bool)it)
				{
					it.SetColor(cpc);
					it.SetAssetActive("axis", splineCategory == SplineCategory.CourseCamera);
				}
			});
			RefreshIfChange(p_force: true);
		}

		protected override void ApplyColors(Material p_material)
		{
		}

		protected new void Awake()
		{
			Invoke("RefreshSpline", 1f / 30f);
		}

		protected void LateUpdate()
		{
			if (base.enabled && m_tdirty)
			{
				RefreshSpline();
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
					if (child.hasChanged)
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
