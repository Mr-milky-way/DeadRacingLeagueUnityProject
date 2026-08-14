using UnityEngine;
using drl.sim.rci;
using thelab.mvc;

namespace drl.game
{
	public class UIChannelSelectionController : Controller<DRLApp>
	{
		public UIChannelSelectionView view => AssertLocal<UIChannelSelectionView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "calibration.channel-selection.dropdown@change":
				if (p_data.Length != 0)
				{
					OnChannelSelectionDropdownChange(p_data[0] as UIChannelItemView);
				}
				break;
			case "calibration.channel-selection.next@click":
			{
				CalibrationData calibrationData = view.SaveChannels();
				Notify("input.manual-calibration-panel.open", calibrationData);
				if (calibrationData != null)
				{
					Notify("calibration.axis.invert", calibrationData);
				}
				calibrationData = null;
				view.list.Clear();
				break;
			}
			case "calibration.channel-selection.save@click":
			{
				CalibrationData calibrationData2 = view.SaveChannels(update: true);
				if (calibrationData2 != null)
				{
					Notify("calibration.axis.invert", calibrationData2);
				}
				calibrationData2 = null;
				Notify("input.calibration-menu-panel.open@click");
				break;
			}
			case "input.calibration-menu-panel.open@click":
				view.list.Clear();
				break;
			case "calibration.channel-selection.invert@change":
				if (p_data.Length != 0)
				{
					UIChannelItemView item2 = (UIChannelItemView)p_data[0];
					OnInvertToggle(item2);
				}
				break;
			case "calibration.channel-selection.midstick@change":
				if (p_data.Length != 0)
				{
					UIChannelItemView item = (UIChannelItemView)p_data[0];
					OnMidStickToggle(item);
				}
				break;
			}
		}

		private void OnMidStickToggle(UIChannelItemView item)
		{
			if (view.data != null && item.axisSelection.index == 1)
			{
				view.data.ZeroThrottle = (item.midStickToggle.isOn ? 0f : (-1f));
				Notify("calibration.axis.invert", view.data);
			}
		}

		private void OnInvertToggle(UIChannelItemView item)
		{
			if (view.data != null && item.axisSelection.index != 0)
			{
				view.data.Invert[GetAxisForItem(item)] = item.invertToggle.isOn;
				Notify("calibration.axis.invert", view.data);
			}
		}

		private void Update()
		{
			if (!view.initialized)
			{
				return;
			}
			foreach (UIChannelItemView channelItem in view.channelItems)
			{
				float num = 0f;
				if (channelItem.channel >= 0)
				{
					if (channelItem.channel < RCI.GetAxisCount())
					{
						num = RCI.GetRawFromIndex(channelItem.channel);
						num = (ChannelInverted(channelItem) ? (0f - num) : num);
					}
					else
					{
						num = (RCI.GetButtonRawIndex(channelItem.channel) ? 1f : (-1f));
					}
				}
				if (num > 0f)
				{
					channelItem.leftBar.fillAmount = num;
					channelItem.rightBar.fillAmount = 0f;
				}
				else
				{
					channelItem.leftBar.fillAmount = 0f;
					channelItem.rightBar.fillAmount = Mathf.Abs(num);
				}
			}
		}

		private void OnChannelSelectionDropdownChange(UIChannelItemView p_item)
		{
			int num = 0;
			foreach (UIChannelItemView channelItem in view.channelItems)
			{
				if (channelItem.channel != p_item.channel && channelItem.axisSelection.index == p_item.axisSelection.index)
				{
					channelItem.axisSelection.Select(0);
					SetInvert(channelItem);
					SetMidStick(channelItem);
				}
			}
			if (p_item.axisSelection.index > 0 && p_item.axisSelection.index < 5)
			{
				SetInvert(p_item, p_active: true);
				p_item.invertToggle.isOn = ChannelInverted(p_item);
			}
			if (p_item.axisSelection.index == 1)
			{
				SetMidStick(p_item, p_active: true, IsMidStick());
			}
			p_item.SetDetected(p_item.axisSelection.index != 0);
			if (p_item.axisSelection.index == 0)
			{
				SetInvert(p_item);
				SetMidStick(p_item);
			}
			if (view.data != null)
			{
				bool[] array = new bool[4];
				foreach (UIChannelItemView channelItem2 in view.channelItems)
				{
					switch (channelItem2.axisSelection.index)
					{
					case 1:
						view.data.ElementIDs[RawAxis.LeftStickY] = channelItem2.channel;
						array[0] = true;
						num++;
						break;
					case 2:
						view.data.ElementIDs[RawAxis.LeftStickX] = channelItem2.channel;
						array[1] = true;
						num++;
						break;
					case 3:
						view.data.ElementIDs[RawAxis.RightStickY] = channelItem2.channel;
						array[2] = true;
						num++;
						break;
					case 4:
						view.data.ElementIDs[RawAxis.RightStickX] = channelItem2.channel;
						array[3] = true;
						num++;
						break;
					}
				}
				if (!array[0])
				{
					view.data.ElementIDs[RawAxis.LeftStickY] = -1;
				}
				if (!array[1])
				{
					view.data.ElementIDs[RawAxis.LeftStickX] = -1;
				}
				if (!array[2])
				{
					view.data.ElementIDs[RawAxis.RightStickY] = -1;
				}
				if (!array[3])
				{
					view.data.ElementIDs[RawAxis.RightStickX] = -1;
				}
				Notify("calibration.axis.invert", view.data);
			}
			Notify("calibration.channel-selection.complete", num == 4, view.calibrationInProgress);
		}

		public bool ChannelInverted(RawAxis p_axis)
		{
			if (view.data == null || !view.data.Invert.ContainsKey(p_axis))
			{
				return false;
			}
			return view.data.Invert[p_axis];
		}

		public bool ChannelInverted(UIChannelItemView p_item)
		{
			return ChannelInverted(GetAxisForItem(p_item));
		}

		private RawAxis GetAxisForItem(UIChannelItemView p_item)
		{
			if (p_item.axisSelection.index == 0)
			{
				return (RawAxis)0;
			}
			return p_item.axisSelection.index switch
			{
				1 => RawAxis.LeftStickY, 
				2 => RawAxis.LeftStickX, 
				3 => RawAxis.RightStickY, 
				4 => RawAxis.RightStickX, 
				5 => RawAxis.ToggleA, 
				6 => RawAxis.ToggleB, 
				_ => (RawAxis)0, 
			};
		}

		private bool IsMidStick()
		{
			if (view.data == null)
			{
				return false;
			}
			return Mathf.Abs(view.data.ZeroThrottle) < 0.05f;
		}

		private void SetInvert(UIChannelItemView p_item, bool p_active = false)
		{
			if (p_item.isButton)
			{
				p_active = false;
			}
			p_item.invertToggle.isOn = false;
			p_item.invertToggle.interactable = p_active;
			p_item.invertToggleView.interactable = p_active;
		}

		private void SetMidStick(UIChannelItemView p_item, bool p_active = false, bool is_midstick = false)
		{
			if (p_item.isButton)
			{
				p_active = false;
			}
			p_item.midStickToggle.isOn = is_midstick;
			p_item.midStickToggle.interactable = p_active;
			p_item.midStickToggleView.interactable = p_active;
		}
	}
}
