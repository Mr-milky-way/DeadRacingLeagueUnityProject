using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	public class UIGarageCOGMarker : MonoBehaviour
	{
		public RectTransform cogMarker;

		public Camera viewerCamera;

		public RawImage backgroundImage;

		public bool active;

		private Transform m_com;

		public Transform centerOfMass
		{
			get
			{
				if (m_com == null)
				{
					GameObject gameObject = GameObject.Find("center-of-mass");
					if ((bool)gameObject)
					{
						m_com = gameObject.transform;
					}
				}
				return m_com;
			}
			set
			{
				m_com = value;
			}
		}

		private void Update()
		{
			if (active && (bool)centerOfMass)
			{
				Vector3 vector = viewerCamera.WorldToScreenPoint(centerOfMass.position);
				float x = vector.x / backgroundImage.uvRect.width - 1920f * (1f - backgroundImage.uvRect.width) / (2f * backgroundImage.uvRect.width);
				float y = vector.y / backgroundImage.uvRect.width;
				cogMarker.anchoredPosition = new Vector2(x, y);
			}
		}
	}
}
