using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMapEditorTemplatesController : Controller<DRLApp>
	{
		public UIMapEditorTemplatesView view => AssertLocal<UIMapEditorTemplatesView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
			{
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				List<DRLMap> maps = base.app.model.storage.GetMaps(true);
				maps = base.app.model.storage.Filter(maps, GameFlag.MapEditor);
				if (!base.app.model.storage.state.player.profile.isDeveloper)
				{
					maps.RemoveAll((DRLMap it) => it.tags.Contains(GameFlag.MapEditorDevOnly));
				}
				view.Set(maps);
				break;
			}
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			case "map-editor.templates-card@click":
			{
				DRLMap data = ((UICardButtonMap)p_target).data;
				base.app.controller.LoadMapEditor(data, null, view.gameMode);
				base.enabled = false;
				break;
			}
			}
		}
	}
}
