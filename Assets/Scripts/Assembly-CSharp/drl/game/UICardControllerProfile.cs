using System.Collections.Generic;
using UnityEngine.UI;
using drl.sim;
using drl.sim.rci;
using thelab.core;

namespace drl.game
{
	public class UICardControllerProfile : UICardView
	{
		public Text title0Field;

		public Image grayMarker;

		public Image greenMarker;

		public Image greenOutline;

		public List<ControllerTypeTag> controllerTypeList;

		public new RCDeviceData data;

		public override UICardType type => UICardType.ButtonControllerProfile;

		public override bool selected
		{
			get
			{
				if (!greenOutline)
				{
					return false;
				}
				return greenOutline.gameObject.activeInHierarchy;
			}
			set
			{
				if ((bool)greenOutline)
				{
					grayMarker.gameObject.SetActive(!value);
					greenMarker.gameObject.SetActive(value);
					greenOutline.gameObject.SetActive(value);
				}
			}
		}

		public string title0
		{
			get
			{
				return UIReflection.Get<string>(title0Field);
			}
			set
			{
				UIReflection.Set(title0Field, value);
			}
		}

		public ControllerStateType controllerState
		{
			set
			{
				for (int i = 0; i < controllerTypeList.Count; i++)
				{
					ControllerTypeTag controllerTypeTag = controllerTypeList[i];
					if ((bool)controllerTypeTag && controllerTypeTag.tags.Count > 0)
					{
						controllerTypeTag.gameObject.SetActive(controllerTypeTag.tags[0] == value);
					}
				}
			}
		}

		public void Set(int p_id, RCDeviceData p_data)
		{
			if (p_data != null)
			{
				title0 = base.app.model.storage.locale.Get("controller-profiles-menu.controller", "CONTROLLER") + " " + p_id.ToString("00");
				controllerState = RCI.GetControllerStateType(ControllerStateType.Taranis, p_data);
				data = p_data;
			}
		}

		public override void Build()
		{
			base.Build();
		}
	}
}
