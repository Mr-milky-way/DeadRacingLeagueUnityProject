using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using drl.sim.rci;
using thelab.core;

namespace drl.game
{
	public class UIChannelSelectionView : UIScreenView
	{
		public ListComponent list;

		[HideInInspector]
		public CalibrationData data;

		public GameObject warningMessage;

		public new UINavigation leftNavigation;

		public new UINavigationLinkList rightNavigation;

		[HideInInspector]
		public List<UIChannelItemView> channelItems = new List<UIChannelItemView>();

		private Dictionary<RawAxis, int> IDs = new Dictionary<RawAxis, int>
		{
			{
				RawAxis.LeftStickX,
				-1
			},
			{
				RawAxis.LeftStickY,
				-1
			},
			{
				RawAxis.RightStickX,
				-1
			},
			{
				RawAxis.RightStickY,
				-1
			},
			{
				RawAxis.ToggleA,
				-2
			},
			{
				RawAxis.ToggleB,
				-2
			}
		};

		public bool initialized { get; set; }

		public bool calibrationInProgress { get; set; }

		public void Setup(CalibrationData p_data)
		{
			if (p_data == null && !RCI.HasSavedProfile())
			{
				return;
			}
			data = p_data;
			this.list.Clear();
			int[] array = new int[4];
			int axisCount = RCI.GetAxisCount();
			warningMessage.gameObject.SetActive(axisCount == 0);
			if (axisCount == 0)
			{
				return;
			}
			for (int i = 0; i < axisCount; i++)
			{
				this.list.Push<UIChannelItemView>().Set(i);
			}
			if (data == null && RCI.HasSavedProfile())
			{
				calibrationInProgress = false;
				RCDeviceData savedProfile = RCI.GetSavedProfile();
				array = new int[4]
				{
					savedProfile.GetAAD(RawAxis.LeftStickY).ElementID,
					savedProfile.GetAAD(RawAxis.LeftStickX).ElementID,
					savedProfile.GetAAD(RawAxis.RightStickY).ElementID,
					savedProfile.GetAAD(RawAxis.RightStickX).ElementID
				};
				if (data == null)
				{
					data = GenerateData(savedProfile);
				}
				bool flag = true;
				AssignedAxisData[] assignedAxisData = savedProfile.assignedAxisData;
				foreach (AssignedAxisData it in assignedAxisData)
				{
					if (it.ElementID >= 0 && it.rawAxis != RawAxis.ToggleA && it.rawAxis != RawAxis.ToggleB)
					{
						int num = 0;
						switch (it.rawAxis)
						{
						case RawAxis.LeftStickY:
							num = 1;
							break;
						case RawAxis.LeftStickX:
							num = 2;
							break;
						case RawAxis.RightStickY:
							num = 3;
							break;
						case RawAxis.RightStickX:
							num = 4;
							break;
						}
						if (num == 0)
						{
							flag = false;
						}
						UIChannelItemView uIChannelItemView = this.list.GetList<UIChannelItemView>().Find((UIChannelItemView o) => o.channel == it.ElementID);
						if (uIChannelItemView != null)
						{
							uIChannelItemView.axisSelection.Select(num);
							data.ElementIDs[it.rawAxis] = it.ElementID;
							data.Invert[it.rawAxis] = it.inverted;
							uIChannelItemView.midStickToggleView.interactable = num == 1;
							uIChannelItemView.midStickToggle.interactable = num == 1;
							uIChannelItemView.midStickToggle.isOn = num == 1 && Mathf.Abs(it.zeroThrottle) < 0.05f;
							uIChannelItemView.invertToggle.interactable = num != 0;
							uIChannelItemView.invertToggleView.interactable = num != 0;
							uIChannelItemView.invertToggle.isOn = num != 0 && it.inverted;
						}
					}
				}
				Notify("calibration.channel-selection.complete", flag, calibrationInProgress);
				Notify("calibration.axis.invert", data);
			}
			else if (data != null)
			{
				calibrationInProgress = true;
				array = data.ActiveChannels;
			}
			List<UIChannelItemView> list = this.list.GetList<UIChannelItemView>();
			if (data != null)
			{
				int toggleAID = data.ElementIDs[RawAxis.ToggleA];
				int toggleBID = data.ElementIDs[RawAxis.ToggleB];
				foreach (UIChannelItemView item in list)
				{
					if (item.axisSelection.index >= 5)
					{
						item.axisSelection.Select(0);
						item.preDetected = false;
					}
				}
				if (toggleAID >= 0 && toggleAID < axisCount)
				{
					UIChannelItemView uIChannelItemView3 = this.list.GetList<UIChannelItemView>().Find((UIChannelItemView o) => o.channel == toggleAID);
					if ((bool)uIChannelItemView3)
					{
						uIChannelItemView3.axisSelection.Select(5);
					}
				}
				if (toggleBID >= 0 && toggleBID < axisCount)
				{
					UIChannelItemView uIChannelItemView4 = this.list.GetList<UIChannelItemView>().Find((UIChannelItemView o) => o.channel == toggleBID);
					if ((bool)uIChannelItemView4)
					{
						uIChannelItemView4.axisSelection.Select(6);
					}
				}
				if (toggleAID >= axisCount)
				{
					UIChannelItemView uIChannelItemView5 = this.list.Push<UIChannelItemView>();
					uIChannelItemView5.Set(data.ElementIDs[RawAxis.ToggleA], p_button: true);
					uIChannelItemView5.axisSelection.Select(5);
					uIChannelItemView5.invertToggle.isOn = false;
					uIChannelItemView5.invertToggleView.interactable = false;
					uIChannelItemView5.invertToggle.interactable = false;
				}
				if (toggleBID >= axisCount)
				{
					UIChannelItemView uIChannelItemView6 = this.list.Push<UIChannelItemView>();
					uIChannelItemView6.Set(data.ElementIDs[RawAxis.ToggleB], p_button: true);
					uIChannelItemView6.axisSelection.Select(6);
					uIChannelItemView6.invertToggle.isOn = false;
					uIChannelItemView6.invertToggleView.interactable = false;
					uIChannelItemView6.invertToggle.interactable = false;
				}
				if ((toggleAID == -1 || toggleBID == -1) && RCI.IsRCController())
				{
					for (int num2 = RCI.GetAxisCount(); num2 < RCI.GetAxisCount() + RCI.GetButtonCount(); num2++)
					{
						UIChannelItemView uIChannelItemView7 = this.list.Push<UIChannelItemView>();
						uIChannelItemView7.Set(num2, p_button: true);
						uIChannelItemView7.axisSelection.Select(0);
						uIChannelItemView7.invertToggle.isOn = false;
						uIChannelItemView7.invertToggleView.interactable = false;
						uIChannelItemView7.invertToggle.interactable = false;
					}
				}
			}
			foreach (UIChannelItemView item2 in list)
			{
				item2.SetDetected(p_detected: false);
				item2.preDetected = false;
				if (calibrationInProgress && item2.axisSelection.index < 5)
				{
					item2.axisSelection.Select(0);
				}
				for (int num3 = 0; num3 < array.Length; num3++)
				{
					if (array[num3] == item2.channel || item2.axisSelection.index > 4)
					{
						item2.preDetected = true;
					}
				}
				item2.SetDetected(item2.preDetected);
			}
			this.list.Sort(SortChannels);
			initialized = true;
			channelItems.Clear();
			channelItems = this.list.GetList<UIChannelItemView>();
			SetupNavigation();
		}

