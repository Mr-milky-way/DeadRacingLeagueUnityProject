using System;
using UnityEngine;

namespace thelab.core
{
	[ExecuteInEditMode]
	public class BillboardComponent : MonoBehaviour
	{
		public Transform target;

		public Vector3 rotation;

		public Vector3 scale = Vector3.one;

		public bool orient = true;

		public bool resize;

		private MeshFilter m_mfilter;

		private Renderer m_renderer;

		private Transform m_transform_cache;

		private Transform m_target;

		protected virtual void Awake()
		{
			if (base.enabled)
			{
				AssertComponents();
			}
		}

		protected virtual void Start()
		{
		}

		protected void AssertComponents()
		{
			if (!m_mfilter)
			{
				m_mfilter = GetComponent<MeshFilter>();
				if ((bool)m_mfilter && !m_mfilter.sharedMesh)
				{
					m_mfilter.hideFlags = HideFlags.HideInInspector;
				}
			}
			if (!m_renderer)
			{
				m_renderer = GetComponent<Renderer>();
			}
			if (!m_transform_cache)
			{
				m_transform_cache = base.transform;
			}
			if (!m_target)
			{
				m_target = (target ? target : m_transform_cache);
			}
		}

		protected virtual void OnWillRenderObject()
		{
			bool flag = true;
			if (!m_mfilter)
			{
				flag = false;
			}
			if ((bool)m_mfilter && !m_mfilter.sharedMesh)
			{
				flag = false;
			}
			if (flag)
			{
				_ = (bool)m_renderer;
			}
			if (!base.enabled)
			{
				return;
			}
			Camera current = Camera.current;
			Transform transform = current.transform;
			Transform transform2 = m_target;
			if (orient)
			{
				Quaternion quaternion = Quaternion.LookRotation(transform.forward, transform.up);
				transform2.rotation = quaternion * Quaternion.Euler(rotation);
			}
			if (resize)
			{
				float num = Vector3.Dot(transform2.position - transform.position, transform.forward);
				float num2 = current.pixelWidth;
				float num3 = current.pixelHeight;
				if (!(num3 <= 0f))
				{
					_ = num2 / num3;
				}
				float num4 = Mathf.Sqrt(num2 * num2 + num3 * num3) * Mathf.Tan(current.fieldOfView * ((float)Math.PI / 180f));
				float num5 = Mathf.Max(0.0001f, num / num4 * 100f);
				Vector3 localScale = scale * num5;
				transform2.localScale = Vector3.one;
				Vector3 lossyScale = transform2.lossyScale;
				lossyScale.x = ((Mathf.Abs(lossyScale.x) <= 0.0001f) ? 0f : (1f / lossyScale.x));
				lossyScale.y = ((Mathf.Abs(lossyScale.y) <= 0.0001f) ? 0f : (1f / lossyScale.y));
				lossyScale.z = ((Mathf.Abs(lossyScale.z) <= 0.0001f) ? 0f : (1f / lossyScale.z));
				localScale.Scale(lossyScale);
				transform2.localScale = localScale;
			}
		}

		protected void OnDestroy()
		{
		}
	}
}
