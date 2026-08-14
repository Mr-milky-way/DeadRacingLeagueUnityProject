using System.Collections.Generic;
using UnityEngine;

public class CameraFadeReflectionProbes : MonoBehaviour
{
	public Camera gameCamera;

	public GameObject targetLevel;

	public GameObject targetTracks;

	public Vector3 rangePadding = new Vector3(100f, 100f, 100f);

	public float distancePow = 2.5f;

	public bool debugGameObjectState;

	private List<ReflectionProbe> m_probesStatic;

	private List<BoxCollider> m_probesStaticInner;

	private List<BoxCollider> m_probesStaticOuter;

	private Vector3[] m_probesStaticSize;

	private bool[] m_probesStaticEnabled;

	private Vector3 m_lastCameraPosition;

	private bool m_updateAll;

	private int m_chunks = 6;

	private int m_iterator;

	public bool drawGizmo;

	private void OnDisable()
	{
		if (m_probesStatic == null)
		{
			return;
		}
		for (int i = 0; i < m_probesStatic.Count; i++)
		{
			ReflectionProbe reflectionProbe = m_probesStatic[i];
			if ((bool)reflectionProbe)
			{
				reflectionProbe.size = m_probesStaticSize[i];
				reflectionProbe.enabled = m_probesStaticEnabled[i];
				if (debugGameObjectState)
				{
					reflectionProbe.gameObject.SetActive(reflectionProbe.enabled);
				}
			}
		}
	}

	public void Initialize()
	{
		m_probesStatic = new List<ReflectionProbe>();
		m_probesStatic.AddRange(targetLevel.GetComponentsInChildren<ReflectionProbe>());
		if (targetTracks != null)
		{
			m_probesStatic.AddRange(targetTracks.GetComponentsInChildren<ReflectionProbe>());
		}
		m_probesStaticInner = new List<BoxCollider>();
		m_probesStaticOuter = new List<BoxCollider>();
		for (int i = 0; i < m_probesStatic.Count; i++)
		{
			ReflectionProbe reflectionProbe = m_probesStatic[i];
			BoxCollider boxCollider = reflectionProbe.gameObject.AddComponent<BoxCollider>();
			boxCollider.center = reflectionProbe.center;
			boxCollider.size = reflectionProbe.size;
			boxCollider.enabled = false;
			boxCollider.isTrigger = true;
			m_probesStaticInner.Add(boxCollider);
			boxCollider = reflectionProbe.gameObject.AddComponent<BoxCollider>();
			boxCollider.center = reflectionProbe.center;
			boxCollider.size = reflectionProbe.size + rangePadding;
			boxCollider.enabled = false;
			boxCollider.isTrigger = true;
			m_probesStaticOuter.Add(boxCollider);
		}
		m_probesStaticSize = new Vector3[m_probesStatic.Count];
		m_probesStaticEnabled = new bool[m_probesStatic.Count];
		for (int j = 0; j < m_probesStatic.Count; j++)
		{
			m_probesStaticSize[j] = m_probesStatic[j].size;
			m_probesStaticEnabled[j] = m_probesStatic[j].enabled;
		}
	}

