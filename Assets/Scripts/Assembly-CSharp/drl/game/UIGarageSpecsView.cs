using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIGarageSpecsView : View<DRLApp>
	{
		public new Text name;

		public DRLDroneSpecBar thrustSpecBar;

		public DRLDroneSpecBar weightSpecBar;

		public DRLDroneSpecBar topSpeedSpecBar;

		public DRLDroneSpecBar torqueSpecBar;

		public DRLDroneSpecBar dragSpecBar;

		public DRLDroneSpecBar rpmSpecBar;

		public DRLDroneSpecBar temperatureSpecBar;

		public DRLDroneSpecBar efficiencySpecBar;

		public Text flightTime;

		public RawImage brandLogo;

		[Tooltip("Thrust\nWeight\nTopSpeed\nTorque\nDrag\nRPM\nTemperature\nEfficiency")]
		public float[] specBarsMaximums;

		public FadeComponent unableToFlyWarning;

		[HideInInspector]
		public bool unableToFly;

		public FadeComponent notSecureForFlyWarning;

		[HideInInspector]
		public bool notSecureForFly;

		public void SetBarMaximums()
		{
			if (specBarsMaximums.Length < 8)
			{
				Debug.LogError("[ Garage ] > some of specsBarsMaximums are missing");
				return;
			}
			thrustSpecBar.max = specBarsMaximums[0];
			weightSpecBar.max = specBarsMaximums[1];
			topSpeedSpecBar.max = specBarsMaximums[2];
			torqueSpecBar.max = specBarsMaximums[3];
			dragSpecBar.max = specBarsMaximums[4];
			rpmSpecBar.max = specBarsMaximums[5];
			temperatureSpecBar.max = specBarsMaximums[6];
			efficiencySpecBar.max = specBarsMaximums[7];
		}

		public void ToggleTemperatureBar(bool p_enable)
		{
			temperatureSpecBar.gameObject.SetActive(p_enable);
		}

		public void SetCustom(bool p_thrust, bool p_weight, bool p_torque, bool p_drag, bool p_topspeed)
		{
			thrustSpecBar.isCustom = p_thrust;
			weightSpecBar.isCustom = p_weight;
			torqueSpecBar.isCustom = p_torque;
			dragSpecBar.isCustom = p_drag;
			topSpeedSpecBar.isCustom = p_topspeed;
		}

		public void ToggleUnableToFly(bool p_enable, bool p_check = false)
		{
			if (!p_check || unableToFly)
			{
				if (p_enable)
				{
					unableToFlyWarning.FadeIn();
				}
				else
				{
					unableToFlyWarning.FadeOut();
				}
			}
		}

		public void ToggleNotSecureForFly(bool p_enable, bool p_check = false)
		{
			if (!p_check || notSecureForFly)
			{
				if (p_enable)
				{
					notSecureForFlyWarning.FadeIn();
				}
				else
				{
					notSecureForFlyWarning.FadeOut();
				}
			}
		}
	}
}
