using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MapEditorView : View<DRLApp>
	{
		public List<GridComponent> grids;

		public UIMapEditorView ui;

		public Texture2D screenshot;

		public MECamera camera => AssertFind<MECamera>("camera");

		public CameraCapture capture => Assert<CameraCapture>("capture");

		public MESceneView scene => AssertFind<MESceneView>("scene");

		public MEHandleView handle => AssertFind<MEHandleView>("handle");

		public LevelFactory factory => controller.game.level.factory;

		public MapEditorController controller => AssertLocal<MapEditorController>("controller");

		public MapEditorModel model => AssertLocal<MapEditorModel>("model");

		public void SetGridState(MapEditorGridStateType p_type, float p_unit = 1f)
		{
			float num = 0.3f;
			float p_size = grids[1].size;
			switch (p_type)
			{
			case MapEditorGridStateType.Off:
				num = 0f;
				break;
			case MapEditorGridStateType.Size2:
				p_size = 2f;
				break;
			case MapEditorGridStateType.Size3:
				p_size = 3f;
				break;
			case MapEditorGridStateType.Size4:
				p_size = 4f;
				break;
			case MapEditorGridStateType.Size5:
				p_size = 5f;
				break;
			}
			grids[0].transform.localScale = new Vector3(p_unit, 1f, p_unit);
			grids[1].transform.localScale = new Vector3(p_unit, 1f, p_unit);
			grids[0].Fade(num * 0.12f, 0.15f);
			grids[0].FadeSize(p_size, 0.15f);
			grids[1].Fade(num, 0.15f);
			grids[1].FadeSize(p_size, 0.15f);
		}

		public void SetGridActive(bool p_flag)
		{
			for (int i = 0; i < grids.Count; i++)
			{
				grids[i].gameObject.SetActive(p_flag);
			}
		}

		public void SetEditorCameraStart(DroneCamera p_camera)
		{
			if ((bool)p_camera)
			{
				Debug.Log("MapEditorView> SetCamera - camera[" + p_camera?.ToString() + "]");
				Vector3 p_offset = new Vector3(0f, 1f, 1f);
				if (scene.FindPodiums().Count <= 0)
				{
					base.app.model.game.level.track.SetStartsFrontTransform(p_camera.transform, p_offset);
				}
				else
				{
					MoveCloseToPodiums(p_camera.transform, p_offset);
				}
				p_camera.orbit.Snap();
			}
		}

		public void MoveCloseToPodiums(Transform p_target, Vector3 p_offset)
		{
			Vector3 podiumsCenter = scene.GetPodiumsCenter();
			Quaternion quaternion = Quaternion.Euler(scene.GetPodiumsRotation());
			quaternion = Quaternion.LookRotation(quaternion * Vector3.forward, Vector3.up);
			p_target.position = podiumsCenter;
			p_target.localRotation = quaternion;
			p_target.position += p_target.right * p_offset.x;
			p_target.position += p_target.up * p_offset.y;
			p_target.position += p_target.forward * p_offset.z;
		}

		public void CaptureScreenshot(int p_width, int p_height, Action<string, string, Texture2D> p_callback = null)
		{
			if ((bool)screenshot)
			{
				UnityEngine.Object.Destroy(screenshot);
				screenshot = null;
			}
			ServiceModel sm = base.app.model.service;
			capture.width = p_width;
			capture.height = p_height;
			capture.smooth = false;
			Notify("map-editor.save.map-thumb@start");
			if (p_callback != null)
			{
				p_callback("map-editor.save.map-thumb@start", null, null);
			}
			capture.Capture(delegate(Texture2D p_texture)
			{
				screenshot = p_texture;
				Debug.Log("MapEditorView> StorageImage / CaptureScreenshot Complete");
				if (!screenshot)
				{
					Notify("map-editor.save.map-thumb@error");
					if (p_callback != null)
					{
						p_callback("map-editor.save.map-thumb@error", null, null);
					}
				}
				else if (!DRLApp.offline)
				{
					sm.StorageImage("map-editor-thumb", screenshot.EncodeToJPG(), delegate(string p_thumb_url)
					{
						Debug.Log("MapEditorView> StorageImage / Complete - url[" + p_thumb_url + "]");
						if (string.IsNullOrEmpty(p_thumb_url))
						{
							Notify("map-editor.save.map-thumb@error");
							if (p_callback != null)
							{
								p_callback("map-editor.save.map-thumb@error", null, null);
							}
						}
						else
						{
							if (p_callback != null)
							{
								p_callback("map-editor.save.map-thumb@success", p_thumb_url, p_texture);
							}
							Notify(1f / 60f, "map-editor.save.map-thumb@success", screenshot, p_thumb_url, p_texture);
						}
					});
				}
				else
				{
					this.TimerRunOnce(delegate
					{
						if (p_callback != null)
						{
							p_callback("map-editor.save.map-thumb@success", "", p_texture);
						}
						Notify(1f / 60f, "map-editor.save.map-thumb@success", screenshot, "", p_texture);
					}, 1f);
				}
			}, p_defer: true);
		}

		public void RefreshSnapMove()
		{
			bool flag = model.selection.anyEntity && model.selection.TrueForAllEntities(MapAssetType.NoForceGrid);
			bool snapMap = model.state.metric.snapMap;
			ui.SetMetricSnapMoveLock(snapMap && !flag);
			model.state.metric.snapMap = snapMap && !flag;
			bool metricSnapMoveEnabled = model.state.metric.IsSnapMove();
			float metricSnapMove = (model.state.metric.snapMap ? model.state.metric.snapMapMoveUnit : model.state.metric.snapMoveUnit);
			float snapMoveUnit = model.state.metric.GetSnapMoveUnit();
			model.state.metric.snapMap = snapMap;
			model.lockInput = true;
			ui.SetMetricSnapMoveEnabled(metricSnapMoveEnabled);
			ui.SetMetricSnapMove(metricSnapMove);
			handle.move.snap = snapMoveUnit;
			model.lockInput = false;
		}

		public void RefreshSnapRotate()
		{
			bool flag = model.selection.anyEntity && model.selection.TrueForAllEntities(MapAssetType.NoForceGrid);
			bool snapMap = model.state.metric.snapMap;
			ui.SetMetricSnapRotateLock(snapMap && !flag);
			model.state.metric.snapMap = snapMap && !flag;
			bool metricSnapRotateEnabled = model.state.metric.IsSnapRotate();
			float metricSnapRotate = (model.state.metric.snapMap ? model.state.metric.snapMapRotateUnit : model.state.metric.snapRotateUnit);
			float snapRotateUnit = model.state.metric.GetSnapRotateUnit();
			model.state.metric.snapMap = snapMap;
			model.lockInput = true;
			ui.SetMetricSnapRotateEnabled(metricSnapRotateEnabled);
			ui.SetMetricSnapRotate(metricSnapRotate);
			handle.rotate.snap = snapRotateUnit;
			model.lockInput = false;
		}

		public void AssertSceneSnapping(IList p_targets)
		{
			if (!model.state.metric.snapMap)
			{
				return;
			}
			float snapMapRotateUnit = model.state.metric.snapMapRotateUnit;
			float snapMapMoveUnit = model.state.metric.snapMapMoveUnit;
			IList list;
			if (p_targets != null)
			{
				list = p_targets;
			}
			else
			{
				IList hierarchy = scene.hierarchy;
				list = hierarchy;
			}
			IList list2 = list;
			for (int i = 0; i < list2.Count; i++)
			{
				object obj = list2[i];
				if (obj == null)
				{
					continue;
				}
				Transform transform = null;
				if (obj is Component)
				{
					transform = ((Component)obj).transform;
				}
				if (obj is GameObject)
				{
					transform = ((GameObject)obj).transform;
				}
				if (!transform)
				{
					continue;
				}
				MARenderer componentCached = scene.GetComponentCached<MARenderer>(transform.gameObject);
				if ((bool)componentCached && !componentCached.tags.Contains(MapAssetType.NoForceGrid))
				{
					if (snapMapMoveUnit > 0f)
					{
						Vector3 position = transform.position;
						position.x = SignedRound(position.x / snapMapMoveUnit) * snapMapMoveUnit;
						position.y = SignedRound(position.y / snapMapMoveUnit) * snapMapMoveUnit;
						position.z = SignedRound(position.z / snapMapMoveUnit) * snapMapMoveUnit;
						transform.position = position;
					}
					if (snapMapRotateUnit > 0f)
					{
						Vector3 localEulerAngles = transform.localEulerAngles;
						localEulerAngles.x = SignedRound(localEulerAngles.x / snapMapRotateUnit) * snapMapRotateUnit;
						localEulerAngles.y = SignedRound(localEulerAngles.y / snapMapRotateUnit) * snapMapRotateUnit;
						localEulerAngles.z = SignedRound(localEulerAngles.z / snapMapRotateUnit) * snapMapRotateUnit;
						transform.localEulerAngles = localEulerAngles;
					}
				}
			}
		}

		private float SignedRound(float v)
		{
			float num = ((v < 0f) ? (-1f) : 1f);
			return Mathf.Round(Mathf.Abs(v)) * num;
		}

		public void AssertSceneSnapping()
		{
			AssertSceneSnapping(null);
		}

		private bool IsNoForceGrid(MapAssetType f)
		{
			return f == MapAssetType.NoForceGrid;
		}
	}
}
