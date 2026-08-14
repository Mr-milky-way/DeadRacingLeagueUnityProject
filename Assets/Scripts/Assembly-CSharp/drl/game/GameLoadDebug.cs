using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class GameLoadDebug : Controller<DRLApp>
	{
		public GameFlag type;

		public GameFlag mode;

		public DRLMap map;

		public string customMapGUID;

		public DRLMapTrack track;

		public DRLMission mission;

		public DRLQuest quest;

		public List<GamePlayerData> players;

		public TextAsset[] records;

		public TextAsset rig;

		public bool unload;

		public bool runOnAwake = true;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event != null && p_event == "boot@complete" && runOnAwake)
			{
				if (unload)
				{
					StartCoroutine(Unload());
				}
				else
				{
					ApplyLoad();
				}
			}
		}

		private IEnumerator Unload()
		{
			if (mission != null)
			{
				for (int i = 0; i < SceneManager.sceneCount; i++)
				{
					if (SceneManager.GetSceneAt(i).name == mission.map.scene)
					{
						AsyncOperation asyncUnloading = SceneManager.UnloadSceneAsync(mission.map.scene);
						while (!asyncUnloading.isDone)
						{
							yield return null;
						}
					}
				}
			}
			yield return new WaitForSeconds(0.9f);
			ApplyLoad();
		}

		public void ApplyLoad()
		{
			base.app.arguments.game.type = type;
			base.app.arguments.game.mode = mode;
			base.app.arguments.game.map = (mission ? mission.map : (track ? track.map : map));
			base.app.arguments.game.track = (track ? track : (mission ? mission.track : null));
			base.app.arguments.game.mission = mission;
			base.app.arguments.game.quest = quest;
			if (records == null)
			{
				Debug.LogWarning("GameLoadDebug> No replay data provided.");
			}
			List<BlackboxRecord> list = new List<BlackboxRecord>();
			if (records != null)
			{
				for (int i = 0; i < records.Length; i++)
				{
					list.Add(Serialize.FromBytes<BlackboxRecord>(records[i].bytes));
				}
			}
			BlackboxRecord blackboxRecord = BlackboxRecord.Merge(list.ToArray());
			_ = base.app.model.service;
			switch (type)
			{
			case GameFlag.Replay:
				if (records.Length == 0)
				{
					Debug.LogWarning("GameLoadDebug> No replay data provided.");
					break;
				}
				if (blackboxRecord.clips.Count <= 0)
				{
					Debug.LogWarning("GameLoadDebug> No replay data provided.");
					break;
				}
				Debug.LogWarning("GameLoadDebug> Replay - clips[" + blackboxRecord.clips.Count + "]");
				base.app.scene.Load(blackboxRecord);
				break;
			case GameFlag.Mission:
				base.app.arguments.game.players.Clear();
				base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
				base.app.scene.Load(base.app.arguments);
				break;
			case GameFlag.MapEditor:
			{
				string text = customMapGUID;
				if (!string.IsNullOrEmpty(text))
				{
					base.app.scene.LoadCommunityMap(text);
					break;
				}
				DRLMap dRLMap2 = base.app.arguments.game.map;
				if (!dRLMap2)
				{
					Debug.LogWarning("GameLoadDebug> Failed to Find Map!");
					break;
				}
				DRLMapTrack dRLMapTrack2 = base.app.model.storage.GetMapTracks(dRLMap2, GameFlag.Freestyle)[0];
				MapData mapData = new MapData();
				mapData.playerId = base.app.model.service.backend.playerId;
				mapData.mapTitle = "new-map";
				mapData.mapId = dRLMap2.guid;
				mapData.root.name = mapData.mapTitle;
				dRLMap2.data = mapData;
				Debug.Log("GameLoadDebug> ApplyLoad / scene[" + dRLMap2.scene + "] data[" + mapData.guid + "]");
				base.app.arguments.game.map = dRLMap2;
				base.app.arguments.game.track = dRLMapTrack2;
				base.app.scene.Load(base.app.arguments);
				break;
			}
			case GameFlag.Freestyle:
			case GameFlag.Race:
			case GameFlag.Campaign:
			case GameFlag.Sandbox:
			{
				GamePlayerData playerData = base.app.model.storage.state.player.playerData;
				playerData.rigData = rig;
				base.app.arguments.game.players = new List<GamePlayerData>(players);
				base.app.arguments.game.AddPlayer(playerData);
				players.Add(playerData);
				base.app.arguments.game.AddReplay(blackboxRecord);
				DRLMap dRLMap = base.app.arguments.game.map;
				DRLMapTrack dRLMapTrack = base.app.arguments.game.track;
				string mapGUID = blackboxRecord.GetMapGUID();
				string trackGUID = blackboxRecord.GetTrackGUID();
				if (!dRLMap)
				{
					dRLMap = base.app.model.storage.library.FindByGUID<DRLMap>(mapGUID);
				}
				if (!dRLMapTrack)
				{
					dRLMapTrack = base.app.model.storage.library.FindByGUID<DRLMapTrack>(trackGUID);
				}
				base.app.arguments.game.map = dRLMap;
				base.app.arguments.game.track = dRLMapTrack;
				base.app.scene.Load(base.app.arguments);
				break;
			}
			case GameFlag.FreeCamera:
				break;
			}
		}
	}
}
