using System.Collections.Generic;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MEInfoHelpLayer : View<DRLApp>
	{
		public ListComponent list;

		public List<MEInfoHelpTagView> tags;

		private MonoActivity m_refresh_timer;

		public MEInfoHelpLayerModel model => AssertLocal<MEInfoHelpLayerModel>("model");

		public MEInfoHelpLayerController controller => AssertLocal<MEInfoHelpLayerController>("controller");

		public void Initialize()
		{
			for (int i = 0; i < model.helpData.Count; i++)
			{
				model.helpData[i].order += i;
			}
			model.helpData.Sort((MEInfoHelpData a, MEInfoHelpData b) => (a.order != b.order) ? ((a.order >= b.order) ? 1 : (-1)) : 0);
			for (int num = 0; num < model.helpData.Count; num++)
			{
				Add(model.helpData[num]);
			}
			Clear();
			RunOnce(Refresh, 0.05f);
		}

		public void Clear()
		{
			for (int i = 0; i < tags.Count; i++)
			{
				tags[i].gameObject.SetActive(value: false);
			}
		}

		public void SetDirty()
		{
			if (m_refresh_timer == null)
			{
				m_refresh_timer = RunOnce(delegate
				{
					m_refresh_timer = null;
					Refresh();
				}, 0.05f);
			}
		}

		public void Refresh()
		{
			Clear();
			int num = 0;
			int num2 = -1;
			for (int i = 0; i < tags.Count; i++)
			{
				MEInfoHelpTagView mEInfoHelpTagView = tags[i];
				bool flag = OnHelpTagFilter(mEInfoHelpTagView);
				mEInfoHelpTagView.gameObject.SetActive(flag);
				if (flag)
				{
					int background = ((num > 0) ? 1 : 0);
					mEInfoHelpTagView.SetBackground(background);
					num++;
					num2 = i;
				}
			}
			if (num2 >= 0 && num2 < tags.Count)
			{
				tags[num2].SetBackground((num <= 1) ? (-1) : 2);
			}
		}

		public MEInfoHelpTagView Add(string p_label, string p_default_label, string p_icon, string p_key, bool p_reverse = false)
		{
			MEInfoHelpTagView mEInfoHelpTagView = list.Push<MEInfoHelpTagView>();
			string p_label2 = p_label;
			if (p_default_label != null)
			{
				Localization instance = Localization.instance;
				p_label2 = (instance ? instance.Get<string>(p_label, p_default_label) : p_default_label);
			}
			mEInfoHelpTagView.Set(p_label2, p_icon, p_key, p_reverse);
			tags.Add(mEInfoHelpTagView);
			return mEInfoHelpTagView;
		}

		public MEInfoHelpTagView Add(string p_label, string p_icon, string p_key, bool p_reverse = false)
		{
			return Add(p_label, null, p_icon, p_key, p_reverse);
		}

		public MEInfoHelpTagView Add(MEInfoHelpData p_data)
		{
			MEInfoHelpTagView mEInfoHelpTagView = list.Push<MEInfoHelpTagView>();
			mEInfoHelpTagView.Set(p_data);
			tags.Add(mEInfoHelpTagView);
			return mEInfoHelpTagView;
		}

		protected bool OnHelpTagFilter(MEInfoHelpTagView p_tag)
		{
			MapEditorController editor = controller.controller.editor;
			MEStateModel state = editor.model.state;
			MEInfoHelpData data = p_tag.data;
			bool anyEntity = editor.model.selection.anyEntity;
			bool anyAsset = editor.model.selection.anyAsset;
			int count = editor.model.selection.assets.Count;
			bool flag = count > 0;
			bool moving = editor.view.handle.moving;
			bool flag2 = false;
			if (data == null)
			{
				return flag2;
			}
			switch (data.type)
			{
			case MEInfoHelpType.HideUI:
				flag2 = state.input == MEInputStateType.Action && !anyAsset && !anyEntity;
				break;
			case MEInfoHelpType.InputCameraNav:
				flag2 = state.input == MEInputStateType.Action && state.action == MEActionStateType.Select;
				break;
			case MEInfoHelpType.CameraNavMove:
				flag2 = state.input == MEInputStateType.Navigate;
				break;
			case MEInfoHelpType.InputCameraOrbit:
				flag2 = state.input == MEInputStateType.Action && state.action == MEActionStateType.Select;
				break;
			case MEInfoHelpType.CameraOrbitDistance:
				flag2 = state.input == MEInputStateType.Orbit;
				break;
			case MEInfoHelpType.CameraOrbitRotate:
				flag2 = state.input == MEInputStateType.Orbit;
				break;
			case MEInfoHelpType.InputCameraPan:
				flag2 = state.input == MEInputStateType.Action && state.action == MEActionStateType.Select;
				break;
			case MEInfoHelpType.ShiftFastRate:
				flag2 = flag2 || state.input == MEInputStateType.Navigate || state.action != MEActionStateType.Select;
				break;
			case MEInfoHelpType.RaceLayoutMode:
				flag2 = state.render == MERenderStateType.Scene && state.input == MEInputStateType.Action && !anyEntity;
				break;
			case MEInfoHelpType.SceneLayoutMode:
				flag2 = state.render == MERenderStateType.Race && state.input == MEInputStateType.Action && !anyEntity;
				break;
			case MEInfoHelpType.EntityTransform:
				flag2 = anyEntity;
				break;
			case MEInfoHelpType.EntityClone:
				flag2 = state.input == MEInputStateType.Action;
				flag2 = flag2 && anyEntity && state.action == MEActionStateType.Select;
				break;
			case MEInfoHelpType.EntityFocus:
				flag2 = state.input == MEInputStateType.Action;
				flag2 = flag2 && anyEntity && state.action == MEActionStateType.Select;
				break;
			case MEInfoHelpType.EntityTRSMove:
				flag2 = anyEntity && state.action == MEActionStateType.Move && state.input == MEInputStateType.Action;
				break;
			case MEInfoHelpType.EntityTRSRotate:
				flag2 = anyEntity && state.action == MEActionStateType.Rotate && state.input == MEInputStateType.Action;
				break;
			case MEInfoHelpType.EntityTRSScale:
				flag2 = anyEntity && state.action == MEActionStateType.Scale && state.input == MEInputStateType.Action;
				break;
			case MEInfoHelpType.SnapOnOff:
				flag2 = moving;
				break;
			case MEInfoHelpType.SnapMoveOnOff:
				flag2 = !moving && state.action == MEActionStateType.Move;
				break;
			case MEInfoHelpType.SnapRotateOnOff:
				flag2 = !moving && state.action == MEActionStateType.Rotate;
				break;
			case MEInfoHelpType.StatsTargetMode:
				flag2 = state.input == MEInputStateType.Action;
				break;
			}
			if (flag)
			{
				flag2 = false;
				switch (data.type)
				{
				case MEInfoHelpType.PreviewDrop:
					flag2 = true;
					break;
				case MEInfoHelpType.PreviewSwitch:
					flag2 = count > 1;
					break;
				case MEInfoHelpType.PreviewDistance:
					flag2 = true;
					break;
				case MEInfoHelpType.PreviewAngle:
					flag2 = true;
					break;
				case MEInfoHelpType.PreviewAlignOnOff:
					flag2 = true;
					break;
				case MEInfoHelpType.ShiftFastRate:
					flag2 = true;
					break;
				}
			}
			return flag2;
		}
	}
}
