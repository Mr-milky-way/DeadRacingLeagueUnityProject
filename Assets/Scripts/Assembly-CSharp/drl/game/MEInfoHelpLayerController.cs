using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class MEInfoHelpLayerController : Controller<DRLApp>
	{
		public UIMapEditorController controller => AssertFindReverse<UIMapEditorController>();

		public MEInfoHelpLayer view => AssertLocal<MEInfoHelpLayer>("view");

		public MEInfoHelpLayerModel model => AssertLocal<MEInfoHelpLayerModel>("model");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!(base.app.view.ui.screens.current != controller.view.screen) && p_event != null)
			{
				switch (p_event)
				{
				case "map-editor.ready":
					view.Initialize();
					break;
				case "map-editor.selection.assets@change":
					view.SetDirty();
					break;
				case "map-editor.selection.assets@add":
					view.SetDirty();
					break;
				case "map-editor.selection.entities@add":
					view.SetDirty();
					break;
				case "map-editor.action.state.change":
					view.SetDirty();
					break;
				case "map-editor.input.state.change":
					view.SetDirty();
					break;
				case "map-editor.render.state.change":
					view.SetDirty();
					break;
				case "map-editor.handle@start":
					view.SetDirty();
					break;
				case "map-editor.handle@drag-end":
					view.SetDirty();
					break;
				}
			}
		}
	}
}
