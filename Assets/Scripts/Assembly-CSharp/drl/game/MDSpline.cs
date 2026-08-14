using thelab.core;

namespace drl.game
{
	public class MDSpline : MDRenderer
	{
		public SplineType mode
		{
			get
			{
				return (SplineType)Get("spline-mode", 1);
			}
			set
			{
				Set("spline-mode", (int)value);
			}
		}

		public SplineCategory splineCategory
		{
			get
			{
				return (SplineCategory)Get("spline-category", 0);
			}
			set
			{
				Set("spline-category", (int)value);
			}
		}

		public bool isRaceSpline
		{
			get
			{
				return Get("is-race-spline", d: false);
			}
			set
			{
				Set("is-race-spline", value);
			}
		}

		public bool isLoop
		{
			get
			{
				return Get("is-spline-loop", d: false);
			}
			set
			{
				Set("is-spline-loop", value);
			}
		}

		public float alpha
		{
			get
			{
				return Get("spline-alpha", 1f);
			}
			set
			{
				Set("spline-alpha", value);
			}
		}

		public float startWidth
		{
			get
			{
				return Get("spline-start-width", 0.2f);
			}
			set
			{
				Set("spline-start-width", value);
			}
		}

		public float thickness
		{
			get
			{
				return Get("spline-thickness", 0.2f);
			}
			set
			{
				Set("spline-thickness", value);
			}
		}

		public float endWidth
		{
			get
			{
				return Get("spline-end-width", 0.2f);
			}
			set
			{
				Set("spline-end-width", value);
			}
		}

		public float courseCameraSpeed
		{
			get
			{
				return Get("spline-course-camera-speed", 4f);
			}
			set
			{
				Set("spline-course-camera-speed", value);
			}
		}

		public float courseCameraFOV
		{
			get
			{
				return Get("spline-course-camera-fov", 60f);
			}
			set
			{
				Set("spline-course-camera-fov", value);
			}
		}

		public int courseCameraIndex
		{
			get
			{
				return Get("spline-course-camera-index", 0);
			}
			set
			{
				Set("spline-course-camera-index", value);
			}
		}

		public MDSpline()
		{
			base.type = MapAssetType.Spline;
		}
	}
}
