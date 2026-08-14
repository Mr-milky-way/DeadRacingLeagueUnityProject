using UnityEngine;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIBatteryMeterController : Controller<DRLApp>
	{
		public UIBatteryMeter capacityMeter;

		public UIBatteryMeter voltageMeter;

		public UIBatteryMeter ampDrawMeter;

		public FadeComponent fader;

		private Drone m_drone;

		private bool m_visible;

		private bool m_can_show;

		private bool m_can_show_drain;

		private bool m_raceEnabled;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			base.OnNotification(p_event, p_target, p_data);
			switch (p_event)
			{
			case "game.simulation.drone@armed":
				m_drone = GetDrone();
				break;
			case "game.race.request-restart":
				Recharge();
				break;
			case "game.race.enabled":
				m_can_show = base.app.arguments.game.type != GameFlag.Mission && m_drone != null && m_drone.physics.batteryDrain;
				if (base.app.inVirtualSeason)
				{
					m_can_show = true;
				}
				capacityMeter.gameObject.SetActive(m_drone != null && m_drone.physics.batteryDrain);
				m_raceEnabled = true;
				break;
			case "game.race.slowmo@start":
				m_can_show = false;
				break;
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

		public void Toggle(bool p_on)
		{
			if (p_on)
			{
				Show();
			}
			else
			{
				Hide();
			}
		}

		public void Show()
		{
			fader.FadeIn();
			m_visible = true;
		}

		public void Hide()
		{
			fader.FadeOut();
			m_visible = false;
		}

		public void SetCapacity(float p_capacity, float p_max, float p_min)
		{
			capacityMeter.min = p_min;
			capacityMeter.max = p_max;
			capacityMeter.SetValue1(p_capacity);
		}

		public void SetVoltage(float p_voltage, float p_available, float p_max, float p_min)
		{
			voltageMeter.min = p_min;
			voltageMeter.max = p_max;
			voltageMeter.SetValue1(p_voltage);
			voltageMeter.SetValue2(p_available);
		}

		public void SetAmperageDraw(float p_amperage, float p_max, float p_min)
		{
			ampDrawMeter.min = p_min;
			ampDrawMeter.max = p_max;
			ampDrawMeter.SetValue1(p_amperage);
		}

		public void Recharge()
		{
			Drone drone = m_drone;
			if (drone == null || !(drone.body != null) || !(drone.body.frame != null) || drone.body.frame.batteries == null)
			{
				return;
			}
			foreach (DroneBattery battery in drone.body.frame.batteries)
			{
				if (battery != null)
				{
					battery.Recharge();
				}
			}
		}

		private void Update()
		{
			if (!m_can_show)
			{
				if (m_visible)
				{
					Hide();
				}
				return;
			}
			if (m_drone == null)
			{
				m_drone = GetDrone();
			}
			if (m_drone == null || !m_drone.hasFc || m_drone.fc.sensor.electrical == null || !m_drone.hasPhysics)
			{
				return;
			}
			if (base.app.view.ui.screens.current != null && base.app.view.ui.screens.current.name == "game-spectate-screen")
			{
				if (m_visible)
				{
					Hide();
				}
				return;
			}
			if ((m_drone.fc.drainBatteries || base.app.inVirtualSeason) && !m_visible)
			{
				Show();
			}
			if (!m_drone.fc.drainBatteries && !base.app.inVirtualSeason && m_visible)
			{
				Hide();
			}
			if (m_visible)
			{
				float totalCapacity = m_drone.fc.sensor.electrical.totalCapacity;
				float remainingCharge = m_drone.fc.sensor.electrical.remainingCharge;
				float voltageMax = m_drone.fc.sensor.electrical.voltageMax;
				float voltage = m_drone.fc.sensor.electrical.voltage;
				float voltageAvailable = m_drone.fc.sensor.electrical.voltageAvailable;
				float currentDraw = m_drone.fc.sensor.electrical.currentDraw;
				float currentMax = m_drone.fc.sensor.electrical.currentMax;
				float voltageMin = m_drone.fc.sensor.electrical.voltageMin;
				float p_min = 0f;
				float p_min2 = 0f;
				SetCapacity(remainingCharge, totalCapacity, p_min);
				if (!m_raceEnabled)
				{
					SetVoltage(voltageMax, voltageMax, voltageMax, voltageMin);
				}
				else
				{
					SetVoltage(voltage, voltageAvailable, voltageMax, voltageMin);
				}
				SetAmperageDraw(currentDraw, currentMax, p_min2);
			}
		}

		public void SetBatteryUI(bool p_raceEnabled, DroneBatteryPowerData p_data)
		{
			SetCapacity(p_data.remainingCharge, p_data.totalCapacity, 0f);
			if (p_raceEnabled)
			{
				SetVoltage(p_data.voltage, p_data.voltageAvailable, p_data.voltageMax, p_data.voltageMin);
			}
			else
			{
				SetVoltage(p_data.voltageMax, p_data.voltageMax, p_data.voltageMax, p_data.voltageMin);
			}
			SetAmperageDraw(p_data.currentDraw, p_data.currentMax, 0f);
		}
	}
}
