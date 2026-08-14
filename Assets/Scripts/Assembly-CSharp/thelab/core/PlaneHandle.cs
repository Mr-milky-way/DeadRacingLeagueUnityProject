using UnityEngine;

namespace thelab.core
{
	[RequireComponent(typeof(PlaneGizmoRenderer))]
	public class PlaneHandle : GizmoHandle
	{
		public bool styleFill = true;

		public bool styleLine = true;

		public float area = 0.3f;

		public Vector4 modifier = Vector3.one;

		public Transform guide;

		public new BoxCollider collider => base.collider as BoxCollider;

		public new PlaneGizmoRenderer gizmo => base.gizmo as PlaneGizmoRenderer;

		public override Ray ray
		{
			get
			{
				if (!base.transform.hasChanged)
				{
					return m_ray;
				}
				return m_ray = new Ray(base.transform.position, base.transform.up);
			}
		}

		protected override Collider AssertCollider()
		{
			return base.gameObject.AddComponent<BoxCollider>();
		}

		protected override void UpdateCollider()
		{
			BoxCollider boxCollider = collider;
			Vector3 center = boxCollider.center;
			Vector3 size = boxCollider.size;
			size.y = area;
			size.x = gizmo.size.x;
			size.z = gizmo.size.y;
			size.Scale(modifier);
			center.z = size.z * 0.5f + modifier.w;
			center.x = size.x * 0.5f + modifier.w;
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
				Color outlineColor = p_color;
				outlineColor.a = gizmo.outlineColor.a;
				gizmo.outlineColor = outlineColor;
			}
			if (styleFill)
			{
				Color fillColor = p_color;
				fillColor.a = gizmo.fillColor.a;
				gizmo.fillColor = fillColor;
			}
		}

		public Vector3 GetMouseProjection(Camera p_camera, Ray p_ray)
		{
			return ProjectScreenPointToPlane(p_camera, Input.mousePosition, p_ray);
		}

		public Vector3 GetMouseProjection(Camera p_camera)
		{
			return ProjectScreenPointToPlane(p_camera, Input.mousePosition, ray);
		}

		protected void OnWillRenderObject()
		{
			if ((bool)guide)
			{
				Vector3 mouseProjection = GetMouseProjection(base.camera);
				guide.transform.position = mouseProjection;
			}
		}
	}
}
