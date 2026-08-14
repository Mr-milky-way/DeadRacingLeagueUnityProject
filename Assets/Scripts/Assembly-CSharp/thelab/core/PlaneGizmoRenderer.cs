using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class PlaneGizmoRenderer : GizmoRenderer
	{
		public bool outline;

		public bool fill;

		[SerializeField]
		protected Vector2 m_size = Vector3.one;

		[Range(0f, 1f)]
		public float culling;

		public Color fillColor = Color.white;

		public Color outlineColor = Color.white;

		protected Mesh m_fill_mesh;

		protected Mesh m_outline_mesh;

		protected Material m_fill_material;

		protected Material m_outline_material;

		protected List<Vector3> m_vl;

		protected List<Vector3> m_nl;

		public Vector2 size
		{
			get
			{
				return m_size;
			}
			set
			{
				m_size = value;
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
				if (m_nl != null)
				{
					m_nl.Clear();
				}
				m_nl = null;
			}
			base.Refresh(p_force);
		}

		protected override void OnRefresh()
		{
			if (!m_fill_mesh)
			{
				m_vl = null;
				m_nl = null;
			}
			if (!m_outline_mesh)
			{
				m_vl = null;
				m_nl = null;
			}
			m_fill_mesh = AssertMesh(m_fill_mesh, "plane-gizmo.solid");
			m_outline_mesh = AssertMesh(m_outline_mesh, "plane-gizmo-outline");
			m_fill_material = AssertMaterial(m_fill_material, material);
			m_outline_material = AssertMaterial(m_outline_material, material);
			if (m_vl == null)
			{
				m_vl = new List<Vector3>();
				m_vl.Add(Vector3.zero);
				m_vl.Add(new Vector3(1f, 0f, 0f));
				m_vl.Add(new Vector3(1f, 0f, 1f));
				m_vl.Add(new Vector3(0f, 0f, 1f));
				m_vl.Add(new Vector3(0f, 0f, 0f));
				m_nl = new List<Vector3>();
				m_nl.Add(Vector3.up);
				m_nl.Add(Vector3.up);
				m_nl.Add(Vector3.up);
				m_nl.Add(Vector3.up);
				m_nl.Add(Vector3.up);
				List<int> list = new List<int>();
				for (int i = 0; i < m_vl.Count; i++)
				{
					list.Add(i);
				}
				m_fill_mesh.Clear();
				m_fill_mesh.SetVertices(m_vl);
				m_fill_mesh.SetNormals(m_nl);
				m_fill_mesh.SetIndices(list.ToArray(), MeshTopology.Quads, 0);
				m_fill_mesh.bounds = new Bounds(Vector3.zero, new Vector3(m_size.x, 0f, m_size.y));
				m_fill_mesh.UploadMeshData(markNoLongerReadable: false);
				list.Clear();
				for (int j = 0; j < m_vl.Count; j++)
				{
					list.Add(j);
				}
				m_outline_mesh.Clear();
				m_outline_mesh.SetVertices(m_vl);
				m_outline_mesh.SetNormals(m_nl);
				m_outline_mesh.SetIndices(list.ToArray(), MeshTopology.LineStrip, 0);
				m_outline_mesh.RecalculateBounds();
				m_outline_mesh.UploadMeshData(markNoLongerReadable: false);
				m_outline_mesh.bounds = new Bounds(Vector3.zero, new Vector3(m_size.x, 0f, m_size.y));
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
			Vector4 value = new Vector4(m_size.x, m_size.y, culling, 0f);
			Material fill_material = m_fill_material;
			if ((bool)fill_material && fill)
			{
				fill_material.SetPass(2);
				value.w = 1f;
				fill_material.SetVector("_Params", value);
				fill_material.SetColor("_Color", fillColor * color);
				Graphics.DrawMeshNow(m_fill_mesh, base.transform.localToWorldMatrix);
			}
			fill_material = m_outline_material;
			if ((bool)fill_material && outline)
			{
				fill_material.SetPass(2);
				value.w = 1f;
				fill_material.SetVector("_Params", value);
				fill_material.SetColor("_Color", outlineColor * color);
				Graphics.DrawMeshNow(m_outline_mesh, base.transform.localToWorldMatrix);
			}
		}
	}
}
