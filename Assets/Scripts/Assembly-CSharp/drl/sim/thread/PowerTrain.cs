using System;
using UnityEngine;

namespace drl.sim.thread
{
	public class PowerTrain : MonoBehaviour
	{
		private float[] currentInput = new float[4];

		private float lastAvgSignal;

		private float d_machRatio;

		private float d_dragRatio;

		[HideInInspector]
		public float[] d_rpm = new float[4];

		[HideInInspector]
		public float[] d_ratio = new float[4];

		private Vector3[] calculatedForce = new Vector3[5];

		private float battery_ratio;

		private Vector3 thrust_dir;

		private float total_torque;

		private float true_airspeed;

		private DroneESC[] escs;

		public float maxThrust { get; private set; }

		public float maxTorque { get; private set; }

		public Vector3[] OnUpdate(float deltaTime, Drone m_drone, Vector3 m_unityDroneTransformUp, Vector3 m_dronePosition, Rigidbody m_rigidbody, DroneESC[] m_escs, DroneIntertial m_droneInertial, float m_groundEffectScale)
		{
			battery_ratio = 1f;
			total_torque = 0f;
			thrust_dir = m_unityDroneTransformUp;
			true_airspeed = m_droneInertial.VelocityY.magnitude * Mathf.Sign(Vector3.Dot(m_droneInertial.ActualVelocity, m_unityDroneTransformUp));
			m_drone.d_trueAirspeed = true_airspeed;
			escs = m_escs;
			GroundEffectCalculate();
			float m_batt_current = 0f;
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 0f;
			float num7 = m_drone.body.frame.batteries.Count;
			float num8 = ((num7 <= 0f) ? 0f : (1f / num7));
			for (int i = 0; i < m_drone.body.frame.batteries.Count; i++)
			{
				DroneBattery droneBattery = m_drone.body.frame.batteries[i];
				num += (m_drone.physics.batteryDrain ? droneBattery.voltage : droneBattery.max) * num8;
				num2 += droneBattery.max * num8;
				num3 += droneBattery.min * num8;
				num4 += droneBattery.resistance;
				num5 += (m_drone.physics.batteryDrain ? droneBattery.mah : droneBattery.capacity);
				num6 += droneBattery.capacity;
			}
			if (!m_drone.physics.batterySag)
			{
				num4 = 0.0001f;
			}
			battery_ratio = ((num6 > 0f) ? (num5 / num6) : 0f);
			float battery_power_scale = ((num2 > 0f) ? (num / num2) : 0f) * ((battery_ratio > 0.1f) ? 1f : Mathf.Clamp01(1f - Mathf.Pow(0.2f, 100f * battery_ratio)));
			float num9 = 0f;
			float num10 = 0f;
			float num11 = 0f;
			bool flag = false;
			num = ((num7 <= 0f) ? 16.8f : num);
			m_drone.fc.sensor.electrical.m_voltageMax = num2;
			m_drone.fc.sensor.electrical.m_voltageMin = num3;
			m_drone.fc.sensor.electrical.m_voltageAvailable = num;
			m_drone.fc.sensor.electrical.m_remainingCharge = num5;
			m_drone.fc.sensor.electrical.m_totalCapacity = num6;
			ResetCurrentThrust(m_drone);
			float m_averageSignal = 0f;
			CalculateAverageSignal(m_drone, ref m_averageSignal, battery_power_scale);
			float num12 = num;
			BatterySagCalculation(m_drone, ref num, ref lastAvgSignal, m_averageSignal, num12, deltaTime);
			SampleAmpereDemand(ref m_batt_current);
			float num13 = m_batt_current * num4 * 0.001f;
			num -= num13 * 0.25f;
			m_drone.fc.sensor.electrical.m_voltage = num;
			float battery_voltage_drop = ((num12 > 0f) ? Mathf.Clamp01(num / num12) : 0f);
			ApplyBatterySag(m_drone, battery_voltage_drop, ref m_averageSignal);
			AdvancedPropLimitCalculation(m_drone, m_droneInertial);
			m_drone.rigidbody.CheckMotorCount(escs.Length);
			for (int j = 0; j < escs.Length; j++)
			{
				DroneESC droneESC = escs[j];
				DroneMotorSpec.BenchData benchData = (droneESC.hasMotor ? droneESC.motor.spec.data : null);
				droneESC.voltage = num;
				droneESC.motor.voltage = droneESC.voltage;
				float a = num5 * 1E-06f * droneESC.motor.voltage;
				droneESC.motor.watts = benchData?.watts.Evaluate(droneESC.amperes) ?? 0f;
				float num14 = deltaTime * 0.000277777f;
				float b = droneESC.motor.watts * num14;
				b = Mathf.Min(a, b);
				float num15 = ((num14 <= 0f) ? 0f : (b / num14));
				float num16 = 1f;
				if (m_drone.physics.batterySag)
				{
					num16 = ((droneESC.motor.watts <= 0f) ? 0f : Mathf.Clamp01(num15 / droneESC.motor.watts));
				}
				droneESC.motor.watts = num15;
				UpdateMotorPhysics(droneESC, m_drone, deltaTime, ref currentInput, j, benchData, num16);
				float num17 = droneESC.motor.thrustNewton;
				maxThrust = benchData.thrust.Evaluate(benchData.rpm.Evaluate(benchData.watts.Evaluate(benchData.amperes.Evaluate(1f))));
				if (m_drone.physics.linearThrust)
				{
					num17 = Mathf.Clamp01(currentInput[j]) * maxThrust * 0.001f * 9.80665f;
					if (m_drone.fc.batterySag)
					{
						num17 *= num16;
					}
				}
				if (m_drone.physics.thrust > 0f)
				{
					num17 *= m_drone.physics.thrust / maxThrust;
				}
				float num18 = Mathf.Abs(droneESC.motor.torque);
				maxTorque = benchData.watts.Evaluate(benchData.amperes.Evaluate(1f)) * deltaTime;
				if (m_drone.physics.linearTorque)
				{
					maxTorque = benchData.torque.Evaluate(benchData.watts.Evaluate(benchData.amperes.Evaluate(1f)));
					num18 = Mathf.Clamp01(currentInput[j]) * maxTorque;
					if (m_drone.fc.batterySag)
					{
						num18 *= num16;
					}
				}
				if (m_drone.physics.torque > 0f)
				{
					num18 *= m_drone.physics.torque / maxTorque;
				}
				if (droneESC.motor.ccw)
				{
					num18 = 0f - num18;
				}
				float num19;
				if ((num19 = m_drone.physics.efficiency) <= 0f)
				{
					flag = true;
					float num20 = droneESC.motor.prop.AdvanceRatio(droneESC.motor.rpm, true_airspeed);
					num19 = droneESC.motor.prop.Boost(droneESC.motor.rpm, true_airspeed);
					num10 += num19;
					m_drone.d_advanceRatio += num20;
				}
				float num21 = num17 * num19;
				m_drone.d_torqueBoost = m_drone.physics.torqueBoostWeight * Mathf.Clamp01((10f - true_airspeed) / 80f) * Mathf.Clamp01(m_drone.physics.torque / 35f) * m_drone.fc.rawSignal.throttle;
				if (m_drone.physics.groundEffectStrength > 0f)
				{
					num9 += num21 * (m_groundEffectScale - 1f);
				}
				Vector3 zero = Vector3.zero;
				zero += thrust_dir * (flag ? num17 : num21);
				num11 += num17;
				calculatedForce[j] = zero;
				total_torque += num18;
				m_drone.d_currentThrust += num21;
				m_drone.rigidbody.currentThrust[j] = num21;
				m_drone.rigidbody.currentMotorThrust[j] = num17 * 1000f / 9.80665f;
				m_drone.d_dynamicDragWeight = droneESC.motor.rpmRatio;
			}
			calculatedForce[4] = new Vector3(0f, 0f - total_torque, 0f);
			if (!m_drone.fc.HasPower())
			{
				for (int k = 0; k < escs.Length; k++)
				{
					escs[k].motor.SetRPM(0f, deltaTime);
					escs[k].motor.rpm = 0f;
				}
			}
			m_drone.d_currentThrust *= 101.97162f;
			m_drone.d_advanceRatio /= 4f;
			m_drone.d_propEfficiency = (m_drone.physics.arcadePhysics ? 0.85f : ((m_drone.physics.efficiency > 0f) ? m_drone.physics.efficiency : escs[0].motor.prop.EvaluateEfficiencyCurve(m_drone.d_advanceRatio)));
			m_drone.d_dynamicDragWeight /= 4f;
			return calculatedForce;
		}

