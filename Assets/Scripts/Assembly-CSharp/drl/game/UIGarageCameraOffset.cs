using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class UIGarageCameraOffset : MonoBehaviour
	{
		public enum aspects
		{
			a169 = 0,
			a1610 = 1,
			a43 = 2,
			a54 = 3,
			a32 = 4
		}

		private Vector2 inInterval = new Vector2(0.3f, 0.45f);

		private Vector2 inVal169 = new Vector2(0.0151f, 0.0225f);

		private Vector2 inVal1610 = new Vector2(0.0137f, 0.02f);

		private Vector2 inVal43 = new Vector2(0.0111f, 0.0167f);

		private Vector2 inVal54 = new Vector2(0.0105f, 0.0155f);

		private Vector2 inVal32 = new Vector2(0.0128f, 0.019f);

		public Camera camera;

		public OrbitTransform orbitTransform;

		private float aspect169 = 1.7777778f;

		private float aspect1610 = 1.6f;

		private float aspect43 = 1.3333334f;

		private float aspect54 = 1.25f;

		private float aspect32 = 1.5f;

		private float m_aspect;

		private aspects m_currentAspect;

		public float aspect
		{
			get
			{
				return m_aspect;
			}
			set
			{
				m_aspect = value;
				if (m_aspect < aspect169 + 0.01f && m_aspect > aspect169 - 0.01f)
				{
					m_currentAspect = aspects.a169;
				}
				if (m_aspect < aspect1610 + 0.01f && m_aspect > aspect1610 - 0.01f)
				{
					m_currentAspect = aspects.a1610;
				}
				if (m_aspect < aspect43 + 0.01f && m_aspect > aspect43 - 0.01f)
				{
					m_currentAspect = aspects.a43;
				}
				if (m_aspect < aspect54 + 0.01f && m_aspect > aspect54 - 0.01f)
				{
					m_currentAspect = aspects.a54;
				}
				if (m_aspect < aspect32 + 0.01f && m_aspect > aspect32 - 0.01f)
				{
					m_currentAspect = aspects.a32;
				}
			}
		}

		private void Update()
		{
			float t = Mathf.InverseLerp(inInterval.x, inInterval.y, orbitTransform.distance);
			float x = 0f;
			switch (m_currentAspect)
			{
			case aspects.a169:
				x = Mathf.Lerp(inVal169.x, inVal169.y, t);
				break;
			case aspects.a1610:
				x = Mathf.Lerp(inVal1610.x, inVal1610.y, t);
				break;
			case aspects.a43:
				x = Mathf.Lerp(inVal43.x, inVal43.y, t);
				break;
			case aspects.a54:
				x = Mathf.Lerp(inVal54.x, inVal54.y, t);
				break;
			case aspects.a32:
				x = Mathf.Lerp(inVal32.x, inVal32.y, t);
				break;
			}
			base.transform.localPosition = new Vector3(x, base.transform.localPosition.y, base.transform.localPosition.z);
		}
	}
}
