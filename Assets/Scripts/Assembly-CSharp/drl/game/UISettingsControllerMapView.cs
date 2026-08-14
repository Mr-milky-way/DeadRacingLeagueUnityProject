using System.Collections.Generic;
using UnityEngine;
using drl.sim;

namespace drl.game
{
	public class UISettingsControllerMapView : UIScreenView
	{
		public List<ControllerTypeTag> controllerTags;

		public UIControllerOverlay controllerOverlay;

		public void EnableControllerOverlay(bool enable)
		{
			if (!(controllerOverlay == null))
			{
				controllerOverlay.gameObject.SetActive(enable);
			}
		}

		public void Set(ControllerStateType p_type)
		{
			for (int i = 0; i < controllerTags.Count; i++)
			{
				ControllerTypeTag controllerTypeTag = controllerTags[i];
				if ((bool)controllerTypeTag)
				{
					ControllerStateType controllerStateType = controllerTypeTag.tags[0];
					controllerTypeTag.gameObject.SetActive(controllerStateType == p_type);
				}
			}
			controllerOverlay.SetController(p_type);
			Debug.Log("UISettingsControllerMapView> Set - [" + p_type.ToString() + "]");
		}
	}
}
