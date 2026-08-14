using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace thelab.core
{
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	public class MeshCombineComponent : MonoBehaviour
	{
		public class CombineJob
		{
			public class Buffer
			{
				public Vector3[] vertexes;

				public Vector3[] normals;

				public Color[] colors;

				public Vector4[] tangents;

				public Vector2[] uv0;

				public Vector2[] uv1;

				public List<int[]> triangles;

				public Buffer(Mesh p_mesh = null)
				{
					vertexes = ((p_mesh == null) ? new Vector3[0] : p_mesh.vertices);
					normals = ((p_mesh == null) ? new Vector3[0] : p_mesh.normals);
					tangents = ((p_mesh == null) ? new Vector4[0] : p_mesh.tangents);
					colors = ((p_mesh == null) ? new Color[0] : p_mesh.colors);
					uv0 = ((p_mesh == null) ? new Vector2[0] : p_mesh.uv);
					uv1 = ((p_mesh == null) ? new Vector2[0] : p_mesh.uv2);
					triangles = new List<int[]>();
					if (p_mesh != null)
					{
						for (int i = 0; i < p_mesh.subMeshCount; i++)
						{
							triangles.Add(p_mesh.GetTriangles(i));
						}
					}
				}

				public void Alloc(int p_vertex_count, int[] p_submesh_triangle_count)
				{
					vertexes = new Vector3[p_vertex_count];
					normals = new Vector3[p_vertex_count];
					tangents = new Vector4[p_vertex_count];
					colors = new Color[p_vertex_count];
					uv0 = new Vector2[p_vertex_count];
					uv1 = new Vector2[p_vertex_count];
					triangles = new List<int[]>();
					foreach (int num in p_submesh_triangle_count)
					{
						triangles.Add(new int[num]);
					}
				}
			}

			public int vertexOffset;

			public int[] trianglesOffset;

			public Matrix4x4 root;

			public Matrix4x4 transform;

			public Buffer source;

			public Buffer destination;

			public CombineJob(Mesh p_mesh, Transform p_container, int p_vertex_offset, int[] p_triangles_offset)
			{
				source = new Buffer(p_mesh);
				transform = p_container.localToWorldMatrix;
				root = Matrix4x4.identity;
				vertexOffset = p_vertex_offset;
				trianglesOffset = new int[p_triangles_offset.Length];
				for (int i = 0; i < trianglesOffset.Length; i++)
				{
					trianglesOffset[i] = p_triangles_offset[i];
				}
			}

			public void Transform()
			{
				Matrix4x4 matrix4x = transform;
				for (int i = 0; i < source.vertexes.Length; i++)
				{
					source.vertexes[i] = matrix4x.MultiplyPoint3x4(source.vertexes[i]);
					source.vertexes[i] = root.MultiplyPoint3x4(source.vertexes[i]);
				}
				for (int j = 0; j < source.triangles.Count; j++)
				{
					int[] array = source.triangles[j];
					for (int k = 0; k < array.Length; k++)
					{
						array[k] += vertexOffset;
					}
				}
			}

			public void Apply()
			{
				Buffer buffer = destination;
				int num = vertexOffset;
				for (int i = 0; i < source.vertexes.Length; i++)
				{
					if (i < source.vertexes.Length && num < buffer.vertexes.Length)
					{
						buffer.vertexes[num] = source.vertexes[i];
					}
					if (i < source.normals.Length && num < buffer.normals.Length)
					{
						buffer.normals[num] = source.normals[i];
					}
					if (i < source.tangents.Length && num < buffer.tangents.Length)
					{
						buffer.tangents[num] = source.tangents[i];
					}
					if (i < source.colors.Length && num < buffer.colors.Length)
					{
						buffer.colors[num] = source.colors[i];
					}
					if (i < source.uv0.Length && num < buffer.uv0.Length)
					{
						buffer.uv0[num] = source.uv0[i];
					}
					if (i < source.uv1.Length && num < buffer.uv1.Length)
					{
						buffer.uv1[num] = source.uv1[i];
					}
					num++;
				}
				int[] array = new int[source.triangles.Count];
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = trianglesOffset[j];
				}
				for (int k = 0; k < source.triangles.Count; k++)
				{
					int[] array2 = source.triangles[k];
					for (int l = 0; l < array2.Length; l++)
					{
						buffer.triangles[k][array[k]] = array2[l];
						array[k]++;
					}
				}
			}

			public void Clear()
			{
				source.vertexes = null;
				source.normals = null;
				source.tangents = null;
				source.colors = null;
				source.uv0 = null;
				source.uv1 = null;
				for (int i = 0; i < source.triangles.Count; i++)
				{
					source.triangles[i] = null;
				}
				source.triangles.Clear();
			}
		}

		public List<MeshRenderer> targets;

		public bool snapCenter;

		public bool enable32bitIndex;

		public bool ignoreDisabled;

		private MeshRenderer m_renderer;

		private MeshFilter m_mfilter;

		private Mesh m_combined_mesh;

		private List<CombineJob> m_jobs;

		public uint maxVertexIndex
		{
			get
			{
				if (!SystemInfo.supports32bitsIndexBuffer || !enable32bitIndex)
				{
					return 65535u;
				}
				return uint.MaxValue;
			}
		}

		public MeshRenderer renderer
		{
			get
			{
				if (!m_renderer)
				{
					return m_renderer = GetComponent<MeshRenderer>();
				}
				return m_renderer;
			}
		}

		public MeshFilter mfilter
		{
			get
			{
				if (!m_mfilter)
				{
					return m_mfilter = GetComponent<MeshFilter>();
				}
				return m_mfilter;
			}
		}

		public void Apply()
		{
			if (m_jobs == null)
			{
				m_jobs = new List<CombineJob>();
			}
			if (m_jobs.Count > 0)
			{
				for (int i = 0; i < m_jobs.Count; i++)
				{
					m_jobs[i].Clear();
				}
			}
			m_jobs.Clear();
			List<MeshRenderer> list = new List<MeshRenderer>(targets);
			list.RemoveAll(delegate(MeshRenderer it)
			{
				if (!it)
				{
					return true;
				}
				return !it.gameObject.activeInHierarchy || !it.enabled;
			});
			for (int num = 0; num < list.Count; num++)
			{
				MeshRenderer meshRenderer = list[num];
				if (!meshRenderer)
				{
					list.RemoveAt(num--);
					continue;
				}
				MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
				if (!component)
				{
					list.RemoveAt(num--);
					continue;
				}
				Material[] sharedMaterials = meshRenderer.sharedMaterials;
				bool flag = true;
				for (int num2 = 0; num2 < sharedMaterials.Length; num2++)
				{
					if (!sharedMaterials[num2])
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					Debug.LogWarning("MeshCombineComponent> Invalid Materials / combiner[" + base.name + "] renderer[" + meshRenderer.name + "]");
					list.RemoveAt(num--);
				}
				else if (!component.sharedMesh)
				{
					Debug.LogWarning("MeshCombineComponent> Invalid Mesh / combiner[" + base.name + "] renderer[" + meshRenderer.name + "]");
					list.RemoveAt(num--);
				}
			}
			int vertexCount = GetVertexCount();
			if (vertexCount >= maxVertexIndex)
			{
				Debug.LogError("MeshCombineComponent> Apply / Limit of [" + maxVertexIndex + "v] at [" + vertexCount + "] vertex exceeded for [" + base.name + "]");
				return;
			}
			CombineJob.Buffer buffer = new CombineJob.Buffer();
			int[] triangleCountPerSubMesh = GetTriangleCountPerSubMesh();
			buffer.Alloc(vertexCount, triangleCountPerSubMesh);
			bool flag2 = snapCenter && list.Count > 0;
			Vector3 vector = (flag2 ? Vector3.zero : base.transform.position);
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				if ((bool)list[num3])
				{
					vector += list[num3].transform.position;
				}
			}
			float num4 = (flag2 ? (1f / (float)list.Count) : 1f);
			vector *= num4;
			int num5 = 0;
			int[] array = new int[4];
			for (int num6 = 0; num6 < array.Length; num6++)
			{
				array[num6] = 0;
			}
			for (int num7 = 0; num7 < list.Count; num7++)
			{
				MeshRenderer meshRenderer2 = list[num7];
				if (!meshRenderer2)
				{
					continue;
				}
				MeshFilter component2 = meshRenderer2.GetComponent<MeshFilter>();
				if (!component2)
				{
					continue;
				}
				Mesh sharedMesh = component2.sharedMesh;
				if (flag2)
				{
					meshRenderer2.transform.position += base.transform.position - vector;
				}
				CombineJob combineJob = new CombineJob(sharedMesh, meshRenderer2.transform, num5, array);
				combineJob.root = base.transform.worldToLocalMatrix;
				combineJob.destination = buffer;
				m_jobs.Add(combineJob);
				if (flag2)
				{
					meshRenderer2.transform.position -= base.transform.position - vector;
				}
				if ((bool)sharedMesh)
				{
					num5 += sharedMesh.vertexCount;
					for (int num8 = 0; num8 < sharedMesh.subMeshCount; num8++)
					{
						array[num8] += sharedMesh.GetTriangles(num8).Length;
					}
				}
			}
			for (int num9 = 0; num9 < m_jobs.Count; num9++)
			{
				CombineJob combineJob2 = m_jobs[num9];
				combineJob2.Transform();
				combineJob2.Apply();
			}
			Mesh mesh = new Mesh();
			mesh.indexFormat = (SystemInfo.supports32bitsIndexBuffer ? IndexFormat.UInt32 : IndexFormat.UInt16);
			mesh.name = base.name + "-$combine-mesh";
			mesh.vertices = buffer.vertexes;
			mesh.normals = buffer.normals;
			mesh.tangents = buffer.tangents;
			mesh.colors = buffer.colors;
			mesh.uv = buffer.uv0;
			mesh.uv2 = buffer.uv1;
			mesh.subMeshCount = buffer.triangles.Count;
			for (int num10 = 0; num10 < buffer.triangles.Count; num10++)
			{
				mesh.SetTriangles(buffer.triangles[num10], num10);
			}
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			mesh.RecalculateTangents();
			mesh.hideFlags = HideFlags.DontSave;
			SetMesh(mesh);
			for (int num11 = 0; num11 < m_jobs.Count; num11++)
			{
				m_jobs.Clear();
			}
		}

		public int GetVertexCount()
		{
			int num = 0;
			for (int i = 0; i < targets.Count; i++)
			{
				MeshRenderer meshRenderer = targets[i];
				if ((bool)meshRenderer)
				{
					MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
					if ((bool)component)
					{
						Mesh sharedMesh = component.sharedMesh;
						num += (sharedMesh ? sharedMesh.vertexCount : 0);
					}
				}
			}
			return num;
		}

		public int[] GetTriangleCountPerSubMesh()
		{
			List<int> list = new List<int>();
			for (int i = 0; i < targets.Count; i++)
			{
				MeshRenderer meshRenderer = targets[i];
				if (!meshRenderer)
				{
					continue;
				}
				MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
				if (!component)
				{
					continue;
				}
				Mesh sharedMesh = component.sharedMesh;
				if (!sharedMesh)
				{
					continue;
				}
				for (int j = 0; j < sharedMesh.subMeshCount; j++)
				{
					if (j >= list.Count)
					{
						list.Add(0);
					}
					list[j] += sharedMesh.GetTriangles(j).Length;
				}
			}
			return list.ToArray();
		}

		public void SetTargetsEnabled(bool p_flag)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				MeshRenderer meshRenderer = targets[i];
				if ((bool)meshRenderer)
				{
					meshRenderer.enabled = p_flag;
				}
			}
		}

		public void Clear()
		{
			if ((bool)m_combined_mesh)
			{
				Object.Destroy(m_combined_mesh);
			}
		}

		protected void SetMesh(Mesh p_mesh)
		{
			if ((bool)m_combined_mesh)
			{
				Object.Destroy(m_combined_mesh);
			}
			m_combined_mesh = p_mesh;
			mfilter.sharedMesh = p_mesh;
		}
	}
}
