using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIHUDPhysicsController : Controller<DRLApp>
	{
		public enum Graph
		{
			Motors = 0,
			Throttle = 1,
			PitchRoll = 2,
			Yaw = 3,
			Efficiency = 4,
			Force = 5,
			Speed = 6,
			Electric = 7
		}

		private bool m_initialized;

		private bool m_showing;

		private float[] lastRPM = new float[4];

		private float[] lastRPMratio = new float[4];

		public UIHUDPhysicsView view => AssertLocal<UIHUDPhysicsView>("view");

		public FadeComponent fade => AssertLocal<FadeComponent>("fade");

		public bool isShowing => m_showing;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "settings.controller.disconnect":
			case "settings.controller.connect":
				view.RefreshNavigationTooltips();
				break;
			}
			if (!base.enabled || !base.gameObject.activeInHierarchy || p_event == null || !(p_event == "game.simulation.drone@ready"))
			{
				return;
			}
			Drone d = Reflection<object>.Get<Drone>(p_data, 0);
			if (!(d != null))
			{
				return;
			}
			d.d_topSpeed = -1f;
			Activity.RunOnce(delegate
			{
				if (d != null)
				{
					d.d_topSpeed = -1f;
				}
			}, 0.05f);
		}

		public void Show()
		{
			m_showing = true;
			fade.FadeIn(0.25f);
			RedrawGraphCurves();
			SingleColumn(p_flag: false);
		}

		public void Hide()
		{
			m_showing = false;
			fade.FadeOut(0.25f);
		}

		public void SingleColumn(bool p_flag)
		{
			if (p_flag)
			{
				view.containerGraphsRight.anchoredPosition = Vector2.zero;
				view.containerGraphsLeft.anchoredPosition = Vector2.zero;
				while (view.containerGraphsRight.childCount > 0)
				{
					view.containerGraphsRight.GetChild(0).SetParent(view.containerGraphsLeft);
				}
			}
			else
			{
				if (ActiveChildCount(view.containerGraphsLeft) <= 4)
				{
					return;
				}
				view.UpdateGraphsOffsets();
				List<Transform> list = new List<Transform>();
				for (int i = 0; i < view.containerGraphsLeft.childCount; i++)
				{
					if (view.containerGraphsLeft.GetChild(i).gameObject.activeSelf)
					{
						RectTransform component = view.containerGraphsLeft.GetChild(i).GetComponent<RectTransform>();
						if (component.sizeDelta.y - component.anchoredPosition.y - view.containerGraphsLeft.anchoredPosition.y > 1080f)
						{
							list.Add(view.containerGraphsLeft.GetChild(i));
						}
					}
				}
				for (int j = 0; j < list.Count; j++)
				{
					list[j].SetParent(view.containerGraphsRight);
				}
			}
		}

		private int ActiveChildCount(Transform p_transform)
		{
			int num = 0;
			for (int i = 0; i < p_transform.childCount; i++)
			{
				if (p_transform.GetChild(i).gameObject.activeSelf)
				{
					num++;
				}
			}
			return num;
		}

		public void Init()
		{
			if (!m_initialized)
			{
				m_initialized = true;
				view.graphMotors.showVoltage = false;
				view.graphMotors.showThrust = true;
				view.graphMotors.showTorque = true;
				view.graphMotors.showRpm = true;
				view.Align();
				RedrawGraphCurves(p_force: true);
			}
		}

		public void ToggleGraph(Graph p_graph, bool p_show)
		{
			switch (p_graph)
			{
			case Graph.Motors:
				view.graphMotors.gameObject.SetActive(p_show);
				break;
			case Graph.Throttle:
				view.graphThrottle.gameObject.SetActive(p_show);
				break;
			case Graph.PitchRoll:
				view.graphPitchRoll.gameObject.SetActive(p_show);
				break;
			case Graph.Yaw:
				view.graphYaw.gameObject.SetActive(p_show);
				break;
			case Graph.Efficiency:
				view.graphEfficiency.gameObject.SetActive(p_show);
				break;
			case Graph.Force:
				view.graphForce.gameObject.SetActive(p_show);
				break;
			case Graph.Speed:
				view.graphSpeed.gameObject.SetActive(p_show);
				break;
			case Graph.Electric:
				view.graphElectric.gameObject.SetActive(p_show);
				break;
			}
			if (p_show)
			{
				RedrawGraphCurves();
			}
		}

		private Drone GetDrone()
		{
			Drone result = null;
			if ((bool)base.app && (bool)base.app.controller && (bool)base.app.controller.game && (bool)base.app.controller.game.model)
			{
				result = base.app.controller.game.model.playerDrone;
			}
			return result;
		}

		public void RedrawGraphCurves(bool p_force = false)
		{
			Drone drone = GetDrone();
			if (drone == null)
			{
				Debug.LogWarning("UIHUDPhysicsController:: player drone not found");
			}
			else if (!drone.hasFc)
			{
				Debug.LogWarning("UIHUDPhysicsController:: drone not initialized yet");
			}
			else
			{
				if (!drone.hasBody || !drone.body.hasFrame || !drone.hasPhysics || !drone.hasFc || drone.body.frame.escs == null || drone.body.frame.escs.Count == 0 || drone.body.frame.escs[0] == null || drone.body.frame.escs[0].motor == null || drone.body.frame.escs[0].motor.prop == null || drone.body.frame.escs[0].motor.spec == null || drone.body.frame.escs[0].motor.spec.data == null || drone.body.frame.batteries == null || drone.body.frame.batteries.Count == 0 || drone.body.frame.batteries[0] == null)
				{
					return;
				}
				if (p_force || view.graphThrottle.gameObject.activeInHierarchy)
				{
					Vector2[] array = new Vector2[21];
					view.graphThrottle.SetBounds(0f, 1f, 0f, 1f);
					for (int i = 0; i < 21; i++)
					{
						float num = (float)i / 20f;
						array[i] = new Vector2(num, BetaflightRates.GetThrottle(num, drone.fc.profile.expo.throttle, drone.fc.profile.superRate.throttle));
					}
					view.graphThrottle.UpdateGraph(array);
				}
				if (p_force || view.graphYaw.gameObject.activeInHierarchy)
				{
					Vector2[] array2 = new Vector2[21];
					view.graphYaw.SetBounds(-1f, 1f, BetaflightRates.GetMin(drone.fc.profile.superRate.yaw, drone.fc.profile.rcRate.yaw, drone.fc.profile.expo.yaw), BetaflightRates.GetMax(drone.fc.profile.superRate.yaw, drone.fc.profile.rcRate.yaw, drone.fc.profile.expo.yaw));
					for (int j = -10; j <= 10; j++)
					{
						float num2 = (float)j / 10f;
						array2[j + 10] = new Vector2(num2, BetaflightRates.GetRate(num2, drone.fc.profile.superRate.yaw, drone.fc.profile.rcRate.yaw, drone.fc.profile.expo.yaw));
					}
					view.graphYaw.UpdateGraph(array2);
				}
				if (p_force || view.graphPitch.gameObject.activeInHierarchy)
				{
					Vector2[] array3 = new Vector2[21];
					view.graphPitch.SetBounds(-1f, 1f, BetaflightRates.GetMin(drone.fc.profile.superRate.pitch, drone.fc.profile.rcRate.pitch, drone.fc.profile.expo.pitch), BetaflightRates.GetMax(drone.fc.profile.superRate.pitch, drone.fc.profile.rcRate.pitch, drone.fc.profile.expo.pitch));
					for (int k = -10; k <= 10; k++)
					{
						float num3 = (float)k / 10f;
						array3[k + 10] = new Vector2(num3, BetaflightRates.GetRate(num3, drone.fc.profile.superRate.pitch, drone.fc.profile.rcRate.pitch, drone.fc.profile.expo.pitch));
					}
					view.graphPitch.UpdateGraph(array3);
				}
				if (p_force || view.graphRoll.gameObject.activeInHierarchy)
				{
					Vector2[] array4 = new Vector2[21];
					view.graphRoll.SetBounds(-1f, 1f, BetaflightRates.GetMin(drone.fc.profile.superRate.roll, drone.fc.profile.rcRate.roll, drone.fc.profile.expo.roll), BetaflightRates.GetMax(drone.fc.profile.superRate.roll, drone.fc.profile.rcRate.roll, drone.fc.profile.expo.roll));
					for (int l = -10; l <= 10; l++)
					{
						float num4 = (float)l / 10f;
						array4[l + 10] = new Vector2(num4, BetaflightRates.GetRate(num4, drone.fc.profile.superRate.roll, drone.fc.profile.rcRate.roll, drone.fc.profile.expo.roll));
					}
					view.graphRoll.UpdateGraph(array4);
				}
				if (p_force || view.graphEfficiency.gameObject.activeInHierarchy)
				{
					Vector2[] array5 = new Vector2[41];
					view.graphEfficiency.SetBounds(0f, 2f, 0f, 1f);
					for (int m = 0; m <= 40; m++)
					{
						float num5 = Mathf.Lerp(0f, 2f, (float)m / 40f);
						array5[m] = new Vector2(num5, drone.body.frame.escs[0].motor.prop.EvaluateEfficiencyCurve(num5));
					}
					view.graphEfficiency.UpdateGraph(array5);
				}
			}
		}

		private void SaveToggleState(DRLToggleView p_toggle)
		{
			PlayerPrefs.SetInt("dashboardtoggle-" + p_toggle.name, p_toggle.toggle.isOn ? 1 : 0);
		}

		private void LoadToggleState(DRLToggleView p_toggle)
		{
			p_toggle.toggle.isOn = PlayerPrefs.GetInt("dashboardtoggle-" + p_toggle.name, p_toggle.toggle.isOn ? 1 : 0) == 1;
		}

		private string Format(float f)
		{
			return f.ToString("0.######");
		}

		private void Update()
		{
			UpdateGraphs();
		}

		private void UpdateGraphs()
		{
			Drone drone = GetDrone();
			if (drone == null || !drone.ready || !drone.hasPhysics || drone.body.frame.escs == null || drone.body.frame.escs.Count == 0 || drone.body.frame.escs[0] == null || drone.body.frame.escs[0].motor == null || drone.body.frame.escs[0].motor.prop == null || drone.body.frame.escs[0].motor.spec == null || drone.body.frame.escs[0].motor.spec.data == null || drone.body.frame.batteries == null || drone.body.frame.batteries.Count == 0 || drone.body.frame.batteries[0] == null)
			{
				return;
			}
			if (view.graphThrottle.gameObject.activeInHierarchy)
			{
				view.graphThrottle.SetCurrent(drone.fc.rawSignal.throttle, drone.fc.signal.throttle);
			}
			if (view.graphPitch.gameObject.activeInHierarchy)
			{
				view.graphPitch.SetCurrent(drone.fc.rawSignal.pitch, drone.fc.signal.pitch);
			}
			if (view.graphRoll.gameObject.activeInHierarchy)
			{
				view.graphRoll.SetCurrent(drone.fc.rawSignal.roll, drone.fc.signal.roll);
			}
			if (view.graphYaw.gameObject.activeInHierarchy)
			{
				view.graphYaw.SetCurrent(drone.fc.rawSignal.yaw, drone.fc.signal.yaw);
			}
			if (view.graphEfficiency.gameObject.activeInHierarchy)
			{
				view.graphEfficiency.SetCurrent(drone.d_advanceRatio, drone.d_propEfficiency);
			}
			if (view.graphMotors.gameObject.activeInHierarchy)
			{
				for (int i = 0; i < drone.body.frame.escs.Count; i++)
				{
					DroneESC droneESC = drone.body.frame.escs[i];
					lastRPM[i] = Mathf.Lerp(lastRPM[i], drone.d_rpm[i], 0.5f);
					lastRPMratio[i] = Mathf.Lerp(lastRPMratio[i], drone.d_ratio[i], (lastRPMratio[i] > drone.d_ratio[i]) ? 0.75f : 0.5f);
					view.graphMotors.SetRpm(i, lastRPM[i], lastRPMratio[i]);
					view.graphMotors.SetThrust(i, drone.rigidbody.currentMotorThrust[i]);
					view.graphMotors.SetTorque(i, droneESC.motor.torque);
					view.graphMotors.SetVoltage(i, droneESC.motor.voltage);
				}
			}
			if (view.graphSpeed.gameObject.activeInHierarchy)
			{
				view.labelSpeedGlobalX.text = FormatNumber(drone.fc.sensor.inertial.actualVelocity.x, 2);
				view.labelSpeedGlobalY.text = FormatNumber(drone.fc.sensor.inertial.actualVelocity.y, 2);
				view.labelSpeedGlobalZ.text = FormatNumber(drone.fc.sensor.inertial.actualVelocity.z, 2);
				view.labelSpeedLocalX.text = FormatNumber(drone.fc.sensor.inertial.speeds.x, 2);
				view.labelSpeedLocalY.text = FormatNumber(drone.fc.sensor.inertial.speeds.y, 2);
				view.labelSpeedLocalZ.text = FormatNumber(drone.fc.sensor.inertial.speeds.z, 2);
				view.labelSpeedRotationX.text = FormatNumber(drone.fc.sensor.gyro.averageVelocity.x, 0);
				view.labelSpeedRotationY.text = FormatNumber(drone.fc.sensor.gyro.averageVelocity.y, 0);
				view.labelSpeedRotationZ.text = FormatNumber(drone.fc.sensor.gyro.averageVelocity.z, 0);
				view.labelSpeedAirspeed.text = FormatNumber(drone.d_trueAirspeed, 2);
				view.labelSpeedFlightMps.text = FormatNumber(drone.fc.sensor.inertial.speed, 2);
				view.labelSpeedFlightKmh.text = FormatNumber(drone.fc.sensor.inertial.speed * 3.6f, 0);
				view.labelSpeedGroundMps.text = FormatNumber(drone.fc.sensor.inertial.groundSpeed, 2);
				view.labelSpeedGroundKmh.text = FormatNumber(drone.fc.sensor.inertial.groundSpeedKph, 0);
				view.labelSpeedTopspeedKmh.text = FormatNumber(Mathf.Round(drone.rig.topSpeed), 0);
			}
			if (view.graphForce.gameObject.activeInHierarchy)
			{
				view.labelForceGlobalX.text = FormatNumber(drone.d_globalForce.x, 2);
				view.labelForceGlobalY.text = FormatNumber(drone.d_globalForce.y, 2);
				view.labelForceGlobalZ.text = FormatNumber(drone.d_globalForce.z, 2);
				view.labelForceLocalX.text = FormatNumber(drone.d_localForce.x, 2);
				view.labelForceLocalY.text = FormatNumber(drone.d_localForce.y, 2);
				view.labelForceLocalZ.text = FormatNumber(drone.d_localForce.z, 2);
				view.labelForceDragX.text = FormatNumber(drone.physics.aerodynamics.totalForce.x, 2);
				view.labelForceDragY.text = FormatNumber(drone.physics.aerodynamics.totalForce.y, 2);
				view.labelForceDragZ.text = FormatNumber(drone.physics.aerodynamics.totalForce.z, 2);
				view.labelForceDrag.text = FormatNumber(drone.physics.aerodynamics.dragForce.magnitude, 2);
				view.labelForceLift.text = FormatNumber(drone.physics.aerodynamics.liftForce.magnitude, 2);
				view.labelForceDamage.text = FormatNumber(drone.damage * 100f, 1);
				view.labelForceLastEnergy.text = FormatNumber(drone.rigidbody.lastEnergy, 1);
			}
			if (!(view.graphElectric != null) || !view.graphElectric.gameObject.activeInHierarchy)
			{
				return;
			}
			if (drone.body.frame.batteries != null)
			{
				float num = 0f;
				foreach (DroneBattery battery in drone.body.frame.batteries)
				{
					if (battery != null)
					{
						num += (drone.physics.batteryDrain ? battery.mah : battery.capacity);
					}
				}
				view.labelElectricBatteryCharge.text = FormatNumber(num, 0);
			}
			if (drone.body == null || drone.body.frame == null || drone.body.frame.escs == null || drone.body.frame.escs.Count == 0)
			{
				view.labelElectricBatteryVoltage.text = "0.0";
				view.labelElectricTemperature.text = "0.0";
			}
			else
			{
				view.labelElectricBatteryVoltage.text = FormatNumber(drone.body.frame.escs[0].voltage, 1);
				view.labelElectricTemperature.text = FormatNumber(drone.d_temperature, 1);
			}
		}

		public string FormatNumber(float p_value, int p_decimals)
		{
			if (p_decimals < 1)
			{
				return ((int)p_value).ToString();
			}
			switch (p_decimals)
			{
			case 1:
				return ((float)(int)(p_value * 10f) * 0.1f).ToString();
			case 2:
				return ((float)(int)(p_value * 100f) * 0.01f).ToString();
			case 3:
				return ((float)(int)(p_value * 1000f) * 0.001f).ToString();
			case 4:
				return ((float)(int)(p_value * 10000f) * 0.0001f).ToString();
			default:
			{
				int num = (int)Mathf.Pow(10f, p_decimals);
				return ((float)(int)(p_value * (float)num) * (1f / (float)num)).ToString();
			}
			}
		}
	}
}
