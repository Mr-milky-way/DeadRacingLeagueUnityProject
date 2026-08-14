using UnityEngine;

namespace thelab.core
{
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	public class GridRenderer : MonoBehaviour
	{
		private MeshRenderer m_renderer;

		private MeshFilter m_mf;

		public int steps = 5000;

		public float size = 1f;

		protected Mesh m_grid;

		public MeshRenderer renderer => m_renderer ?? (m_renderer = GetComponent<MeshRenderer>());

		protected MeshFilter mf => m_mf ?? (m_mf = GetComponent<MeshFilter>());

		protected void Awake()
		{
			Resize(size);
		}

		public void Resize(float p_size)
		{
			size = Mathf.Max(0f, p_size);
			if (!m_grid)
			{
				m_grid = new Mesh();
				m_grid.name = "grid-" + GetInstanceID().ToString("x6");
				m_grid.hideFlags = HideFlags.HideAndDontSave;
			}
			Vector3[] array = new Vector3[steps * 4];
			int[] array2 = new int[steps * 4];
			for (int i = 0; i < steps * 4; i++)
			{
				array[i] = Vector3.zero;
			}
			float num = steps;
			float num2 = (0f - num) * 0.5f * p_size;
			float num3 = (0f - num) * 0.5f * p_size;
			float num4 = (0f - num) * 0.5f * p_size;
			float num5 = num * 0.5f * p_size;
			int num6 = 0;
			int num7 = 0;
			for (int j = 0; j < steps; j++)
			{
				Vector3 vector = array[num6];
				vector.Set(num2, 0f, num4);
				array2[num6] = num7++;
				array[num6] = vector;
				num6++;
				vector = array[num6];
				vector.Set(num2, 0f, num5);
				array2[num6] = num7++;
				array[num6] = vector;
				num6++;
				num2 += p_size;
			}
			for (int k = 0; k < steps; k++)
			{
				Vector3 vector = array[num6];
				vector.Set(num4, 0f, num3);
				array2[num6] = num7++;
				array[num6] = vector;
				num6++;
				vector = array[num6];
				vector.Set(num5, 0f, num3);
				array2[num6] = num7++;
				array[num6] = vector;
				num6++;
				num3 += p_size;
			}
			m_grid.vertices = array;
			m_grid.SetIndices(array2, MeshTopology.Lines, 0);
			m_grid.UploadMeshData(markNoLongerReadable: false);
			mf.sharedMesh = m_grid;
		}
	}
}
