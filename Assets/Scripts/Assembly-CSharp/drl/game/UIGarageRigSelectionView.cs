using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIGarageRigSelectionView : UIScreenView
	{
		public List<ListComponent> grids;

		public UINavigation createCardNav;

		public List<UICardView> createCards;

		public List<UINavigation> createDroneButtons;

		public List<RectTransform> gridHeaders;

		public List<GameObject> gridContainers;

		public bool allowCreation = true;

		public bool selectionOnly;

		public bool loadOriginalRigs;

		public bool allowCustomPhysics = true;

		public bool unlockedRigsOnly;

		public bool backButtonDoubleReturn;

		public bool openStoreOnSelection;

		public List<DroneRigData> promoList;

		public List<DroneRigData> overrideList;

		public List<int> overrideSizes;

		public bool openedFromDashboard;

		public bool openedAsTemplateSelector;

		public bool openedFromBrackets;

		public bool openedAsTournamentSelector;

		public UIElementView backButton;

		public GameObject droneSpecsPanel;

		private UIGarageSpecsController specsController;

		public Dictionary<string, float> flightTimes = new Dictionary<string, float>();

		public Dictionary<string, Texture2D> brandLogos = new Dictionary<string, Texture2D>();

		public Dictionary<string, DroneRigSpecData> droneSpecsData = new Dictionary<string, DroneRigSpecData>();

		private Vector2 droneSpecsPanelPos = new Vector2(0f, -20f);

		private CanvasGroup droneSpecsCanvasGroup;

		public Transform gridsContainer;

		public void CreateSpecsPanel()
		{
			if (droneSpecsPanel == null)
			{
				return;
			}
			droneSpecsPanel.transform.SetParent(base.app.view.ui.screens.transform, worldPositionStays: false);
			droneSpecsPanel.transform.SetAsLastSibling();
			droneSpecsPanel.GetComponent<RectTransform>().anchoredPosition = droneSpecsPanelPos;
			droneSpecsCanvasGroup = droneSpecsPanel.GetComponent<CanvasGroup>();
			droneSpecsCanvasGroup.alpha = 1f;
			specsController = droneSpecsPanel.GetComponent<UIGarageSpecsController>();
			specsController.view.SetBarMaximums();
			AssetLibrary library = base.app.model.storage.library;
			float num = 0f;
			for (int i = 0; i < grids.Count; i++)
			{
				List<RectTransform> list = grids[i].GetList<RectTransform>();
				for (int j = 0; j < list.Count; j++)
				{
					RectTransform rectTransform = list[j];
					UICardButtonDroneRig r = rectTransform.GetComponent<UICardButtonDroneRig>();
					if (!r || r.data == null)
					{
						continue;
					}
					if (!flightTimes.ContainsKey(r.data.guid))
					{
						flightTimes.Add(r.data.guid, 0f);
						Activity.RunOnce(delegate
						{
							base.app.model.service.GetCommunityDroneTime(r.data.guid, delegate(DRLServiceResult res)
							{
								if (res != null && res.data != null)
								{
									flightTimes[r.data.guid] = res.GetData<float>();
								}
							});
						}, num);
						num += 1f / 12f;
					}
					if (!brandLogos.ContainsKey(r.data.guid))
					{
						Texture2D logo = library.FindByGUID<DronePart>(r.data.frame).info.logo;
						brandLogos.Add(r.data.guid, logo);
					}
					if (!droneSpecsData.ContainsKey(r.data.guid))
					{
						DroneRigSpecData droneSpecData = base.app.model.storage.state.player.garage.GetDroneSpecData(r.data);
						droneSpecsData.Add(r.data.guid, droneSpecData);
					}
				}
			}
			SetupSpecsPanel();
		}

		public void ClearSpecsPanel()
		{
			if (droneSpecsCanvasGroup == null)
			{
				return;
			}
			droneSpecsCanvasGroup.alpha = 0f;
			this.TimerRunOnce(delegate
			{
				if (!(droneSpecsPanel == null))
				{
					droneSpecsPanel.transform.SetParent(base.transform, worldPositionStays: false);
					droneSpecsPanel.transform.SetAsLastSibling();
					droneSpecsPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
				}
			}, 0.1f);
		}

		public void SetupSpecsPanel()
		{
			if (droneSpecsPanel == null)
			{
				return;
			}
			DroneRigData currentRigData = base.app.model.storage.state.player.garage.currentRigData;
			if (!droneSpecsData.ContainsKey(currentRigData.guid))
			{
				return;
			}
			DroneRigSpecData droneRigSpecData = GetDroneSpecsData(currentRigData.guid);
			if (!(currentRigData == null) && !(specsController == null))
			{
				specsController.SetBars(droneRigSpecData, droneRigSpecData);
				specsController.SetName(currentRigData);
				if (brandLogos.ContainsKey(currentRigData.guid))
				{
					specsController.SetLogo(brandLogos[currentRigData.guid]);
				}
				SetFlightTime(currentRigData);
			}
		}

		public void RefreshSpecsPanel(DroneRigData p_previewDrone)
		{
			if (droneSpecsPanel == null)
			{
				return;
			}
			DroneRigData currentRigData = base.app.model.storage.state.player.garage.currentRigData;
			if (!(currentRigData == null) && !(p_previewDrone == null))
			{
				DroneRigSpecData p_saved = GetDroneSpecsData(currentRigData.guid);
				DroneRigSpecData p_modified = GetDroneSpecsData(p_previewDrone.guid);
				specsController.SetBars(p_saved, p_modified);
				specsController.SetName(p_previewDrone);
				if (brandLogos.ContainsKey(p_previewDrone.guid))
				{
					specsController.SetLogo(brandLogos[p_previewDrone.guid]);
				}
				SetFlightTime(p_previewDrone);
			}
		}

		private void SetFlightTime(DroneRigData p_drone)
		{
			if (flightTimes.ContainsKey(p_drone.guid))
			{
				specsController.SetFlightTime(flightTimes[p_drone.guid]);
				return;
			}
			flightTimes.Add(p_drone.guid, 0f);
			specsController.SetFlightTime(0f);
		}

		private DroneRigSpecData GetDroneSpecsData(string p_guid)
		{
			if (!droneSpecsData.ContainsKey(p_guid))
			{
				return default(DroneRigSpecData);
			}
			return droneSpecsData[p_guid];
		}

		public void SetCreationEnabled(bool p_flag)
		{
			allowCreation = p_flag;
			for (int i = 0; i < createDroneButtons.Count; i++)
			{
				createDroneButtons[i].gameObject.SetActive(p_flag);
			}
			for (int j = 0; j < gridHeaders.Count; j++)
			{
				gridHeaders[j].gameObject.SetActive(!p_flag);
			}
		}

		public void SetDroneClassEnabled(bool p_flag, params int[] p_drone_classes)
		{
			if (p_drone_classes.Length == 0)
			{
				p_drone_classes = new int[4] { 3, 4, 5, 6 };
			}
			for (int i = 0; i < p_drone_classes.Length; i++)
			{
				int num = p_drone_classes[i] - 2;
				if (num >= 0 && num < gridContainers.Count)
				{
					gridContainers[num].SetActive(p_flag);
				}
			}
		}

		public void UpdateScrollContainer(float p_duration)
		{
			UINavigationScroll component = GetComponent<UINavigationScroll>();
			if ((bool)component)
			{
				component.SetContentSizeChanging(p_duration);
			}
		}
	}
}
