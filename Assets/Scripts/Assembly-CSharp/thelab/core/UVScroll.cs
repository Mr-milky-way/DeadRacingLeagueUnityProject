using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class UVScroll : MonoBehaviour
	{
		public List<Renderer> targets;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_speed;

		public bool useTimescale = true;

		protected Vector2 p_target_speed;

		public bool pulsate = true;

		private float pulsePeriod = 1f;

		private float pulseTimer = 1f;

		private Vector2 m_pulseSpeed = new Vector2(0f, 3f);

		private float m_pulseStrength = 2f;

		private float m_pulseDuration = 0.2f;

		public Vector2 speed
		{
			get
			{
				return m_speed;
			}
			set
			{
				m_speed = (p_target_speed = value);
				Tween.Kill(this);
			}
		}

		protected void Awake()
		{
			if (targets == null)
			{
				targets = new List<Renderer>();
			}
			if (targets.Count <= 0)
			{
				Renderer component = GetComponent<Renderer>();
				if ((bool)component)
				{
					targets.Add(component);
				}
			}
			p_target_speed = speed;
		}

		public void Pulse(Vector2 p_speed, float p_strength, float p_duration)
		{
			Tween.Kill(this);
			float p_duration2 = ((p_strength <= 0f) ? 1f : (1f / p_strength));
			Tween.Add(this, "p_target_speed", p_speed, p_duration2, Cubic.Out);
			Tween.Add(this, "p_target_speed", m_speed, p_duration2, p_duration, Cubic.Out);
		}

		protected void Update()
		{
			if (!base.enabled || !base.gameObject.activeInHierarchy)
			{
				return;
			}
			Vector2 vector = p_target_speed;
			vector.y = 0f - vector.y;
			float num = (useTimescale ? Time.deltaTime : Time.unscaledDeltaTime);
			for (int i = 0; i < targets.Count; i++)
			{
				Renderer renderer = targets[i];
				if ((bool)renderer)
				{
					Material sharedMaterial = renderer.sharedMaterial;
					Vector2 mainTextureOffset = sharedMaterial.mainTextureOffset;
					Vector2 mainTextureOffset2 = mainTextureOffset + vector * num;
					if (!(Mathf.Abs(mainTextureOffset2.x - mainTextureOffset.x) < 0.01f) || !(Mathf.Abs(mainTextureOffset2.y - mainTextureOffset.y) < 0.01f))
					{
						sharedMaterial.mainTextureOffset = mainTextureOffset2;
					}
				}
			}
			if (pulsate)
			{
				if (pulseTimer > 0f)
				{
					pulseTimer -= Time.deltaTime;
					return;
				}
				Pulse(m_pulseSpeed, m_pulseStrength, m_pulseDuration);
				pulseTimer = pulsePeriod;
			}
		}
	}
}
