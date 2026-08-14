using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIDroneSelectionView : UIScreenView
	{
		public ListComponent listField;

		public RawImage droneCardBackgroundField;

		public Text droneCardNameField;

		public RectTransform droneLeaderboardWarningRT;

		public void ClearDrones()
		{
			if ((bool)listField)
			{
				listField.Clear();
			}
		}

		public void Set(DRLDroneRig p_drone)
		{
			droneCardBackgroundField.texture = p_drone.image;
			droneCardBackgroundField.enabled = p_drone.image != null;
			droneCardNameField.text = p_drone.label.ToUpper();
			droneLeaderboardWarningRT.gameObject.SetActive(!p_drone.allowLeaderboard);
		}

		public void Set(List<DRLDroneRig> p_drones)
		{
			ClearDrones();
			for (int i = 0; i < p_drones.Count; i++)
			{
				AddDrone(p_drones[i]);
			}
		}

		public void AddDrone(DRLDroneRig p_item)
		{
			if ((bool)listField && (bool)p_item)
			{
				UICardButtonDroneRig uICardButtonDroneRig = listField.Push<UICardButtonDroneRig>();
				_ = listField.Count;
				uICardButtonDroneRig.notification = "fly.drone-selection.card";
				uICardButtonDroneRig.Set(p_item);
			}
		}
	}
}
