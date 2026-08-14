using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class ControllerProfileStateModel : Model<DRLApp>
	{
		public SettingsStateModel parent => AssertParent<SettingsStateModel>("parent");

		public DataFlow data => parent.data;

		public List<RCDeviceData> profiles
		{
			get
			{
				RCISettings rCISettings;
				string value;
				if (data.Contains("settings-controller-profiles"))
				{
					value = data.Get<string>("settings-controller-profiles");
					object obj = JsonConvert.DeserializeObject(value);
					if (obj is JObject)
					{
						rCISettings = ((JObject)obj).ToObject<RCISettings>();
						if (rCISettings.Version != 3)
						{
							rCISettings = RCISettings.AttemptToUpdateData(rCISettings.Version, rCISettings);
							data.Set("settings-controller-profiles", Serialize.ToJson(rCISettings));
							Refresh();
						}
						return rCISettings.Data;
					}
					if (obj is JArray)
					{
						rCISettings = RCISettings.AttemptToUpdateData(1, ((JArray)obj).ToObject<List<RCDeviceData>>());
						data.Set("settings-controller-profiles", Serialize.ToJson(rCISettings));
						Refresh();
						return rCISettings.Data;
					}
					rCISettings = RCISettings.AttemptToUpdateData(-1, obj);
					data.Set("settings-controller-profiles", Serialize.ToJson(rCISettings));
					Refresh();
					return rCISettings.Data;
				}
				rCISettings = new RCISettings();
				value = (string)data.Set("settings-controller-profiles", Serialize.ToJson(rCISettings));
				return rCISettings.Data;
			}
			set
			{
				RCISettings rCISettings = new RCISettings();
				if (value != null)
				{
					rCISettings.Data = value;
				}
				data.Set("settings-controller-profiles", Serialize.ToJson(rCISettings));
				Refresh();
			}
		}

		public string profileActiveGUID
		{
			get
			{
				bool num = data.Contains("settings-controller-profile-active-guid");
				RCDeviceData rCDeviceData = null;
				if (num)
				{
					string text = data.Get<string>("settings-controller-profile-active-guid");
					rCDeviceData = GetProfile(text);
					if (rCDeviceData != null)
					{
						return text;
					}
				}
				List<RCDeviceData> list = profiles;
				rCDeviceData = ((list.Count <= 0) ? null : list[0]);
				if (rCDeviceData != null)
				{
					profileActiveGUID = rCDeviceData.guid;
				}
				if (rCDeviceData != null)
				{
					return rCDeviceData.guid;
				}
				return "";
			}
			set
			{
				data.Set("settings-controller-profile-active-guid", value);
				Refresh();
			}
		}

		public void AddProfile(RCDeviceData p_data)
		{
			if (p_data != null)
			{
				List<RCDeviceData> list = profiles;
				if (GetProfile(list, p_data.guid) == null)
				{
					list.Add(p_data);
					profiles = list;
				}
			}
		}

		public void RemoveProfile(RCDeviceData p_data)
		{
			if (p_data != null)
			{
				List<RCDeviceData> list = profiles;
				list.RemoveAll((RCDeviceData it) => it.guid == p_data.guid);
				profiles = list;
			}
		}

		public RCDeviceData GetProfile(string p_guid)
		{
			return GetProfile(profiles, p_guid);
		}

		public void UpdateProfile(RCDeviceData p_data)
		{
			if (p_data == null)
			{
				return;
			}
			List<RCDeviceData> list = profiles;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].guid == p_data.guid)
				{
					list[i] = p_data;
				}
			}
			profiles = list;
		}

		protected RCDeviceData GetProfile(List<RCDeviceData> p_list, string p_guid)
		{
			if (string.IsNullOrEmpty(p_guid) || p_list == null)
			{
				return null;
			}
			return p_list.Find((RCDeviceData it) => it.guid == p_guid);
		}

		public bool IsActive(RCDeviceData p_data)
		{
			if (p_data != null)
			{
				return p_data.guid == profileActiveGUID;
			}
			return false;
		}

		public RCDeviceData GetActive()
		{
			string text = profileActiveGUID;
			List<RCDeviceData> list = profiles;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].guid == text)
				{
					return list[i];
				}
			}
			return null;
		}

		public void Refresh()
		{
			if ((bool)parent)
			{
				parent.Refresh();
			}
		}
	}
}
