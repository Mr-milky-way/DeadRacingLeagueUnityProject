using System;
using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UICommunityDronesView : UIScreenView
	{
		public ListComponent listField;

		public DRLStepperView showStepper;

		public DRLStepperView sizeStepper;

		public DRLStepperView physicsStepper;

		public DRLStepperView sortStepper;

		public DRLStepperView ratingStepper;

		public DRLInputFieldView searchInput;

		public FadeComponent feedbackFade;

		public FadeComponent listFade;

		public List<GameObject> feedbacks;

		public DRLPagePickerView pageField;

		public UINavigation newDroneButton;

		public UINavigation backButton;

		public bool inGame;

		public UICommunityDronesShowCriteria showCriteria;

		public List<DRLCommunityDroneData> drones;

		public bool showCreateButton
		{
			get
			{
				if (newDroneButton != null)
				{
					return newDroneButton.gameObject.activeInHierarchy;
				}
				return false;
			}
			set
			{
				if (newDroneButton != null)
				{
					newDroneButton.gameObject.SetActive(value);
				}
			}
		}

		public bool showMyDrones
		{
			get
			{
				return showCriteria == UICommunityDronesShowCriteria.MyDrones;
			}
			set
			{
				showCriteria = (value ? UICommunityDronesShowCriteria.MyDrones : UICommunityDronesShowCriteria.CommunityDrones);
				if (showStepper != null)
				{
					showStepper.index = (value ? 1 : 0);
					showStepper.Refresh();
				}
			}
		}

		public void Clear()
		{
			listField.Clear();
			drones = new List<DRLCommunityDroneData>();
		}

		public void UpdateList(List<DRLCommunityDroneData> p_drones, int p_page, int p_page_length, int p_pages_count = -1, bool p_allow_search = false)
		{
			List<DRLCommunityDroneData> collection = ((p_drones == null) ? new List<DRLCommunityDroneData>() : p_drones);
			collection = new List<DRLCommunityDroneData>(collection);
			if (p_allow_search)
			{
				collection.RemoveAll(delegate(DRLCommunityDroneData p_it)
				{
					string text = searchInput.field.text;
					text = text.Trim().ToLower();
					return !string.IsNullOrEmpty(text) && !p_it.droneName.ToLower().Contains(text);
				});
			}
			int num = ((p_page_length > 0) ? ((collection.Count - 1) / p_page_length) : 0) + 1;
			if (p_pages_count > 0)
			{
				num = p_pages_count;
			}
			int num2 = Mathf.Clamp(p_page, 0, num - 1);
			List<DRLCommunityDroneData> list = new List<DRLCommunityDroneData>();
			int num3 = ((collection.Count > p_page_length) ? Mathf.Max(0, num2 * p_page_length) : 0);
			for (int num4 = 0; num4 < p_page_length; num4++)
			{
				if (num3 >= collection.Count)
				{
					break;
				}
				DRLCommunityDroneData item = collection[num3];
				list.Add(item);
				num3++;
			}
			List<DRLCommunityDroneData> list2 = new List<DRLCommunityDroneData>();
			List<DRLCommunityDroneData> list3 = new List<DRLCommunityDroneData>();
			if (drones == null)
			{
				drones = new List<DRLCommunityDroneData>();
			}
			for (int num5 = 0; num5 < list.Count; num5++)
			{
				if (!ContainsDrone(drones, list[num5]))
				{
					list2.Add(list[num5]);
				}
			}
			for (int num6 = 0; num6 < drones.Count; num6++)
			{
				if (!ContainsDrone(list, drones[num6]))
				{
					list3.Add(drones[num6]);
				}
			}
			for (int num7 = 0; num7 < list3.Count; num7++)
			{
				RemoveDrone(list3[num7]);
			}
			for (int num8 = 0; num8 < list2.Count; num8++)
			{
				if (drones.Count < p_page_length)
				{
					AddDrone(list2[num8]);
				}
			}
			for (int num9 = 0; num9 < list.Count; num9++)
			{
				int droneIndex = GetDroneIndex(list[num9]);
				if (droneIndex >= 0)
				{
					drones[droneIndex] = list[num9];
				}
			}
			for (int num10 = 0; num10 < drones.Count; num10++)
			{
				UpdateDrone(drones[num10]);
			}
			FadeComponent fade = pageField.fade;
			if (fade.alpha < 0f)
			{
				fade.alpha = 0f;
			}
			if (num > 1)
			{
				fade.FadeIn(0.3f);
			}
			else
			{
				fade.FadeOut(0.3f);
			}
			UpdateNavigation(num);
			pageField.Set(num);
			pageField.index = num2;
			UICommunityDronesFeedbackType p_type = ((drones.Count <= 0) ? UICommunityDronesFeedbackType.NoDrones : UICommunityDronesFeedbackType.None);
			SetFeedback(p_type, p_hide_list: true, 0.1f);
		}

		protected void UpdateNavigation(int p_totalPages)
		{
			ListComponent listComponent = listField;
			List<UINavigation> entry_navs = new List<UINavigation>();
			List<UINavigation> fly_navs = new List<UINavigation>();
			List<UINavigation> add_navs = new List<UINavigation>();
			List<UINavigation> list = new List<UINavigation>();
			List<UINavigation> del_navs = new List<UINavigation>();
			List<UINavigation> list2 = new List<UINavigation>();
			List<UINavigation> list3 = new List<UINavigation>();
			List<UINavigation> list4 = new List<UINavigation>();
			UINavigation page_nav = ((p_totalPages > 1) ? pageField.GetComponent<UINavigation>() : null);
			if (p_totalPages > 1)
			{
				((Component)this).ActivityRun((Func<bool>)delegate
				{
					if (pageField.selection == null)
					{
						return true;
					}
					page_nav = pageField.selection.GetComponent<UINavigation>();
					int count2 = entry_navs.Count;
					if (count2 > 0)
					{
						entry_navs[count2 - 1].down = page_nav;
						fly_navs[count2 - 1].down = page_nav;
						add_navs[count2 - 1].down = page_nav;
						del_navs[count2 - 1].down = page_nav;
					}
					return false;
				}, 0f);
			}
			UINavigation component = searchInput.GetComponent<UINavigation>();
			UINavigation component2 = showStepper.GetComponent<UINavigation>();
			for (int num = 0; num < listComponent.Count; num++)
			{
				UICommunityDronesItemView uICommunityDronesItemView = listComponent.Get<UICommunityDronesItemView>(num);
				entry_navs.Add(uICommunityDronesItemView.dataGroup);
				fly_navs.Add(uICommunityDronesItemView.flyButton);
				add_navs.Add(uICommunityDronesItemView.saveButton);
				list.Add(uICommunityDronesItemView.cloneButton);
				del_navs.Add(uICommunityDronesItemView.deleteButton);
				list2.Add(uICommunityDronesItemView.editButton);
				list3.Add(uICommunityDronesItemView.buttonsTopProxy);
				list4.Add(uICommunityDronesItemView.buttonsBottomProxy);
			}
			int count = entry_navs.Count;
			for (int num2 = 0; num2 < count; num2++)
			{
				entry_navs[num2].up = ((num2 > 0) ? entry_navs[num2 - 1] : component2);
				entry_navs[num2].down = ((num2 < count - 1) ? entry_navs[num2 + 1] : page_nav);
				entry_navs[num2].left = backButton;
				fly_navs[num2].up = ((num2 > 0) ? fly_navs[num2 - 1] : component);
				fly_navs[num2].down = ((num2 < count - 1) ? fly_navs[num2 + 1] : page_nav);
				fly_navs[num2].right = newDroneButton;
				add_navs[num2].up = ((num2 > 0) ? list4[num2 - 1] : component);
				add_navs[num2].down = ((num2 < count - 1) ? list3[num2 + 1] : page_nav);
				list2[num2].up = ((num2 > 0) ? list4[num2 - 1] : component);
				del_navs[num2].down = ((num2 < count - 1) ? list3[num2 + 1] : page_nav);
			}
			backButton.right = ((count > 0) ? entry_navs[0] : component2);
			newDroneButton.left = ((count > 0) ? fly_navs[0] : component);
			if (page_nav != null)
			{
				page_nav.up = ((count > 0) ? entry_navs[count - 1] : component2);
			}
		}

		public void AddDrone(DRLCommunityDroneData p_data)
		{
			drones.Add(p_data);
			UICommunityDronesItemView uICommunityDronesItemView = listField.Push<UICommunityDronesItemView>();
			uICommunityDronesItemView.garage = base.app.model.storage.state.player.garage;
			uICommunityDronesItemView.Set(p_data, showCriteria == UICommunityDronesShowCriteria.MyDrones, base.app.model.storage.state.player.profile.playerId);
		}

		public void RemoveDrone(DRLCommunityDroneData p_data)
		{
			for (int i = 0; i < drones.Count; i++)
			{
				if (drones[i].guid == p_data.guid)
				{
					drones.RemoveAt(i);
					break;
				}
			}
			for (int j = 0; j < listField.Count; j++)
			{
				UICommunityDronesItemView uICommunityDronesItemView = listField.Get<UICommunityDronesItemView>(j);
				if ((bool)uICommunityDronesItemView && uICommunityDronesItemView.data.guid == p_data.guid)
				{
					listField.Remove(j);
					break;
				}
			}
		}

		public void UpdateDrone(DRLCommunityDroneData p_data)
		{
			UICommunityDronesItemView byDroneId = GetByDroneId(p_data.guid);
			if ((bool)byDroneId)
			{
				string p_overrideProfileImgUrl = null;
				if (string.IsNullOrEmpty(p_data.profileThumbURL))
				{
					p_overrideProfileImgUrl = base.app.model.storage.state.player.profile.photoURL;
				}
				byDroneId.garage = base.app.model.storage.state.player.garage;
				byDroneId.Set(p_data, showCriteria == UICommunityDronesShowCriteria.MyDrones, base.app.model.storage.state.player.profile.playerId, p_overrideProfileImgUrl);
			}
		}

		public UICommunityDronesItemView GetByDroneId(string p_id)
		{
			for (int i = 0; i < listField.Count; i++)
			{
				UICommunityDronesItemView uICommunityDronesItemView = listField.Get<UICommunityDronesItemView>(i);
				if (uICommunityDronesItemView.data != null && uICommunityDronesItemView.data.guid == p_id)
				{
					return uICommunityDronesItemView;
				}
			}
			return null;
		}

		public int GetDroneIndex(DRLCommunityDroneData p_drone)
		{
			for (int i = 0; i < drones.Count; i++)
			{
				if (drones[i].guid == p_drone.guid)
				{
					return i;
				}
			}
			return -1;
		}

		public bool ContainsDrone(List<DRLCommunityDroneData> p_list, DRLCommunityDroneData p_drone)
		{
			if (p_drone == null)
			{
				return false;
			}
			if (p_list == null)
			{
				return false;
			}
			if (p_list.Count <= 0)
			{
				return false;
			}
			for (int i = 0; i < p_list.Count; i++)
			{
				if (p_list[i].guid == p_drone.guid)
				{
					return true;
				}
			}
			return false;
		}

		public void SetFeedback(UICommunityDronesFeedbackType p_type, bool p_hide_list, float p_delay)
		{
			float feedback_alpha = ((p_type == UICommunityDronesFeedbackType.None) ? (-0.1f) : 1f);
			float content_alpha = ((p_type == UICommunityDronesFeedbackType.None) ? 1f : (p_hide_list ? (-0.1f) : 1f));
			Action action = delegate
			{
				feedbackFade.Fade(feedback_alpha, 0.3f, 0.05f, Cubic.Out);
				listFade.Fade(content_alpha, 0.3f, 0f, Cubic.Out);
				if (p_type != UICommunityDronesFeedbackType.None)
				{
					int num = (int)p_type;
					for (int i = 0; i < feedbacks.Count; i++)
					{
						feedbacks[i].SetActive(i == num);
					}
				}
			};
			if (p_delay <= 0f)
			{
				action();
			}
			else
			{
				RunOnce(p_delay, action);
			}
		}

		public void SetFeedback(UICommunityDronesFeedbackType p_type, bool p_hide_list)
		{
			SetFeedback(p_type, p_hide_list, 0f);
		}

		public void SetFeedback(UICommunityDronesFeedbackType p_type)
		{
			SetFeedback(p_type, p_hide_list: true, 0f);
		}
	}
}
