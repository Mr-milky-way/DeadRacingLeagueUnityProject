using UnityEngine;

namespace drl.game
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
	public class MeshCombineRenderer : MonoBehaviour
	{
		public Mesh baseMesh;

		public Material baseMaterial;

		private MeshRenderer m_renderer;

		private MeshFilter m_mfilter;

		public Texture2D parameters;

		public Color[] parametersValues;

		public int instances = 300;

		private int m_prev_child_count;

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

		private void Awake()
		{
			Resize();
		}

		[ContextMenu("Resize")]
		public void Resize()
		{
			if ((bool)mfilter.sharedMesh)
			{
				Object.Destroy(mfilter.sharedMesh);
			}
			if ((bool)baseMesh)
			{
				if (!parameters)
				{
					parameters = new Texture2D(1, 1, TextureFormat.RGBAFloat, mipChain: false);
					parameters.name = "mcr-params-" + parameters.GetHashCode().ToString("x");
					parameters.filterMode = FilterMode.Point;
					parameters.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
					parameters.Apply(updateMipmaps: false);
					parameters.hideFlags = HideFlags.HideAndDontSave;
				}
				parameters.Resize(5, instances);
				parametersValues = parameters.GetPixels();
				for (int i = 0; i < parametersValues.Length; i++)
				{
					parametersValues[i] = new Color(0f, 0f, 0f, 0f);
				}
				int[] indices = baseMesh.GetIndices(0);
				Vector3[] vertices = baseMesh.vertices;
				Vector3[] normals = baseMesh.normals;
				Vector4[] tangents = baseMesh.tangents;
				Vector2[] uv = baseMesh.uv;
				Vector2[] uv2 = baseMesh.uv2;
				Mesh mesh = new Mesh();
				mesh.name = "mcr-" + mesh.GetHashCode().ToString("x");
				mesh.hideFlags = HideFlags.HideAndDontSave;
				int[] array = new int[indices.Length * instances];
				Vector3[] array2 = new Vector3[vertices.Length * instances];
				Vector3[] array3 = new Vector3[normals.Length * instances];
				Vector4[] array4 = new Vector4[tangents.Length * instances];
				Vector2[] array5 = new Vector2[uv.Length * instances];
				Vector2[] array6 = new Vector2[uv2.Length * instances];
				Vector2[] array7 = new Vector2[uv2.Length * instances];
				int num = vertices.Length;
				int num2 = indices.Length;
				for (int j = 0; j < num2 * instances; j++)
				{
					array[j] = indices[j % num2] + num * (j / num2);
				}
				num2 = vertices.Length;
				for (int k = 0; k < num2 * instances; k++)
				{
					array2[k] = vertices[k % num2];
				}
				num2 = normals.Length;
				for (int l = 0; l < num2 * instances; l++)
				{
					array3[l] = normals[l % num2];
				}
				num2 = tangents.Length;
				for (int m = 0; m < num2 * instances; m++)
				{
					array4[m] = tangents[m % num2];
				}
				num2 = uv.Length;
				for (int n = 0; n < num2 * instances; n++)
				{
					array5[n] = uv[n % num2];
				}
				num2 = uv2.Length;
				for (int num3 = 0; num3 < num2 * instances; num3++)
				{
					array6[num3] = uv2[num3 % num2];
				}
				num2 = uv2.Length;
				for (int num4 = 0; num4 < num2 * instances; num4++)
				{
					array7[num4] = new Vector2(num4 / num2, 0f);
				}
				mesh.vertices = array2;
				mesh.normals = array3;
				mesh.tangents = array4;
				mesh.uv = array5;
				mesh.uv2 = array6;
				mesh.uv3 = array7;
				mesh.SetIndices(array, MeshTopology.Triangles, 0);
				mesh.UploadMeshData(markNoLongerReadable: true);
				mfilter.sharedMesh = mesh;
				if ((bool)renderer.sharedMaterial)
				{
					Object.Destroy(renderer.sharedMaterial);
				}
				Material material = (baseMaterial ? Object.Instantiate(baseMaterial) : null);
				if ((bool)material)
				{
					material.name = baseMaterial.name + "@" + material.GetHashCode().ToString("x");
					renderer.sharedMaterial = material;
				}
				Refresh();
			}
		}

		[ContextMenu("Clear")]
		public void Clear()
		{
			for (int i = 0; i < instances; i++)
			{
				int num = i * 5;
				parametersValues[num].r = 0f;
			}
			parameters.SetPixels(parametersValues);
			parameters.Apply();
		}

		[ContextMenu("Refresh")]
		public void Refresh()
		{
			if (!baseMesh)
			{
				return;
			}
			Bounds bounds = default(Bounds);
			Bounds bounds2 = baseMesh.bounds;
			Transform transform = base.transform;
			int num = 0;
			int prev_child_count = m_prev_child_count;
			for (int i = 0; i < prev_child_count; i++)
			{
				int num2 = i * 5;
				parametersValues[num2].r = 0f;
			}
			prev_child_count = (m_prev_child_count = Mathf.Min(transform.childCount, instances));
			num = 0;
			for (int j = 0; j < prev_child_count; j++)
			{
				Transform child = base.transform.GetChild(j);
				Matrix4x4 matrix4x = Matrix4x4.TRS(child.localPosition, child.localRotation, child.localScale * 0.5f);
				for (int k = 0; k < 5; k++)
				{
					Color color = parametersValues[num];
					switch (k)
					{
					case 0:
						color.r = 1f;
						break;
					case 1:
						color = matrix4x.GetRow(0);
						break;
					case 2:
						color = matrix4x.GetRow(1);
						break;
					case 3:
						color = matrix4x.GetRow(2);
						break;
					case 4:
						color = matrix4x.GetRow(3);
						break;
					}
					parametersValues[num++] = color;
				}
				Vector3 size = bounds2.size;
				size.Scale(child.localScale);
				Bounds bounds3 = new Bounds(child.localPosition, size);
				if (j <= 0)
				{
					bounds = bounds3;
				}
				else
				{
					bounds.Encapsulate(bounds3);
				}
			}
			mfilter.sharedMesh.bounds = bounds;
			parameters.SetPixels(parametersValues);
			parameters.Apply();
			Material sharedMaterial = renderer.sharedMaterial;
			sharedMaterial.SetFloat("_InstanceCount", instances);
			sharedMaterial.SetTexture("_ParamsTex", parameters);
		}

		private void LateUpdate()
		{
			if (!mfilter.sharedMesh)
			{
				Resize();
			}
			Transform transform = base.transform;
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (child.hasChanged)
				{
					child.hasChanged = false;
					Refresh();
					break;
				}
			}
		}

		private void OnDestroy()
		{
			if ((bool)mfilter.sharedMesh)
			{
				Object.Destroy(mfilter.sharedMesh);
			}
			if ((bool)parameters)
			{
				Object.Destroy(parameters);
			}
		}
	}
}
