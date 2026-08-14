using System.Collections.Generic;
using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class MEInspectorPanelController : Controller<DRLApp>
	{
		public MapEditorController editor;

		public MEInspectorPanelView view => AssertLocal<MEInspectorPanelView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "map-editor.entity.delete":
			case "map-editor.selection.entities@change":
			{
				List<MAEntity> entities = editor.model.selection.entities;
				view.SetTargets(entities);
				break;
			}
			case "map-editor.inspector.dirty":
				editor.view.handle.Refresh();
				break;
			}
			if ((bool)view.current)
			{
				view.current.OnNotification(p_event, p_target, p_data);
			}
		}
	}
}
