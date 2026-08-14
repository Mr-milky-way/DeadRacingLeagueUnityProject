using UnityEngine;

namespace thelab.core
{
	[RequireComponent(typeof(RadialGizmoRenderer))]
	public class RadialHandle : GizmoHandle
	{
		public float area = 0.1f;

		public float scale = 1f;

		public Quaternion rotationPivot;

		public Quaternion rotationAnchor;

		public Vector3 YAxisDown;

		public Vector3 TopAxisDown;

		public new SphereCollider collider => base.collider as SphereCollider;

		public new RadialGizmoRenderer gizmo => base.gizmo as RadialGizmoRenderer;

		protected override void Awake()
		{
			base.Awake();
			rotationAnchor = base.transform.localRotation;
		}

		public void LockRotationAtPoint(Vector3 p_point)
		{
			Vector3 position = p_point;
			position = base.transform.InverseTransformPoint(position);
			position.y = 0f;
			LockRotationAtLocalPoint(position);
		}

		public void LockRotationAtLocalPoint(Vector3 p_point)
		{
			Vector3 position = p_point;
			position.y = 0f;
			position = base.transform.TransformPoint(position);
			base.transform.LookAt(position, base.transform.up);
			rotationPivot = base.transform.rotation;
		}

		protected override Collider AssertCollider()
		{
			return base.gameObject.AddComponent<SphereCollider>();
		}

		protected override void UpdateCollider()
		{
			SphereCollider sphereCollider = collider;
			_ = sphereCollider.center;
			float radius = sphereCollider.radius;
			radius = gizmo.radius;
			sphereCollider.radius = radius * scale;
		}

		protected override void OnEvent(HandleEventType p_type)
		{
			if (!gizmo)
			{
				return;
			}
			switch (p_type)
			{
			case HandleEventType.Enabled:
				gizmo.color = (base.active ? style.active : style.normal);
				break;
			case HandleEventType.Disabled:
				gizmo.color = style.disabled;
				break;
			}
			if (!base.enabled)
			{
				return;
			}
			if (base.moving)
			{
				gizmo.color = style.active;
				return;
			}
			switch (p_type)
			{
			case HandleEventType.Enter:
				gizmo.color = (base.active ? style.active : style.over);
				break;
			case HandleEventType.KeyDown:
				gizmo.color = (base.active ? style.active : style.down);
				break;
			case HandleEventType.Down:
				gizmo.color = (base.active ? style.active : style.down);
				break;
			case HandleEventType.KeyUp:
				gizmo.color = (base.active ? style.active : style.normal);
				break;
			case HandleEventType.Exit:
				gizmo.color = (base.active ? style.active : style.normal);
				break;
			case HandleEventType.Up:
			case HandleEventType.DragEnd:
				gizmo.color = (base.active ? style.active : (over ? style.over : style.normal));
				break;
			}
			switch (p_type)
			{
			case HandleEventType.Down:
				base.mouse.ray = (base.mouse.rayDown = new Ray(base.transform.position, base.transform.forward));
				break;
			case HandleEventType.Up:
				base.mouse.ray = (base.mouse.rayUp = new Ray(base.transform.position, base.transform.forward));
				break;
			case HandleEventType.Stay:
				base.mouse.ray = new Ray(base.transform.position, base.transform.forward);
				break;
			case HandleEventType.Enter:
				base.mouse.ray = new Ray(base.transform.position, base.transform.forward);
				break;
			case HandleEventType.DragEnd:
				base.mouse.ray = (base.mouse.rayUp = new Ray(base.transform.position, base.transform.forward));
				break;
			case HandleEventType.Exit:
			case HandleEventType.DragStart:
			case HandleEventType.Drag:
				break;
			}
		}

		public Vector3 GetMouseProjection(Camera p_camera, Ray p_ray)
		{
			return ProjectScreenPointToRay(p_camera, Input.mousePosition, p_ray);
		}

		public Vector3 GetMouseProjection(Camera p_camera)
		{
			return ProjectScreenPointToRay(p_camera, Input.mousePosition, ray);
		}

		protected override bool IsColliderMouseOver()
		{
			if (!base.IsColliderMouseOver())
			{
				return false;
			}
			Camera camera = base.camera;
			RaycastHit hit = base.mouse.hit;
			Vector3 point = hit.point;
			Vector3 vector = base.transform.InverseTransformPoint(point);
			float num = area;
			float f = Vector3.Dot(base.transform.up, camera.transform.forward);
			float x = Mathf.Abs(vector.x) / collider.radius;
			float f2 = Mathf.Abs(vector.y) / collider.radius;
			float y = Mathf.Abs(vector.z) / collider.radius;
			if (Mathf.Abs(f2) <= num)
			{
				return true;
			}
			Vector2 vector2 = new Vector2(x, y);
			if (Mathf.Abs(f) < 0.9f)
			{
				return false;
			}
			if (vector2.magnitude >= 1f - num)
			{
				return true;
			}
			return false;
		}
	}
}