		private int SortChannels(Component x, Component y)
		{
			UIChannelItemView component = x.GetComponent<UIChannelItemView>();
			UIChannelItemView component2 = y.GetComponent<UIChannelItemView>();
			bool preDetected = component.preDetected;
			bool preDetected2 = component2.preDetected;
			int num = preDetected2.CompareTo(preDetected);
			int result = component.channel.CompareTo(component2.channel);
			if (num != 0)
			{
				return num;
			}
			return result;
		}

		private CalibrationData GenerateData(RCDeviceData rcd)
		{
			CalibrationData calibrationData = new CalibrationData();
			calibrationData.Centers = new float[RCI.GetAxisCount()];
			foreach (RawAxis item in (IEnumerable<RawAxis>)calibrationData.ElementIDs.Keys.ToList())
			{
				AssignedAxisData aAD = rcd.GetAAD(item);
				calibrationData.ElementIDs[item] = aAD.ElementID;
				calibrationData.Invert[item] = aAD.inverted;
				calibrationData.RangeMax[item] = aAD.max;
				calibrationData.RangeMin[item] = aAD.min;
				calibrationData.Deadzone[item] = aAD.deadzone;
				if (item == RawAxis.LeftStickY)
				{
					calibrationData.ZeroThrottle = aAD.zeroThrottle;
				}
				if (aAD.ElementID >= 0 && aAD.ElementID < calibrationData.Centers.Length)
				{
					calibrationData.Centers[aAD.ElementID] = aAD.center;
				}
			}
			return calibrationData;
		}

