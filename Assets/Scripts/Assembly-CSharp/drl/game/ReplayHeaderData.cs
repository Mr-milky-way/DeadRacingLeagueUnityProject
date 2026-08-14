using System;
using Newtonsoft.Json.Linq;
using thelab.core;

namespace drl.game
{
	[Serializable]
	public class ReplayHeaderData : SerializedData
	{
		private SerializedData m_drone_rig;

		private SerializedData m_fc_profile;

		private SerializedData m_physics_tune;

		public SerializedData droneRig
		{
			get
			{
				if (m_drone_rig != null)
				{
					return m_drone_rig;
				}
				object obj = Get<object>("drone-rig");
				if (obj is JObject)
				{
					JObject jObject = (JObject)obj;
					m_drone_rig = jObject.ToObject<SerializedData>();
				}
				if (obj is SerializedData)
				{
					m_drone_rig = (SerializedData)obj;
				}
				return m_drone_rig;
			}
			set
			{
				m_drone_rig = value;
				Set("drone-rig", m_drone_rig);
			}
		}

		public SerializedData fcProfile
		{
			get
			{
				if (m_fc_profile != null)
				{
					return m_fc_profile;
				}
				object obj = Get<object>("fc-profile");
				if (obj is JObject)
				{
					JObject jObject = (JObject)obj;
					m_fc_profile = jObject.ToObject<SerializedData>();
				}
				if (obj is SerializedData)
				{
					m_fc_profile = (SerializedData)obj;
				}
				return m_fc_profile;
			}
			set
			{
				m_fc_profile = value;
				Set("fc-profile", m_fc_profile);
			}
		}

		public SerializedData physicsTune
		{
			get
			{
				if (m_physics_tune != null)
				{
					return m_physics_tune;
				}
				object obj = Get<object>("physics-tune");
				if (obj is JObject)
				{
					JObject jObject = (JObject)obj;
					m_physics_tune = jObject.ToObject<SerializedData>();
				}
				if (obj is SerializedData)
				{
					m_physics_tune = (SerializedData)obj;
				}
				return m_physics_tune;
			}
			set
			{
				m_physics_tune = value;
				Set("physics-tune", m_physics_tune);
			}
		}

		public new void Clear()
		{
			base.Clear();
			if (m_drone_rig != null)
			{
				m_drone_rig.Clear();
			}
			if (m_fc_profile != null)
			{
				m_fc_profile.Clear();
			}
			if (m_physics_tune != null)
			{
				m_physics_tune.Clear();
			}
			m_drone_rig = null;
			m_fc_profile = null;
			m_physics_tune = null;
		}
	}
}
