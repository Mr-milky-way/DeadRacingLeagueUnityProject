using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class RadialGizmoRenderer : GizmoRenderer
	{
		public bool outline;

		public bool fill;

		[SerializeField]
		protected bool m_closed;

		public float angle;

		[SerializeField]
		protected float m_radius = 1f;

		[Range(0f, 1f)]
		public float culling;

		public Color fillColor = Color.white;

		public Color outlineColor = Color.white;

		protected Mesh m_fill_mesh;

		protected Mesh m_outline_mesh;

		protected Material m_fill_material;

		protected Material m_outline_material;

		protected List<Vector3> m_vl;

		public bool closed
		{
			get
			{
				return m_closed;
			}
			set
			{
				m_closed = value;
				Refresh(p_force: true);
			}
		}

		public float radius
		{
			get
			{
				return m_radius;
			}
			set
			{
				m_radius = value;
				Refresh(p_force: true);
			}
		}

		public override void Refresh(bool p_force)
		{
			if (p_force)
			{
				DestroyProper(m_fill_mesh);
				DestroyProper(m_outline_mesh);
				if (m_vl != null)
				{
					m_vl.Clear();
				}
				m_vl = null;
			}
			base.Refresh(p_force);
		}

		protected override void OnRefresh()
		{
			if (!m_fill_mesh)
			{
				m_vl = null;
			}
			if (!m_outline_mesh)
			{
				m_vl = null;
			}
			m_fill_mesh = AssertMesh(m_fill_mesh, "radial-gizmo.solid");
			m_outline_mesh = AssertMesh(m_outline_mesh, "radial-gizmo-outline");
			m_fill_material = AssertMaterial(m_fill_material, material);
			m_outline_material = AssertMaterial(m_outline_material, material);
			if (m_vl == null)
			{
				Vector3 item = new Vector3(1f, 0f, 0f);
				float num = 800f;
				m_vl = new List<Vector3>();
				m_vl.Add(Vector3.zero);
				for (int i = 0; (float)i < num; i++)
				{
					float z = (float)i / (num - 1f);
					item.z = z;
					m_vl.Add(item);
				}
				List<int> list = new List<int>();
				for (int j = 2; j < m_vl.Count; j++)
				{
					list.Add(0);
					list.Add(j - 1);
					list.Add(j);
				}
				m_fill_mesh.Clear();
				m_fill_mesh.SetVertices(m_vl);
				m_fill_mesh.SetIndices(list.ToArray(), MeshTopology.Triangles, 0);
				m_fill_mesh.bounds = new Bounds(Vector3.zero, Vector3.one * radius);
				m_fill_mesh.UploadMeshData(markNoLongerReadable: false);
				if (closed)
				{
					m_vl.Add(new Vector3(0f, 0f, 0f));
				}
				else
				{
					m_vl.RemoveAt(0);
				}
				list.Clear();
				for (int k = 0; k < m_vl.Count; k++)
				{
					list.Add(k);
				}
				m_outline_mesh.Clear();
				m_outline_mesh.SetVertices(m_vl);
				m_outline_mesh.SetIndices(list.ToArray(), MeshTopology.LineStrip, 0);
				m_outline_mesh.RecalculateBounds();
				m_outline_mesh.UploadMeshData(markNoLongerReadable: false);
				m_outline_mesh.bounds = new Bounds(Vector3.zero, Vector3.one * radius);
			}
			if ((bool)mfilter && !mfilter.sharedMesh)
			{
				mfilter.sharedMesh = m_outline_mesh;
			}
		}

		protected override bool IsDirty()
		{
			bool result = false;
			if (!mfilter)
			{
				result = true;
			}
			if ((bool)mfilter && !mfilter.sharedMesh)
			{
				result = true;
			}
			if (!m_fill_mesh)
			{
				result = true;
			}
			if (!m_outline_mesh)
			{
				result = true;
			}
			if (!m_fill_material)
			{
				result = true;
			}
			if (!m_outline_material)
			{
				result = true;
			}
			return result;
		}

		protected override void OnRender()
		{
			Vector4 value = new Vector4(angle, radius, culling, 0f);
			Material fill_material = m_fill_material;
			if ((bool)fill_material && fill)
			{
				fill_material.SetPass(0);
				fill_material.SetVector("_Params", value);
				fill_material.SetColor("_Color", fillColor * color);
				Graphics.DrawMeshNow(m_fill_mesh, base.transform.localToWorldMatrix);
			}
			fill_material = m_outline_material;
			if ((bool)fill_material && outline)
			{
				fill_material.SetPass(0);
				fill_material.SetVector("_Params", value);
				fill_material.SetColor("_Color", outlineColor * color);
				Graphics.DrawMeshNow(m_outline_mesh, base.transform.localToWorldMatrix);
			}
		}
	}
}