		private void GroundEffectCalculate()
		{
		}

		private void CalculateAverageSignal(Drone m_drone, ref float m_averageSignal, float m_battery_power_scale)
		{
			if (!m_drone.fc.HasPower())
			{
				for (int i = 0; i < escs.Length; i++)
				{
					m_drone.body.frame.escs[i].input = 0f;
				}
			}
			else
			{
				for (int j = 0; j < escs.Length; j++)
				{
					if (m_drone.physics.batterySag)
					{
						m_drone.body.frame.escs[j].input += m_battery_power_scale;
					}
					m_averageSignal += m_drone.body.frame.escs[j].input;
				}
			}
			m_averageSignal /= escs.Length;
		}

		private void BatterySagCalculation(Drone m_drone, ref float m_batt_voltage, ref float m_lastAvgSignal, float m_avgSignal, float m_maxVoltage, float m_deltaTime)
		{
			if (m_drone.physics.batterySag)
			{
				m_batt_voltage = Mathf.Clamp(m_batt_voltage + 0.03f * m_batt_voltage * (lastAvgSignal - m_avgSignal), 0f, m_maxVoltage);
				m_lastAvgSignal = Mathf.Lerp(lastAvgSignal, m_avgSignal, m_deltaTime * 20f);
			}
		}

		private void SampleAmpereDemand(ref float m_batt_current)
		{
			for (int i = 0; i < escs.Length; i++)
			{
				DroneESC droneESC = escs[i];
				droneESC.motor.esc = droneESC;
				droneESC.amperes = (droneESC.hasMotor ? droneESC.motor.spec.data : null)?.amperes.Evaluate(droneESC.input) ?? 0f;
				droneESC.motor.amperes = droneESC.amperes;
				m_batt_current += droneESC.amperes;
			}
		}

