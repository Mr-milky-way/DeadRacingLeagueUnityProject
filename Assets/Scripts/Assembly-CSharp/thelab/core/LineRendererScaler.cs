using UnityEngine;

namespace thelab.core
{
	[ExecuteInEditMode]
	public class LineRendererScaler : MonoBehaviour
	{
		[SerializeField]
		private Renderer m_target;

		public float width = 1f;

		public float startSize = 1f;

		public float endSize = 1f;

		public bool sampleClosest;

		private Vector3[] m_line_points;

		public Renderer target
		{
			get
			{
				if (!m_target)
				{
					return m_target = GetComponent<Renderer>();
				}
				return m_target;
			}
		}

		protected void OnWillRenderObject()
		{
			if (!target)
			{
				return;
			}
			Camera current = Camera.current;
			Vector4 vector = base.transform.position;
			if (sampleClosest && (bool)target && target is LineRenderer)
			{
				LineRenderer lineRenderer = target as LineRenderer;
				if (m_line_points == null)
				{
					m_line_points = new Vector3[0];
				}
				if (m_line_points.Length != lineRenderer.positionCount)
				{
					m_line_points = new Vector3[lineRenderer.positionCount];
				}
				Vector3 position = current.transform.position;
				lineRenderer.GetPositions(m_line_points);
				if (m_line_points.Length != 0)
				{
					Vector3 vector2 = m_line_points[0];
					float num = Vector3.Distance(vector2, position);
					for (int i = 1; i < m_line_points.Length; i++)
					{
						Vector3 vector3 = m_line_points[i];
						float num2 = Vector3.Distance(position, vector3);
						if (num2 < num)
						{
							num = num2;
							vector2 = vector3;
						}
					}
					vector = vector2;
				}
			}
			vector.w = 1f;
			vector = current.worldToCameraMatrix * vector;
			vector = current.projectionMatrix * vector;
			float num3 = 2f * width / (float)Screen.width;
			vector.w = Mathf.Max(width * 0.5f, vector.w);
			num3 *= vector.w;
			if (target is TrailRenderer)
			{
				TrailRenderer trailRenderer = target as TrailRenderer;
				trailRenderer.startWidth = num3 * startSize * trailRenderer.widthMultiplier;
				trailRenderer.endWidth = num3 * endSize * trailRenderer.widthMultiplier;
			}
			if (target is LineRenderer)
			{
				(target as LineRenderer).widthMultiplier = num3;
			}
		}
	}
}
