using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MEActionModel : Model<DRLApp>
	{
		public List<MEActionData> actions;

		public int index;

		public int max = 80;

		private MonoActivity m_record_timer;

		public MapEditorController editor => AssertParent<MapEditorController>("editor");

		public int count => actions.Count;

		public bool undoAllowed
		{
			get
			{
				if (count <= 0)
				{
					return false;
				}
				return index >= 1;
			}
		}

		public bool redoAllowed
		{
			get
			{
				if (count <= 0)
				{
					return false;
				}
				return index <= count - 1;
			}
		}

		public MEActionData current
		{
			get
			{
				if (index >= 0)
				{
					return Reflection<object>.Get(actions, index);
				}
				return null;
			}
		}

		public MEActionData Get(int p_index)
		{
			return Reflection<object>.Get(actions, p_index);
		}

		public void Record(MEActionData p_action, bool p_apply)
		{
			if (p_action == null)
			{
				return;
			}
			while (index < actions.Count)
			{
				if (actions.Count > 0)
				{
					actions.RemoveAt(actions.Count - 1);
				}
			}
			actions.Add(p_action);
			if (actions.Count >= max)
			{
				actions.RemoveAt(0);
			}
			index++;
			AssertIndex();
			Notify("map-editor.action.record", p_action);
			if (p_apply)
			{
				Notify("map-editor.action.apply", p_action);
			}
		}

		public void Record(MEActionData p_action)
		{
			Record(p_action, p_apply: true);
		}

		public void Record(MEActionType p_type, bool p_apply, params object[] p_data)
		{
			if (!(this == null))
			{
				MEActionData mEActionData = new MEActionData();
				mEActionData.id = p_type.ToString().ToLower();
				mEActionData.type = p_type;
				OnActionInitialize(mEActionData, p_data);
				Record(mEActionData, p_apply);
			}
		}

		public void RecordDelay(MEActionType p_type, bool p_apply, float p_delay, params object[] p_data)
		{
			if (m_record_timer == null)
			{
				_ = editor.model.selection.entitiesIds;
				Action action = null;
				action = delegate
				{
					m_record_timer = null;
					Record(p_type, p_apply, p_data);
				};
				if (p_delay <= 0f)
				{
					action();
				}
				else
				{
					m_record_timer = RunOnce(action, p_delay);
				}
			}
		}

		protected void OnActionInitialize(MEActionData p_action, params object[] p_data)
		{
			_ = editor.model.state;
			MESelectionModel selection = editor.model.selection;
			List<string> p_value = new List<string>(selection.entitiesIds);
			p_action.Set("selection-entity", p_value);
			p_action.Set("selection-asset", selection.assetsGUIDs);
			object obj = ((p_data.Length != 0) ? p_data[0] : null);
			object obj2 = ((p_data.Length > 1) ? p_data[1] : null);
			if (p_data.Length > 2)
			{
				_ = p_data[2];
			}
			if (p_data.Length > 3)
			{
				_ = p_data[3];
			}
			switch (p_action.type)
			{
			case MEActionType.ChangeRenderState:
			{
				MERenderStateType mERenderStateType = (MERenderStateType)obj;
				MERenderStateType mERenderStateType2 = (MERenderStateType)obj2;
				p_action.Set("render-state", mERenderStateType);
				p_action.Set("render-state-to", mERenderStateType2);
				break;
			}
			case MEActionType.ChangeEntitySelection:
			{
				List<string> p_value6 = obj as List<string>;
				List<string> p_value7 = obj2 as List<string>;
				p_action.Set("value-from", p_value6);
				p_action.Set("value-to", p_value7);
				break;
			}
			case MEActionType.ChangeAssetSelection:
			{
				List<string> p_value4 = obj as List<string>;
				List<string> p_value5 = obj2 as List<string>;
				p_action.Set("value-from", p_value4);
				p_action.Set("value-to", p_value5);
				break;
			}
			case MEActionType.ChangeTransform:
			{
				List<TransformVector> collection3 = obj as List<TransformVector>;
				List<TransformVector> collection4 = obj2 as List<TransformVector>;
				p_action.Set("value-from", new List<TransformVector>(collection3));
				p_action.Set("value-to", new List<TransformVector>(collection4));
				break;
			}
			case MEActionType.ChangeProperty:
			{
				List<string> collection = obj as List<string>;
				List<string> collection2 = obj2 as List<string>;
				p_action.Set("value-from", new List<string>(collection));
				p_action.Set("value-to", new List<string>(collection2));
				break;
			}
			case MEActionType.ChangeGateOrder:
			{
				int num3 = (int)obj;
				int num4 = (int)obj2;
				p_action.Set("value-from", num3);
				p_action.Set("value-to", num4);
				break;
			}
			case MEActionType.ChangePodiumOrder:
			{
				int num = (int)obj;
				int num2 = (int)obj2;
				p_action.Set("value-from", num);
				p_action.Set("value-to", num2);
				break;
			}
			case MEActionType.EntityCreate:
			case MEActionType.EntityClone:
			{
				List<string> p_value3 = obj as List<string>;
				p_action.Set("value-to", p_value3);
				break;
			}
			case MEActionType.EntityDelete:
			{
				List<string> p_value2 = obj as List<string>;
				p_action.Set("value-to", p_value2);
				break;
			}
			}
		}

		public void Undo()
		{
			if (undoAllowed)
			{
				index--;
				AssertIndex();
				MEActionData mEActionData = current;
				if (mEActionData != null)
				{
					Notify("map-editor.action.apply-reverse", mEActionData);
					Notify("map-editor.action.undo", mEActionData);
				}
			}
		}

		public void Redo()
		{
			if (redoAllowed)
			{
				MEActionData mEActionData = current;
				index++;
				AssertIndex();
				if (mEActionData != null)
				{
					Notify("map-editor.action.apply", mEActionData);
					Notify("map-editor.action.redo", mEActionData);
				}
			}
		}

		public void Create(List<MAEntity> p_assets, Component p_container = null)
		{
			Notify("map-editor.entity.create", p_assets, p_container);
		}

		public void Create(MAEntity p_asset, Component p_container = null)
		{
			Create(new List<MAEntity> { p_asset }, p_container);
		}

		private void AssertIndex()
		{
			index = Mathf.Clamp(index, -1, actions.Count);
		}
	}
}
