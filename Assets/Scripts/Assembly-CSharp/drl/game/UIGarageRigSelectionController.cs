using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIGarageRigSelectionController : Controller<DRLApp>
	{
		private GarageStateModel m_model;

		public UICardButtonDroneRig rigCard;

		private bool m_inGame;

		private bool m_midRace;

		public UIGarageRigSelectionView view => AssertLocal<UIGarageRigSelectionView>("view");

		public GarageStateModel model
		{
			get
			{
				if (!m_model)
				{
					return base.app.model.storage.state.player.garage;
				}
				return m_model;
			}
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "ui.screen@close":
				if (p_data.Length != 0)
				{
					UIScreen uIScreen = (UIScreen)p_data[0];
					if (uIScreen != null && uIScreen == view.screen)
					{
						view.ClearSpecsPanel();
					}
				}
				break;
			case "ui.screen@open":
				if (p_data[0] as UIScreen != view.screen)
				{
					view.ClearSpecsPanel();
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
			{
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				m_inGame = base.app.level.IsLevelLoaded("game");
				bool flag = m_inGame && base.app.model.game.mode != GameFlag.NetworkMultiplayer && base.app.model.game.type == GameFlag.Race && base.app.controller.game.race != null && base.app.controller.game.race.model.status == RaceStatusType.Running;
				bool flag2 = m_inGame && base.app.model.game.mode != GameFlag.NetworkMultiplayer && base.app.model.game.type == GameFlag.Collectable && base.app.controller.game.collectable != null && base.app.controller.game.collectable.model.status == RaceStatusType.Running;
				m_midRace = flag || flag2;
				m_model = base.app.model.storage.state.player.garage;
				if (m_inGame)
				{
					view.loadOriginalRigs = true;
				}
				else
				{
					view.loadOriginalRigs = false;
				}
				if (m_inGame)
				{
					DRLTournamentData tournamentData = base.app.arguments.game.tournamentData;
					if (tournamentData != null && tournamentData.droneClass > 1)
					{
						view.overrideSizes = new List<int>(1) { tournamentData.droneClass };
					}
				}
				PopulateCards();
				view.CreateSpecsPanel();
				break;
			}
			case "garage.selection.create@click":
			{
				UIElementView obj = p_target as UIElementView;
				int p_class = 6;
				switch (obj.name)
				{
				case "new-3":
					p_class = 3;
					break;
				case "new-4":
					p_class = 4;
					break;
				case "new-5":
					p_class = 5;
					break;
				case "new-6":
					p_class = 6;
					break;
				case "new-7":
					p_class = 7;
					break;
				}
				DroneRigData rd = model.GetTemplate(p_class);
				if (rd == null)
				{
					Debug.LogWarning("UIGarageRigSelectionController> Failed to create a rig template at [" + p_class + "\"]");
					break;
				}
				view.ClearSpecsPanel();
				rd.guid = DroneRigData.GenerateGUID();
				base.app.model.storage.PreloadDroneBundleData(null, null, m_inGame, delegate
				{
					if (m_inGame)
					{
						OpenGarageEditFromGame(rd);
					}
					else
					{
						OpenGarageEdit(rd);
					}
				});
				break;
			}
			case "garage.selection.item@click":
			{
				Debug.Log($"UIGarageRigSelectionController> SelectionItemClick / button[{p_target}] ingame[{m_inGame}]");
				UICardButtonDroneRig uICardButtonDroneRig2 = p_target as UICardButtonDroneRig;
				rigCard = null;
				if (!uICardButtonDroneRig2)
				{
					break;
				}
				DroneRigData rd2 = uICardButtonDroneRig2.data;
				if (rd2 == null)
				{
					Debug.LogWarning("UIGarageRigSelectionController> Rig Item Click Failed - Null Rig");
					break;
				}
				if (!view.selectionOnly)
				{
					rigCard = uICardButtonDroneRig2;
					uICardButtonDroneRig2.OpenMenu();
					break;
				}
				if (!view.openedAsTemplateSelector && (bool)model)
				{
					model.activeRigData = rd2;
				}
				StorageModel storage2 = base.app.model.storage;
				bool is_tournament = base.app.inTournament;
				if (m_inGame)
				{
					Drone player_drone2 = GetPlayerDrone();
					FCMode playerActiveFCMode2 = GetPlayerActiveFCMode();
					FlightControllerMode drone_fcmode2 = GetFlightControllerMode(playerActiveFCMode2);
					Action p_on_complete = delegate
					{
						base.enabled = true;
						if (m_midRace)
						{
							Drone drone = null;
							if ((bool)player_drone2)
							{
								drone = ChangeRig(base.app.controller.game.model.playerDrone, rd2);
								model.activeRigData = rd2;
								FCProfileData playerActiveFCProfileData = GetPlayerActiveFCProfileData();
								if ((bool)drone.fc)
								{
									drone.fc.profile.SetData(playerActiveFCProfileData);
									if (drone_fcmode2 != FlightControllerMode.Pro && drone.rig.diameter != 0)
									{
										drone.fc.SetMode(FlightControllerMode.Pro);
									}
									if (drone.rig.diameter == 0)
									{
										drone.fc.SetMode(FlightControllerMode.Intermediate);
										drone.fc.SetMode(FlightControllerMode.Pro);
									}
									Notify("garage.drone.changed", 3, drone.rig.diameter);
								}
							}
							if (!view.openedFromBrackets)
							{
								CloseScreenFromGame(drone, rd2);
							}
							else
							{
								base.app.view.ui.screens.Return();
								base.app.view.ui.footer.Hide(0f);
							}
							if ((bool)rigCard)
							{
								rigCard.CloseMenu();
							}
							view.ClearSpecsPanel();
						}
						else
						{
							Debug.LogWarning(player_drone2);
							if (is_tournament)
							{
								model.activeRigData = rd2;
							}
							else if ((bool)player_drone2)
							{
								Drone drone2 = ChangeRig(player_drone2, rd2);
								if ((bool)drone2 && (bool)drone2.fc)
								{
									FCProfileData playerActiveFCProfileData2 = GetPlayerActiveFCProfileData();
									drone2.fc.profile.SetData(playerActiveFCProfileData2);
									if (drone_fcmode2 != FlightControllerMode.Pro && drone2.rig.diameter != 0)
									{
										drone2.fc.SetMode(FlightControllerMode.Pro);
									}
									if (drone2.rig.diameter == 0)
									{
										drone2.fc.SetMode(FlightControllerMode.Intermediate);
										drone2.fc.SetMode(FlightControllerMode.Pro);
									}
									Notify("garage.drone.changed", 3, drone2.rig.diameter);
								}
							}
						}
					};
					base.enabled = false;
					storage2.PreloadDroneBundleData(uICardButtonDroneRig2, rd2, m_inGame, p_on_complete);
				}
				Debug.Log("View.TemplateSelector: " + view.openedAsTemplateSelector);
				if (view.openedAsTemplateSelector)
				{
					base.enabled = false;
					storage2.PreloadDroneBundleData(uICardButtonDroneRig2, null, m_inGame, delegate
					{
						base.enabled = true;
						if (m_inGame)
						{
							OpenGarageEditFromGame(rd2, p_templateSelector: true);
						}
						else
						{
							OpenGarageEdit(rd2, p_templateSelector: true);
						}
						view.unlockedRigsOnly = false;
						view.openedAsTemplateSelector = false;
						view.ClearSpecsPanel();
					});
					break;
				}
				Action p_on_complete2 = delegate
				{
					base.enabled = true;
					view.backButtonDoubleReturn = false;
					base.app.view.audio.PlayUIGenericSuccess();
					if (!m_midRace && !view.openedAsTournamentSelector)
					{
						base.app.view.ui.screens.Return();
					}
					if (!m_midRace && view.openedAsTournamentSelector)
					{
						view.openedAsTournamentSelector = false;
						view.ClearSpecsPanel();
						Notify("tournament.drone.selected");
					}
				};
				base.enabled = false;
				storage2.PreloadDroneBundleData(uICardButtonDroneRig2, rd2, m_inGame, p_on_complete2);
				break;
			}
			case "garage.selection.item@over":
			{
				UICardButtonDroneRig uICardButtonDroneRig = p_target as UICardButtonDroneRig;
				if ((bool)uICardButtonDroneRig && uICardButtonDroneRig.data != null)
				{
					view.RefreshSpecsPanel(uICardButtonDroneRig.data);
				}
				else
				{
					view.SetupSpecsPanel();
				}
				break;
			}
			case "garage.selection.item@out":
				view.SetupSpecsPanel();
				break;
			case "garage.selection.item.menu@click":
			{
				Debug.Log("Log was reached");
				UIElementView uIElementView = p_target as UIElementView;
				switch (uIElementView.name)
				{
				case "fly":
				{
					if (!m_inGame)
					{
						break;
					}
					FCMode playerActiveFCMode = GetPlayerActiveFCMode();
					Drone player_drone = GetPlayerDrone();
					DroneRigData drone_rd = rigCard.data;
					FlightControllerMode drone_fcmode = GetFlightControllerMode(playerActiveFCMode);
					FCProfileData d = GetPlayerActiveFCProfileData();
					model.activeRigData = drone_rd;
					rigCard.CloseMenu();
					UICardButtonDroneRig p_button2 = Hierarchy.FindReverse<UICardButtonDroneRig>(uIElementView.transform);
					base.enabled = false;
					base.app.model.storage.PreloadDroneBundleData(p_button2, drone_rd, m_inGame, delegate
					{
						base.enabled = true;
						Drone drone = ChangeRig(player_drone, drone_rd);
						drone.fc.profile.SetData(d);
						if (drone_fcmode != FlightControllerMode.Pro && drone.rig.diameter != 0)
						{
							drone.fc.SetMode(FlightControllerMode.Pro);
						}
						if (drone.rig.diameter == 0)
						{
							drone.fc.SetMode(FlightControllerMode.Intermediate);
							drone.fc.SetMode(FlightControllerMode.Pro);
						}
						Notify("garage.drone.changed", 3, drone.rig.diameter);
						CloseScreenFromGame(drone, drone_rd);
						view.ClearSpecsPanel();
					});
					break;
				}
				case "edit":
				{
					if (!rigCard)
					{
						break;
					}
					view.ClearSpecsPanel();
					rigCard.CloseMenu();
					UICardButtonDroneRig p_button = Hierarchy.FindReverse<UICardButtonDroneRig>(uIElementView.transform);
					StorageModel storage = base.app.model.storage;
					base.enabled = false;
					storage.PreloadDroneBundleData(p_button, null, m_inGame, delegate
					{
						base.enabled = true;
						if (m_inGame)
						{
							OpenGarageEditFromGame(rigCard.data);
						}
						else
						{
							OpenGarageEdit(rigCard.data);
						}
					});
					break;
				}
				case "delete":
					if ((bool)rigCard)
					{
						model.DeleteRig(rigCard.data);
						rigCard.CloseMenu();
						ListComponent listComponent = Hierarchy.FindReverse<ListComponent>(rigCard.transform);
						if (!(listComponent == null))
						{
							int value = listComponent.list.IndexOf(rigCard);
							listComponent.Remove(rigCard);
							value = Mathf.Clamp(value, -1, listComponent.transform.childCount - 1);
							PopulateCards();
							UINavigation.Focus((value >= 0) ? ((Component)listComponent.transform.GetChild(value)) : ((Component)view.leftNavigation));
						}
					}
					break;
				}
				break;
			}
			case "ui.screen.return@click":
				view.ClearSpecsPanel();
				view.unlockedRigsOnly = false;
				if (view.openedAsTemplateSelector)
				{
					base.app.view.ui.screens.Return(1);
					view.openedAsTemplateSelector = false;
				}
				else if (view.openedFromDashboard)
				{
					base.app.view.ui.screens.Close("garage-rig-selection-screen");
					base.app.view.ui.game.hud.Show();
					view.openedFromDashboard = false;
				}
				else if (view.backButtonDoubleReturn)
				{
					base.app.view.ui.screens.Return(1);
					view.backButtonDoubleReturn = false;
				}
				else
				{
					base.app.view.ui.screens.Return();
				}
				break;
			}
		}

		protected Drone GetPlayerDrone()
		{
			if (!base.validContext)
			{
				return null;
			}
			GameController game = base.app.controller.game;
			if (!game)
			{
				return null;
			}
			if (!game.model)
			{
				return null;
			}
			return game.model.playerDrone;
		}

		protected FCMode GetPlayerActiveFCMode()
		{
			if (!base.validContext)
			{
				return FCMode.Pro;
			}
			return base.app.model.storage.state.player.activeFCMode;
		}

		protected FlightControllerMode GetFlightControllerMode(FCMode p_mode)
		{
			return p_mode switch
			{
				FCMode.Beginner => FlightControllerMode.Beginner, 
				FCMode.Pro => FlightControllerMode.Pro, 
				FCMode.Intermediate => FlightControllerMode.Intermediate, 
				_ => FlightControllerMode.Pro, 
			};
		}

		protected FCProfileData GetPlayerActiveFCProfileData()
		{
			if (!base.validContext)
			{
				return null;
			}
			return base.app.model.storage.state.player.settings.tuning.GetActive();
		}

		protected void OpenGarageEdit(DroneRigData p_data, bool p_templateSelector = false)
		{
			if (p_data == null)
			{
				Debug.LogWarning("UIGarageRigSelectionController> Failed to Open Edit");
			}
			else if (!p_data.isLocked)
			{
				UIGarageRigEditView uIGarageRigEditView = base.app.view.ui.screens.Open<UIGarageRigEditView>("garage-rig-edit-screen");
				uIGarageRigEditView.showStoreOnOpen = view.openStoreOnSelection;
				uIGarageRigEditView.data = p_data;
				uIGarageRigEditView.data.isPublic = false;
				if (p_templateSelector)
				{
					uIGarageRigEditView.openedFromRigTemplateSelector = true;
				}
				else
				{
					uIGarageRigEditView.openedFromRigSelection = true;
				}
				view.openStoreOnSelection = false;
			}
		}

		protected void OpenGarageEditFromGame(DroneRigData p_data, bool p_templateSelector = false)
		{
			if (p_data == null)
			{
				Debug.LogWarning("UIGarageRigSelectionController> Failed to Open Edit");
			}
			else if (!p_data.isLocked)
			{
				Drone playerDrone = base.app.controller.game.model.playerDrone;
				Drone drone = ChangeRig(playerDrone, p_data);
				UIGarageRigEditView uIGarageRigEditView = base.app.view.ui.screens.Open<UIGarageRigEditView>("garage-rig-edit-screen");
				uIGarageRigEditView.data = drone.rig;
				uIGarageRigEditView.data.isPublic = false;
				uIGarageRigEditView.externalDrone = drone;
				uIGarageRigEditView.openedFromDashboard = false;
				uIGarageRigEditView.openedFromPause = true;
				if (p_templateSelector)
				{
					uIGarageRigEditView.openedFromRigTemplateSelector = true;
				}
				else
				{
					uIGarageRigEditView.openedFromRigSelection = true;
				}
			}
		}

		protected Drone ChangeRig(Drone p_old, DroneRigData p_new)
		{
			if (p_old == null || p_new == null)
			{
				return null;
			}
			if (p_old != null)
			{
				Notify("game.simulation.drone.flight-time@update", p_old.rig);
			}
			GamePlayerData playerData = base.app.model.game.GetPlayerData(p_old);
			int channel = ((p_old != null && p_old.receiver != null) ? p_old.receiver.channel : 0);
			if (p_new.isOriginal && p_new.allowDynamicColor)
			{
				p_new.color0 = playerData.color;
				p_new.color2 = playerData.color;
			}
			Drone newDrone = base.app.model.storage.factory.Replace(p_new, p_old, p_old.transform.parent, p_old.transform.parent, p_async: false);
			base.app.controller.game.ApplyCommunityDroneToDrone(newDrone);
			base.app.controller.game.PodiumReset(newDrone);
			newDrone.OnEvent.AddListener(delegate(DroneEvent p_event)
			{
				switch (p_event.type)
				{
				case DroneEventType.NanRecover:
				{
					DroneSimulation simulation = base.app.model.game.simulation;
					if ((bool)simulation)
					{
						simulation.PlaceDrone(newDrone, -1, p_force_podium: true);
						newDrone.ClearForces();
					}
					break;
				}
				case DroneEventType.Ready:
					newDrone.receiver.channel = channel;
					newDrone.fc.armed = true;
					base.app.model.game.camera.drone = newDrone;
					Notify("game.simulation.drone@armed", newDrone);
					RunOnce(0.1f, delegate
					{
						if (newDrone != null && newDrone.hasRig)
						{
							if (newDrone.rig.hasCustomPhysics)
							{
								DronePhysicsData dronePhysicsData = DronePhysicsData.FromJson(newDrone.rig.tune);
								if (dronePhysicsData != null)
								{
									newDrone.physics = dronePhysicsData;
								}
							}
							if (newDrone.rig.hasCustomProfile)
							{
								DroneProfileData droneProfileData = DroneProfileData.FromJson(newDrone.rig.profile);
								if (droneProfileData != null)
								{
									newDrone.profile = droneProfileData;
								}
							}
							newDrone.SetPropwash(base.app.model.storage.state.player.settings.game.propwash);
						}
					});
					break;
				}
			});
			Notify("game.simulation.drone@replace", playerData.drone, newDrone);
			playerData.drone = newDrone;
			newDrone.position = base.app.model.game.simulation.podiums.list[0].spawn.position;
			newDrone.transform.rotation = base.app.model.game.simulation.podiums.list[0].spawn.rotation;
			newDrone.fc.Reset();
			newDrone.ClearForces();
			if (newDrone.physics != null && newDrone.physics.aerodynamics != null)
			{
				newDrone.physics.aerodynamics.Reset();
			}
			newDrone.renderer.SetTrailsEnabled(p_flag: false);
			string text = base.app.model.storage.state.player.profile.playerId.ToString();
			GamePlayerData playerById = base.app.arguments.game.GetPlayerById(text);
			if (playerById == null)
			{
				Debug.LogWarning("UIGarageRigSelectionController> Player [" + text + "] not found!");
			}
			if (playerById != null)
			{
				playerById.rig = newDrone.rig;
			}
			return newDrone;
		}

		protected void CloseScreenFromGame(Drone p_drone = null, DroneRigData p_rigData = null)
		{
			base.app.view.ui.screens.Close("garage-rig-selection-screen");
			base.app.controller.game.input.controller.Pause(p_flag: false, p_pause_physics: false);
			Notify("game.unpause");
			if (p_drone != null && p_rigData != null)
			{
				Notify("garage.edit.fly.ready", p_rigData, p_drone);
			}
			RunOnce(0.05f, delegate
			{
				Notify("game.ui.dashboard@hide");
			});
		}

		protected void PopulateCards()
		{
			List<UICardView> createCards = view.createCards;
			for (int i = 0; i < createCards.Count; i++)
			{
				createCards[i].gameObject.SetActive(value: false);
				createCards[i].transform.SetParent(base.transform);
			}
			List<ListComponent> list = new List<ListComponent>();
			for (int j = 0; j < view.grids.Count; j++)
			{
				ListComponent listComponent = view.grids[j];
				listComponent.Clear();
				list.Add(listComponent);
			}
			List<DroneRigData> list2 = new List<DroneRigData>();
			if (view.overrideList != null)
			{
				list2.AddRange(view.overrideList);
			}
			else if (!view.openedAsTemplateSelector)
			{
				list2.AddRange(model.rigs);
			}
			if (view.overrideSizes != null && view.overrideSizes.Count > 0)
			{
				for (int k = 0; k < list2.Count; k++)
				{
					if (!view.overrideSizes.Contains(list2[k].diameter))
					{
						list2.RemoveAt(k--);
					}
				}
			}
			if (!view.allowCustomPhysics)
			{
				for (int l = 0; l < list2.Count; l++)
				{
					if (list2[l] != null && !string.IsNullOrEmpty(list2[l].tune))
					{
						list2.RemoveAt(l--);
					}
				}
			}
			int count = list2.Count;
			if (view.overrideList == null && (view.selectionOnly || view.loadOriginalRigs))
			{
				List<DroneRigData> originalRigs = model.GetOriginalRigs();
				for (int m = 0; m < originalRigs.Count; m++)
				{
					if (!view.unlockedRigsOnly || !originalRigs[m].isLocked)
					{
						string frame = originalRigs[m].frame;
						DroneFrame droneFrame = base.app.model.storage.library.FindByGUID<DroneFrame>(frame);
						DRLAsset.Info info = (droneFrame ? droneFrame.info : null);
						originalRigs[m].name = ((info == null) ? "UNKNOWN" : info.name);
						int num = ((view.overrideSizes != null) ? view.overrideSizes.Count : 0);
						bool flag = view.overrideSizes != null && view.overrideSizes.Contains(originalRigs[m].diameter);
						if ((num <= 0 || flag) && (!view.openedAsTemplateSelector || originalRigs[m].diameter > 1))
						{
							list2.Add(originalRigs[m]);
						}
					}
				}
			}
			bool flag2 = base.app.level.IsLevelLoaded("game");
			List<string> list3 = new List<string>();
			if (view.promoList != null)
			{
				for (int n = 0; n < view.promoList.Count; n++)
				{
					list3.Add(view.promoList[n].guid);
				}
			}
			if (list2 != null && list2.Count > 0)
			{
				for (int num2 = 0; num2 < list2.Count; num2++)
				{
					DroneRigData droneRigData = list2[num2];
					if (list3.Contains(droneRigData.guid))
					{
						continue;
					}
					int index = Mathf.Clamp(droneRigData.diameter - 1, 1, 6);
					ListComponent listComponent2 = list[index];
					UICardButtonDroneRig uICardButtonDroneRig = listComponent2.Push<UICardButtonDroneRig>();
					uICardButtonDroneRig.CloseMenu();
					if (num2 < count)
					{
						uICardButtonDroneRig.defaultRig = false;
					}
					else
					{
						uICardButtonDroneRig.defaultRig = true;
					}
					uICardButtonDroneRig.model = model;
					uICardButtonDroneRig.Set(droneRigData);
					uICardButtonDroneRig.selected = false;
					if (view.selectionOnly || view.loadOriginalRigs)
					{
						uICardButtonDroneRig.selected = base.app.model.storage.state.player.garage.currentRigData == droneRigData;
					}
					uICardButtonDroneRig.name = (listComponent2.Count - 1).ToString("00");
					Transform transform = uICardButtonDroneRig.transform.Find("content");
					if ((bool)transform)
					{
						transform = transform.Find("menu");
					}
					if ((bool)transform)
					{
						Transform transform2 = transform.Find("fly");
						Transform transform3 = transform.Find("edit");
						Transform transform4 = transform.Find("delete");
						if ((bool)transform2 && flag2)
						{
							transform2.gameObject.SetActive(value: true);
						}
						else
						{
							transform2.gameObject.SetActive(value: false);
						}
						if (uICardButtonDroneRig.defaultRig)
						{
							transform3.gameObject.SetActive(value: false);
							transform4.gameObject.SetActive(value: false);
						}
						else
						{
							transform3.gameObject.SetActive(value: false);
							transform4.gameObject.SetActive(value: true);
						}
					}
				}
			}
			else
			{
				for (int num3 = 1; num3 < view.gridContainers.Count; num3++)
				{
					view.gridContainers[num3].SetActive(value: false);
				}
			}
			view.gridContainers[1].SetActive(list[1].Count > 0);
			if (view.promoList != null && view.promoList.Count > 0)
			{
				view.gridContainers[0].SetActive(value: true);
				for (int num4 = 0; num4 < view.promoList.Count; num4++)
				{
					DroneRigData droneRigData2 = view.promoList[num4];
					ListComponent listComponent3 = list[0];
					UICardButtonDroneRig uICardButtonDroneRig2 = listComponent3.Push<UICardButtonDroneRig>();
					uICardButtonDroneRig2.defaultRig = true;
					uICardButtonDroneRig2.model = model;
					uICardButtonDroneRig2.Set(droneRigData2);
					uICardButtonDroneRig2.selected = false;
					if (view.selectionOnly || view.loadOriginalRigs)
					{
						uICardButtonDroneRig2.selected = base.app.model.storage.state.player.garage.currentRigData == droneRigData2;
					}
					uICardButtonDroneRig2.name = (listComponent3.Count - 1).ToString("00");
					Transform transform5 = uICardButtonDroneRig2.transform.Find("content");
					if ((bool)transform5)
					{
						transform5 = transform5.Find("menu");
					}
					if ((bool)transform5)
					{
						Transform transform6 = transform5.Find("fly");
						Transform transform7 = transform5.Find("edit");
						Transform transform8 = transform5.Find("delete");
						if ((bool)transform6 && flag2)
						{
							transform6.gameObject.SetActive(value: true);
						}
						else
						{
							transform6.gameObject.SetActive(value: false);
						}
						if (uICardButtonDroneRig2.defaultRig)
						{
							transform7.gameObject.SetActive(value: false);
							transform8.gameObject.SetActive(value: false);
						}
						else
						{
							transform7.gameObject.SetActive(value: false);
							transform8.gameObject.SetActive(value: true);
						}
					}
				}
			}
			else
			{
				view.gridContainers[0].SetActive(value: false);
			}
			view.gridContainers[view.gridContainers.Count - 1].SetActive(value: false);
			LinkCards();
			for (int num5 = 0; num5 < view.gridContainers.Count; num5++)
			{
				view.gridContainers[num5].SetActive(list[num5].Count > 0);
			}
			for (int num6 = 0; num6 < list.Count; num6++)
			{
				ListComponent listComponent4 = list[num6];
				createCards[num6].gameObject.SetActive(listComponent4.Count <= 0);
				createCards[num6].GetComponent<CanvasGroup>().alpha = 0f;
				createCards[num6].GetComponent<UINavigation>().enabled = false;
				listComponent4.Insert(0, createCards[num6]);
			}
			this.TimerRunOnce(delegate
			{
				UINavigation.Focus(view.gridsContainer);
			}, 0.1f);
		}

		protected void LinkCards()
		{
			List<ListComponent> list = new List<ListComponent>();
			List<UINavigation> list2 = new List<UINavigation>();
			for (int i = 0; i < view.grids.Count; i++)
			{
				ListComponent listComponent = view.grids[i];
				if (listComponent.isActiveAndEnabled)
				{
					list.Add(listComponent);
					list2.Add(view.createDroneButtons[i]);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				ListComponent listComponent2 = ((j <= 0) ? null : list[j - 1]);
				List<Component> list3 = (listComponent2 ? listComponent2.list : null);
				int num = list3?.Count ?? 0;
				ListComponent listComponent3 = list[j];
				UINavigation.Link(p_left: (j <= 0) ? view.leftNavigation : ((list3 == null) ? null : ((num <= 0) ? list2[j - 1] : list3[num - 1])), p_right: (j >= list.Count - 1) ? null : ((list[j + 1].Count > 0) ? ((MonoBehaviour)list[j + 1]) : ((MonoBehaviour)list2[j + 1])), p_layout: listComponent3.GetComponent<LayoutGroup>(), p_up: list2[j]);
			}
		}

		private void Update()
		{
			if (view.droneSpecsPanel == null)
			{
				return;
			}
			Transform transform = view.droneSpecsPanel.transform;
			if (view.allowCreation)
			{
				foreach (UINavigation createDroneButton in view.createDroneButtons)
				{
					CanvasGroup component = createDroneButton.GetComponent<CanvasGroup>();
					if (!(component == null))
					{
						component.alpha = (0f - transform.InverseTransformPoint(component.transform.position).x - 380f) / 520f;
					}
				}
				return;
			}
			foreach (RectTransform gridHeader in view.gridHeaders)
			{
				CanvasGroup component2 = gridHeader.GetComponent<CanvasGroup>();
				if (!(component2 == null))
				{
					component2.alpha = (0f - transform.InverseTransformPoint(component2.transform.position).x - 380f) / 520f;
				}
			}
		}
	}
}
