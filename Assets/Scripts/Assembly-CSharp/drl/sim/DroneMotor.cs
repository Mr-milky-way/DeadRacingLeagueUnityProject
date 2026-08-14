using System;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	[RequireComponent(typeof(DroneMotorSpec))]
	public class DroneMotor : DronePart
	{
		[SerializeField]
		private DroneESC m_esc;

		[NonSerialized]
		public float temperature;

		public float maxAmpere = 20f;

		[SerializeField]
		private DroneProp m_prop;

		private bool m_hasProp;

		[SerializeField]
		private DroneMotorAnimation m_animation;

		private bool m_hasAnimation;

		[SerializeField]
		private DroneMotorSpec m_spec;

		private bool m_hasSpec;

		public bool ccw;

		[Range(0f, 25.2f)]
		public float voltage;

		[Range(0f, 60f)]
		public float amperes;

		[Range(0f, 600f)]
		public float watts;

		[Range(0f, 40000f)]
		public float rpm;

		public float minRpm;

		public float torque;

		public float thrust;

		public float rpmAudio;

		private float m_rpmMax = -1f;

		private float m_rpmRatioSmooth;

		private float m_rpmSmooth;

		private FloatInterpolator m_interpolateRpm;

		private float m_interpolator_elapsed;

		public bool overrideRpm;

		public DroneESC esc
		{
			get
			{
				return m_esc;
			}
			set
			{
				m_esc = value;
			}
		}

		public DroneProp prop
		{
			get
			{
				if (m_hasProp)
				{
					return m_prop;
				}
				if ((bool)m_prop)
				{
					m_hasProp = true;
					return m_prop;
				}
				m_prop = Hierarchy.Find<DroneProp>(base.transform);
				if ((bool)m_prop)
				{
					m_hasProp = true;
					return m_prop;
				}
				return null;
			}
			set
			{
				m_prop = value;
				m_hasProp = m_prop != null;
			}
		}

		public bool hasProp => m_hasProp;

		public DroneMotorAnimation animation
		{
			get
			{
				if (m_hasAnimation)
				{
					return m_animation;
				}
				if ((bool)m_animation)
				{
					m_hasAnimation = true;
					return m_animation;
				}
				m_animation = Hierarchy.Find<DroneMotorAnimation>(base.transform);
				if ((bool)m_animation)
				{
					m_hasAnimation = true;
					return m_animation;
				}
				return null;
			}
			set
			{
				m_animation = value;
				m_hasAnimation = m_animation != null;
			}
		}

		public bool hasAnimation => m_hasAnimation;

		public DroneMotorSpec spec
		{
			get
			{
				if (m_hasSpec)
				{
					return m_spec;
				}
				if ((bool)m_spec)
				{
					m_hasSpec = true;
					return m_spec;
				}
				m_spec = GetComponent<DroneMotorSpec>();
				if ((bool)m_spec)
				{
					m_hasSpec = true;
					return m_spec;
				}
				return null;
			}
			set
			{
				m_spec = value;
				m_hasSpec = m_spec != null;
			}
		}

		public bool hasSpec => m_hasSpec;

		public float rps => rpm * 0.016666666f;

		public float thrustNewton => thrust * 0.001f * 9.80665f;

		public float rpmRatio
		{
			get
			{
				float num = rpmMax;
				float num2 = rpm;
				if (!(num <= 0f))
				{
					return Mathf.Clamp01(num2 / num);
				}
				return 0f;
			}
		}

		public float rpmAudioRatio
		{
			get
			{
				float num = rpmMax;
				float num2 = rpmAudio;
				if (!(num <= 0f))
				{
					return Mathf.Clamp01(num2 / num);
				}
				return 0f;
			}
		}

		public float rpmMax
		{
			get
			{
				if (spec.data == null)
				{
					return 0f;
				}
				if (!(m_rpmMax >= 0f))
				{
					return m_rpmMax = spec.data.GetMaxRPM();
				}
				return m_rpmMax;
			}
		}

		public float rpmRatioSmooth
		{
			get
			{
				float num = rpmMax;
				float num2 = rpmSmooth;
				m_rpmRatioSmooth = ((num <= 0f) ? 0f : Mathf.Clamp01(num2 / num));
				return m_rpmRatioSmooth;
			}
		}

		public float rpmSmooth => m_rpmSmooth;

		public FloatInterpolator interpolateRpm
		{
			get
			{
				if (m_interpolateRpm == null)
				{
					m_interpolateRpm = new FloatInterpolator(InterpolationType.Predictive);
					m_interpolateRpm.estimate.SetSampling(4, 0.5f);
					m_interpolateRpm.estimate.maxDeviation = 5000f;
				}
				return m_interpolateRpm;
			}
		}

		public void Build()
		{
			if ((bool)animation)
			{
				animation.Build();
			}
			spec.Build();
		}

		public void RefreshData()
		{
			if (spec != null && spec.data != null)
			{
				spec.data.RefreshMaximums();
			}
			m_rpmMax = -1f;
		}

		internal void Step(float p_dt)
		{
			float num = 0f;
			torque = watts * p_dt;
			num = ((spec.data == null) ? 0f : spec.data.rpm.Evaluate(watts));
			if (num < 0f)
			{
				num = 0f;
			}
			if (!overrideRpm)
			{
				float num2 = ((num > rpm) ? spec.data.spinupDelay : spec.data.spindownDelay);
				if (num2 <= Mathf.Epsilon)
				{
					rpm = num;
				}
				else
				{
					rpm = Mathf.MoveTowards(rpm, num, spec.data.GetMaxRPM() * p_dt / num2 * 3f);
				}
			}
			thrust = ((spec.data == null) ? 0f : spec.data.thrust.Evaluate(rpm));
			minRpm = 0f;
			m_interpolator_elapsed += p_dt;
			if (m_interpolator_elapsed >= 0.03f)
			{
				m_interpolator_elapsed = 0f;
			}
			m_rpmSmooth = rpm;
			float maxThrust = spec.data.GetMaxThrust();
			if (base.drone.physics.linearThrust)
			{
				thrust = Mathf.Clamp01(esc.input) * maxThrust;
			}
			if (base.drone.physics.thrust > 0f)
			{
				thrust *= base.drone.physics.thrust / maxThrust;
			}
			else if (spec.data.thrustScale > 0f)
			{
				thrust *= spec.data.thrustScale / maxThrust;
			}
			float num3 = spec.data.GetMaxWatts() * p_dt;
			if (base.drone.physics.realisticTorque)
			{
				torque = spec.data.torque.Evaluate(watts);
				num3 = spec.data.torque.Evaluate(spec.data.watts.Evaluate(spec.data.amperes.Evaluate(1f)));
			}
			if (base.drone.physics.linearTorque)
			{
				torque = Mathf.Clamp01(esc.input) * num3;
			}
			if (base.drone.physics.torque > 0f)
			{
				torque *= base.drone.physics.torque / num3;
			}
			if (ccw)
			{
				torque = 0f - torque;
			}
		}

		public void SetRPM(float p_rpm, float p_dt)
		{
			rpm = p_rpm;
			m_interpolator_elapsed += p_dt;
			if (m_interpolator_elapsed >= 0.03f)
			{
				m_rpmSmooth = interpolateRpm.Evaluate(rpm, Time.time);
				m_interpolator_elapsed = 0f;
			}
			m_rpmSmooth = rpm;
		}

		public override string GetPrefix()
		{
			return "M";
		}

		public void ForceStop()
		{
			rpm = 0f;
			rpmAudio = 0f;
			m_rpmSmooth = 0f;
			m_rpmRatioSmooth = 0f;
			if ((bool)animation)
			{
				animation.ForceStop();
			}
		}

		public void Unlock()
		{
			if ((bool)animation)
			{
				animation.speed = 1f;
			}
		}
	}
}
