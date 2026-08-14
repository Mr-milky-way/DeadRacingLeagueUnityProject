using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MEControlLayoutLayerController : Controller<DRLApp>
	{
		public List<MARenderer> anchors;

		public List<MARenderer> targets;

		public UIMapEditorController ui => AssertFindReverse<UIMapEditorController>();

		public MEControlLayoutLayer view => AssertLocal<MEControlLayoutLayer>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != ui.view.screen)
			{
				return;
			}
			MEStateModel state = ui.editor.model.state;
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "map-editor.controls.layout.state.change":
			{
				bool layerEnabled = (bool)p_data[0];
				if (!state.AllowLayoutTools)
				{
					layerEnabled = false;
				}
				SetLayerEnabled(layerEnabled);
				break;
			}
			case "map-editor.graph.layout.widget.event@click":
			case "map-editor.graph.layout.widget.event@change":
			case "map-editor.graph.layout.widget.event@end-edit":
			case "map-editor.graph.layout.widget.event":
				OnWidgetFormEvent(p_event, p_target, p_data);
				break;
			case "map-editor.entity.delete":
			case "map-editor.inspector.end-change":
			case "map-editor.render.state.change":
			case "map-editor.selection.assets@change":
			case "map-editor.selection.entities@change":
			{
				bool allowLayoutTools = state.AllowLayoutTools;
				SetLayerEnabled(allowLayoutTools);
				break;
			}
			}
		}

		protected void OnWidgetFormEvent(string p_event, Object p_target, params object[] p_data)
		{
			MELayoutWidget mELayoutWidget = p_target as MELayoutWidget;
			Component component = p_target as Component;
			if (!mELayoutWidget)
			{
				mELayoutWidget = Hierarchy.FindReverse<MELayoutWidget>(component.transform);
			}
			if (!mELayoutWidget)
			{
				Debug.LogWarning("MEControlLayoutLayerController> OnWidgetFormEvent / Failed to find the widget");
				return;
			}
			MEStateModel state = ui.editor.model.state;
			bool num = p_event.Contains("@click");
			p_event.Contains("@change");
			p_event.Contains("@end-edit");
			_ = state.AllowSceneControls;
			Button button = ((!num) ? null : ((p_data.Length == 0) ? null : ((Button)p_data[0])));
			string text = (button ? button.name : p_target.name);
			bool flag = false;
			bool p_rebuild = false;
			switch (text)
			{
			case "close":
				if ((bool)mELayoutWidget.anchor)
				{
					mELayoutWidget.anchor.isLayout = false;
					SetLayerEnabled(p_flag: false);
					ui.editor.ScheduleSave();
				}
				break;
			case "preview":
			{
				flag = true;
				p_rebuild = true;
				MELayoutSurface mELayoutSurface = (mELayoutWidget.anchor ? mELayoutWidget.anchor.GetComponent<MELayoutSurface>() : null);
				if ((bool)mELayoutSurface)
				{
					mELayoutSurface.Randomize();
				}
				break;
			}
			case "apply":
			{
				flag = true;
				List<MAEntity> p_targets = new List<MAEntity>(mELayoutWidget.instances);
				if ((bool)mELayoutWidget.surface)
				{
					ui.editor.ApplyClone(p_targets, null, p_force_parent: true, p_force_selection: false);
				}
				break;
			}
			case "layout-pattern-wrap":
			case "layout-pattern":
			case "layout-instance-count":
				flag = true;
				p_rebuild = true;
				break;
			case "layout-sizes":
				flag = true;
				break;
			case "layout-offset":
				flag = true;
				break;
			case "layout-dither-position":
				flag = true;
				break;
			case "layout-orient-offset":
				flag = true;
				break;
			case "layout-orient-step":
				flag = true;
				break;
			case "layout-orient-toggle":
				flag = true;
				break;
			case "layout-margins":
				flag = true;
				break;
			case "layout-spacing":
			{
				flag = true;
				DRLNumberFieldView dRLNumberFieldView = p_target as DRLNumberFieldView;
				if (Mathf.Abs(dRLNumberFieldView.value) <= 0.001f)
				{
					dRLNumberFieldView.input.text = "";
				}
				break;
			}
			}
			if (flag)
			{
				mELayoutWidget.Generate(p_rebuild);
			}
		}

		public void SetLayerEnabled(bool p_flag)
		{
			bool flag = p_flag;
			_ = ui.editor.model.state;
			MESelectionModel selection = ui.editor.model.selection;
			MESceneView scene = ui.editor.view.scene;
			view.Fade(flag ? 1f : (-0.1f), 0.2f);
			if (!flag)
			{
				return;
			}
			view.Clear();
			anchors.Clear();
			targets.Clear();
			if (!selection.anyEntity && !selection.anyAsset)
			{
				return;
			}
			List<MARenderer> list = new List<MARenderer>();
			List<MARenderer> rl = scene.FindAll<MARenderer>(FilterIsLayout);
			if (rl.Count > 0)
			{
				list.AddRange(rl);
				List<MARenderer> list2 = new List<MARenderer>();
				list2.AddRange(selection.ConvertEntities<MARenderer>());
				if (list2.Count <= 0)
				{
					list2.AddRange(selection.ConvertAssets<MARenderer>());
				}
				list2.RemoveAll((MARenderer it) => it is MAGuide || rl.Contains(it));
				if (list2.Count > 0)
				{
					view.Set(list, list2);
					anchors = list;
					targets = list2;
				}
			}
		}

		private bool FilterIsLayout(MARenderer it)
		{
			bool result = it.isLayout;
			if (it is MASpline && (it as MASpline).transform.childCount <= 0)
			{
				result = false;
			}
			if (it is MASplineControlPoint)
			{
				MASplineControlPoint mASplineControlPoint = it as MASplineControlPoint;
				if ((bool)mASplineControlPoint.spline && mASplineControlPoint.spline.transform.childCount <= 0)
				{
					result = false;
				}
			}
			return result;
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
