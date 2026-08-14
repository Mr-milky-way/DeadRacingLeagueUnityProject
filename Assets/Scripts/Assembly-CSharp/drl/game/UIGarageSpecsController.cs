using System;
using System.Collections;
using UnityEngine;
using drl.sim;
using thelab.mvc;

namespace drl.game
{
	public class UIGarageSpecsController : Controller<DRLApp>
	{
		private Coroutine m_specsBarUpdateCoroutine;

		public UIGarageSpecsView view => AssertLocal<UIGarageSpecsView>("view");

		public void StartUpdateSpeedBar(float p_current, float p_next)
		{
			if (m_specsBarUpdateCoroutine != null)
			{
				StopCoroutine(m_specsBarUpdateCoroutine);
			}
			m_specsBarUpdateCoroutine = StartCoroutine(UpdateSpeedBar(p_current, p_next));
		}

		public void StopUpdateSpeedBar()
		{
			if (m_specsBarUpdateCoroutine != null)
			{
				StopCoroutine(m_specsBarUpdateCoroutine);
			}
		}

		private IEnumerator UpdateSpeedBar(float p_current, float p_next)
		{
			yield break;
		}

		public void SetBars(DroneRigSpecData p_saved, DroneRigSpecData p_modified)
		{
			if (!(view.thrustSpecBar == null))
			{
				view.thrustSpecBar.SetCurrentAndNext(p_saved.thrust, p_modified.thrust, 0.3f);
				view.weightSpecBar.SetCurrentAndNext(p_saved.weight, p_modified.weight, 0.3f);
				view.topSpeedSpecBar.SetCurrentAndNext(p_saved.topSpeed, p_modified.topSpeed, 0.3f);
				view.torqueSpecBar.SetCurrentAndNext(p_saved.torque, p_modified.torque, 0.3f);
				view.dragSpecBar.SetCurrentAndNext(p_saved.drag, p_modified.drag, 0.3f);
				view.rpmSpecBar.SetCurrentAndNext(p_saved.rpm, p_modified.rpm, 0.3f);
				view.temperatureSpecBar.SetCurrentAndNext(p_saved.temperature, p_modified.temperature, 0.3f);
				view.efficiencySpecBar.SetCurrentAndNext(p_saved.efficiency, p_modified.efficiency, 0.3f);
			}
		}

		public void SetName(DroneRigData p_drone)
		{
			if (!(view.name == null) && !(p_drone == null))
			{
				view.name.text = p_drone.name.ToUpper();
			}
		}

		public void SetLogo(Texture2D p_logo)
		{
			if (p_logo == null)
			{
				view.brandLogo.gameObject.SetActive(value: false);
				return;
			}
			view.brandLogo.texture = p_logo;
			view.brandLogo.gameObject.SetActive(value: true);
		}

		public void SetFlightTime(float p_time)
		{
			if (!(view.flightTime == null))
			{
				if ((int)p_time / 60 > 0)
				{
					view.flightTime.text = (int)p_time / 60 + " " + base.app.model.storage.locale.Get("settings.profile-screen-dev.hours", "Hours");
				}
				else
				{
					view.flightTime.text = (int)p_time + " " + base.app.model.storage.locale.Get("garage.selection-screen.minutes", "Minutes");
				}
			}
		}

		public void SetTopSpeed(DroneRigSpecData p_saved, DroneRigSpecData p_modified)
		{
			if (!(view.topSpeedSpecBar == null))
			{
				view.topSpeedSpecBar.SetCurrentAndNext(p_saved.topSpeed, p_modified.topSpeed, 0.3f);
			}
		}

		public void RefreshBars(DroneRigSpecData p_modified, GarageStateModel p_saved, bool p_unableToFly, Drone p_drone, DroneRigData p_rigData)
		{
			if (p_unableToFly)
			{
				p_modified.topSpeed = 0f;
				p_modified.thrust = 0f;
				p_modified.torque = 0f;
				p_modified.rpm = 0f;
				p_modified.temperature = 200f;
				view.unableToFlyWarning.FadeIn();
				view.unableToFly = true;
			}
			else
			{
				int counter = 30;
				Run((Func<bool>)delegate
				{
					if (counter-- < 0)
					{
						return false;
					}
					if (view == null || p_drone == null)
					{
						return false;
					}
					float num = p_drone.EstimateTopSpeed() * 3.6f;
					if (num < 0f)
					{
						return true;
					}
					p_modified.topSpeed = num;
					SetTopSpeed(p_saved.lastSavedRigSpecData, p_modified);
					return false;
				}, 0f, false);
				if (p_drone != null)
				{
					float p_next = p_drone.EstimateTopSpeed() * 3.6f;
					StartUpdateSpeedBar(p_saved.lastSavedRigSpecData.topSpeed, p_next);
				}
				view.unableToFlyWarning.FadeOut();
				view.unableToFly = false;
			}
			if (p_rigData.hasCustomPhysics)
			{
				DronePhysicsData dronePhysicsData = DronePhysicsData.FromJson(p_rigData.tune);
				view.SetCustom(dronePhysicsData.thrust > 0f, dronePhysicsData.mass > 0f, dronePhysicsData.torque > 0f, p_drag: true, p_topspeed: true);
			}
			else
			{
				view.SetCustom(p_thrust: false, p_weight: false, p_torque: false, p_drag: false, p_topspeed: false);
			}
			SetBars(p_saved.lastSavedRigSpecData, p_modified);
			if (p_modified.thrust > p_modified.weight * 5f && p_drone != null && p_drone.profile != null)
			{
				if (p_drone.profile.pitchPID.p > 65f)
				{
					p_drone.profile.pitchPID.p = 65f;
				}
				if (p_drone.profile.pitchPID.d > 45f)
				{
					p_drone.profile.pitchPID.d = 45f;
				}
				if (p_drone.profile.rollPID.p > 65f)
				{
					p_drone.profile.rollPID.p = 65f;
				}
				if (p_drone.profile.rollPID.d > 45f)
				{
					p_drone.profile.rollPID.d = 45f;
				}
				p_rigData.profile = p_drone.profile.ToJson();
			}
			if (p_modified.thrust > p_modified.weight * 8f && p_drone != null && p_drone.profile != null)
			{
				if (p_drone.profile.pitchPID.p > 50f)
				{
					p_drone.profile.pitchPID.p = 50f;
				}
				if (p_drone.profile.pitchPID.d > 40f)
				{
					p_drone.profile.pitchPID.d = 40f;
				}
				if (p_drone.profile.rollPID.p > 50f)
				{
					p_drone.profile.rollPID.p = 50f;
				}
				if (p_drone.profile.rollPID.d > 40f)
				{
					p_drone.profile.rollPID.d = 40f;
				}
				p_rigData.profile = p_drone.profile.ToJson();
			}
		}
	}
}
