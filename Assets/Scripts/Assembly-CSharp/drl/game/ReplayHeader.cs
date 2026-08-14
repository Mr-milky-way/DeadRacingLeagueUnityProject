using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using drl.backend;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class ReplayHeader : ReplayStream
	{
		private ReplayHeaderData m_data;

		private DroneRigData m_prev_rigdata;

		private DronePhysicsData m_prev_physics_data;

		private List<ReplayEvent> m_events;

		public ReplayHeaderData data
		{
			get
			{
				if (m_data != null)
				{
					return m_data;
				}
				return m_data = new ReplayHeaderData();
			}
		}

		public bool compressed
		{
			get
			{
				return data.Get("file-compressed", d: true);
			}
			set
			{
				data.Set("file-compressed", value);
			}
		}

		public string playerId
		{
			get
			{
				return data.Get("player-id", "");
			}
			set
			{
				data.Set("player-id", value);
			}
		}

		public string platformId
		{
			get
			{
				return data.Get(DRLService.PlatformIdKey, "");
			}
			set
			{
				data.Set(DRLService.PlatformIdKey, value);
			}
		}

		public bool isPlayer
		{
			get
			{
				return data.Get("player", d: false);
			}
			set
			{
				data.Set("player", value);
			}
		}

		public string profileName
		{
			get
			{
				return data.Get("profile-name", "");
			}
			set
			{
				data.Set("profile-name", value);
			}
		}

		public string profilePhoto
		{
			get
			{
				return data.Get("profile-photo", "");
			}
			set
			{
				data.Set("profile-photo", value);
			}
		}

		public string profileColorHex
		{
			get
			{
				return data.Get("profile-color", "ff0000");
			}
			set
			{
				data.Set("profile-color", value);
			}
		}

		public Color profileColor
		{
			get
			{
				uint v = (uint.TryParse(profileColorHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out v) ? v : 16711680u);
				return Colorf.RGBToColor(v);
			}
			set
			{
				profileColorHex = Colorf.ColorToRGB(value).ToString("x6");
			}
		}

		public string profileTournamentColorHex
		{
			get
			{
				return data.Get("profile-tournament-color", "ff0000");
			}
			set
			{
				data.Set("profile-tournament-color", value);
			}
		}

		public Color profileTournamentColor2
		{
			get
			{
				uint v = (uint.TryParse(profileTournamentColor2Hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out v) ? v : 16711680u);
				return Colorf.RGBToColor(v);
			}
			set
			{
				profileTournamentColor2Hex = Colorf.ColorToRGB(value).ToString("x6");
			}
		}

		public string profileTournamentColor2Hex
		{
			get
			{
				return data.Get("profile-tournament-color-2", "ff0000");
			}
			set
			{
				data.Set("profile-tournament-color-2", value);
			}
		}

		public string mapGUID
		{
			get
			{
				return data.Get("map", "");
			}
			set
			{
				data.Set("map", value);
			}
		}

		public string customMapGUID
		{
			get
			{
				return data.Get("custom-map", "");
			}
			set
			{
				data.Set("custom-map", value);
			}
		}

		public bool isCustomMap
		{
			get
			{
				return data.Get("is-custom-map", d: false);
			}
			set
			{
				data.Set("is-custom-map", value);
			}
		}

		public string trackGUID
		{
			get
			{
				return data.Get("track", "");
			}
			set
			{
				data.Set("track", value);
			}
		}

		public string podiumGUID
		{
			get
			{
				return data.Get("podium-id", "");
			}
			set
			{
				data.Set("podium-id", value);
			}
		}

		public string title
		{
			get
			{
				return data.Get("title", "");
			}
			set
			{
				data.Set("title", value);
			}
		}

		public bool isMultiplayer
		{
			get
			{
				return data.Get("multiplayer", d: false);
			}
			set
			{
				data.Set("multiplayer", value);
			}
		}

		public float raceTime
		{
			get
			{
				return data.Get("race-time", 0f);
			}
			set
			{
				data.Set("race-time", value);
			}
		}

		public int order
		{
			get
			{
				return data.Get("order", 0);
			}
			set
			{
				data.Set("order", value);
			}
		}

		public int gameType
		{
			get
			{
				return data.Get("game-type", 0);
			}
			set
			{
				data.Set("game-type", value);
			}
		}

		public GameFlag gameTypeFlag
		{
			get
			{
				return (GameFlag)gameType;
			}
			set
			{
				gameType = (int)value;
			}
		}

		public int controllerType
		{
			get
			{
				return data.Get("controller-type", 0);
			}
			set
			{
				data.Set("controller-type", value);
			}
		}

		public ControllerStateType controllerTypeFlag
		{
			get
			{
				return (ControllerStateType)controllerType;
			}
			set
			{
				controllerType = (int)value;
			}
		}

		public float cameraTilt
		{
			get
			{
				return data.Get("camera-tilt", 0f);
			}
			set
			{
				data.Set("camera-tilt", value);
			}
		}

		public float cameraFOV
		{
			get
			{
				return data.Get("camera-fov", 0f);
			}
			set
			{
				data.Set("camera-fov", value);
			}
		}

		public bool isCustomPhysics
		{
			get
			{
				return data.Get("custom-physics", d: false);
			}
			set
			{
				data.Set("custom-physics", value);
			}
		}

		public List<ReplayEvent> events
		{
			get
			{
				if (m_events != null)
				{
					return m_events;
				}
				List<ReplayEvent> list = null;
				if (m_events == null)
				{
					JArray jArray = data.Get<JArray>("events", null);
					if (jArray != null)
					{
						list = jArray.ToObject<List<ReplayEvent>>();
					}
					if (list == null)
					{
						list = new List<ReplayEvent>();
					}
				}
				for (int i = 0; i < list.Count; i++)
				{
					list[i].Init();
				}
				return m_events = list;
			}
			set
			{
				if (value != m_events)
				{
					m_events = new List<ReplayEvent>((value == null) ? new ReplayEvent[0] : value.ToArray());
				}
				data.Set("events", m_events);
			}
		}

		public DroneRigData GetDroneRig()
		{
			if ((bool)m_prev_rigdata)
			{
				Object.Destroy(m_prev_rigdata);
			}
			if (data.droneRig != null)
			{
				return m_prev_rigdata = DroneRigData.FromSerializedData(data.droneRig);
			}
			return null;
		}

		public void SetDroneRig(DroneRigData p_drone_rig)
		{
			if (p_drone_rig != null)
			{
				data.droneRig = p_drone_rig.ToSerializedData();
			}
		}

		public FCProfileData GetFCProfile()
		{
			if (data.fcProfile == null)
			{
				return null;
			}
			FCProfileData fCProfileData = new FCProfileData();
			fCProfileData.Merge(data.fcProfile);
			return fCProfileData;
		}

		public void SetFCProfile(FCProfileData p_fc_profile)
		{
			if (data.fcProfile == null)
			{
				data.fcProfile = new SerializedData();
			}
			else
			{
				data.fcProfile.Clear();
			}
			if (p_fc_profile != null)
			{
				data.fcProfile.Merge(p_fc_profile);
			}
		}

		public DronePhysicsData GetPhysicsTune()
		{
			if ((bool)m_prev_physics_data)
			{
				Object.Destroy(m_prev_physics_data);
			}
			return m_prev_physics_data = DronePhysicsData.FromSerializedData(data.physicsTune);
		}

		public void SetPhysicsTune(DronePhysicsData p_physics_data)
		{
			data.physicsTune = ((p_physics_data == null) ? null : p_physics_data.ToSerializedData());
		}

		public int GetEventCount(ReplayEventType p_type)
		{
			int num = 0;
			for (int i = 0; i < events.Count; i++)
			{
				num += ((events[i].typeFlag == p_type) ? 1 : 0);
			}
			return num;
		}

		public void Serialize(Stream p_stream)
		{
			p_stream.Position = 0L;
			JsonWriter jsonWriter = new JsonTextWriter(new StreamWriter(p_stream));
			jsonWriter.DateFormatHandling = DateFormatHandling.IsoDateFormat;
			new JsonSerializer().Serialize(jsonWriter, data, typeof(ReplayHeaderData));
			jsonWriter.Flush();
		}

		public void Serialize()
		{
			if (base.valid)
			{
				if (base.file != null)
				{
					base.file.Flush(flushToDisk: true);
					base.file.SetLength(0L);
				}
				Serialize(base.stream);
			}
		}

		public void Deserialize(Stream p_stream)
		{
			p_stream.Position = 0L;
			JsonTextReader jsonTextReader = new JsonTextReader(new StreamReader(p_stream));
			JsonSerializer jsonSerializer = new JsonSerializer();
			data.Clear();
			m_data = jsonSerializer.Deserialize<ReplayHeaderData>(jsonTextReader);
		}

		public void Deserialize()
		{
			if (base.valid)
			{
				Deserialize(base.stream);
			}
		}

		protected override void OnDestroy()
		{
			if (data != null)
			{
				data.Clear();
			}
			if ((bool)m_prev_physics_data)
			{
				DronePhysicsData.SetPool(m_prev_physics_data);
			}
			if ((bool)m_prev_rigdata)
			{
				DroneRigData.SetPool(m_prev_rigdata);
			}
		}
	}
}
