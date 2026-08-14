using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class AudioView : View<DRLApp>
	{
		[SerializeField]
		private AudioSource musicAudioSource;

		[SerializeField]
		private AudioSource sfxAudioSource;

		private float m_volume;

		private float m_volumeMusic;

		private float m_volumeSFX;

		private float m_volumeDrones;

		private float rotational_speed_prev;

		private float average_rpm_prev;

		private DRLAudioComponent m_manager;

		private WWISEManager m_wwise;

		private List<AkBank> m_banks;

		protected Dictionary<string, bool> m_is_playing;

		private Dictionary<GameObject, string> m_motor_id_lut;

		private Activity m_music_main_poll;

		public float volumeMax => 100f;

		public float volume
		{
			get
			{
				return m_volume;
			}
			set
			{
				m_volume = value;
				RefreshVolume();
			}
		}

		public float volumeMusic
		{
			get
			{
				return m_volumeMusic;
			}
			set
			{
				m_volumeMusic = value;
				RefreshVolume();
			}
		}

		public float volumeSFX
		{
			get
			{
				return m_volumeSFX;
			}
			set
			{
				m_volumeSFX = value;
				RefreshVolume();
			}
		}

		public float volumeDrones
		{
			get
			{
				return m_volumeDrones;
			}
			set
			{
				m_volumeDrones = value;
				RefreshVolume();
			}
		}

		public DRLAudioComponent manager
		{
			get
			{
				if (!m_manager)
				{
					return m_manager = AssertLocal<DRLAudioComponent>("manager");
				}
				return m_manager;
			}
		}

		public WWISEManager wwise
		{
			get
			{
				if (!m_wwise)
				{
					return m_wwise = AssertLocal<WWISEManager>("wwise");
				}
				return m_wwise;
			}
		}

		public List<AkBank> banks
		{
			get
			{
				if (m_banks == null)
				{
					m_banks = new List<AkBank>(GetComponentsInChildren<AkBank>());
				}
				return m_banks;
			}
		}

		protected void Awake()
		{
			m_is_playing = new Dictionary<string, bool>();
			m_volume = 0f;
			m_volumeMusic = 0f;
			m_volumeSFX = 0f;
			m_volumeDrones = 0f;
			rotational_speed_prev = 0f;
			average_rpm_prev = 0f;
			SetSplashSceneAudio();
			RefreshVolume();
		}

		public AkBank GetBank(string p_name)
		{
			return banks.Find((AkBank it) => (bool)it && it.name == p_name);
		}

		public void SetSplashSceneAudio()
		{
			m_volume = 0.5f;
			m_volumeMusic = 0.5f;
			m_volumeSFX = 0.5f;
			m_volumeDrones = 0.5f;
		}

		public void Fade(float p_volume, float p_duration = 0.8f, float p_delay = 0f)
		{
			Tween.Add(this, "volume", Mathf.Clamp01(p_volume), p_duration, p_delay, null);
		}

		public void FadeIn(float p_duration = 0.8f, float p_delay = 0f)
		{
			Fade(1f, p_duration, p_delay);
		}

		public void FadeOut(float p_duration = 0.8f, float p_delay = 0f)
		{
			Fade(0f, p_duration, p_delay);
		}

		public void MuteFadeIn(float p_duration = 0.8f, float p_delay = 0f)
		{
			float p_volume = volume;
			volume = 0f;
			Fade(p_volume, p_duration, p_delay);
		}

		public bool IsPlaying(string p_id)
		{
			if (m_is_playing == null)
			{
				m_is_playing = new Dictionary<string, bool>();
			}
			if (m_is_playing.ContainsKey(p_id))
			{
				return m_is_playing[p_id];
			}
			return false;
		}

		public bool IsPlaying(string p_id, bool p_flag)
		{
			if (m_is_playing == null)
			{
				m_is_playing = new Dictionary<string, bool>();
			}
			m_is_playing[p_id] = p_flag;
			return p_flag;
		}

		public void StopAllGameAudio()
		{
			wwise.PostEvent("stop_game_sounds_all");
			wwise.SetRTPC("radio_signal", 1f);
			StopEnvWind();
		}

		public void PauseAllGameAudio()
		{
			wwise.PostEvent("pause_game_sounds_all");
		}

		public void ResumeAllGameAudio()
		{
			wwise.PostEvent("resume_game_sounds_all");
		}

		public void PauseGameMusic()
		{
			wwise.PostEvent("pause_game_music");
		}

		public void ResumeGameMusic()
		{
			wwise.PostEvent("resume_game_music");
		}

		public void PlayEnvWind()
		{
			wwise.PostEvent("play_sx_env_wind");
		}

		public void StopEnvWind()
		{
			wwise.PostEvent("stop_sx_env_wind");
		}

		public void PlayDroneCrash(GameObject p_target)
		{
			PlayDronePartHit(p_target);
		}

		public void PlayDronePropHit(GameObject p_target)
		{
			wwise.PostEvent("play_drone_prop_hit", p_target);
		}

		public void PlayDronePartHit(GameObject p_target, float p_intensity = 1f)
		{
			wwise.SetRTPC("drone_impact_soft", p_intensity * 260f);
			wwise.SetRTPC("drone_impact_hard", p_intensity * 260f);
			wwise.PostEvent("play_drone_impact", p_target);
			D.Log("AudioView> PlayDronePartHit: " + p_target.name + " audio intensity: " + p_intensity);
		}

		public void PlayDroneSpawn(GameObject p_target)
		{
			wwise.PostEvent("play_drone_spawn", p_target);
		}

		public void PlayDroneRespawn(GameObject p_target)
		{
			wwise.PostEvent("play_drone_respawn", p_target);
		}

		public void PlayDroneFlipped(GameObject p_target)
		{
			wwise.PostEvent("play_drone_flipped", p_target);
		}

		public void PlayDroneMotor(GameObject p_target)
		{
			string p_id = "play_drone_motors_" + p_target.GetInstanceID().ToString("X6");
			if (!IsPlaying(p_id))
			{
				IsPlaying(p_id, p_flag: true);
				wwise.PostEvent("play_drone_motors", p_target);
				wwise.SetRTPC("drone_extra_sfx_volume", 0f, p_target);
			}
		}

		public void SetDroneMotorPropState(GameObject p_target, string p_type, int p_blades, float p_size, float p_pitch)
		{
			string text = p_blades + "B";
			string text2 = p_type.ToUpper();
			string text3 = Mathf.FloorToInt(p_size * 10f).ToString();
			string text4 = Mathf.FloorToInt(p_pitch * 10f).ToString();
			_ = text2 + text + text3 + text4;
			wwise.SetSwitch("PropType_Blades", "blade_" + text, p_target);
			wwise.SetSwitch("PropType_Type", "type_" + text2, p_target);
			wwise.SetSwitch("PropType_Size", "size_" + text3, p_target);
		}

		public void PlayDroneMotorIdle(GameObject p_target)
		{
			string p_id = "garage_idle";
			if (!IsPlaying(p_id))
			{
				IsPlaying(p_id, p_flag: true);
				wwise.PostEvent("play_drone_motors_idle", p_target);
			}
		}

		public void UpdateDroneMotorIdle(GameObject p_target)
		{
			string p_id = "garage_idle";
			if (IsPlaying(p_id))
			{
				StopAllDroneSounds();
				PlayDroneMotorIdle(p_target);
			}
		}

		public void PlayDroneMotorStart(GameObject p_target)
		{
			StopDroneMotor(p_target);
			wwise.PostEvent("play_drone_motors_start", p_target);
		}

		public void StopDroneMotor(GameObject p_target)
		{
			if (m_motor_id_lut == null)
			{
				m_motor_id_lut = new Dictionary<GameObject, string>();
			}
			string text = "";
			if (m_motor_id_lut.ContainsKey(p_target))
			{
				text = m_motor_id_lut[p_target];
			}
			else
			{
				text = "play_drone_motors_" + p_target.GetInstanceID().ToString("X6");
				m_motor_id_lut[p_target] = text;
			}
			IsPlaying(text, p_flag: false);
			IsPlaying("garage_idle", p_flag: false);
			wwise.PostEvent("stop_drone_motors", p_target);
			rotational_speed_prev = 0f;
			average_rpm_prev = 0f;
		}

		public void PlayDroneDamage(GameObject p_target, float p_intensity)
		{
			if (!(p_target == null))
			{
				D.Log("AudioView> PlayDroneDamage: " + p_target.name + " intensity: " + p_intensity);
			}
		}

		public void UpdateDroneMotor(GameObject p_target, float p_rpm, float p_arpm)
		{
			float num = p_arpm - average_rpm_prev;
			wwise.SetRTPC("motors_max_rpm", p_rpm, p_target);
			wwise.SetRTPC("motors_average_rpm", p_arpm, p_target);
			wwise.SetRTPC("motors_average_rpm_delta", num * 0.5f, p_target);
			average_rpm_prev = p_arpm;
		}

		public void UpdateDroneMotor(GameObject p_target, float p_rpm, float p_arpm, float p_arpmDelta)
		{
			wwise.SetRTPC("motors_max_rpm", p_rpm, p_target);
			wwise.SetRTPC("motors_average_rpm", p_arpm, p_target);
			wwise.SetRTPC("motors_average_rpm_delta", p_arpmDelta, p_target);
			average_rpm_prev = p_arpm;
		}

		public void StopAllDroneSounds()
		{
			IsPlaying("garage_idle", p_flag: false);
			wwise.PostEvent("stop_drone_sounds_all");
		}

		public void UpdateDroneSpeed(float speed_kph, float rotation_speed_dps_m, float rotation_speed_dps_z, GameObject p_target)
		{
			float p_value = rotation_speed_dps_m - rotational_speed_prev;
			wwise.SetRTPC("drone_speed", speed_kph, p_target);
			wwise.SetRTPC("drone_rotspeed", rotation_speed_dps_m, p_target);
			wwise.SetRTPC("drone_rotspeed_dir", rotation_speed_dps_z, p_target);
			wwise.SetRTPC("drone_rotspeed_delta", p_value, p_target);
			wwise.SetRTPC("drone_extra_sfx_volume", 100f, p_target);
			rotational_speed_prev = Mathf.Abs(rotation_speed_dps_m);
		}

		public void StopMusicAll()
		{
			if ((bool)wwise)
			{
				wwise.PostEvent("stop_mx_all");
			}
		}

		public void PlayMusicMain()
		{
			if (m_music_main_poll != null)
			{
				m_music_main_poll.Stop();
			}
			if (!base.validContext || !wwise)
			{
				return;
			}
			float t = 1f;
			int r = 0;
			m_music_main_poll = Activity.Run((Func<bool>)delegate
			{
				t += Time.unscaledDeltaTime;
				if (t < 1f)
				{
					return true;
				}
				t = 0f;
				if (r > 10)
				{
					return false;
				}
				r++;
				UpdateGameStatus("playing");
				return !wwise.PostEvent("play_mx_mainmenu");
			}, 0f, false);
		}

		public void StopMusicMain()
		{
			wwise.PostEvent("stop_mx_mainmenu");
		}

		public void PlayMusicGame()
		{
			if (!base.validContext)
			{
				return;
			}
			GameModel game = base.app.model.game;
			DRLMap map = base.app.scene.map;
			string text = (map ? map.scene : "gates-of-hell").Replace("-", "_");
			switch (game.type)
			{
			case GameFlag.Mission:
			{
				DRLMission mission2 = base.app.scene.mission;
				string s_state2 = "mission_los";
				if ((bool)mission2)
				{
					GameFlagTag component2 = mission2.GetComponent<GameFlagTag>();
					if ((bool)component2 && !component2.Contains(GameFlag.Training))
					{
						s_state2 = "mission_generic";
					}
				}
				wwise.SetState("GameType", s_state2);
				wwise.PostEvent("play_mx_ingame");
				break;
			}
			case GameFlag.Collectable:
			{
				DRLMission mission = base.app.scene.mission;
				string s_state = "mission_los";
				if ((bool)mission)
				{
					GameFlagTag component = mission.GetComponent<GameFlagTag>();
					if ((bool)component && !component.Contains(GameFlag.Training))
					{
						s_state = "mission_generic";
					}
				}
				wwise.SetState("GameType", s_state);
				wwise.PostEvent("play_mx_ingame");
				break;
			}
			case GameFlag.Race:
			case GameFlag.Campaign:
				wwise.SetState("GameType", "race");
				wwise.SetState("RaceLevel", text);
				wwise.SetRTPC("gates_percentage", -1f);
				wwise.PostEvent("play_mx_race");
				break;
			case GameFlag.Replay:
				wwise.SetState("GameType", "freestyle");
				wwise.SetState("RaceLevel", text);
				wwise.PostEvent("play_mx_replay");
				break;
			case GameFlag.Freestyle:
			case GameFlag.FreeCamera:
			case GameFlag.Sandbox:
				if (text != "usaf")
				{
					wwise.SetState("GameType", "freestyle");
					wwise.PostEvent("play_mx_ingame");
					break;
				}
				wwise.SetState("GameType", "race");
				wwise.SetState("RaceLevel", text);
				wwise.SetRTPC("gates_percentage", 0f);
				wwise.PostEvent("play_mx_race");
				break;
			}
			UpdateGameStatus("playing");
		}

		public void StopMusicGame()
		{
			wwise.PostEvent("stop_mx_ingame");
		}

		public void PlayMusicPostGame(string race_result = "finished")
		{
			switch (base.app.model.game.type)
			{
			case GameFlag.Mission:
				wwise.SetState("GameType", "mission_generic");
				break;
			case GameFlag.Campaign:
				wwise.SetState("GameType", "campaign");
				wwise.SetState("RaceResult", race_result);
				break;
			case GameFlag.Race:
				wwise.SetState("GameType", "race");
				wwise.SetState("RaceResult", race_result);
				break;
			}
			UpdateGameStatus("playing");
			wwise.PostEvent("play_mx_postgame");
		}

		public void StopMusicPostGame()
		{
			wwise.PostEvent("stop_mx_postgame");
		}

		public void UpdateMusicPostGameResult(string race_result)
		{
			wwise.SetState("RaceResult", race_result);
		}

		public void PlayMusicGarage()
		{
			UpdateGameStatus("playing");
			wwise.PostEvent("play_mx_garage");
		}

		public void StopMusicGarage()
		{
			wwise.PostEvent("stop_mx_garage");
		}

		public void PlayMusicMapEditor()
		{
			UpdateGameStatus("playing");
			wwise.PostEvent("play_mx_mapeditor");
		}

		public void StopMusicMapEditor()
		{
			wwise.PostEvent("stop_mx_mapeditor");
		}

		public void UpdateGameStatus(string status)
		{
			wwise.SetState("GameStatus", status);
		}

		public void UpdateRaceGatesPercentage(float percentage)
		{
			wwise.SetRTPC("gates_percentage", percentage);
		}

		public void PlayMusicIntro()
		{
			musicAudioSource.Play();
		}

		public void StopMusicIntro()
		{
			musicAudioSource.Stop();
		}

		public void PlayUIClick()
		{
			wwise.PostEvent("play_sx_ui_click", base.gameObject);
		}

		public void PlayUIOver()
		{
			wwise.PostEvent("play_sx_ui_bigbutton_focus", base.gameObject);
		}

		public void PlayUIFocus()
		{
			wwise.PostEvent("play_sx_ui_bigbutton_focus", base.gameObject);
		}

		public void PlayUISmallClick()
		{
			wwise.PostEvent("play_sx_ui_smallbutton_click", base.gameObject);
		}

		public void PlayUISmallOver()
		{
			wwise.PostEvent("play_sx_ui_focus", base.gameObject);
		}

		public void PlayUISmallFocus()
		{
			wwise.PostEvent("play_sx_ui_focus", base.gameObject);
		}

		public void PlayUITextOver()
		{
			wwise.PostEvent("play_sx_ui_textfield_focus", base.gameObject);
		}

		public void PlayUIScreenForward()
		{
			wwise.PostEvent("play_sx_ui_screen_forward", base.gameObject);
		}

		public void PlayUIScreenBackward()
		{
			wwise.PostEvent("play_sx_ui_screen_backward", base.gameObject);
		}

		public void PlayUIInvalidInteraction()
		{
			wwise.PostEvent("play_sx_ui_invalid_interaction", base.gameObject);
		}

		public void PlayUIGenericError()
		{
			wwise.PostEvent("play_sx_ui_generic_error", base.gameObject);
		}

		public void PlayUIGenericSuccess()
		{
			wwise.PostEvent("play_sx_ui_generic_success", base.gameObject);
		}

		public void PlayUILoadingSuccess()
		{
			wwise.PostEvent("play_sx_ui_loading_success", base.gameObject);
		}

		public void PlayUILoadingError()
		{
			wwise.PostEvent("play_sx_ui_loading_error", base.gameObject);
		}

		public void PlayUIPause()
		{
			wwise.PostEvent("play_sx_ui_pause", base.gameObject);
		}

		public void PlayUIStartGame()
		{
			wwise.PostEvent("play_sx_ui_startgame", base.gameObject);
		}

		public void PlayUILevelRestart()
		{
			wwise.PostEvent("play_sx_ui_level_restart", base.gameObject);
		}

		public void PlayUIChange()
		{
			wwise.PostEvent("play_sx_ui_formtick", base.gameObject);
		}

		public void PlayUISnapshot()
		{
			wwise.PostEvent("play_sx_ui_snapshot", base.gameObject);
		}

		public void PlayUINewRecord()
		{
			wwise.PostEvent("play_sx_ui_new_record", base.gameObject);
		}

		public void PlayUINewResultLine()
		{
			wwise.PostEvent("play_sx_ui_new_result_line", base.gameObject);
		}

		public void PlayUILoadingLoop()
		{
			if (sfxAudioSource != null)
			{
				sfxAudioSource.Play();
			}
			else
			{
				wwise.PostEvent("play_sx_ui_loading_loop", base.gameObject);
			}
		}

		public void StopUILoadingLoop()
		{
			if (sfxAudioSource != null)
			{
				sfxAudioSource.Stop();
			}
			else
			{
				wwise.PostEvent("stop_sx_ui_loading_loop", base.gameObject);
			}
		}

		public void PlayUICounterLoop()
		{
			wwise.PostEvent("play_sx_ui_counter_loop", base.gameObject);
		}

		public void StopUICounterLoop()
		{
			wwise.PostEvent("stop_sx_ui_counter_loop", base.gameObject);
		}

		public void PlayUIStar()
		{
			wwise.PostEvent("play_sx_ui_star_single", base.gameObject);
		}

		public void PlayUIStarHalf()
		{
			wwise.PostEvent("play_sx_ui_star_half", base.gameObject);
		}

		public void PlayUIStarFull()
		{
			wwise.PostEvent("play_sx_ui_stars_full", base.gameObject);
		}

		public void PlayUIStarNone()
		{
			wwise.PostEvent("play_sx_ui_star_none", base.gameObject);
		}

		public void PlayUICameraFForward()
		{
			wwise.PostEvent("play_sx_ui_camera_forward", base.gameObject);
		}

		public void PlayUIMultiplayerRoomStart()
		{
			wwise.PostEvent("play_sx_ui_room_game_start", base.gameObject);
		}

		public void PlayUIMultiplayerRoomCount(int value = 0)
		{
			_ = volumeSFX;
			switch (value)
			{
			case 5:
				_ = volumeSFX;
				break;
			case 4:
				_ = volumeSFX;
				break;
			case 3:
				_ = volumeSFX;
				break;
			case 2:
				_ = volumeSFX;
				break;
			default:
				_ = volumeSFX;
				break;
			}
			wwise.PostEvent("play_sx_ui_room_game_countdown", base.gameObject);
		}

		public void PlayUIGarageSpray()
		{
			wwise.PostEvent("play_sx_ui_garage_spray", base.gameObject);
		}

		public void PlayUIGarageTrailChange()
		{
			wwise.PostEvent("play_sx_ui_garage_trailchange", base.gameObject);
		}

		public void PlayUIGaragPartChange()
		{
			wwise.PostEvent("play_sx_ui_garage_partchange", base.gameObject);
		}

		public void PlayUIMapEditorSelect()
		{
			wwise.PostEvent("play_sx_ui_mapeditor_select", base.gameObject);
		}

		public void PlayUIMapEditorDelete()
		{
			wwise.PostEvent("play_sx_ui_mapeditor_remove", base.gameObject);
		}

		public void PlayUIMapEditorPlace()
		{
			wwise.PostEvent("play_sx_ui_mapeditor_place", base.gameObject);
		}

		public void PlayGameGateValid()
		{
			wwise.PostEvent("play_sx_game_gate_clear", base.gameObject);
		}

		public void PlayGameGateFinalValid()
		{
			wwise.PostEvent("play_sx_game_gate_clear_final", base.gameObject);
		}

		public void PlayGameCountdownTick()
		{
			wwise.PostEvent("play_sx_game_countdown_tick", base.gameObject);
		}

		public void PlayGameCountdownFinish()
		{
			wwise.PostEvent("play_sx_game_countdown_go", base.gameObject);
		}

		public void PlayGameRaceLap()
		{
			wwise.PostEvent("play_sx_game_lap_complete", base.gameObject);
		}

		public void PlayGameRaceFailure()
		{
			wwise.PostEvent("play_sx_game_failure", base.gameObject);
		}

		public void PlayGlassBreak(GameObject p_target)
		{
			wwise.PostEvent("play_glass_breaking", p_target);
		}

		public void PlayGameBalloon(GameObject p_target)
		{
			wwise.PostEvent("play_sx_game_balloon_pop", p_target);
		}

		public void PlayGameBalloonRadar(GameObject p_target)
		{
			wwise.PostEvent("play_sx_game_balloon_radar", p_target);
		}

		public void StopGameBalloonRadar(GameObject p_target)
		{
			wwise.PostEvent("stop_sx_game_balloon_radar", p_target);
		}

		public void PlaySmallStepComplete()
		{
			wwise.PostEvent("play_sx_game_small_step_complete", base.gameObject);
		}

		public void PlayBigStepComplete()
		{
			wwise.PostEvent("play_sx_game_big_step_complete", base.gameObject);
		}

		public void PlayGameRadar()
		{
			m_is_playing["game-radar"] = true;
			wwise.PostEvent("play_sx_game_radar", base.gameObject);
		}

		public void StopGameRadar()
		{
			m_is_playing["game-radar"] = false;
			wwise.PostEvent("stop_sx_game_radar", base.gameObject);
		}

		public void UpdateGameRadar(float p_proximity)
		{
			wwise.SetRTPC("radar_proximity", p_proximity, base.gameObject);
		}

		public void PlayGameRadioSignal()
		{
			wwise.PostEvent("play_sx_game_signal", base.gameObject);
		}

		public void StopGameRadioSignal()
		{
			wwise.PostEvent("stop_sx_game_signal", base.gameObject);
			wwise.SetRTPC("radio_signal", 1f);
		}

		public void PlayOnboardingIntro()
		{
			wwise.PostEvent("play_onboarding_intro", base.gameObject);
		}

		public void PlayOnboardingEnd()
		{
			wwise.PostEvent("play_onboarding_end_pc", base.gameObject);
		}

		public void StopOnboardingIntro()
		{
			wwise.PostEvent("stop_onboarding_intro", base.gameObject);
		}

		public void StopOnboardingEnd()
		{
			wwise.PostEvent("stop_onboarding_end_pc", base.gameObject);
		}

		public void UpdateGameRadioSignal(float p_signal_strength, GameObject p_drone_object)
		{
			float f = Mathf.Clamp01(p_signal_strength / 0.5f);
			f = Mathf.Pow(f, 2f);
			wwise.SetRTPC("radio_signal", f, base.gameObject);
			wwise.SetRTPC("radio_signal", f);
			if ((bool)p_drone_object)
			{
				wwise.SetRTPC("radio_signal", f, p_drone_object);
			}
		}

		public void ResetGameRadioSignal(GameObject p_drone_object = null)
		{
			StopGameRadioSignal();
			UpdateGameRadioSignal(1f, p_drone_object);
		}

		public void UpdateTimescale(float p_timescale)
		{
			wwise.SetRTPC("timescale", p_timescale);
		}

		public void SceneMainToGame(float p_delay = 1f)
		{
			FadeStopMusicMain(p_delay);
			PlayUIStartGame();
		}

		public void FadeStopMusicMain(float p_delay)
		{
			FadeOut(0.5f);
			RunOnce(StopMusicMain, p_delay);
		}

		public void SceneGameToMain(float p_delay = 1f)
		{
			FadeOut(0.5f);
			Activity.RunOnce(delegate
			{
				StopMusicAll();
				StopGameRadioSignal();
				StopGameRadar();
				wwise.PostEvent("stop_drone_sounds_all", base.gameObject);
			}, p_delay);
			PlayUIStartGame();
		}

		public void FadeStopMusic(float p_duration = 0.5f, float p_delay = 1f)
		{
			FadeOut(p_duration);
			Activity.RunOnce(delegate
			{
				StopMusicAll();
			}, p_delay);
		}

		protected void RefreshVolume()
		{
			float num = (AudioListener.volume = volume);
			float num3 = m_volumeMusic;
			float num4 = m_volumeSFX;
			float num5 = num;
			if (!IsAudioMotorEnabled())
			{
				num5 = 0f;
			}
			wwise.SetRTPC("volume_mx", volumeMax * num3 * num);
			wwise.SetRTPC("volume_sx", volumeMax * num4 * num);
			wwise.SetRTPC("volume_dx", volumeDrones * volumeMax * num5);
		}

		private bool IsAudioMotorEnabled()
		{
			if (!base.app.model.storage)
			{
				return true;
			}
			if (!base.app.model.storage.state)
			{
				return true;
			}
			if (!base.app.model.storage.state.player)
			{
				return true;
			}
			if (!base.app.model.storage.state.player.settings)
			{
				return true;
			}
			if (!base.app.model.storage.state.player.settings.audio)
			{
				return true;
			}
			return base.app.model.storage.state.player.settings.audio.audioMotorEnabled;
		}
	}
}
