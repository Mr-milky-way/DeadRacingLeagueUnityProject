using UnityEngine;

namespace drl.game
{
	public class MDCameraToolControlPoint : MDGuide
	{
		public float fov
		{
			get
			{
				return Get("ctcp-fov", 60f);
			}
			set
			{
				Set("ctcp-fov", value);
			}
		}

		public CameraToolTrackingMode trackingMode
		{
			get
			{
				return (CameraToolTrackingMode)Get("ctcp-tracking-mode", 1);
			}
			set
			{
				Set("ctcp-tracking-mode", value);
			}
		}

		public float trackingDelay
		{
			get
			{
				return Get("ctcp-tracking-delay", 0f);
			}
			set
			{
				Set("ctcp-tracking-delay", value);
			}
		}

		public Vector3 cameraOffset
		{
			get
			{
				return GetVector3("ctcp-camera-offset", Vector3.zero);
			}
			set
			{
				SetVector3("ctcp-camera-offset", value);
			}
		}

		public float cameraDistance
		{
			get
			{
				return Get("ctcp-camera-distance", 0f);
			}
			set
			{
				Set("ctcp-camera-distance", value);
			}
		}

		public Vector2 cameraOrbitAngle
		{
			get
			{
				return GetVector2("ctcp-camera-orbit-angle", Vector2.zero);
			}
			set
			{
				SetVector2("ctcp-camera-orbit-angle", value);
			}
		}

		public MDCameraToolControlPoint()
		{
			base.type = MapAssetType.CameraToolControlPoint;
		}
	}
}
