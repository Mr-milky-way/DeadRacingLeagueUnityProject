using System;
using UnityEngine;
using drl.backend;
using drl.network;
using drl.sim;
using thelab.core;

namespace drl.game
{
	[Serializable]
	public class GamePlayerData
	{
		public static DRLService service;

		public string name;

		public string upperName;

		public GamePlayerType type;

		public int order;

		public int id = -1;

		public string playerId;

		public string platformId;

		public Texture2D photo;

		public Color color;

		private Color m_color2 = Color.magenta;

		[NonSerialized]
		public DroneRigData rig;

		public TextAsset rigData;

		public string droneRigData;

		public string podiumId;

		[NonSerialized]
		public BlackboxData replay;

		[NonSerialized]
		public ReplayFile replayV2;

		public int replayClip;

		public TextAsset recordData;

		public Drone drone;

		public float raceTime;

		public float cameraTilt = 45f;

		public float cameraFOV = 90f;

		public RaceStatusType raceStatus;

		public bool isRacer
		{
			get
			{
				if (type == GamePlayerType.Ghost)
				{
					return true;
				}
				if (type == GamePlayerType.Human)
				{
					return true;
				}
				if (type == GamePlayerType.Network)
				{
					return true;
				}
				return false;
			}
		}

		public Color color2
		{
			get
			{
				return m_color2;
			}
			set
			{
				m_color2 = value;
			}
		}

		public GamePlayerData()
		{
		}

		public GamePlayerData(GamePlayerData p_data)
		{
			if (p_data != null)
			{
				name = p_data.name;
				upperName = name.ToUpper();
				type = p_data.type;
				order = p_data.order;
				id = p_data.id;
				playerId = p_data.playerId;
				photo = p_data.photo;
				color = p_data.color;
				color2 = p_data.color2;
				rig = p_data.rig;
				rigData = p_data.rigData;
				replay = p_data.replay;
				replayV2 = p_data.replayV2;
				replayClip = p_data.replayClip;
				recordData = p_data.recordData;
				drone = p_data.drone;
			}
		}

		public void Initialize()
		{
			if (replay == null && (bool)recordData)
			{
				SetReplay(recordData, replayClip);
			}
			if (rig == null && (bool)rigData)
			{
				rig = ScriptableObject.CreateInstance<DroneRigData>();
				rig.Set(rigData.bytes);
			}
			if (ReplayFile.EnableVersion2)
			{
				ReplayHeader replayHeader = ((replayV2 == null) ? null : replayV2.header);
				if (rig != null)
				{
					replayHeader = null;
				}
				if (replayHeader != null)
				{
					rig = replayHeader.GetDroneRig();
				}
				return;
			}
			SerializedData serializedData = ((replay == null) ? null : replay.header);
			if (rig != null)
			{
				serializedData = null;
			}
			if (serializedData != null)
			{
				string text = serializedData.Get("drone-rig", "");
				if (!string.IsNullOrEmpty(text))
				{
					rig = DroneRigData.FromJson(text);
				}
			}
		}

		public void SetReplay(BlackboxRecord p_record, int p_clip)
		{
			replayClip = p_clip;
			if (p_record == null)
			{
				Debug.LogWarning("GamePlayerData> Replay record is null!");
			}
			else if (p_record.clips.Count <= 0)
			{
				Debug.LogWarning("GamePlayerData> Replay record is empty!");
			}
			else if (replayClip < 0)
			{
				Debug.LogWarning("GamePlayerData> Replay Clip Id out of bounds");
			}
			else if (replayClip >= p_record.clips.Count)
			{
				Debug.LogWarning("GamePlayerData> Replay Clip Id out of bounds");
			}
			else
			{
				replay = p_record.clips[replayClip];
			}
		}

		public void SetReplay(ReplayFile p_replay)
		{
			replayClip = 0;
			if (p_replay == null)
			{
				Debug.LogWarning("GamePlayerData> Replay file is null!");
			}
			else
			{
				replayV2 = p_replay;
			}
		}

		public void SetReplay(TextAsset p_record, int p_clip)
		{
			if (!p_record)
			{
				Debug.LogWarning("GamePlayerData> Record data is null!");
				return;
			}
			BlackboxRecord p_record2 = Serialize.FromBytes<BlackboxRecord>(recordData.bytes);
			SetReplay(p_record2, p_clip);
		}

