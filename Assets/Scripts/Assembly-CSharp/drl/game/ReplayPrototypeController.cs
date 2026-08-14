using UnityEngine;
using drl.sim;
using drl.sim.rci;
using thelab.mvc;

namespace drl.game
{
	public class ReplayPrototypeController : Controller<DRLApp>
	{
		public ReplayFile replay;

		public int fps = 50;

		public float recordElapsed;

		public bool recordEnabled;

		public RaceController race;

		protected override void Start()
		{
			if (base.enabled)
			{
				base.Start();
				replay = new ReplayFile();
				replay.AddSimulatorChannels();
				replay.header.Initialize(ReplayStream.GetReplayTempFilePath("", "header_"));
				race = GetComponent<RaceController>();
			}
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!base.enabled)
			{
				return;
			}
			switch (p_event)
			{
			case "game.race.request-restart":
				replay.ClearChannels();
				recordElapsed = 0f;
				recordEnabled = true;
				break;
			case "game.pause":
				recordEnabled = false;
				break;
			case "game.unpause":
				recordEnabled = true;
				break;
			case "game.count@complete":
				replay.ClearChannels();
				recordElapsed = 0f;
				recordEnabled = true;
				Debug.Log("ReplayPrototypeController> Replay Record Enabled!");
				break;
			case "game.race.slowmo@start":
				RunOnce(delegate
				{
					if (!(this == null) && base.validContext)
					{
						Debug.Log("ReplayPrototypeController> RaceEndSlowmotionEffectStart / Stop");
						recordEnabled = false;
						string hash = base.app.hash;
						replay.Initialize(DRLPaths.Storage.replaysRoot + hash + ".rpl2.bytes");
						Debug.Log("ReplayPrototypeController> RaceEndSlowmotionEffectStart / Header Write");
						DRLMap map = base.app.scene.map;
						DRLMapTrack track = base.app.scene.track;
						bool flag = map.data != null;
						GameModel model = race.game.model;
						Drone playerDrone = model.playerDrone;
						PlayerStateModel player = base.app.model.storage.state.player;
						GamePlayerData playerData = race.game.model.playerData;
						FCProfileData active = player.settings.tuning.GetActive();
						int num = playerData?.order ?? 0;
						ReplayHeader header = replay.header;
						header.Clear();
						header.title = "ClipPrototype";
						header.isMultiplayer = false;
						header.isPlayer = true;
						header.mapGUID = map.guid;
						header.trackGUID = track.guid;
						header.isCustomMap = flag;
						header.customMapGUID = (flag ? map.data.guid : "");
						header.platformId = player.profile.platformId;
						header.playerId = player.profile.playerId;
						header.profileName = player.profile.username;
						header.profileColorHex = player.profile.colorHex;
						header.profilePhoto = player.profile.photoURL;
						header.raceTime = race.model.time;
						header.gameTypeFlag = model.type;
						header.controllerTypeFlag = RCI.GetControllerStateType(ControllerStateType.Taranis);
						header.cameraTilt = playerDrone.body.frame.camera.tilt;
						header.cameraFOV = playerDrone.body.frame.camera.fov;
						header.podiumGUID = player.podiumId;
						header.SetDroneRig(playerData.rig);
						header.SetFCProfile(active);
						header.SetPhysicsTune(playerDrone.physics);
						header.order = ((num >= 0) ? num : 0);
						Debug.Log("ReplayPrototypeController> RaceEndSlowmotionEffectStart / Serialize");
						replay.Serialize();
					}
				}, 2.8f, unscaledTime: true);
				break;
			}
		}

		protected void Update()
		{
			if (base.enabled && recordEnabled)
			{
				float num = 1f / (float)fps;
				recordElapsed += Time.deltaTime;
				if (recordElapsed >= num)
				{
					recordElapsed -= num;
					float time = race.model.time;
					Drone playerDrone = race.model.playerDrone;
					replay.Write(time, playerDrone);
				}
			}
		}
	}
}