		public CalibrationData SaveChannels(bool update = false)
		{
			if (data == null)
			{
				return null;
			}
			foreach (UIChannelItemView item in list.GetList<UIChannelItemView>())
			{
				if (item.axisSelection.index != 0)
				{
					switch (item.axisSelection.index)
					{
					case 1:
						data.ElementIDs[RawAxis.LeftStickY] = item.channel;
						break;
					case 2:
						data.ElementIDs[RawAxis.LeftStickX] = item.channel;
						break;
					case 3:
						data.ElementIDs[RawAxis.RightStickY] = item.channel;
						break;
					case 4:
						data.ElementIDs[RawAxis.RightStickX] = item.channel;
						break;
					case 5:
						data.ElementIDs[RawAxis.ToggleA] = item.channel;
						break;
					case 6:
						data.ElementIDs[RawAxis.ToggleB] = item.channel;
						break;
					}
				}
			}
			if (update)
			{
				RCI.SetActiveControllerFromIndex(data);
			}
			else
			{
				foreach (KeyValuePair<RawAxis, int> elementID in data.ElementIDs)
				{
					if (data.ChannelRange.ContainsKey(elementID.Value))
					{
						data.RangeMin[elementID.Key] = data.ChannelRange[elementID.Value].Item2;
						data.RangeMax[elementID.Key] = data.ChannelRange[elementID.Value].Item1;
					}
				}
			}
			initialized = false;
			return data;
		}

		private void SetupNavigation()
		{
			UINavigation uINavigation = leftNavigation;
			UINavigation uINavigation2 = null;
			Component[] links = rightNavigation.links;
			foreach (Component component in links)
			{
				if (component.gameObject.activeInHierarchy)
				{
					uINavigation2 = component.GetComponent<UINavigation>();
					break;
				}
			}
			for (int j = 0; j < channelItems.Count; j++)
			{
				if (uINavigation != null)
				{
					channelItems[j].dropdownNavigation.left = uINavigation;
					if (j == 0)
					{
						uINavigation.right = channelItems[j].dropdownNavigation;
					}
				}
				if (uINavigation2 != null)
				{
					channelItems[j].invertNavigation.right = uINavigation2;
					if (j == 0)
					{
						uINavigation2.left = channelItems[j].invertNavigation;
					}
				}
				if (j > 0)
				{
					channelItems[j].dropdownNavigation.up = channelItems[j - 1].dropdownNavigation;
					channelItems[j].midStickNavigation.up = channelItems[j - 1].midStickNavigation;
					channelItems[j].invertNavigation.up = channelItems[j - 1].invertNavigation;
				}
				if (j < channelItems.Count - 1)
				{
					channelItems[j].dropdownNavigation.down = channelItems[j + 1].dropdownNavigation;
					channelItems[j].midStickNavigation.down = channelItems[j + 1].midStickNavigation;
					channelItems[j].invertNavigation.down = channelItems[j + 1].invertNavigation;
					channelItems[j].axisSelection.downNavigation = channelItems[j + 1].dropdownNavigation;
				}
			}
		}
	}
}
