using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class DronePhysicsSettings : DronePart
	{
		public DronePhysicsData data;

		public DronePhysicsData djiData;

		public DroneProfileData profile;

		public DroneProfileData djiProfile;

		private int startcounter = 10;

		[TextArea(2, 6)]
		public string description;

		public void Start()
		{
			if (--startcounter >= 0)
			{
				if (base.drone == null)
				{
					this.TimerRunOnce(Start);
				}
				else
				{
					InitializeSettings();
				}
			}
		}

		public void InitializeSettings(bool dji = false)
		{
			if (base.drone.defaultphysics == null)
			{
				base.drone.defaultphysics = data;
			}
			if (base.drone.djiphysics == null)
			{
				base.drone.djiphysics = djiData;
			}
			if (base.drone.fc != null && (base.drone.fc.mode == FlightControllerMode.Beginner || base.drone.fc.mode == FlightControllerMode.DJI))
			{
				dji = true;
			}
			base.drone.physics = (dji ? djiData : data);
			if (base.drone.defaultprofile == null)
			{
				base.drone.defaultprofile = profile;
			}
			if (base.drone.djiprofile == null)
			{
				base.drone.djiprofile = djiProfile;
			}
			base.drone.profile = (dji ? djiProfile : profile);
		}

		public bool HasSupport(string p_frame_guid)
		{
			StringTag component = GetComponent<StringTag>();
			if (!component)
			{
				return false;
			}
			if (!component.Match("*"))
			{
				return component.Match(p_frame_guid);
			}
			return true;
		}

		public bool HasSupport(DroneFrame p_frame)
		{
			if (!p_frame)
			{
				return false;
			}
			return HasSupport(p_frame.guid);
		}

		public override string GetPrefix()
		{
			return "PH";
		}
	}
}
