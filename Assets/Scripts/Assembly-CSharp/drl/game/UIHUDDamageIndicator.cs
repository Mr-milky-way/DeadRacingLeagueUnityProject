using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIHUDDamageIndicator : View<DRLApp>
	{
		public enum HUDDamageLayer
		{
			PropUpperLeft = 0,
			PropUpperRight = 1,
			PropLowerLeft = 2,
			PropLowerRight = 3,
			Body = 4
		}

		public FadeComponent fade;

		public Image bodyLayer;

		public List<Image> propsLayers;

		public AnimationCurve pulseCurve;

		private Dictionary<HUDDamageLayer, Activity> m_animationActivities = new Dictionary<HUDDamageLayer, Activity>();

		public Color damageYellow;

		public Color damageOrange;

		public Color damageRed;

		private float m_bodyDamage;

		private float[] m_propsDamage = new float[4];

		[Header("Camera damage shake test UI:")]
		public GameObject shakeTestContainer;

		public Text intensityLabel;

		public Text durationLabel;

		public Slider intensitySlider;

		public Slider durationSlider;

		public bool isVisible => fade.alpha > 0.2f;

		private void Start()
		{
		}

		public void RefreshShake()
		{
			intensityLabel.text = "SHAKE INTESITY: " + intensitySlider.value;
			durationLabel.text = "SHAKE DURATION: " + durationSlider.value;
			DroneCamera camera = base.app.model.game.camera;
			if (camera != null)
			{
				camera.fx.shake.intensityMultiplier = intensitySlider.value;
				camera.fx.shake.duration = durationSlider.value;
			}
		}

		public void Show(bool p_flag)
		{
			if (p_flag)
			{
				fade.FadeIn(0.2f);
			}
			else
			{
				fade.FadeOut(0f);
			}
			this.TimerRunOnce(delegate
			{
				Notify("chat.toggle.height");
			}, 0.25f);
		}

		private void AnimateLayer(HUDDamageLayer p_layer, float p_damage, float p_duration = 0.3f)
		{
			if (m_animationActivities.ContainsKey(p_layer) && m_animationActivities[p_layer] != null)
			{
				m_animationActivities[p_layer].Stop();
				m_animationActivities[p_layer] = null;
			}
			Image layer;
			if (p_layer <= HUDDamageLayer.PropLowerRight)
			{
				layer = propsLayers[(int)p_layer];
			}
			else
			{
				layer = bodyLayer;
			}
			if (layer == null)
			{
				return;
			}
			if (p_damage < 0.085f)
			{
				layer.color = damageYellow;
			}
			else if (p_damage < 0.185f)
			{
				layer.color = damageOrange;
			}
			else
			{
				layer.color = damageRed;
			}
			float dt = 0f;
			Color tc = layer.color;
			if (pulseCurve != null && pulseCurve.keys.Length != 0)
			{
				float curve_duration = pulseCurve.keys[pulseCurve.keys.Length - 1].time;
				Activity value = this.ActivityRun(delegate
				{
					float time = dt / p_duration * curve_duration;
					tc.a = pulseCurve.Evaluate(time);
					layer.color = tc;
					dt += Time.deltaTime;
				}, p_duration, 0f);
				if (m_animationActivities.ContainsKey(p_layer))
				{
					m_animationActivities[p_layer] = value;
				}
				else
				{
					m_animationActivities.Add(p_layer, value);
				}
			}
		}

		public void SetDamage(float p_bodyDamage, float[] p_propsDamage)
		{
			if (p_bodyDamage > 0f)
			{
				m_bodyDamage += p_bodyDamage;
				AnimateLayer(HUDDamageLayer.Body, m_bodyDamage);
			}
			if (p_propsDamage == null || p_propsDamage.Length < 4)
			{
				return;
			}
			for (int i = 0; i < p_propsDamage.Length; i++)
			{
				if (!(p_propsDamage[i] <= 0f))
				{
					m_propsDamage[i] += p_propsDamage[i];
					AnimateLayer((HUDDamageLayer)i, m_propsDamage[i]);
				}
			}
		}

		public void SetDamageSpectator(float p_bodyDamage, float[] p_propsDamage)
		{
			Reset();
			if (p_bodyDamage > 0f)
			{
				AnimateLayer(HUDDamageLayer.Body, p_bodyDamage);
			}
			if (p_propsDamage == null || p_propsDamage.Length < 4)
			{
				return;
			}
			for (int i = 0; i < p_propsDamage.Length; i++)
			{
				if (!(p_propsDamage[i] <= 0f))
				{
					AnimateLayer((HUDDamageLayer)i, p_propsDamage[i]);
				}
			}
		}

		public void SetCrash()
		{
			for (int i = 0; i < 5; i++)
			{
				AnimateLayer((HUDDamageLayer)i, 1f);
			}
		}

		public void Reset()
		{
			bodyLayer.color = Color.clear;
			m_bodyDamage = 0f;
			foreach (KeyValuePair<HUDDamageLayer, Activity> animationActivity in m_animationActivities)
			{
				animationActivity.Value?.Stop();
			}
			for (int i = 0; i < propsLayers.Count; i++)
			{
				propsLayers[i].color = Color.clear;
				m_propsDamage[i] = 0f;
			}
		}
	}
}
