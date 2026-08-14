public class N
{
	public class Scene
	{
		public const string Load = "scene.load";

		public const string Start = "scene.start";

		public const string LoadProgress = "scene.load.progress";

		public const string MapSceneLoad = "scene.map.load";

		public const string TrackSceneLoad = "scene.track.load";

		public const string MissionSceneLoad = "scene.mission.load";

		public const string Fail = "scene.fail";

		public const string GameScenesStart = "scene.game.scenes@start";

		public const string GameScenesComplete = "scene.game.scenes@complete";

		public const string TrackBuildStart = "scene.track.build@start";

		public const string TrackBuildProgress = "scene.track.build@progress";

		public const string TrackBuildComplete = "scene.track.build@complete";

		public const string SimForceReset = "scene.force-reset";
	}

	public class Boot
	{
		public const string PlatformAuth = "boot.platform.login";

		public const string MissingDLL = "boot.missing.dll";

		public const string ConnectionTimeoutStart = "boot.timeout@start";

		public const string ConnectionTimeoutStop = "boot.timeout@stop";

		public const string ConnectionTimeoutUpdate = "boot.timeout@update";

		public const string ConnectionTimeout = "boot.timeout";

		public const string DRLAuthLicenseCheck = "boot.drl.license.check";

		public const string DRLAuthSuccess = "boot.drl.login@success";

		public const string DRLAuthStart = "boot.drl.login@start";

		public const string DRLAuthRetry = "boot.drl.login@retry";

		public const string DRLAuthFail = "boot.drl.login@fail";

		public const string DRLPlatformFail = "boot.drl.platform@fail";

		public const string DRLLoadOfflineLayout = "boot.drl.offline-layout";

		public const string StateStart = "boot.drl.state@start";

		public const string StateFail = "boot.drl.state@fail";

		public const string LocalizationComplete = "boot.drl.localization@complete";

		public const string LoadContentManifest = "boot.drl.content.manifest";

		public const string ContentDownloadStart = "boot.drl.content.download@start";

		public const string ContentDownloadProgress = "boot.drl.content.download@progress";

		public const string ContentDownloadComplete = "boot.drl.content.download@complete";

		public const string BundlesLoadStart = "boot.drl.bundle.load@start";

		public const string BundlesLoadProgress = "boot.drl.bundle.load@progress";

		public const string BundlesLoadComplete = "boot.drl.bundle.load@complete";

		public const string Complete = "boot@complete";

		public const string DRLTryoutsAuth = "boot.drl.tryouts.login";

		public const string DRLTryoutsAuthSuccess = "boot.drl.tryouts.login@success";

		public const string DRLTryoutsAuthDismiss = "boot.drl.tryouts.login.dismiss";

		public const string DRLTryoutsLoginHandleEndEdit = "boot.drl.tryouts.login.handle@end-edit";

		public const string DRLTryoutsLoginHandleChange = "boot.drl.tryouts.login.handle@change";

		public const string DRLTryoutsLoginColorChange = "boot.drl.tryouts.login.color@change";

		public const string DRLTryoutsLoginSubmitClick = "boot.drl.tryouts.login.submit@click";

		public const string DRLTryoutsLoginDismissClick = "boot.drl.tryouts.login.dismiss@click";

		public const string OfflineMapDownloadStart = "boot.drl.offline-maps.download@start";

		public const string OfflineMapStoreStart = "boot.drl.offline-maps.store@start";

		public const string OfflineMapsStoreProgress = "boot.drl.offline-maps.store@progress";

		public const string OfflineMapsDownloadProgress = "boot.drl.offline-maps.download@progress";

		public const string OfflineMapsDownloadError = "boot.drl.offline-maps.download@error";
	}

	public class AsyncLoader
	{
		public const string Create = "async-loader@create";

		public const string Start = "async-loader@start";

		public const string Progress = "async-loader@progress";

		public const string UploadProgress = "async-loader@upload-progress";

		public const string Complete = "async-loader@complete";

		public const string Cancel = "async-loader@cancel";

		public const string BatchComplete = "async-loader@batch-complete";
	}

	public class Splash
	{
		public const string IntroComplete = "splash.intro@complete";

		public const string LoadComplete = "splash.load@complete";

		public const string LoadStart = "splash.load@start";

		public const string QuitClick = "splash.quit@click";

		public const string ConnectionCheckClick = "splash.connection-check@click";

		public const string StartOfflineClick = "splash.offline-mode@click";

		public const string ConnectionRetryClick = "splash.connection-retry@click";
	}

	public class Home
	{
		public const string FreestyleClick = "home.freestyle@click";

		public const string CircuitsClick = "home.circuits@click";

		public const string MultiplayerClick = "home.multiplayer@click";

		public const string RaceClick = "home.race@click";

		public const string MissionsClick = "home.missions@click";

		public const string DMVClick = "home.DMV@click";

		public const string SandboxClick = "home.sandbox@click";

		public const string StoreClick = "home.store@click";

		public const string TryoutsClick = "home.tryouts@click";

		public const string VDRLClick = "home.vdrl@click";

		public const string MyDronesClick = "home.my-drones@click";

		public const string MapEditorClick = "home.map-editor@click";

		public const string GarageClick = "home.garage@click";

		public const string ShopClick = "home.shop@click";

		public const string SettingsClick = "home.settings@click";

		public const string ProfileClick = "home.profile@click";

		public const string DRLLeaderboardsClick = "home.leaderboards.drl@click";

		public const string OpenLeaderboardsClick = "home.leaderboards.open@click";

		public const string CommunityDronesClick = "home.community.drones@click";

		public const string CommunityMapsClick = "home.community.maps@click";

		public const string QuitClick = "home.quit@click";

		public const string PurchaseClick = "home.purchase@click";

		public const string USAFClick = "home.usaf@click";

		public const string AllianzClick = "home.allianz@click";

		public const string CollectableClick = "home.collectable@click";

		public const string FlyClick = "home.fly@click";

		public const string LeadersClick = "home.leaders@click";

		public const string DebugMapEditorClick = "home.debug.map-editor@click";

		public const string DebugDMVClick = "home.debug.dmv@click";

		public const string DebugDMVChange = "home.debug.dmv@change";

		public const string DebugUAVClick = "home.debug.uav@click";
	}

	public class Missions
	{
		public const string MissionCardEvent = "missions.mission-card";

		public const string MissionCardClick = "missions.mission-card@click";

		public const string QuestCardEvent = "missions.quest-card";

		public const string TestCardEvent = "missions.test-card";

		public const string QuestCardClick = "missions.quest-card@click";

		public const string TestCardClick = "missions.test-card@click";

		public const string MissionOverviewStartClick = "missions.mission-overview.start@click";

		public const string MissionOverviewFormEvent = "missions.mission-overview.form.event";

		public const string MissionOverviewFormEventClick = "missions.mission-overview.form.event@click";

		public const string TestOverviewStartClick = "missions.test-overview.start@click";

		public const string MissionComplete = "missions.mission-complete";

		public const string MissionCompleteQuestsClick = "missions.mission-complete.quests@click";

		public const string LessonCompleteMenuClick = "missions.lesson-complete.tests@click";

		public const string MissionCompleteExitClick = "missions.mission-complete.exit@click";

		public const string MissionCompleteNextClick = "missions.mission-complete.next@click";

		public const string LessonCompleteNextClick = "missions.lesson-complete.next@click";

		public const string MissionCompleteRestartClick = "missions.mission-complete.restart@click";

		public const string TestSubmenuOpened = "test.submenu@opened";

		public const string TestSubmenuClosed = "test.submenu@closed";

		public const string DMVSubmenuToggleClick = "test.toggle.submenu@click";

		public const string DMVTotalProgress = "missions.dmv.total-progress";

		public const string MissionCompleteSoftResetClick = "missions.mission-complete.soft-reset@click";

		public const string LessonsScoreSet = "missions.scoring.set";

		public const string DMVUserRankUpdated = "missions.dmv.rank.updated";

		public const string AcquireCertificateClick = "missions.certificate-acquired@click";

		public const string MissionEnterMenu = "missions.enter.menu@click";
	}

	public class Onboarding
	{
		public const string OnboardingStartBeginnerClick = "onboarding.start.beginner@click";

		public const string OnboardingStartIntermediateClick = "onboarding.start.intermediate@click";

		public const string OnboardingStartProClick = "onboarding.start.pro@click";

		public const string OnboardingStartRace = "onboarding.start.race";

		public const string OnboardingFinishClick = "onboarding.finish@click";

		public const string OnboardingStartCurrentStepClick = "onboarding.step.current@click";

		public const string OnboardingStartNextStep = "onboarding.step.next@start";

		public const string OnboardingStartPreviousStep = "onboarding.step.previous@start";

		public const string OnboardingSkipClick = "onboarding.skip@click";

		public const string OnboardingStarted = "onboarding.campaign@start";

		public const string OnboardingStopped = "onboarding.campaign@stop";

		public const string OnboardingIncreaseCurrentStep = "onboarding.progress@increase";

		public const string OnboardingMissionComplete = "onboarding.mission.complete@increase";

		public const string OnboardingProgressDecrease = "onboarding.progress@decrease";

		public const string OnboardingHeaderRefresh = "onboarding.header.refresh";

		public const string OnboardingProMissionClick = "onboarding.pro.mission@click";

		public const string OnboardingOrientationExitClick = "onboarding.orientation.exit@click";

		public const string OnboardingProgressNextClick = "onboarding.progress.next@click";

		public const string OnboardingProgressReset = "onboarding.progress@click";

		public const string OnboardingProgressResetAll = "onboarding.progress@reset-all";

		public const string OnboardingProgressResetAllClick = "onboarding.progress.reset.all@click";

		public const string OnboardingStop = "onboarding.stop";

		public const string OnboardingRestartTraining = "onboarding.restart.training";

		public const string OnboardingFailedTrainingClick = "onboarding.failed.training@click";

		public const string OnboardingFailedRaceRestartClick = "onboarding.failed.race-restart@click";

		public const string OnboardingMissionsCompleteNext = "onboarding.missions-complete.next@click";

		public const string OnboardingEnterMenu = "onboarding.enter.menu@click";

		public const string OnboardingOverview = "onboarding.enter.overview";

		public const string OnboardingOpenComplete = "onboarding.open.complete";

		public const string OnboardingBackToOverview = "onboarding.back.overview@click";

		public const string OnboardingBackToHome = "onboarding.back.home@click";

		public const string OnboardingCompleteNext = "onboarding.complete.next@click";

		public const string OnboardingVideoClick = "onboarding.video.click@click";
	}

	public class Tournament
	{
		public const string TournamentSkillCardEvent = "tournament.skill-card";

		public const string TournamentSkillCardClick = "tournament.skill-card@click";

		public const string TournamentCardEvent = "tournament-card";

		public const string TournamentCardClick = "tournament-card@click";

		public const string PastTournamentCardClick = "past-tournament-card@click";

		public const string TournamentDroneCardClick = "tournament.drone-card@click";

		public const string TournamentMatchCardEvent = "tournament.match-card";

		public const string TournamentMatchCardClick = "tournament.match-card@click";

		public const string TournamentMatchCardFormEventClick = "tournament.match-card.form.event@click";

		public const string TournamentMatchComplete = "tournament.match.complete";

		public const string TournamentListFormEvent = "tournament.list.form.event";

		public const string TournamentListFormEventClick = "tournament.list.form.event@click";

		public const string TournamentOverviewFormEventClick = "tournament.overview.form.event@click";

		public const string TournamentOverviewCardHover = "tournament.overview.card@over";

		public const string TournamentOverviewCardOut = "tournament.overview.card@out";

		public const string TournamentOverviewCardClick = "tournament.overview.card@click";

		public const string OpenBracketsScreen = "tournament.brackets.open";

		public const string TournamentResultsNextClick = "tournament.results.next@click";

		public const string TournamentLeaderboardNextClick = "tournament.leaderboards.next@click";

		public const string TournamentEnterMatchClick = "tournament.enter-match@click";

		public const string TournamentSettingsClick = "tournament.settings@click";

		public const string TournamentExitClick = "tournament.exit@click";

		public const string TournamentWinnersClick = "tournament.winners@click";

		public const string TournamentStandingsClick = "tournament.standings@click";

		public const string TournamentRefreshState = "tournament.action.refresh";

		public const string TournamentRefreshRacers = "tournament.action.refresh-racers";

		public const string TournamentSwappedPlayers = "tournament.action.swapped";

		public const string TournamentMatchReset = "tournament.action.reset-match";

		public const string TournamentHeatReset = "tournament.action.reset-heat";

		public const string TournamentMatchStart = "tournament.action.start-match";

		public const string TournamentHeatQuit = "tournament.action.quit-heat";

		public const string TournamentHeatQuitUser = "tournament.action.quit-heat-user";

		public const string TournamentMatchStarting = "tournament.action.match-starting";

		public const string TournamentPullRacers = "tournament.action.match-pull";

		public const string MatchResultsArrived = "tournament.match.results-arrived";

		public const string TournamentStoppedRefreshListener = "tournament.refresh-listener.stopped";

		public const string TournamentCountdownStart = "tournament.countdown-start";

		public const string TournamentPlacementsClick = "tournament.placements@click";

		public const string DroneSelected = "tournament.drone.selected";

		public const string TournamentRefreshData = "tournament.refresh.data";

		public const string TournamentRefreshLobby = "tournament.refresh.lobby";

		public const string TournamentBracketsOpen = "tournament.brackets.open";

		public const string TournamentBracketsClose = "tournament.brackets.close";

		public const string TournamentResetModel = "tournament.model.reset";

		public const string TournamentUpdateRoundTimer = "tournametn.timer.update";

		public const string TournamentExited = "tournament.exited";

		public const string TournamentHeatReplayClick = "tournament.match-heat.replay@click";

		public const string TournamentReplayIncoming = "tournament.replay.incoming";
	}

	public class Fly
	{
		public const string CampaignClick = "fly.campaign@click";

		public const string FreecameraClick = "fly.freecamera@click";

		public const string MapCardEvent = "fly.map-card";

		public const string MapCardClick = "fly.map-card@click";

		public const string MapTrackCardEvent = "fly.map-track-card";

		public const string MapTrackCardClick = "fly.map-track-card@click";

		public const string DebugCommunityMapsClick = "fly.debug.community-maps@click";

		public const string CommunityMaps = "fly.community-maps@click";

		public const string SDCommunityMaps = "fly.sd-community-maps@click";

		public const string SimCup = "fly.sim-cup@click";

		public const string VirtualSeason = "fly.virtual-season@click";

		public const string SimpleCourses = "fly.simple-courses@click";

		public const string MultiGP = "fly.multigp@click";

		public const string FavoriteMaps = "fly.favorite-maps@click";

		public const string MegaMaps = "fly.mega-maps@click";

		public const string DRLMaps = "fly.drl-maps@click";

		public const string SDDRLMaps = "fly.sd-drl-maps@click";

		public const string SDDRLMapOverview = "fly.sd-drl-map-overview@click";

		public const string GatesOfHell = "fly.gates-of-hell@click";

		public const string OutOfService = "fly.out-of-service@click";

		public const string FeaturedTracks = "fly.featured-tracks@click";

		public const string MapTrackOverviewStartClick = "fly.map-track-overview.start@click";

		public const string MapTrackOverviewReady = "fly.map-track-overview.ready";

		public const string CircuitsOverviewReady = "fly.circuits-overview.ready";

		public const string MapTrackOverviewFormEvent = "fly.map-track-overview.form.event";

		public const string MapTrackOverviewFormEventClick = "fly.map-track-overview.form.event@click";

		public const string MapTrackOverviewFormEventChange = "fly.map-track-overview.form.event@change";

		public const string MapTrackProModeClick = "fly.map-track-overview.pro@click";

		public const string MapTrackProModeOver = "fly.map-track-overview.pro@over";

		public const string MapTrackProModeOut = "fly.map-track-overview.pro@out";

		public const string DroneSelectionCardEvent = "fly.drone-selection.card";

		public const string DroneSelectionCardClick = "fly.drone-selection.card@click";

		public const string DroneSelectionCardFocus = "fly.drone-selection.card@focus";
	}

	public class Campaign
	{
		public const string CampaignCardEvent = "campaign.campaign-card";

		public const string CampaignCardClick = "campaign.campaign-card@click";

		public const string TryoutsOnboardingFormEvent = "campaign.tryouts.onboarding.form.event";

		public const string TryoutsOnboardingFormClick = "campaign.tryouts.onboarding.form.event@click";

		public const string TryoutsOnboardingFormChange = "campaign.tryouts.onboarding.form.event@change";

		public const string TryoutsLeadersItemClick = "campaign.tryouts.leaders.item@click";

		public const string TryoutsLeadersClick = "campaign.tryouts.leaders@click";

		public const string TryoutsResultsClick = "campaign.tryouts.results@click";

		public const string CampaignRegisterFormEvent = "campaign.register.form.event";

		public const string CampaignRegisterFormClick = "campaign.register.form.event@click";

		public const string CampaignRegisterFormChange = "campaign.register.form.event@change";

		public const string CampaignRegisterFormSubmit = "campaign.register.form.event@submit";

		public const string CampaignMapCardEvent = "campaign.campaign-map-card";

		public const string CampaignMapCardClick = "campaign.campaign-map-card@click";

		public const string CampaignRestartClick = "campaign.restart@click";

		public const string CampaignResultReplayComplete = "campaign.result.replay@complete";

		public const string CampaignOpenResults = "campaign.open.results@click";

		public const string CampaignOpenLeaders = "campaign.open.leaders@click";

		public const string CampaignCloseResults = "campaign.close.results@click";
	}

	public class Garage
	{
		public const string SelectionFlyClick = "garage.selection.fly@click";

		public const string SelectionDeleteClick = "garage.selection.delete@click";

		public const string SelectionEditClick = "garage.selection.edit@click";

		public const string SelectionCloneClick = "garage.selection.clone@click";

		public const string SelectionSaveClick = "garage.selection.save@click";

		public const string SelectionCreateClick = "garage.selection.create@click";

		public const string SelectionItemClick = "garage.selection.item@click";

		public const string SelectionItemOver = "garage.selection.item@over";

		public const string SelectionItemOut = "garage.selection.item@out";

		public const string SelectionItemMenuClick = "garage.selection.item.menu@click";

		public const string SelectionDataFocusOn = "garage.selection.data@focus";

		public const string SelectionDataClick = "garage.selection.data@click";

		public const string SelectionDataFocusOff = "garage.selection.data@unfocus";

		public const string SelectionStoreClick = "garage.store@click";

		public const string EditFormEvent = "garage.edit.form.event";

		public const string EditFormEventClick = "garage.edit.form.event@click";

		public const string EditFormEventSubmit = "garage.edit.form.event@submit";

		public const string EditFormEventFocusEnd = "garage.edit.form.event@end-edit";

		public const string EditFlyClick = "garage.edit.fly@click";

		public const string EditPreviewClick = "garage.edit.preview@click";

		public const string EditApplyClick = "garage.edit.apply@click";

		public const string EditApplyFocus = "garage.edit.apply@focus";

		public const string EditTabClick = "garage.edit.tab@click";

		public const string EditTabChange = "garage.edit.tab@change";

		public const string EditItemClick = "garage.edit.item@click";

		public const string EditItemFocus = "garage.edit.item@focus";

		public const string EditViewerHitOver = "garage.edit.viewer.hit@no-sound@over";

		public const string EditViewerHitOut = "garage.edit.viewer.hit@no-sound@out";

		public const string EditGridExit = "garage.edit.grid@out";

		public const string RigTrailColorClick = "garage.edit.rig-trailcolor@click";

		public const string RigTrailColorFocus = "garage.edit.rig-trailcolor@focus";

		public const string RigTrailColorUnfocus = "garage.edit.rig-trailcolor@unfocus";

		public const string RigPropColorClick = "garage.edit.rig-propcolor@click";

		public const string RigPropColorFocus = "garage.edit.rig-propcolor@focus";

		public const string RigPropColorUnfocus = "garage.edit.rig-propcolor@unfocus";

		public const string RigTextureColorClick = "garage.edit.rig-texturecolor@click";

		public const string RigTextureColorFocus = "garage.edit.rig-texturecolor@focus";

		public const string RigTextureColorUnfocus = "garage.edit.rig-texturecolor@unfocus";

		public const string RigEdgeColorClick = "garage.edit.rig-edgecolor@click";

		public const string RigEdgeColorFocus = "garage.edit.rig-edgecolor@focus";

		public const string RigEdgeColorUnfocus = "garage.edit.rig-edgecolor@unfocus";

		public const string TrailColorsClick = "garage.edit.trailcolors@click";

		public const string TrailColorsFocus = "garage.edit.trailcolors@focus";

		public const string TrailColorsUnfocus = "garage.edit.trailcolors@unfocus";

		public const string PropColorsClick = "garage.edit.propcolors@click";

		public const string PropColorsFocus = "garage.edit.propcolors@focus";

		public const string PropColorsUnfocus = "garage.edit.propcolors@unfocus";

		public const string TextureColorsClick = "garage.edit.texturecolors@click";

		public const string TextureColorsFocus = "garage.edit.texturecolors@focus";

		public const string TextureColorsUnfocus = "garage.edit.texturecolors@unfocus";

		public const string EdgeColorsClick = "garage.edit.edgecolors@click";

		public const string EdgeColorsFocus = "garage.edit.edgecolors@focus";

		public const string EdgeColorsUnfocus = "garage.edit.edgecolors@unfocus";

		public const string EditRigSaved = "garage.edit.rig.saved";

		public const string EditFlyReady = "garage.edit.fly.ready";

		public const string EditDatasheet = "garage.edit.datasheet@click";

		public const string EditTestvideo = "garage.edit.testvideo@click";

		public const string EditCOG = "garage.edit.cog@click";

		public const string EditPageNext = "garage.edit.page-next@click";

		public const string EditPagePrevious = "garage.edit.page-previous@click";

		public const string EditPagePreviousFocus = "garage.edit.page-previous@focus";

		public const string EditSpin = "garage.edit.spin@click";

		public const string EditDone = "garage.edit.done";

		public const string OpenGarage = "garage.open";

		public const string GarageIsOpen = "garage.isOpen";

		public const string GarageIsClosed = "garage.isClosed";

		public const string PropSpinImpulse = "garage.edit.prop-spin-impulse";

		public const string PropSpinStart = "garage.edit.prop-spin-start";

		public const string PropSpinStop = "garage.edit.prop-spin-stop";

		public const string ChangePart = "garage.edit.change-part";

		public const string ChangeStyle = "garage.edit.change-style";

		public const string ChangeFrame = "garage.edit.change-frame";

		public const string EditClearPhysics = "garage.edit.clear-physics@click";

		public const string EditReturnScreen = "garage.edit.back@click";

		public const string EditToggleDevelopment = "garage.edit.enabledev@click";

		public const string EditToggleAllowance = "garage.edit.enableunallowed@click";

		public const string EditFilter0FormEvent = "garage.edit.filter0.form.event";

		public const string EditFilter0FormClick = "garage.edit.filter0.form.event@click";

		public const string EditFilter0FormChange = "garage.edit.filter0.form.event@change";

		public const string EditFilter0FormSubmit = "garage.edit.filter0.form.event@submit";

		public const string EditFilter0FormFocus = "garage.edit.filter0.form.event@focus";

		public const string EditFilter1FormEvent = "garage.edit.filter1.form.event";

		public const string EditFilter1FormClick = "garage.edit.filter1.form.event@click";

		public const string EditFilter1FormChange = "garage.edit.filter1.form.event@change";

		public const string EditFilter1FormSubmit = "garage.edit.filter1.form.event@submit";

		public const string ChartsDatasheet = "garage.charts.datasheet@click";

		public const string ChartsReview = "garage.charts.review@click";

		public const string ChartsReviewNext = "garage.charts.review.next@click";

		public const string ChartsReviewPrevious = "garage.charts.review.previous@click";

		public const string DroneChanged = "garage.drone.changed";

		public const string DroneFCModeChanged = "garage.drone.fc-changed";

		public const string CommunityDronesFormEvent = "community-drones.form.event";

		public const string CommunityDronesFormEventClick = "community-drones.form.event@click";

		public const string CommunityDronesFormEventChange = "community-drones.form.event@change";

		public const string CommunityDronesFormEndEdit = "community-drones.form.event@end-edit";

		public const string CommunityDronesFormSubmit = "community-drones.form.event@submit";

		public const string CommunityDronesCreateNew = "community-drones.create-new@click";

		public const string CommunityDronesPageSelect = "community-drones.page@select";

		public const string CommunityDronesPageNext = "community-drones.page-next@click";

		public const string CommunityDronesPagePrevious = "community-drones.page-previous@click";

		public const string CommunityDronesCreateNew3 = "community-drones.create-new3@click";

		public const string CommunityDronesCreateNew4 = "community-drones.create-new4@click";

		public const string CommunityDronesCreateNew5 = "community-drones.create-new5@click";

		public const string CommunityDronesCreateNew6 = "community-drones.create-new6@click";

		public const string CommunityDronesCreateNew7 = "community-drones.create-new7@click";
	}

	public class Store
	{
		public const string StoreFormEvent = "store.form.event";

		public const string StoreFormEventClick = "store.form.event@click";

		public const string StoreFormEventChange = "store.form.event@change";

		public const string StoreFormEndEdit = "store.form.event@end-edit";

		public const string StoreItemPreviewClick = "store.item.preview@click";

		public const string StoreItemBuyClick = "store.item.buy@click";

		public const string StorePageSelect = "store.page@select";

		public const string StorePageNext = "store.page-next@click";

		public const string StorePagePrevious = "store.page-previous@click";
	}

	public class Circuits
	{
		public const string CircuitsCardEvent = "circuits.circuit-card@click";

		public const string CircuitsOverviewReset = "circuits.circuit-reset@click";

		public const string CircuitsMapSelection = "circiuits.circuit-map@click";

		public const string CircuitsOpponentFormEvent = "circuits.opponent-form.event";

		public const string CircuitsOpponentFormEventClick = "circuits.opponent-form.event@click";

		public const string CircuitsOpponentFormEventChange = "circuits.opponent-form.event@change";

		public const string CircuitsOverviewExitClick = "circuits.circuit-overview.exit@click";

		public const string CircuitsSelectionExitClick = "circuits.circuit-selection.exit@click";

		public const string CircuitsOverviewSelectionClick = "circuits.circuit-overview.selection@click";

		public const string CircuitsOverviewLeaderClick = "circuits.circuit-overview.leader@click";
	}

	public class UI
	{
		public const string ScreenOpen = "ui.screen@open";

		public const string ScreenClose = "ui.screen@close";

		public const string ScreenChange = "ui.screen@change";

		public const string ScreenSwitch = "ui.screen@switch";

		public const string ScreenReturn = "ui.screen@return";

		public const string ScreenPreviewClick = "ui.screen.preview@click";

		public const string ScreenReturnClick = "ui.screen.return@click";

		public const string ScreenReturnFocus = "ui.screen.return@focus";

		public const string ScreenNavLeftClick = "ui.screen.nav-left@click";

		public const string ScreenNavRightClick = "ui.screen.nav-right@click";

		public const string ScreenHistoryAdd = "ui.screen.history.add";

		public const string ScreenHistoryRemove = "ui.screen.history.remove";

		public const string ScreenNavigationModeChange = "ui.screen.navigation-mode@change";

		public const string ScreenBreadCrumbClick = "ui.screen.breadcrumb@click";

		public const string FooterOpen = "ui.footer@open";

		public const string FooterClose = "ui.footer@close";

		public const string FooterCalibrateController = "ui.footer.calibrate@click";

		public const string FooterDrone = "ui.footer.drone@click";

		public const string FooterSocial = "ui.footer.social@click";

		public const string FooterProfile = "ui.footer.profile@click";

		public const string FooterSettings = "ui.footer.settings@click";

		public const string FooterConnection = "ui.footer.connection@click";

		public const string FooterExit = "ui.footer.exit@click";

		public const string PauseExit = "ui.pause.exit@click";

		public const string ResetLeaderboards = "ui.reset.leaderboards@click";

		public const string ResetTrackLeaderboard = "ui.reset.track-leaderboard@click";

		public const string TooltipShow = "ui.tooltip@show";

		public const string TooltipHide = "ui.tooltip@hide";

		public const string ScreenVideoPlayerOpen = "ui.screen.video-player@open";

		public const string ScreenVideoPlayerPlay = "ui.screen.video-player@play";

		public const string ScreenVideoPlayerFinished = "ui.screen.video-player@end";

		public const string ScreenVideoPlayerStop = "ui.screen.video-player-close@click";

		public const string DialogButtonConfirmClick = "ui.dialog.button.confirm@click";

		public const string DialogButtonCancelClick = "ui.dialog.button.cancel@click";

		public const string DialogNavRightClick = "ui.dialog.nav.right@click";

		public const string DialogNavLeftClick = "ui.dialog.nav.left@click";

		public const string DialogToggleClick = "ui.dialog.toggle@click";

		public const string SocialMediaLinkClick = "social-media.link@click";

		public const string SliderHandleDeselect = "ui.slider.handle@unfocus";
	}

	public class Storage
	{
		public const string StateRefresh = "storage.state@refresh";

		public const string ProgressionRefresh = "storage.progression@refresh";

		public const string StateParse = "storage.state@parse";

		public const string StateWrite = "storage.state@write";

		public const string GameStateParse = "storage.game.state@parse";

		public const string LicenseChange = "storage.license@change";

		public const string LicenseRefresh = "storage.license@refresh";

		public const string LocalizationRefresh = "storage.localization@refresh";

		public const string DroneRefresh = "storage.drone@refresh";
	}

	public class Service
	{
		public const string GameStateRefresh = "service.state.game@refresh";

		public const string GameStateError = "service.state.game@error";

		public const string StateRefresh = "service.state@refresh";

		public const string StateWrite = "service.state.write";

		public const string StateError = "service.state@error";

		public const string LoginSuccess = "service.login@success";

		public const string LoginValidateSuccess = "service.login.validate@success";

		public const string LoginError = "service.login@error";

		public const string LoginValidateError = "service.login.validate@error";

		public const string ContentManifestSuccess = "service.content.manifest@success";

		public const string ContentManifestError = "service.content.manifest@error";

		public const string TimeRefresh = "service.time@refresh";

		public const string TimeError = "service.time@error";

		public const string LicenseError = "service.license@error";

		public const string TryoutsStatusSuccess = "service.tryouts.status@success";

		public const string TryoutsStatusError = "service.tryouts.status@error";
	}

	public class Achievements
	{
		public const string Init = "achievements.state@init";

		public const string Refresh = "achievements.state@refresh";

		public const string AchievementsRefresh = "achievements.state@refresh";

		public const string AchievementsPageSelect = "achievements.state@select";

		public const string AchievementsPagePrevious = "achievements.page-previous@click";

		public const string AchievementsPageNext = "achievements.page-next@click";

		public const string AchievementsButtonClick = "settings.profile.achievements@click";

		public const string AchievementsDetailClick = "settings.profile.achievements.detail@click";

		public const string AchievementsDetailUpdate = "settings.profile.achievements.detail.update";
	}

	public class Main
	{
	}

	public class State
	{
		public const string TimeRefresh = "state.time@refresh";
	}

	public class Settings
	{
		public const string Ready = "settings.ready";

		public const string StartApplyGraphics = "settings.startup.graphics.apply";

		public const string GraphicsApply = "settings.graphics.apply";

		public const string GraphicsResolution = "settings.graphics.resolution";

		public const string GraphicsQuality = "settings.graphics.quality";

		public const string GraphicsMapLightingApply = "settings.graphics.map.lighting.apply";

		public const string SoundApply = "settings.sound.apply";

		public const string SoundVolume = "settings.sound.volume";

		public const string SoundVolumeMusic = "settings.sound.volume-music";

		public const string SoundVolumeSFX = "settings.sound.volume-sfx";

		public const string LanguageApply = "settings.language.apply";

		public const string ProfileColorApply = "settings.profile-color.apply";

		public const string ControllerConnect = "settings.controller.connect";

		public const string ControllerPredisconnect = "settings.controller.predisconnect";

		public const string ControllerDisconnect = "settings.controller.disconnect";

		public const string ControllerClick = "settings.controller@click";

		public const string SystemClick = "settings.system@click";

		public const string TuningClick = "settings.tuning@click";

		public const string GameClick = "settings.game@click";

		public const string HelpClick = "settings.help@click";

		public const string LegalClick = "settings.legal@click";

		public const string ControllerProfileClick = "settings.controller.profile@click";

		public const string ControllerProfileNewClick = "settings.controller.profile.new@click";

		public const string ControllerProfileMapClick = "settings.controller.profile.map@click";

		public const string ControllerProfileHelpClick = "settings.controller.profile.help@click";

		public const string ControllerProfileEvent = "settings.controller.profile";

		public const string ControllerProfileCustom = "settings.controller.profile.custom@click";

		public const string ControllerHelpClick = "settings.controller.help@click";

		public const string ControllerCalibrationFormClick = "settings.controller.profile.calibration.form.event@click";

		public const string ControllerCalibrationFormChange = "settings.controller.profile.calibration.form.event@change";

		public const string ControllerCalibrationCenterPointMode = "settings.controller.profile.calibration.centerpoint@click";

		public const string ControllerCalibrationRawInputMode = "settings.controller.profile.calibration.rawinput@click";

		public const string ControllerCalibrationLeftStickY = "settings.controller.profile.calibration.ly.c@click";

		public const string ControllerCalibrationLeftStickX = "settings.controller.profile.calibration.lx.c@click";

		public const string ControllerCalibrationRightStickY = "settings.controller.profile.calibration.ry.c@click";

		public const string ControllerCalibrationRightStickX = "settings.controller.profile.calibration.rx.c@click";

		public const string ControllerCalibrationToggleA = "settings.controller.profile.calibration.ta.c@click";

		public const string ControllerCalibrationToggleB = "settings.controller.profile.calibration.tb.c@click";

		public const string ControllerCustomFindNext = "settings.controller.custom.findnext@click";

		public const string ControllerCustomToggleGamepad = "settings.controller.custom.togglegamepad@click";

		public const string ControllerCustomToggleInputMode = "settings.controller.custom.toggleinputmode@click";

		public const string ControllerCalibrationSave = "settings.controller.profile.save";

		public const string ProfileFormClick = "settings.profile.form.event@click";

		public const string ProfileFormChange = "settings.profile.form.event@change";

		public const string ProfileColorClick = "settings.profile.color@click";

		public const string ProfileColorFocus = "settings.profile.color@focus";

		public const string ProfileColorUnfocus = "settings.profile.color@unfocus";

		public const string ProfileColorPickerClick = "settings.profile.color-picker@click";

		public const string ProfileColorPickerFocus = "settings.profile.color-picker@focus";

		public const string ProfileColorPickerUnfocus = "settings.profile.color-picker@unfocus";

		public const string ProfileColorChanged = "settings.profile.color@changed";

		public const string PlayerDataClick = "settings.profile.player-data@click";

		public const string ProgressionManualClick = "settings.profile.progression-manual@click";

		public const string ProgressionRankEnable = "settings.profile.progression.rank.enable";

		public const string ProgressionRankFinish = "settings.profile.progression.rank.finish";

		public const string DiscardDataClick = "settings.profile.discard-data@click";

		public const string SaveDataClick = "settings.profile.save-data@click";

		public const string TuningProfilePresetClick = "settings.tuning.profile.preset@click";

		public const string TuningProfileItemClick = "settings.tuning.profile.item@click";

		public const string TuningProfileSave = "settings.tuning.profile.save";

		public const string TuningFormClick = "settings.tuning.form.element@click";

		public const string TuningFormChange = "settings.tuning.form.element@change";

		public const string TuningFormTiltChange = "settings.game.form.tilt";

		public const string TuningFormFOVChange = "settings.game.form.fov";

		public const string TuningFormDroneClick = "settings.tuning.drone@click";

		public const string SystemFormEventClick = "settings.system.form.event@click";

		public const string SystemFormEventChange = "settings.system.form.event@change";

		public const string SystemScreenApply = "settings.system.screen.apply";

		public const string SystemQualityGroupApply = "settings.system.quality-group.apply";

		public const string GameFormEventClick = "settings.game.form.event@click";

		public const string GameFormEventChange = "settings.game.form.event@change";

		public const string GameScreenApply = "settings.game.screen.apply";

		public const string RaceLineColorClick = "settings.race-line-color.color@click";

		public const string RaceLineColorFocus = "settings.race-line-color.color@focus";

		public const string RaceLineColorUnfocus = "settings.race-line-color.color@unfocus";

		public const string RaceLineColorPickerClick = "settings.race-line-color.color-picker@click";

		public const string RaceLineColorPickerFocus = "settings.race-line-color.color-picker@focus";

		public const string RaceLineColorPickerUnfocus = "settings.race-line-color.color-picker@unfocus";

		public const string RaceLineColorChanged = "settings.race-line-color.color@changed";

		public const string CheckPointColorClick = "settings.check-point-color.color@click";

		public const string CheckPointColorFocus = "settings.check-point-color.color@focus";

		public const string CheckPointColorUnfocus = "settings.check-point-color.color@unfocus";

		public const string CheckPointColorPickerClick = "settings.check-point-color.color-picker@click";

		public const string CheckPointColorPickerFocus = "settings.check-point-color.color-picker@focus";

		public const string CheckPointColorPickerUnfocus = "settings.check-point-color.color-picker@unfocus";

		public const string CheckPointColorChanged = "settings.check-point-color.color@changed";
	}

	public class Leaderboards
	{
		public const string FilterFormEventClick = "leaderboards.filter.form.event@click";

		public const string FilterFormEventChange = "leaderboards.filter.form.event@change";

		public const string FilterFormEventSubmit = "leaderboards.filter.form.event@submit";

		public const string PageSelect = "leaderboards.page@select";

		public const string PageNext = "leaderboards.page-next@click";

		public const string PagePrevious = "leaderboards.page-previous@click";

		public const string ItemClick = "leaderboards.item@click";

		public const string ItemReplayClick = "leaderboards.item.replay@click";

		public const string ReplayLoadComplete = "leaderboards.replay.load@complete";

		public const string ItemSaveDroneClick = "leaderboards.item.savedrone@click";

		public const string ChooseMapClick = "leaderboards.choose-map@click";

		public const string LeaderboardSearch = "leaderboards.search@submit";

		public const string LeaderboardSearchClick = "leaderboards.search@click";

		public const string LeaderboardSearchReset = "leaderboards.search.reset@click";

		public const string ChooseResetMapClick = "leaderboards.choose-reset-map@click";

		public const string ResetFilterFormEventClick = "leaderboards.reset.filter.form.event@click";

		public const string ResetFilterFormEventChange = "leaderboards.reset.filter.form.event@change";

		public const string ResetFilterFormEventSubmit = "leaderboards.reset.filter.form.event@submit";
	}

	public class OpponentSelection
	{
		public const string FilterFormEventClick = "opponent-selection.filter.form.event@click";

		public const string FilterFormEventChange = "opponent-selection.filter.form.event@change";

		public const string FilterFormEventSubmit = "opponent-selection.filter.form.event@submit";

		public const string PageSelect = "opponent-selection.page@select";

		public const string PageNext = "opponent-selection.page-next@click";

		public const string PagePrevious = "opponent-selection.page-previous@click";

		public const string ItemClick = "opponent-selection.item@click";

		public const string ItemReplayClick = "opponent-selection.item.replay@click";

		public const string ReplayLoadComplete = "opponent-selection.replay.load@complete";

		public const string ItemSaveDroneClick = "opponent-selection.item.savedrone@click";

		public const string ChooseMapClick = "opponent-selection.choose-map@click";

		public const string OpponentSelectionStartClick = "opponent-selection.start@click";
	}

	public class Game
	{
		public const string NPCOverlayNextClick = "game.npc-overlay.next@click";

		public const string NPCOverlayBackClick = "game.npc-overlay.back@click";

		public const string NPCOverlayExitClick = "game.npc-overlay.exit@click";

		public const string SimulationDroneAdd = "game.simulation.drone@add";

		public const string SimulationDroneRemove = "game.simulation.drone@remove";

		public const string SimulationDroneReplace = "game.simulation.drone@replace";

		public const string SimulationDroneReady = "game.simulation.drone@ready";

		public const string SimulationAllDroneReady = "game.simulation.drone.all@ready";

		public const string SimulationDroneArmed = "game.simulation.drone@armed";

		public const string SimulationDroneDisarmed = "game.simulation.drone@disarmed";

		public const string SimulationANTArmed = "game.simulation.arm-and-turtle@armed";

		public const string SimulationANTDisarmed = "game.simulation.arm-and-turtle@disarmed";

		public const string SimulationDroneTurtleOn = "game.simulation.drone.turtle@on";

		public const string SimulationDroneTurtleOff = "game.simulation.drone.turtle@off";

		public const string SimulationCameraAdd = "game.simulation.camera@add";

		public const string SimulationLoadComplete = "game.simulation.load@complete";

		public const string SimulationDroneCollision = "game.simulation.drone@collision";

		public const string SimulationDroneScrape = "game.simulation.drone@scrape";

		public const string SimulationDronePropScrape = "game.simulation.drone@prop-scrape";

		public const string SimulationDroneCrash = "game.simulation.drone@crash";

		public const string SimulationDroneRecover = "game.simulation.drone@recover";

		public const string SimulationDroneFlip = "game.simulation.drone@flip";

		public const string SimulationDroneUpdateFlightTime = "game.simulation.drone.flight-time@update";

		public const string DroneSignalUpdate = "game.drone.signal-update";

		public const string DroneSignalLost = "game.drone.signal-lost";

		public const string DroneSignalDrop = "game.drone.signal-drop";

		public const string DroneSignalRecover = "game.drone.signal-recover";

		public const string DroneSignalFull = "game.drone.signal-full";

		public const string DronesUnfrozen = "game.drones.unfrozen";

		public const string TrackLoadComplete = "game.track.load@complete";

		public const string LevelLoadComplete = "game.level.load@complete";

		public const string IntroAnimationComplete = "game.intro.animation@complete";

		public const string IntroAnimationStart = "game.intro.animation@start";

		public const string DroneDebugDashboardToggle = "game.ui.debug.dashboard@toggle";

		public const string DroneDashboardShow = "game.ui.dashboard@show";

		public const string DroneDashboardHide = "game.ui.dashboard@hide";

		public const string DroneDashboardToggle = "game.ui.dashboard@toggle";

		public const string DroneDashboardFormEvent = "game.ui.dashboard.form.event";

		public const string DroneDashboardFormEventClick = "game.ui.dashboard.form.event@click";

		public const string DroneDashboardFormEventChange = "game.ui.dashboard.form.event@change";

		public const string DroneDashboardFormEventEndEdit = "game.ui.dashboard.form.event@end-edit";

		public const string DroneDashboardFormEventStartEdit = "game.ui.dashboard.form.event@start-edit";

		public const string DroneDashboardFormEventFocus = "game.ui.dashboard.form.event@focus";

		public const string PauseFormEvent = "game.pause.form.event";

		public const string PauseFormEventClick = "game.pause.form.event@click";

		public const string PauseFormEventChange = "game.pause.form.event@change";

		public const string PauseProModeClick = "game.pause.pro-card@click";

		public const string PauseProModeOver = "game.pause.pro-card@over";

		public const string PauseProModeOut = "game.pause.pro-card@out";

		public const string Pause = "game.pause";

		public const string PauseExitClick = "game.pause.exit@click";

		public const string PauseReturnClick = "game.pause.return@click";

		public const string Unpause = "game.unpause";

		public const string Restart = "game.restart";

		public const string ChangeGameClick = "game.change-game@click";

		public const string ChangeMissionClick = "game.change-mission@click";

		public const string CountStart = "game.count@start";

		public const string CountStep = "game.count@step";

		public const string CountComplete = "game.count@complete";

		public const string StandingsUpdate = "game.standings@update";

		public const string RequestRaceForfeit = "game.race.request-forfeit";

		public const string RequestRaceRestart = "game.race.request-restart";

		public const string RaceGateStep = "game.race.gate@step";

		public const string RaceLapStep = "game.race.lap@step";

		public const string RaceLapChange = "game.race.lap@change";

		public const string RaceGateComplete = "game.race.gate@complete";

		public const string RaceEndSlowmotionEffectStart = "game.race.slowmo@start";

		public const string LeaderboardComplete = "game.race.leaderboard-complete";

		public const string LeaderboardSet = "game.race.leaderboard-set";

		public const string RaceEndSlowmotionEffectStop = "game.race.slowmo@stop";

		public const string RaceEnabled = "game.race.enabled";

		public const string RaceComplete = "game.race.complete";

		public const string RaceReplayUploadStarted = "game.race.replay-upload@start";

		public const string RaceReplayUploadComplete = "game.race.replay-upload@complete";

		public const string RaceReplayStorageTempComplete = "game.race.replay-storage@complete";

		public const string RaceCompleteRestartClick = "game.race-complete.restart@click";

		public const string RaceCompleteExitClick = "game.race-complete.exit@click";

		public const string RaceCompleteNextClick = "game.race-complete.next@click";

		public const string RaceCompleteSpectateClick = "game.race-complete.spectate@click";

		public const string RaceCompleteShareClick = "game.race-complete.share@click";

		public const string RaceCompleteSettingsClick = "game.race-complete.settings@click";

		public const string RaceCompleteMapRating = "game.race-complete.map-rating@click";

		public const string RaceCompleteDroneRating = "game.race-complete.drone-rating@click";

		public const string RaceProcessReplayStart = "game.race.process-replay.start";

		public const string RaceProcessReplayComplete = "game.race.process-replay.complete";

		public const string RaceCompleteTimeAnimationStart = "game.race-complete.time.animation@start";

		public const string RaceCompleteTimeAnimationComplete = "game.race-complete.time.animation@complete";

		public const string TournamentRaceCompleteNextClick = "game.tournament-race-complete.next@click";

		public const string RaceOverviewRestartClick = "game.race-overview.restart@click";

		public const string RaceOverviewMapsClick = "game.race-overview.maps@click";

		public const string RaceOverviewReplayClick = "game.race-overview.replay@click";

		public const string RaceOverviewRoomClick = "game.race-overview.room@click";

		public const string RaceOverviewNextClick = "game.race-overview.next@click";

		public const string RaceOverviewCampaignClick = "game.race-overview.campaign@click";

		public const string RaceOverviewSettingsClick = "game.race-overview.settings@click";

		public const string RaceOverviewExitClick = "game.race-overview.exit@click";

		public const string RaceOverviewCircuitsClick = "game.race-overview.circuits@click";

		public const string TournamentResultsSubmit = "game.tournament.results@submit";

		public const string RaceOverviewFavoriteChange = "game.race-overview.favorite@change";

		public const string RaceOverviewFavoriteClick = "game.race-overview.favorite@click";

		public const string VoteTrackCardClick = "ui.game.vote-track.card@click";

		public const string VoteTrackCardFocus = "ui.game.vote-track.card@focus";

		public const string VoteTrackCardUnfocus = "ui.game.vote-track.card@unfocus";

		public const string Boot = "game.boot";

		public const string Initialized = "game.ready";

		public const string CheatWarning = "game.cheat.warning";

		public static string CameraModeChanged = "game.camera.mode@change";
	}

	public class Viewer
	{
		public const string ControlsRaceSkipClick = "viewer.controls.race-skip@click";

		public const string ControlsNavSettingsClick = "viewer.controls.nav.settings@click";

		public const string ControlsNavExitClick = "viewer.controls.nav.exit@click";

		public const string FormEventClick = "viewer.form.event@click";

		public const string FormEventChange = "viewer.form.event@change";
	}

	public class Spectate
	{
		public const string FormEventClick = "spectate.form.event@click";

		public const string FormEventChange = "spectate.form.event@change";

		public const string FormEventOver = "spectate.form.event@over";

		public const string FormEventOut = "spectate.form.event@out";

		public const string TargetsChange = "spectate.targets@change";

		public const string TargetsReady = "spectate.targets.ready";

		public const string TargetSelect = "spectate.target.select";

		public const string CameraModeChange = "spectate.camera-mode@change";

		public const string CameraToolsChange = "spectate.camera-tools@change";

		public const string CameraToolFocusListChange = "spectate.camera-tool.focus-list@change";

		public const string CameraToolFocusChange = "spectate.camera-tool.focus@change";

		public const string CourseCamerasChange = "spectate.course-cameras@change";

		public const string CourseCameraModeChange = "spectate.course-camera-mode@change";

		public const string CourseCameraFocusChange = "spectate.course-camera.focus@change";

		public const string DroneTrailModeChange = "spectate.drone-trail-mode@change";

		public const string DroneTrailWidthModeChange = "spectate.drone-trail-width-mode@change";

		public const string FocusChange = "spectate.focus@change";

		public const string Pausecommand = "spectate.pause-command";
	}

	public class Network
	{
		public const string ConnectionStart = "network.connection@start";

		public const string ConnectionComplete = "network.connection@complete";

		public const string ConnectionError = "network.connection@error";

		public const string Disconnect = "network.disconnect";

		public const string LobbyEnter = "network.lobby@enter";

		public const string LobbyExit = "network.lobby@exit";

		public const string LobbyUpdate = "network.lobby@update";

		public const string LobbyRoomList = "network.lobby.room-list";

		public const string LobbyJoinFailed = "network.lobby.join-failed";

		public const string EnableServerList = "network.lobby.server-list@enable";

		public const string DisableServerList = "network.lobby.server-list@disable";

		public const string EnableFooterNetwork = "network.footer@enable";

		public const string DisableFooterNetwork = "network.footer@disable";

		public const string LoadLevel = "network.load-level";

		public const string CustomMapLoadStart = "network.custom-map.load@start";

		public const string PingUpdate = "network.ping.update";

		public const string RoomEnter = "network.room@enter";

		public const string RoomLock = "network.room@lock";

		public const string RoomLoadGame = "network.room.load-game";

		public const string RoomExit = "network.room@exit";

		public const string RoomUpdate = "network.room.update";

		public const string RoomCreateError = "network.room-create@error";

		public const string RoomEnterError = "network.room-enter@error";

		public const string RoomIncomingChat = "network.room.chat.incoming";

		public const string RoomWithoutRacers = "network.room.no.racers";

		public const string RoomVoteTrackGenerated = "network.room.vote-track.generated";

		public const string RoomMasterChanged = "network.room.master.changed";

		public const string RoomFullError = "network.room.full";

		public const string RoomNotActiveError = "network.room.not-active";

		public const string CrossplayMismatchError = "network.crossplay.mismatch";

		public const string QuickMatchStateChanged = "network.qm.state.changed";

		public const string PlayerRoomEnter = "network.player.room@enter";

		public const string PlayerRoomExit = "network.player.room@exit";

		public const string PlayerUpdate = "network.player@update";

		public const string PlayerKicked = "network.player-kicked";

		public const string PlayerToRacer = "network.player.racer";

		public const string PlayerToSpectator = "network.player.spectator";

		public const string PlayerMarkedReady = "network.player.marked.ready";

		public const string PlayerReadyAll = "network.player.all.ready";

		public const string PlayerVotedTrack = "network.player.voted.track";

		public const string PlayerCompletedRace = "network.player.completed.race";

		public const string PlayerForfeitRace = "network.player.forfeit.race";

		public const string PlayerCrashed = "network.player.crashed";

		public const string PlayerDamage = "network.player.damage";

		public const string PlayerRecovered = "network.player.recovered";

		public const string PlayerAssignMaster = "network.player.master@click";

		public const string PlayerOrderUpdate = "network.player.order@update";

		public const string FirstRacerFinished = "network.room.first-racer-finshed";

		public const string InstantiateLocalDrone = "network.instantiate.local";

		public const string InstantiateRemoteDrone = "network.instantiate.remote";

		public const string InstantiateRace = "network.instantiate.race";

		public const string LocalDroneAdded = "network.local.transmitter.added";

		public const string RemoteDroneAdded = "network.remote.transmitter.added";

		public const string RemoteDroneFinished = "network.remote.drone.finished";

		public const string DroneRigChanged = "network.drone.changed";

		public const string RaceCount = "network.race.count";

		public const string RaceCountComplete = "network.race.count@complete";

		public const string RaceGateHit = "network.race.gate@hit";

		public const string RaceEnd = "network.race.end";

		public const string RaceReplayIncoming = "network.race.replay.incoming";

		public const string RaceReplayReady = "network.race.replay.ready";

		public const string RaceReplayReadyAll = "network.race.replay.ready.all";

		public const string LANServerStarting = "network.LAN.starting";

		public const string LANServerOnline = "network.LAN.online";

		public const string LANServerStopping = "network.LAN.stopping";

		public const string LANServerOffline = "network.LAN.offline";

		public const string OnSendGameInvite = "network.room.invite";

		public const string UpdatingGhostData = "network.ghosts.update-ui";

		public const string GhostDataRefreshed = "network.ghosts.refreshed-ui";

		public const string GhostsLoadingStateChanged = "network.ghosts.status";

		public const string GhostsCountChanged = "network.ghosts.count";

		public const string StateOnline = "network.state.online";

		public const string StateOffline = "network.state.offline";

		public const string UpdateOffline = "network.update.offline";

		public const string PullUsersIntoMatch = "network.tournament.pull-users";

		public const string MapSelectionComplete = "network.selection-complete";

		public const string DroneDamageUpdate = "network.drone-damage.update";
	}

	public class Multiplayer
	{
		public const string LobbyFormEvent = "multiplayer.lobby.form.event";

		public const string LobbyFormEventClick = "multiplayer.lobby.form.event@click";

		public const string LobbyFormEventChange = "multiplayer.lobby.form.event@change";

		public const string LobbyFormEventEndEdit = "multiplayer.lobby.form.event@end-edit";

		public const string LobbyPageSelect = "multiplayer.lobby.page@select";

		public const string LobbyPageNext = "multiplayer.lobby.page.next@click";

		public const string LobbyPagePrevious = "multiplayer.lobby.page.previous@click";

		public const string LobbyItemEntryClick = "multiplayer.lobby.item.entry@click";

		public const string LobbyItemActionClick = "multiplayer.lobby.item.action@click";

		public const string LobbyItemPrivateJoinClick = "multiplayer.lobby.item.private.join@click";

		public const string LobbyServerListButtonClick = "multiplayer.lobby.server-list-button@click";

		public const string LobbyLANStartServerClick = "multiplayer.lobby.lan-start@click";

		public const string LobbyLANStartServerFocus = "multiplayer.lobby.lan-start@focus";

		public const string LobbyLANStartServerUnfocus = "multiplayer.lobby.lan-start@unfocus";

		public const string LobbyLANConnect = "multiplayer.lobby.lan-connect";

		public const string LobbyLANDisconnect = "multiplayer.lobby.lan-disconnect";

		public const string LobbyLANTryConnectUIClick = "multiplayer.lobby.lan-ui-connect@click";

		public const string LobbyLANInputIPChange = "multiplayer.lobby.lan-ip@change";

		public const string LobbyLANDisconnectClick = "multiplayer.lobby.lan-disconnect@click";

		public const string LobbyServerListItemClick = "multiplayer.lobby.server-list-item@click";

		public const string RoomFormEvent = "multiplayer.room.form.event";

		public const string RoomFormEventClick = "multiplayer.room.form.event@click";

		public const string RoomFormEventChange = "multiplayer.room.form.event@change";

		public const string RoomFormEventEndEdit = "multiplayer.room.form.event@end-edit";

		public const string RoomMapVoteItemClick = "multiplayer.room.map-vote.item@click";

		public const string LANConnected = "multiplayer.lan.connected";

		public const string LANDisconnected = "multiplayer.lan.disconnected";

		public const string RoomRacerItemClick = "multiplayer.room.racer-item@click";

		public const string RoomSpectatorItemClick = "multiplayer.room.spectator-item@click";

		public const string RoomRacerItemMenuClick = "multiplayer.room.racer-item.menu@click";

		public const string RoomSpectatorMenuItemClick = "multiplayer.room.spectator-item.menu@click";

		public const string RoomNotificationInviteClick = "multiplayer.room.invite@click";

		public const string ChatInputChange = "multiplayer.chat.input@change";

		public const string ChatInputEndEdit = "multiplayer.chat.input@end-edit";

		public const string ChatPanelClick = "multiplayer.chat.panel@click";
	}

	public class Intro
	{
		public const string IntroPhysicsStep01NextClick = "fn.intro.step01.next@click";

		public const string IntroPhysicsStep02NextClick = "fn.intro.step02.next@click";

		public const string IntroPhysicsStep03NextClick = "fn.intro.step03.next@click";

		public const string IntroPhysicsStep04NextClick = "fn.intro.step04.next@click";

		public const string IntroPhysicsStep05NextClick = "fn.intro.step05.next@click";

		public const string IntroPhysicsExitClick = "fn.intro.exit@click";

		public const string IntroPhysicsCalibrationClick = "fn.intro.calibration@click";

		public const string IntroPhysicsCalibrationOpen = "intro.calibration@open";

		public const string IntroPhysicsCalibrationClose = "fn.intro.calibration-close@click";

		public const string IntroPhysicsClose = "intro.screens.close";

		public const string IntroPhysicsGraphicsOpen = "intro.graphics@open";

		public const string IntroPhysicsGraphicsClick = "fn.intro.graphics@click";

		public const string IntroPhysicsGraphicsClose = "fn.intro.graphics.screen@close";

		public const string IntroPhysicsSandboxOpen = "intro.sandbox@open";

		public const string IntroPhysicsSandboxClick = "fn.intro.sandbox@click";

		public const string ControllerStoreOpen = "fn.intro.controller-store@click";
	}

	public class Social
	{
		public const string ToggleSocialPanelClick = "social.panel.toggle@click";

		public const string SocialPanelTabClick = "social.panel.tab@click";

		public const string SocialPanelTabChange = "social.panel.tab@change";

		public const string SocialPanelHidden = "social.panel.hidden";

		public const string SocialPanelShown = "social.panel.shown";

		public const string FriendsSortOrderChange = "social.friends.order@change";

		public const string FriendsRefresh = "service.social.friends@refresh";

		public const string FriendInviteStart = "service.social.friends.invite@start";

		public const string FriendInviteSuccess = "service.social.friends.invite@success";

		public const string FriendInviteComplete = "service.social.friends.invite@complete";

		public const string FriendInviteFail = "service.social.friends.invite@fail";

		public const string FriendsSubmenuToggle = "social.friend.item@click";

		public const string FriendsSubmenuOpen = "social.friend.item@open";

		public const string FriendsSubmenuClose = "social.friend.item@close";

		public const string FriendsItemFocus = "social.friend.item@focus";

		public const string FriendsPrivateMessage = "social.friend.pm-button@click";

		public const string FriendsJoinLobbyClick = "social.friend.join-button@click";

		public const string FriendAddStart = "social.friend.add-friend-button@click";

		public const string FriendAddSuccess = "social.friend.add-friend-button@success";

		public const string FriendAddFail = "social.friend.add-friend-button@fail";

		public const string FriendRemove = "social.friend.remove-friend-button@click";

		public const string NotificationBadgesDirty = "social.badges.dirty";

		public const string NotificationBadgesClear = "social.badges.clear";

		public const string ChatBadgesClear = "social.chat.badges.clear";

		public const string FriendsSearchInputFieldFocus = "social.friends.search.form@focus";

		public const string FriendsSearchInputFieldUnfocus = "social.friends.search.form@unfocus";

		public const string ThreadRead = "social.threads.read";
	}

	public class Chat
	{
		public const string Connected = "chat.server.connected";

		public const string Disconnected = "chat.server.disconnected";

		public const string Connecting = "chat.server.connecting";

		public const string ChatRoomJoined = "chat.room.joined";

		public const string ChatRoomLeft = "chat.room.left";

		public const string ChatInputChange = "chat.message.input@change";

		public const string ChatInputEndEdit = "chat.message.input@end-edit";

		public const string ChatInputUnfocus = "chat.message.input@unfocus";

		public const string ChatPrivateInputChange = "chat.message.private.input@change";

		public const string ChatPrivateInputEndEdit = "chat.message.private.input@end-edit";

		public const string ChatPrivateInputUnfocus = "chat.message.private.input@unfocus";

		public const string IncomingPublicMessage = "chat.incoming.public";

		public const string IncomingPrivateMessage = "chat.incoming.private";

		public const string IncomingGameInvite = "chat.incoming.invite";

		public const string StartPrivateChat = "chat.private.invite";

		public const string ChatChannelChange = "chat.channel@change";

		public const string ChatChannelClick = "chat.channel@click";

		public const string ChatPanelClick = "chat.panel@click";

		public const string ChatPanelActive = "chat.panel@active";

		public const string ChatPanelInactive = "chat.panel@inactive";

		public const string ChatMessageFocus = "chat.message@focus";

		public const string ChatMessageClick = "chat.message@click";

		public const string UserSubmenuUnFold = "chat.ui.submenu.unfold";

		public const string FriendRemove = "chat.friend-remove";

		public const string PlayerJoinedChannel = "chat.channnel.player.joined";

		public const string PlayerLeftChannel = "chat.channnel.player.left";

		public const string ChatInfoHelp = "chat.info.help@click";

		public const string ChatInfoDiscord = "chat.info.discord@click";

		public const string ChatInfoZendesk = "chat.info.zendesk@click";

		public const string ChatInfoSteam = "chat.info.steam@click";

		public const string ToggleInGameChat = "chat.toggle";

		public const string ToggleChatHeight = "chat.toggle.height";

		public const string BlockUser = "chat.block-user@click";

		public const string UnBlockUser = "chat.unblock-user@click";
	}

	public class Notifications
	{
		public const string Queue = "notifications.queue";

		public const string Push = "notifications.push";

		public const string PushMessage = "notifications.push-message";

		public const string PushWarning = "notifications.push-warning";

		public const string PushError = "notifications.push-error";

		public const string Remove = "notifications.remove";

		public const string Receive = "notifications.receive";

		public const string Action = "notifications.action";

		public const string TournamentOpened = "notifications.tournament.opened";

		public const string TournamentStarted = "notifications.tournament.started";

		public const string TournamentStarting = "notifications.tournament.soon-to-start";

		public const string PanelToggleClick = "notifications.ui.panel.toggle@click";

		public const string AcceptClick = "notifications.ui.accept@click";

		public const string DeclineClick = "notifications.ui.decline@click";

		public const string DetailsClick = "notifications.ui.details@click";

		public const string CloseClick = "notifications.ui.close@click";

		public const string Snooze15Click = "notifications.ui.snooze.15@click";

		public const string Snooze30Click = "notifications.ui.snooze.30@click";

		public const string Snooze60Click = "notifications.ui.snooze.60@click";

		public const string Snooze90Click = "notifications.ui.snooze.90@click";

		public const string SnoozeClear = "notifications.ui.snooze.clear";

		public const string HeaderTabChange = "notifications.ui.header.tab@change";

		public const string HeaderTabClick = "notifications.ui.header.tab@click";

		public const string Expired = "notifications.ui.expired";

		public const string Timeout = "notifications.ui.timeout";

		public const string Connected = "notifications.ui.connected";

		public const string Registered = "notifications.ui.registered";

		public const string Joined = "notifications.ui.joined";

		public const string CardClick = "notifications.ui.card@click";

		public const string CardOver = "notifications.ui.card@over";

		public const string CardOut = "notifications.ui.card@out";
	}

	public class MapEditor
	{
		public const string Initialized = "map-editor.ready";

		public const string InputEvent = "map-editor.input.event";

		public const string InputStateChange = "map-editor.input.state.change";

		public const string ActionStateChange = "map-editor.action.state.change";

		public const string RenderStateChange = "map-editor.render.state.change";

		public const string PivotStateChange = "map-editor.pivot.state.change";

		public const string FormEvent = "map-editor.form.event";

		public const string FormEventClick = "map-editor.form.event@click";

		public const string FormEventChange = "map-editor.form.event@change";

		public const string FormEventEndEdit = "map-editor.form.event@end-edit";

		public const string RightTab = "map-editor.right.tab";

		public const string MetricModeChange = "map-editor.metric.mode.change";

		public const string MetricRulerStateChange = "map-editor.metric.ruler.state.change";

		public const string MetricSnapMoveDirty = "map-editor.metric.snap.move.dirty";

		public const string MetricSnapRotateDirty = "map-editor.metric.snap.rotate.dirty";

		public const string ControlDirty = "map-editor.control.dirty";

		public const string ControlBeginChange = "map-editor.control.begin-change";

		public const string ControlEndChange = "map-editor.control.end-change";

		public const string SelectionAssets = "map-editor.selection.assets";

		public const string SelectionAssetsRemove = "map-editor.selection.assets@remove";

		public const string SelectionAssetsAdd = "map-editor.selection.assets@add";

		public const string SelectionAssetsChange = "map-editor.selection.assets@change";

		public const string SelectionEntities = "map-editor.selection.entities";

		public const string SelectionEntitiesMouse = "map-editor.selection.entities.mouse";

		public const string SelectionEntitiesRemove = "map-editor.selection.entities@remove";

		public const string SelectionEntitiesAdd = "map-editor.selection.entities@add";

		public const string SelectionEntitiesChange = "map-editor.selection.entities@change";

		public const string Action = "map-editor.action";

		public const string ActionRecord = "map-editor.action.record";

		public const string ActionUndo = "map-editor.action.undo";

		public const string ActionRedo = "map-editor.action.redo";

		public const string ActionApply = "map-editor.action.apply";

		public const string ActionApplyReverse = "map-editor.action.apply-reverse";

		public const string HandleDown = "map-editor.handle@down";

		public const string HandleDragEnd = "map-editor.handle@drag-end";

		public const string HandleDragStart = "map-editor.handle@start";

		public const string HandleDragUpdate = "map-editor.handle@update";

		public const string SaveMapDataSchedule = "map-editor.save.map-data.schedule";

		public const string SaveMapDataStart = "map-editor.save.map-data@start";

		public const string SaveMapDataError = "map-editor.save.map-data@error";

		public const string SaveMapDataSuccess = "map-editor.save.map-data@success";

		public const string SaveMapDataBlocked = "map-editor.save.map-data@blocked";

		public const string SaveMapThumbStart = "map-editor.save.map-thumb@start";

		public const string SaveMapThumbError = "map-editor.save.map-thumb@error";

		public const string SaveMapThumbSuccess = "map-editor.save.map-thumb@success";

		public const string AssetFormEvent = "map-editor.asset.form.event";

		public const string AssetFormEventClick = "map-editor.asset.form.event@click";

		public const string AssetFormEventChange = "map-editor.asset.form.event@change";

		public const string AssetFormEventEndEdit = "map-editor.asset.form.event@end-edit";

		public const string SceneEntityCreate = "map-editor.scene.entity.create";

		public const string SceneEntityClone = "map-editor.scene.entity.clone";

		public const string EntityCreate = "map-editor.entity.create";

		public const string EntityClone = "map-editor.entity.clone";

		public const string EntityDelete = "map-editor.entity.delete";

		public const string AssetItem = "map-editor.asset.item";

		public const string AssetItemClick = "map-editor.asset.item@click";

		public const string AssetPage = "map-editor.asset.page";

		public const string AssetPageClick = "map-editor.asset.page@click";

		public const string GraphGateEvent = "map-editor.graph.gate.event";

		public const string GraphGateChange = "map-editor.graph.gate.event@change";

		public const string GraphGateEndEdit = "map-editor.graph.gate.event@end-edit";

		public const string GateOrderChange = "map-editor.gate.order@change";

		public const string GraphPodiumEvent = "map-editor.graph.podium.event";

		public const string GraphPodiumChange = "map-editor.graph.podium.event@change";

		public const string GraphPodiumEndEdit = "map-editor.graph.podium.event@end-edit";

		public const string PodiumOrderChange = "map-editor.podium.order@change";

		public const string GraphRulersEvent = "map-editor.graph.rulers.event";

		public const string GraphRulersWidgetEvent = "map-editor.graph.rulers.widget.event";

		public const string GraphRulersWidgetClick = "map-editor.graph.rulers.widget.event@click";

		public const string ControlsLayoutStateChange = "map-editor.controls.layout.state.change";

		public const string GraphLayoutEvent = "map-editor.graph.layout.event";

		public const string GraphLayoutWidgetEvent = "map-editor.graph.layout.widget.event";

		public const string GraphLayoutWidgetClick = "map-editor.graph.layout.widget.event@click";

		public const string GraphLayoutWidgetChange = "map-editor.graph.layout.widget.event@change";

		public const string GraphLayoutWidgetEndEdit = "map-editor.graph.layout.widget.event@end-edit";

		public const string InspectorDirty = "map-editor.inspector.dirty";

		public const string InspectorBeginChange = "map-editor.inspector.begin-change";

		public const string InspectorEndChange = "map-editor.inspector.end-change";

		public const string InspectorFormEvent = "map-editor.inspector.form.event";

		public const string InspectorFormEventClick = "map-editor.inspector.form.event@click";

		public const string InspectorFormEventChange = "map-editor.inspector.form.event@change";

		public const string InspectorFormEventEndEdit = "map-editor.inspector.form.event@end-edit";

		public const string CameraSignalUpdate = "map-editor.camera.signal-update";

		public const string CameraSignalLost = "map-editor.camera.signal-lost";

		public const string CameraSignalDrop = "map-editor.camera.signal-drop";

		public const string CameraSignalRecover = "map-editor.camera.signal-recover";

		public const string CameraSignalFull = "map-editor.camera.signal-full";

		public const string TemplatesMapCardEvent = "map-editor.templates-card";

		public const string TemplatesMapCardClick = "map-editor.templates-card@click";

		public const string CommunityAddItemClick = "community-maps.item.add@click";

		public const string CommunityDeleteItemClick = "community-maps.item.delete@click";

		public const string CommunityEditItemClick = "community-maps.item.edit@click";

		public const string CommunityFlyItemClick = "community-maps.item.fly@click";

		public const string CommunityCloneItemClick = "community-maps.item.clone@click";

		public const string CommunityNewMapClick = "community-maps.new-map@click";

		public const string CommunityNewMapRaceClick = "community-maps.new-map-race@click";

		public const string CommunityNewMapCollectableClick = "community-maps.new-map-collectable@click";

		public const string CommunityFormEvent = "community-maps.form.event";

		public const string CommunityFormClick = "community-maps.form.event@click";

		public const string CommunityFormChange = "community-maps.form.event@change";

		public const string CommunityFormEndEdit = "community-maps.form.event@end-edit";

		public const string CommunityFormSubmit = "community-maps.form.event@submit";

		public const string CommunityPageSelect = "community-maps.page@select";

		public const string CommunityPageNext = "community-maps.page-next@click";

		public const string CommunityPagePrevious = "community-maps.page-previous@click";

		public const string CommunityExit = "community-maps.exit@click";

		public const string CommunityDataFocusOn = "community-maps.data@focus";

		public const string CommunityDataFocusOff = "community-maps.data@unfocus";

		public const string CommunityDataClick = "community-maps.data@click";
	}

	public class Maps
	{
		public const string MapSelectionComplete = "maps.selection-complete";

		public const string MapSelectionCanceled = "maps.selection-canceled";

		public const string TrackSelectionComplete = "maps.track-selection-complete";

		public const string CommunityMapSelectionComplete = "maps.community-map-selection-complete";

		public const string MapsSelectionFavoriteClick = "maps.track-selection.favorite@click";

		public const string MapsSelectionFavoriteChange = "maps.track-selection.favorite@change";
	}

	public class USAF
	{
		public const string USAFDay = "usaf.day@click";

		public const string USAFNight = "usaf.night@click";
	}

	public class Analytics
	{
		public class UI
		{
			public const string MenuOpened = "analytics.ui.menu.opened";
		}

		public class Gameplay
		{
			public const string LoadGame = "analytics.gameplay.loadgame";
		}

		public class Controller
		{
			public const string ControllerConnected = "analytics.controller.connected";
		}

		public class Tryouts
		{
			public const string Registered = "analytics.tryouts.registered";

			public const string CompletedStep = "analytics.tryouts.completed-step";
		}
	}

	public class Input
	{
		public const string AutoCalibrationClick = "input.auto-calibration@click";

		public const string ManualCalibrationClick = "input.manual-calibration@click";

		public const string TransmitterSettingsToggle = "input.transmitter-settings@change";

		public const string ChannelSelectionClick = "input.channel-selection@click";

		public const string FineTuneControllerClick = "input.fine-tune@click";

		public const string SensitivityButtonClick = "input.sensitivity@click";

		public const string HelpClick = "input.help@click";

		public const string HelpNoHardwareClick = "input.help.no-hardware@click";

		public const string CalibrationStepStartClick = "calibration.step.start@click";

		public const string CalibrationStepComplete = "calibration.step.complete@timer.complete";

		public const string CalibrationNextStepClick = "calibration.step.next@click";

		public const string CalibrationBackStepClick = "calibration.step.back@click";

		public const string CalibrationPanelOpenClick = "input.calibration-menu-panel.open@click";

		public const string AutoCalibrationSaveClick = "input.auto-calibration.save@click";

		public const string ManualCalibrationSaveClick = "input.manual-calibration.save@click";

		public const string ManualCalibrationPanelOpen = "input.manual-calibration-panel.open";

		public const string CalibrationUpdateChannelData = "calibration.axis.invert";

		public const string CalibrationResetChannelData = "calibration.invert.reset";

		public const string ChannelSelectionDropdownChange = "calibration.channel-selection.dropdown@change";

		public const string ChannelSelectionInvertChange = "calibration.channel-selection.invert@change";

		public const string ChannelSelectionMidstickChange = "calibration.channel-selection.midstick@change";

		public const string ControllerSelectionChange = "calibration.controller.dropdown@change";

		public const string ChannelSelectionNextClick = "calibration.channel-selection.next@click";

		public const string ChannelSelectionComplete = "calibration.channel-selection.complete";

		public const string CalibrationSaveComplete = "calibration.save.complete";

		public const string ActiveControllerChanged = "input.active-controller.changed";

		public const string CalibrationAxisNotDetected = "calibration.axis.undetected";

		public const string OpenChannelSelection = "calibration.channel-selection.open";

		public const string ChannelSelectionSave = "calibration.channel-selection.save@click";

		public const string MouseCursorHide = "input.mouse-cursor.hide";

		public const string MouseCursorShow = "input.mouse-cursor.show";
	}
}
