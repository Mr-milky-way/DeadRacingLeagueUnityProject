using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIDroneSelectionController : Controller<DRLApp>
	{
		public UIDroneSelectionView view => AssertLocal<UIDroneSelectionView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					List<DRLDroneRig> drones = base.app.model.storage.GetDrones();
					view.Set(drones);
					UINavigation.Link(view.listField.GetComponent<LayoutGroup>(), view.leftNavigation);
				}
				break;
			case "fly.drone-selection.card@focus":
			{
				UICardButtonDroneRig uICardButtonDroneRig = p_target as UICardButtonDroneRig;
				view.Set(uICardButtonDroneRig.asset);
				break;
			}
			case "fly.drone-selection.card@click":
				if (!(p_target as UICardButtonDroneRig).asset.rigFile)
				{
					Debug.LogWarning("UIDroneSelectionController> Null Rig File!");
					base.app.view.audio.PlayUIGenericError();
				}
				else
				{
					base.app.view.audio.PlayUIGenericSuccess();
					base.app.view.ui.screens.Return();
				}
				break;
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			case "ui.screen.return@focus":
				break;
			}
		}
	}
}
