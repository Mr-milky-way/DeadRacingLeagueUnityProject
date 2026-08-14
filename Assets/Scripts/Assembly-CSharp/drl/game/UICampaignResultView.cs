using System;
using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class UICampaignResultView : UIScreenView
	{
		public FadeComponent m_textAnchorFadeComp;

		public Text m_levelNameField;

		public Text m_totalTimeField;

		public GameObject m_rightExitButton;

		public UICardPodiumView m_cardTemplate;

		private UICardPodiumView m_card;

		public RectTransform m_cardAnchorContainerRect;

		public RectTransform m_droneAnchorContainerRect;

		public float m_droneAnchorOffsetX = 175f;

		public UIDroneOverlay m_droneOverlay;

		public DRLCampaign m_data;

		public RectTransform m_backgroundContainer;

		private bool m_initialized;

		private float m_totalTime;

		private float m_timer;

		public float droneAnchorX
		{
			get
			{
				return m_droneAnchorContainerRect.anchoredPosition.x;
			}
			set
			{
				Vector2 anchoredPosition = m_droneAnchorContainerRect.anchoredPosition;
				anchoredPosition.x = value;
				m_droneAnchorContainerRect.anchoredPosition = anchoredPosition;
			}
		}

		public float totalTimeField
		{
			get
			{
				return m_timer;
			}
			set
			{
				m_timer = value;
				m_totalTimeField.text = Format.SecondsToTime(m_timer, 2, p_use_ms: true);
			}
		}

		public void Init()
		{
			if (!m_initialized)
			{
				if ((bool)m_backgroundContainer)
				{
					m_backgroundContainer.gameObject.SetActive(!m_data.tournament);
				}
				GameObject gameObject = UnityEngine.Object.Instantiate(m_cardTemplate.gameObject, m_cardAnchorContainerRect.gameObject.transform);
				m_card = gameObject.GetComponent<UICardPodiumView>();
				RectTransform component = gameObject.GetComponent<RectTransform>();
				Vector2 pivot = component.pivot;
				pivot.y = 0.5f;
				component.pivot = pivot;
				m_card.gradientField.gameObject.SetActive(value: false);
				if ((bool)m_textAnchorFadeComp)
				{
					m_textAnchorFadeComp.FadeOut(0f);
				}
				if ((bool)m_droneOverlay)
				{
					m_droneOverlay.autoShow = false;
					m_droneOverlay.fade.alpha = 0f;
				}
				m_totalTime = 0f;
				m_initialized = true;
			}
		}

		public void Set(string p_name, Texture p_photo, Color p_color, string p_labelText, float p_totalTime)
		{
			if (!m_initialized)
			{
				Init();
			}
			UICardPodiumView card = m_card;
			if (card == null)
			{
				return;
			}
			card.profileName = p_name;
			card.color = p_color;
			m_totalTime = p_totalTime;
			if (p_labelText != "")
			{
				m_levelNameField.text = p_labelText;
			}
			if ((bool)p_photo)
			{
				card.photo = p_photo;
			}
			Activity.Run((Func<bool>)delegate
			{
				if (!m_droneOverlay.drone || !m_droneOverlay.drone.ready)
				{
					return true;
				}
				Activity.RunOnce(StopDroneProps, 1f / 12f);
				m_droneOverlay.drone.renderer.playerColor = p_color;
				return false;
			}, 0f, false);
		}

		private void StopDroneProps()
		{
			if ((bool)m_droneOverlay.drone && m_droneOverlay.drone.ready)
			{
				DroneFrame frame = m_droneOverlay.drone.body.frame;
				m_droneOverlay.drone.fc.armed = false;
				m_droneOverlay.drone.rigidbody.isKinematic = true;
				for (int i = 0; i < frame.escs.Count; i++)
				{
					frame.escs[i].motor.animation.rpm = 0f;
				}
			}
		}

		public void Show(float p_delay)
		{
			if (!m_initialized)
			{
				Init();
			}
			float num = p_delay;
			m_textAnchorFadeComp.FadeOut(0f);
			m_textAnchorFadeComp.FadeIn(0.7f, 0.1f);
			m_card.ShowFadeIn(num);
			m_card.ScaleDown(num);
			num += 1f;
			m_card.MoveOutX(num);
			if ((bool)m_droneOverlay)
			{
				StopDroneProps();
				m_droneOverlay.fade.alpha = 0f;
				m_droneOverlay.fade.FadeIn(0.6f, num, Cubic.InOut);
				droneAnchorX = 0f;
				Tween.Kill(this, "droneAnchorX");
				Tween.Add(this, "droneAnchorX", m_droneAnchorOffsetX, 0.6f, num, Cubic.Out);
				num += 0.8f;
			}
			totalTimeField = 0f;
			Tween.Kill(this, "totalTimeField");
			Tween.Add(this, "totalTimeField", m_totalTime, 0.8f, num, Cubic.Out);
		}

		public void Hide(float p_delay)
		{
			m_textAnchorFadeComp.FadeOut();
			m_card.HideFadeOut(p_delay, 0.4f);
			m_card.MoveInX(p_delay);
			if ((bool)m_droneOverlay)
			{
				m_droneOverlay.fade.FadeIn(0f);
				m_droneOverlay.fade.FadeOut(0.3f, p_delay, Cubic.Out);
				droneAnchorX = m_droneAnchorOffsetX;
				Tween.Kill(this, "droneAnchorX");
				Tween.Add(this, "droneAnchorX", 0f, 0.4f, p_delay, Cubic.Out);
			}
		}

		public void ToggleExitButton(bool p_visible)
		{
			m_rightExitButton.SetActive(p_visible);
		}

		public void Clear()
		{
		}

		private void Update()
		{
			_ = base.gameObject.activeInHierarchy;
		}
	}
}