		public GamePlayerData SetPlayer(GamePlayerType p_type, string p_player_id, string p_platform_id, string p_name, Color[] p_colors)
		{
			type = p_type;
			playerId = p_player_id;
			platformId = p_platform_id;
			name = p_name + ((p_type == GamePlayerType.Ghost && !p_name.Contains("NPC")) ? " (BOT)" : "");
			upperName = name.ToUpper();
			this.color = p_colors[0];
			color2 = p_colors[1];
			color2 = p_colors[1];
			string[] obj = new string[6] { "Setting GamePlayerData for: ", name, " Color: ", null, null, null };
			Color color = this.color;
			obj[3] = color.ToString();
			obj[4] = " ";
			obj[5] = color2.ToString();
			Debug.Log(string.Concat(obj));
			return this;
		}

		public GamePlayerData RefreshPlayerPhoto(Action p_callback = null)
		{
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false, QualitySettings.activeColorSpace == ColorSpace.Linear);
			texture2D.SetPixel(0, 0, Colorf.transparent);
			texture2D.Apply();
			photo = texture2D;
			if ((bool)service)
			{
				service.GetPlayerAvatar(playerId, delegate(Texture2D p_result)
				{
					if ((bool)p_result)
					{
						Texture2D texture2D2 = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false, QualitySettings.activeColorSpace == ColorSpace.Linear);
						texture2D2.SetPixel(0, 0, Colorf.transparent);
						texture2D2.Apply();
						photo = texture2D2;
						photo = p_result;
						if ((bool)p_result)
						{
							photo = p_result;
						}
						else
						{
							photo = texture2D2;
						}
						p_callback?.Invoke();
					}
				});
			}
			return this;
		}

		public GamePlayerData RefreshPlayerPhotoByURL()
		{
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false);
			texture2D.SetPixel(0, 0, Colorf.transparent);
			texture2D.Apply();
			photo = texture2D;
			string p_id = DRLService.baseUri + "/images/avatar/drl-avatar.png";
			if ((bool)service)
			{
				service.GetPlayerAvatarOnboarding(p_id, delegate(Texture2D p_result)
				{
					if ((bool)p_result)
					{
						photo.name = p_result.name;
						DuplicateTexture(p_result);
					}
				});
			}
			return this;
		}

		private Texture2D DuplicateTexture(Texture2D source)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
			Graphics.Blit(source, temporary);
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = temporary;
			Texture2D texture2D = new Texture2D(source.width, source.height);
			texture2D.ReadPixels(new Rect(0f, 0f, temporary.width, temporary.height), 0, 0);
			texture2D.Apply();
			RenderTexture.active = active;
			RenderTexture.ReleaseTemporary(temporary);
			return texture2D;
		}

		public GamePlayerData SetNetwork(NetworkActor actor)
		{
			GamePlayerType p_type = (actor.IsSpectator ? GamePlayerType.Spectator : ((!actor.IsLocal) ? GamePlayerType.Network : GamePlayerType.Human));
			bool flag = playerId != actor.PlayerId;
			bool flag2 = droneRigData != actor.DroneRigData;
			Color[] p_colors = new Color[2] { actor.MainColor, actor.SecondaryColor };
			GamePlayerData gamePlayerData = SetPlayer(p_type, actor.PlayerId, actor.PlatformId, actor.ProfileName, p_colors);
			gamePlayerData.id = actor.ID;
			gamePlayerData.order = actor.Order;
			gamePlayerData.cameraTilt = actor.CameraTilt;
			gamePlayerData.cameraFOV = actor.CameraFOV;
			if (flag)
			{
				gamePlayerData = RefreshPlayerPhoto();
			}
			if (flag2)
			{
				gamePlayerData.droneRigData = actor.DroneRigData;
				gamePlayerData.rig = DroneRigData.FromJson(actor.DroneRigData);
			}
			if (gamePlayerData.drone != null && gamePlayerData.drone.body != null && gamePlayerData.drone.body.frame != null && gamePlayerData.drone.body.frame.camera != null && !gamePlayerData.drone.isBroken)
			{
				gamePlayerData.drone.body.frame.camera.tilt = actor.CameraTilt;
				gamePlayerData.drone.body.frame.camera.fov = actor.CameraFOV;
			}
			return gamePlayerData;
		}
	}
}
