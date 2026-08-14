using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MapsStorageModel : Model<DRLApp>
	{
		private List<MapData> m_maps;

		private bool m_cache_save_active;

		private float m_downloadTracksProgress;

		private WebAsyncRequest[] m_downloadTracksRequests;

		private List<DRLCommunityMapVersionData> m_mapVersions = new List<DRLCommunityMapVersionData>();

		public StorageModel storage => AssertParent<StorageModel>("storage");

		public List<MapData> maps
		{
			get
			{
				if (m_maps != null)
				{
					return m_maps;
				}
				return m_maps = new List<MapData>();
			}
		}

		public List<MapData> Find(bool p_race_only, params GameFlag[] p_categories)
		{
			return maps.FindAll(delegate(MapData it)
			{
				if (p_categories.Length == 0)
				{
					return true;
				}
				if (Array.IndexOf(p_categories, it.mapCategoryFlag) < 0)
				{
					return false;
				}
				return (!p_race_only || it.mode.race.allowed) ? true : false;
			});
		}

		public List<MapData> FetchSDMaps()
		{
			return maps.FindAll((MapData it) => it.mode.typeFlag == GameFlag.Collectable);
		}

		public List<MapData> Find(params GameFlag[] p_categories)
		{
			return Find(p_race_only: false, p_categories);
		}

		public List<MapData> Find(bool p_race_only, string p_map_id)
		{
			return maps.FindAll(delegate(MapData it)
			{
				if (it.mapId != p_map_id)
				{
					return false;
				}
				return !p_race_only || it.mode.race.allowed;
			});
		}

		public List<MapData> Find(string p_map_id)
		{
			return Find(p_race_only: false, p_map_id);
		}

		public List<MapData> FindFeatured()
		{
			return maps.FindAll((MapData o) => o.mapCategoryFlag == GameFlag.MapFeatured);
		}

		public MapData FindByGUID(string p_guid)
		{
			return maps.Find((MapData it) => it.guid == p_guid);
		}

		public List<MapData> FindByMapGUID(string p_guid)
		{
			return maps.FindAll((MapData it) => it.mapId == p_guid);
		}

		public void Populate(DRLCommunityMapData p_custom_map)
		{
			if (p_custom_map != null)
			{
				MapData mapData = maps.Find((MapData o) => o.guid == p_custom_map.guid);
				if (mapData != null)
				{
					maps.Remove(mapData);
				}
				MapData mapData2 = new MapData();
				mapData2.Load(p_custom_map.ToJson());
				maps.Add(mapData2);
			}
		}

		public void Populate(MapData p_map)
		{
			if (p_map != null)
			{
				MapData mapData = maps.Find((MapData o) => o.guid == p_map.guid);
				if (mapData != null)
				{
					maps.Remove(mapData);
				}
				if (p_map.IsAllowedOnPlatform())
				{
					maps.Add(p_map);
				}
			}
		}

		public void Populate(IList<DRLCommunityMapData> p_custom_maps)
		{
			for (int i = 0; i < p_custom_maps.Count; i++)
			{
				Populate(p_custom_maps[i]);
			}
		}

		public void Populate(List<MapData> p_maps)
		{
			if (p_maps != null)
			{
				for (int i = 0; i < p_maps.Count; i++)
				{
					Populate(p_maps[i]);
				}
			}
		}

		public async void SaveCache(List<MapData> p_maps, MapData[] p_newMaps, Action p_callback)
		{
			if (m_cache_save_active || p_maps == null || p_maps.Count == 0)
			{
				p_callback?.Invoke();
				return;
			}
			m_downloadTracksProgress = 0f;
			await StoreLiteCache(p_maps);
			await DownloadAndStoreTracks(p_newMaps, OnMapDownloadProgressUpdate, p_custom: false);
			while (m_downloadTracksProgress < 1f)
			{
				await Task.Delay(100);
			}
			if (m_downloadTracksRequests != null)
			{
				for (int i = 0; i < m_downloadTracksRequests.Length; i++)
				{
					if (m_downloadTracksRequests[i] != null && m_downloadTracksRequests[i].loader != null)
					{
						m_downloadTracksRequests[i].loader.Dispose();
					}
				}
			}
			Debug.Log("MapsStorageModel> All maps downloaded and stored...");
			m_downloadTracksRequests = null;
			m_cache_save_active = false;
			GCCollect();
			p_callback?.Invoke();
		}

		private void OnMapDownloadProgressUpdate(float p_progress)
		{
			m_downloadTracksProgress = p_progress;
			Notify("boot.drl.offline-maps.store@progress", m_downloadTracksProgress);
		}

		private async Task StoreLiteCache(List<MapData> p_maps)
		{
			string offlineMapsRoot = DRLPaths.Storage.offlineMapsRoot;
			string offlineMapsHash = DRLPaths.Storage.offlineMapsHash;
			List<string> list = new List<string>(Directory.GetFiles(offlineMapsRoot, "*.mdc"));
			m_cache_save_active = true;
			foreach (string item in list)
			{
				if (File.Exists(item))
				{
					File.Delete(item);
				}
			}
			Populate(p_maps);
			string contents = Serialize.ToJson(p_maps);
			string text = "";
			for (int i = 0; i < maps.Count; i++)
			{
				text += ((i > 0) ? "\n" : "");
				text = text + maps[i].guid + " " + maps[i].mapCategory + " " + maps[i].mapTitle;
			}
			if (File.Exists(offlineMapsHash))
			{
				File.Delete(offlineMapsHash);
			}
			File.WriteAllText(offlineMapsHash, contents);
			FileStream fileStream = new FileStream(offlineMapsHash, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
			Serialize.EncryptXOR(63, fileStream);
			fileStream.Close();
			fileStream.Dispose();
		}

		private async Task DownloadAndStoreTracks(MapData[] p_maps, Action<float> p_onProgressUpdate, bool p_custom)
		{
			if (p_maps == null || p_maps.Length == 0)
			{
				p_onProgressUpdate?.Invoke(1f);
				return;
			}
			string text = (p_custom ? DRLPaths.Storage.offlineMapsCustomRoot : DRLPaths.Storage.offlineMapsRoot);
			float progress = 0f;
			m_downloadTracksRequests = new WebAsyncRequest[p_maps.Length];
			for (int i = 0; i < p_maps.Length; i++)
			{
				if (string.IsNullOrEmpty(p_maps[i].fullTrackURL))
				{
					continue;
				}
				string path = text + p_maps[i].guid + ".cmp";
				m_downloadTracksRequests[i] = base.app.model.service.DownloadMap(path, p_maps[i].fullTrackURL, delegate
				{
					try
					{
						FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
						Serialize.EncryptXOR(63, fileStream);
						fileStream.Close();
						fileStream.Dispose();
						float num = progress;
						progress = num + 1f;
						p_onProgressUpdate?.Invoke(progress / (float)p_maps.Length);
					}
					catch (Exception ex)
					{
						Debug.Log("MapsStorageModel> Failed to store track: " + ex.Message);
						float num = progress;
						progress = num + 1f;
						p_onProgressUpdate?.Invoke(progress / (float)p_maps.Length);
					}
				});
			}
		}

		public void LoadCache(string p_version, Action<bool> p_on_complete)
		{
			string path = DRLPaths.Storage.offlineMapsRoot + p_version;
			byte[] data = null;
			string json = null;
			if (File.Exists(path))
			{
				data = File.ReadAllBytes(path);
				for (int i = 0; i < data.Length; i++)
				{
					data[i] ^= 0x3F;
				}
				json = Encoding.UTF8.GetString(data);
				if (string.IsNullOrEmpty(json))
				{
					p_on_complete?.Invoke(obj: false);
					return;
				}
				m_maps = new List<MapData>();
				if (m_maps == null)
				{
					m_maps = new List<MapData>();
				}
				else
				{
					m_maps.Clear();
				}
				Thread thread = new Thread((ThreadStart)delegate
				{
					try
					{
						m_maps = Serialize.FromJson<List<MapData>>(json);
					}
					catch
					{
						this.TimerRunOnce(delegate
						{
							data = null;
							json = null;
							Debug.Log("MapsStorageModel> Load maps cache failed, requires full download..");
							GCCollect();
							p_on_complete?.Invoke(obj: false);
						}, 1f / 60f);
						return;
					}
					this.TimerRunOnce(delegate
					{
						data = null;
						json = null;
						Debug.Log("MapStorageModel> Load maps cache success.");
						GCCollect();
						p_on_complete?.Invoke(obj: true);
					}, 1f / 60f);
				});
				thread.Start();
				thread.Priority = System.Threading.ThreadPriority.Highest;
			}
			else
			{
				p_on_complete?.Invoke(obj: false);
			}
		}

		public void LoadFromCache(string p_guid, Action<MapData> p_on_complete)
		{
			string text = DRLPaths.Storage.offlineMapsRoot + p_guid + ".cmp";
			string text2 = DRLPaths.Storage.offlineMapEditorMapsRoot + p_guid + ".cmp";
			string text3 = DRLPaths.Storage.offlineMapsCustomRoot + p_guid + ".cmp";
			DRLApp.LogMemStats("MapStorageModel> LoadFromCache / Start", p_show_delta: true);
			string path = (File.Exists(text2) ? text2 : null);
			if (string.IsNullOrEmpty(path))
			{
				path = (File.Exists(text) ? text : null);
			}
			if (string.IsNullOrEmpty(path))
			{
				path = (File.Exists(text3) ? text3 : null);
			}
			if (string.IsNullOrEmpty(path))
			{
				Debug.Log("MapsStorageModel> LoadFromCache: Failed to locate[" + p_guid + "]");
				p_on_complete?.Invoke(null);
				return;
			}
			new Thread((ThreadStart)delegate
			{
				byte[] data = null;
				string json = null;
				MapData md = null;
				try
				{
					data = File.ReadAllBytes(path);
					for (int i = 0; i < data.Length; i++)
					{
						data[i] ^= 0x3F;
					}
					json = Encoding.UTF8.GetString(data);
					DRLMapDataServiceResult dRLMapDataServiceResult = Serialize.FromJson<DRLMapDataServiceResult>(json);
					if (dRLMapDataServiceResult?.data?.data == null || dRLMapDataServiceResult.data.data.Length == 0)
					{
						this.TimerRunOnce(delegate
						{
							data = null;
							json = null;
							GCCollect();
							p_on_complete?.Invoke(null);
						}, 1f / 60f);
						return;
					}
					md = dRLMapDataServiceResult.data.data[0];
				}
				catch
				{
					this.TimerRunOnce(delegate
					{
						data = null;
						json = null;
						GCCollect();
						p_on_complete?.Invoke(null);
					}, 1f / 60f);
					return;
				}
				this.TimerRunOnce(delegate
				{
					data = null;
					json = null;
					GCCollect();
					DRLApp.LogMemStats("MapStorageModel> Complete", p_show_delta: true);
					p_on_complete?.Invoke(md);
				}, 1f / 60f);
			}).Start();
		}

		public void GetMapEditorLocalMaps(Action<List<Tuple<string, MapData>>> p_complete)
		{
			string offlineMapEditorMapsRoot = DRLPaths.Storage.offlineMapEditorMapsRoot;
			List<string> cache_files = new List<string>(Directory.GetFiles(offlineMapEditorMapsRoot, "*.cmp"));
			if (cache_files == null || cache_files.Count == 0)
			{
				if (p_complete != null)
				{
					p_complete(null);
				}
				return;
			}
			List<Tuple<string, MapData>> me_maps = new List<Tuple<string, MapData>>();
			new Thread((ThreadStart)delegate
			{
				try
				{
					for (int i = 0; i < cache_files.Count; i++)
					{
						byte[] array = (File.Exists(cache_files[i]) ? File.ReadAllBytes(cache_files[i]) : null);
						if (array != null)
						{
							for (int j = 0; j < array.Length; j++)
							{
								array[j] ^= 0x3F;
							}
							string p_json = Encoding.UTF8.GetString(array);
							MapData mapData = new MapData();
							mapData.Load(p_json);
							me_maps.Add(new Tuple<string, MapData>(cache_files[i], mapData));
						}
					}
					if (p_complete != null)
					{
						this.TimerRunOnce(delegate
						{
							p_complete(me_maps);
						}, 1f / 60f);
					}
				}
				catch (Exception ex)
				{
					Debug.Log("MapStorageModel> Failed to get map editor maps: " + ex.Message);
					if (p_complete != null)
					{
						this.TimerRunOnce(delegate
						{
							p_complete(me_maps);
						}, 1f / 60f);
					}
				}
			}).Start();
		}

		public void GetMapEditorImages(Action<List<Texture2D>> p_complete)
		{
			string offlineMapEditorMapsRoot = DRLPaths.Storage.offlineMapEditorMapsRoot;
			List<string> cache_files = new List<string>(Directory.GetFiles(offlineMapEditorMapsRoot, "*.jpg"));
			if (cache_files == null || cache_files.Count == 0)
			{
				if (p_complete != null)
				{
					p_complete(null);
				}
				return;
			}
			List<Texture2D> me_map_images = new List<Texture2D>();
			new Thread((ThreadStart)delegate
			{
				for (int i = 0; i < cache_files.Count; i++)
				{
					byte[] array = (File.Exists(cache_files[i]) ? File.ReadAllBytes(cache_files[i]) : null);
					if (array != null)
					{
						Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
						texture2D.LoadImage(array, markNonReadable: false);
						texture2D.hideFlags = HideFlags.HideAndDontSave;
						me_map_images.Add(texture2D);
					}
				}
				if (p_complete != null)
				{
					this.TimerRunOnce(delegate
					{
						p_complete(me_map_images);
					}, 1f / 60f);
				}
			}).Start();
		}

		public void GetMapEditorImages(Action<List<Tuple<string, byte[]>>> p_complete)
		{
			string offlineMapEditorMapsRoot = DRLPaths.Storage.offlineMapEditorMapsRoot;
			List<string> cache_files = new List<string>(Directory.GetFiles(offlineMapEditorMapsRoot, "*.jpg"));
			if (cache_files == null || cache_files.Count == 0)
			{
				if (p_complete != null)
				{
					p_complete(null);
				}
				return;
			}
			List<Tuple<string, byte[]>> me_map_images = new List<Tuple<string, byte[]>>();
			new Thread((ThreadStart)delegate
			{
				for (int i = 0; i < cache_files.Count; i++)
				{
					byte[] array = (File.Exists(cache_files[i]) ? File.ReadAllBytes(cache_files[i]) : null);
					if (array != null)
					{
						me_map_images.Add(new Tuple<string, byte[]>(cache_files[i], array));
					}
				}
				if (p_complete != null)
				{
					this.TimerRunOnce(delegate
					{
						p_complete(me_map_images);
					}, 1f / 60f);
				}
			}).Start();
		}

		public bool HasCache(string p_version)
		{
			return File.Exists(DRLPaths.Storage.offlineMapsRoot + p_version);
		}

		public string GetCache(string p_version)
		{
			if (!File.Exists(DRLPaths.Storage.offlineMapsRoot + p_version))
			{
				return null;
			}
			return File.ReadAllText(DRLPaths.Storage.offlineMapsHash);
		}

		public void SaveCommunityMap(MapData p_mapData, Action p_complete, bool p_is_map_editor = false)
		{
			if (p_mapData == null)
			{
				return;
			}
			string text = (p_is_map_editor ? DRLPaths.Storage.offlineMapEditorMapsRoot : DRLPaths.Storage.offlineMapsCustomRoot);
			string hash_filepath = (p_is_map_editor ? (DRLPaths.Storage.offlineMapEditorMapsRoot + DRLPaths.Storage.offlineMapEditorMapsHash) : DRLPaths.Storage.offlineMapsCustomHash);
			string map_filepath = text + p_mapData.guid + ".cmp";
			LoadCustomMapCache(delegate(List<DRLCommunityMapData> p_result)
			{
				new Thread((ThreadStart)delegate
				{
					DRLMapDataResult data = new DRLMapDataResult
					{
						data = new MapData[1] { p_mapData }
					};
					string s = Serialize.ToJson(new DRLMapDataServiceResult
					{
						data = data
					});
					byte[] bytes = Encoding.UTF8.GetBytes(s);
					for (int i = 0; i < bytes.Length; i++)
					{
						bytes[i] ^= 0x3F;
					}
					if (File.Exists(map_filepath))
					{
						File.Delete(map_filepath);
					}
					File.WriteAllBytes(map_filepath, bytes);
					if (p_is_map_editor)
					{
						Debug.Log("SaveCommunityMap> Saving map editor map " + p_mapData.guid);
					}
					else
					{
						Debug.Log("SaveCommunityMap> Saving community map " + p_mapData.guid);
					}
					p_mapData.root = null;
					DRLCommunityMapData dRLCommunityMapData = new DRLCommunityMapData();
					dRLCommunityMapData.Load(p_mapData.ToJson());
					List<DRLCommunityMapData> list = ((p_result == null) ? new List<DRLCommunityMapData>() : p_result);
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].guid == dRLCommunityMapData.guid)
						{
							list.RemoveAt(j--);
						}
					}
					list.Add(dRLCommunityMapData);
					string s2 = Serialize.ToJson(list);
					if (File.Exists(hash_filepath))
					{
						File.Delete(hash_filepath);
					}
					byte[] bytes2 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(s2);
					using (FileStream fileStream = new FileStream(hash_filepath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite))
					{
						fileStream.Write(bytes2, 0, bytes2.Length);
						fileStream.Flush(flushToDisk: true);
						fileStream.Close();
					}
					Debug.Log("MapStorageModel> SaveCommunityMap / Cache Update - guid[" + p_mapData.guid + "]");
					this.TimerRunOnce(delegate
					{
						GCCollect();
						if (p_complete != null)
						{
							p_complete();
						}
					}, 1f / 30f);
				}).Start();
			}, p_is_map_editor);
		}

		public void StoreCommunityMaps(MapData[] p_communityMaps, Action p_complete)
		{
			if (p_communityMaps == null || p_communityMaps.Length == 0)
			{
				return;
			}
			_ = DRLPaths.Storage.offlineMapsCustomRoot;
			string hash_filepath = DRLPaths.Storage.offlineMapsCustomHash;
			string mjs = null;
			byte[] mjrb = null;
			DownloadAndStoreTracks(p_communityMaps, delegate(float progress)
			{
				if (!(progress < 1f))
				{
					LoadCustomMapCache(delegate(List<DRLCommunityMapData> p_result)
					{
						List<DRLCommunityMapData> m = ((p_result == null) ? new List<DRLCommunityMapData>() : p_result);
						new Thread((ThreadStart)delegate
						{
							foreach (MapData mapData in p_communityMaps)
							{
								DRLCommunityMapData dRLCommunityMapData = new DRLCommunityMapData();
								dRLCommunityMapData.Load(mapData.ToJson());
								for (int j = 0; j < m.Count; j++)
								{
									if (m[j].guid == dRLCommunityMapData.guid)
									{
										m.RemoveAt(j--);
									}
								}
								m.Add(dRLCommunityMapData);
							}
							string js = Serialize.ToJson(m);
							if (File.Exists(hash_filepath))
							{
								File.Delete(hash_filepath);
							}
							byte[] bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(js);
							using (FileStream fileStream = new FileStream(hash_filepath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite))
							{
								fileStream.Write(bytes, 0, bytes.Length);
								fileStream.Flush(flushToDisk: true);
								fileStream.Close();
							}
							Debug.Log("MapStorageModel> SaveCommunityMap / Cache Updated.");
							this.TimerRunOnce(delegate
							{
								js = null;
								mjs = null;
								mjrb = null;
								GCCollect();
								if (p_complete != null)
								{
									p_complete();
								}
							}, 1f / 30f);
						}).Start();
					});
				}
			}, p_custom: true);
		}

		public void DeleteLocalCommunityMap(string p_guid, bool p_is_map_editor = false, Action p_callback = null)
		{
			if (string.IsNullOrEmpty(p_guid))
			{
				return;
			}
			string obj = (p_is_map_editor ? DRLPaths.Storage.offlineMapEditorMapsRoot : DRLPaths.Storage.offlineMapsCustomRoot);
			string hash_filepath = (p_is_map_editor ? (DRLPaths.Storage.offlineMapEditorMapsRoot + DRLPaths.Storage.offlineMapEditorMapsHash) : DRLPaths.Storage.offlineMapsCustomHash);
			string path = obj + p_guid + ".cmp";
			string path2 = obj + p_guid + ".jpg";
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			if (File.Exists(path2))
			{
				File.Delete(path2);
			}
			LoadCustomMapCache(delegate(List<DRLCommunityMapData> p_maps)
			{
				new Thread((ThreadStart)delegate
				{
					if (p_maps != null && p_maps.Count != 0)
					{
						for (int i = 0; i < p_maps.Count; i++)
						{
							if (p_maps[i].guid == p_guid)
							{
								p_maps.RemoveAt(i--);
							}
						}
						if (p_maps.Count == 0)
						{
							if (File.Exists(hash_filepath))
							{
								File.Delete(hash_filepath);
							}
						}
						else
						{
							string contents = Serialize.ToJson(p_maps);
							File.WriteAllText(hash_filepath, contents);
						}
						Debug.Log("DeleteLocalCommunityMap> Removed map from root-less cache " + p_guid);
						this.TimerRunOnce(delegate
						{
							if (p_callback != null)
							{
								p_callback();
							}
						}, 1f / 30f);
					}
				}).Start();
			}, p_is_map_editor);
		}

		public void LoadCustomMapCache(Action<List<DRLCommunityMapData>> p_complete, bool p_is_map_editor = false)
		{
			string hash_filepath = (p_is_map_editor ? (DRLPaths.Storage.offlineMapEditorMapsRoot + DRLPaths.Storage.offlineMapEditorMapsHash) : DRLPaths.Storage.offlineMapsCustomHash);
			if (!File.Exists(hash_filepath))
			{
				if (p_complete != null)
				{
					p_complete(null);
				}
				return;
			}
			new Thread((ThreadStart)delegate
			{
				string p_data = File.ReadAllText(hash_filepath);
				List<DRLCommunityMapData> m = Serialize.FromJson<List<DRLCommunityMapData>>(p_data);
				this.TimerRunOnce(delegate
				{
					if (p_complete != null)
					{
						p_complete(m);
					}
				}, 1f / 30f);
			}).Start();
		}

		public void GetLocalMapEditorMaps(int p_is_race_allowed, int p_page, int p_total, Action<DRLCommunityMapResult> p_callback)
		{
			string maps_root = DRLPaths.Storage.offlineMapEditorMapsRoot;
			LoadCustomMapCache(delegate(List<DRLCommunityMapData> map_list)
			{
				if (map_list == null)
				{
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					for (int i = 0; i < map_list.Count; i++)
					{
						string text = maps_root + map_list[i].guid + ".jpg";
						map_list[i].mapThumbURL = (File.Exists(text) ? ("$" + text) : "");
						if (p_is_race_allowed == 1 && !map_list[i].isRaceAllowed)
						{
							map_list.RemoveAt(i--);
						}
					}
					Mathf.Min(p_page * p_total + p_total, map_list.Count);
					DRLCommunityMapResult dRLCommunityMapResult = new DRLCommunityMapResult();
					dRLCommunityMapResult.pagging = new DRLServicePageData();
					dRLCommunityMapResult.pagging.page = p_page;
					dRLCommunityMapResult.pagging.pageTotal = (map_list.Count - 1) / p_total + 1;
					dRLCommunityMapResult.data = map_list.ToArray();
					if (p_callback != null)
					{
						p_callback(dRLCommunityMapResult);
					}
				}
			}, p_is_map_editor: true);
		}

		public void GetLocalMaps(Action<List<MapData>> p_callback)
		{
			List<DRLCommunityMapData> custom_maps = new List<DRLCommunityMapData>();
			LoadCustomMapCache(delegate(List<DRLCommunityMapData> map_editor_maps)
			{
				if (map_editor_maps != null && map_editor_maps.Count > 0)
				{
					for (int i = 0; i < map_editor_maps.Count; i++)
					{
						custom_maps.Add(map_editor_maps[i]);
					}
				}
				LoadCustomMapCache(delegate(List<DRLCommunityMapData> community_maps)
				{
					if (community_maps != null && community_maps.Count > 0)
					{
						for (int j = 0; j < community_maps.Count; j++)
						{
							custom_maps.Add(community_maps[j]);
						}
					}
					LoadCache(DRLPaths.Storage.offlineMapsHashFilename, delegate
					{
						List<MapData> maps = ((m_maps != null) ? new List<MapData>(m_maps) : new List<MapData>());
						new Thread((ThreadStart)delegate
						{
							for (int k = 0; k < custom_maps.Count; k++)
							{
								maps.Add(custom_maps[k].Convert<MapData>());
							}
							this.TimerRunOnce(delegate
							{
								if (p_callback != null)
								{
									p_callback(maps);
								}
							}, 1f / 30f);
						}).Start();
					});
				});
			}, p_is_map_editor: true);
		}

		public void SyncLocalMapVersions(Action p_callback)
		{
			m_mapVersions.Clear();
			LoadCustomMapCache(delegate(List<DRLCommunityMapData> custom_maps)
			{
				int num = custom_maps?.Count ?? 0;
				for (int i = 0; i < maps.Count + num; i++)
				{
					if (i < maps.Count)
					{
						m_mapVersions.Add(new DRLCommunityMapVersionData(maps[i].guid, maps[i].version));
					}
					else if (custom_maps != null)
					{
						m_mapVersions.Add(new DRLCommunityMapVersionData(custom_maps[i - maps.Count].guid, custom_maps[i - maps.Count].version));
					}
				}
				base.app.model.service.SyncLocalMapVersions(m_mapVersions, delegate
				{
					m_mapVersions.Clear();
					p_callback?.Invoke();
				});
			});
		}

		public void ClearMapEditorCache()
		{
			string path = DRLPaths.Storage.offlineMapEditorMapsRoot + DRLPaths.Storage.offlineMapEditorMapsHash;
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		public void ClearCommunityMapsCache()
		{
			string offlineMapsCustomHash = DRLPaths.Storage.offlineMapsCustomHash;
			if (File.Exists(offlineMapsCustomHash))
			{
				File.Delete(offlineMapsCustomHash);
			}
		}

		public void ClearLocalCache()
		{
			string offlineMapsHash = DRLPaths.Storage.offlineMapsHash;
			if (File.Exists(offlineMapsHash))
			{
				File.Delete(offlineMapsHash);
			}
		}

		public void ClearCache()
		{
			ClearLocalCache();
			ClearCommunityMapsCache();
			ClearMapEditorCache();
			Debug.Log("MapsStorageModel> Cleared local maps cache.");
		}

		[ContextMenu("Test Encryption")]
		public void TestEncryption()
		{
			FileStream fileStream = new FileStream(DRLPaths.Storage.offlineMapsRoot + "test_file.tft", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
			Serialize.EncryptXOR(63, fileStream);
			fileStream.Close();
		}

		private void GCCollect()
		{
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.Default;
		}
	}
}
