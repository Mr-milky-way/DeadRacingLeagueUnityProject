using UnityEngine;

namespace thelab.core
{
	public class ScreenshotData : MonoBehaviour
	{
		public Vector3 angle;

		public float fov;

		public Vector3 offset;

		public float scale = 1f;

		public float fitScale = 1f;

		public bool autoFit = true;

		public void Set(ScreenshotData v)
		{
			angle = v.angle;
			fov = v.fov;
			offset = v.offset;
			scale = v.scale;
			fitScale = v.fitScale;
			autoFit = v.autoFit;
		}

		public Bounds GetWorldBounds()
		{
			Bounds b = default(Bounds);
			b.center = base.transform.position;
			b.size = Vector3.one;
			Hierarchy.Traverse(base.transform, delegate(Renderer p_it)
			{
				b.Encapsulate(p_it.bounds);
			});
			return b;
		}

		public void SnapToViewCenter(Bounds p_camera_bounds)
		{
			Bounds bounds = p_camera_bounds;
			Bounds worldBounds = GetWorldBounds();
			offset = bounds.center - worldBounds.center;
		}
	}
}
