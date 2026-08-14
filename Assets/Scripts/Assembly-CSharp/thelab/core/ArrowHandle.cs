using UnityEngine;

namespace thelab.core
{
	[RequireComponent(typeof(ArrowGizmoRenderer))]
	public class ArrowHandle : GizmoHandle
	{
		public bool styleCap = true;

		public bool styleLine = true;

		public float area = 0.3f;

		public Vector4 modifier = Vector3.one;

		public new BoxCollider collider => base.collider as BoxCollider;

		public new ArrowGizmoRenderer gizmo => base.gizmo as ArrowGizmoRenderer;

		protected override Collider AssertCollider()
		{
			return base.gameObject.AddComponent<BoxCollider>();
		}

		protected override void UpdateCollider()
		{
			BoxCollider boxCollider = collider;
			Vector3 center = boxCollider.center;
			Vector3 size = boxCollider.size;
			size.x = (size.y = area);
			size.z = gizmo.size;
			size.Scale(modifier);
			center.z = size.z * 0.5f + modifier.w;
			boxCollider.center = center;
			boxCollider.size = size;
		}

		protected override void OnEvent(HandleEventType p_type)
		{
			if (!gizmo)
			{
				return;
			}
			Color white = Color.white;
			switch (p_type)
			{
			case HandleEventType.Enabled:
				white = (base.active ? style.active : style.normal);
				SetGizmoColor(white);
				break;
			case HandleEventType.Disabled:
				white = style.disabled;
				SetGizmoColor(white);
				break;
			}
			if (!base.enabled)
			{
				return;
			}
			if (base.moving)
			{
				white = style.active;
				SetGizmoColor(white);
				return;
			}
			switch (p_type)
			{
			case HandleEventType.Enter:
				white = (base.active ? style.active : style.over);
				SetGizmoColor(white);
				break;
			case HandleEventType.KeyDown:
				white = (base.active ? style.active : style.down);
				SetGizmoColor(white);
				break;
			case HandleEventType.Down:
				white = (base.active ? style.active : style.down);
				SetGizmoColor(white);
				break;
			case HandleEventType.Exit:
				white = (base.active ? style.active : style.normal);
				SetGizmoColor(white);
				break;
			case HandleEventType.KeyUp:
				white = (base.active ? style.active : style.normal);
				SetGizmoColor(white);
				break;
			case HandleEventType.Up:
			case HandleEventType.DragEnd:
				white = (base.active ? style.active : (over ? style.over : style.normal));
				SetGizmoColor(white);
				break;
			}
			switch (p_type)
			{
			case HandleEventType.Down:
				base.mouse.ray = (base.mouse.rayDown = ray);
				break;
			case HandleEventType.Up:
				base.mouse.ray = (base.mouse.rayUp = ray);
				break;
			case HandleEventType.Stay:
				base.mouse.ray = ray;
				break;
			case HandleEventType.Enter:
				base.mouse.ray = ray;
				break;
			case HandleEventType.DragEnd:
				base.mouse.ray = (base.mouse.rayUp = ray);
				break;
			case HandleEventType.Exit:
			case HandleEventType.DragStart:
			case HandleEventType.Drag:
				break;
			}
		}

		protected void SetGizmoColor(Color p_color)
		{
			if (styleLine)
			{
				gizmo.lineColor = p_color;
			}
			if (styleCap)
			{
				gizmo.capColor = p_color;
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
	}
}
