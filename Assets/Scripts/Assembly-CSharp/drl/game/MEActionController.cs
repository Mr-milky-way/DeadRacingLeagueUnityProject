using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MEActionController : Controller<DRLApp>
	{
		public bool allowRecord;

		public MapEditorController editor => AssertParent<MapEditorController>("editor");

		public MEActionModel model => AssertLocal<MEActionModel>("model");

		protected void Awake()
		{
			allowRecord = true;
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			MEStateModel state = editor.model.state;
			MESelectionModel selection = editor.model.selection;
			if (!allowRecord)
			{
				return;
			}
			object obj = ((p_data.Length != 0) ? p_data[0] : null);
			object obj2 = ((p_data.Length > 1) ? p_data[1] : null);
			object obj3 = ((p_data.Length > 2) ? p_data[2] : null);
			object obj4 = ((p_data.Length > 3) ? p_data[3] : null);
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "map-editor.inspector.begin-change":
			case "map-editor.inspector.end-change":
				OnInspectorNotification(p_event, p_target, p_data);
				break;
			case "map-editor.control.begin-change":
			case "map-editor.control.end-change":
				OnControlNotification(p_event, p_target, p_data);
				break;
			case "map-editor.handle@down":
				if (selection.anyEntity)
				{
					TRSHandle tRSHandle = p_data[0] as TRSHandle;
					state.SetTransformsFrom(tRSHandle.targets);
				}
				break;
			case "map-editor.handle@drag-end":
				if (selection.anyEntity)
				{
					TRSHandle tRSHandle2 = p_data[0] as TRSHandle;
					editor.view.scene.ApplySceneBounds(tRSHandle2.targets);
					editor.view.AssertSceneSnapping(tRSHandle2.targets);
					state.SetTransformsTo(tRSHandle2.targets);
					model.Record(MEActionType.ChangeTransform, true, state.transformFrom, state.transformTo);
				}
				break;
			case "map-editor.selection.entities@change":
			{
				List<string> list2 = p_data[0] as List<string>;
				List<string> list3 = p_data[1] as List<string>;
				model.Record(MEActionType.ChangeEntitySelection, false, list2, list3);
				break;
			}
			case "map-editor.selection.assets@change":
			{
				List<string> list4 = p_data[0] as List<string>;
				List<string> list5 = p_data[1] as List<string>;
				model.Record(MEActionType.ChangeAssetSelection, false, list4, list5);
				break;
			}
			case "map-editor.entity.clone":
			case "map-editor.entity.create":
			{
				List<MAEntity> list6 = ((obj == null) ? new List<MAEntity>() : ((List<MAEntity>)obj));
				Component p_parent = ((obj2 == null) ? editor.view.scene.root : (obj2 as Component));
				bool flag = obj3 != null && (bool)obj3;
				bool flag2 = obj4 == null || (bool)obj4;
				List<string> list7 = new List<string>();
				bool flag3 = p_event == "map-editor.entity.clone";
				List<string> list8 = new List<string>();
				if (flag3 && !flag)
				{
					p_parent = null;
				}
				string p_id = "";
				for (int num5 = 0; num5 < list6.Count; num5++)
				{
					MAEntity mAEntity = list6[num5];
					if (!mAEntity)
					{
						list6.RemoveAt(num5--);
						continue;
					}
					string item = editor.model.CloneDataJson(mAEntity, ref p_id, p_parent);
					list7.Add(item);
					if (flag3)
					{
						list8.Add(p_id);
					}
				}
				MEActionType p_type = (flag3 ? MEActionType.EntityClone : MEActionType.EntityCreate);
				model.Record(p_type, true, list7);
				if (flag3)
				{
					List<MAEntity> list9 = editor.view.scene.FindAllById<MAEntity>(list8);
					editor.view.scene.InsertGates(list9);
					if (flag2)
					{
						editor.model.selection.SetEntity(list9, p_combine: false);
						editor.RefreshActionHandle();
					}
					Debug.Log("MEActionController> EntityClone / count[" + list9.Count + "]");
				}
				break;
			}
			case "map-editor.entity.delete":
			{
				List<string> list = ((obj == null) ? new List<MAEntity>() : ((List<MAEntity>)obj)).ConvertAll((MAEntity it) => it.data.ToJson());
				model.Record(MEActionType.EntityDelete, true, list);
				break;
			}
			case "map-editor.gate.order@change":
			{
				int num3 = (int)obj;
				int num4 = (int)obj2;
				model.Record(MEActionType.ChangeGateOrder, true, num3, num4);
				break;
			}
			case "map-editor.podium.order@change":
			{
				int num = (int)obj;
				int num2 = (int)obj2;
				model.Record(MEActionType.ChangePodiumOrder, true, num, num2);
				break;
			}
			case "map-editor.action.undo":
			case "map-editor.action.redo":
			case "map-editor.action.record":
				editor.view.ui.SetUndoEnabled(model.undoAllowed);
				editor.view.ui.SetRedoEnabled(model.redoAllowed);
				break;
			case "map-editor.action.apply":
				OnActionApply(obj as MEActionData);
				break;
			case "map-editor.action.apply-reverse":
				OnActionApplyReverse(obj as MEActionData);
				break;
			}
		}

		protected void OnControlNotification(string p_event, Object p_target, params object[] p_data)
		{
			MEStateModel state = editor.model.state;
			_ = editor.model.selection;
			object obj = ((p_data.Length != 0) ? p_data[0] : null);
			object obj2 = ((p_data.Length > 1) ? p_data[1] : null);
			Component component = (Component)obj;
			string obj3 = (component ? component.GetType().Name : "");
			string text = (string)obj2;
			string text2 = obj3 + (string.IsNullOrEmpty(text) ? "" : ("." + text));
			if (text2 != null && text2 == "MERulersMetricWidget.snap")
			{
				MERulersMetricWidget mERulersMetricWidget = (MERulersMetricWidget)component;
				switch (p_event)
				{
				case "map-editor.control.begin-change":
					state.SetTransformsFrom(mERulersMetricWidget.targets);
					break;
				case "map-editor.control.end-change":
					editor.view.scene.ApplySceneBounds(mERulersMetricWidget.targets);
					editor.view.AssertSceneSnapping(mERulersMetricWidget.targets);
					state.SetTransformsTo(mERulersMetricWidget.targets);
					model.Record(MEActionType.ChangeTransform, false, state.transformFrom, state.transformTo);
					break;
				}
			}
		}

		protected void OnInspectorNotification(string p_event, Object p_target, params object[] p_data)
		{
			MEInspector mEInspector = p_data[0] as MEInspector;
			MEStateModel state = editor.model.state;
			MESelectionModel selection = editor.model.selection;
			if (p_data.Length != 0)
			{
				_ = p_data[0];
			}
			string text = (string)((p_data.Length > 1) ? p_data[1] : null);
			if (text.Contains("layout-distribute"))
			{
				text = "layout-distribute";
			}
			if (text.Contains("layout-align"))
			{
				text = "layout-align";
			}
			if (text.Contains("layout-orient"))
			{
				text = "layout-orient";
			}
			MEActionType mEActionType = MEActionType.None;
			if (MEInspectorFieldIds.PropertiesUndoFields.Contains(text))
			{
				mEActionType = MEActionType.ChangeProperty;
			}
			if (MEInspectorFieldIds.TransformUndoFields.Contains(text))
			{
				mEActionType = MEActionType.ChangeTransform;
			}
			switch (mEActionType)
			{
			case MEActionType.ChangeTransform:
				switch (p_event)
				{
				case "map-editor.inspector.begin-change":
					state.SetTransformsFrom(mEInspector.targets);
					break;
				case "map-editor.inspector.end-change":
					if (selection.anyEntity)
					{
						editor.view.scene.ApplySceneBounds(mEInspector.targets);
						editor.view.AssertSceneSnapping(mEInspector.targets);
						state.SetTransformsTo(mEInspector.targets);
						model.RecordDelay(mEActionType, true, 0.5f, state.transformFrom, state.transformTo);
						if (text != null && text == "physics-simulation-toggle")
						{
							editor.view.handle.SetHandle(HandleModeType.None);
						}
					}
					break;
				}
				break;
			case MEActionType.ChangeProperty:
				switch (p_event)
				{
				case "map-editor.inspector.begin-change":
					state.SetPropertiesFrom(mEInspector.targets);
					break;
				case "map-editor.inspector.end-change":
					if (selection.anyEntity)
					{
						state.SetPropertiesTo(mEInspector.targets);
						model.RecordDelay(mEActionType, true, 0.5f, state.propertyFrom, state.propertyTo);
					}
					break;
				}
				break;
			}
		}

		protected void OnActionApply(MEActionData p_action)
		{
			MEStateModel state = editor.model.state;
			MESelectionModel selection = editor.model.selection;
			_ = model;
			List<string> p_ids = p_action.Get("selection-entity", new List<string>());
			p_action.Get("selection-asset", new List<string>());
			bool flag = allowRecord;
			allowRecord = false;
			switch (p_action.type)
			{
			case MEActionType.ChangeEntitySelection:
			{
				List<string> p_ids2 = p_action.Get("value-to", new List<string>());
				List<MAEntity> p_items = editor.view.scene.FindAllById<MAEntity>(p_ids2);
				selection.SetEntity(p_items, p_combine: false);
				break;
			}
			case MEActionType.ChangeAssetSelection:
			{
				List<string> p_guids = p_action.Get("value-to", new List<string>());
				List<MapAsset> p_items2 = base.app.model.storage.library.FindByGUID<MapAsset>(p_guids);
				selection.SetAsset(p_items2, p_combine: false);
				break;
			}
			case MEActionType.ChangeRenderState:
			{
				MERenderStateType render = p_action.Get<MERenderStateType>("render-state-to");
				state.render = render;
				break;
			}
			case MEActionType.ChangeTransform:
			{
				List<TransformVector> list2 = p_action.Get<List<TransformVector>>("value-to");
				List<MAEntity> list3 = editor.view.scene.FindAllById<MAEntity>(p_ids);
				int num = Mathf.Min(list3.Count, list2.Count);
				for (int num2 = 0; num2 < num; num2++)
				{
					for (int num3 = 0; num3 < num; num3++)
					{
						bool flag2 = list2[num3].target.name == list3[num2].name;
						bool num4 = list2[num3].target == list3[num2].transform;
						bool flag3 = false;
						if (num4)
						{
							flag3 = list2[num3].Get(list3[num2].transform, p_local: true);
						}
						if (flag3)
						{
							break;
						}
						if (flag2)
						{
							flag3 = list2[num3].Get(list3[num2].transform, p_local: true);
						}
						if (flag3)
						{
							break;
						}
					}
				}
				editor.view.handle.Refresh();
				break;
			}
			case MEActionType.ChangeProperty:
			{
				List<string> list6 = p_action.Get<List<string>>("value-to");
				List<MAEntity> list7 = editor.view.scene.FindAllById<MAEntity>(p_ids);
				int num5 = Mathf.Min(list7.Count, list6.Count);
				for (int num6 = 0; num6 < num5; num6++)
				{
					MAEntity mAEntity = list7[num6];
					if (!mAEntity)
					{
						continue;
					}
					string id = mAEntity.id;
					for (int num7 = 0; num7 < list6.Count; num7++)
					{
						string text = list6[num7];
						if (!text.Contains(id))
						{
							Debug.LogWarning("MEActionController> Failed to find [" + id + "] id");
							continue;
						}
						mAEntity.data.Load(text);
						mAEntity.Read();
					}
				}
				break;
			}
			case MEActionType.EntityCreate:
			case MEActionType.EntityClone:
			{
				List<MDEntity> list4 = p_action.Get("value-to", new List<string>()).ConvertAll(delegate(string it)
				{
					MDEntity mDEntity = new MDEntity();
					mDEntity.Load(it);
					return mDEntity;
				});
				List<MAEntity> list5 = editor.view.scene.Create<MAEntity>(list4, null);
				string p_event = ((p_action.type == MEActionType.EntityClone) ? "map-editor.scene.entity.clone" : "map-editor.scene.entity.create");
				Notify(p_event, list5);
				list4.Clear();
				break;
			}
			case MEActionType.EntityDelete:
			{
				List<string> list = p_action.Get("value-to", new List<string>()).ConvertAll(delegate(string it)
				{
					MDEntity mDEntity = new MDEntity();
					mDEntity.Load(it);
					return mDEntity.id;
				});
				editor.view.scene.Destroy(list);
				list.Clear();
				break;
			}
			case MEActionType.ChangeGateOrder:
			{
				int p_i3 = p_action.Get<int>("value-from");
				int p_i4 = p_action.Get<int>("value-to");
				List<MAGate> gatesGraph = editor.view.scene.SetGateOrder(p_i3, p_i4);
				editor.view.ui.controls.SetGatesGraph(gatesGraph);
				break;
			}
			case MEActionType.ChangePodiumOrder:
			{
				int p_i = p_action.Get<int>("value-from");
				int p_i2 = p_action.Get<int>("value-to");
				List<MAPodium> podiumsGraph = editor.view.scene.SetPodiumOrder(p_i, p_i2);
				editor.view.ui.controls.SetPodiumsGraph(podiumsGraph);
				break;
			}
			}
			editor.ScheduleSave();
			allowRecord = flag;
		}

		protected void OnActionApplyReverse(MEActionData p_action)
		{
			MEStateModel state = editor.model.state;
			MESelectionModel selection = editor.model.selection;
			_ = model;
			List<string> p_ids = p_action.Get("selection-entity", new List<string>());
			p_action.Get("selection-asset", new List<string>());
			bool flag = allowRecord;
			allowRecord = false;
			switch (p_action.type)
			{
			case MEActionType.ChangeEntitySelection:
			{
				List<string> p_ids2 = p_action.Get("value-from", new List<string>());
				List<MAEntity> p_items2 = editor.view.scene.FindAllById<MAEntity>(p_ids2);
				selection.SetEntity(p_items2, p_combine: false);
				break;
			}
			case MEActionType.ChangeAssetSelection:
			{
				List<string> p_guids = p_action.Get("value-from", new List<string>());
				List<MapAsset> p_items = base.app.model.storage.library.FindByGUID<MapAsset>(p_guids);
				selection.SetAsset(p_items, p_combine: false);
				break;
			}
			case MEActionType.ChangeRenderState:
			{
				MERenderStateType render = p_action.Get<MERenderStateType>("render-state");
				state.render = render;
				break;
			}
			case MEActionType.ChangeTransform:
			{
				List<TransformVector> list2 = p_action.Get<List<TransformVector>>("value-from");
				List<MAEntity> list3 = editor.view.scene.FindAllById<MAEntity>(p_ids);
				int num = Mathf.Min(list3.Count, list2.Count);
				for (int num2 = 0; num2 < num; num2++)
				{
					list2[num2].Get(list3[num2].transform, p_local: true);
				}
				editor.view.handle.Refresh();
				break;
			}
			case MEActionType.ChangeProperty:
			{
				List<string> list7 = p_action.Get<List<string>>("value-from");
				List<MAEntity> list8 = editor.view.scene.FindAllById<MAEntity>(p_ids);
				int num5 = Mathf.Min(list8.Count, list7.Count);
				for (int num6 = 0; num6 < num5; num6++)
				{
					MAEntity mAEntity = list8[num6];
					if (!mAEntity)
					{
						continue;
					}
					string id = mAEntity.id;
					for (int num7 = 0; num7 < list7.Count; num7++)
					{
						string text = list7[num7];
						if (text.Contains(id))
						{
							mAEntity.data.Load(text);
							mAEntity.Read();
						}
					}
				}
				break;
			}
			case MEActionType.EntityDelete:
			{
				List<MDEntity> list4 = p_action.Get("value-to", new List<string>()).ConvertAll(delegate(string it)
				{
					MDEntity mDEntity = new MDEntity();
					mDEntity.Load(it);
					return mDEntity;
				});
				List<MAEntity> list5 = editor.view.scene.Create<MAEntity>(list4, null);
				List<MASpline> list6 = new List<MASpline>();
				for (int num3 = 0; num3 < list5.Count; num3++)
				{
					if (list5[num3].data.type == MapAssetType.SplineControlPoint)
					{
						MASpline spline = ((MASplineControlPoint)list5[num3]).spline;
						if ((bool)spline && !list6.Contains(spline))
						{
							list6.Add(spline);
						}
					}
				}
				for (int num4 = 0; num4 < list6.Count; num4++)
				{
					list6[num4].AssertSiblingIndexes();
				}
				list4.Clear();
				break;
			}
			case MEActionType.EntityCreate:
			case MEActionType.EntityClone:
			{
				List<string> list = p_action.Get("value-to", new List<string>()).ConvertAll(delegate(string it)
				{
					MDEntity mDEntity = new MDEntity();
					mDEntity.Load(it);
					return mDEntity.id;
				});
				editor.view.scene.Destroy(list);
				list.Clear();
				break;
			}
			case MEActionType.ChangeGateOrder:
			{
				int p_i3 = p_action.Get<int>("value-to");
				int p_i4 = p_action.Get<int>("value-from");
				List<MAGate> gatesGraph = editor.view.scene.SetGateOrder(p_i3, p_i4);
				editor.view.ui.controls.SetGatesGraph(gatesGraph);
				break;
			}
			case MEActionType.ChangePodiumOrder:
			{
				int p_i = p_action.Get<int>("value-to");
				int p_i2 = p_action.Get<int>("value-from");
				List<MAPodium> podiumsGraph = editor.view.scene.SetPodiumOrder(p_i, p_i2);
				editor.view.ui.controls.SetPodiumsGraph(podiumsGraph);
				break;
			}
			}
			editor.ScheduleSave();
			allowRecord = flag;
		}
	}
}
