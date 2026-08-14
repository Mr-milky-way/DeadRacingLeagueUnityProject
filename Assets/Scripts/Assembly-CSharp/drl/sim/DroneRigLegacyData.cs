using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	[Serializable]
	public class DroneRigLegacyData : SerializedData
	{
		public struct ComponentPair
		{
			public DronePart part;

			public DronePart against;

			public ComponentPair(DronePart p_part, DronePart p_against)
			{
				part = p_part;
				against = p_against;
			}
		}

		public Color color0
		{
			get
			{
				return Colorf.ARGBToColor(Get("drone-color-0", 4294901760u));
			}
			set
			{
				Set("drone-color-0", Colorf.ColorToARGB(value));
			}
		}

		public Color color1
		{
			get
			{
				return Colorf.ARGBToColor(Get("drone-color-1", 4278255360u));
			}
			set
			{
				Set("drone-color-1", Colorf.ColorToARGB(value));
			}
		}

		public Color color2
		{
			get
			{
				return Colorf.ARGBToColor(Get("drone-color-2", 4278190335u));
			}
			set
			{
				Set("drone-color-2", Colorf.ColorToARGB(value));
			}
		}

		public Color[] colors
		{
			get
			{
				return new Color[3] { color0, color1, color2 };
			}
			set
			{
				if (value != null)
				{
					int num = Mathf.Min(4, value.Length);
					for (int i = 0; i < num; i++)
					{
						SetColor(i, value[i]);
					}
				}
			}
		}

		public List<string> parts => new List<string>(new string[14]
		{
			frame, esc, motor, prop, camera, fc, battery, antenna, receiver, attachment0,
			attachment1, trail, skinFrame, physics
		});

		public string guid
		{
			get
			{
				return Get("drone-guid", "");
			}
			set
			{
				Set("drone-guid", value);
			}
		}

		public string name
		{
			get
			{
				return Get("drone-name", "New Drone");
			}
			set
			{
				Set("drone-name", value);
			}
		}

		public int diameter
		{
			get
			{
				return Get("drone-diameter", 5);
			}
			set
			{
				Set("drone-diameter", value);
			}
		}

		public string thumb0
		{
			get
			{
				return Get("drone-thumb-0", "");
			}
			set
			{
				Set("drone-thumb-0", value);
			}
		}

		public string thumb1
		{
			get
			{
				return Get("drone-thumb-1", "");
			}
			set
			{
				Set("drone-thumb-1", value);
			}
		}

		public string thumb2
		{
			get
			{
				return Get("drone-thumb-2", "");
			}
			set
			{
				Set("drone-thumb-2", value);
			}
		}

		public string frame
		{
			get
			{
				return Get("drone-frame", "F-68d");
			}
			set
			{
				Set("drone-frame", value);
			}
		}

		public string esc
		{
			get
			{
				return Get("drone-esc", "E-000");
			}
			set
			{
				Set("drone-esc", value);
			}
		}

		public string motor
		{
			get
			{
				return Get("drone-motor", "M-000");
			}
			set
			{
				Set("drone-motor", value);
			}
		}

		public string prop
		{
			get
			{
				return Get("drone-prop", "P-000");
			}
			set
			{
				Set("drone-prop", value);
			}
		}

		public string camera
		{
			get
			{
				return Get("drone-camera", "C-000");
			}
			set
			{
				Set("drone-camera", value);
			}
		}

		public string fc
		{
			get
			{
				return Get("drone-fc", "FC-04d");
			}
			set
			{
				Set("drone-fc", value);
			}
		}

		public string battery
		{
			get
			{
				return Get("drone-battery", "B-000");
			}
			set
			{
				Set("drone-battery", value);
			}
		}

		public string antenna
		{
			get
			{
				return Get("drone-antenna", "TX-000");
			}
			set
			{
				Set("drone-antenna", value);
			}
		}

		public string receiver
		{
			get
			{
				return Get("drone-receiver", "RC-c21");
			}
			set
			{
				Set("drone -receiver", value);
			}
		}

		public string attachment0
		{
			get
			{
				return Get("drone-attachment-0", "");
			}
			set
			{
				Set("drone-attachment-0", value);
			}
		}

		public string attachment1
		{
			get
			{
				return Get("drone-attachment-1", "");
			}
			set
			{
				Set("drone-attachment-1", value);
			}
		}

		public string trail
		{
			get
			{
				return Get("drone-trail", "TR-a83");
			}
			set
			{
				Set("drone-trail", value);
			}
		}

		public string skinFrame
		{
			get
			{
				return Get("drone-skin-frame", "SK-000");
			}
			set
			{
				Set("drone-skin-frame", value);
			}
		}

		public string physics
		{
			get
			{
				return Get("drone-physics", "PH-000");
			}
			set
			{
				Set("drone-physics", value);
			}
		}

		public string tune
		{
			get
			{
				return Get("cummunity-tune", "");
			}
			set
			{
				Set("cummunity-tune", value);
			}
		}

		public string profile
		{
			get
			{
				return Get("drone-profile", "");
			}
			set
			{
				Set("drone-profile", value);
			}
		}

		public Color GetColor(int p_index)
		{
			return p_index switch
			{
				0 => color0, 
				1 => color1, 
				2 => color2, 
				_ => Color.black, 
			};
		}

		public void SetColor(int p_index, Color p_value)
		{
			switch (p_index)
			{
			case 0:
				color0 = p_value;
				break;
			case 1:
				color1 = p_value;
				break;
			case 2:
				color2 = p_value;
				break;
			}
		}

		public static string GenerateGUID()
		{
			return "DRD-" + GUID.Create(12, "", 200, 0, 15, "x1");
		}

		public void SetByPartGUID(DronePart p_target)
		{
			if (!p_target)
			{
				return;
			}
			string text = p_target.guid;
			if (p_target is DroneFrame)
			{
				frame = text;
			}
			if (p_target is DroneESC)
			{
				esc = text;
			}
			if (p_target is DroneMotor)
			{
				motor = text;
			}
			if (p_target is DroneProp)
			{
				prop = text;
			}
			if (p_target is DroneRFCamera)
			{
				camera = text;
			}
			if (p_target is DroneFlightController)
			{
				fc = text;
			}
			if (p_target is DroneBattery)
			{
				battery = text;
			}
			if (p_target is DroneAntennaTx)
			{
				antenna = text;
			}
			if (p_target is DroneReceiver)
			{
				receiver = text;
			}
			if (p_target is DroneTrail)
			{
				trail = text;
			}
			if (p_target is DronePhysicsSettings)
			{
				physics = text;
			}
			if (p_target is DroneSkin && ((DroneSkin)p_target).category == DroneAssetTagType.Frame)
			{
				skinFrame = text;
			}
			if (p_target is DroneAttachment)
			{
				if (attachment0 == null || attachment0 == "")
				{
					attachment0 = text;
				}
				else if (attachment1 == null || attachment1 == "")
				{
					attachment1 = text;
				}
				else
				{
					Debug.LogError("DroneRigData.SetByPartGUID :: too many attachments");
				}
			}
		}

		public void SetByPartGUID(UnityEngine.Object[] p_target)
		{
			for (int i = 0; i < p_target.Length; i++)
			{
				DronePart dronePart = ((p_target[i] is DronePart) ? ((DronePart)p_target[i]) : null);
				if (!dronePart)
				{
					GameObject gameObject = ((p_target[i] is GameObject) ? ((GameObject)p_target[i]) : null);
					if ((bool)gameObject)
					{
						dronePart = gameObject.GetComponent<DronePart>();
					}
				}
				SetByPartGUID(dronePart);
			}
		}

		public bool FunctionallyIdentical(DroneRigData p_rig)
		{
			if (p_rig == null)
			{
				return false;
			}
			if (p_rig.frame != frame)
			{
				return false;
			}
			if (p_rig.motor != motor)
			{
				return false;
			}
			if (p_rig.prop != prop)
			{
				return false;
			}
			if (p_rig.battery != battery)
			{
				return false;
			}
			if (p_rig.esc != esc)
			{
				return false;
			}
			if (p_rig.camera != camera)
			{
				return false;
			}
			if (p_rig.antenna != antenna)
			{
				return false;
			}
			if (p_rig.receiver != receiver)
			{
				return false;
			}
			if (p_rig.fc != fc)
			{
				return false;
			}
			if (p_rig.attachment0 != attachment0)
			{
				return false;
			}
			if (p_rig.attachment1 != attachment1)
			{
				return false;
			}
			return true;
		}

		public List<ComponentPair> CheckForConflicts(AssetLibrary parts = null)
		{
			return null;
		}

		public void Validate()
		{
			if (string.IsNullOrEmpty(antenna))
			{
				antenna = "TX-000";
			}
			if (string.IsNullOrEmpty(attachment0))
			{
				attachment0 = "AT-000";
			}
			if (string.IsNullOrEmpty(battery))
			{
				battery = "B-000";
			}
			if (string.IsNullOrEmpty(camera))
			{
				camera = "C-000";
			}
			if (string.IsNullOrEmpty(esc))
			{
				esc = "E-000";
			}
			if (string.IsNullOrEmpty(fc))
			{
				fc = "FC-000";
			}
			if (string.IsNullOrEmpty(frame))
			{
				frame = "F-000";
			}
			if (string.IsNullOrEmpty(motor))
			{
				motor = "M-000";
			}
			if (string.IsNullOrEmpty(physics))
			{
				physics = "PH-000";
			}
			if (string.IsNullOrEmpty(prop))
			{
				prop = "P-000";
			}
			if (string.IsNullOrEmpty(receiver))
			{
				receiver = "RC-000";
			}
			if (string.IsNullOrEmpty(skinFrame))
			{
				skinFrame = "SK-000";
			}
			if (string.IsNullOrEmpty(trail))
			{
				trail = "TR-000";
			}
			if (physics == "PH-77f")
			{
				physics = "PH-000";
			}
			if (skinFrame == "SK-990")
			{
				skinFrame = "SK-000";
			}
			if (attachment0 == "AT-06c")
			{
				attachment0 = "AT-000";
			}
			if (battery == "B-10d")
			{
				battery = "B-000";
			}
			if (esc == "E-f33")
			{
				esc = "E-000";
			}
		}
	}
}
