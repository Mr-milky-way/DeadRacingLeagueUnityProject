using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MapEditorModel : Model<DRLApp>
	{
		public MAEntity root;

		public MapData data;

		public DRLMap map;

		public DRLMapTrack track;

		public bool willLoadGame;

		public GameFlag loadGameType;

		public bool lockInput;

		[SerializeField]
		private List<BlackboxRecord> m_cached_replays;

		[SerializeField]
		private List<ReplayFile> m_cached_replays_v2;

		public MESelectionModel selection => AssertFind<MESelectionModel>("selection");

		public MEStateModel state => Assert<MEStateModel>("state");

		public MEActionModel action => AssertFind<MEActionModel>("action");

		public List<BlackboxRecord> cachedReplays
		{
			get
			{
				if (m_cached_replays != null)
				{
					return m_cached_replays;
				}
				return m_cached_replays = new List<BlackboxRecord>();
			}
			set
			{
				m_cached_replays = ((value == null) ? new List<BlackboxRecord>() : new List<BlackboxRecord>(value));
			}
		}

		public List<ReplayFile> cachedReplaysV2
		{
			get
			{
				if (m_cached_replays_v2 != null)
				{
					return m_cached_replays_v2;
				}
				return m_cached_replays_v2 = new List<ReplayFile>();
			}
			set
			{
				m_cached_replays_v2 = ((value == null) ? new List<ReplayFile>() : new List<ReplayFile>(value));
			}
		}

		public int cachedReplaysCount
		{
			get
			{
				if (!ReplayFile.EnableVersion2)
				{
					return cachedReplays.Count;
				}
				return cachedReplaysV2.Count;
			}
		}

		public void OnRenderStateChange(MERenderStateType p_from, MERenderStateType p_to)
		{
			state.m_render = p_to;
			Notify("map-editor.render.state.change", p_from, p_to);
		}

		public void OnInputStateChange(MEInputStateType p_from, MEInputStateType p_to)
		{
			state.m_input = p_to;
			Notify("map-editor.input.state.change", p_from, p_to);
		}

		public void OnActionStateChange(MEActionStateType p_from, MEActionStateType p_to)
		{
			state.m_action = p_to;
			Notify("map-editor.action.state.change", p_from, p_to);
		}

		public void OnMetricModeChange(MEMetricMode p_from, MEMetricMode p_to)
		{
			state.metric.m_mode = p_to;
			Notify("map-editor.metric.mode.change", p_from, p_to);
		}

		public void OnPivotModeChange(MEHandlePivotType p_from, MEHandlePivotType p_to)
		{
			state.m_pivot = p_to;
			Notify("map-editor.pivot.state.change", p_from, p_to);
		}

		protected MapData GetDataGeneric(Action<MapData> p_callback)
		{
			if (data == null)
			{
				if (p_callback != null)
				{
					p_callback(null);
				}
				return null;
			}
			if (!root)
			{
				if (p_callback != null)
				{
					p_callback(null);
				}
				return null;
			}
			float rm_map_distance = 0f;
			int cm_collectable_count = 0;
			switch (data.mode.typeFlag)
			{
			case GameFlag.Freestyle:
			case GameFlag.Race:
				rm_map_distance = GetMapDistance();
				break;
			case GameFlag.Collectable:
				cm_collectable_count = GetMapCollectableCount();
				break;
			}
			Vector2Int mstats = GetRendererStats();
			MapData d = new MapData();
			Hierarchy.Traverse(root.transform, delegate(MAEntity it)
			{
				List<MapAssetType> list = (it ? it.tags : null);
				if (list != null && list.Contains(MapAssetType.NoSave))
				{
					return false;
				}
				MAEntity component = it.transform.parent.GetComponent<MAEntity>();
				MDEntity parent = (component ? component.data : null);
				it.data.ClearChildren();
				it.data.parent = parent;
				it.Write();
				return true;
			});
			ThreadStart threadStart = delegate
			{
				d.Load(data.ToJson());
				d.mapId = map.guid;
				d.mapTriangleCount = mstats.x;
				d.mapObjectCount = mstats.y;
				switch (d.mode.typeFlag)
				{
				case GameFlag.Freestyle:
				case GameFlag.Race:
					d.mode.race.distance = rm_map_distance;
					d.mode.race.allowed = rm_map_distance > 0f;
					break;
				case GameFlag.Collectable:
					d.mode.collectable.collectableCount = cm_collectable_count;
					break;
				}
				d.root = new MDEntity();
				d.root.Load(root.data.ToJson());
				if (p_callback != null)
				{
					Activity.RunOnce(delegate
					{
						p_callback(d);
					});
				}
			};
			if (p_callback != null)
			{
				new Thread(threadStart).Start();
			}
			else
			{
				threadStart();
			}
			if (p_callback != null)
			{
				return null;
			}
			return d;
		}

		public MapData GetData()
		{
			return GetDataGeneric(null);
		}

		public void GetData(Action<MapData> p_callback)
		{
			GetDataGeneric(p_callback);
		}

		public string CloneDataJson(MAEntity p_target, ref string p_id, Component p_parent = null)
		{
			Transform parent = p_target.transform.parent;
			Transform transform = (p_parent ? p_parent.transform : null);
			string text = p_target.name;
			string id = p_target.id;
			if ((bool)transform)
			{
				p_target.transform.SetParent(transform.transform, worldPositionStays: true);
			}
			p_target.name = text;
			p_target.name = (p_target.name.Contains("$") ? p_target.name.Split('$')[0] : p_target.name);
			p_target.name = p_target.name + "$" + UnityEngine.Random.Range(0, 512).ToString("x4");
			p_target.data.id = MDObject.GenerateId();
			p_target.Write();
			string id2 = p_target.data.id;
			string result = p_target.data.ToJson();
			if ((bool)transform)
			{
				p_target.transform.SetParent(parent, worldPositionStays: true);
			}
			p_target.name = text;
			p_target.data.id = id;
			p_target.Write();
			p_id = id2;
			return result;
		}

		public void Save(bool p_force = false)
		{
			GetData(delegate(MapData md)
			{
				if (!md.writeEnabled)
				{
					Debug.LogWarning("MapEditorModel> Map is <b>not</b> write enabled!");
					Notify("map-editor.save.map-data@blocked", md);
				}
				else
				{
					if (p_force)
					{
						md.mapDirty = true;
					}
					if (!md.mapDirty)
					{
						Debug.LogWarning("MapEditorModel> Save / Map is not Dirty yet");
					}
					else
					{
						Notify("map-editor.save.map-data@start");
						Activity.RunOnce(delegate
						{
							Write(md);
						}, 0.3f);
					}
				}
			});
		}

		public void Write(MapData p_data)
		{
			if (p_data == null)
			{
				Debug.LogWarning("MapEditorModel> Write / Invalid Map Data");
				return;
			}
			bool flag = true;
			bool will_upload = !DRLApp.offline;
			Debug.Log($"MapEditorModel> Write / save-disk[{flag}] will-upload[{will_upload}]");
			if (!flag)
			{
				return;
			}
			base.app.model.storage.maps.SaveCommunityMap(p_data.Clone(), delegate
			{
				this.TimerRunOnce(delegate
				{
					if (will_upload)
					{
						base.app.model.service.SetCommunityMaps(p_data, delegate(DRLCommunityMapData p_result)
						{
							if (p_result == null)
							{
								Debug.LogWarning("MapEditorModel> SetCommunityMaps / SaveMapDataError");
								Notify("map-editor.save.map-data@error");
							}
							else
							{
								Debug.Log("MapEditorModel> SetCommunityMaps / SaveMapDataSuccess");
								Notify("map-editor.save.map-data@success", p_data);
							}
						});
					}
					else
					{
						Debug.Log("MapEditorModel> SaveCommunityMap / SaveMapDataSuccess");
						Notify("map-editor.save.map-data@success", p_data);
					}
				}, 1f / 30f);
			}, p_is_map_editor: true);
		}

		public Vector2Int GetRendererStats()
		{
			Vector2Int vc = default(Vector2Int);
			Hierarchy.Traverse(root.transform, delegate(MARenderer it)
			{
				vc.x += it.triangleCount;
				vc.y++;
			});
			return vc;
		}

		public float GetMapDistance()
		{
			if (!root)
			{
				return 0f;
			}
			List<MAPodium> pl = new List<MAPodium>();
			List<MAGate> gl = new List<MAGate>();
			Hierarchy.Traverse(root.transform, delegate(MAEntity it)
			{
				if ((bool)it)
				{
					if (it is MAGate)
					{
						MAGate item = it as MAGate;
						gl.Add(item);
					}
					if (it is MAPodium)
					{
						MAPodium item2 = it as MAPodium;
						pl.Add(item2);
					}
				}
			});
			gl.RemoveAll((MAGate it) => it == null || !it.isTrigger);
			pl.RemoveAll((MAPodium it) => it == null);
			if (pl.Count <= 0)
			{
				return 0f;
			}
			if (gl.Count <= 0)
			{
				return 0f;
			}
			Transform transform = pl[0].transform;
			float num = Vector3.Distance(transform.position, gl[0].transform.position);
			for (int num2 = 1; num2 < pl.Count; num2++)
			{
				float num3 = Vector3.Distance(transform.position, pl[num2].transform.position);
				if (!(num3 >= num))
				{
					num = num3;
					transform = pl[num2].transform;
				}
			}
			List<Transform> list = new List<Transform>();
			list.Add(transform.transform);
			for (int num4 = 0; num4 < gl.Count; num4++)
			{
				list.Add(gl[num4].transform);
			}
			list.RemoveAll((Transform it) => it == null);
			float num5 = 0f;
			for (int num6 = 1; num6 < list.Count; num6++)
			{
				num5 += Vector3.Distance(list[num6 - 1].position, list[num6].position);
			}
			return num5;
		}

		public int GetMapCollectableCount()
		{
			if (!root)
			{
				return 0;
			}
			int c = 0;
			Hierarchy.Traverse(root.transform, delegate(MAEntity it)
			{
				if ((bool)it)
				{
					MACollectable component = it.GetComponent<MACollectable>();
					if ((bool)component && component.collectableMode == MapCollectableMode.Regular)
					{
						c++;
					}
				}
			});
			return c;
		}

		public void ApplySceneSettings()
		{
			LevelSettings settings = base.app.controller.game.model.level.settings;
			float num = settings.scene.assetsScale;
			if (num <= 0.01f)
			{
				num = 1f;
			}
			Vector3 vector = Vector3.one * num;
			root.transform.localScale = vector;
			state.preview.scale = vector;
			if (!base.app.model.storage.state.player.profile.isDeveloper)
			{
				Debug.Log("MapEditorModel> ApplySceneSettings / Not a developer - removing layers & styles");
				settings.scene.ClearAssetLayers("$dev");
				settings.scene.ClearStyles("$dev");
			}
		}

		protected void OnDestroy()
		{
			for (int i = 0; i < cachedReplaysV2.Count; i++)
			{
				cachedReplaysV2[i].Destroy();
			}
		}
	}
}
