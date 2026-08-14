using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class ArrowGizmoRenderer : GizmoRenderer
	{
		public enum Type
		{
			None = 0,
			Cone = 1,
			Pyramid = 2,
			Cube = 3,
			Sphere = 4
		}

		public Type type = Type.Cone;

		public bool line = true;

		public bool cap = true;

		[SerializeField]
		protected float m_size = 1f;

		[Range(0f, 1f)]
		public float culling;

		public Vector3 scale = Vector3.one;

		public Vector3 offset = Vector3.zero;

		public Color lineColor = Color.white;

		public Color capColor = Color.white;

		[SerializeField]
		[HideInInspector]
		protected Mesh m_cone_cap_msh;

		[SerializeField]
		[HideInInspector]
		protected Mesh m_pyramid_cap_msh;

		[SerializeField]
		[HideInInspector]
		protected Mesh m_cube_cap_msh;

		[SerializeField]
		[HideInInspector]
		protected Mesh m_sphere_cap_msh;

		[SerializeField]
		[HideInInspector]
		protected Mesh m_line_mesh;

		protected List<Mesh> m_cap_meshes;

		protected Material m_line_material;

		protected Material m_cap_material;

		protected List<Vector3> m_vl;

		public float size
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

		protected Mesh m_current_cap
		{
			get
			{
				int num = (int)type;
				if (num < 0)
				{
					return null;
				}
				if (m_cap_meshes == null)
				{
					return null;
				}
				if (num >= m_cap_meshes.Count)
				{
					return null;
				}
				return m_cap_meshes[num];
			}
		}

		public override void Refresh(bool p_force)
		{
			if (p_force)
			{
				DestroyProper(m_line_mesh);
				m_line_mesh = null;
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
			if (!m_line_mesh)
			{
				m_vl = null;
			}
			m_line_mesh = AssertMesh(m_line_mesh, "arrow-line");
			m_line_material = AssertMaterial(m_line_material, material);
			m_cap_material = AssertMaterial(m_cap_material, material);
			m_cap_meshes = new List<Mesh> { null, m_cone_cap_msh, m_pyramid_cap_msh, m_cube_cap_msh, m_sphere_cap_msh };
			if (m_vl == null)
			{
				m_vl = new List<Vector3>();
				m_vl.Add(Vector3.zero);
				m_vl.Add(new Vector3(0f, 0f, 1f));
				List<int> list = new List<int>();
				list.Add(0);
				list.Add(1);
				m_line_mesh.SetVertices(m_vl);
				m_line_mesh.SetIndices(list.ToArray(), MeshTopology.LineStrip, 0);
				m_line_mesh.bounds = new Bounds(Vector3.zero, Vector3.one * size);
				m_line_mesh.UploadMeshData(markNoLongerReadable: true);
				if ((bool)mfilter)
				{
					mfilter.sharedMesh = m_line_mesh;
				}
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
			if (!m_line_mesh)
			{
				result = true;
			}
			if (!m_line_material)
			{
				result = true;
			}
			if (!m_cap_material)
			{
				result = true;
			}
			if (m_cap_meshes == null)
			{
				result = true;
			}
			return result;
		}

		protected override void OnRender()
		{
			Vector4 value = new Vector4(size, 0f, culling, 0f);
			Material line_material = m_line_material;
			if ((bool)line_material && line)
			{
				line_material.SetPass(1);
				value.w = 1f;
				line_material.SetVector("_Params", value);
				line_material.SetColor("_Color", lineColor * color);
				Vector3 lossyScale = base.transform.lossyScale;
				Matrix4x4 matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, lossyScale);
				Graphics.DrawMeshNow(m_line_mesh, matrix);
			}
			Mesh current_cap = m_current_cap;
			line_material = m_cap_material;
			if ((bool)line_material && (bool)current_cap && cap)
			{
				line_material.SetPass(1);
				value.w = 0f;
				line_material.SetVector("_Params", value);
				line_material.SetVector("_Scale", scale);
				line_material.SetColor("_Color", capColor * color);
				Vector3 zero = Vector3.zero;
				zero += base.transform.forward * size;
				zero.Scale(base.transform.lossyScale);
				zero += base.transform.right * offset.x;
				zero += base.transform.up * offset.y;
				zero += base.transform.forward * offset.z;
				Vector3 lossyScale2 = base.transform.lossyScale;
				lossyScale2 *= 0.3f;
				Matrix4x4 matrix2 = Matrix4x4.TRS(base.transform.position + zero, base.transform.rotation, lossyScale2);
				Graphics.DrawMeshNow(current_cap, matrix2);
			}
		}
	}
}
