using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MESelectionController : Controller<DRLApp>
	{
		public MapEditorController editor => AssertParent<MapEditorController>("editor");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "map-editor.selection.assets@add":
			{
				List<MapAsset> p_items = Reflection<object>.Get<List<MapAsset>>(p_data, 0);
				editor.model.state.preview.Create(p_items);
				break;
			}
			case "map-editor.selection.assets@remove":
				Reflection<object>.Get<List<MapAsset>>(p_data, 0);
				break;
			case "map-editor.selection.entities@add":
			{
				List<MAEntity> list2 = Reflection<object>.Get<List<MAEntity>>(p_data, 0);
				editor.view.camera.SetSelection(p_flag: true, list2.ToArray());
				editor.view.camera.SetHilight(false);
				editor.RefreshActionHandle();
				break;
			}
			case "map-editor.selection.entities@remove":
			{
				List<MAEntity> list = Reflection<object>.Get<List<MAEntity>>(p_data, 0);
				editor.view.camera.SetSelection(p_flag: false, list.ToArray());
				editor.view.camera.SetHilight(false);
				break;
			}
			}
		}

		public void SelectObjects(List<MapAsset> p_list)
		{
		}

		public void UnSelectObjects(List<MapAsset> p_list)
		{
		}
	}
}
