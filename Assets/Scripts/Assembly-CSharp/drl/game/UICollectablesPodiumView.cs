using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class UICollectablesPodiumView : UIScreenView
	{
		private const int MAX_CARDS = 3;

		public RaceController race;

		public UICardPodiumView m_cardTemplate;

		private UICardPodiumView[] m_card = new UICardPodiumView[3];

		public RectTransform m_cardAnchorsContainerRect;

		public Vector2 m_cardAnchorsPositionOffset;

		public RectTransform[] m_cardAnchorRect = new RectTransform[3];

		public GameObject[] m_droneAnchor = new GameObject[3];

		public GameObject[] m_droneAttachObj = new GameObject[3];

		public float m_droneAttachObjLocalYOnScreen;

		public float m_droneAttachObjLocalYOffScreen = -1f;

		public Camera m_uiCamera;

		public RawImage m_uiCameraBackgroundImage;

		public float m_delayCardMove = 0.3f;

		public float m_delayCardShow = 0.1f;

		public float m_delayDroneMove = 0.1f;

		public GameObject promo;

		private Canvas m_canvas;

		private bool m_initialized;

		private int m_podiumPlacesCount;

		private Drone[] m_drone = new Drone[3];

		public float m_droneAttachObjLocalY0
		{
			get
			{
				return m_droneAttachObj[0].transform.localPosition.y;
			}
			set
			{
				Vector3 localPosition = m_droneAttachObj[0].transform.localPosition;
				localPosition.y = value;
				m_droneAttachObj[0].transform.localPosition = localPosition;
			}
		}

		public float m_droneAttachObjLocalY1
		{
			get
			{
				return m_droneAttachObj[1].transform.localPosition.y;
			}
			set
			{
				Vector3 localPosition = m_droneAttachObj[1].transform.localPosition;
				localPosition.y = value;
				m_droneAttachObj[1].transform.localPosition = localPosition;
			}
		}

		public float m_droneAttachObjLocalY2
		{
			get
			{
				return m_droneAttachObj[2].transform.localPosition.y;
			}
			set
			{
				Vector3 localPosition = m_droneAttachObj[2].transform.localPosition;
				localPosition.y = value;
				m_droneAttachObj[2].transform.localPosition = localPosition;
			}
		}

		public void Init()
		{
			if (!m_initialized)
			{
				for (int i = 0; i < 3; i++)
				{
					GameObject gameObject = Object.Instantiate(m_cardTemplate.gameObject, m_cardAnchorRect[i].gameObject.transform);
					m_card[i] = gameObject.GetComponent<UICardPodiumView>();
				}
				m_initialized = true;
			}
		}

		public void SetPromoEnabled(bool p_flag)
		{
			if ((bool)promo)
			{
				promo.SetActive(p_flag);
			}
		}

		public void Set(int p_id, string p_name, Texture p_photo, Color p_color, string p_droneRigData)
		{
			if (!m_initialized)
			{
				Init();
			}
			m_podiumPlacesCount = p_id + 1;
			if (m_podiumPlacesCount > 3)
			{
				m_podiumPlacesCount = 3;
			}
			UICardPodiumView uICardPodiumView = m_card[p_id];
			if (uICardPodiumView == null)
			{
				return;
			}
			uICardPodiumView.profileName = p_name;
			uICardPodiumView.color = p_color;
			if ((bool)p_photo)
			{
				uICardPodiumView.photo = p_photo;
			}
			Transform p_parent = m_droneAttachObj[p_id].transform;
			if (m_drone[p_id] != null)
			{
				m_drone[p_id].Destroy();
				m_drone[p_id] = null;
			}
			DroneRigData p_rig = DroneRigData.FromJson(p_droneRigData);
			Drone drone = base.app.model.storage.factory.InstantiateDummy(p_rig, p_parent);
			drone.transform.localPosition = Vector3.zero;
			drone.transform.localRotation = Quaternion.identity;
			drone.transform.localScale = Vector3.one;
			for (int i = 0; i < drone.body.frame.escs.Count; i++)
			{
				DroneESC droneESC = drone.body.frame.escs[i];
				if (droneESC != null && droneESC.motor != null && droneESC.motor.animation != null)
				{
					droneESC.motor.animation.ForceShader();
				}
			}
			m_drone[p_id] = drone;
		}

		public void Clear()
		{
			m_podiumPlacesCount = 0;
		}

		public void Show(float p_delay)
		{
			ValidateAndRecreateRenderTexture();
			if (m_podiumPlacesCount == 0)
			{
				Debug.LogError("UIRacePodiumView: no podium places configured, `Set` funtion hasn't been called!");
			}
			if (!m_initialized)
			{
				Init();
			}
			float num = p_delay;
			for (int i = 0; i < 3; i++)
			{
				bool active = i < m_podiumPlacesCount;
				m_droneAnchor[i].gameObject.SetActive(active);
				m_cardAnchorRect[i].gameObject.SetActive(active);
			}
			for (int j = 0; j < m_podiumPlacesCount; j++)
			{
				MoveInCard(j, num);
				num += m_delayCardMove;
				MoveInDrone(j, num);
				num += m_delayDroneMove;
				ShowCard(j, num);
				num += m_delayCardShow;
			}
		}

		private void ValidateAndRecreateRenderTexture()
		{
			Camera uiCamera = m_uiCamera;
			int num = Mathf.Max(Screen.width, 2);
			int num2 = Mathf.Max(Screen.height, 2);
			bool flag = true;
			bool num3 = (bool)uiCamera && uiCamera.allowHDR;
			RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGBHalf;
			RenderTextureFormat renderTextureFormat2 = (num3 ? renderTextureFormat : RenderTextureFormat.ARGB32);
			RenderTexture renderTexture = uiCamera.targetTexture;
			if ((bool)renderTexture && renderTexture.width == num && renderTexture.height == num2 && renderTexture.format == renderTextureFormat2)
			{
				flag = false;
			}
			if (flag)
			{
				uiCamera.targetTexture = null;
				if ((bool)renderTexture)
				{
					Object.Destroy(renderTexture);
					renderTexture = null;
				}
				renderTexture = new RenderTexture(num, num2, 16, renderTextureFormat2);
				renderTexture.useMipMap = false;
				renderTexture.antiAliasing = 8;
			}
			uiCamera.targetTexture = renderTexture;
			m_uiCameraBackgroundImage.texture = renderTexture;
		}

		public void Hide(float p_delay)
		{
			float num = p_delay;
			for (int num2 = m_podiumPlacesCount - 1; num2 >= 0; num2--)
			{
				MoveOutDrone(num2, num);
				num += 0.1f;
				HideCard(num2, num);
				num += 0.1f;
				MoveOutCard(num2, num);
			}
		}

		public void ShowCard(int p_cardIndex, float p_delay)
		{
			if ((bool)m_card[p_cardIndex])
			{
				m_card[p_cardIndex].Show(p_delay);
			}
		}

		public void HideCard(int p_cardIndex, float p_delay)
		{
			if ((bool)m_card[p_cardIndex])
			{
				m_card[p_cardIndex].Hide(p_delay);
			}
		}

		public void MoveInCard(int p_cardIndex, float p_delay)
		{
			if ((bool)m_card[p_cardIndex])
			{
				m_card[p_cardIndex].MoveIn(p_delay);
			}
		}

		public void MoveOutCard(int p_cardIndex, float p_delay)
		{
			if ((bool)m_card[p_cardIndex])
			{
				m_card[p_cardIndex].MoveOut(p_delay);
			}
		}

		public void MoveDrone(int p_id, float p_y, float p_delay)
		{
			Tween.Kill(this, "m_droneAttachObjLocalY" + p_id);
			Tween.Add(this, "m_droneAttachObjLocalY" + p_id, p_y, 0.8f, p_delay, Cubic.Out);
		}

		public void MoveInDrone(int p_id, float p_delay)
		{
			if (p_id == 0)
			{
				m_droneAttachObjLocalY0 = m_droneAttachObjLocalYOffScreen;
			}
			if (p_id == 1)
			{
				m_droneAttachObjLocalY1 = m_droneAttachObjLocalYOffScreen;
			}
			if (p_id == 2)
			{
				m_droneAttachObjLocalY2 = m_droneAttachObjLocalYOffScreen;
			}
			MoveDrone(p_id, m_droneAttachObjLocalYOnScreen, p_delay);
		}

		public void MoveOutDrone(int p_id, float p_delay)
		{
			if (p_id == 0)
			{
				m_droneAttachObjLocalY0 = m_droneAttachObjLocalYOnScreen;
			}
			if (p_id == 1)
			{
				m_droneAttachObjLocalY1 = m_droneAttachObjLocalYOnScreen;
			}
			if (p_id == 2)
			{
				m_droneAttachObjLocalY2 = m_droneAttachObjLocalYOnScreen;
			}
			MoveDrone(p_id, m_droneAttachObjLocalYOffScreen, p_delay);
		}

		private void UpdateCardAnchorPositions()
		{
			Camera uiCamera = m_uiCamera;
			if (!uiCamera)
			{
				return;
			}
			RectTransform cardAnchorsContainerRect = m_cardAnchorsContainerRect;
			Vector3 position = uiCamera.transform.position;
			if (!m_canvas)
			{
				m_canvas = Hierarchy.FindReverse<Canvas>(base.transform);
			}
			for (int i = 0; i < 3; i++)
			{
				uiCamera = m_uiCamera;
				position = uiCamera.transform.position;
				Vector3 position2 = m_droneAnchor[i].transform.GetChild(0).position;
				float num = Vector3.Dot(position2 - position, uiCamera.transform.forward);
				Vector2 vector = uiCamera.WorldToViewportPoint(position2);
				if (num <= 0f)
				{
					Vector2 vector2 = vector - new Vector2(0.5f, 0.5f);
					vector2.Normalize();
					vector += vector2 * 2f;
				}
				vector.x = Mathf.Clamp01(vector.x);
				vector.y = Mathf.Clamp01(vector.y);
				if (num <= 0f)
				{
					vector.x = 1f - vector.x;
				}
				Vector2 scale = new Vector2(Screen.width, Screen.height);
				Vector2 screenPoint = vector;
				screenPoint.Scale(scale);
				if ((bool)m_canvas)
				{
					uiCamera = m_canvas.worldCamera;
					if (m_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
					{
						uiCamera = null;
					}
				}
				Vector2 localPoint = Vector2.zero;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(cardAnchorsContainerRect, screenPoint, uiCamera, out localPoint);
				localPoint += m_cardAnchorsPositionOffset;
				m_cardAnchorRect[i].anchoredPosition = localPoint;
			}
		}

		public void ToggleDroneCamera(bool p_enabled)
		{
			m_uiCamera.enabled = p_enabled;
		}

		private void Update()
		{
			if (base.gameObject.activeInHierarchy)
			{
				UpdateCardAnchorPositions();
			}
		}
	}
}
