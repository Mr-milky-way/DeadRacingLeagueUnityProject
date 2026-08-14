using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.network;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMultiplayerRoomView : UIScreenView
	{
		public NetworkRoom room;

		[Header("Room")]
		public Image roomNameInputIcon;

		public DRLInputFieldView roomNameInput;

		public DRLInputFieldView roomPasswordInput;

		public DRLStepperView maxRacerStepper;

		public DRLStepperView maxSpectatorStepper;

		public DRLToggleView ghostsAllowedToggle;

		public DRLStepperView roomPrivacyStepper;

		public UIStatusView roomStatusField;

		public UIElementView inviteButton;

		public GameObject privacyRoomIcon;

		public List<UINavigation> rightButtons;

		public GameObject backButton;

		public UIElementView startButton;

		[Header("Game")]
		public DRLStepperView raceTimeLimit;

		public DRLStepperView droneClassSelector;

		public DRLToggleView drlPilotModeToggle;

		public GameObject droneClassLabel;

		public UIElementView droneSelector;

		public RectTransform droneSelectorInfo;

		public RawImage droneSelectorThumb;

		public Text droneSelectorName;

		public Text droneSelectorClass;

		public Image droneSelectorClassBG;

		public DRLStepperView campaignStepper;

		public DRLDropdownView voteOptionsDropdown;

		public DRLDropdownView timeoutModeDropdown;

		public DRLDropdownView armAndTurtleDropdown;

		[Header("Map")]
		public RawImage mapCard;

		public RawImage trackCard;

		public RawImage mapPreviewCard;

		public RawImage trackPreviewCard;

		public Texture GrayGradient;

		public Text mapName;

		public Text trackName;

		public UIElementView mapSelector;

		[Header("Racers")]
		public ListComponent racerGridField;

		public GridLayoutGroup racerGridGroup;

		public ListComponent spectatorGridField;

		public UINavigation startNav;

		public UINavigation stateNav;

		public UINavigation timerNav;

		public DRLToggleView readyToggleNav;

		public Image readyToggleYesBG;

		public Image readyToggleNoBG;

		public Text readyToggleText;

		public UINavigation exitNav;

		public Text timerNavClockField;

		public Text timerNavLabelField;

		[Header("Navigation")]
		public UINavigationLinkList racersRightProxy;

		public UINavigationLinkList racersUpProxy;

		public List<UINavigation> spectatorNavs;

		[Header("vDRL")]
		public DRLInputFieldView tournamentGUIDField;

		public Image tournamentGUIDOutlineField;

		public DRLStepperView tournamentHeatField;

		public Image tournamentHeatOutlineField;

		public UIElementView tournamentNav;

		public List<Sprite> badgeSprites;

		public const int AllDronesClass = 100;

		public const int SpecificDroneClass = 101;

		public const int NanoDronesClass = 102;

		[HideInInspector]
		public string customMapPhotoURL;

		[HideInInspector]
		public bool leaveRoomOnExit;

		private bool m_has_init;

		private bool m_specDroneThumbInit;

		private WebAsyncRequest m_droneThumbLoader;

		[SerializeField]
		private FadeComponent m_mapThumbFader;

		[SerializeField]
		private FadeComponent m_trackThumbFader;

		[SerializeField]
		private FadeComponent m_trackPreviewThumbFader;

		public GameFlag gameType;

		public bool isMaster;

		public List<NetworkActor> racers;

		public List<NetworkActor> spectators;

		public List<NetworkGhost> ghosts;

		private bool m_roomDirty;

		public int selectedDroneClass;

		public string selectedDroneRigData;

		public MapData customMap;

		public DRLMap map;

		public DRLMapTrack track;

		public DRLCampaign campaign;

		public DRLDroneRig drone;

		private bool m_isRefreshingRoom;

		private string m_activeGUID;

		public float droneInfoWidth
		{
			get
			{
				return droneSelectorInfo.sizeDelta.x;
			}
			set
			{
				Vector2 sizeDelta = droneSelectorInfo.sizeDelta;
				sizeDelta.x = value;
				droneSelectorInfo.sizeDelta = sizeDelta;
			}
		}

		public Texture droneThumb
		{
			set
			{
				if ((bool)droneSelectorThumb)
				{
					droneSelectorThumb.texture = value;
				}
				if ((bool)droneSelectorThumb)
				{
					droneSelectorThumb.color = ((value != null) ? Color.white : Colorf.transparent);
				}
			}
		}

		public string droneName
		{
			set
			{
				droneSelectorName.text = value;
			}
		}

		public int droneClass
		{
			set
			{
				if ((bool)droneSelectorClass)
				{
					droneSelectorClass.text = value + "\"";
				}
				if ((bool)droneSelectorClassBG)
				{
					droneSelectorClassBG.color = DRLColor.classColors[value];
				}
			}
		}

		public Texture mapThumb
		{
			set
			{
				if ((bool)mapCard)
				{
					mapCard.texture = value;
				}
				if ((bool)mapCard)
				{
					mapCard.color = ((value != null) ? Color.white : Colorf.transparent);
				}
			}
		}

		public Texture trackThumb
		{
			set
			{
				if ((bool)trackCard)
				{
					trackCard.texture = value;
				}
				if ((bool)trackCard)
				{
					trackCard.color = ((value != null) ? Color.white : Colorf.transparent);
				}
			}
		}

		private FadeComponent mapThumbFader
		{
			get
			{
				if (!m_mapThumbFader)
				{
					m_mapThumbFader = (mapCard ? mapCard.GetComponent<FadeComponent>() : null);
				}
				return m_mapThumbFader;
			}
		}

		private FadeComponent trackThumbFader
		{
			get
			{
				if (!m_trackThumbFader)
				{
					m_trackThumbFader = (trackCard ? trackCard.GetComponent<FadeComponent>() : null);
				}
				return m_trackThumbFader;
			}
		}

		private FadeComponent trackPreviewThumbFader
		{
			get
			{
				if (!m_trackPreviewThumbFader)
				{
					m_trackPreviewThumbFader = (trackPreviewCard ? trackPreviewCard.GetComponent<FadeComponent>() : null);
				}
				return m_trackPreviewThumbFader;
			}
		}

		public bool userCommunicationAllowed => true;

		private int lobbyCountdown
		{
			set
			{
				timerNavClockField.text = value.ToString("00");
			}
		}

		private int maxRacer
		{
			set
			{
				int num = Mathf.Max(0, value - 2);
				if (num != maxRacerStepper.index)
				{
					maxRacerStepper.index = num;
					maxRacerStepper.Refresh();
				}
				RefreshAvailableSlots(p_racers: true, value);
			}
		}

		private int maxSpectator
		{
			set
			{
				int num = Mathf.Max(0, value);
				if (num != maxSpectatorStepper.index)
				{
					maxSpectatorStepper.index = num;
					maxSpectatorStepper.Refresh();
				}
				RefreshAvailableSlots(p_racers: false, value);
			}
		}

		public bool allowGhosts
		{
			get
			{
				return ghostsAllowedToggle.toggle.isOn;
			}
			private set
			{
				ghostsAllowedToggle.toggle.isOn = value;
			}
		}

		public bool allowVote
		{
			get
			{
				return IsVoteAllowed();
			}
			set
			{
				if (!value)
				{
					voteOptionsDropdown.Select("OFF");
					voteOptionsDropdown.interactable = false;
				}
			}
		}

		public string customMapGUID
		{
			get
			{
				if (customMap == null)
				{
					return "";
				}
				return customMap.guid;
			}
		}

		public string mapGUID
		{
			get
			{
				if (!map)
				{
					return "";
				}
				return map.guid;
			}
		}

		public string trackGUID
		{
			get
			{
				if (!track)
				{
					return "";
				}
				return track.guid;
			}
		}

		public string campaignGUID
		{
			get
			{
				if (!campaign)
				{
					return "";
				}
				return campaign.guid;
			}
		}

		public string droneGUID
		{
			get
			{
				if (!drone)
				{
					return "";
				}
				return drone.guid;
			}
		}

		public void Clear()
		{
			racers = new List<NetworkActor>();
			spectators = new List<NetworkActor>();
			ghosts = new List<NetworkGhost>();
			ListComponent listComponent = racerGridField;
			for (int i = 0; i < listComponent.Count; i++)
			{
				UIMultiplayerRoomItemView uIMultiplayerRoomItemView = listComponent.Get<UIMultiplayerRoomItemView>(i);
				if ((bool)uIMultiplayerRoomItemView)
				{
					uIMultiplayerRoomItemView.Clear();
				}
			}
			listComponent = spectatorGridField;
			for (int j = 0; j < listComponent.Count; j++)
			{
				UIMultiplayerRoomItemView uIMultiplayerRoomItemView2 = listComponent.Get<UIMultiplayerRoomItemView>(j);
				if ((bool)uIMultiplayerRoomItemView2)
				{
					uIMultiplayerRoomItemView2.Clear();
				}
			}
			readyToggleNav.toggle.isOn = false;
			RefreshReadyToggle();
			m_specDroneThumbInit = false;
		}

		public int FindItemIndex(UIMultiplayerRoomItemView p_item, bool p_is_spectator)
		{
			if (!p_item)
			{
				return -1;
			}
			ListComponent listComponent = (p_is_spectator ? spectatorGridField : racerGridField);
			for (int i = 0; i < listComponent.Count; i++)
			{
				UIMultiplayerRoomItemView uIMultiplayerRoomItemView = listComponent.Get<UIMultiplayerRoomItemView>(i);
				if ((bool)uIMultiplayerRoomItemView && uIMultiplayerRoomItemView == p_item && uIMultiplayerRoomItemView.IsAvailable())
				{
					return i;
				}
			}
			return -1;
		}

		public void SetTournamentActive(bool p_flag)
		{
			tournamentGUIDOutlineField.color = (p_flag ? DRLColor.green : DRLColor.yellowDark);
			tournamentHeatOutlineField.color = (p_flag ? DRLColor.green : DRLColor.yellowDark);
			if (tournamentNav != null)
			{
				tournamentNav.interactable = p_flag;
			}
		}

		public void EnableTournamentControls(bool p_enable)
		{
			if (tournamentHeatField != null)
			{
				tournamentHeatField.interactable = p_enable;
			}
			if (tournamentGUIDField != null)
			{
				tournamentGUIDField.interactable = p_enable;
			}
			if ((bool)readyToggleNav)
			{
				readyToggleNav.gameObject.SetActive(value: false);
			}
		}

		private void RefreshAvailableSlots(bool p_racers, int p_max_slots)
		{
			ListComponent listComponent = (p_racers ? racerGridField : spectatorGridField);
			for (int i = 0; i < listComponent.Count; i++)
			{
				UIMultiplayerRoomItemView uIMultiplayerRoomItemView = listComponent.Get<UIMultiplayerRoomItemView>(i);
				if (!uIMultiplayerRoomItemView)
				{
					continue;
				}
				if (i < p_max_slots)
				{
					if (!uIMultiplayerRoomItemView.IsAvailable())
					{
						uIMultiplayerRoomItemView.SetAvailable(p_available: true);
					}
				}
				else if (uIMultiplayerRoomItemView.IsAvailable())
				{
					uIMultiplayerRoomItemView.SetAvailable(p_available: false);
				}
			}
		}

		public void RefreshReadyToggle()
		{
			if ((bool)readyToggleNav)
			{
				bool isOn = readyToggleNav.toggle.isOn;
				readyToggleYesBG.gameObject.SetActive(isOn);
				readyToggleNoBG.gameObject.SetActive(!isOn);
			}
		}

		private bool IsVoteAllowed()
		{
			return voteOptionsDropdown.Value().text == "OFF";
		}

		public bool IsRandomMapMode()
		{
			return voteOptionsDropdown.Value().text == "RANDOM";
		}

		private void RefreshRightSideNavButtons(NetworkRoom p_room, bool p_is_master)
		{
			if (p_room == null)
			{
				return;
			}
			if (p_room.IsQuickMatch)
			{
				startNav.gameObject.SetActive(value: false);
				readyToggleNav.gameObject.SetActive(value: false);
				timerNav.gameObject.SetActive(gameType == GameFlag.Race);
				bool flag = room.CanSpectate && racers.Count > 2;
				EnablePlayerStateButton(room.IsSpectator ? room.CanRace : flag);
				return;
			}
			timerNav.gameObject.SetActive(room.IsTournamentMatch && room.LobbyCountdownAllowed);
			if (gameType == GameFlag.Race)
			{
				bool flag2 = p_room.State == NetworkRoom.StateCode.MatchMaking;
				bool active = p_is_master && flag2;
				startNav.gameObject.SetActive(active);
				bool flag3 = flag2 && !room.IsSpectator;
				if (room.IsTournamentMatch)
				{
					flag3 = false;
					readyToggleNav.gameObject.SetActive(racers.Count == room.MaxRacers && flag3);
				}
				else
				{
					readyToggleNav.gameObject.SetActive(racers.Count >= 2 && flag3);
				}
			}
			else
			{
				startNav.gameObject.SetActive(p_is_master);
			}
			bool active2 = p_is_master && room.CanRace && !room.IsTournamentMatch && !room.IsPrivate && userCommunicationAllowed;
			inviteButton.gameObject.SetActive(active2);
			bool flag4 = room.CanSpectate && racers.Count >= 2;
			EnablePlayerStateButton((room.IsSpectator ? room.CanRace : flag4) && !room.IsTournamentMatch);
			RefreshRightButtonsNavigation();
		}

		public void EnablePlayerStateButton(bool p_enable)
		{
			UIElementView component = stateNav.gameObject.GetComponent<UIElementView>();
			if ((bool)component)
			{
				component.interactable = p_enable;
			}
		}

		public void SetExitButtonEnabled(bool p_enabled)
		{
			exitNav.gameObject.SetActive(p_enabled);
		}

		public UIMultiplayerRoomItemView SetPlayer(ListComponent p_list, int p_id, string p_name, Color p_color, bool p_master, bool p_ready, object p_photo, bool p_animate, bool p_newThumbnails, float p_delay = 0f, Sprite p_badge = null, bool p_is_ghost = false, string p_platform = "undefined", string p_droneThumbURL = "", bool p_isSpectator = false)
		{
			UIMultiplayerRoomItemView it = p_list.Get<UIMultiplayerRoomItemView>(p_id);
			if (!it)
			{
				return null;
			}
			bool context_menu_enabled = true;
			if (room != null && room.IsQuickMatch)
			{
				p_master = false;
				context_menu_enabled = false;
			}
			it.droneThumbURL = p_droneThumbURL;
			if (p_delay <= 0f)
			{
				it.Set(p_name, p_color, p_master, p_ready, p_photo, p_animate, p_newThumbnails, p_badge, p_is_ghost, p_platform, p_isSpectator);
				it.SetContextMenuEnabled(context_menu_enabled);
				return it;
			}
			RunOnce(delegate
			{
				it.Set(p_name, p_color, p_master, p_ready, p_photo, p_animate, p_newThumbnails, p_badge, p_is_ghost, p_platform, p_isSpectator);
				it.SetContextMenuEnabled(context_menu_enabled);
			}, p_delay);
			return it;
		}

		private UIMultiplayerRoomItemView SetRacer(int p_id, string p_name, Color p_color, bool p_master, bool p_ready, object p_photo, bool p_newThumbnails, bool p_animate = false, float p_delay = 0f, Sprite badge = null, string p_platfrom = "undefined", string p_droneThumbURL = "")
		{
			return SetPlayer(racerGridField, p_id, p_name, p_color, p_master, p_ready, p_photo, p_animate, p_newThumbnails, p_delay, badge, p_is_ghost: false, p_platfrom, p_droneThumbURL);
		}

		private UIMultiplayerRoomItemView SetSpectator(int p_id, string p_name, Color p_color, bool p_master, object p_photo, bool p_animate = false, float p_delay = 0f, Sprite badge = null, string p_platfrom = "undefined")
		{
			return SetPlayer(spectatorGridField, p_id, p_name, p_color, p_master, p_ready: true, p_photo, p_animate, p_newThumbnails: false, p_delay, badge, p_is_ghost: false, p_platfrom, "", p_isSpectator: true);
		}

		private UIMultiplayerRoomItemView SetGhost(int p_id, string p_name, Color p_color, object p_photo, bool p_newThumbnails, float p_delay = 0f, string p_droneThumbURL = "")
		{
			return SetPlayer(racerGridField, p_id, p_name, p_color, p_master: false, p_ready: false, p_photo, p_animate: true, p_newThumbnails, p_delay, null, p_is_ghost: true, "undefined", p_droneThumbURL);
		}

		private UIMultiplayerRoomItemView SetRacer(int p_id, NetworkActor p_target, bool p_animate = false, float p_delay = 0f)
		{
			if (p_target == null)
			{
				return null;
			}
			Sprite badge = null;
			if (p_target.BadgeLevel > 0 && p_target.BadgeLevel <= badgeSprites.Count)
			{
				badge = badgeSprites[p_target.BadgeLevel - 1];
			}
			string playerId = p_target.PlayerId;
			string text = (p_target.IsLocal ? base.app.model.storage.state.player.garage.currentRigData.thumb1 : DroneRigData.FromJson(p_target.DroneRigData).thumb1);
			if (string.IsNullOrEmpty(text))
			{
				DroneRigData droneRigData = DroneRigData.FromJson(p_target.DroneRigData);
				if (droneRigData != null)
				{
					text = base.app.model.storage.state.player.garage.GetClonedOriginalbyFrame(droneRigData.frame).thumb1;
				}
			}
			UIMultiplayerRoomItemView uIMultiplayerRoomItemView = SetRacer(p_id, p_target.ProfileName, p_target.MainColor, p_target.IsMaster, p_target.IsRoomReady, playerId, p_newThumbnails: true, p_animate, p_delay, badge, p_target.Platform, text);
			uIMultiplayerRoomItemView.data = p_target;
			return uIMultiplayerRoomItemView;
		}

		private UIMultiplayerRoomItemView SetSpectator(int p_id, NetworkActor p_target, bool p_animate = false, float p_delay = 0f)
		{
			if (p_target == null)
			{
				return null;
			}
			string playerId = p_target.PlayerId;
			UIMultiplayerRoomItemView uIMultiplayerRoomItemView = SetSpectator(p_id, p_target.ProfileName, p_target.ProfileColor, p_target.IsMaster, playerId, p_animate, p_delay, null, p_target.Platform);
			uIMultiplayerRoomItemView.data = p_target;
			return uIMultiplayerRoomItemView;
		}

		private UIMultiplayerRoomItemView SetGhost(int p_id, NetworkGhost p_target)
		{
			if (p_target == null)
			{
				return null;
			}
			string playerId = p_target.PlayerId;
			string p_droneThumbURL = p_target.DronePhoto;
			bool p_newThumbnails = false;
			if (!string.IsNullOrEmpty(p_target.DroneRig))
			{
				DroneRigData droneRigData = DroneRigData.FromJson(p_target.DroneRig);
				DroneRigData originalByFrame = base.app.model.storage.state.player.garage.GetOriginalByFrame(droneRigData.frame);
				if (originalByFrame != null)
				{
					p_droneThumbURL = originalByFrame.thumb1;
					p_newThumbnails = true;
				}
			}
			UIMultiplayerRoomItemView uIMultiplayerRoomItemView = SetGhost(p_id, p_target.ProfileName, p_target.GetProfileColor(), playerId, p_newThumbnails, 0f, p_droneThumbURL);
			uIMultiplayerRoomItemView.data = p_target;
			return uIMultiplayerRoomItemView;
		}

		public UIMultiplayerRoomItemView SetPlayer(int p_id, NetworkActor p_target, bool p_animate = false, float p_delay = 0f)
		{
			if (p_target == null)
			{
				return null;
			}
			string playerId = p_target.PlayerId;
			string text = (p_target.IsLocal ? base.app.model.storage.state.player.garage.currentRigData.thumb1 : DroneRigData.FromJson(p_target.DroneRigData).thumb1);
			if (string.IsNullOrEmpty(text))
			{
				DroneRigData droneRigData = DroneRigData.FromJson(p_target.DroneRigData);
				if (droneRigData != null)
				{
					text = base.app.model.storage.state.player.garage.GetClonedOriginalbyFrame(droneRigData.frame).thumb1;
				}
			}
			if (p_target.IsSpectator)
			{
				UIMultiplayerRoomItemView uIMultiplayerRoomItemView = SetSpectator(p_id, p_target.ProfileName, p_target.ProfileColor, p_target.IsMaster, playerId, p_animate, p_delay, null, p_target.Platform);
				uIMultiplayerRoomItemView.data = p_target;
				return uIMultiplayerRoomItemView;
			}
			UIMultiplayerRoomItemView uIMultiplayerRoomItemView2 = SetRacer(p_id, p_target.ProfileName, p_target.MainColor, p_target.IsMaster, p_target.IsRoomReady, playerId, p_animate, p_animate: true, p_delay, null, p_target.Platform, text);
			uIMultiplayerRoomItemView2.data = p_target;
			return uIMultiplayerRoomItemView2;
		}

		public void MasterInit(int p_drone_class, string p_selected_drone_rig_data)
		{
			map = null;
			customMap = null;
			bool flag = false;
			GarageStateModel garage = base.app.model.storage.state.player.garage;
			Debug.Log("UIMultiplayerRoomView> MasterInit\nselectedDroneRigData\n   " + selectedDroneRigData + "\np_selected_drone_rig_data\n  " + p_selected_drone_rig_data);
			if (selectedDroneRigData != p_selected_drone_rig_data)
			{
				selectedDroneRigData = p_selected_drone_rig_data;
				int p_index = -1;
				DroneRigData rigByGUID = garage.GetRigByGUID(p_selected_drone_rig_data, out p_index);
				if (rigByGUID == null)
				{
					Debug.LogWarning("UIMultiplayerRoomView> OnNotification / Failed finding DroneRigData for [" + p_selected_drone_rig_data + "] trying original drone.");
				}
				DroneRigData droneRigData = ((rigByGUID == null) ? garage.GetOriginalByGUID(p_selected_drone_rig_data) : rigByGUID);
				if (droneRigData == null)
				{
					Debug.LogWarning("UIMultiplayerRoomView> OnNotification / Couldn't find default initial drone guid:" + p_selected_drone_rig_data + "}");
					droneRigData = garage.GetFirstOriginalRigWithDiameter(garage.currentRigData.diameter);
				}
				if (droneRigData == null)
				{
					Debug.LogWarning("UIMultiplayerRoomView> OnNotification / Couldn't get a fallback drone");
				}
				garage.currentRigData = droneRigData;
				base.app.model.network.room.Local.DroneRigData = ((droneRigData == null) ? "" : droneRigData.ToJson());
				flag = true;
				DroneRigData currentRigData = base.app.model.storage.state.player.garage.currentRigData;
				if (currentRigData != null)
				{
					SetSpecificDroneData(currentRigData);
				}
			}
			if (selectedDroneClass != p_drone_class)
			{
				selectedDroneClass = p_drone_class;
				flag = true;
				RefreshDroneClassSelectorStepper();
			}
			if (flag)
			{
				UpdateDrone();
			}
		}

		public void Set(NetworkRoom p_room, bool p_force = false)
		{
			m_roomDirty = true;
			PlatformService ps = base.app.model.service.platform;
			m_isRefreshingRoom = m_isRefreshingRoom && !p_force;
			if (m_isRefreshingRoom)
			{
				return;
			}
			m_isRefreshingRoom = true;
			this.TimerRunOnce(delegate
			{
				if (!base.validContext)
				{
					m_isRefreshingRoom = false;
				}
				else if (!m_roomDirty)
				{
					m_isRefreshingRoom = false;
				}
				else if (p_room == null)
				{
					m_isRefreshingRoom = false;
				}
				else
				{
					m_roomDirty = false;
					room = p_room;
					if (!isMaster)
					{
						ps.TextValidate(room.RoomTitle, delegate(bool p_result, string p_value)
						{
							if (!roomNameInput.IsEditing)
							{
								roomNameInput.field.text = (p_result ? p_value : "ROOM");
							}
						});
					}
					maxRacer = room.MaxRacers;
					maxSpectator = room.MaxSpectators;
					lobbyCountdown = Mathf.Max(0, room.LobbyCountdown);
					allowVote = room.AllowMapVoting;
					allowGhosts = room.AllowGhosts;
					drlPilotModeToggle.isOn = room.DRLPilotMode;
					RefreshPrivacyStepper();
					RefreshMapVoteOptions();
					RefreshArmAndTurtle();
					SetTimeLimit();
					if (room.UsingCustomMap)
					{
						UpdateCustomMap(room.CustomMapId, room.CustomMapName);
					}
					else
					{
						UpdateMap(room.MapId);
						UpdateMapTrack(room.TrackId);
					}
					if (room.DroneClass == 0)
					{
						room.DroneClass = 100;
					}
					if (!room.IsMaster)
					{
						if (selectedDroneClass != room.DroneClass)
						{
							selectedDroneClass = room.DroneClass;
							UpdateDrone();
						}
						RefreshDroneClassSelectorStepper();
						UpdateSpecificDrone();
					}
					if (room.Local != null)
					{
						SetPlayerMode(room.Local.IsSpectator);
					}
					UpdatePlayerList(room.Racers, p_spectators: false, p_force);
					UpdatePlayerList(room.Spectators, p_spectators: true, p_force);
					ClearUnusedCards();
					SetAvailableOptions(room.IsMaster, room.IsTournamentMatch);
					m_isRefreshingRoom = false;
				}
			}, 0.7f);
		}

		private void RefreshArmAndTurtle()
		{
			if (armAndTurtleDropdown.gameObject.activeInHierarchy)
			{
				int p_option = (room.ArmAndTurtle ? 1 : 0);
				armAndTurtleDropdown.Select(p_option);
			}
		}

		private void RefreshRightButtonsNavigation()
		{
			for (int i = 0; i < rightButtons.Count; i++)
			{
				if (!rightButtons[i].gameObject.activeInHierarchy || i >= rightButtons.Count - 1)
				{
					continue;
				}
				for (int j = i + 1; j < rightButtons.Count; j++)
				{
					if (rightButtons[j].gameObject.activeInHierarchy)
					{
						rightButtons[i].down = rightButtons[j];
						rightButtons[j].up = rightButtons[i];
						break;
					}
				}
			}
		}

		private void RefreshMapVoteOptions()
		{
			int mapVotingCategory = (int)room.MapVotingCategory;
			if (mapVotingCategory != voteOptionsDropdown.dropdown.value)
			{
				voteOptionsDropdown.Select(mapVotingCategory);
			}
		}

		public void OnSwapModeActivated(UIMultiplayerRoomItemView p_item)
		{
			if ((bool)p_item)
			{
				bool isSpectator = p_item.data.IsSpectator;
				int num = 0;
				for (int i = 0; i < 2; i++)
				{
					ListComponent listComponent = ((i == 0) ? racerGridField : spectatorGridField);
					for (int j = 0; j < listComponent.Count; j++)
					{
						UIMultiplayerRoomItemView uIMultiplayerRoomItemView = listComponent.Get<UIMultiplayerRoomItemView>(j);
						if ((bool)uIMultiplayerRoomItemView && uIMultiplayerRoomItemView.IsAvailable())
						{
							bool flag = true;
							if (i == 0 && !isSpectator)
							{
								flag = uIMultiplayerRoomItemView.IsTaken();
							}
							if (i == 1 && isSpectator)
							{
								flag = uIMultiplayerRoomItemView.IsTaken();
							}
							if (flag)
							{
								uIMultiplayerRoomItemView.SetAsPotentialSwapSlot(p_yes: true);
							}
							if (i == 1)
							{
								num++;
							}
							if ((i == 1 && !isSpectator && room != null && room.SpectatorsCount == num && room.RacersCount == 1) || (i == 0 && isSpectator && !uIMultiplayerRoomItemView.IsTaken()) || (i == 1 && !isSpectator && !uIMultiplayerRoomItemView.IsTaken()))
							{
								break;
							}
						}
					}
				}
				p_item.SetForSwapping(p_yes: true);
				return;
			}
			for (int k = 0; k < 2; k++)
			{
				ListComponent listComponent2 = ((k == 0) ? racerGridField : spectatorGridField);
				for (int l = 0; l < listComponent2.Count; l++)
				{
					UIMultiplayerRoomItemView uIMultiplayerRoomItemView2 = listComponent2.Get<UIMultiplayerRoomItemView>(l);
					if ((bool)uIMultiplayerRoomItemView2)
					{
						uIMultiplayerRoomItemView2.SetForSwapping(p_yes: false);
					}
				}
			}
		}

		public void UpdatePlayerList(List<NetworkActor> p_players, bool p_spectators, bool p_force)
		{
			List<NetworkActor> list = new List<NetworkActor>((p_players == null) ? new List<NetworkActor>() : p_players);
			List<NetworkActor> list2 = new List<NetworkActor>();
			List<NetworkActor> list3 = new List<NetworkActor>();
			List<NetworkActor> list4 = (p_spectators ? spectators : racers);
			for (int i = 0; i < list.Count; i++)
			{
				if (!ContainsPlayer(list4, list[i]))
				{
					list2.Add(list[i]);
				}
			}
			for (int j = 0; j < list4.Count; j++)
			{
				if (!ContainsPlayer(list, list4[j]))
				{
					list3.Add(list4[j]);
				}
			}
			bool flag = list2.Count > 0 || list3.Count > 0 || p_force;
			for (int k = 0; k < list3.Count; k++)
			{
				RemovePlayer(list3[k], p_spectators);
			}
			int num = 0;
			int num2 = 0;
			for (int l = 0; l < list2.Count; l++)
			{
				AddPlayer(list2[l], p_spectators);
			}
			list = list4;
			list.Sort(delegate(NetworkActor a, NetworkActor b)
			{
				if (a.Order > b.Order)
				{
					return 1;
				}
				return (a.Order < b.Order) ? (-1) : 0;
			});
			if (!flag)
			{
				return;
			}
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				NetworkActor networkActor = list[num3];
				bool p_animate = false;
				for (int num4 = 0; num4 < list2.Count; num4++)
				{
					p_animate = networkActor == list2[num4];
				}
				if (networkActor.IsSpectator)
				{
					SetSpectator(num2, networkActor, p_animate);
					num2++;
				}
				else
				{
					SetRacer(num, networkActor, p_animate);
					num++;
				}
			}
		}

		public void UpdateGhosts(List<NetworkGhost> p_ghosts, int p_player_count)
		{
			int num = p_player_count;
			ghosts = p_ghosts;
			foreach (NetworkGhost p_ghost in p_ghosts)
			{
				SetGhost(num, p_ghost);
				num++;
			}
		}

		private void ClearUnusedCards()
		{
			int count = spectators.Count;
			int count2 = racers.Count;
			int count3 = ghosts.Count;
			for (int i = count; i < spectatorGridField.Count; i++)
			{
				UIMultiplayerRoomItemView uIMultiplayerRoomItemView = spectatorGridField.Get<UIMultiplayerRoomItemView>(i);
				if ((bool)uIMultiplayerRoomItemView)
				{
					uIMultiplayerRoomItemView.Clear();
				}
			}
			for (int j = count2 + count3; j < racerGridField.Count; j++)
			{
				UIMultiplayerRoomItemView uIMultiplayerRoomItemView2 = racerGridField.Get<UIMultiplayerRoomItemView>(j);
				if ((bool)uIMultiplayerRoomItemView2)
				{
					uIMultiplayerRoomItemView2.Clear();
				}
			}
		}

		private void AddPlayer(NetworkActor p_data, bool p_spectators)
		{
			(p_spectators ? spectators : racers).Add(p_data);
		}

		private void RemovePlayer(NetworkActor p_data, bool p_spectators)
		{
			List<NetworkActor> list = (p_spectators ? spectators : racers);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].PlayerId == p_data.PlayerId)
				{
					list.RemoveAt(i);
					break;
				}
			}
		}

		public UIMultiplayerRoomItemView GetPlayerById(string p_player_id)
		{
			ListComponent listComponent = racerGridField;
			for (int i = 0; i < listComponent.Count; i++)
			{
				UIMultiplayerRoomItemView uIMultiplayerRoomItemView = listComponent.Get<UIMultiplayerRoomItemView>(i);
				if (uIMultiplayerRoomItemView.data != null && uIMultiplayerRoomItemView.data.PlayerId == p_player_id)
				{
					return uIMultiplayerRoomItemView;
				}
			}
			listComponent = spectatorGridField;
			for (int j = 0; j < listComponent.Count; j++)
			{
				UIMultiplayerRoomItemView uIMultiplayerRoomItemView2 = listComponent.Get<UIMultiplayerRoomItemView>(j);
				if (uIMultiplayerRoomItemView2.data != null && uIMultiplayerRoomItemView2.data.PlayerId == p_player_id)
				{
					return uIMultiplayerRoomItemView2;
				}
			}
			return null;
		}

		public UIMultiplayerRoomItemView GetUIMaster()
		{
			ListComponent listComponent = racerGridField;
			for (int i = 0; i < listComponent.Count; i++)
			{
				UIMultiplayerRoomItemView uIMultiplayerRoomItemView = listComponent.Get<UIMultiplayerRoomItemView>(i);
				if (uIMultiplayerRoomItemView.data != null && uIMultiplayerRoomItemView.isMaster)
				{
					return uIMultiplayerRoomItemView;
				}
			}
			listComponent = spectatorGridField;
			for (int j = 0; j < listComponent.Count; j++)
			{
				UIMultiplayerRoomItemView uIMultiplayerRoomItemView2 = listComponent.Get<UIMultiplayerRoomItemView>(j);
				if (uIMultiplayerRoomItemView2.data != null && uIMultiplayerRoomItemView2.isMaster)
				{
					return uIMultiplayerRoomItemView2;
				}
			}
			return null;
		}

		public bool ContainsPlayer(List<NetworkActor> p_list, NetworkActor p_target)
		{
			if (p_target == null)
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
				if (p_list[i].PlayerId == p_target.PlayerId)
				{
					return true;
				}
			}
			return false;
		}

		public void SetPlayerMode(bool p_is_spectator)
		{
			stateNav.gameObject.SetActive(value: true);
			stateNav.transform.Find("race").gameObject.SetActive(value: false);
			stateNav.transform.Find("spectate").gameObject.SetActive(value: false);
			string text = (p_is_spectator ? "race" : "spectate");
			stateNav.transform.Find(text).gameObject.SetActive(value: true);
			if (room != null && text == "race" && room.RacersCount >= room.MaxRacers)
			{
				stateNav.gameObject.SetActive(value: false);
			}
			if (room != null && !room.IsQuickMatch && !room.IsTournamentMatch)
			{
				readyToggleNav.gameObject.SetActive(!p_is_spectator);
			}
		}

		private void SetTimeLimit()
		{
			int num = ((int)room.TimeLimit / 30 + 14) % 20;
			if (raceTimeLimit.index != num)
			{
				raceTimeLimit.index = num;
				raceTimeLimit.Refresh();
			}
		}

		public void SetGameType(GameFlag p_type)
		{
			_ = base.app.model.storage;
			gameType = p_type;
			mapSelector.interactable = false;
			campaignStepper.gameObject.SetActive(value: false);
			maxRacerStepper.max = ((gameType == GameFlag.Freestyle) ? 12 : 6) - 2;
			NetworkRoom networkRoom = base.app.model.network.room;
			if (networkRoom != null)
			{
				switch (p_type)
				{
				case GameFlag.Race:
					mapSelector.interactable = !networkRoom.IsTournamentMatch;
					droneClassSelector.interactable = !networkRoom.IsTournamentMatch;
					break;
				case GameFlag.Freestyle:
					mapSelector.interactable = true;
					break;
				case GameFlag.Campaign:
					PopulateCampaigns(0);
					campaignStepper.gameObject.SetActive(value: true);
					break;
				}
			}
		}

		public void SetGameType(NetworkRoom.GameType p_type)
		{
			switch (p_type)
			{
			case NetworkRoom.GameType.Race:
			case NetworkRoom.GameType.Tournament:
				SetGameType(GameFlag.Race);
				break;
			case NetworkRoom.GameType.Freestyle:
				SetGameType(GameFlag.Freestyle);
				break;
			}
		}

		public void SetAvailableOptions(bool p_is_master, bool p_is_tournament)
		{
			if (room == null)
			{
				return;
			}
			isMaster = p_is_master && !p_is_tournament;
			bool isQuickMatch = room.IsQuickMatch;
			bool flag = base.app.model.game != null;
			bool flag2 = room != null && room.AllowMapVoting;
			bool flag3 = !flag || !flag2;
			bool flag4 = gameType == GameFlag.Freestyle;
			bool dRLPilotMode = room.DRLPilotMode;
			roomNameInput.interactable = isMaster;
			roomPasswordInput.interactable = isMaster;
			roomPrivacyStepper.interactable = isMaster;
			maxRacerStepper.interactable = isMaster;
			maxSpectatorStepper.interactable = isMaster;
			ghostsAllowedToggle.interactable = isMaster && room.RacersCount < room.MaxRacers && !flag4;
			drlPilotModeToggle.interactable = isMaster;
			raceTimeLimit.interactable = isMaster && !flag4;
			timeoutModeDropdown.interactable = isMaster && !flag4;
			if (flag4 || isQuickMatch)
			{
				drlPilotModeToggle.isOn = false;
				room.DRLPilotMode = false;
				drlPilotModeToggle.interactable = false;
			}
			if (isQuickMatch)
			{
				timerNavLabelField.text = "RACE IN";
				isMaster = false;
				droneClassSelector.interactable = false;
				droneSelector.interactable = false;
				voteOptionsDropdown.interactable = false;
				armAndTurtleDropdown.interactable = false;
				campaignStepper.interactable = false;
				mapSelector.interactable = false;
			}
			else
			{
				timerNavLabelField.text = (isMaster ? "RESET" : "RACE IN");
				droneClassSelector.interactable = isMaster && !flag4 && !dRLPilotMode;
				droneSelector.interactable = (isMaster || room.DroneClass != 101) && !(flag4 && flag) && !dRLPilotMode;
				voteOptionsDropdown.interactable = isMaster && !flag4;
				armAndTurtleDropdown.interactable = isMaster && !flag4;
				campaignStepper.interactable = isMaster && flag3;
				mapSelector.interactable = isMaster && flag3;
				if (IsRandomMapMode())
				{
					mapSelector.interactable = false;
					flag2 = false;
				}
			}
			if (base.app.view.ui.footer.droneButton.interactable != droneSelector.interactable)
			{
				base.app.view.ui.footer.droneButton.interactable = droneSelector.interactable;
			}
			RefreshRightSideNavButtons(room, isMaster);
		}

		public void AssignRandomMap()
		{
			GameFlag[] p_categories = new GameFlag[2]
			{
				GameFlag.MapFeatured,
				GameFlag.MapDRL
			};
			List<MapData> list = base.app.model.storage.maps.Find(p_categories);
			foreach (string item in new List<string>
			{
				"MP-f95", "MP-615", "MP-95a", "MP-103", "MP-409", "MP-b59", "MP-23c", "MP-b9d", "MP-50c", "MP-19c",
				"MP-2cb"
			})
			{
				foreach (MapData item2 in base.app.model.storage.maps.FindByMapGUID(item))
				{
					if (item2.mapCategoryFlag != GameFlag.Collectable && item2.mapCategoryFlag != GameFlag.Freestyle)
					{
						list.Add(item2);
					}
				}
			}
			MapData mapData2 = list[Random.Range(0, list.Count)];
			string guid = base.app.model.storage.library.FindByGUID<DRLMap>(mapData2.mapId).guid;
			string text = "";
			string guid2 = mapData2.guid;
			bool flag = true;
			Notify("network.selection-complete", guid, text, guid2, flag, null, null, mapData2);
		}

		public void SetPlayerLayout(int p_row, int p_col, bool p_race)
		{
			int num = p_row * p_col;
			for (int i = 0; i < racerGridField.Count; i++)
			{
				if (i > num - 1)
				{
					racerGridField[i].gameObject.SetActive(value: false);
				}
				else
				{
					racerGridField[i].gameObject.SetActive(value: true);
				}
			}
			racerGridGroup.constraintCount = p_row;
			if (p_race)
			{
				racerGridGroup.cellSize = new Vector2(340f, 224f);
			}
			else
			{
				racerGridGroup.cellSize = new Vector2(252f, 146f);
			}
			SetCardsNavigation(p_row, p_col);
		}

		private void SetCardsNavigation(int p_row, int p_col)
		{
			for (int i = 0; i < racerGridField.Count; i++)
			{
				UINavigation component = racerGridField[i].GetComponent<UINavigation>();
				component.right = null;
				component.left = null;
				component.up = null;
				component.down = null;
			}
			int num = p_row * p_col;
			int num2 = 0;
			for (int j = 0; j < p_col; j++)
			{
				for (int k = 0; k < p_row; k++)
				{
					UINavigation component = racerGridField[num2].GetComponent<UINavigation>();
					if (num2 + p_row < num)
					{
						if (racerGridField[num2 + p_row] != null)
						{
							component.down = racerGridField[num2 + p_row];
						}
					}
					else
					{
						component.down = spectatorNavs[k];
						spectatorNavs[k].up = component;
						if (p_row < 4)
						{
							spectatorNavs[p_row].up = component;
						}
					}
					if (j == 0)
					{
						component.up = racersUpProxy;
					}
					else if (num2 - p_row >= 0 && racerGridField[num2 - p_row] != null)
					{
						component.up = racerGridField[num2 - p_row];
					}
					if (k + 1 == p_row)
					{
						component.right = racersRightProxy;
					}
					else if (num2 + 1 < num)
					{
						component.right = racerGridField[num2 + 1];
					}
					if (k == 0)
					{
						component.left = backButton.GetComponent<UINavigation>();
					}
					else if (num2 - 1 >= 0)
					{
						component.left = racerGridField[num2 - 1];
					}
					if (num2 == p_col)
					{
						mapSelector.GetComponent<UINavigation>().left = component;
					}
					num2++;
				}
			}
		}

		private void UpdateMap(string p_guid, bool p_force = false)
		{
			if (gameType == GameFlag.Campaign || (!string.IsNullOrEmpty(mapGUID) && !p_force && mapGUID == p_guid))
			{
				return;
			}
			StorageModel storage = base.app.model.storage;
			if (storage == null)
			{
				return;
			}
			List<DRLMap> list = ((gameType == GameFlag.Freestyle) ? storage.GetMaps() : storage.GetRaceMaps());
			list.RemoveAll((DRLMap it) => (bool)it.tags && it.tags.Contains(GameFlag.MapEditorOnly));
			int num = -1;
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				if (list[num2].guid == p_guid)
				{
					num = num2;
					break;
				}
			}
			if (num < 0)
			{
				Debug.LogWarning("UIMultiplayerRoomView> UpdateMap - Invalid Map - guid[" + p_guid + "]");
				return;
			}
			map = list[num];
			customMap = null;
			UpdateMapUI();
		}

		private void UpdateMapUI()
		{
			mapName.text = map.title.ToUpper();
			mapThumbFader.Fade(0f, 1f);
			mapPreviewCard.texture = mapCard.texture;
			RunOnce(1f, delegate
			{
				if (map != null && mapThumbFader != null)
				{
					mapThumb = map.preview;
				}
				mapThumbFader.Fade(1f, 1f);
			});
		}

		private void UpdateMapTrack(string p_guid, bool p_force = false)
		{
			if (gameType == GameFlag.Campaign || (!string.IsNullOrEmpty(trackGUID) && !p_force && trackGUID == p_guid))
			{
				return;
			}
			StorageModel storage = base.app.model.storage;
			if (storage == null)
			{
				return;
			}
			if (!map)
			{
				Debug.LogWarning("UIMultiplayerRoomView> UpdateTrack - Null Map - track-guid[" + p_guid + "]");
				return;
			}
			DRLMapTrack mapTrack = storage.GetMapTrack(map.guid, p_guid, gameType == GameFlag.Freestyle);
			if (mapTrack == null)
			{
				Debug.LogWarning("UIMultiplayerRoomView> UpdateTrack  - Invalid Track - guid[" + p_guid + "]");
				return;
			}
			track = mapTrack;
			UpdateTrackUI(mapTrack.label);
		}

		private void UpdateTrackUI(string p_track_title)
		{
			trackName.text = p_track_title;
			trackThumbFader.Fade(0f, 1f);
			trackPreviewCard.texture = trackCard.texture;
			RunOnce(1f, delegate
			{
				trackThumb = GrayGradient;
				trackThumbFader.Fade(1f, 1f);
			});
		}

		private void UpdateCustomMap(string p_guid, string p_custom_map_title)
		{
			if (!this || room == null || gameType == GameFlag.Campaign || (!string.IsNullOrEmpty(customMapGUID) && customMapGUID == p_guid) || m_activeGUID == p_guid)
			{
				return;
			}
			m_activeGUID = p_guid;
			trackName.text = (string.IsNullOrEmpty(p_custom_map_title) ? string.Empty : p_custom_map_title.ToUpper());
			ServiceModel service = base.app.model.service;
			base.app.model.network.StartKeepAliveLoop();
			service.GetCommunityMaps(p_guid, p_has_root: false, delegate(DRLCommunityMapResult p_result)
			{
				if (room != null && (bool)this)
				{
					m_activeGUID = "";
					DRLCommunityMapData d = ((p_result.data.Length == 0) ? null : p_result.data[0]);
					if (d == null)
					{
						Debug.LogWarning("UIMultiplayerRoomController> UpdateCustomMap / Failed to Load DRLCommunityMapData - guid[" + p_guid + "]");
					}
					else
					{
						new Thread((ThreadStart)delegate
						{
							MapData md = d.Convert<MapData>();
							if (md == null)
							{
								Debug.LogWarning("UIMultiplayerRoomController> UpdateCustomMap / Failed to Parse MapData - guid[" + p_guid + "]");
							}
							else
							{
								Activity.RunOnce(delegate
								{
									DRLMap base_map = base.app.model.storage.library.FindByGUID<DRLMap>(md.mapId);
									customMap = md;
									map = null;
									customMapPhotoURL = d.mapThumbURL;
									mapName.text = base_map.title.ToUpper();
									trackName.text = d.mapTitle.ToUpper();
									room.CustomMapName = d.mapTitle.ToUpper();
									room.MapId = base_map.guid;
									room.TrackId = d.trackId;
									base.app.model.network.StopKeepAliveLoop(1f);
									mapThumbFader.Fade(0f, 1f);
									mapPreviewCard.texture = mapCard.texture;
									RunOnce(1f, delegate
									{
										if (room != null && (bool)this && (bool)base_map && (bool)mapThumbFader)
										{
											mapThumb = base_map.preview;
											mapThumbFader.Fade(1f, 1f);
										}
									});
								}, 1f / 60f);
							}
						}).Start();
					}
				}
			});
		}

		private void UpdateSpecificDrone()
		{
			if (room == null || room.DroneClass != 101)
			{
				return;
			}
			NetworkActor master = room.Master;
			NetworkActor local = room.Local;
			if (master == null || local == null)
			{
				return;
			}
			DroneRigData droneRigData = DroneRigData.FromJson(master.DroneRigData);
			if (!room.IsMaster && !room.IsQuickMatch && !room.IsTournamentMatch)
			{
				if (!droneRigData.FunctionallyIdentical(base.app.model.storage.state.player.garage.currentRigData))
				{
					base.app.model.storage.state.player.garage.currentRigData = droneRigData;
					room.Local.DroneRigData = ((droneRigData == null) ? "" : droneRigData.ToJson());
					if (!(droneRigData == null))
					{
						SetSpecificDroneData(droneRigData);
					}
				}
			}
			else if (room.IsTournamentMatch && !m_specDroneThumbInit)
			{
				m_specDroneThumbInit = true;
				DroneRigData currentRigData = base.app.model.storage.state.player.garage.currentRigData;
				SetSpecificDroneData(currentRigData);
			}
		}

		public void SetSpecificDroneData(DroneRigData p_rig_data)
		{
			if (base.app.model.service == null || room == null)
			{
				return;
			}
			if (p_rig_data == null)
			{
				Debug.Log("UIMultiplayerRoomView> SetSpecificDroneData / Can't load Drone Rig Data UI data");
				return;
			}
			droneName = p_rig_data.name.ToUpper() + (p_rig_data.hasCustomPhysics ? " *" : "");
			droneSelectorName.color = (p_rig_data.hasCustomPhysics ? Color.yellow : Color.white);
			droneInfoWidth = 0f;
			if (room.IsMaster)
			{
				room.SelectedDrone = p_rig_data.guid;
			}
			Debug.Log("UIMultiplayerRoomView> SetSpecificDroneData - rig-name[" + p_rig_data.name + "]\nrig_data-guid\n  " + p_rig_data.guid + "\nselected\n  " + room.SelectedDrone);
			if (droneSelectorThumb.isActiveAndEnabled)
			{
				RawImage droneImage = GetComponentInChildren<UIMultiplayerRoomItemView>().droneImage;
				droneThumb = droneImage.texture;
			}
		}

		public void UpdateDrone()
		{
			_ = base.app.model.service;
			GarageStateModel garage = base.app.model.storage.state.player.garage;
			DroneRigData droneRigData = garage.currentRigData;
			if (room == null)
			{
				return;
			}
			bool flag = false;
			if (room.IsTournamentMatch && room.DroneClass == 101)
			{
				garage.currentRigData = droneRigData;
				base.app.model.network.room.Local.DroneRigData = ((droneRigData == null) ? "" : droneRigData.ToJson());
			}
			else if (!isMaster && room.DroneClass == 101 && room.Master != null && room.Local != null)
			{
				NetworkActor master = room.Master;
				NetworkActor local = room.Local;
				DroneRigData droneRigData2 = DroneRigData.FromJson(master.DroneRigData);
				if (!droneRigData2.FunctionallyIdentical(garage.currentRigData))
				{
					droneRigData = droneRigData2;
					local.DroneRigData = ((droneRigData2 == null) ? "" : master.DroneRigData);
				}
			}
			else
			{
				if (track != null && track.promoDrones != null && track.promoDrones.Length != 0 && !new List<DroneRigData>(track.promoDrones).Contains(droneRigData))
				{
					if (track.promoDronesOnly)
					{
						droneRigData = track.promoDrones[0];
					}
					else if (!garage.RigExists(droneRigData))
					{
						droneRigData = garage.defaultRig;
					}
				}
				if (map != null && map.promoDrones != null && map.promoDrones.Length != 0)
				{
					if (!new List<DroneRigData>(map.promoDrones).Contains(droneRigData))
					{
						if (map.promoDronesOnly)
						{
							droneRigData = map.promoDrones[0];
						}
						else if (!garage.RigExists(droneRigData))
						{
							droneRigData = garage.defaultRig;
						}
					}
				}
				else if (track != null && track.droneSizes != null && track.droneSizes.Length != 0)
				{
					List<int> list = new List<int>(track.droneSizes);
					if (flag || !list.Contains(droneRigData.diameter))
					{
						for (int i = 0; i < garage.originalRigs.Count; i++)
						{
							droneRigData = garage.originalRigs[i];
							if (list.Contains(droneRigData.diameter))
							{
								garage.currentRigData = droneRigData;
								break;
							}
						}
					}
				}
				else if (map != null && map.droneSizes != null && map.droneSizes.Length != 0)
				{
					List<int> list2 = new List<int>(map.droneSizes);
					if (flag || !list2.Contains(droneRigData.diameter))
					{
						for (int j = 0; j < garage.originalRigs.Count; j++)
						{
							droneRigData = garage.originalRigs[j];
							if (list2.Contains(droneRigData.diameter))
							{
								garage.currentRigData = droneRigData;
								break;
							}
						}
					}
				}
				else if (selectedDroneClass > 0 && selectedDroneClass < 100 && droneRigData.diameter != selectedDroneClass)
				{
					for (int k = 0; k < garage.originalRigs.Count; k++)
					{
						droneRigData = garage.originalRigs[k];
						if (droneRigData.diameter == selectedDroneClass)
						{
							break;
						}
					}
				}
				else
				{
					if (!garage.RigExists(droneRigData))
					{
						garage.currentRigData = null;
						droneRigData = garage.currentRigData;
					}
					if (!garage.RigExists(droneRigData))
					{
						droneRigData = (garage.currentRigData = garage.defaultRig);
					}
					if (flag)
					{
						droneRigData = garage.defaultRig;
						if (droneRigData.diameter != selectedDroneClass)
						{
							for (int l = 0; l < garage.originalRigs.Count; l++)
							{
								droneRigData = garage.originalRigs[l];
								if (droneRigData.diameter == selectedDroneClass)
								{
									break;
								}
							}
						}
					}
				}
			}
			garage.currentRigData = droneRigData;
			if (room?.Local != null)
			{
				room.Local.DroneRigData = ((droneRigData == null) ? "" : droneRigData.ToJson());
			}
			if (!(droneRigData == null))
			{
				SetSpecificDroneData(droneRigData);
			}
		}

		public void RefreshDroneClassSelectorStepper()
		{
			int num = 0;
			switch (selectedDroneClass)
			{
			case 100:
				num = 0;
				droneClassLabel.SetActive(value: true);
				break;
			case 101:
				num = 6;
				droneClassLabel.SetActive(value: false);
				break;
			default:
				num = selectedDroneClass - 2;
				droneClassLabel.SetActive(value: true);
				break;
			}
			if (num < 0)
			{
				num = 0;
			}
			if (droneClassSelector.index != num)
			{
				droneClassSelector.index = num;
				droneClassSelector.Refresh();
			}
		}

		private void RefreshPrivacyStepper()
		{
			roomPasswordInput.gameObject.SetActive(room.IsPrivate);
			roomPasswordInput.text = (room.IsPrivate ? room.Password : "");
			if ((roomPrivacyStepper.index != 1 || !room.IsPrivate) && (roomPrivacyStepper.index != 0 || room.IsPrivate))
			{
				roomPrivacyStepper.index = (room.IsPrivate ? 1 : 0);
				roomPrivacyStepper.Refresh();
			}
		}

		public void UpdateCampaign(string p_guid)
		{
			if (gameType != GameFlag.Campaign || (!string.IsNullOrEmpty(campaignGUID) && campaignGUID == p_guid))
			{
				return;
			}
			List<DRLCampaign> campaigns = base.app.model.storage.GetCampaigns();
			int num = -1;
			for (int i = 0; i < campaigns.Count; i++)
			{
				if (campaigns[i].guid == p_guid)
				{
					num = i;
					break;
				}
			}
			if (num < 0)
			{
				Debug.LogWarning("UIMultiplayerRoomView> UpdateCampaign - Invalid Campaign - guid[" + p_guid + "]");
			}
			else
			{
				PopulateCampaigns(num);
			}
		}

		public void PopulateCampaigns(int p_index)
		{
			List<string> campaignNames = base.app.model.storage.GetCampaignNames();
			for (int i = 0; i < campaignNames.Count; i++)
			{
				campaignNames[i] = campaignNames[i].ToUpper().Replace("\n", " ");
			}
			DRLStepperView dRLStepperView = campaignStepper;
			dRLStepperView.min = 0;
			dRLStepperView.max = campaignNames.Count - 1;
			dRLStepperView.labels = campaignNames.ToArray();
			if (p_index >= 0)
			{
				campaign = null;
			}
			List<DRLCampaign> campaigns = base.app.model.storage.GetCampaigns();
			dRLStepperView.index = Mathf.Clamp(p_index, dRLStepperView.min, dRLStepperView.max);
			if ((bool)campaign)
			{
				dRLStepperView.index = campaigns.IndexOf(campaign);
			}
			else
			{
				campaign = ((campaigns.Count <= 0) ? null : campaigns[dRLStepperView.index]);
			}
			dRLStepperView.Refresh();
		}
	}
}
