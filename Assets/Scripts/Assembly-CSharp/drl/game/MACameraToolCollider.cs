using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class MACameraToolCollider : MAGuide
	{
		[SerializeField]
		private LineRenderer m_box_renderer;

		[SerializeField]
		private LineRendererScaler m_box_renderer_scaler;

		[SerializeField]
		private MACameraTool m_tool;

		[SerializeField]
		private BoxCollider m_collider;

		public Vector3 size => base.transform.localScale;

		public LineRenderer boxRenderer => m_box_renderer;

		public LineRendererScaler boxRendererScaler => m_box_renderer_scaler;

		public Color color
		{
			get
			{
				Material material = (m_box_renderer ? m_box_renderer.sharedMaterial : null);
				if (!material)
				{
					return Color.black;
				}
				return material.GetColor("_Color");
			}
			set
			{
				Material material = (m_box_renderer ? m_box_renderer.sharedMaterial : null);
				if ((bool)material)
				{
					material.SetColor("_Color", value);
					m_box_renderer.sharedMaterial = material;
				}
			}
		}

		public float alpha
		{
			get
			{
				return color.a;
			}
			set
			{
				Color color = this.color;
				color.a = value;
				this.color = color;
			}
		}

		public MACameraTool tool
		{
			get
			{
				if (!m_tool)
				{
					return m_tool = Hierarchy.FindReverse<MACameraTool>(base.transform);
				}
				return m_tool;
			}
		}

		public BoxCollider collider
		{
			get
			{
				if (!m_collider)
				{
					return m_collider = GetComponent<BoxCollider>();
				}
				return m_collider;
			}
		}

		public new MDGuide data
		{
			get
			{
				return base.data;
			}
			set
			{
				base.data = value;
			}
		}

		public void Hilight(bool p_flag)
		{
			alpha = (p_flag ? 1f : 0.5f);
			boxRendererScaler.width = (p_flag ? 4f : 2f);
		}

		public override void Write()
		{
			base.Write();
			_ = data;
		}

		public override void Read()
		{
			_ = m_data is MDGuide;
			base.Read();
		}

		protected override MDObject NewData()
		{
			return new MDGuide();
		}

		public bool IsInside(Vector3 p_world_position)
		{
			Vector3 vector = base.transform.InverseTransformPoint(p_world_position);
			if (vector.x < -0.5f)
			{
				return false;
			}
			if (vector.x > 0.5f)
			{
				return false;
			}
			if (vector.y < -0.5f)
			{
				return false;
			}
			if (vector.y > 0.5f)
			{
				return false;
			}
			if (vector.z < -0.5f)
			{
				return false;
			}
			if (vector.z > 0.5f)
			{
				return false;
			}
			return true;
		}

		public bool Raycast(Ray p_ray, float p_max_distance)
		{
			if (!collider)
			{
				return false;
			}
			BoxCollider boxCollider = collider;
			if (p_ray.direction.sqrMagnitude < 0.001f)
			{
				Vector3 origin = p_ray.origin;
				return IsInside(origin);
			}
			RaycastHit hitInfo;
			return boxCollider.Raycast(p_ray, out hitInfo, p_max_distance);
		}

		public bool Raycast(Vector3 p_p0, Vector3 p_p1)
		{
			float p_max_distance = Vector3.Distance(p_p0, p_p1);
			Ray p_ray = new Ray(p_p0, p_p1 - p_p0);
			return Raycast(p_ray, p_max_distance);
		}

		public override void OnEditorSelect()
		{
			base.OnEditorSelect();
			if ((bool)m_box_renderer)
			{
				tool.SetColliderActiveAsync(p_flag: true);
				Hilight(p_flag: true);
			}
		}

		public override void OnEditorUnselect()
		{
			base.OnEditorUnselect();
			if ((bool)m_box_renderer)
			{
				Hilight(p_flag: false);
				tool.SetColliderActiveAsync(p_flag: false);
			}
		}
	}
}
