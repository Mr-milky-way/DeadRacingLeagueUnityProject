using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.mvc;

namespace drl.game
{
	public class MEControlRulersLayerController : Controller<DRLApp>
	{
		public List<MAEntity> anchors;

		public List<Transform> targets;

		public UIMapEditorController controller => AssertFindReverse<UIMapEditorController>();

		public MEControlRulersLayer view => AssertLocal<MEControlRulersLayer>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != controller.view.screen)
			{
				return;
			}
			MEStateModel state = controller.editor.model.state;
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "map-editor.metric.ruler.state.change":
			{
				bool layerEnabled2 = (bool)p_data[0];
				if (!state.AllowSceneControls)
				{
					layerEnabled2 = false;
				}
				SetLayerEnabled(layerEnabled2);
				break;
			}
			case "map-editor.graph.rulers.widget.event@click":
			case "map-editor.graph.rulers.widget.event":
				OnWidgetFormEvent(p_event, p_target, p_data);
				break;
			case "map-editor.entity.delete":
			case "map-editor.render.state.change":
			case "map-editor.selection.assets@change":
			case "map-editor.selection.entities@change":
			{
				bool layerEnabled = state.AllowSceneControls && state.metric.showRulers;
				SetLayerEnabled(layerEnabled);
				break;
			}
			}
		}

		protected void OnWidgetFormEvent(string p_event, Object p_target, params object[] p_data)
		{
			MERulersMetricWidget mERulersMetricWidget = p_target as MERulersMetricWidget;
			Button button = (Button)p_data[0];
			MEStateModel state = controller.editor.model.state;
			string text = (button ? button.name : p_target.name);
			p_event.Contains("@click");
			p_event.Contains("@change");
			p_event.Contains("@end-edit");
			bool flag = state.AllowSceneControls && state.metric.showRulers;
			switch (text)
			{
			case "close":
				if ((bool)mERulersMetricWidget.anchor)
				{
					mERulersMetricWidget.anchor.attribs = mERulersMetricWidget.anchor.attribs & (MDEntityAttribFlag)(-2);
					if (flag)
					{
						SetLayerEnabled(flag);
					}
					controller.editor.ScheduleSave();
				}
				break;
			case "snap":
				BeginChange(text, mERulersMetricWidget);
				mERulersMetricWidget.Snap();
				EndChange(text, mERulersMetricWidget);
				mERulersMetricWidget.Refresh(p_force: true);
				controller.editor.view.handle.Refresh();
				break;
			case "mode":
				mERulersMetricWidget.useAbsolutePosition = !mERulersMetricWidget.useAbsolutePosition;
				mERulersMetricWidget.Refresh(p_force: true);
				break;
			}
		}

		public void SetLayerEnabled(bool p_flag)
		{
			bool flag = p_flag;
			_ = controller.editor.model.state;
			MESelectionModel selection = controller.editor.model.selection;
			MESceneView scene = controller.editor.view.scene;
			view.Fade(flag ? 1f : (-0.1f), 0.2f);
			if (!flag)
			{
				return;
			}
			view.Clear();
			anchors.Clear();
			targets.Clear();
			if (!selection.anyEntity)
			{
				return;
			}
			List<Transform> p_targets = selection.ConvertEntities<Transform>();
			List<MAEntity> list = scene.FindAll<MAEntity>(MDEntityAttribFlag.Ruler);
			List<MAEntity> entities = selection.entities;
			for (int i = 0; i < list.Count; i++)
			{
				if (entities.Contains(list[i]))
				{
					list.RemoveAt(i--);
				}
			}
			view.Set(list, p_targets);
			anchors = list;
			targets = p_targets;
		}

		public void BeginChange(string p_field, Object p_target)
		{
			Notify("map-editor.control.begin-change", p_target, p_field);
		}

		public void EndChange(string p_field, Object p_target)
		{
			Notify("map-editor.control.end-change", p_target, p_field);
		}
	}
}
