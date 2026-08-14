using System.Collections.Generic;
using GPUInstancer;
using UnityEngine;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class TrackController : Controller<DRLApp>
	{
		private Transform cmb_root_parent;

		private Vector3 cmb_root_pos;

		private Vector3 cmb_root_rot;

		public TrackModel model => AssertLocal<TrackModel>("model");

		public LevelController level => AssertParent<LevelController>("level");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "scene.start":
				model.data = base.app.arguments.game.track;
				break;
			case "scene.game.scenes@complete":
			{
				model.containers = base.app.scene.GetTrackSceneObjects();
				Debug.Log("TrackController> GameScenesComplete / TrackBuildStart");
				Notify("scene.track.build@start");
				if (!model.root)
				{
					Debug.LogWarning("TrackController> Failed to find root GameObject!");
					Notify("scene.track.build@progress", 1f);
					Notify("scene.track.build@complete");
					Notify("game.track.load@complete");
					break;
				}
				MapData mapData = ((base.app.model.game.type == GameFlag.MapEditor) ? null : ((model.data.map.data != null) ? model.data.map.data.Clone() : null));
				if (mapData == null)
				{
					Debug.Log("TrackController> GameScenesComplete / Not a Custom Map");
					model.RefreshStarts();
					model.RefreshGates();
					model.GenerateLaps(1);
					RunOnce(1f, delegate
					{
						model.RefreshPath();
					});
					int lightingPreset = model.data.map.lightingPreset;
					if (lightingPreset >= 0)
					{
						level.SetLightingPreset(lightingPreset);
					}
					OnTrackBuildComplete();
					break;
				}
				Debug.Log("TrackController> GameScenesComplete / Custom Map Found");
				level.SetLightingPreset(mapData.mapLighting);
				if (mapData.root != null)
				{
					mapData.root.RefreshParenting();
				}
				model.SetTrackEnabled(mapData.trackId, p_flag: true);
				cmb_root_parent = model.root.transform.parent;
				cmb_root_pos = model.root.transform.position;
				cmb_root_rot = model.root.transform.eulerAngles;
				model.root.transform.parent = null;
				model.root.transform.localPosition = Vector3.zero;
				model.root.transform.localEulerAngles = Vector3.zero;
				string text = ((mapData == null) ? "" : mapData.podiumId);
				string podium_guid = model.data.podium;
				if (mapData.mapCategoryFlag == GameFlag.MapMultiGP)
				{
					podium_guid = "PD-265";
				}
				if (!string.IsNullOrEmpty(text))
				{
					Debug.Log("TrackController> GameScenesComplete / MapData Custom Podium GUID - podium-guid[" + text + "]");
					podium_guid = text;
				}
				if (!base.app.model.storage.library.FindByGUID<DronePodium>(podium_guid))
				{
					podium_guid = "PD-f20";
					Debug.LogWarning("TrackController> GameScenesComplete / Failed to find Podium Asset - podium-guid[" + podium_guid + "] fallback to " + podium_guid);
				}
				Debug.Log("TrackController> GameScenesComplete / Searching for podium - guid[" + podium_guid + " @ " + mapData.mapCategoryFlag.ToString() + "]");
				if (!string.IsNullOrEmpty(podium_guid))
				{
					level.factory.Traverse(mapData.root, delegate(MDEntity p, MDEntity n)
					{
						if (n is MDPodium || n.category == MapAssetType.Podium)
						{
							n.guid = podium_guid;
						}
					});
				}
				level.factory.inGame = true;
				bool is_gpu_instancing_thread = false;
				level.factory.Build(mapData.root, model.root.transform, delegate(MAEntity p_root, float p_progress, MAEntity p_node)
				{
					if ((bool)p_node)
					{
						OnCustomMapEntityBuild(p_node);
						float num = (is_gpu_instancing_thread ? (p_progress * 0.5f) : p_progress);
						Notify("scene.track.build@progress", num);
					}
					if (p_progress >= 1f)
					{
						OnCustomTrackBuildComplete(p_root);
					}
				}, p_async: true);
				break;
			}
			}
		}

		protected void OnCustomTrackBuildComplete(MAEntity p_root)
		{
			Vector3 position = Vector3.zero;
			model.rootMap = p_root;
			MapData mapData = ((base.app.model.game.type == GameFlag.MapEditor) ? null : ((model.data.map.data != null) ? model.data.map.data : null));
			List<MAGate> p_list = (p_root ? p_root.GetSortedGates() : new List<MAGate>());
			List<MAPodium> list = (p_root ? p_root.GetSortedPodiums() : new List<MAPodium>());
			List<MACameraTool> cameraTools = (p_root ? p_root.GetCameraTools() : new List<MACameraTool>());
			List<MASpline> courseCameras = (p_root ? p_root.GetCourseCameras() : new List<MASpline>());
			MAGate mAGate = (p_root ? p_root.GetFinishGate() : null);
			MAGate mAGate2 = (p_root ? p_root.GetLapStartGate() : null);
			MAGate mAGate3 = (p_root ? p_root.GetLapEndGate() : null);
			Collider p_finish_gate = (mAGate ? mAGate.trigger : null);
			Collider p_lap_start = (mAGate2 ? mAGate2.trigger : null);
			Collider p_lap_end = (mAGate3 ? mAGate3.trigger : null);
			model.cameraTools = cameraTools;
			model.courseCameras = courseCameras;
			if ((bool)p_root)
			{
				Hierarchy.Traverse(p_root.transform, delegate(MARenderer p_it)
				{
					if ((bool)p_it)
					{
						List<MapAssetType> tags = p_it.tags;
						if (tags != null && tags.Contains(MapAssetType.NoCollision))
						{
							p_it.SetHitEnabled(p_flag: false);
						}
					}
				});
			}
			if (!p_root)
			{
				Debug.LogWarning("TrackController> OnCustomTrackBuildComplete / Failed to build from MapData [" + mapData.guid + "]");
			}
			if ((bool)p_root)
			{
				position = p_root.transform.position;
			}
			model.ClearGates();
			if (list.Count > 0)
			{
				model.ClearStarts();
			}
			model.RefreshStarts(list);
			model.RefreshGates(p_list);
			model.GenerateLaps(mapData.mode.race.lapCount, p_finish_gate, p_lap_start, p_lap_end);
			RunOnce(1f, delegate
			{
				model.RefreshPath();
			});
			model.root.transform.SetParent(cmb_root_parent, worldPositionStays: true);
			model.root.transform.position = cmb_root_pos;
			model.root.transform.eulerAngles = cmb_root_rot;
			if ((bool)p_root)
			{
				p_root.transform.position = position;
			}
			level.model.SetAssetLayerIndex(0, mapData.mapAssetLayer0);
			level.model.SetAssetLayerIndex(1, mapData.mapAssetLayer1);
			level.model.SetAssetLayerIndex(2, mapData.mapAssetLayer2);
			level.model.settings.scene.SetStyle(0, mapData.mapStyle0);
			level.model.settings.scene.SetStyle(1, mapData.mapStyle1);
			level.model.settings.scene.SetStyle(2, mapData.mapStyle2);
			Debug.Log("TrackController> OnCustomTrackBuildComplete / track-id[" + mapData.trackId + "] root-map[" + p_root?.ToString() + "] gates[" + model.gates.Count + "] podiums[" + model.podiums.Count + "] laps[" + model.laps + "]");
			base.app.model.storage.PruneDependencies();
			OnTrackBuildComplete();
		}

		protected void OnTrackBuildComplete()
		{
			MapData mapData = ((base.app.model.game.type == GameFlag.MapEditor) ? null : ((model.data.map.data != null) ? model.data.map.data : null));
			string p_id = (model.data ? model.data.id : "");
			if (mapData != null)
			{
				p_id = mapData.trackId;
			}
			bool baseAssetsEnabled = mapData?.baseAssetsEnabled ?? true;
			model.SetTrackEnabled(p_id, p_flag: true);
			level.model.SetBaseAssetsEnabled(baseAssetsEnabled);
			model.RefreshNavMeshes();
			if (model.hasTrackAnimation)
			{
				Camera camera = Hierarchy.Find<Camera>(model.trackAnimation.transform);
				if ((bool)camera)
				{
					camera.gameObject.SetActive(value: false);
				}
			}
			GPUInstancerDetailManager gPUInstancerDetailManager = (level.model.settings ? level.model.settings.terrain.gpuInstancerDetailManager : null);
			Debug.Log($"TrackController> OnTrackBuildComplete / podiums[{model.podiums.Count}] gpu-instancing[{gPUInstancerDetailManager != null}]");
			Notify("scene.track.build@progress", 1f);
			Debug.Log("TrackController> OnTrackBuildComplete / TrackBuildComplete");
			Notify("scene.track.build@complete");
			Debug.Log("TrackController> OnTrackBuildComplete / TrackLoadComplete");
			Notify("game.track.load@complete");
		}

		protected void OnCustomMapEntityBuild(MAEntity p_entity)
		{
			if (!p_entity || p_entity.data == null)
			{
				return;
			}
			List<MAEntity> list = new List<MAEntity>();
			if (model.actions == null)
			{
				model.actions = new List<MapAssetAction>();
			}
			model.actions.AddRange(p_entity.actions);
			if (p_entity is MARenderer)
			{
				MARenderer mARenderer = p_entity as MARenderer;
				if (mARenderer.replacedGUID)
				{
					Debug.Log("TrackController> OnCustomMapEntityBuild / [" + mARenderer.name + "] GUID Replaced");
					mARenderer.ResetRendererMaterials();
				}
			}
			if (p_entity.data.category == MapAssetType.Renderer)
			{
				MARenderer mARenderer2 = p_entity as MARenderer;
				if ((bool)mARenderer2)
				{
					ModularScaleComponent component = mARenderer2.GetComponent<ModularScaleComponent>();
					if ((bool)component)
					{
						component.enabled = false;
					}
					mARenderer2.SetRenderersLayer(0, 2f);
				}
			}
			switch (p_entity.data.type)
			{
			case MapAssetType.Spline:
			{
				MASpline mASpline = p_entity as MASpline;
				if (mASpline == null)
				{
					break;
				}
				switch (mASpline.data.Get("spline-category", -1))
				{
				case 1:
					mASpline.isRaceSpline = true;
					break;
				case 2:
					mASpline.gameObject.SetActive(value: false);
					break;
				}
				mASpline.ClearInvalids();
				Timer.Set(mASpline, "enabled", 0.05f, false);
				SplineCategory splineCategory = mASpline.splineCategory;
				if (splineCategory != SplineCategory.Visual && (uint)(splineCategory - 1) <= 1u)
				{
					mASpline.splineRenderer.enabled = false;
					mASpline.splineRenderer.renderer.enabled = false;
					if (mASpline.splineCategory != SplineCategory.CourseCamera)
					{
						model.pathSpline = mASpline.spline;
					}
				}
				break;
			}
			case MapAssetType.CameraTool:
			{
				MACameraTool mACameraTool = p_entity as MACameraTool;
				if ((bool)mACameraTool)
				{
					mACameraTool.ClearInvalids();
					mACameraTool.gameObject.SetActive(value: false);
				}
				break;
			}
			case MapAssetType.Guide:
			case MapAssetType.SplineControlPoint:
			case MapAssetType.CameraToolControlPoint:
			{
				MAGuide mAGuide = p_entity as MAGuide;
				if ((bool)mAGuide)
				{
					mAGuide.SetRenderersEnabled(p_flag: false, p_force: true);
					mAGuide.SetHitEnabled(p_flag: false);
					mAGuide.gameObject.SetActive(value: false);
				}
				break;
			}
			case MapAssetType.Collectable:
			{
				MACollectable obj = p_entity as MACollectable;
				obj.SetCollision();
				obj.SetRenderersLayer(0, 2f);
				break;
			}
			}
			for (int i = 0; i < list.Count; i++)
			{
				if ((bool)list[i])
				{
					Object.Destroy(list[i].gameObject);
				}
			}
		}

		protected void OnDestroy()
		{
			Debug.Log("TrackController> Destroy / Cleaning out cached Materials!");
			MARenderer.ClearCache();
		}
	}
}
