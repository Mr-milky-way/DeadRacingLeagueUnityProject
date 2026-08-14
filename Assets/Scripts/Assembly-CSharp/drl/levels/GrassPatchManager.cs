using UnityEngine;

namespace drl.levels
{
	public class GrassPatchManager : MonoBehaviour
	{
		public MeshFilter[] meshFilters;

		public MeshRenderer[] meshRenderers;

		public float hideDistance = 10f;

		public Vector3 boundingBoxCenter = new Vector3(0f, 0f, 0f);

		public Vector3 boundingBoxSize = new Vector3(100f, 10f, 100f);

		[HideInInspector]
		public bool isEnabled = true;

		[HideInInspector]
		public bool drawGuizmo;

		private Vector3 m_boundingBoxCenter;

		private Vector3 m_boundingBoxSize;

		private bool m_currentState = true;

		private void Awake()
		{
			if (meshFilters.Length == 0)
			{
				meshFilters = base.transform.gameObject.GetComponentsInChildren<MeshFilter>(includeInactive: true);
				meshRenderers = base.transform.gameObject.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			}
			UpdateMeshBounds();
		}

		private void Update()
		{
			if (m_boundingBoxCenter != boundingBoxCenter || m_boundingBoxSize != boundingBoxSize)
			{
				UpdateMeshBounds();
			}
			if (!(Camera.main == null))
			{
				Vector3 position = base.transform.position;
				Vector3 vector = new Vector3(hideDistance, hideDistance, hideDistance);
				if (new Bounds(boundingBoxCenter + position, boundingBoxSize + vector).Contains(Camera.main.transform.position))
				{
					SetMeshRenderersState(p_state: true);
				}
				else
				{
					SetMeshRenderersState(p_state: false);
				}
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (drawGuizmo)
			{
				Vector3 position = base.transform.position;
				Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
				Gizmos.DrawWireCube(boundingBoxCenter + position, boundingBoxSize);
				Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
				Gizmos.DrawCube(boundingBoxCenter + position, boundingBoxSize);
			}
		}

		private void UpdateMeshBounds()
		{
			Vector3 position = base.transform.position;
			for (int i = 0; i < meshFilters.Length; i++)
			{
				meshFilters[i].sharedMesh.bounds = new Bounds(boundingBoxCenter + position, boundingBoxSize);
			}
			m_boundingBoxCenter = boundingBoxCenter;
			m_boundingBoxSize = boundingBoxSize;
		}

		public void SetState(bool p_enabled)
		{
			if (isEnabled != p_enabled)
			{
				isEnabled = p_enabled;
				base.gameObject.SetActive(p_enabled);
			}
		}

		private void SetMeshRenderersState(bool p_state)
		{
			if (m_currentState != p_state)
			{
				m_currentState = p_state;
				MeshRenderer[] array = meshRenderers;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].enabled = p_state;
				}
			}
		}
	}
}
