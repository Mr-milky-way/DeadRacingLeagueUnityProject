using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class GarageStateModel : Model<DRLApp>
	{
		public DroneRigData defaultRig;

		public Texture2D garageLoadBackground;

		public List<DroneRigData> templates;

		[SerializeField]
		private List<DroneRigData> m_originalRigs = new List<DroneRigData>();

		private bool m_originalsInitialized;

		private List<DroneRigData> m_clonedOriginalRigs;

		[SerializeField]
		private List<DroneRigData> m_officialRigs = new List<DroneRigData>();

		[SerializeField]
		private List<DroneRigData> m_cachedRigs = new List<DroneRigData>();

		[SerializeField]
		private List<DroneRigData> m_devOnlyRigs = new List<DroneRigData>();

		public DroneRigSpecData lastSavedRigSpecData;

		protected List<DroneRigData> m_rigs;

		public List<DRLCommunityDroneData> m_dronesData;

		private bool m_conversionInProgress;

		private DroneRigData m_activeRigData;

		private DroneRigData m_currentRigData;

		private bool m_currentRigDataSet;

		private bool m_rigsInitialized;

		private bool m_refreshRequested;

		public Dictionary<string, string> rigsGUIDName;

		private Dictionary<string, Texture2D> thumbnailCache = new Dictionary<string, Texture2D>();

		private Dictionary<string, WebAsyncRequest> thumbnailLoaders = new Dictionary<string, WebAsyncRequest>();

		public Texture2D[] fallbackThumbnails;

		[SerializeField]
		private GATechLookupData[] DragData;

		public string PartsWhitelist;

		public PlayerStateModel parent => AssertParent<PlayerStateModel>("parent");

		public DataFlow data => parent.data;

		public List<DroneRigData> originalRigs
		{
			get
			{
				if (m_originalRigs == null)
				{
					m_originalRigs = new List<DroneRigData>();
				}
				if (!m_originalsInitialized)
				{
					for (int i = 0; i < m_originalRigs.Count; i++)
					{
						if (m_originalRigs[i] == null)
						{
							m_originalRigs.RemoveAt(i--);
						}
						else
						{
							m_originalRigs[i].isOriginal = true;
						}
					}
					m_originalsInitialized = true;
				}
				return m_originalRigs;
			}
		}

		public List<DroneRigData> officialRigs
		{
			get
			{
				if (m_officialRigs == null)
				{
					m_officialRigs = new List<DroneRigData>();
				}
				return m_officialRigs;
			}
		}

		public List<DroneRigData> cachedRigs
		{
			get
			{
				if (m_cachedRigs == null)
				{
					m_cachedRigs = new List<DroneRigData>();
				}
				return m_cachedRigs;
			}
		}

		public List<DroneRigData> devOnlyRigs
		{
			get
			{
				if (m_devOnlyRigs == null)
				{
					m_devOnlyRigs = new List<DroneRigData>();
				}
				return m_devOnlyRigs;
			}
		}

		public List<DroneRigData> rigs
		{
			get
			{
				if (m_rigs == null)
				{
					PreloadDrones();
				}
				return m_rigs;
			}
		}

		public List<DRLCommunityDroneData> dronesData
		{
			get
			{
				if (m_dronesData == null)
				{
					PreloadDrones();
				}
				return m_dronesData;
			}
		}

		public DroneRigData activeRigData
		{
			protected get
			{
				if (m_activeRigData != null)
				{
					return m_activeRigData;
				}
				string text = data.Get("garage-active-rig", "");
				if (string.IsNullOrEmpty(text))
				{
					text = defaultRig.guid;
					m_activeRigData = defaultRig;
				}
				else
				{
					m_activeRigData = GetRigByGUID(text);
				}
				if (m_activeRigData == null)
				{
					m_activeRigData = GetOriginalByGUID(text);
				}
				if (m_activeRigData == null)
				{
					m_activeRigData = defaultRig;
				}
				data.Set("garage-active-rig", m_activeRigData.guid);
				Refresh();
				Notify("storage.drone@refresh");
				return m_activeRigData;
			}
			set
			{
				m_activeRigData = ((value == null) ? defaultRig : value);
				m_currentRigDataSet = false;
				data.Set("garage-active-rig", m_activeRigData.guid);
				Notify("storage.drone@refresh");
				Refresh();
			}
		}

		public DroneRigData currentRigData
		{
			get
			{
				if (m_currentRigDataSet)
				{
					return m_currentRigData;
				}
				if (m_rigsInitialized)
				{
					return activeRigData;
				}
				if (!m_refreshRequested)
				{
					m_refreshRequested = true;
					Activity.Run((Func<bool>)delegate
					{
						if (m_rigsInitialized)
						{
							_ = activeRigData;
							Notify("storage.drone@refresh");
						}
						return !m_rigsInitialized;
					}, 0f, false);
				}
				string text = data.Get("garage-active-rig", "");
				DroneRigData droneRigData = null;
				droneRigData = ((!string.IsNullOrEmpty(text)) ? GetRigByGUID(text) : defaultRig);
				if (droneRigData == null)
				{
					droneRigData = GetOriginalByGUID(text);
				}
				if (droneRigData == null)
				{
					droneRigData = defaultRig;
				}
				return droneRigData;
			}
			set
			{
				if (value == null)
				{
					m_currentRigDataSet = false;
				}
				else
				{
					m_currentRigDataSet = true;
					m_currentRigData = value;
				}
				Notify("storage.drone@refresh");
			}
		}

		public void FilterDronesData(int p_droneSize, int p_physics, DRLCommunityDroneData.SortType p_sort, string p_search, Action<List<DRLCommunityDroneData>> p_callback)
		{
			List<DRLCommunityDroneData> list = new List<DRLCommunityDroneData>();
			if (dronesData == null)
			{
				return;
			}
			_ = string.Empty;
			_ = string.Empty;
			string text = (string.IsNullOrEmpty(p_search) ? "" : p_search.Trim().ToLower());
			bool flag = !string.IsNullOrEmpty(text);
			bool flag2 = text.StartsWith("@");
			if (flag2)
			{
				text.Substring(1);
			}
			for (int i = 0; i < dronesData.Count; i++)
			{
				DRLCommunityDroneData dRLCommunityDroneData = dronesData[i];
				if (dRLCommunityDroneData == null)
				{
					continue;
				}
				string text2 = "";
				text2 = ((!flag2) ? dRLCommunityDroneData.droneName.ToLower() : dRLCommunityDroneData.profileName.ToLower());
				if (!flag || text2.Contains(text) || text.Contains(text2))
				{
					bool flag3 = p_physics == 1;
					if ((p_droneSize <= 0 || dRLCommunityDroneData.droneSize == p_droneSize) && (p_physics <= -1 || dRLCommunityDroneData.isCustomPhysics == flag3))
					{
						list.Add(dronesData[i]);
					}
				}
			}
			switch (p_sort)
			{
			case DRLCommunityDroneData.SortType.WeightAsc:
				list = list.OrderBy((DRLCommunityDroneData o) => o.droneWeight).ToList();
				break;
			case DRLCommunityDroneData.SortType.RatingCountDesc:
				list = list.OrderByDescending((DRLCommunityDroneData o) => o.rating).ToList();
				break;
			case DRLCommunityDroneData.SortType.ThrustDesc:
				list = list.OrderByDescending((DRLCommunityDroneData o) => o.droneThrust).ToList();
				break;
			case DRLCommunityDroneData.SortType.WeightDesc:
				list = list.OrderByDescending((DRLCommunityDroneData o) => o.droneWeight).ToList();
				break;
			case DRLCommunityDroneData.SortType.ScoreDesc:
				list = list.OrderByDescending((DRLCommunityDroneData o) => o.score).ToList();
				break;
			}
			p_callback(list);
		}

		protected void ConvertLegacyDrones()
		{
			if (m_conversionInProgress)
			{
				return;
			}
			m_conversionInProgress = true;
			string text = data.Get("garage-rigs", "*[]");
			if (text == "*[]")
			{
				return;
			}
			List<DroneRigData> rd;
			if (text.StartsWith("*"))
			{
				List<string> list = Serialize.FromJson<List<string>>(text.Substring(1));
				rd = new List<DroneRigData>(list.Count);
				foreach (string item in list)
				{
					rd.Add(DroneRigData.FromJson(item));
				}
			}
			else
			{
				List<DroneRigLegacyData> list2 = Serialize.FromJson<List<DroneRigLegacyData>>(text);
				rd = new List<DroneRigData>(list2.Count);
				foreach (DroneRigLegacyData item2 in list2)
				{
					DroneRigData droneRigData = ScriptableObject.CreateInstance<DroneRigData>();
					droneRigData.FromLegacy(item2);
					rd.Add(droneRigData);
				}
			}
			foreach (DroneRigData item3 in rd)
			{
				item3.guid = DroneRigData.GenerateGUID();
				m_rigs.Add(item3);
				rigsGUIDName.Add(item3.guid, item3.name);
			}
			data.Set("garage-rigs", "*[]");
			Refresh();
			bool inProgress = false;
			Activity.Run((Func<bool>)delegate
			{
				if (rd.Count == 0)
				{
					m_rigs.Clear();
					m_dronesData.Clear();
					m_rigs = null;
					rigsGUIDName.Clear();
					PreloadDrones();
					return false;
				}
				if (inProgress)
				{
					return true;
				}
				inProgress = true;
				base.app.model.service.SetCommunityDrones(rd[0], delegate(DRLCommunityDroneData p_data)
				{
					if (p_data == null)
					{
						Activity.RunOnce(delegate
						{
							inProgress = false;
						}, 10f);
					}
					else
					{
						inProgress = false;
						rd.RemoveAt(0);
					}
				});
				return true;
			}, 0f, false);
		}

		public void PreloadDrones()
		{
			if (GATechLookupStorage.DragData == null || GATechLookupStorage.DragData.Length == 0)
			{
				GATechLookupStorage.DragData = DragData;
			}
			if (m_rigs != null && m_dronesData != null)
			{
				return;
			}
			rigsGUIDName = new Dictionary<string, string>();
			m_rigs = new List<DroneRigData>();
			m_dronesData = new List<DRLCommunityDroneData>();
			if (data.Get("garage-rigs", "*[]") != "*[]")
			{
				ConvertLegacyDrones();
			}
			ServiceModel service = base.app.model.service;
			string playerId = base.app.model.storage.state.player.playerData.playerId;
			if (string.IsNullOrEmpty(playerId))
			{
				Debug.LogWarning("GarageStateModel> PreloadDrones / Invalid PlayerId");
			}
			DRLCommunityDroneData.SortType p_sort = DRLCommunityDroneData.SortType.None;
			if (!string.IsNullOrEmpty(playerId))
			{
				service.GetCommunityDrones(playerId, null, -1, -1, -1, -1, p_sort, null, delegate(DRLCommunityDroneResult p_result)
				{
					m_rigsInitialized = true;
					if (p_result != null && p_result.data != null)
					{
						DRLCommunityDroneData[] array = p_result.data;
						foreach (DRLCommunityDroneData dRLCommunityDroneData in array)
						{
							DroneRigData droneRigData = DroneRigData.FromJson(dRLCommunityDroneData.droneRigData);
							if (!rigsGUIDName.ContainsKey(droneRigData.guid))
							{
								m_dronesData.Add(dRLCommunityDroneData);
								droneRigData.isPublic = dRLCommunityDroneData.isPublic || IsOriginal(droneRigData);
								m_rigs.Add(droneRigData);
								rigsGUIDName.Add(droneRigData.guid, droneRigData.name);
							}
							else
							{
								Debug.LogError("GarageStateModel > PreloadDrones: duplicate GUID [" + droneRigData.guid + "] for drones [" + droneRigData.name + "] and [" + rigsGUIDName[droneRigData.guid] + "]");
							}
						}
					}
				});
			}
			StartCoroutine(PreloadThumbnails(0.05f));
		}

		private IEnumerator PreloadThumbnails(float p_delay = 0.1f)
		{
			yield return new WaitForSeconds(p_delay);
			GetRigThumbnail(defaultRig, 320, 0, null);
			yield return new WaitForSeconds(p_delay);
			foreach (DroneRigData officialRig in officialRigs)
			{
				GetRigThumbnail(officialRig, 320, 0, null);
				yield return new WaitForSeconds(p_delay);
			}
			foreach (DroneRigData cachedRig in cachedRigs)
			{
				GetRigThumbnail(cachedRig, 320, 0, null);
				yield return new WaitForSeconds(p_delay);
			}
			foreach (DroneRigData originalRig in originalRigs)
			{
				GetRigThumbnail(originalRig, 320, 0, null);
				yield return new WaitForSeconds(p_delay);
			}
			foreach (DroneRigData rig in rigs)
			{
				GetRigThumbnail(rig, 320, 0, null);
				yield return new WaitForSeconds(p_delay);
			}
		}

		public void ResetCurrentRigData()
		{
			currentRigData = activeRigData;
		}

		public DroneRigData GetRigByGUID(string p_guid, out int p_index)
		{
			List<DroneRigData> list = rigs;
			list.ConvertAll((DroneRigData it) => (!(it == null)) ? ("[" + it.guid + "] " + it.rigName) : "<null>");
			p_index = -1;
			for (int num = 0; num < list.Count; num++)
			{
				if (list[num].guid == p_guid)
				{
					p_index = num;
					return list[num];
				}
			}
			return null;
		}

		public int GetDroneDataByGUID(string p_guid)
		{
			_ = dronesData;
			for (int i = 0; i < dronesData.Count; i++)
			{
				if (dronesData[i].guid == p_guid)
				{
					return i;
				}
			}
			return -1;
		}

		public bool RigExists(string p_guid)
		{
			if (string.IsNullOrEmpty(p_guid))
			{
				return false;
			}
			for (int i = 0; i < rigs.Count; i++)
			{
				if (rigs[i].guid == p_guid)
				{
					return true;
				}
			}
			for (int j = 0; j < originalRigs.Count; j++)
			{
				if (originalRigs[j].guid == p_guid)
				{
					return true;
				}
			}
			for (int k = 0; k < officialRigs.Count; k++)
			{
				if (officialRigs[k].guid == p_guid)
				{
					return true;
				}
			}
			return false;
		}

		public bool RigExists(DroneRigData p_rig)
		{
			if (p_rig == null)
			{
				return false;
			}
			return RigExists(p_rig.guid);
		}

		public Dictionary<string, string> GetRigNames(bool p_lowercase = true)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> item in rigsGUIDName)
			{
				string value = (p_lowercase ? item.Value.ToLower() : item.Value);
				dictionary.Add(item.Key, value);
			}
			List<DroneRigData> list = originalRigs;
			for (int i = 0; i < list.Count; i++)
			{
				if (dictionary.ContainsKey(list[i].guid))
				{
					Debug.LogError("GarageStateModel > Duplicate rig GUID " + list[i].guid + " on " + list[i].name);
				}
				else
				{
					dictionary.Add(list[i].guid, p_lowercase ? list[i].name.ToLower() : list[i].name);
				}
			}
			return dictionary;
		}

		public DroneRigData GetRigByGUID(string p_guid)
		{
			int p_index = 0;
			return GetRigByGUID(p_guid, out p_index);
		}

		public bool UpdateRig(Drone p_drone)
		{
			if (p_drone == null)
			{
				return false;
			}
			DroneRigData droneRigData = p_drone.rig;
			if (droneRigData == null)
			{
				return false;
			}
			if (!string.IsNullOrEmpty(droneRigData.profile))
			{
				PlayerPrefs.SetString("drone-profile-" + droneRigData.guid, droneRigData.profile);
			}
			if (GetOriginalByGUID(droneRigData.guid) != null)
			{
				droneRigData = (p_drone.rig = droneRigData.Clone());
				droneRigData.name = "MY " + droneRigData.name.Replace("DRL ", "");
			}
			if (GetRigByGUID(droneRigData.guid, out var p_index) == null)
			{
				droneRigData.guid = DroneRigData.GenerateGUID();
				Debug.Log("Adding new rig = " + droneRigData.name + " " + droneRigData.guid);
				rigs.Add(droneRigData);
				rigsGUIDName.Add(droneRigData.guid, droneRigData.name);
			}
			else
			{
				rigs[p_index] = droneRigData;
				rigsGUIDName[droneRigData.guid] = droneRigData.name;
				Debug.Log("Updating ex rig = " + droneRigData.name + " " + droneRigData.guid);
			}
			int droneDataByGUID = GetDroneDataByGUID(droneRigData.guid);
			DRLCommunityDroneData drone;
			if (droneDataByGUID == -1)
			{
				drone = new DRLCommunityDroneData();
				drone.guid = droneRigData.guid;
				drone.isDroneOfficial = false;
				drone.playerId = base.app.model.service.backend.playerId;
				drone.profileName = base.app.model.service.platform.playerName;
			}
			else
			{
				drone = dronesData[droneDataByGUID];
			}
			drone.droneThumbURL = droneRigData.thumb1;
			drone.droneName = droneRigData.name;
			drone.isCustomPhysics = droneRigData.hasCustomPhysics;
			drone.droneSize = droneRigData.diameter;
			drone.isPublic = droneRigData.isPublic;
			if (p_drone.body != null)
			{
				drone.droneWeight = p_drone.body.weight;
				if (p_drone.body.frame != null && p_drone.body.frame.escs != null && p_drone.body.frame.escs.Count > 0)
				{
					DroneESC droneESC = p_drone.body.frame.escs[0];
					if (droneESC != null && droneESC.motor != null && droneESC.motor.spec != null && droneESC.motor.spec.data != null)
					{
						if (droneESC.motor.spec.data.thrustScale > 0f)
						{
							drone.droneThrust = 4f * droneESC.motor.spec.data.thrustScale;
						}
						else
						{
							drone.droneThrust = 4f * droneESC.motor.spec.data.GetMaxThrust();
						}
						drone.droneRPM = droneESC.motor.spec.data.GetMaxRPM();
					}
				}
			}
			if (p_drone.physics.mass > 0f)
			{
				drone.droneWeight = p_drone.physics.mass * 1000f;
			}
			if (p_drone.physics.thrust > 0f)
			{
				drone.droneThrust = 4f * p_drone.physics.thrust;
			}
			drone.droneSpeed = p_drone.EstimateTopSpeed();
			drone.droneFrameId = droneRigData.frame;
			drone.droneMotorId = droneRigData.motor;
			drone.dronePropId = droneRigData.prop;
			drone.droneBatteryId = droneRigData.battery;
			drone.droneRigData = droneRigData.ToJson();
			drone.dronePhysicsData = (string.IsNullOrEmpty(droneRigData.tune) ? null : droneRigData.tune);
			drone.droneProfileData = (string.IsNullOrEmpty(droneRigData.profile) ? null : droneRigData.profile);
			if (droneDataByGUID == -1)
			{
				dronesData.Add(drone);
			}
			base.app.model.service.SetCommunityDrones(drone, delegate(DRLCommunityDroneData p_result)
			{
				if (p_result == null)
				{
					Debug.LogError("GarageStateModel> Failed to save drone " + drone.droneName + " [" + drone.guid + "] to backend");
				}
				else
				{
					Debug.Log("GarageStateModel> Drone " + drone.droneName + " [" + drone.guid + "] saved");
				}
			});
			return true;
		}

		public bool DeleteRig(DroneRigData p_rig)
		{
			if (p_rig == null)
			{
				return false;
			}
			return DeleteRig(p_rig.guid);
		}

		public bool DeleteRig(string p_guid, Action<DRLServiceResult> p_callback = null)
		{
			if (string.IsNullOrEmpty(p_guid))
			{
				return false;
			}
			int p_index = -1;
			DroneRigData rigByGUID = GetRigByGUID(p_guid, out p_index);
			int droneDataByGUID = GetDroneDataByGUID(p_guid);
			if (rigByGUID == null)
			{
				return false;
			}
			rigs.RemoveAt(p_index);
			rigsGUIDName.Remove(p_guid);
			if (droneDataByGUID != -1)
			{
				dronesData.RemoveAt(droneDataByGUID);
			}
			base.app.model.service.RemoveCommunityDrones(p_guid, delegate(DRLServiceResult p_result)
			{
				if (p_result == null || !p_result.success)
				{
					Debug.LogError("GarageStateModel> Failed to delete drone [" + p_guid + "] on backend: " + p_result?.message);
				}
				else
				{
					Debug.Log("GarageStateModel> Drone [" + p_guid + "] deleted");
				}
				if (p_callback != null)
				{
					p_callback(p_result);
				}
			});
			return true;
		}

		public DroneRigData GetTemplate(int p_class)
		{
			for (int i = 0; i < templates.Count; i++)
			{
				if (templates[i].diameter == p_class)
				{
					return templates[i].Clone();
				}
			}
			return null;
		}

		public DroneRigData GetTemplateByGUID(string p_guid)
		{
			ScriptableObject.CreateInstance<DroneRigData>();
			for (int i = 0; i < templates.Count; i++)
			{
				if (templates[i].guid == p_guid)
				{
					return templates[i].Clone();
				}
			}
			return null;
		}

		public void GetRigThumbnail(DroneRigData p_data, int p_width, int p_height, Action<Texture2D> p_callback)
		{
			if (p_data == null)
			{
				Debug.LogError("no data");
			}
			else if (string.IsNullOrEmpty(p_data.guid))
			{
				Debug.LogError("empty guid on rig " + p_data.name + " rigName " + p_data.rigName + " motor " + p_data.motor);
			}
			else
			{
				GetRigThumbnail(p_data.guid, p_data.thumb1, p_width, p_height, p_callback);
			}
		}

		public void GetRigThumbnail(string p_guid, string p_url, int p_width, int p_height, Action<Texture2D> p_callback)
		{
			if (string.IsNullOrEmpty(p_guid))
			{
				Debug.LogError("no guid");
				return;
			}
			if (thumbnailCache.ContainsKey(p_guid))
			{
				if (p_callback != null)
				{
					p_callback(thumbnailCache[p_guid]);
				}
				return;
			}
			if (thumbnailLoaders.ContainsKey(p_guid))
			{
				thumbnailLoaders[p_guid].Cancel();
				thumbnailLoaders.Remove(p_guid);
			}
			if (string.IsNullOrEmpty(p_url))
			{
				return;
			}
			WebAsyncRequest image = base.app.model.service.GetImage(p_url, p_width, p_height, delegate(Texture2D p_result)
			{
				Texture2D texture2D = null;
				Texture2D texture2D2 = p_result;
				if ((bool)texture2D2 && texture2D2.width <= 8)
				{
					texture2D2 = null;
				}
				if (!texture2D2)
				{
					Texture2D[] array = fallbackThumbnails;
					foreach (Texture2D texture2D3 in array)
					{
						if (texture2D3.name == p_guid)
						{
							texture2D2 = texture2D3;
							break;
						}
					}
				}
				UpdateCachedThumbnail(p_guid, texture2D2);
				texture2D = (thumbnailCache.ContainsKey(p_guid) ? thumbnailCache[p_guid] : null);
				if (p_callback != null)
				{
					p_callback(texture2D);
				}
				thumbnailLoaders.Remove(p_guid);
			});
			thumbnailLoaders.Add(p_guid, image);
		}

		public void ClearCachedThumbnail(string p_guid)
		{
			if (thumbnailCache.ContainsKey(p_guid))
			{
				thumbnailCache.Remove(p_guid);
			}
		}

		public void UpdateCachedThumbnail(string p_guid, Texture2D p_thumbnail)
		{
			if (thumbnailCache.ContainsKey(p_guid))
			{
				if (!p_thumbnail)
				{
					thumbnailCache.Remove(p_guid);
				}
				else
				{
					thumbnailCache[p_guid] = p_thumbnail;
				}
			}
			else
			{
				thumbnailCache.Add(p_guid, p_thumbnail);
			}
		}

		public DroneRigData GetOriginalByFrame(string p_guid)
		{
			for (int i = 0; i < originalRigs.Count; i++)
			{
				if (originalRigs[i].frame == p_guid)
				{
					return originalRigs[i];
				}
			}
			for (int j = 0; j < templates.Count; j++)
			{
				if (templates[j].frame == p_guid)
				{
					return templates[j];
				}
			}
			return null;
		}

		public DroneRigData GetClonedOriginalbyFrame(string p_guid)
		{
			if (m_clonedOriginalRigs == null)
			{
				m_clonedOriginalRigs = new List<DroneRigData>();
				for (int i = 0; i < originalRigs.Count; i++)
				{
					DroneRigData droneRigData = originalRigs[i].Clone();
					droneRigData.isOriginal = true;
					droneRigData.guid = originalRigs[i].guid;
					m_clonedOriginalRigs.Add(droneRigData);
				}
				for (int j = 0; j < templates.Count; j++)
				{
					DroneRigData droneRigData2 = templates[j].Clone();
					droneRigData2.isOriginal = true;
					droneRigData2.guid = templates[j].guid;
					m_clonedOriginalRigs.Add(droneRigData2);
				}
			}
			for (int k = 0; k < m_clonedOriginalRigs.Count; k++)
			{
				if (m_clonedOriginalRigs[k].frame == p_guid)
				{
					return m_clonedOriginalRigs[k];
				}
			}
			return null;
		}

		public DroneRigData GetOriginalByGUID(string p_guid)
		{
			for (int i = 0; i < originalRigs.Count; i++)
			{
				if (originalRigs[i].guid == p_guid)
				{
					return originalRigs[i];
				}
			}
			return null;
		}

		public List<DroneRigData> GetOriginalRigs()
		{
			return originalRigs;
		}

		public DroneRigData GetFirstOriginalRigWithDiameter(int p_diameter)
		{
			for (int i = 0; i < originalRigs.Count; i++)
			{
				if (originalRigs[i].diameter == p_diameter)
				{
					return originalRigs[i];
				}
			}
			return null;
		}

		public DRLDroneRig GetDroneRigByGUID(string p_guid)
		{
			return base.app.model.storage.library.FindByGUID<DRLDroneRig>(p_guid);
		}

		public bool IsOfficial(DroneRigData p_rig = null)
		{
			if (p_rig == null)
			{
				p_rig = currentRigData;
			}
			if (p_rig == null)
			{
				return false;
			}
			if (p_rig.hasCustomPhysics)
			{
				return false;
			}
			if (officialRigs == null || officialRigs.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < officialRigs.Count; i++)
			{
				if (p_rig.FunctionallyIdentical(officialRigs[i]))
				{
					return true;
				}
			}
			return false;
		}

		public bool CanUseDamage(DroneRigData p_rig = null)
		{
			if (p_rig == null)
			{
				p_rig = currentRigData;
			}
			if (p_rig == null)
			{
				return false;
			}
			if (officialRigs == null || officialRigs.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < officialRigs.Count; i++)
			{
				if (p_rig.FunctionallyIdentical(officialRigs[i]))
				{
					return true;
				}
			}
			return false;
		}

		public DroneRigData SetOfficialRig()
		{
			if (IsOfficial())
			{
				return currentRigData;
			}
			currentRigData = officialRigs[0];
			return currentRigData;
		}

		public bool IsOriginal(DroneRigData p_rig)
		{
			if (p_rig == null)
			{
				return false;
			}
			if (p_rig.hasCustomPhysics)
			{
				return false;
			}
			if (originalRigs == null || originalRigs.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < originalRigs.Count; i++)
			{
				if (originalRigs[i].guid == p_rig.guid)
				{
					p_rig.isOriginal = true;
					return true;
				}
			}
			return false;
		}

		public bool TryGetBaseDrone(DroneRigData p_rig, out DroneRigData p_base_drone)
		{
			foreach (DroneRigData originalRig in originalRigs)
			{
				if (p_rig.FunctionallyIdentical(originalRig))
				{
					p_base_drone = originalRig;
					return true;
				}
			}
			foreach (DroneRigData officialRig in officialRigs)
			{
				if (p_rig.FunctionallyIdentical(officialRig))
				{
					p_base_drone = officialRig;
					return true;
				}
			}
			p_base_drone = null;
			return false;
		}

		public bool TryGetOfficialBaseDrone(DroneRigData p_rig, out DroneRigData p_official_drone)
		{
			foreach (DroneRigData officialRig in officialRigs)
			{
				if (p_rig.FunctionallyIdentical(officialRig))
				{
					p_official_drone = officialRig;
					return true;
				}
			}
			p_official_drone = null;
			return false;
		}

		public bool TryGetOriginalBaseDrone(DroneRigData p_rig, out DroneRigData p_original_drone)
		{
			foreach (DroneRigData originalRig in originalRigs)
			{
				if (p_rig.FunctionallyIdentical(originalRig))
				{
					p_original_drone = originalRig;
					return true;
				}
			}
			p_original_drone = null;
			return false;
		}

		public void ClearPhysicsOnOriginals()
		{
			if (originalRigs != null && originalRigs.Count != 0)
			{
				for (int i = 0; i < originalRigs.Count; i++)
				{
					originalRigs[i].tune = null;
				}
			}
		}

		public DroneRigSpecData GetDroneSpecData(DroneRigData p_rig)
		{
			DroneRigSpecData result = default(DroneRigSpecData);
			if (p_rig == null)
			{
				return result;
			}
			List<string> parts = p_rig.parts;
			AssetLibrary library = base.app.model.storage.library;
			DroneFrame droneFrame = null;
			DroneMotor droneMotor = null;
			DroneProp droneProp = null;
			DroneBattery droneBattery = null;
			float num = 0f;
			for (int i = 0; i < parts.Count; i++)
			{
				DronePart dronePart = library.FindByGUID<DronePart>(parts[i]);
				if (!(dronePart == null))
				{
					if (dronePart is DroneFrame)
					{
						droneFrame = dronePart as DroneFrame;
						num += dronePart.weight + droneFrame.extraWeight;
					}
					else if (dronePart is DroneBattery)
					{
						droneBattery = dronePart as DroneBattery;
						num += dronePart.weight;
					}
					else if (dronePart is DroneMotor)
					{
						droneMotor = dronePart as DroneMotor;
						num += dronePart.weight * 4f;
					}
					else if (dronePart is DroneProp)
					{
						droneProp = dronePart as DroneProp;
						num += dronePart.weight * 4f;
					}
					else
					{
						num = ((!(dronePart is DroneESC)) ? (num + dronePart.weight) : (num + dronePart.weight * 4f));
					}
				}
			}
			result.weight = num;
			DronePhysicsData dronePhysicsData = (string.IsNullOrEmpty(p_rig.tune) ? null : DronePhysicsData.FromJson(p_rig.tune));
			if ((bool)dronePhysicsData && dronePhysicsData.mass > 0f)
			{
				result.weight = dronePhysicsData.mass * 1000f;
			}
			if (!droneFrame)
			{
				return result;
			}
			if (!droneMotor)
			{
				return result;
			}
			if (!droneProp)
			{
				return result;
			}
			result.drag = droneFrame.cD.y;
			DroneMotorSpec spec = droneMotor.spec;
			if (!spec)
			{
				return result;
			}
			int p_cells = (droneBattery ? ((droneBattery.cells != null) ? droneBattery.cells.Length : 0) : 0);
			DroneMotorSpec.BenchData benchData = spec.GetBenchData(droneProp, p_cells);
			if (benchData == null || (benchData.overload && p_rig.rigName != "Goldberg"))
			{
				return result;
			}
			float num2 = ((dronePhysicsData != null && dronePhysicsData.thrust > 0f) ? dronePhysicsData.thrust : ((benchData.thrustScale > 0f) ? benchData.thrustScale : benchData.GetMaxThrust()));
			float num3 = ((dronePhysicsData != null && dronePhysicsData.torque > 0f) ? dronePhysicsData.torque : benchData.GetMaxTorque());
			float maxRPM = benchData.GetMaxRPM();
			result.thrust = num2 * 4f;
			result.topSpeed = p_rig.topSpeed;
			result.torque = ((p_rig.rigName != "Goldberg") ? (num3 * 4f) : 0.15165319f);
			result.rpm = maxRPM;
			result.temperature = ((p_rig.rigName != "Goldberg") ? benchData.temperature : 45f);
			result.dataURL = benchData.dataUrl;
			result.videoURL = benchData.videoUrl;
			result.efficiency = benchData.mechanicalEfficiency;
			return result;
		}

		public void SaveCurrentRigSpecData(DroneRigData p_rig)
		{
			DroneRigSpecData droneSpecData = GetDroneSpecData(p_rig);
			lastSavedRigSpecData.weight = droneSpecData.weight;
			lastSavedRigSpecData.thrust = droneSpecData.thrust;
			lastSavedRigSpecData.topSpeed = p_rig.topSpeed;
			lastSavedRigSpecData.torque = droneSpecData.torque;
			lastSavedRigSpecData.rpm = droneSpecData.rpm;
			lastSavedRigSpecData.drag = droneSpecData.drag;
			lastSavedRigSpecData.temperature = droneSpecData.temperature;
			lastSavedRigSpecData.efficiency = droneSpecData.efficiency;
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
