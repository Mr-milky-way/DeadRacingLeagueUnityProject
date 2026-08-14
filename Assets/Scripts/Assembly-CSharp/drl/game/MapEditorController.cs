using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class MapEditorController : GameTypeController
	{
		public UIMapEditorController screen;

		private MonoActivity m_asset_filter_timer;

		private Activity m_preview_loop;

		private float m_raycast_rate = 0.03f;

		private Activity m_save_timer;

		private Activity m_saveplay_timeout;

		private MonoActivity m_map_distance_timer;

		private Activity m_renderer_stats_timer;

		public MapEditorModel model => AssertLocal<MapEditorModel>("model");

		public MapEditorView view => AssertLocal<MapEditorView>("view");

		public MEActionController action => AssertFind<MEActionController>("action");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!base.validContext)
			{
				return;
			}
			switch (p_event)
			{
			case "game.boot":
			{
				if (!CheckActivation())
				{
					break;
				}
				model.data = base.app.arguments.game.map.data;
				model.map = base.app.arguments.game.map;
				model.track = base.app.arguments.game.track;
				MECamera camera2 = view.camera;
				camera2.gameObject.SetActive(value: true);
				camera2.SetFreeCamera();
				base.game.model.camera = camera2;
				base.game.model.level.radio.enabled = false;
				Notify("game.simulation.camera@add", camera2);
				base.game.input.SetController(this);
				base.game.input.listening = true;
				base.game.input.FindMap("map-editor").commands.RemoveAll((GameCommand it) => it.hash.Contains("@dev"));
				view.ui = base.app.view.ui.screens.Open<UIMapEditorView>("map-editor-screen");
				view.ui.assetLibraryPanel.libraryList = new List<string> { "bundle-dynamic", "pack-me-shared-library" };
				view.ui.assetLibraryPanel.groupList = new List<string>(model.map.allowedAssetGroups);
				Debug.Log("MapEditorController> Allowed Libraries\n" + string.Join("\n", view.ui.assetLibraryPanel.libraryList));
				Debug.Log("MapEditorController> Allowed Groups\n" + string.Join("\n", view.ui.assetLibraryPanel.groupList));
				if (ReplayFile.EnableVersion2)
				{
					List<ReplayFile> list = base.app.model.storage.replays.ReadMapEditorReplaysV2(ReplayCacheFilter, 6);
					for (int num = 0; num < list.Count; num++)
					{
						ReplayHeader header = list[num].header;
						header.profileName = "REPLAY " + (num + 1).ToString("00");
						header.profileColor = DRLColor.profileTryoutsColors[num % DRLColor.profileTryoutsColors.Length];
					}
					model.cachedReplaysV2 = list;
				}
				else
				{
					List<BlackboxRecord> list2 = base.app.model.storage.replays.ReadMapEditorReplays(ReplayCacheFilter, 6);
					for (int num2 = 0; num2 < list2.Count; num2++)
					{
						BlackboxRecord blackboxRecord = list2[num2];
						int num3 = num2;
						if (blackboxRecord.clips.Count > 0)
						{
							SerializedData header2 = list2[num2].clips[0].header;
							header2.Set("profile-name", "REPLAY " + (num3 + 1).ToString("00"));
							num3 %= DRLColor.profileTryoutsColors.Length;
							string v = Colorf.ColorToRGB(DRLColor.profileTryoutsColors[num3]).ToString("x6");
							header2.Set("profile-color", v);
							list2[num2].clips[0].header = header2;
						}
					}
					model.cachedReplays = list2;
				}
				view.ui.SetReplayCacheCount(model.cachedReplaysCount);
				base.app.view.ui.screens.controller.BlockDark();
				screen = view.ui.GetComponent<UIMapEditorController>();
				screen.editor = this;
				Notify(0.01f, "game.ready");
				Notify(0.01f, "map-editor.ready");
				return;
			}
			case "map-editor.ready":
			{
				if (base.app.arguments.game.type != GameFlag.MapEditor)
				{
					break;
				}
				LevelModel level = base.game.model.level;
				MECamera camera = view.camera;
				Transform p_container = view.scene.transform;
				_ = base.app.model.storage.state.player.profile.isDeveloper;
				model.state.input = MEInputStateType.Action;
				model.state.render = MERenderStateType.Scene;
				model.state.action = MEActionStateType.Select;
				view.ui.gizmoGridState = MapEditorGridStateType.Off;
				view.camera.SetHilight(false);
				view.camera.SetSelection(false);
				view.camera.SetMotionBlurEnabled(p_flag: false);
				view.camera.orbit.transition = (OrbitTransform.Transition)8738;
				base.game.model.level.radio.enabled = true;
				camera.fx.radio = 1f;
				base.game.model.level.radio.UpdateTarget();
				MapData data = model.data;
				view.ui.SetMapModeInfo(data);
				view.ui.SetRendererStats(data.mapTriangleCount, data.mapObjectCount);
				RefreshRendererStats();
				view.ui.SetUndoEnabled(p_flag: false);
				view.ui.SetRedoEnabled(p_flag: false);
				Debug.Log($"MapEditorController> MapData [{data.mapTitle}/{data.mapDifficulty}]");
				base.game.level.factory.inGame = false;
				model.root = base.game.level.factory.Build(data.root, p_container, delegate(MAEntity p_root, float p_progress, MAEntity p_node)
				{
					if ((bool)p_node)
					{
						OnMapEntityBuild(p_node);
					}
				});
				model.ApplySceneSettings();
				view.scene.LoadHierarchy();
				view.SetEditorCameraStart(base.game.model.camera);
				model.state.cameraPosition = camera.transform.position;
				model.state.cameraRotation = camera.transform.localRotation;
				OnMapPostBuildCheck();
				InitializePanels();
				switch (model.data.mode.typeFlag)
				{
				case GameFlag.Freestyle:
				case GameFlag.Race:
					view.ui.gameTestButtons[0].gameObject.SetActive(value: true);
					view.ui.gameTestButtons[1].gameObject.SetActive(value: true);
					view.ui.gameTestButtons[2].gameObject.SetActive(value: false);
					break;
				case GameFlag.Collectable:
					view.ui.gameTestButtons[0].gameObject.SetActive(value: false);
					view.ui.gameTestButtons[1].gameObject.SetActive(value: false);
					view.ui.gameTestButtons[2].gameObject.SetActive(value: true);
					break;
				}
				view.ui.SetPivotModeState(MEHandlePivotType.Local);
				view.ui.SetPhysicsDropEnabled(p_flag: false);
				LevelSettings.Scene.Grid grid = level.settings.scene.grid;
				bool flag = grid?.enabled ?? false;
				Vector3 vector = grid?.size ?? Vector3.zero;
				Vector3 vector2 = grid?.angle ?? Vector3.zero;
				model.state.metric.snapMap = flag;
				model.state.metric.snapMapMoveUnit = (flag ? Mathf.Min(vector.x, vector.y, vector.z) : 0f);
				model.state.metric.snapMapRotateUnit = (flag ? Mathf.Min(vector2.x, vector2.y, vector2.z) : 0f);
				model.state.metric.snapMove = false;
				model.state.metric.snapRotate = false;
				model.state.metric.snapMoveUnit = 1f;
				model.state.metric.snapRotateUnit = 30f;
				model.state.metric.SetMetricConvertables(MEMetricMode.Metric);
				view.RefreshSnapMove();
				view.RefreshSnapRotate();
				level.SetAssetLayerIndex(0, data.mapAssetLayer0);
				level.SetAssetLayerIndex(1, data.mapAssetLayer1);
				level.SetAssetLayerIndex(2, data.mapAssetLayer2);
				level.settings.scene.SetStyle(0, data.mapStyle0);
				level.settings.scene.SetStyle(1, data.mapStyle1);
				level.settings.scene.SetStyle(2, data.mapStyle2);
				base.game.model.level.track.SetTrackEnabled(data.trackId, p_flag: true);
				base.game.level.model.SetBaseAssetsEnabled(data.baseAssetsEnabled);
				int lightingPreset = level.data.lightingPreset;
				if (lightingPreset >= 0 && !data.containsMapLighting)
				{
					data.mapLighting = lightingPreset;
				}
				base.game.level.SetLightingPreset(data.mapLighting);
				base.app.view.audio.PlayMusicMapEditor();
				break;
			}
			}
			base.OnNotification(p_event, p_target, p_data);
			if (base.app.arguments.game.type != GameFlag.MapEditor)
			{
				return;
			}
			switch (p_event)
			{
			case "map-editor.metric.snap.move.dirty":
				view.RefreshSnapMove();
				break;
			case "map-editor.metric.snap.rotate.dirty":
				view.RefreshSnapRotate();
				break;
			case "map-editor.handle@update":
			{
				bool isCtrl = model.state.IsCtrl;
				if (model.state.metric.snapKeyboard != isCtrl)
				{
					model.state.metric.snapKeyboard = isCtrl;
					model.state.metric.snapMove = isCtrl;
					model.state.metric.snapRotate = isCtrl;
					view.RefreshSnapMove();
					view.RefreshSnapRotate();
				}
				break;
			}
			case "map-editor.handle@drag-end":
				if (model.state.metric.snapKeyboard)
				{
					model.state.metric.snapKeyboard = false;
					model.state.metric.snapMove = false;
					model.state.metric.snapRotate = false;
					view.RefreshSnapMove();
					view.RefreshSnapRotate();
				}
				break;
			case "map-editor.metric.mode.change":
				RefreshGrid();
				break;
			}
			switch (p_event)
			{
			case "map-editor.ready":
				Activity.RunOnce(base.app.controller.settings.ApplySimulationCameras, 1f / 60f);
				if ((bool)base.app.model.game.camera)
				{
					base.app.model.game.camera.wasd.useJoystick = false;
				}
				ClearScheduledSave();
				RefreshGameModeStats();
				goto IL_1847;
			case "map-editor.input.event":
				OnInputLayerEvent((UIEventType)p_data[0], (EventComponent)p_data[1]);
				goto IL_1847;
			case "map-editor.input.state.change":
				OnInputStateChange((MEInputStateType)p_data[0], (MEInputStateType)p_data[1]);
				goto IL_1847;
			case "map-editor.action.state.change":
				OnActionStateChange((MEActionStateType)p_data[0], (MEActionStateType)p_data[1]);
				goto IL_1847;
			case "map-editor.render.state.change":
				OnRenderStateChange((MERenderStateType)p_data[0], (MERenderStateType)p_data[1]);
				goto IL_1847;
			case "map-editor.pivot.state.change":
				view.ui.SetPivotModeState(model.state.pivot);
				view.handle.SetHandlePivot(model.state.pivot);
				goto IL_1847;
			case "map-editor.asset.form.event@change":
			case "map-editor.asset.form.event@click":
			case "map-editor.form.event@end-edit":
			case "map-editor.form.event@change":
			case "map-editor.form.event@click":
				OnFormEvent(p_event, p_target as Component);
				goto IL_1847;
			case "map-editor.graph.podium.event@change":
			case "map-editor.graph.podium.event@end-edit":
			case "map-editor.graph.gate.event@end-edit":
			case "map-editor.graph.gate.event@change":
				OnRaceGraphFormEvent(p_event, p_target as Component, p_data);
				goto IL_1847;
			case "map-editor.camera.signal-lost":
			{
				DroneCamera camera3 = base.game.model.camera;
				camera3.transform.position = model.state.cameraPosition;
				camera3.transform.localRotation = model.state.cameraRotation;
				goto IL_1847;
			}
			case "map-editor.selection.assets@change":
			{
				DRLMapEditorLibraryView assetLibraryPanel = view.ui.assetLibraryPanel;
				MEStateModel state2 = model.state;
				MESelectionModel selection = model.selection;
				state2.PreviewVisible = selection.anyAsset;
				assetLibraryPanel.SetSelection(selection.assets);
				view.ui.SetPhysicsDropEnabled(selection.anyAsset);
				if (!selection.anyAsset)
				{
					state2.physics.enabled = false;
					view.ui.SetPhysicsDropState(p_flag: false);
				}
				goto IL_1847;
			}
			case "map-editor.selection.assets@add":
				SetPreviewUpdateEnabled(p_flag: true);
				goto IL_1847;
			case "map-editor.selection.entities@change":
			{
				if (model.state.anyEntity)
				{
					if (model.state.inspectorTabAfterSelection < 0)
					{
						model.state.inspectorTabAfterSelection = view.ui.tabGroupRight.index;
					}
					view.ui.tabGroupRight.index = 1;
				}
				else
				{
					if (model.state.inspectorTabAfterSelection >= 0)
					{
						view.ui.tabGroupRight.index = model.state.inspectorTabAfterSelection;
					}
					model.state.inspectorTabAfterSelection = -1;
				}
				view.RefreshSnapMove();
				view.RefreshSnapRotate();
				bool multiEntity = model.selection.multiEntity;
				bool flag2 = model.selection.TrueForAnyEntities(MapAssetType.NoGroupMove);
				bool flag3 = model.selection.TrueForAnyEntities(MapAssetType.NoGroupRotate);
				bool flag4 = model.selection.TrueForAnyEntities(MapAssetType.NoGroupScale);
				bool flag5 = false;
				if (multiEntity)
				{
					switch (model.state.action)
					{
					case MEActionStateType.Move:
						flag5 = flag2;
						break;
					case MEActionStateType.Rotate:
						flag5 = flag3;
						break;
					case MEActionStateType.Scale:
						flag5 = flag4;
						break;
					}
				}
				if (flag5)
				{
					model.state.action = MEActionStateType.Select;
				}
				goto IL_1847;
			}
			case "map-editor.save.map-data@start":
				SetMapSaveEnabled(p_flag: true);
				if (!model.lockInput)
				{
				}
				goto IL_1847;
			case "map-editor.save.map-data@error":
				SetMapSaveEnabled(p_flag: false);
				SetInputLocked(p_flag: false);
				goto IL_1847;
			case "map-editor.save.map-data@blocked":
			case "map-editor.save.map-data@success":
				_ = p_event == "map-editor.save.map-data@blocked";
				SetMapSaveEnabled(p_flag: false);
				ClearScheduledSave();
				if (!model.willLoadGame)
				{
					SetInputLocked(p_flag: false);
				}
				else
				{
					MapData mapData = Reflection<object>.Get<MapData>(p_data, 0);
					model.willLoadGame = false;
					Debug.Log($"MapEditorController> {p_event} / will-load-game[{model.willLoadGame}]");
					if (mapData == null)
					{
						Debug.LogWarning("MapEditorController> " + p_event + " / Invalid MapData");
						SetInputLocked(p_flag: false);
					}
					else
					{
						GameFlag loadGameType = model.loadGameType;
						model.loadGameType = GameFlag.None;
						if (loadGameType == GameFlag.Replay && model.cachedReplaysCount <= 0)
						{
							Debug.LogWarning("MapEditorController> Tried to load Replay with no records");
						}
						else
						{
							DRLAppArguments arguments = base.app.arguments;
							arguments.Clear();
							arguments.game.editor = true;
							base.app.view.audio.PlayUIGenericSuccess();
							base.enabled = false;
							base.app.model.storage.maps.Populate(mapData);
							if (loadGameType == GameFlag.Replay)
							{
								if (ReplayFile.EnableVersion2)
								{
									base.app.scene.Load(model.cachedReplaysV2.ToArray());
								}
								else
								{
									BlackboxRecord p_replay = BlackboxRecord.Merge(model.cachedReplays.ToArray());
									base.app.scene.Load(p_replay);
								}
							}
							else
							{
								arguments.game.type = loadGameType;
								arguments.game.mode = GameFlag.SinglePlayer;
								GamePlayerData playerData = base.app.model.storage.state.player.playerData;
								base.app.arguments.game.AddPlayer(playerData);
								if (DRLApp.offline)
								{
									base.app.scene.LoadCommunityMap(mapData.guid);
								}
								else
								{
									base.app.scene.Load(mapData);
								}
							}
						}
					}
				}
				goto IL_1847;
			case "map-editor.save.map-thumb@start":
				if (!model.lockInput)
				{
					view.SetGridActive(p_flag: false);
					view.scene.SetGatesTriggerVisible(p_flag: false);
					view.scene.SetGuidesVisible(p_flag: false);
				}
				goto IL_1847;
			case "map-editor.save.map-thumb@error":
			case "map-editor.save.map-thumb@success":
				view.SetGridActive(p_flag: true);
				view.scene.SetGatesTriggerVisible(p_flag: true);
				view.scene.SetGuidesVisible(p_flag: true);
				goto IL_1847;
			case "map-editor.asset.item@click":
				OnAssetItemClick(p_target as UICardButtonMapEditorAssetItem);
				goto IL_1847;
			case "map-editor.scene.entity.clone":
			case "map-editor.scene.entity.create":
			{
				List<MAEntity> list3 = p_data[0] as List<MAEntity>;
				bool p_clone = p_event == "map-editor.scene.entity.clone";
				OnMapEntityCreate(list3, p_clone);
				MEStateModel state = model.state;
				if (state.physics.willApply && state.physics.enabled)
				{
					state.physics.willApply = false;
					state.physics.Push(list3, view.camera.transform);
				}
				goto IL_1847;
			}
			case "game.pause":
				base.game.model.camera.wasd.enabled = false;
				goto IL_1847;
			case "game.unpause":
				base.game.model.camera.wasd.enabled = true;
				goto IL_1847;
			default:
				switch (p_event)
				{
				case "map-editor.render.state.change":
				case "map-editor.save.map-thumb@success":
				case "map-editor.inspector.dirty":
				case "map-editor.handle@drag-end":
				case "map-editor.entity.delete":
				case "map-editor.entity.clone":
				case "map-editor.entity.create":
					ScheduleSave();
					RefreshGameModeStats();
					break;
				}
				break;
			case null:
				break;
				IL_1847:
				if (p_event == null)
				{
					break;
				}
				goto default;
			}
			switch (p_event)
			{
			case "map-editor.action.undo":
			case "map-editor.action.redo":
			case "map-editor.entity.delete":
			case "map-editor.entity.clone":
			case "map-editor.entity.create":
				RefreshRendererStats();
				break;
			}
		}

		protected void OnMapEntityBuild(MAEntity p_entity)
		{
			List<MapAssetAction> actions = p_entity.actions;
			for (int i = 0; i < actions.Count; i++)
			{
				actions[i].enabled = false;
			}
			if (p_entity is MARenderer)
			{
				MARenderer mARenderer = p_entity as MARenderer;
				if (mARenderer.replacedGUID)
				{
					Debug.Log("MapEditorController> OnMapEntityBuild / [" + mARenderer.name + "] GUID Replaced");
					mARenderer.ResetRendererMaterials();
					mARenderer.replacedGUID = false;
				}
			}
			switch (p_entity.data.type)
			{
			case MapAssetType.Gate:
			{
				MAGate mAGate = p_entity as MAGate;
				if ((bool)mAGate)
				{
					bool flag = mAGate.name.Contains("-empty");
					mAGate.SetTriggerRendererEnabled(mAGate.isTrigger || flag);
				}
				break;
			}
			case MapAssetType.CameraTool:
				p_entity.ClearInvalids();
				Activity.RunOnce((p_entity as MACameraTool).ClearInvalidColliders, 1f / 15f);
				break;
			case MapAssetType.Spline:
				p_entity.ClearInvalids();
				break;
			case MapAssetType.Collectable:
			{
				MACollectable mACollectable = p_entity as MACollectable;
				if ((bool)mACollectable)
				{
					mACollectable.SetCollision();
				}
				break;
			}
			}
			if (p_entity is MARenderer)
			{
				MARenderer obj = p_entity as MARenderer;
				obj.mapStyle0 = model.data.mapStyle0;
				obj.mapStyle1 = model.data.mapStyle1;
				obj.mapStyle2 = model.data.mapStyle2;
			}
		}

		protected void OnMapPostBuildCheck()
		{
			List<MASpline> list = view.scene.FindAll<MASpline>();
			for (int i = 0; i < list.Count; i++)
			{
				MASpline mASpline = list[i];
				if (mASpline.transform.childCount <= 0)
				{
					Object.Destroy(mASpline.gameObject);
				}
			}
			List<MACameraTool> list2 = view.scene.FindAll<MACameraTool>();
			for (int j = 0; j < list2.Count; j++)
			{
				MACameraTool mACameraTool = list2[j];
				if (mACameraTool.GetControlPoints().Count <= 0)
				{
					Object.Destroy(mACameraTool.gameObject);
				}
			}
			List<MAGate> list3 = view.scene.FindAll<MAGate>();
			for (int k = 0; k < list3.Count; k++)
			{
				MAGate mAGate = list3[k];
				MAGuide respawnGuide = mAGate.GetRespawnGuide();
				if ((bool)respawnGuide)
				{
					respawnGuide.gameObject.SetActive(mAGate.isRespawnVisible);
				}
			}
		}

		protected void OnMapEntityCreate(List<MAEntity> p_entities, bool p_clone = false)
		{
			if (p_entities != null)
			{
				for (int i = 0; i < p_entities.Count; i++)
				{
					OnMapEntityCreate(p_entities[i], p_clone);
				}
			}
		}

		protected void OnMapEntityCreate(MAEntity p_entity, bool p_clone = false)
		{
			if (!p_entity)
			{
				return;
			}
			List<MapAssetAction> actions = p_entity.actions;
			for (int i = 0; i < actions.Count; i++)
			{
				actions[i].enabled = false;
			}
			view.scene.HierarchyAdd(p_entity);
			switch (p_entity.data.type)
			{
			case MapAssetType.Gate:
				if (p_clone)
				{
					MAGuide respawnGuide = (p_entity as MAGate).GetRespawnGuide();
					if ((bool)respawnGuide)
					{
						respawnGuide.GenerateId();
					}
				}
				break;
			case MapAssetType.SplineControlPoint:
			{
				MASplineControlPoint mASplineControlPoint = p_entity as MASplineControlPoint;
				MASpline spline = mASplineControlPoint.spline;
				if ((bool)spline)
				{
					view.scene.HierarchyAdd(spline);
					if (p_clone)
					{
						mASplineControlPoint.index++;
						spline.SetControlPointIndex(mASplineControlPoint, mASplineControlPoint.index, p_refresh: false);
					}
					spline.DelayedRefreshHierarchy();
					spline.DelayRefresh();
				}
				break;
			}
			case MapAssetType.Spline:
			{
				MASpline mASpline = p_entity as MASpline;
				if (!p_clone)
				{
					view.scene.HierarchyAdd(mASpline.GetControlPoints());
				}
				mASpline.RefreshHierarchy();
				mASpline.Refresh();
				break;
			}
			case MapAssetType.CameraToolControlPoint:
			{
				MACameraToolControlPoint mACameraToolControlPoint = p_entity as MACameraToolControlPoint;
				MACameraTool tool = mACameraToolControlPoint.tool;
				if ((bool)tool)
				{
					mACameraToolControlPoint.SetIconMode(p_flag: true);
					view.scene.HierarchyAdd(tool);
					view.scene.HierarchyAdd(tool.collider);
					int num = tool.RefreshHierarchy();
					AssetLibrary library = base.app.model.storage.library;
					if ((uint)(num - 1) <= 1u)
					{
						tool.FitCollider();
						tool.collider.gameObject.SetActive(value: true);
					}
					if (num >= 2)
					{
						MapAsset p_item = library.FindByGUID<MapAsset>(tool.guid);
						model.selection.InvalidateAssets();
						model.selection.SetAsset(p_item, p_combine: false);
						model.selection.InvalidateEntities();
					}
					tool.DelayRefresh();
				}
				break;
			}
			case MapAssetType.CameraTool:
			{
				MACameraTool mACameraTool = p_entity as MACameraTool;
				if (!p_clone)
				{
					view.scene.HierarchyAdd(mACameraTool.GetControlPoints());
				}
				mACameraTool.Refresh();
				break;
			}
			case MapAssetType.Collectable:
				(p_entity as MACollectable).SetCollision();
				break;
			}
			if (p_entity is MARenderer)
			{
				(p_entity as MARenderer).mapStyle0 = model.data.mapStyle0;
			}
		}

		protected void OnInputStateChange(MEInputStateType p_from, MEInputStateType p_to)
		{
			if (base.enabled && !model.lockInput)
			{
				screen.view.SetInputState(p_to);
				view.camera.SetHilight(false);
				switch (p_to)
				{
				case MEInputStateType.Action:
					view.camera.SetMode(DMEModeType.Action, view.camera.orbit.distance);
					view.handle.SetHandlesInputEnabled(p_flag: true);
					model.state.PreviewVisible = true;
					break;
				case MEInputStateType.Navigate:
					view.camera.SetMode(DMEModeType.WASD);
					view.handle.SetHandlesInputEnabled(p_flag: false);
					model.state.PreviewVisible = false;
					break;
				case MEInputStateType.Orbit:
					view.camera.SetMode(DMEModeType.Orbit, view.camera.orbit.distance);
					view.handle.SetHandlesInputEnabled(p_flag: false);
					model.state.PreviewVisible = false;
					break;
				case MEInputStateType.Pan:
					view.camera.SetMode(DMEModeType.Pan);
					view.handle.SetHandlesInputEnabled(p_flag: false);
					model.state.PreviewVisible = false;
					break;
				}
				RefreshActionHandle();
			}
		}

		protected void OnActionStateChange(MEActionStateType p_from, MEActionStateType p_to)
		{
			if (base.enabled && !model.lockInput)
			{
				screen.view.SetActionState(p_to);
				if (p_from != p_to)
				{
					base.app.view.audio.PlayUIChange();
				}
				switch (p_to)
				{
				}
				RefreshActionHandle();
			}
		}

		public void RefreshActionHandle()
		{
			HandleModeType p_type = HandleModeType.None;
			if (model.state.AllowActionChange)
			{
				switch (model.state.action)
				{
				case MEActionStateType.Move:
					p_type = HandleModeType.Move;
					break;
				case MEActionStateType.Rotate:
					p_type = HandleModeType.Rotate;
					break;
				case MEActionStateType.Scale:
					p_type = HandleModeType.Scale;
					break;
				}
			}
			view.handle.SetHandle(p_type, model.selection.entities);
			view.handle.Refresh();
		}

		protected void OnRenderStateChange(MERenderStateType p_from, MERenderStateType p_to)
		{
			if (base.enabled && !model.lockInput)
			{
				Debug.Log("MapEditorController> OnRenderStateChange - from[" + p_from.ToString() + "] to[" + p_to.ToString() + "]");
				screen.view.SetRenderstate(p_to);
				screen.view.overlay.ClearSelectionBox();
				switch (p_to)
				{
				case MERenderStateType.Scene:
					base.app.view.audio.PlayUIScreenForward();
					view.camera.SetBlueprint(false);
					model.state.action = MEActionStateType.Select;
					view.ui.controls.gates.Clear(0.3f);
					view.ui.controls.gates.FadeOut(0.2f);
					view.ui.controls.podiums.Clear(0.3f);
					view.ui.controls.podiums.FadeOut(0.2f);
					RefreshActionHandle();
					break;
				case MERenderStateType.Race:
				{
					base.app.view.audio.PlayUIScreenForward();
					List<MAGate> list = view.scene.FindGates();
					model.selection.ClearEntities();
					MECamera camera = view.camera;
					Component[] p_targets = list.ToArray();
					camera.SetBlueprint(p_flag: true, p_targets);
					model.state.action = MEActionStateType.None;
					view.handle.SetHandle(HandleModeType.None);
					view.ui.controls.SetGatesGraph(list);
					view.ui.controls.gates.FadeIn(0.3f, 0.3f);
					List<MAPodium> podiumsGraph = view.scene.FindPodiums();
					view.ui.controls.SetPodiumsGraph(podiumsGraph);
					view.ui.controls.podiums.FadeIn(0.3f, 0.3f);
					break;
				}
				}
			}
		}

		protected void OnFormEvent(string p_event, Component p_target)
		{
			if (model.lockInput)
			{
				return;
			}
			string text = p_target.name;
			bool num = p_event.Contains("@click");
			bool flag = p_event.Contains("@change");
			p_event.Contains("@end-edit");
			bool flag2 = false;
			MEStateModel state = model.state;
			MEActionModel mEActionModel = model.action;
			List<MapAssetType> entityTags = model.selection.entityTags;
			bool num2 = model.selection.entities.Count > 1;
			bool flag3 = (!num2 || !entityTags.Contains(MapAssetType.NoGroupMove)) && !entityTags.Contains(MapAssetType.NoTranformMove);
			bool flag4 = (!num2 || !entityTags.Contains(MapAssetType.NoGroupRotate)) && !entityTags.Contains(MapAssetType.NoTranformRotate);
			bool flag5 = (!num2 || !entityTags.Contains(MapAssetType.NoGroupScale)) && !entityTags.Contains(MapAssetType.NoTranformScale);
			if (num)
			{
				switch (text)
				{
				case "render-state-scene":
					if (state.render != MERenderStateType.Scene)
					{
						mEActionModel.Record(MEActionType.ChangeRenderState, true, state.render, MERenderStateType.Scene);
					}
					break;
				case "render-state-race":
					if (state.render != MERenderStateType.Race)
					{
						mEActionModel.Record(MEActionType.ChangeRenderState, true, state.render, MERenderStateType.Race);
					}
					break;
				case "action-state-select":
					if (state.AllowActionChange && state.action != MEActionStateType.Select)
					{
						state.action = MEActionStateType.Select;
					}
					break;
				case "action-state-move":
					if (state.AllowActionChange && model.selection.anyEntity && flag3 && state.action != MEActionStateType.Move)
					{
						state.action = MEActionStateType.Move;
					}
					break;
				case "action-state-rotate":
					if (state.AllowActionChange && model.selection.anyEntity && flag4 && state.action != MEActionStateType.Rotate)
					{
						state.action = MEActionStateType.Rotate;
					}
					break;
				case "action-state-scale":
					if (state.AllowActionChange && model.selection.anyEntity && flag5 && state.action != MEActionStateType.Scale)
					{
						state.action = MEActionStateType.Scale;
					}
					break;
				case "game-test-play":
				case "game-test-freestyle":
				case "game-test-race":
				case "game-test-collectable":
				case "game-test-replay":
				{
					GameFlag gameFlag = GameFlag.None;
					switch (text)
					{
					case "game-test-freestyle":
						gameFlag = GameFlag.Freestyle;
						break;
					case "game-test-play":
						gameFlag = model.data.mode.typeFlag;
						break;
					case "game-test-race":
						gameFlag = model.data.mode.typeFlag;
						break;
					case "game-test-collectable":
						gameFlag = model.data.mode.typeFlag;
						break;
					case "game-test-replay":
						gameFlag = GameFlag.Replay;
						break;
					}
					if (gameFlag == GameFlag.Replay && model.cachedReplaysCount <= 0)
					{
						view.ui.BlinkTestReplayWarning();
						break;
					}
					ClearScheduledSave();
					model.willLoadGame = true;
					model.Save(p_force: true);
					SetInputLocked(p_flag: true);
					model.loadGameType = gameFlag;
					if (m_saveplay_timeout != null)
					{
						m_saveplay_timeout.Stop();
					}
					m_saveplay_timeout = Activity.RunOnce(delegate
					{
						m_saveplay_timeout = null;
						SetInputLocked(p_flag: false);
						SetMapSaveEnabled(p_flag: false);
					}, 15f);
					break;
				}
				case "editor-exit":
				{
					UICommunityMapsView uICommunityMapsView = base.app.view.ui.screens.Open<UICommunityMapsView>("community-maps-screen");
					uICommunityMapsView.screen.title = base.app.model.storage.locale.Get("maps.community.title", "Community Maps");
					uICommunityMapsView.allowExit = true;
					uICommunityMapsView.InitFilter(p_isMultiGP: false);
					break;
				}
				case "editor-settings":
					base.app.view.ui.screens.Open<UISettingsView>("settings-screen");
					break;
				case "snap-move-state":
				{
					DRLToggleView dRLToggleView2 = p_target as DRLToggleView;
					state.metric.snapMove = dRLToggleView2.toggle.isOn;
					Notify("map-editor.metric.snap.move.dirty");
					break;
				}
				case "snap-rotate-state":
				{
					DRLToggleView dRLToggleView = p_target as DRLToggleView;
					state.metric.snapRotate = dRLToggleView.toggle.isOn;
					Notify("map-editor.metric.snap.rotate.dirty");
					break;
				}
				}
			}
			if (flag)
			{
				switch (text)
				{
				case "asset-query":
				case "asset-category-0":
				case "asset-category-1":
					break;
				case "gizmo-grid-state":
					goto IL_07fb;
				case "metric-rulers-state":
					goto IL_0806;
				case "metric-mode-state":
					goto IL_0834;
				case "snap-move-field":
					goto IL_0865;
				case "snap-rotate-field":
					goto IL_0891;
				default:
					goto IL_08c1;
				case null:
					goto IL_098c;
				}
				RefreshAssetFilter();
				model.selection.InvalidateAssets();
			}
			goto IL_08bb;
			IL_08c1:
			switch (text)
			{
			case "handle-pivot-state":
				state.model.state.pivot = ((state.model.state.pivot == MEHandlePivotType.Local) ? MEHandlePivotType.Global : MEHandlePivotType.Local);
				break;
			case "physics-drop-state":
				state.model.state.physics.enabled = !state.model.state.physics.enabled;
				view.ui.SetPhysicsDropState(state.model.state.physics.enabled);
				break;
			case "undo":
				mEActionModel.Undo();
				break;
			case "redo":
				mEActionModel.Redo();
				break;
			}
			goto IL_098c;
			IL_0865:
			DRLNumberFieldView dRLNumberFieldView = p_target as DRLNumberFieldView;
			state.metric.snapMoveUnit = dRLNumberFieldView.value;
			Notify("map-editor.metric.snap.move.dirty");
			goto IL_08bb;
			IL_0834:
			DRLToggleView dRLToggleView3 = p_target as DRLToggleView;
			model.state.metric.mode = (dRLToggleView3.toggle.isOn ? MEMetricMode.Imperial : MEMetricMode.Metric);
			goto IL_08bb;
			IL_08bb:
			if (text != null)
			{
				goto IL_08c1;
			}
			goto IL_098c;
			IL_0891:
			DRLNumberFieldView dRLNumberFieldView2 = p_target as DRLNumberFieldView;
			state.metric.snapRotateUnit = dRLNumberFieldView2.value;
			Notify("map-editor.metric.snap.rotate.dirty");
			goto IL_08bb;
			IL_0806:
			DRLToggleView dRLToggleView4 = p_target as DRLToggleView;
			model.state.metric.showRulers = dRLToggleView4.toggle.isOn;
			goto IL_08bb;
			IL_098c:
			if (flag2)
			{
				ScheduleSave();
			}
			return;
			IL_07fb:
			RefreshGrid();
			goto IL_08bb;
		}

		protected void RefreshAssetFilter()
		{
			if (m_asset_filter_timer != null)
			{
				m_asset_filter_timer.Stop();
			}
			m_asset_filter_timer = RunOnce(delegate
			{
				DRLMapEditorLibraryView assetLibraryPanel = view.ui.assetLibraryPanel;
				view.ui.category1Switcher.index = view.ui.category0Stepper.index;
				string assetQuery = view.ui.assetQuery;
				MapAssetType category0Flag = view.ui.category0Flag;
				MapAssetType category1Flag = view.ui.category1Flag;
				int assetQueryCount = assetLibraryPanel.SetFilter(assetQuery, category0Flag, category1Flag);
				view.ui.assetQueryCount = assetQueryCount;
				m_asset_filter_timer = null;
			}, 0.1f);
		}

		protected void InitializePanels()
		{
			view.ui.tabGroupRight.index = 0;
			int assetQueryCount = view.ui.assetLibraryPanel.SetFilter("", MapAssetType.Prop, MapAssetType.None);
			view.ui.assetQueryCount = assetQueryCount;
			model.lockInput = true;
			Localization locale = base.app.model.storage.locale;
			List<MapAssetType> list = new List<MapAssetType>
			{
				MapAssetType.Prop,
				MapAssetType.RaceProp,
				MapAssetType.Collectable,
				MapAssetType.Tool
			};
			List<string> list2 = new List<string>
			{
				locale.Get("map-editor.assets-category.environment.label", "Environment"),
				locale.Get("map-editor.assets-category.race.label", "Race"),
				locale.Get("map-editor.assets-category.collectable.label", "Collectable"),
				locale.Get("map-editor.assets-category.tools.label", "Tools")
			};
			List<List<MapAssetType>> list3 = new List<List<MapAssetType>>
			{
				new List<MapAssetType>
				{
					MapAssetType.None,
					MapAssetType.Misc,
					MapAssetType.Vehicles,
					MapAssetType.Nature,
					MapAssetType.Rocks,
					MapAssetType.Primitives,
					MapAssetType.Grid,
					MapAssetType.Path
				},
				new List<MapAssetType>
				{
					MapAssetType.None,
					MapAssetType.Official,
					MapAssetType.DRL,
					MapAssetType.Inflatables,
					MapAssetType.Markers,
					MapAssetType.Missions,
					MapAssetType.MultiGP,
					MapAssetType.Regional,
					MapAssetType.Neon,
					MapAssetType.Others
				},
				new List<MapAssetType> { MapAssetType.None },
				new List<MapAssetType> { MapAssetType.None }
			};
			List<List<string>> list4 = new List<List<string>>
			{
				new List<string>
				{
					locale.Get("map-editor.assets-category.all.label", "All"),
					locale.Get("map-editor.assets-category.misc.label", "Misc"),
					locale.Get("map-editor.assets-category.vehicles.label", "Vehicles"),
					locale.Get("map-editor.assets-category.nature.label", "Nature"),
					locale.Get("map-editor.assets-category.rocks.label", "Rocks"),
					locale.Get("map-editor.assets-category.primitives.label", "Primitives"),
					locale.Get("map-editor.assets-category.grid.label", "Grid"),
					locale.Get("map-editor.assets-category.path.label", "Path")
				},
				new List<string>
				{
					locale.Get("map-editor.assets-category.all.label", "All"),
					locale.Get("map-editor.assets-category.official.label", "Official"),
					locale.Get("map-editor.assets-category.drl.label", "DRL"),
					locale.Get("map-editor.assets-category.inflatables.label", "Inflatables"),
					locale.Get("map-editor.assets-category.markers.label", "Markers"),
					locale.Get("map-editor.assets-category.missions.label", "Missions"),
					locale.Get("map-editor.assets-category.multigp.label", "MultiGP"),
					locale.Get("map-editor.assets-category.regional.label", "Regional"),
					locale.Get("map-editor.assets-category.neon.label", "Neon"),
					locale.Get("map-editor.assets-category.others.label", "Others")
				},
				new List<string> { locale.Get("map-editor.assets-category.all.label", "All") },
				new List<string> { locale.Get("map-editor.assets-category.all.label", "All") }
			};
			DRLMapEditorLibraryView assetLibraryPanel = view.ui.assetLibraryPanel;
			view.ui.category0Flags = list.ToArray();
			view.ui.category0Stepper.min = 0;
			view.ui.category0Stepper.max = list.Count - 1;
			view.ui.category0Stepper.index = 0;
			view.ui.category0Stepper.labels = list2.ConvertAll((string it) => it.ToUpper()).ToArray();
			view.ui.category0Stepper.Refresh();
			for (int num = 0; num < list.Count; num++)
			{
				MapAssetType mapAssetType = list[num];
				List<MapAssetType> list5 = list3[num];
				List<string> list6 = list4[num];
				for (int num2 = 0; num2 < list5.Count; num2++)
				{
					MapAssetType mapAssetType2 = list5[num2];
					if (mapAssetType2 != MapAssetType.None)
					{
						int filterCount = assetLibraryPanel.GetFilterCount("", mapAssetType, mapAssetType2);
						Debug.Log("MapEditorController> InitializeAssetFilters / Filtering label[" + list6[num2] + "] flag[" + mapAssetType2.ToString() + "] count[" + filterCount + "]");
						int num3 = 0;
						if (mapAssetType2 == MapAssetType.DRL)
						{
							num3 = 1;
						}
						if (filterCount <= num3)
						{
							list6.RemoveAt(num2);
							list5.RemoveAt(num2);
							num2--;
						}
					}
				}
				DRLStepperView dRLStepperView = view.ui.category1Switcher.Get<DRLStepperView>(num);
				if ((bool)dRLStepperView)
				{
					dRLStepperView.min = 0;
					dRLStepperView.max = list5.Count - 1;
					dRLStepperView.index = 0;
					dRLStepperView.labels = list6.ConvertAll((string it) => it.ToUpper()).ToArray();
					dRLStepperView.Refresh();
				}
				view.ui.category1Flags[num] = list5.ToArray();
			}
			model.lockInput = false;
			view.ui.inspector.SetTargets(null);
		}

		protected void OnRaceGraphFormEvent(string p_event, Component p_target, params object[] p_data)
		{
			if (model.lockInput)
			{
				return;
			}
			Transform parent = p_target.transform.parent.parent;
			string text = parent.name;
			MEGraphLayer component = parent.GetComponent<MEGraphLayer>();
			_ = p_target.name;
			p_event.Contains("@click");
			p_event.Contains("@change");
			bool flag = p_event.Contains("@end-edit");
			bool flag2 = false;
			_ = model.state;
			switch (text)
			{
			case "gates":
			{
				if (!flag)
				{
					break;
				}
				DRLNumberFieldView dRLNumberFieldView2 = p_target as DRLNumberFieldView;
				if (!dRLNumberFieldView2)
				{
					break;
				}
				float value2 = dRLNumberFieldView2.value;
				if (p_data.Length != 0)
				{
					_ = (float)p_data[0];
				}
				int num2 = (int)(value2 - 1f);
				int nodeIndex2 = component.GetNodeIndex(dRLNumberFieldView2);
				if (nodeIndex2 != num2)
				{
					List<MAGate> list = view.scene.FindGates();
					if ((nodeIndex2 >= list.Count || !list[nodeIndex2].isFinish) && (num2 >= list.Count || !list[num2].isFinish))
					{
						Notify("map-editor.gate.order@change", nodeIndex2, num2);
					}
				}
				break;
			}
			case "podiums":
			{
				if (!flag)
				{
					break;
				}
				DRLNumberFieldView dRLNumberFieldView = p_target as DRLNumberFieldView;
				if ((bool)dRLNumberFieldView)
				{
					float value = dRLNumberFieldView.value;
					if (p_data.Length != 0)
					{
						_ = (float)p_data[0];
					}
					int num = (int)(value - 1f);
					int nodeIndex = component.GetNodeIndex(dRLNumberFieldView);
					if (nodeIndex != num)
					{
						Notify("map-editor.podium.order@change", nodeIndex, num);
					}
				}
				break;
			}
			}
			RefreshGameModeStats();
			if (flag2)
			{
				ScheduleSave();
			}
		}

		protected void OnInputLayerEvent(UIEventType p_type, EventComponent p_target)
		{
			if (!model.lockInput)
			{
				MEStateModel state = model.state;
				_ = p_target.data;
				UpdateState(p_type, p_target);
				bool moving = view.handle.moving;
				view.camera.SetHilight(false);
				if (state.AllowHilight && !moving)
				{
					view.camera.SetHilight(p_flag: true, state.entities.ToArray());
				}
				UpdateActionState(p_type, p_target);
				UpdateNavigateState(p_type, p_target);
				UpdatePanState(p_type, p_target);
				UpdateAssetModeState(p_type, p_target);
			}
		}

		protected void UpdateState(UIEventType p_type, EventComponent p_target)
		{
			MEStateModel state = model.state;
			PointerEventData data = p_target.data;
			bool flag = data.button == PointerEventData.InputButton.Left;
			bool flag2 = data.button == PointerEventData.InputButton.Right;
			bool flag3 = data.button == PointerEventData.InputButton.Middle;
			bool flag4 = state.input == MEInputStateType.Action;
			bool moving = view.handle.moving;
			switch (p_type)
			{
			case UIEventType.Down:
				if (flag2 && flag4)
				{
					state.input = MEInputStateType.Navigate;
					view.ui.contentInputEnabled = false;
				}
				break;
			case UIEventType.Up:
				if (flag2 || flag3)
				{
					state.input = MEInputStateType.Action;
					view.ui.contentInputEnabled = true;
				}
				break;
			case UIEventType.DragStart:
				if (flag3)
				{
					state.input = MEInputStateType.Pan;
					view.ui.contentInputEnabled = false;
				}
				break;
			}
			switch (p_type)
			{
			case UIEventType.Enter:
				state.mouse.focus = true;
				break;
			case UIEventType.Exit:
				state.mouse.focus = false;
				if (!flag4)
				{
					state.input = MEInputStateType.Action;
				}
				break;
			case UIEventType.Scroll:
				state.mouse.scroll = p_target.data.scrollDelta;
				break;
			}
			if (flag)
			{
				switch (p_type)
				{
				case UIEventType.DragStart:
					if (!moving)
					{
						view.ui.contentInputEnabled = false;
						if (state.AllowSelect)
						{
							state.ActiveDragSelect = true;
							state.mouse.rect = new Rect(p_target.dragStartPosition.x, p_target.dragStartPosition.y, 0f, 0f);
							view.handle.SetHandlesInputEnabled(p_flag: false);
						}
					}
					break;
				case UIEventType.DragUpdate:
					if (!moving)
					{
						state.mouse.rect = p_target.dragRect;
						if (!state.AllowSelectBoxRefresh)
						{
							state.ActiveDragSelect = false;
							screen.view.overlay.ClearSelectionBox();
						}
						else
						{
							screen.view.overlay.SelectionBox(state.mouse.rect);
						}
					}
					break;
				}
			}
			state.AllowRaycast = false;
			switch (p_type)
			{
			case UIEventType.Move:
			case UIEventType.DragUpdate:
				state.AllowRaycast = true;
				state.mouse.screenSize = view.ui.overlay.size;
				break;
			case UIEventType.Down:
			case UIEventType.Up:
			case UIEventType.DragEnd:
				state.AllowRaycast = state.input == MEInputStateType.Action;
				view.ui.contentInputEnabled = state.input == MEInputStateType.Action;
				UpdateStateRaycasts();
				break;
			}
			if (p_type == UIEventType.Up || p_type == UIEventType.DragEnd)
			{
				view.handle.SetHandlesInputEnabled(p_flag: true);
			}
		}

		protected void UpdateStateRaycasts()
		{
			MEStateModel state = model.state;
			state.mouse.hit = view.scene.ScreenRaycast();
			state.entities.Clear();
			if (state.AllowSelectBoxRefresh)
			{
				state.SetEntities(view.scene.SelectRectangle<MAEntity>(state.mouse.screenRect));
				return;
			}
			MAGuide mAGuide = view.scene.SelectRay<MAGuide>();
			MAEntity p_item = (mAGuide ? mAGuide : view.scene.SelectRay<MAEntity>());
			state.PushEntity(p_item);
		}

		protected void UpdateActionState(UIEventType p_type, EventComponent p_target)
		{
			MEStateModel state = model.state;
			if (state.input != MEInputStateType.Action)
			{
				return;
			}
			PointerEventData data = p_target.data;
			bool flag = data.button == PointerEventData.InputButton.Left;
			bool flag2 = data.button == PointerEventData.InputButton.Right;
			_ = data.button;
			bool moving = view.handle.moving;
			if (flag)
			{
				switch (p_type)
				{
				case UIEventType.Up:
					if (!moving)
					{
						if (!state.anyEntity && state.AllowSelect && !state.IsCtrl)
						{
							model.selection.SetEntity((List<MAEntity>)null, false);
							return;
						}
						if (!state.ActiveDragSelect && state.ApplySelect)
						{
							model.selection.SetEntity(state.entities, state.IsCtrl);
							Notify("map-editor.selection.entities.mouse", state.entities, state.IsCtrl);
						}
					}
					break;
				case UIEventType.DragEnd:
					if (state.ApplyBoxSelect)
					{
						model.selection.SetEntity(state.entities, state.IsCtrl);
						Notify("map-editor.selection.entities.mouse", state.entities, state.IsCtrl);
						state.ActiveDragSelect = false;
						screen.view.overlay.ClearSelectionBox();
						view.camera.SetHilight(false);
					}
					break;
				case UIEventType.MultiClick:
				{
					if (!state.AllowSelect || state.ActiveDragSelect)
					{
						break;
					}
					List<MAEntity> list = view.scene.SelectRayAll<MAEntity>();
					if (list.Count <= 0)
					{
						break;
					}
					bool isCtrl = state.IsCtrl;
					bool flag3 = isCtrl && state.IsShift;
					MAEntity mAEntity = list[0];
					MapAssetType mapAssetType = mAEntity.data.type;
					List<MapAssetType> tags = mAEntity.tags;
					if (!isCtrl)
					{
						if (!tags.Contains(MapAssetType.NoFocus))
						{
							view.camera.Focus(mAEntity);
						}
						break;
					}
					List<MAEntity> list2 = new List<MAEntity>();
					switch (mapAssetType)
					{
					case MapAssetType.CameraToolControlPoint:
						mapAssetType = MapAssetType.None;
						break;
					case MapAssetType.Guide:
						if ((bool)(mAEntity as MAGuide).GetComponentInParent<MAGate>())
						{
							mapAssetType = MapAssetType.None;
						}
						break;
					}
					switch (mapAssetType)
					{
					case MapAssetType.None:
						list2.Add(mAEntity);
						break;
					case MapAssetType.SplineControlPoint:
					{
						MASpline spline = (mAEntity as MASplineControlPoint).spline;
						if ((bool)spline)
						{
							list2.AddRange(spline.GetControlPoints());
						}
						break;
					}
					default:
					{
						string guid = mAEntity.guid;
						list2.AddRange(view.scene.FindAllByGUID<MAEntity>(guid));
						break;
					}
					}
					if (!list2.Contains(mAEntity))
					{
						list2.Add(mAEntity);
					}
					if (flag3)
					{
						for (int i = 0; i < list2.Count; i++)
						{
							MAEntity mAEntity2 = list2[i];
							if (!(mAEntity2 is MARenderer))
							{
								list2.RemoveAt(i--);
								continue;
							}
							MARenderer p_target2 = mAEntity2 as MARenderer;
							if (!view.camera.IsRendererVisible(p_target2))
							{
								list2.RemoveAt(i--);
							}
						}
					}
					model.selection.InvalidateEntities();
					model.selection.SetEntity(list2, state.IsCtrl);
					break;
				}
				}
			}
			if (flag2 && p_type == UIEventType.Down)
			{
				state.input = MEInputStateType.Navigate;
			}
		}

		protected void UpdateNavigateState(UIEventType p_type, EventComponent p_target)
		{
			if (model.state.input == MEInputStateType.Navigate)
			{
				PointerEventData data = p_target.data;
				_ = data.button;
				bool flag = data.button == PointerEventData.InputButton.Right;
				_ = data.button;
				if (flag && p_type == UIEventType.Up)
				{
					model.state.input = MEInputStateType.Action;
				}
			}
		}

		protected void UpdatePanState(UIEventType p_type, EventComponent p_target)
		{
			MEStateModel state = model.state;
			if (state.input != MEInputStateType.Pan)
			{
				return;
			}
			PointerEventData data = p_target.data;
			_ = data.button;
			_ = data.button;
			if (data.button == PointerEventData.InputButton.Middle)
			{
				switch (p_type)
				{
				case UIEventType.Up:
					view.camera.orbit.Snap(p_position: true, p_angle: false);
					state.input = MEInputStateType.Action;
					break;
				case UIEventType.DragStart:
					view.camera.panPosition = view.camera.orbit.anchor;
					view.camera.orbit.Snap(p_position: true, p_angle: false);
					break;
				case UIEventType.DragUpdate:
					view.camera.Pan(p_target.dragOffset);
					break;
				}
			}
		}

		protected void UpdateAssetModeState(UIEventType p_type, EventComponent p_target)
		{
			MEStateModel state = model.state;
			if (!state.ActivePreview)
			{
				return;
			}
			PointerEventData data = p_target.data;
			bool flag = data.button == PointerEventData.InputButton.Left;
			_ = data.button;
			bool flag2 = data.button == PointerEventData.InputButton.Middle;
			int num;
			int num2;
			int num3;
			int index;
			switch (p_type)
			{
			case UIEventType.Scroll:
			{
				if (!state.AllowPreviewInput)
				{
					break;
				}
				float y = state.mouse.scroll.y;
				if (!state.IsCtrl)
				{
					num = ((!state.IsCommand) ? 1 : 0);
					if (num != 0)
					{
						num2 = ((y < 0f) ? 1 : (-1));
						goto IL_0097;
					}
				}
				else
				{
					num = 0;
				}
				num2 = 0;
				goto IL_0097;
			}
			case UIEventType.Enter:
				{
					SetPreviewUpdateEnabled(p_flag: true);
					break;
				}
				IL_0097:
				num3 = num2;
				index = state.preview.index;
				if (num != 0)
				{
					int num4 = (index + num3) % state.preview.objects.Count;
					if (num4 < 0)
					{
						num4 = state.preview.objects.Count - 1;
					}
					state.preview.index = num4;
				}
				break;
			}
			if (flag && p_type == UIEventType.Up && state.AllowCreate && state.mouse.valid)
			{
				MALayoutGeometryTool mALayoutGeometryTool = view.scene.SelectRay<MALayoutGeometryTool>();
				if ((bool)mALayoutGeometryTool)
				{
					mALayoutGeometryTool.SetTemplates(model.selection.assets);
					model.selection.ClearAssets();
					view.ui.assetLibraryPanel.SetSelection((MapAsset)null);
					List<MAEntity> list = new List<MAEntity> { mALayoutGeometryTool };
					model.selection.SetEntity(list, p_combine: false);
					Notify("map-editor.selection.entities.mouse", list, false);
				}
				else
				{
					state.preview.Place();
					state.preview.ClearContainer();
					state.physics.willApply = true;
					MAEntity p_asset = state.preview.Get<MAEntity>();
					MAEntity container = state.preview.container;
					if ((bool)container && !container.transform.IsChildOf(view.scene.root.transform))
					{
						container.transform.SetParent(view.scene.root.transform, worldPositionStays: true);
					}
					model.action.Create(p_asset, container);
				}
			}
			if (flag2 && p_type == UIEventType.Up && state.AllowCreate && state.mouse.valid)
			{
				state.preview.Reset();
			}
		}

		private bool OnPreviewUpdate()
		{
			if (!base.validContext)
			{
				return false;
			}
			MEStateModel state = model.state;
			if (!model.selection.anyAsset)
			{
				m_preview_loop = null;
				return false;
			}
			if (!state.ActivePreview)
			{
				return true;
			}
			if (!state.AllowPreviewInput)
			{
				return true;
			}
			float unscaledDeltaTime = Time.unscaledDeltaTime;
			float num = (state.IsShift ? 3f : 0.2f);
			float num2 = (state.IsShift ? 360f : 45f);
			float num3 = 0f;
			float num4 = 0f;
			if (Input.GetKey(KeyCode.A))
			{
				num4 -= num2 * unscaledDeltaTime;
			}
			if (Input.GetKey(KeyCode.D))
			{
				num4 += num2 * unscaledDeltaTime;
			}
			if (Input.GetKey(KeyCode.W))
			{
				num3 += num * unscaledDeltaTime;
			}
			if (Input.GetKey(KeyCode.S))
			{
				num3 -= num * unscaledDeltaTime;
			}
			if (Input.GetKeyDown(KeyCode.LeftShift))
			{
				state.physics.enabled = true;
				view.ui.SetPhysicsDropState(state.model.state.physics.enabled);
			}
			if (Input.GetKeyUp(KeyCode.LeftShift))
			{
				state.physics.enabled = false;
				view.ui.SetPhysicsDropState(state.model.state.physics.enabled);
			}
			bool flag = model.selection.anyAsset && model.selection.TrueForAllAssets(MapAssetType.NoForceGrid);
			bool snapMap = model.state.metric.snapMap;
			model.state.metric.snapMap = model.state.metric.snapMap && !flag;
			float snapMoveUnit = model.state.metric.GetSnapMoveUnit();
			float snapRotateUnit = model.state.metric.GetSnapRotateUnit();
			model.state.metric.snapMap = snapMap;
			state.preview.orient = !state.IsCtrl;
			state.preview.angleSnap = snapRotateUnit;
			state.preview.distanceSnap = snapMoveUnit;
			state.preview.positionSnap = snapMoveUnit;
			if (Mathf.Abs(num4) > 0f)
			{
				state.preview.angle += num4;
			}
			if (Mathf.Abs(num3) > 0f)
			{
				state.preview.distance += num3;
			}
			return true;
		}

		private void SetPreviewUpdateEnabled(bool p_flag)
		{
			if (p_flag)
			{
				if (m_preview_loop == null)
				{
					m_preview_loop = Activity.Run(OnPreviewUpdate, 0f, false);
				}
			}
			else if (m_preview_loop != null)
			{
				m_preview_loop.Stop();
				m_preview_loop = null;
			}
		}

		protected void OnAssetItemClick(UICardButtonMapEditorAssetItem p_item)
		{
			if (!base.enabled || !p_item)
			{
				return;
			}
			MEStateModel state = model.state;
			if (state.render != MERenderStateType.Race)
			{
				_ = view.ui.assetLibraryPanel;
				MapAsset data = p_item.data;
				if ((bool)data)
				{
					bool flag = model.selection.CanMultiSelectAsset();
					bool p_combine = (1u & (flag ? 1u : 0u)) != 0 && state.IsCtrl;
					model.selection.SetAsset(data, p_combine);
					model.selection.InvalidateEntities();
				}
			}
		}

		public override bool OnGameCommand(GameCommand p_command)
		{
			if (p_command == null)
			{
				return true;
			}
			MEStateModel state = model.state;
			if (p_command.hash.Contains("@dev"))
			{
				return false;
			}
			string hash = p_command.hash;
			hash = hash.Replace("@dev", "");
			if (!base.enabled)
			{
				return false;
			}
			if (model.lockInput)
			{
				return false;
			}
			if (state.inputFocus)
			{
				return false;
			}
			Debug.Log("MapEditorController> OnGameCommand - hash[" + p_command.hash + "," + p_command.down + "]");
			if (p_command.type == GameCommandType.Pause)
			{
				return false;
			}
			MESelectionModel selection = model.selection;
			MEActionModel mEActionModel = model.action;
			List<MapAssetType> entityTags = model.selection.entityTags;
			bool flag = state.input == MEInputStateType.Action;
			bool flag2 = state.render == MERenderStateType.Race;
			bool num = model.selection.entities.Count > 1;
			bool flag3 = (!num || !entityTags.Contains(MapAssetType.NoGroupMove)) && !entityTags.Contains(MapAssetType.NoTranformMove);
			bool flag4 = (!num || !entityTags.Contains(MapAssetType.NoGroupRotate)) && !entityTags.Contains(MapAssetType.NoTranformRotate);
			bool flag5 = (!num || !entityTags.Contains(MapAssetType.NoGroupScale)) && !entityTags.Contains(MapAssetType.NoTranformScale);
			DRLMapEditorLibraryView assetLibraryPanel = view.ui.assetLibraryPanel;
			switch (hash)
			{
			case "dme-alt":
				if (!p_command.down)
				{
					state.input = MEInputStateType.Action;
				}
				break;
			case "dme-gizmo-grid-state-switch":
				view.ui.gizmoGridStepper.Next();
				break;
			case "dme-cancel":
				if (selection.any)
				{
					base.app.view.audio.PlayUIScreenBackward();
				}
				if (!selection.none)
				{
					selection.ClearEntities();
					selection.ClearAssets();
					assetLibraryPanel.SetSelection((MapAsset)null);
				}
				break;
			case "dme-focus-selection":
			{
				List<MAEntity> entities2 = model.selection.entities;
				if (entities2.Count > 0)
				{
					view.camera.Focus(entities2.ToArray());
				}
				break;
			}
			case "dme-delete":
				if (model.selection.anyEntity)
				{
					List<MAEntity> list = new List<MAEntity>(model.selection.entities);
					OnEntitiesDelete(list);
					if (list.Count > 0)
					{
						model.selection.InvalidateEntities();
						Notify("map-editor.entity.delete", list);
						model.state.action = MEActionStateType.Select;
					}
				}
				break;
			case "dme-undo":
				mEActionModel.Undo();
				break;
			case "dme-redo":
				mEActionModel.Redo();
				break;
			case "dme-clone":
			{
				List<MAEntity> entities = model.selection.entities;
				model.selection.InvalidateEntities();
				ApplyClone(new List<MAEntity>(entities), null, p_force_parent: false, p_force_selection: true);
				break;
			}
			}
			if (flag)
			{
				switch (p_command.hash)
				{
				case "dme-alt":
					if (p_command.down)
					{
						state.input = MEInputStateType.Orbit;
					}
					break;
				case "dme-render-state-switch":
					if (!model.state.inputFocus)
					{
						if (!view.ui.IsScreenVisible())
						{
							view.SetGridActive(p_flag: true);
							view.scene.SetGatesTriggerVisible(p_flag: true);
							view.ui.SetScreenVisible(p_flag: true);
						}
						else
						{
							MERenderStateType mERenderStateType = ((state.render != MERenderStateType.Scene) ? MERenderStateType.Scene : MERenderStateType.Race);
							mEActionModel.Record(MEActionType.ChangeRenderState, true, state.render, mERenderStateType);
						}
					}
					break;
				case "dme-ui-hide-all":
				{
					bool flag6 = view.ui.SwitchScreenVisible();
					view.SetGridActive(!flag6);
					view.scene.SetGatesTriggerVisible(!flag6);
					view.scene.SetGuidesVisible(!flag6);
					break;
				}
				case "dme-select-asset":
					if (!flag2)
					{
					}
					break;
				case "dme-metric-snap-move-switch":
					state.metric.snapMove = !state.metric.snapMove;
					view.RefreshSnapMove();
					break;
				case "dme-metric-snap-rotate-switch":
					state.metric.snapRotate = !state.metric.snapRotate;
					view.RefreshSnapRotate();
					break;
				case "dme-action-state-select":
					if (state.AllowActionChange)
					{
						state.action = MEActionStateType.Select;
					}
					break;
				case "dme-action-state-move":
					if (state.AllowActionChange && model.selection.anyEntity && flag3)
					{
						state.action = MEActionStateType.Move;
					}
					break;
				case "dme-action-state-rotate":
					if (state.AllowActionChange && model.selection.anyEntity && flag4)
					{
						state.action = MEActionStateType.Rotate;
					}
					break;
				case "dme-action-state-scale":
					if (state.AllowActionChange && model.selection.anyEntity && flag5)
					{
						state.action = MEActionStateType.Scale;
					}
					break;
				}
			}
			return true;
		}

		protected void OnEntitiesDelete(List<MAEntity> p_targets)
		{
			p_targets.RemoveAll((MAEntity it) => it.tags.Contains(MapAssetType.NoDelete));
			for (int num = 0; num < p_targets.Count; num++)
			{
				MAEntity mAEntity = p_targets[num];
				if (!mAEntity.tags.Contains(MapAssetType.DeleteHierarchy))
				{
					continue;
				}
				Transform parent = mAEntity.transform.parent;
				if (!parent || parent == view.scene.root.transform)
				{
					continue;
				}
				List<MAEntity> list = new List<MAEntity>();
				for (int num2 = 0; num2 < parent.childCount; num2++)
				{
					MAEntity component = parent.GetChild(num2).GetComponent<MAEntity>();
					if ((bool)component)
					{
						list.Add(component);
					}
				}
				for (int num3 = 0; num3 < list.Count; num3++)
				{
					MAEntity item = list[num3];
					if (!p_targets.Contains(item))
					{
						p_targets.Insert(0, item);
						num++;
					}
				}
			}
			view.scene.HierarchyAdd(p_targets);
		}

		protected void OnEntitiesClone(List<MAEntity> p_targets)
		{
			p_targets.RemoveAll((MAEntity it) => it.tags.Contains(MapAssetType.NoClone));
		}

		public void ApplyClone(List<MAEntity> p_targets, Component p_container, bool p_force_parent, bool p_force_selection)
		{
			List<MAEntity> list = new List<MAEntity>(p_targets);
			OnEntitiesClone(list);
			if (list.Count > 0)
			{
				Notify("map-editor.entity.clone", list, p_container, p_force_parent, p_force_selection);
			}
		}

		protected void FixedUpdate()
		{
			if (!base.enabled || model.lockInput)
			{
				return;
			}
			MEStateModel state = model.state;
			if (state.AllowRaycast)
			{
				m_raycast_rate -= Time.fixedUnscaledDeltaTime;
				if (m_raycast_rate <= 0f)
				{
					m_raycast_rate = 0.03f;
					UpdateStateRaycasts();
				}
			}
			if (state.ActivePreview)
			{
				if (state.AllowPreviewRefresh && state.mouse.valid)
				{
					state.preview.Place();
				}
				state.preview.RefreshContainer();
			}
		}

		public void ScheduleSave(float p_delay = 3f, bool p_force = false)
		{
			if (base.validContext && (p_force || model.data.prefs.autoSave))
			{
				if (m_save_timer != null)
				{
					m_save_timer.Stop();
					m_save_timer = null;
				}
				Notify("map-editor.save.map-data.schedule");
				m_save_timer = this.TimerRunOnce(delegate
				{
					Debug.Log("MapEditorController> Schedule Save...");
					model.Save();
				}, p_delay);
			}
		}

		public void ClearScheduledSave()
		{
			Debug.Log("MapEditorController> ClearScheduledSave");
			if (m_save_timer != null)
			{
				m_save_timer.Stop();
			}
			m_save_timer = null;
			if (m_saveplay_timeout != null)
			{
				m_saveplay_timeout.Stop();
			}
			m_saveplay_timeout = null;
		}

		protected void SetInputLocked(bool p_flag)
		{
			if (base.validContext)
			{
				model.lockInput = p_flag;
				view.ui.screen.mouseEnabled = !p_flag;
			}
		}

		protected void SetMapSaveEnabled(bool p_flag)
		{
			if (base.validContext)
			{
				view.ui.isMapSaving = p_flag;
			}
		}

		public void RefreshGrid()
		{
			float p_unit = ((model.state.metric.mode == MEMetricMode.Metric) ? 1f : 3.28084f);
			view.SetGridState(view.ui.gizmoGridState, p_unit);
		}

		public void RefreshGameModeStats()
		{
			if (m_map_distance_timer != null)
			{
				return;
			}
			m_map_distance_timer = RunOnce(delegate
			{
				m_map_distance_timer = null;
				view.ui.collectableCountFade.gameObject.SetActive(value: false);
				view.ui.trackDistanceFade.gameObject.SetActive(value: false);
				switch (model.data.mode.typeFlag)
				{
				case GameFlag.Freestyle:
				case GameFlag.Race:
				{
					view.ui.trackDistanceFade.gameObject.SetActive(value: true);
					float mapDistance = model.GetMapDistance();
					view.ui.SetMapDistance(mapDistance);
					break;
				}
				case GameFlag.Collectable:
				{
					view.ui.collectableCountFade.gameObject.SetActive(value: true);
					int mapCollectableCount = model.GetMapCollectableCount();
					view.ui.SetCollectableCount(mapCollectableCount);
					break;
				}
				}
			}, 1f / 60f);
		}

		public void RefreshRendererStats()
		{
			if (m_renderer_stats_timer == null)
			{
				m_map_distance_timer = RunOnce(delegate
				{
					m_renderer_stats_timer = null;
					Vector2Int rendererStats = model.GetRendererStats();
					bool flag = base.game.model.level.settings.scene.IsBaseAssetsEnabled();
					int num = 0;
					int num2 = 0;
					LevelSettings.Scene scene = base.game.model.level.settings.scene;
					num += (flag ? (scene?.GetBaseAssetsCount() ?? 0) : 0);
					scene = base.game.model.level.track.settings.scene;
					num += (flag ? (scene?.GetBaseAssetsCount() ?? 0) : 0);
					LevelSettings.Stats stats = base.game.model.level.settings.stats;
					num2 += (flag ? stats.trianglesCount : stats.trianglesCountClean);
					num += rendererStats.y;
					num2 += rendererStats.x;
					view.ui.SetRendererStats(num2, num);
				}, 1f / 12f);
			}
		}

		private bool ReplayCacheFilter(FileInfo it)
		{
			if (model.data == null)
			{
				return false;
			}
			return it.FullName.Contains(model.data.guid);
		}
	}
}
