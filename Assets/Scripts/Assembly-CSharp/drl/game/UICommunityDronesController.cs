using System;
using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UICommunityDronesController : Controller<DRLApp>
	{
		public int pageLength = 10;

		public DRLCommunityDroneData currentDrone;

		public DRLCommunityDroneData.SortType sortingCriteria = DRLCommunityDroneData.SortType.ScoreDesc;

		public int filterDroneSize;

		public int filterPhysics = -1;

		public string searchQuery;

		public List<DRLCommunityDroneData> dronesList;

		private int m_pagesTotalCount;

		private int m_currentPage;

		private MonoActivity m_refresh_timer;

		private MonoActivity m_search_timer;

		private bool m_showing;

		private bool m_lock_ui;

		protected WebAsyncRequest m_web_loader;

		protected MonoActivity deleteButtonCoolDown;

		private bool m_inGame;

		public UICommunityDronesView view => AssertLocal<UICommunityDronesView>("view");

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "ui.screen@close":
				base.app.view.audio.StopUILoadingLoop();
				CancelWebLoad();
				break;
			case "community-drones.create-new@click":
				OpenDroneSelectionScreen(0);
				if (!(base.app == null) && !(base.app.model == null) && !(base.app.model.storage == null) && !(base.app.model.storage.state == null) && !(base.app.model.storage.state.player == null))
				{
					_ = base.app.model.storage.state.player.garage == null;
				}
				break;
			}
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					m_inGame = base.app.level.IsLevelLoaded("game");
					if (dronesList == null)
					{
						dronesList = new List<DRLCommunityDroneData>();
					}
					if (m_inGame)
					{
						SetIgnoredGameCommands();
					}
					m_showing = true;
					base.app.model.service.platform.RefreshFlags(delegate
					{
						Refresh(0.2f);
					});
					this.TimerRunOnce(delegate
					{
						UINavigation.Focus(view.showStepper);
					}, 0.5f);
					view.physicsStepper.labelField.text = view.physicsStepper.label;
					view.sizeStepper.labelField.text = view.sizeStepper.label;
					view.sortStepper.labelField.text = view.sortStepper.label;
				}
				break;
			case "community-drones.form.event@click":
				OnFormNotification(p_target, p_is_change: false, p_event);
				break;
			case "community-drones.form.event@change":
				OnFormNotification(p_target, p_is_change: true, p_event);
				break;
			case "community-drones.form.event@end-edit":
				OnFormNotification(p_target, p_is_change: false, p_event);
				break;
			case "community-drones.form.event@submit":
				OnFormNotification(p_target, p_is_change: false, p_event);
				break;
			case "community-drones.page@select":
				if (!m_lock_ui)
				{
					int p_page = (m_currentPage = (int)p_data[0]);
					Debug.Log("UICommunityDronesController> Page Select [" + p_page + "] + index = ");
					UpdatePage(p_page, pageLength, filterDroneSize, filterPhysics, sortingCriteria, searchQuery);
				}
				break;
			case "community-drones.page-next@click":
				if (!m_lock_ui && view.pageField.index + 1 != view.pageField.listField.Count)
				{
					view.pageField.index = view.pageField.index + 1;
					m_currentPage = view.pageField.index;
					UpdatePage(m_currentPage, pageLength, filterDroneSize, filterPhysics, sortingCriteria, searchQuery);
				}
				break;
			case "community-drones.page-previous@click":
				if (!m_lock_ui && view.pageField.index != 0)
				{
					view.pageField.index = view.pageField.index - 1;
					m_currentPage = view.pageField.index;
					UpdatePage(m_currentPage, pageLength, filterDroneSize, filterPhysics, sortingCriteria, searchQuery);
				}
				break;
			case "garage.selection.data@click":
			case "garage.selection.fly@click":
			case "garage.selection.edit@click":
			case "garage.selection.clone@click":
			case "garage.selection.save@click":
			{
				if (m_lock_ui)
				{
					break;
				}
				Component component2 = p_target as Component;
				if (!component2)
				{
					break;
				}
				UICommunityDronesItemView uICommunityDronesItemView2 = Hierarchy.FindReverse<UICommunityDronesItemView>(component2.transform);
				DRLCommunityDroneData ri = uICommunityDronesItemView2.data;
				currentDrone = ri;
				if (string.IsNullOrEmpty(ri.droneRigData))
				{
					base.app.view.audio.PlayUIGenericError();
					break;
				}
				StorageModel storage = base.app.model.storage;
				GarageStateModel garage = base.app.model.storage.state.player.garage;
				bool flag = base.app.level.IsLevelLoaded("game");
				string playerId = base.app.model.storage.state.player.profile.playerId;
				bool num = ((ri == null) ? "" : ri.playerId.ToString()) == playerId;
				bool flag2 = p_event == "garage.selection.save@click";
				bool flag3 = p_event == "garage.selection.clone@click";
				bool flag4 = ri != null && ri.isPublic;
				bool flag5 = num;
				switch (p_event)
				{
				case "garage.selection.edit@click":
					base.enabled = false;
					storage.PreloadDroneBundleData(null, null, p_ingame: false, delegate
					{
						base.enabled = true;
						LoadGarage(ri);
					});
					break;
				case "garage.selection.clone@click":
				case "garage.selection.save@click":
				{
					DroneRigData clonerig = DroneRigData.FromJson(ri.droneRigData);
					if (!(clonerig == null) && (!flag3 || flag5) && (!flag2 || flag4))
					{
						clonerig.guid = DroneRigData.GenerateGUID();
						if (flag2)
						{
							clonerig.isPublic = false;
						}
						clonerig.name = "";
						base.enabled = false;
						storage.PreloadDroneBundleData(null, null, p_ingame: false, delegate
						{
							base.enabled = true;
							LoadGarage(clonerig);
						});
					}
					break;
				}
				default:
				{
					DroneRigData droneRigData = garage.GetRigByGUID(ri.guid);
					if (droneRigData != null)
					{
						garage.currentRigData = droneRigData;
					}
					else
					{
						droneRigData = DroneRigData.FromJson(ri.droneRigData);
						if (droneRigData == null)
						{
							break;
						}
						if (ri.playerId != base.app.model.service.backend.playerId)
						{
							droneRigData.guid = DroneRigData.GenerateGUID();
							droneRigData.isLocked |= !ri.isPublic;
						}
					}
					garage.currentRigData = droneRigData;
					Notify("storage.drone@refresh");
					if (flag)
					{
						ApplyCurrentDrone(garage.currentRigData);
						break;
					}
					Notify("garage.edit.fly.ready", garage.currentRigData, null);
					break;
				}
				}
				break;
			}
			case "garage.selection.delete@click":
			{
				if (m_lock_ui)
				{
					break;
				}
				Component component3 = p_target as Component;
				if (!component3)
				{
					break;
				}
				UICommunityDronesItemView bt = Hierarchy.FindReverse<UICommunityDronesItemView>(component3.transform);
				DRLCommunityDroneData data = bt.data;
				Debug.Log("UICommunityDronesController> delete drone " + data.droneName);
				if (!bt.confirmDelete)
				{
					if (deleteButtonCoolDown != null)
					{
						m_lock_ui = false;
						break;
					}
					bt.ShowConfirmDelete();
					deleteButtonCoolDown = RunOnce(delegate
					{
						if (bt != null)
						{
							bt.ShowDeleteButton();
						}
						deleteButtonCoolDown = null;
					}, 3f);
					m_lock_ui = false;
					break;
				}
				if (deleteButtonCoolDown != null)
				{
					deleteButtonCoolDown.Stop();
					deleteButtonCoolDown = null;
				}
				view.SetFeedback(UICommunityDronesFeedbackType.Loading);
				this.TimerRunOnce(delegate
				{
					UINavigation.Focus(view.showStepper);
				}, 0.5f);
				base.app.model.storage.state.player.garage.DeleteRig(data.guid, delegate(DRLServiceResult p_result)
				{
					if (p_result != null)
					{
						UpdatePage(m_currentPage, pageLength, filterDroneSize, filterPhysics, sortingCriteria, searchQuery);
						base.app.controller.RefreshFooterDrone();
					}
				});
				break;
			}
			case "garage.selection.data@focus":
			case "garage.selection.data@unfocus":
			{
				if (m_lock_ui)
				{
					break;
				}
				Component component = p_target as Component;
				if ((bool)component)
				{
					UICommunityDronesItemView uICommunityDronesItemView = Hierarchy.FindReverse<UICommunityDronesItemView>(component.transform);
					if ((bool)uICommunityDronesItemView)
					{
						uICommunityDronesItemView.droneThumbnailAnimation.animationType = ((p_event == "garage.selection.data@focus") ? AnimateImageLayout.AnimationType.OscilateVertical : AnimateImageLayout.AnimationType.None);
					}
				}
				break;
			}
			case "ui.screen.return@click":
				CancelWebLoad();
				if (view.inGame)
				{
					base.app.view.ui.screens.Close("community-drones-screen");
					base.app.view.ui.game.hud.Show();
					Notify("game.ui.dashboard@show");
				}
				else
				{
					base.app.view.ui.screens.Return();
				}
				break;
			}
		}

		private void OpenDroneSelectionScreen(int p_droneClass)
		{
			UIGarageRigSelectionView uIGarageRigSelectionView = base.app.view.ui.screens.Open<UIGarageRigSelectionView>("garage-rig-selection-screen");
			uIGarageRigSelectionView.screen.title = base.app.model.storage.locale.Get("multiplayer.select-drone-screen.title", "Select your Drone");
			uIGarageRigSelectionView.SetCreationEnabled(p_flag: false);
			uIGarageRigSelectionView.allowCustomPhysics = true;
			uIGarageRigSelectionView.selectionOnly = true;
			uIGarageRigSelectionView.unlockedRigsOnly = true;
			uIGarageRigSelectionView.openedAsTemplateSelector = true;
			if (p_droneClass > 1)
			{
				uIGarageRigSelectionView.SetDroneClassEnabled(true);
				uIGarageRigSelectionView.overrideList = null;
				uIGarageRigSelectionView.overrideSizes = new List<int>(1) { p_droneClass };
			}
			else
			{
				uIGarageRigSelectionView.SetDroneClassEnabled(true);
				uIGarageRigSelectionView.overrideList = null;
				uIGarageRigSelectionView.overrideSizes = null;
			}
		}

		public void Refresh(float p_delay)
		{
			if (m_refresh_timer != null)
			{
				m_refresh_timer.Stop();
			}
			m_refresh_timer = RunOnce(delegate
			{
				UpdatePage(0, pageLength, filterDroneSize, filterPhysics, sortingCriteria, searchQuery);
			}, p_delay);
		}

		public async void UpdatePage(int p_page, int p_total, int p_droneSize, int p_physics, DRLCommunityDroneData.SortType p_sort, string p_search, float p_userDronesRefreshDelay = 0.5f)
		{
			PlatformService ps = base.app.model.service.platform;
			if (ps.ContainsFlag(PlatformServiceFlagType.XBoxUGCBlocked))
			{
				view.SetFeedback(UICommunityDronesFeedbackType.UGCBlock);
				this.TimerRunOnce(delegate
				{
					ps.CheckPlatformUGCPrivilege(delegate
					{
						if (ps.ContainsFlag(PlatformServiceFlagType.XBoxUGCBlocked))
						{
							base.app.view.ui.screens.Return();
						}
						else
						{
							UpdatePage(p_page, p_total, p_droneSize, p_physics, p_sort, p_search, p_userDronesRefreshDelay);
						}
					});
				}, 2f);
				return;
			}
			CancelWebLoad();
			view.Clear();
			if (p_userDronesRefreshDelay > 0.01f)
			{
				view.SetFeedback(UICommunityDronesFeedbackType.Loading);
			}
			string p_player_id = ((view.showCriteria == UICommunityDronesShowCriteria.CommunityDrones) ? "" : base.app.model.service.backend.playerId);
			base.app.view.audio.PlayUILoadingLoop();
			if (view.showCriteria == UICommunityDronesShowCriteria.CommunityDrones)
			{
				ServiceModel service = base.app.model.service;
				m_web_loader = service.GetCommunityDrones(p_player_id, null, p_page, p_total, p_droneSize, p_physics, p_sort, p_search, delegate(DRLCommunityDroneResult p_result)
				{
					base.app.view.audio.StopUILoadingLoop();
					if (m_web_loader.status != AsyncRequestStatus.Created && m_web_loader.status != AsyncRequestStatus.Cancelled)
					{
						if (p_result == null)
						{
							base.app.view.audio.PlayUIGenericError();
							view.SetFeedback(UICommunityDronesFeedbackType.OperationFailure);
							Debug.LogWarning("UICommunityDronesController> UpdatePage - Failed!");
						}
						else
						{
							base.app.view.audio.PlayUILoadingSuccess();
							dronesList = new List<DRLCommunityDroneData>(p_result.data);
							m_pagesTotalCount = p_result.pagging.pageTotal;
							if (dronesList.Count > 0)
							{
								view.UpdateList(dronesList, p_page, p_total, p_result.pagging.pageTotal);
							}
							else
							{
								view.SetFeedback(UICommunityDronesFeedbackType.NoDrones);
								view.pageField.Set(0);
							}
						}
					}
				});
			}
			else
			{
				if (view.showCriteria != UICommunityDronesShowCriteria.MyDrones)
				{
					return;
				}
				base.app.model.storage.state.player.garage.FilterDronesData(p_droneSize, p_physics, p_sort, p_search, delegate(List<DRLCommunityDroneData> p_result)
				{
					dronesList = p_result;
					m_pagesTotalCount = -1;
					m_refresh_timer = RunOnce(delegate
					{
						base.app.view.audio.StopUILoadingLoop();
						if (dronesList.Count > 0)
						{
							view.UpdateList(dronesList, p_page, p_total);
						}
						else
						{
							view.SetFeedback(UICommunityDronesFeedbackType.NoDrones);
							view.pageField.Set(0);
							Component focus = UINavigation.focus;
							if (!focus || !focus.transform.IsChild(view.transform) || !focus.gameObject.activeInHierarchy)
							{
								UINavigation.Focus(view.showStepper);
							}
						}
					}, p_userDronesRefreshDelay);
				});
			}
		}

		protected void CancelWebLoad()
		{
			if (m_web_loader != null)
			{
				m_web_loader.Cancel();
			}
		}

		public bool HasCurrentDrone()
		{
			return currentDrone != null;
		}

		public bool IsCurrentDroneMine()
		{
			if (!HasCurrentDrone())
			{
				return false;
			}
			return base.app.model.storage.state.player.garage.GetDroneRigByGUID(currentDrone.guid) != null;
		}

		public int GetCurrentDroneRating()
		{
			if (HasCurrentDrone())
			{
				if (IsCurrentDroneMine())
				{
					return Mathf.RoundToInt(currentDrone.score * 5f);
				}
				return Mathf.RoundToInt(currentDrone.rating * 5f);
			}
			return 0;
		}

		public void SetCurrentDroneRating(int p_rating)
		{
			_ = (float)p_rating / 5f;
			_ = base.app.model.service;
		}

		public string GetCurrentDroneName()
		{
			return "MY DRONE";
		}

		public void SaveNewDrone(Drone p_drone, string p_name, Action<DRLCommunityDroneData> p_callback)
		{
		}

		protected void LoadGarage(DRLCommunityDroneData p_rigData)
		{
			if (string.IsNullOrEmpty(p_rigData.droneRigData))
			{
				return;
			}
			DroneRigData droneRigData = DroneRigData.FromJson(p_rigData.droneRigData);
			if (droneRigData == null)
			{
				return;
			}
			if (p_rigData.playerId != base.app.model.service.backend.playerId)
			{
				if (!p_rigData.isPublic)
				{
					return;
				}
				droneRigData.guid = DroneRigData.GenerateGUID();
			}
			droneRigData.isPublic = p_rigData.isPublic || base.app.model.storage.state.player.garage.IsOriginal(droneRigData);
			base.app.model.storage.state.player.garage.currentRigData = droneRigData;
			base.app.view.ui.screens.Open<UIGarageRigEditView>("garage-rig-edit-screen").data = droneRigData;
		}

		protected void LoadGarage(DroneRigData p_rig)
		{
			if (!(p_rig == null))
			{
				base.app.model.storage.state.player.garage.currentRigData = p_rig;
				base.app.view.ui.screens.Open<UIGarageRigEditView>("garage-rig-edit-screen").data = p_rig;
			}
		}

		public void UpdateCurrentDrone(Drone p_drone, string p_name, Action<DRLCommunityDroneData> p_callback)
		{
			if (currentDrone == null)
			{
				SaveNewDrone(p_drone, p_name, p_callback);
			}
		}

		protected Drone ChangeRig(Drone p_old, DroneRigData p_new)
		{
			GamePlayerData playerData = base.app.model.game.GetPlayerData(p_old);
			Drone drone = base.app.model.storage.factory.Replace(p_new, p_old, p_old.transform.parent, p_old.transform.parent, p_async: false);
			base.app.controller.game.ApplyCommunityDroneToDrone(drone);
			Notify("game.simulation.drone@replace", playerData.drone, drone);
			playerData.drone = drone;
			drone.position = base.app.model.game.simulation.podiums.list[0].spawn.position;
			drone.transform.rotation = base.app.model.game.simulation.podiums.list[0].spawn.rotation;
			drone.fc.Reset();
			drone.renderer.SetTrailsEnabled(p_flag: false);
			return drone;
		}

		public void ApplyCurrentDrone(DroneRigData p_data)
		{
			FCMode activeFCMode = base.app.model.storage.state.player.activeFCMode;
			Drone drone = ChangeRig(base.app.controller.game.model.playerDrone, p_data);
			base.app.model.storage.state.player.garage.activeRigData = p_data;
			switch (activeFCMode)
			{
			case FCMode.Beginner:
				drone.fc.SetMode(FlightControllerMode.Beginner);
				break;
			case FCMode.Pro:
				drone.fc.SetMode(FlightControllerMode.Pro);
				break;
			case FCMode.Intermediate:
				drone.fc.SetMode(FlightControllerMode.Intermediate);
				break;
			}
			FCProfileData active = base.app.model.storage.state.player.settings.tuning.GetActive();
			if (active != null && drone.fc != null)
			{
				drone.fc.profile.SetData(active);
			}
			CloseScreenFromGame(drone, p_data);
		}

		protected void CloseScreenFromGame(Drone p_drone = null, DroneRigData p_rigData = null)
		{
			if (m_inGame)
			{
				ClearIgnoredCommands();
			}
			base.app.view.ui.screens.Close("community-drones-screen");
			base.app.controller.game.input.controller.Pause(p_flag: false, p_pause_physics: false);
			Notify("game.unpause");
			if (p_drone != null && p_rigData != null)
			{
				Notify("garage.edit.fly.ready", p_rigData, p_drone);
			}
			Activity.RunOnce(delegate
			{
				Notify("game.ui.dashboard@hide");
			}, 0.05f);
		}

		private Drone GetDrone()
		{
			Drone result = null;
			if ((bool)base.app && (bool)base.app.controller && (bool)base.app.controller.game && (bool)base.app.controller.game.model)
			{
				result = base.app.controller.game.model.playerDrone;
			}
			return result;
		}

		public void Hide()
		{
			m_showing = false;
			FadeComponent component = base.gameObject.GetComponent<FadeComponent>();
			if ((bool)component)
			{
				component.FadeOut();
			}
		}

		public void SetIgnoredGameCommands()
		{
			if (!base.app.controller)
			{
				return;
			}
			List<GameCommand> list = new List<GameCommand>();
			foreach (GameInputMapComponent map in base.app.controller.game.input.maps)
			{
				foreach (GameCommand command in map.commands)
				{
					if (command.type == GameCommandType.EditDrone || command.type == GameCommandType.ResetDrone || command.type == GameCommandType.ResetDronePodium || command.type == GameCommandType.ResetGame || command.type == GameCommandType.SwitchCameraMode || command.type == GameCommandType.SwitchDebugDashboard || command.type == GameCommandType.SwitchPhysicsDashboard)
					{
						list.Add(command);
					}
				}
			}
			base.app.controller.game.input.SetIgnoredCommands(list);
		}

		private void ClearIgnoredCommands()
		{
			if ((bool)base.app.controller)
			{
				base.app.controller.game.input.ClearIgnoredCommands();
			}
		}

		protected void OnFormNotification(UnityEngine.Object p_target, bool p_is_change, string p_event)
		{
			if (m_lock_ui)
			{
				return;
			}
			bool flag = p_is_change;
			bool flag2 = p_event.Contains("@end-edit");
			bool flag3 = p_event.Contains("@submit");
			switch (p_target.name)
			{
			case "drone-show-stepper":
				if (flag)
				{
					switch ((p_target as DRLStepperView).index)
					{
					case 0:
						view.showCriteria = UICommunityDronesShowCriteria.CommunityDrones;
						break;
					case 1:
						view.showCriteria = UICommunityDronesShowCriteria.MyDrones;
						break;
					}
					Refresh(0.6f);
				}
				break;
			case "drone-class":
				if (flag)
				{
					switch ((p_target as DRLStepperView).index)
					{
					case 0:
						filterDroneSize = 0;
						break;
					case 1:
						filterDroneSize = 3;
						break;
					case 2:
						filterDroneSize = 4;
						break;
					case 3:
						filterDroneSize = 5;
						break;
					case 4:
						filterDroneSize = 6;
						break;
					case 5:
						filterDroneSize = 7;
						break;
					}
					Refresh(0.6f);
				}
				break;
			case "physics-stepper":
				if (flag)
				{
					switch ((p_target as DRLStepperView).index)
					{
					case 0:
						filterPhysics = -1;
						break;
					case 1:
						filterPhysics = 0;
						break;
					case 2:
						filterPhysics = 1;
						break;
					}
					Refresh(0.6f);
				}
				break;
			case "drone-sort-stepper":
				if (flag)
				{
					switch ((p_target as DRLStepperView).index)
					{
					case 0:
						sortingCriteria = DRLCommunityDroneData.SortType.FlightTotalDesc;
						break;
					case 1:
						sortingCriteria = DRLCommunityDroneData.SortType.ScoreDesc;
						break;
					case 2:
						sortingCriteria = DRLCommunityDroneData.SortType.ThrustDesc;
						break;
					case 3:
						sortingCriteria = DRLCommunityDroneData.SortType.WeightAsc;
						break;
					case 4:
						sortingCriteria = DRLCommunityDroneData.SortType.WeightDesc;
						break;
					}
					Refresh(0.6f);
				}
				break;
			case "drone-search-input":
				if (flag3 || flag2)
				{
					DRLInputFieldView dRLInputFieldView = p_target as DRLInputFieldView;
					searchQuery = dRLInputFieldView.field.text.ToUpper();
					Refresh(0.6f);
				}
				break;
			}
		}
	}
}