	private void LateUpdate()
	{
		Vector3 position = gameCamera.transform.position;
		if (m_probesStatic != null && m_probesStatic.Count > 0)
		{
			int num = (m_updateAll ? 1 : m_chunks);
			int num2 = Mathf.Max(m_probesStatic.Count / num, 1);
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				num3 = m_iterator % m_probesStatic.Count;
				ReflectionProbe reflectionProbe = m_probesStatic[num3];
				if (!reflectionProbe)
				{
					continue;
				}
				BoxCollider boxCollider = m_probesStaticInner[num3];
				BoxCollider boxCollider2 = m_probesStaticOuter[num3];
				boxCollider.enabled = true;
				boxCollider2.enabled = true;
				float probeIntensity = GetProbeIntensity(boxCollider, boxCollider2, position);
				Vector3 vector = m_probesStaticSize[num3] * probeIntensity;
				if ((reflectionProbe.size - vector).sqrMagnitude > 0.01f)
				{
					reflectionProbe.size = vector;
				}
				bool flag = probeIntensity > 0f;
				if (flag != reflectionProbe.enabled)
				{
					reflectionProbe.enabled = flag;
					if (debugGameObjectState)
					{
						reflectionProbe.gameObject.SetActive(flag);
					}
				}
				boxCollider.enabled = false;
				boxCollider2.enabled = false;
				m_iterator++;
			}
		}
		_ = m_lastCameraPosition;
		if (Vector3.Distance(m_lastCameraPosition, position) > rangePadding.magnitude * 0.5f)
		{
			m_updateAll = true;
			m_iterator = 0;
		}
		else
		{
			m_updateAll = false;
		}
		m_lastCameraPosition = position;
	}

	protected float GetProbeIntensity(BoxCollider p_inner, BoxCollider p_outer, Vector3 p_camera_pos)
	{
		float normalizedDistance = GetNormalizedDistance(p_inner, p_outer, p_camera_pos);
		normalizedDistance = Mathf.Pow(normalizedDistance, distancePow);
		return 1f - normalizedDistance;
	}

	public float GetNormalizedDistance(BoxCollider p_inner, BoxCollider p_outer, Vector3 p_position, out Vector3 p_inner_pos, out Vector3 p_outer_pos)
	{
		p_inner_pos = Vector3.zero;
		p_outer_pos = Vector3.zero;
		if (!p_outer)
		{
			return 1f;
		}
		if (!p_inner)
		{
			return 1f;
		}
		Vector3 vector = p_inner.ClosestPointOnBounds(p_position);
		Vector3 vector2 = p_position - vector;
		Vector3 vector3 = p_position + vector2.normalized * (p_inner.size - p_outer.size).magnitude * 0.5f;
		for (int i = 0; i < 5; i++)
		{
			vector3 = p_outer.ClosestPointOnBounds(vector3);
			if (i >= 4)
			{
				break;
			}
			vector3 -= vector;
			float num = Vector3.Dot(vector3, vector2.normalized);
			vector3 = vector + vector2.normalized * num;
		}
		p_inner_pos = vector;
		p_outer_pos = vector3;
		vector2 = p_position - vector;
		float num2 = Vector3.Distance(vector3, vector);
		float num3 = Vector3.Dot(vector2, (vector3 - vector).normalized);
		if (num2 <= 0f)
		{
			return 0f;
		}
		return Mathf.Clamp01(num3 / num2);
	}

	public float GetNormalizedDistance(BoxCollider p_inner, BoxCollider p_outer, Vector3 p_position)
	{
		Vector3 p_inner_pos;
		Vector3 p_outer_pos;
		return GetNormalizedDistance(p_inner, p_outer, p_position, out p_inner_pos, out p_outer_pos);
	}

	private void OnDrawGizmos()
	{
		if (!drawGizmo || m_probesStatic == null)
		{
			return;
		}
		Vector3 position = gameCamera.transform.position;
		if (m_probesStatic.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < m_probesStatic.Count; i++)
		{
			ReflectionProbe reflectionProbe = m_probesStatic[i];
			if ((bool)reflectionProbe)
			{
				BoxCollider boxCollider = m_probesStaticInner[i];
				BoxCollider boxCollider2 = m_probesStaticOuter[i];
				boxCollider.enabled = true;
				boxCollider2.enabled = true;
				float probeIntensity = GetProbeIntensity(boxCollider, boxCollider2, position);
				Gizmos.color = ((!reflectionProbe.enabled) ? Color.red : ((probeIntensity >= 1f) ? Color.green : Color.yellow));
				if (!base.enabled)
				{
					Gizmos.color = Color.green;
				}
				Gizmos.DrawWireCube(reflectionProbe.transform.TransformPoint(boxCollider.center), boxCollider.size);
				Gizmos.DrawWireCube(reflectionProbe.transform.TransformPoint(boxCollider2.center), boxCollider2.size);
				probeIntensity = GetNormalizedDistance(boxCollider, boxCollider2, position, out var p_inner_pos, out var p_outer_pos);
				Gizmos.color = Color.magenta;
				Gizmos.DrawSphere(p_inner_pos, 0.6f);
				Gizmos.DrawSphere(p_outer_pos, 0.6f);
				boxCollider.enabled = false;
				boxCollider2.enabled = false;
			}
		}
	}
}
