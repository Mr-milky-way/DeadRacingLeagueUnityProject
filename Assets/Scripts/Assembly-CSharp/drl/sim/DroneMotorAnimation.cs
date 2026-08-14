using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class DroneMotorAnimation : ActivityBehaviour, IUpdateable
	{
		private static Shader m_opaque_shader;

		private static Shader m_alpha_shader;

		[SerializeField]
		private DroneMotor m_motor;

		private bool m_hasMotor;

		[Range(0f, 30000f)]
		public float rpm;

		[Range(0f, 1f)]
		public float speed = 1f;

		public Transform cap;

		public List<MeshRenderer> props;

		public MeshRenderer spinner;

		public Shader shaderPropOpaque;

		public Shader shaderPropAlpha;

		private float m_last_blend;

		private float m_current_rpm;

		private float m_next_rpm;

		private bool m_has_tintcolor;

		public DroneMotor motor
		{
			get
			{
				if (m_hasMotor)
				{
					return m_motor;
				}
				if ((bool)m_motor)
				{
					m_hasMotor = true;
					return m_motor;
				}
				m_motor = GetComponent<DroneMotor>();
				if ((bool)m_motor)
				{
					m_hasMotor = true;
					return m_motor;
				}
				return null;
			}
			set
			{
				m_motor = value;
				m_hasMotor = m_motor != null;
			}
		}

		public bool hasMotor => m_hasMotor;

		public bool ccw => motor.ccw;

		private float GetBlend(float p_rpm)
		{
			if (Time.timeScale < 0.9f)
			{
				return 0f;
			}
			return Mathf.Clamp01((p_rpm - 500f) / 20000f);
		}

		public void Build()
		{
			if (!m_opaque_shader)
			{
				m_opaque_shader = Shader.Find("DRL/Library/Drone");
			}
			if (!m_alpha_shader)
			{
				m_alpha_shader = Shader.Find("DRL/Library/Drone Alpha");
			}
			if ((bool)shaderPropOpaque)
			{
				m_opaque_shader = shaderPropOpaque;
			}
			if ((bool)shaderPropAlpha)
			{
				m_alpha_shader = shaderPropAlpha;
			}
			m_current_rpm = rpm;
			m_next_rpm = rpm;
			m_last_blend = GetBlend(rpm);
			DroneProp droneProp = (motor.hasProp ? motor.prop : GetComponentInChildren<DroneProp>());
			props = Hierarchy.FindAll<MeshRenderer>(droneProp.transform);
			for (int i = 0; i < props.Count; i++)
			{
				if (props[i].name.IndexOf("spinner") >= 0)
				{
					spinner = props[i];
					props.RemoveAt(i);
					break;
				}
			}
			if (!(cap == null))
			{
				return;
			}
			for (int j = 0; j < base.transform.childCount; j++)
			{
				if (base.transform.GetChild(j).name.EndsWith("cap"))
				{
					cap = base.transform.GetChild(j);
					break;
				}
			}
		}

		public void FadeSpeed(float p_speed, float p_duration = 0f)
		{
			if (p_duration <= 0f)
			{
				speed = p_speed;
			}
			else
			{
				Tween.Add(this, "speed", p_speed, p_duration, 0f, Cubic.Out);
			}
		}

		public void OnUpdate()
		{
			if (!spinner || !cap)
			{
				return;
			}
			float num = speed;
			float num2 = ((num < 0f) ? (-1f) : 1f);
			num = Mathf.Pow(Mathf.Abs(num), 0.4f) * num2;
			if (motor.attached && motor.drone.hasFc && !motor.drone.fc.external)
			{
				rpm = ((motor.rpm <= 0f) ? 0f : Mathf.Max(motor.rpm, motor.minRpm));
			}
			m_next_rpm = rpm * num;
			m_current_rpm = Mathf.Lerp(m_current_rpm, m_next_rpm, Mathf.Clamp(Time.deltaTime * 50f, 0.1f, 0.5f));
			float current_rpm = m_current_rpm;
			float blend = GetBlend(Mathf.Abs(current_rpm));
			if (Math.Abs(m_last_blend - blend) > 0.01f)
			{
				m_last_blend = blend;
				Material sharedMaterial = spinner.sharedMaterial;
				if ((bool)sharedMaterial && sharedMaterial.HasProperty("_TintColor"))
				{
					Color color = sharedMaterial.GetColor("_TintColor");
					color.a = blend;
					sharedMaterial.SetColor("_TintColor", color);
				}
				for (int i = 0; i < props.Count; i++)
				{
					sharedMaterial = props[i].sharedMaterial;
					if ((bool)sharedMaterial)
					{
						Shader shader = ((blend <= 0f) ? m_opaque_shader : m_alpha_shader);
						if (shader != sharedMaterial.shader)
						{
							sharedMaterial.shader = shader;
						}
						sharedMaterial.SetFloat("_Alpha", 1f - blend);
					}
				}
			}
			float num3 = current_rpm * 0.01666666f;
			spinner.enabled = blend > 0f;
			for (int j = 0; j < props.Count; j++)
			{
				if ((bool)props[j])
				{
					props[j].enabled = blend < 1f;
				}
			}
			num2 = ((num3 < 0f) ? (-1f) : 1f);
			Vector3 vector = Vector3.up * num3 * 360f * Time.deltaTime;
			Vector3 vector2 = Vector3.up * num2 * 5f * 360f * Time.deltaTime;
			cap.localEulerAngles += (ccw ? (-vector) : vector);
			if (blend > 0f)
			{
				spinner.transform.localEulerAngles += (ccw ? (-(vector2 - vector)) : (vector2 - vector));
			}
		}

		public void ForceUpdate(bool p_immediate = false)
		{
			m_last_blend = -1f;
			if (p_immediate)
			{
				OnUpdate();
			}
		}

		public void ForceShader(bool p_alpha = false)
		{
			Shader shader = (p_alpha ? m_alpha_shader : m_opaque_shader);
			for (int i = 0; i < props.Count; i++)
			{
				Material sharedMaterial = props[i].sharedMaterial;
				if ((bool)sharedMaterial && shader != sharedMaterial.shader)
				{
					sharedMaterial.shader = shader;
				}
			}
		}

		public void ForceStop()
		{
			speed = 0f;
			rpm = 0f;
			m_next_rpm = 0f;
			m_current_rpm = 0f;
			m_last_blend = -1f;
			OnUpdate();
			spinner.enabled = false;
			for (int i = 0; i < props.Count; i++)
			{
				props[i].enabled = true;
				props[i].sharedMaterial.shader = m_opaque_shader;
			}
		}
	}
}