		private void ApplyBatterySag(Drone m_drone, float m_battery_voltage_drop, ref float m_avgSignal)
		{
			if (m_drone.physics.batterySag)
			{
				for (int i = 0; i < escs.Length; i++)
				{
					escs[i].input *= m_battery_voltage_drop;
				}
				m_avgSignal *= m_battery_voltage_drop;
			}
		}

		private void AdvancedPropLimitCalculation(Drone m_drone, DroneIntertial m_droneInertial)
		{
			float num = 0f;
			for (int i = 0; i < escs.Length; i++)
			{
				num += escs[i].motor.rpm;
			}
			num /= (float)escs.Length;
			d_dragRatio = 0f;
			d_machRatio = 0f;
			if (!m_drone.physics.advancedPropLimits)
			{
				return;
			}
			float speed = m_droneInertial.speed;
			float[] array = new float[4];
			for (int j = 0; j < escs.Length; j++)
			{
				array[j] = (float)Math.PI * (escs[j].motor.prop.diameter * 0.0254f) * (num / 60f);
				if (m_drone.physics.maxTipSpeed > 0f && Mathf.Sqrt(array[j] * array[j] + speed * speed) / 343f > m_drone.physics.maxTipSpeed * 0.9f)
				{
					float max = (d_machRatio = Mathf.Clamp01((343f * m_drone.physics.maxTipSpeed - speed) / ((float)Math.PI * (escs[j].motor.prop.diameter * 0.0254f)) * 60f / escs[j].motor.spec.data.GetMaxRPM()));
					escs[j].input = Mathf.Clamp(escs[j].input, 0f, max);
				}
			}
			if (!(m_drone.physics.propDragFactor > 0f))
			{
				return;
			}
			for (int k = 0; k < escs.Length; k++)
			{
				float num2 = escs[k].motor.spec.data.GetMaxTorque() / (escs[k].motor.prop.diameter * 0.0127f);
				float num3 = Mathf.Sqrt(array[k] * array[k] + speed * speed);
				if (0.5f * m_drone.physics.airDensity * (num3 * num3) * m_drone.physics.propDragFactor * (escs[k].motor.prop.diameter * 0.0254f * 0.005f) > 0f)
				{
					float max2 = (d_dragRatio = Mathf.Clamp01((Mathf.Sqrt(num2 * 2f / (m_drone.physics.airDensity * m_drone.physics.propDragFactor * (escs[k].motor.prop.diameter * 0.0254f * 0.005f))) - speed) / ((float)Math.PI * (escs[k].motor.prop.diameter * 0.0254f)) * 60f / escs[k].motor.spec.data.GetMaxRPM()));
					escs[k].input = Mathf.Clamp(escs[k].input, 0f, max2);
				}
			}
		}

		private void ResetCurrentThrust(Drone m_drone)
		{
			if (m_drone.rigidbody.currentThrust == null || m_drone.rigidbody.currentThrust.Length != escs.Length)
			{
				m_drone.rigidbody.currentThrust = new float[escs.Length];
				return;
			}
			for (int i = 0; i < escs.Length; i++)
			{
				m_drone.rigidbody.currentThrust[i] = 0f;
			}
		}

		private void UpdateMotorPhysics(DroneESC m_esc, Drone m_drone, float m_deltaTime, ref float[] currentInput, int i, DroneMotorSpec.BenchData m_mbd, float m_wattDrop)
		{
			m_esc.motor.overrideRpm = m_drone.physics.overrideSpinup;
			m_esc.motor.Step(m_deltaTime);
			if (m_drone.physics.overrideSpinup)
			{
				float num = ((m_drone.physics.spindownTime > 0f) ? (m_deltaTime / m_drone.physics.spindownTime) : 1f);
				if (m_esc.input > currentInput[i])
				{
					num = ((m_drone.physics.spinupTime > 0f) ? (m_deltaTime / m_drone.physics.spinupTime) : 1f);
				}
				currentInput[i] = Mathf.MoveTowards(currentInput[i], m_esc.input, num * 3f);
			}
			else
			{
				currentInput[i] = m_esc.input;
			}
			float rpm = m_mbd.rpm.Evaluate(m_mbd.watts.Evaluate(m_mbd.amperes.Evaluate(currentInput[i]))) * m_wattDrop;
			m_esc.motor.rpm = rpm;
			d_rpm[i] += m_esc.motor.rpm;
			d_ratio[i] += m_esc.motor.rpmRatio;
		}
	}
}
