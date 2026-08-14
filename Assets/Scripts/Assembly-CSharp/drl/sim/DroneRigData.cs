using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	[CreateAssetMenu(fileName = "drone.rig.asset", menuName = "DRL/DroneRig")]
	public class DroneRigData : ScriptableObject
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

		private static List<DroneRigData> m_pool = new List<DroneRigData>();

		public bool allowDynamicColor = true;

		public Color color0 = Colorf.ARGBToColor(4294901760u);

		public Color color1 = Colorf.ARGBToColor(4278255360u);

		public Color color2 = Colorf.ARGBToColor(4278190335u);

		public string guid;

		public string rigName = "New Drone";

		public int diameter = 6;

		public string thumb0 = "";

		public string thumb1 = "";

		public string thumb2 = "";

		public string frame = "F-c2d";

		public string esc = "E-000";

		public string motor = "M-000";

		public string prop = "P-000";

		public string camera = "C-000";

		public string fc = "FC-04d";

		public string battery = "B-000";

		public string antenna = "TX-000";

		public string receiver = "RC-c21";

		public string attachment0 = "";

		public string attachment1 = "";

		public string trail = "TR-a83";

		public string skinFrame = "SK-000";

		public string physics = "PH-000";

		public string tune = "";

		public string profile = "";

		public bool isLocked;

		public float topSpeed;

		public bool isPublic;

		[NonSerialized]
		public bool isOriginal;

		private SerializedData m_data;

		public new string name
		{
			get
			{
				return rigName;
			}
			set
			{
				rigName = value;
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

		public List<string> dependencies => new List<string>(new string[13]
		{
			frame, esc, motor, prop, camera, fc, battery, antenna, receiver, attachment0,
			attachment1, trail, skinFrame
		});

		public bool hasCustomPhysics => !string.IsNullOrEmpty(tune);

		public bool hasCustomProfile => !string.IsNullOrEmpty(profile);

		protected SerializedData data
		{
			get
			{
				if (m_data == null)
				{
					m_data = new SerializedData();
				}
				return m_data;
			}
		}

		public static DroneRigData GetPool()
		{
			DroneRigData droneRigData = null;
			if (m_pool.Count <= 0)
			{
				droneRigData = ScriptableObject.CreateInstance<DroneRigData>();
			}
			else
			{
				droneRigData = m_pool[0];
				m_pool.RemoveAt(0);
			}
			return droneRigData;
		}

		public static void SetPool(DroneRigData p_data)
		{
			if (!m_pool.Contains(p_data))
			{
				m_pool.Add(p_data);
			}
		}

		public static void PoolWarmup(int p_count)
		{
			while (m_pool.Count < p_count)
			{
				m_pool.Add(ScriptableObject.CreateInstance<DroneRigData>());
			}
		}

		public static void ClearPool()
		{
			for (int i = 0; i < m_pool.Count; i++)
			{
				UnityEngine.Object.Destroy(m_pool[i]);
			}
			m_pool.Clear();
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
			return "DRD-" + GUID.Create(24, "", 200, 0, 15, "x1");
		}

		public SerializedData ToSerializedData()
		{
			data.Set("drone-guid", guid);
			data.Set("drone-color-0", Colorf.ColorToARGB(color0));
			data.Set("drone-color-1", Colorf.ColorToARGB(color1));
			data.Set("drone-color-2", Colorf.ColorToARGB(color2));
			data.Set("drone-name", rigName);
			data.Set("drone-diameter", diameter);
			data.Set("drone-thumb-0", thumb0);
			data.Set("drone-thumb-1", thumb1);
			data.Set("drone-thumb-2", thumb2);
			data.Set("drone-frame", frame);
			data.Set("drone-esc", esc);
			data.Set("drone-motor", motor);
			data.Set("drone-prop", prop);
			data.Set("drone-camera", camera);
			data.Set("drone-fc", fc);
			data.Set("drone-battery", battery);
			data.Set("drone-antenna", antenna);
			data.Set("drone-receiver", receiver);
			data.Set("drone-attachment-0", attachment0);
			data.Set("drone-attachment-1", attachment1);
			data.Set("drone-trail", trail);
			data.Set("drone-skin-frame", skinFrame);
			data.Set("drone-physics", physics);
			data.Set("cummunity-tune", tune);
			data.Set("drone-profile", profile);
			data.Set("drone-locked", isLocked);
			data.Set("drone-top-speed", topSpeed);
			data.Set("drone-public", isPublic);
			data.Set("allow-dynamic-color", allowDynamicColor);
			return data;
		}

		public static DroneRigData FromSerializedData(SerializedData p_data)
		{
			DroneRigData pool = GetPool();
			pool.guid = p_data.Get("drone-guid", "");
			pool.color0 = Colorf.ARGBToColor(p_data.Get("drone-color-0", 4294901760u));
			pool.color1 = Colorf.ARGBToColor(p_data.Get("drone-color-1", 4278255360u));
			pool.color2 = Colorf.ARGBToColor(p_data.Get("drone-color-2", 4278190335u));
			pool.rigName = p_data.Get("drone-name", "New Drone");
			pool.diameter = p_data.Get("drone-diameter", 7);
			pool.thumb0 = p_data.Get("drone-thumb-0", "");
			pool.thumb1 = p_data.Get("drone-thumb-1", "");
			pool.thumb2 = p_data.Get("drone-thumb-2", "");
			pool.frame = p_data.Get("drone-frame", "F-631");
			pool.esc = p_data.Get("drone-esc", "E-000");
			pool.motor = p_data.Get("drone-motor", "M-000");
			pool.prop = p_data.Get("drone-prop", "P-000");
			pool.camera = p_data.Get("drone-camera", "C-000");
			pool.fc = p_data.Get("drone-fc", "FC-04d");
			pool.battery = p_data.Get("drone-battery", "B-000");
			pool.antenna = p_data.Get("drone-antenna", "TX-000");
			pool.receiver = p_data.Get("drone-receiver", "RC-c21");
			pool.attachment0 = p_data.Get("drone-attachment-0", "");
			pool.attachment1 = p_data.Get("drone-attachment-1", "");
			pool.trail = p_data.Get("drone-trail", "TR-a83");
			pool.skinFrame = p_data.Get("drone-skin-frame", "SK-000");
			pool.physics = p_data.Get("drone-physics", "PH-000");
			pool.tune = p_data.Get("cummunity-tune", "");
			pool.profile = p_data.Get("drone-profile", "");
			pool.isLocked = p_data.Get("drone-locked", d: false);
			pool.topSpeed = p_data.Get("drone-top-speed", 0f);
			pool.isPublic = p_data.Get("drone-public", d: false);
			pool.allowDynamicColor = p_data.Get("allow-dynamic-color", d: false);
			return pool;
		}

		public string ToJson(bool p_indented = false)
		{
			return ToSerializedData().ToJson(p_indented);
		}

		public static DroneRigData FromJson(string p_json)
		{
			return FromSerializedData(SerializedData.FromJson<SerializedData>(p_json.StartsWith("*") ? p_json.Substring(1) : p_json));
		}

		[Obsolete("Obsolete, remove after converting all old drone rig text assets into new scriptable object format")]
		public void Set(byte[] p_data)
		{
			DroneRigLegacyData droneRigLegacyData = new DroneRigLegacyData();
			droneRigLegacyData.Set(p_data);
			FromLegacy(droneRigLegacyData);
		}

		[Obsolete("Obsolete, remove after converting all old drone rig text assets into new scriptable object format")]
		public byte[] ToBytes()
		{
			return ToLegacy().ToBytes();
		}

		public DroneRigLegacyData ToLegacy()
		{
			return new DroneRigLegacyData
			{
				guid = guid,
				color0 = color0,
				color1 = color1,
				color2 = color2,
				name = rigName,
				diameter = diameter,
				thumb0 = thumb0,
				thumb1 = thumb1,
				thumb2 = thumb2,
				frame = frame,
				esc = esc,
				motor = motor,
				prop = prop,
				camera = camera,
				fc = fc,
				battery = battery,
				antenna = antenna,
				receiver = receiver,
				attachment0 = attachment0,
				attachment1 = attachment1,
				trail = trail,
				skinFrame = skinFrame,
				physics = physics,
				tune = tune,
				profile = profile
			};
		}

		public void FromLegacy(DroneRigLegacyData d)
		{
			guid = d.guid;
			color0 = d.color0;
			color1 = d.color1;
			color2 = d.color2;
			rigName = d.name;
			diameter = d.diameter;
			thumb0 = d.thumb0;
			thumb1 = d.thumb1;
			thumb2 = d.thumb2;
			frame = d.frame;
			esc = d.esc;
			motor = d.motor;
			prop = d.prop;
			camera = d.camera;
			fc = d.fc;
			battery = d.battery;
			antenna = d.antenna;
			receiver = d.receiver;
			attachment0 = d.attachment0;
			attachment1 = d.attachment1;
			trail = d.trail;
			skinFrame = d.skinFrame;
			physics = d.physics;
			tune = d.tune;
			profile = d.profile;
		}

		public static DroneRigData NewFromLegacy(DroneRigLegacyData d)
		{
			DroneRigData droneRigData = ScriptableObject.CreateInstance<DroneRigData>();
			droneRigData.FromLegacy(d);
			return droneRigData;
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
			if (!IsPartEqual(p_rig.frame, frame))
			{
				return false;
			}
			if (!IsPartEqual(p_rig.motor, motor))
			{
				return false;
			}
			if (!IsPartEqual(p_rig.prop, prop))
			{
				return false;
			}
			if (!IsPartEqual(p_rig.battery, battery))
			{
				return false;
			}
			if (!IsPartEqual(p_rig.esc, esc))
			{
				return false;
			}
			if (!IsPartEqual(p_rig.camera, camera))
			{
				return false;
			}
			if (!IsPartEqual(p_rig.antenna, antenna))
			{
				return false;
			}
			if (!IsPartEqual(p_rig.receiver, receiver))
			{
				return false;
			}
			if (!IsPartEqual(p_rig.fc, fc))
			{
				return false;
			}
			if (!IsPartEqual(p_rig.attachment0, attachment0))
			{
				return false;
			}
			if (!IsPartEqual(p_rig.attachment1, attachment1))
			{
				return false;
			}
			return true;
		}

		private bool IsPartEqual(string a, string b)
		{
			if (a == b)
			{
				return true;
			}
			if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
			{
				return true;
			}
			if (string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b) && b.EndsWith("000"))
			{
				return true;
			}
			if (string.IsNullOrEmpty(b) && !string.IsNullOrEmpty(a) && a.EndsWith("000"))
			{
				return true;
			}
			return false;
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

		public DroneRigData Clone()
		{
			DroneRigData droneRigData = ScriptableObject.CreateInstance<DroneRigData>();
			droneRigData.guid = GenerateGUID();
			droneRigData.color0 = color0;
			droneRigData.color1 = color1;
			droneRigData.color2 = color2;
			droneRigData.rigName = rigName;
			droneRigData.diameter = diameter;
			droneRigData.thumb0 = thumb0;
			droneRigData.thumb1 = thumb1;
			droneRigData.thumb2 = thumb2;
			droneRigData.frame = frame;
			droneRigData.esc = esc;
			droneRigData.motor = motor;
			droneRigData.prop = prop;
			droneRigData.camera = camera;
			droneRigData.fc = fc;
			droneRigData.battery = battery;
			droneRigData.antenna = antenna;
			droneRigData.receiver = receiver;
			droneRigData.attachment0 = attachment0;
			droneRigData.attachment1 = attachment1;
			droneRigData.trail = trail;
			droneRigData.skinFrame = skinFrame;
			droneRigData.physics = physics;
			droneRigData.tune = tune;
			droneRigData.profile = profile;
			droneRigData.isLocked = isLocked;
			droneRigData.isPublic = isPublic;
			droneRigData.allowDynamicColor = allowDynamicColor;
			return droneRigData;
		}
	}
}
