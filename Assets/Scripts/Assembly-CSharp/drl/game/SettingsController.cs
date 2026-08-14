using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.Rendering;
using UnityStandardAssets.ImageEffects;
using drl.backend;
using drl.level;
using drl.network;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class SettingsController : Controller<DRLApp>
	{
		private float[] m_graphicsRes;

		private bool m_graphicsFullscreen;

		private bool m_graphicsExclusiveMode;

		private int m_graphicsTargetScreen;

		private bool m_lensDistortionSetting;

		private FullScreenMode m_fullscreenMode;

		public static float damageTier1 = 0.1f;

		public static float damageTier2 = 0.25f;

		public static float damageTier3 = 0.5f;

		public static float speedReduction1 = 0.1f;

		public static float speedReduction2 = 0.2f;

		public static float speedReduction3 = 0.3f;

		public static float lineDeviation1 = 0.1f;

		public static float lineDeviation2 = 0.2f;

		public static float lineDeviation3 = 0.3f;

		public static float damageCrashThreshold = 0.2f;

		private Activity m_refreshFPSTimer;

		private Transform m_current_level_light_root;

		private Transform m_current_track_light_root;

		private List<Light> m_level_lights = new List<Light>();

		private List<Light> m_track_lights = new List<Light>();

		public SettingsStateModel model => base.app.model.storage.state.player.settings;

		private void GetScreenSettings()
		{
			GraphicsStateModel graphics = model.graphics;
			m_graphicsFullscreen = Screen.fullScreen;
			m_graphicsExclusiveMode = Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen;
			m_graphicsRes = new float[2];
			m_graphicsRes[0] = Screen.currentResolution.width;
			m_graphicsRes[1] = Screen.currentResolution.height;
			m_graphicsTargetScreen = graphics.targetScreen;
		}

		private void SaveScreenSettings()
		{
			GraphicsStateModel graphics = model.graphics;
			m_graphicsFullscreen = graphics.fullscreen;
			m_graphicsExclusiveMode = graphics.exclusiveMode;
			m_graphicsRes = graphics.resolution;
			m_graphicsTargetScreen = graphics.targetScreen;
			m_fullscreenMode = ((!graphics.fullscreen) ? FullScreenMode.Windowed : ((!graphics.exclusiveMode) ? FullScreenMode.FullScreenWindow : FullScreenMode.ExclusiveFullScreen));
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "boot@complete":
			{
				GetScreenSettings();
				GameStateModel game = model.game;
				m_lensDistortionSetting = game.lensDistortion;
				ApplyGraphics();
				ApplySound();
				Notify("settings.ready");
				break;
			}
			case "game.boot":
				ApplyGameLevelAndTrack();
				ApplyPPB();
				ApplyPhysicsTime(0, 0, p_default: true);
				break;
			case "settings.system.screen.apply":
				ApplyGraphics();
				ApplySimulationCameras();
				ApplySimulationDrones();
				ApplyGameLevelAndTrack();
				break;
			case "settings.graphics.map.lighting.apply":
				ApplySimulationCameras();
				break;
			case "game.simulation.load@complete":
				Activity.RunOnce(delegate
				{
					ApplySimulationCameras();
					ApplySimulationDrones();
					ApplyGameLevelAndTrack();
				}, 0.5f);
				break;
			case "game.simulation.drone@ready":
				ApplySimulationDrones();
				ApplyGame();
				break;
			case "settings.game.screen.apply":
				if (m_lensDistortionSetting != model.game.lensDistortion)
				{
					ApplySimulationCameras();
					m_lensDistortionSetting = model.game.lensDistortion;
				}
				ApplyGame();
				break;
			case "settings.startup.graphics.apply":
				GetScreenSettings();
				ApplyGraphics();
				break;
			case "game.intro.animation@complete":
				this.TimerRunOnce(delegate
				{
					if (!(base.app.model.game == null))
					{
						RefreshCrosshairVisibility();
					}
				}, 0.15f);
				if (p_data.Length > 1)
				{
					int p_FCMode = (int)p_data[0];
					int p_diameter2 = (int)p_data[1];
					ApplyPhysicsTime(p_FCMode, p_diameter2);
				}
				break;
			case "garage.drone.fc-changed":
			case "garage.drone.changed":
			{
				if (p_data.Length <= 1)
				{
					break;
				}
				int num = (int)p_data[0];
				int p_diameter = (int)p_data[1];
				FCMode fCMode = (FCMode)num;
				FlightControllerMode flightControllerMode = FlightControllerMode.Pro;
				if (base.app.inGame)
				{
					this.TimerRunOnce(delegate
					{
						Drone playerDrone = base.app.model.game.playerDrone;
						if (base.app.model.storage.state.player.garage.CanUseDamage(playerDrone.rig))
						{
							RCI.SetThrottleCap(80f);
							GameStateModel game2 = base.app.model.storage.state.player.settings.game;
							float p_resistance = (base.app.inVirtualSeason ? game2.batteryResistance : 18f);
							playerDrone.SetBatteryResistance(p_sag: true, base.app.inVirtualSeason, game2.batteryCapacity, p_resistance);
							playerDrone.crashEnabled = base.app.model.storage.state.player.activeFCMode == FCMode.DRLPilot;
							playerDrone.UseCrashDelay(base.app.model.game.type, base.app.inMultiplayer);
						}
						else
						{
							playerDrone.ResetBatteryResistance();
							playerDrone.crashEnabled = false;
							RCI.SetThrottleCap(-1f);
						}
					}, 0.1f);
				}
				if (!base.app.model.storage.state.player.garage.CanUseDamage())
				{
					if (fCMode == FCMode.DRLPilot)
					{
						fCMode = FCMode.Pro;
					}
					base.app.model.storage.state.player.activeFCMode = fCMode;
				}
				ApplyPhysicsTime(fCMode switch
				{
					FCMode.Beginner => 2, 
					FCMode.Intermediate => 3, 
					_ => 4, 
				}, p_diameter);
				break;
			}
			case "ui.footer.calibrate@click":
				if (DRLBootController.ready && (!(base.app.view.ui.screens.current != null) || !(base.app.view.ui.screens.current.name == "calibration-menu-screen")))
				{
					UINavigation.Focus(base.app.view.ui.screens.Open<UICalibrationView>("calibration-menu-screen").autoCalibrationButton);
				}
				break;
			case "scene.game.scenes@start":
				DRLUINavigationSystem.IsLoading = true;
				break;
			case "scene.game.scenes@complete":
				DRLUINavigationSystem.IsLoading = false;
				break;
			case "ui.screen@switch":
			case "ui.screen@return":
			case "ui.screen@change":
			case "ui.screen@close":
				base.app.view.ui.dialog?.Close();
				if (base.app.inGame)
				{
					RefreshGameFPS();
				}
				break;
			case "ui.screen@open":
				if (base.app.inGame)
				{
					RefreshGameFPS();
				}
				if (UIFooterView.isVisible)
				{
					base.app.view.ui.footer.RefreshNavigationButtons();
					UIFooterView.SetNavigationTop(null);
				}
				break;
			case "settings.controller.connect":
			case "settings.controller.disconnect":
				DRLGamepadHotkey.RefreshAll();
				break;
			case "game.pause.form.event@change":
			{
				string text = (p_target ? p_target.name : "");
				DroneCamera c = base.app.model.game.camera;
				switch (text)
				{
				case "camera-fov":
					this.TimerRunOnce(delegate
					{
						RefreshDetailQuality(c.fov);
					}, 1f / 12f);
					break;
				case "camera-mode":
					this.TimerRunOnce(delegate
					{
						RefreshCrosshairVisibility(c.mode);
					}, 1f / 12f);
					break;
				}
				break;
			}
			case "ui.footer.drone@click":
				OpenRigEdit(p_show_store: false);
				break;
			case "ui.footer.profile@click":
				if (DRLBootController.ready && (!(base.app.view.ui.screens.current != null) || !(base.app.view.ui.screens.current.name == "settings-profile-screen")))
				{
					base.app.view.ui.screens.Open<UICalibrationView>("settings-profile-screen");
				}
				break;
			case "ui.footer.settings@click":
				if (DRLBootController.ready && (!(base.app.view.ui.screens.current != null) || !(base.app.view.ui.screens.current.name == "settings-screen")))
				{
					base.app.view.ui.screens.Open("settings-screen", 0f);
				}
				break;
			}
		}

		protected void RefreshGameFPS()
		{
			if (m_refreshFPSTimer != null)
			{
				m_refreshFPSTimer.Stop();
			}
			m_refreshFPSTimer = Activity.RunOnce(delegate
			{
				int count = base.app.view.ui.screens.manager.history.Count;
				bool num = count > 0 && base.app.view.ui.screens.manager.history[0].open;
				Debug.LogWarning($"SettingsController> RefreshGameFPS() sc:{count}");
				if (!num)
				{
					SetFps();
				}
				else
				{
					SetFps(60, p_vsync: true);
				}
			}, 1f);
		}

		public void ApplyGraphics()
		{
			Debug.Log("GPUI | ApplyGraphics()");
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			float[] array = new float[2];
			bool flag = false;
			FullScreenMode fullScreenMode = FullScreenMode.Windowed;
			array = graphics.resolution;
			int num = (int)Mathf.Max(array[0], 800f);
			int num2 = (int)Mathf.Max(array[1], 600f);
			flag = graphics.exclusiveMode;
			fullScreenMode = ((!graphics.exclusiveMode && !graphics.fullscreen) ? FullScreenMode.Windowed : ((!flag) ? FullScreenMode.FullScreenWindow : FullScreenMode.ExclusiveFullScreen));
			FullScreenMode num3 = (Application.isEditor ? m_fullscreenMode : Screen.fullScreenMode);
			bool flag2 = false;
			if (m_graphicsRes[0] != (float)num)
			{
				flag2 = true;
			}
			if (m_graphicsRes[1] != (float)num2)
			{
				flag2 = true;
			}
			if (num3 != fullScreenMode)
			{
				flag2 = true;
			}
			if (flag2)
			{
				SaveScreenSettings();
				StartCoroutine(SetResolutionAndScreen(0, num, num2, fullScreenMode));
			}
			else
			{
				Debug.Log("<b><color=#0f0>SettingsController> SetResolutionAndScreen Not Applied!</color></b>");
			}
			if (graphics.hasQuality)
			{
				Debug.Log("SettingsController> ApplyGraphics / Quality Exists!");
			}
			else
			{
				int num4 = graphics.quality;
				switch (OS.context)
				{
				case "xbss":
				case "xbsx":
				case "xbx":
					num4 = 0;
					break;
				case "xbs":
					num4 = 0;
					break;
				case "ps4base":
					num4 = 0;
					break;
				case "ps4pro":
					num4 = 0;
					break;
				}
				Debug.Log($"SettingsController> ApplyGraphics / Quality Not Available / set-preset[{num4}]");
				if (num4 >= 0)
				{
					graphics.SetFromPreset(num4);
				}
			}
			SetTextureQuality(graphics.texture);
			SetShadowQuality(graphics.shadow);
			SetEffectsQuality(graphics.effectsQuality);
			SetDetailsQuality(graphics.details);
			SetTreesQuality();
			SetWaterReflectionQuality(graphics.waterReflection ? 1 : 0);
			SetFps();
			base.app.brightness.exposure = 0f;
			Graphics.activeTier = (GraphicsTier)graphics.tier;
			PlayerPrefs.SetInt("graphics-tier", graphics.tier);
			Debug.Log("<b><color=#ff3>SettingsController> ApplyGraphics / quality[" + graphics.quality + "] tier[" + Graphics.activeTier.ToString() + "] resolution[" + (int)array[0] + "x" + (int)array[1] + "," + fullScreenMode.ToString() + "] vsync[" + QualitySettings.vSyncCount + " @ " + Application.targetFrameRate + "]</color></b>");
			Notify(1f / 30f, "settings.graphics.apply");
		}

		public void SetFps(int p_value, bool p_vsync)
		{
			QualitySettings.vSyncCount = (p_vsync ? 1 : 0);
			Application.targetFrameRate = (p_vsync ? (-1) : p_value);
			if (Application.targetFrameRate > 0)
			{
				int num = Mathf.Max(60, Screen.currentResolution.refreshRate);
				int num2 = Mathf.Max(200, (num + 60) / 10 * 10);
				if (Application.targetFrameRate > num2)
				{
					Debug.LogWarning($"SettingsController> SetFps / Adjusting Target Framerate [{Application.targetFrameRate} -> {num2}]");
				}
				Application.targetFrameRate = Mathf.Min(Application.targetFrameRate, num2);
			}
			Debug.Log($"SettingsController> SetFps / fps[{Application.targetFrameRate}] vsync[{QualitySettings.vSyncCount}]");
		}

		public void SetFps(bool p_forceVsync = false)
		{
			if (!base.validContext)
			{
				return;
			}
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			if ((bool)graphics)
			{
				if (p_forceVsync)
				{
					graphics.vsync = 1;
				}
				SetFps(graphics.fpsLimit, graphics.vsync > 0);
				Debug.Log($"SettingsController> SetFps - Force / m.fpsLimit[{graphics.fpsLimit}] m.vsync[{graphics.vsync}]");
			}
		}

		private IEnumerator SetResolutionAndScreen(int p_target, int p_width, int p_height, FullScreenMode p_fullscreen)
		{
			FullScreenMode fullscreenMode = p_fullscreen;
			Debug.Log("<b><color=#ff0>SettingsController> SetResolutionAndScreen / fullscreen-mode[" + fullscreenMode.ToString() + "] resolution[" + p_width + "," + p_height + "]</color></b>");
			bool flag = true;
			if (Application.isEditor)
			{
				flag = false;
			}
			if (flag)
			{
				Screen.SetResolution(p_width, p_height, fullscreenMode);
			}
			yield return null;
		}

		public void SetFogQuality(PostProcessingProfile p_ppp)
		{
			p_ppp.fog.enabled = true;
			if (!base.app.inGame)
			{
				return;
			}
			LevelSettings settings = base.app.model.game.level.settings;
			DRLMapLightingPreset activePreset = settings.light.GetActivePreset();
			Transform transform = null;
			if (base.app.model.game.type == GameFlag.MapEditor)
			{
				Camera main = Camera.main;
				if ((bool)main)
				{
					transform = main.transform.parent;
				}
			}
			else
			{
				transform = base.app.model.game.camera?.transform;
			}
			Transform transform2 = (transform ? transform.Find("fog") : null);
			if (transform2 == null)
			{
				Debug.LogWarning("SettingsController> SetFogQuality / Camera is null! Couldn't SetFogQuality");
				return;
			}
			Camera camera = (transform ? transform2.GetComponent<Camera>() : null);
			if (!camera)
			{
				Debug.LogWarning("SettingsController> SetFogQuality / Camera is null! Couldn't SetFogQuality");
				return;
			}
			bool active = settings.light.enhancedFog;
			camera.gameObject.SetActive(active);
			camera.targetTexture = settings.light.enhancedFog;
			camera.GetComponent<Skybox>().material = (activePreset ? activePreset.fog : null);
			FogModel.Settings settings2 = p_ppp.fog.settings;
			settings2.texture = settings.light.enhancedFog;
			p_ppp.fog.settings = settings2;
		}

		public void SetAntiAliasingQuality(int p_quality, PostProcessingProfile p_ppp)
		{
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			p_quality = Mathf.Clamp(p_quality, 0, graphics.antiAliasingQualityPresets.Count - 1);
			if (p_quality < 0)
			{
				return;
			}
			AntiAliasingQualitySettings antiAliasingQualitySettings = graphics.antiAliasingQualityPresets[p_quality];
			if (antiAliasingQualitySettings != null)
			{
				AntialiasingModel.Settings settings = p_ppp.antialiasing.settings;
				p_ppp.antialiasing.enabled = antiAliasingQualitySettings.enabled;
				settings.method = antiAliasingQualitySettings.method;
				settings.fxaaSettings.preset = antiAliasingQualitySettings.preset;
				if (settings.method == AntialiasingModel.Method.Fxaa)
				{
					settings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.Quality;
				}
				p_ppp.antialiasing.settings = settings;
			}
		}

		public void SetEyeAdaptationEnabled(bool p_enabled, PostProcessingProfile p_ppp)
		{
			EyeAdaptationModel.Settings settings = p_ppp.eyeAdaptation.settings;
			p_ppp.eyeAdaptation.enabled = p_enabled;
			p_ppp.eyeAdaptation.settings = settings;
		}

		public void SetMotionBlurEnabled(bool p_enabled, PostProcessingProfile p_ppp)
		{
			MotionBlurModel.Settings settings = p_ppp.motionBlur.settings;
			p_ppp.motionBlur.enabled = p_enabled;
			p_ppp.motionBlur.settings = settings;
		}

		public void SetDepthOfFieldQuality(int p_quality, PostProcessingProfile p_ppp)
		{
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			p_quality = Mathf.Clamp(p_quality, 0, graphics.depthOfFieldQualityPresets.Count - 1);
			if (p_quality >= 0)
			{
				DepthOfFieldQualitySettings depthOfFieldQualitySettings = graphics.depthOfFieldQualityPresets[p_quality];
				if (depthOfFieldQualitySettings != null)
				{
					DepthOfFieldModel.Settings settings = p_ppp.depthOfField.settings;
					settings.kernelSize = depthOfFieldQualitySettings.kernelSize;
					p_ppp.depthOfField.settings = settings;
				}
			}
		}

		public void SetDepthOfFieldEnabled(bool p_enabled, PostProcessingProfile p_ppp)
		{
			DepthOfFieldModel.Settings settings = p_ppp.depthOfField.settings;
			p_ppp.depthOfField.enabled = p_enabled;
			p_ppp.depthOfField.settings = settings;
		}

		public void SetAmbientOcclusionQuality(int p_quality, PostProcessingProfile p_ppp)
		{
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			p_quality = Mathf.Clamp(p_quality, 0, graphics.ambientOcclusionQualityPresets.Count - 1);
			if (p_quality >= 0)
			{
				AmbientOcclusionQualitySettings ambientOcclusionQualitySettings = graphics.ambientOcclusionQualityPresets[p_quality];
				if (ambientOcclusionQualitySettings != null)
				{
					AmbientOcclusionModel.Settings settings = p_ppp.ambientOcclusion.settings;
					p_ppp.ambientOcclusion.enabled = ambientOcclusionQualitySettings.enabled;
					settings.sampleCount = ambientOcclusionQualitySettings.sampleCount;
					settings.downsampling = ambientOcclusionQualitySettings.downSampling;
					settings.forceForwardCompatibility = !graphics.advancedRendering && ambientOcclusionQualitySettings.forceForwardCompatibility;
					settings.highPrecision = ambientOcclusionQualitySettings.highPrecision;
					p_ppp.ambientOcclusion.settings = settings;
				}
			}
		}

		public void SetAmbientOcclusionEnabled(bool p_enabled, PostProcessingProfile p_ppp, bool p_low_end = false)
		{
			AmbientOcclusionModel.Settings settings = p_ppp.ambientOcclusion.settings;
			p_ppp.ambientOcclusion.enabled = p_enabled;
			settings.sampleCount = (p_low_end ? AmbientOcclusionModel.SampleCount.Lowest : AmbientOcclusionModel.SampleCount.Medium);
			settings.downsampling = (p_low_end ? true : false);
			settings.forceForwardCompatibility = false;
			settings.highPrecision = false;
			p_ppp.ambientOcclusion.settings = settings;
		}

		public void GetAmbientOcclusionIntensityAndRadius(out float p_int, out float p_rad)
		{
			p_rad = 1f;
			p_int = 1f;
			GraphicsStateModel graphics = model.graphics;
			GraphicsStateModel graphics2 = base.app.model.storage.state.player.settings.graphics;
			int num = Mathf.Clamp(graphics.ambientOcclusion, 0, graphics2.ambientOcclusionQualityPresets.Count - 1);
			if (num >= 0)
			{
				AmbientOcclusionQualitySettings ambientOcclusionQualitySettings = graphics2.ambientOcclusionQualityPresets[num];
				if (ambientOcclusionQualitySettings != null)
				{
					p_rad = ambientOcclusionQualitySettings.radius;
					p_int = ambientOcclusionQualitySettings.intensity;
				}
			}
		}

		public void SetPostProcessingQuality(int p_quality, PostProcessingProfile p_ppp)
		{
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			p_quality = Mathf.Clamp(p_quality, 0, graphics.postProcessingQualityPresets.Count - 1);
			if (p_quality < 0)
			{
				return;
			}
			PostProcessingQualitySettings postProcessingQualitySettings = graphics.postProcessingQualityPresets[p_quality];
			if (postProcessingQualitySettings != null)
			{
				p_ppp.eyeAdaptation.enabled = postProcessingQualitySettings.eyeAdaptation;
				bool num = false || Application.platform == RuntimePlatform.OSXPlayer;
				bool flag = false;
				bool flag2 = false;
				if (num || flag || flag2)
				{
					p_ppp.eyeAdaptation.enabled = false;
				}
				p_ppp.bloom.enabled = postProcessingQualitySettings.bloom;
				p_ppp.chromaticAberration.enabled = postProcessingQualitySettings.chromaticAberration;
				p_ppp.screenSpaceReflection.enabled = false;
				p_ppp.grain.enabled = postProcessingQualitySettings.grain;
				p_ppp.colorGrading.enabled = postProcessingQualitySettings.colorGrading;
			}
		}

		public void SetExposureOffset(PostProcessingProfile p_ppp, float p_offset = 0f)
		{
			float exposure = GetExposure(p_ppp);
			SetExposure(p_ppp, exposure + p_offset);
		}

		public void SetExposure(PostProcessingProfile p_ppp, float p_value)
		{
			ColorGradingModel.Settings settings = p_ppp.colorGrading.settings;
			settings.basic.postExposure = p_value;
			p_ppp.colorGrading.settings = settings;
		}

		public float GetExposure(PostProcessingProfile p_ppp)
		{
			return p_ppp.colorGrading.settings.basic.postExposure;
		}

		public void SetTextureQuality(int p_quality)
		{
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			p_quality = Mathf.Clamp(p_quality, 0, graphics.textureQualityPresets.Count - 1);
			if (p_quality < 0)
			{
				return;
			}
			TextureQualitySettings textureQualitySettings = graphics.textureQualityPresets[p_quality];
			if (textureQualitySettings != null)
			{
				int masterTextureLimit = QualitySettings.masterTextureLimit;
				AnisotropicFiltering anisotropicFiltering = QualitySettings.anisotropicFiltering;
				AnisotropicFiltering anisotropicFiltering2 = ((textureQualitySettings.filtering != TextureQualitySettings.Filtering.ForcedOn) ? AnisotropicFiltering.Enable : AnisotropicFiltering.ForceEnable);
				if (masterTextureLimit != (int)textureQualitySettings.quality)
				{
					QualitySettings.masterTextureLimit = (int)textureQualitySettings.quality;
				}
				if (anisotropicFiltering2 != anisotropicFiltering)
				{
					QualitySettings.anisotropicFiltering = anisotropicFiltering2;
				}
			}
		}

		public void SetShadowQuality(int p_quality)
		{
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			p_quality = Mathf.Clamp(p_quality, 0, graphics.shadowQualityPresets.Count - 1);
			if (p_quality < 0)
			{
				return;
			}
			ShadowQualitySettings shadowQualitySettings = graphics.shadowQualityPresets[p_quality];
			if (shadowQualitySettings != null)
			{
				if (QualitySettings.shadows != shadowQualitySettings.quality)
				{
					QualitySettings.shadows = shadowQualitySettings.quality;
				}
				if (QualitySettings.shadowResolution != shadowQualitySettings.resolution)
				{
					QualitySettings.shadowResolution = shadowQualitySettings.resolution;
				}
				if (QualitySettings.shadowDistance != shadowQualitySettings.distance)
				{
					QualitySettings.shadowDistance = shadowQualitySettings.distance;
				}
				if (QualitySettings.shadowCascades != (int)shadowQualitySettings.cascades)
				{
					QualitySettings.shadowCascades = (int)shadowQualitySettings.cascades;
				}
				if (QualitySettings.shadowCascade2Split != shadowQualitySettings.shadowCascade2Split)
				{
					QualitySettings.shadowCascade2Split = shadowQualitySettings.shadowCascade2Split;
				}
				if (QualitySettings.shadowCascade4Split != shadowQualitySettings.shadowCascade4Split)
				{
					QualitySettings.shadowCascade4Split = shadowQualitySettings.shadowCascade4Split;
				}
			}
			if ((bool)base.app.controller.game && (bool)base.app.controller.game.level)
			{
				base.app.controller.game.level.ApplyLevelSettings();
			}
		}

		public void SetEffectsQuality(int p_quality)
		{
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			p_quality = Mathf.Clamp(p_quality, 0, graphics.effectsQualityPresets.Count - 1);
			if (p_quality >= 0)
			{
				EffectsQualitySettings effectsQualitySettings = graphics.effectsQualityPresets[p_quality];
				if (effectsQualitySettings != null)
				{
					QualitySettings.softParticles = effectsQualitySettings.softParticles;
					QualitySettings.skinWeights = effectsQualitySettings.blendWeights;
					QualityGroup.SetQuality(effectsQualitySettings.categoryName, effectsQualitySettings.categoryEnabled);
				}
			}
		}

		public void SetTreesQuality()
		{
			LevelSettings ls = (base.app.inGame ? base.app.model.game.level.settings : null);
			float lodBias = 1f;
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			float lodBias2 = graphics.detailsQualityPresets[graphics.details].lodBias;
			lodBias = Mathf.Ceil(lodBias2 * (lodBias2 * lodBias2) * 10f) / 10f;
			lodBias = Mathf.Clamp(lodBias, 0.25f, 1f);
			if ((bool)ls)
			{
				this.TimerRunOnce(delegate
				{
					ls.terrain.SetInstancerTreeQuality(lodBias);
				}, 1.5f);
			}
		}

		public void SetDetailsQuality(int p_quality, float p_fov = 0f)
		{
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			p_quality = Mathf.Clamp(p_quality, 0, graphics.detailsQualityPresets.Count - 1);
			if (p_quality < 0)
			{
				return;
			}
			DetailsQualitySettings s = graphics.detailsQualityPresets[p_quality];
			LevelSettings ls = (base.app.inGame ? base.app.model.game.level.settings : null);
			if (s != null)
			{
				FCProfileData active = base.app.model.storage.state.player.settings.tuning.GetActive();
				float num = ((!(p_fov <= 0f)) ? p_fov : (active?.fov ?? 0f));
				float num2 = ((num <= 0f) ? 0f : s.GetLODBiasOffset(num));
				switch (OS.context)
				{
				case "xb":
				case "xbs":
				case "xbx":
				case "xbss":
				case "xbsx":
				case "ps4base":
				case "ps4pro":
				case "ps5":
					num2 = 0f;
					break;
				}
				float num3 = s.lodBias + num2;
				if (Mathf.Abs(QualitySettings.lodBias - num3) > 0f)
				{
					QualitySettings.lodBias = s.lodBias + num2;
				}
				QualityGroup.SetQuality(s.categoryName, s.categoryEnabled);
			}
			if ((bool)ls)
			{
				this.TimerRunOnce(delegate
				{
					ls.terrain.SetInstancerDetailQuality(s.gpuIDetailDensity, s.gpuIMaxDistance, s.gpuIBillboardDistance);
				}, 1.5f);
			}
		}

		public void RefreshDetailQuality(float p_fov = 0f)
		{
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			SetDetailsQuality(graphics.details, p_fov);
		}

		public void SetWaterReflectionQuality(int p_quality)
		{
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			p_quality = Mathf.Clamp(p_quality, 0, graphics.waterReflectionQualityPresets.Count - 1);
			if (p_quality >= 0)
			{
				WaterReflectionQualitySettings waterReflectionQualitySettings = graphics.waterReflectionQualityPresets[p_quality];
				if (waterReflectionQualitySettings != null)
				{
					QualityGroup.SetQuality(waterReflectionQualitySettings.categoryName, waterReflectionQualitySettings.categoryEnabled);
				}
			}
		}

		public void ApplyPPP(PostProcessingBehaviour p_target)
		{
			if (!p_target)
			{
				Debug.LogWarning("SettingsController> ApplyPPP - PostProcessingBehaviour is Null!");
			}
			else if (p_target.profile == null)
			{
				Debug.LogWarning("SettingsController> ApplyPPP - PostProcessingBehaviour.Profile is Null!");
			}
			else if (!(base.app.brightness.ppb == p_target))
			{
				p_target.profile = ApplyPPP(p_target.profile);
			}
		}

		public void ApplyPPP(CameraFX p_camera_fx)
		{
			LevelSettings settings = base.app.model.game.level.settings;
			PostProcessingBehaviour ppb = p_camera_fx.ppb;
			PostProcessingProfile p_ppp = (settings.pppTemplate ? settings.pppTemplate : ppb.profile);
			ppb.profile = ApplyPPP(p_ppp);
		}

		public PostProcessingProfile ApplyPPP(PostProcessingProfile p_ppp)
		{
			PostProcessingProfile postProcessingProfile = (p_ppp ? UnityEngine.Object.Instantiate(p_ppp) : p_ppp);
			if (!postProcessingProfile)
			{
				Debug.LogWarning("SettingsController> ApplyPPP - PostProcessingProfile is Null!");
				return null;
			}
			postProcessingProfile.name = p_ppp.name;
			GraphicsStateModel graphics = model.graphics;
			SetFogQuality(postProcessingProfile);
			SetAntiAliasingQuality(graphics.antialias, postProcessingProfile);
			SetDepthOfFieldQuality(graphics.depthOfField, postProcessingProfile);
			SetAmbientOcclusionQuality(graphics.ambientOcclusion, postProcessingProfile);
			postProcessingProfile.motionBlur.enabled = graphics.motionBlur;
			if ((bool)base.app.model.game && base.app.arguments.game.type == GameFlag.MapEditor)
			{
				postProcessingProfile.motionBlur.enabled = false;
			}
			SetPostProcessingQuality(graphics.postProcessing, postProcessingProfile);
			if ((bool)base.app.model.game)
			{
				LevelSettings settings = base.app.model.game.level.settings;
				float num = (graphics.advancedRendering ? 0f : settings.exposureCompensation.advancedRenderingOff);
				float num2 = (graphics.eyeAdaptation ? 0f : settings.exposureCompensation.eyeAdaptationOff);
				SetExposureOffset(postProcessingProfile, num + num2);
			}
			return postProcessingProfile;
		}

		public void ApplyPPB()
		{
			PostProcessingBehaviour[] array = UnityEngine.Object.FindObjectsOfType<PostProcessingBehaviour>();
			foreach (PostProcessingBehaviour p_target in array)
			{
				ApplyPPP(p_target);
			}
		}

		public void ApplySound()
		{
			Debug.Log("SettingsController> Apply Sound");
			AudioStateModel audio = base.app.model.storage.state.player.settings.audio;
			base.app.view.audio.volume = audio.volumeMain;
			base.app.view.audio.volumeMusic = audio.volumeMusic;
			base.app.view.audio.volumeSFX = audio.volumeSFX;
			base.app.view.audio.volumeDrones = audio.volumeSFX;
			Notify("settings.sound.apply");
		}

		public void ApplySimulationCameras()
		{
			ApplySimulationCameras(false);
		}

		public void ApplySimulationCameras(bool p_force = false)
		{
			if (!base.app.model.game)
			{
				return;
			}
			GameStateModel game = model.game;
			GraphicsStateModel graphics = model.graphics;
			PlayerStateModel player = base.app.model.storage.state.player;
			bool advancedRendering = graphics.advancedRendering;
			_ = graphics.quality;
			DroneSimulation simulation = base.app.model.game.simulation;
			_ = OS.context;
			LevelSettings settings = base.app.model.game.level.settings;
			FCProfileData active = player.settings.tuning.GetActive();
			List<DroneCamera> list = new List<DroneCamera>();
			if ((bool)simulation)
			{
				list.AddRange(simulation.cameras.list);
			}
			else if ((bool)base.app.model.game.camera)
			{
				list.Add(base.app.model.game.camera);
			}
			if (p_force)
			{
				if (list.Count == 0 || list[0] == null || list[0].main == null || list[0].main.targetTexture == null)
				{
					return;
				}
				if (list[0].main.targetTexture.name == "drone-camera-dynamic-rt")
				{
					list[0].main.enabled = true;
					return;
				}
			}
			Debug.Log("SettingsController> ApplySimulationCameras - count[" + list.Count + "]");
			float[] renderScaleResolution = graphics.renderScaleResolution;
			int num = Mathf.FloorToInt(renderScaleResolution[0]);
			int num2 = Mathf.FloorToInt(renderScaleResolution[1]);
			for (int i = 0; i < list.Count; i++)
			{
				DroneCamera droneCamera = list[i];
				if ((bool)droneCamera.drfx)
				{
					droneCamera.drfx.Initialize();
					droneCamera.drfx.Resize(num, num2, p_force);
					droneCamera.drfx.SetCanvas(base.app.view.ui.canvas);
					droneCamera.drfx.SetDynamicResolution(p_flag: false);
					Debug.Log($"SettingsController> ApplySimulationCameras / camera[{droneCamera.name}] is-dynamic[{droneCamera.drfx.auto}] dynamic-resolution[{droneCamera.drfx.ratio}] size[{num},{num2}]");
				}
				settings.terrain.SetCamera(droneCamera.main);
			}
			if (settings.terrain.gpuInstancerTreeManager != null)
			{
				settings.terrain.gpuInstancerTreeManager.gameObject.SetActive(value: true);
			}
			for (int j = 0; j < list.Count; j++)
			{
				DroneCamera droneCamera2 = list[j];
				droneCamera2.SetGameCameraEnabled(p_flag: false);
				droneCamera2.SetGameCameraEnabled(p_flag: true);
				droneCamera2.main.RemoveAllCommandBuffers();
				droneCamera2.main.renderingPath = RenderingPath.DeferredShading;
				droneCamera2.lensDistortionAllowed = game.lensDistortion;
				if ((bool)droneCamera2.fx)
				{
					droneCamera2.fx.depthOfFieldEnabled = graphics.depthOfField > 0;
					if (!droneCamera2.fx.depthOfFieldEnabled)
					{
						droneCamera2.fx.ClearDOF();
					}
					droneCamera2.fx.distortEnabled = droneCamera2.lensDistortionAllowed && droneCamera2.mode == DroneCameraModeType.FPV;
					if ((bool)droneCamera2.fx.ppb && droneCamera2.fx.ppb.enabled)
					{
						droneCamera2.fx.ppb.enabled = false;
						droneCamera2.fx.ppb.enabled = true;
					}
				}
				if (droneCamera2.mode == DroneCameraModeType.FPV)
				{
					droneCamera2.RefreshFOV(active.fov);
				}
				bool sunShafts = base.app.model.storage.state.player.settings.graphics.sunShafts;
				bool flag = (bool)base.app.model.game && base.app.model.game.level.settings.sunshafts.enabled;
				for (int k = 0; k < droneCamera2.cameras.Count; k++)
				{
					SunShafts sunShafts2 = (droneCamera2.cameras[k] ? droneCamera2.cameras[k].GetComponent<SunShafts>() : null);
					if ((bool)sunShafts2)
					{
						droneCamera2.fx.sunshaftsAllowed = sunShafts && flag;
						sunShafts2.enabled = graphics.sunShafts && droneCamera2.fx.sunshaftsAllowed;
					}
				}
				ApplyPPP(droneCamera2.fx);
				droneCamera2.fx.ReadPPP();
				CameraFadeLights component = droneCamera2.main.GetComponent<CameraFadeLights>();
				if ((bool)component)
				{
					component.enabled = advancedRendering;
				}
			}
			if (base.app.model.game != null && base.app.view.ui.screens.manager.InHistory("game-pause-screen"))
			{
				base.app.view.ui.screens.SetStaticBackground();
			}
		}

		public void ApplySimulationDrones()
		{
			if (!base.app.model.game)
			{
				return;
			}
			bool advancedRendering = model.graphics.advancedRendering;
			DroneSimulation simulation = base.app.model.game.simulation;
			if (!simulation)
			{
				return;
			}
			for (int i = 0; i < simulation.drones.list.Count; i++)
			{
				Drone drone = simulation.drones.list[i];
				if ((bool)drone.renderer && (bool)drone.renderer.light)
				{
					drone.renderer.light.enabled = advancedRendering;
				}
			}
			Drone playerDrone = base.app.model.game.playerDrone;
			if (!(playerDrone == null))
			{
				playerDrone.renderer.propsVisible = base.app.model.storage.state.player.settings.game.propsVisible;
				DroneRenderer renderer = playerDrone.renderer;
				renderer.VisibilityChanged = (Action<bool>)Delegate.Remove(renderer.VisibilityChanged, new Action<bool>(OnVisibilityChanged));
				DroneRenderer renderer2 = playerDrone.renderer;
				renderer2.VisibilityChanged = (Action<bool>)Delegate.Combine(renderer2.VisibilityChanged, new Action<bool>(OnVisibilityChanged));
			}
		}

		public void ApplyGameLevelAndTrack()
		{
			if (!base.app.model.game)
			{
				return;
			}
			bool advancedRendering = model.graphics.advancedRendering;
			string message = $"SettingsController> ApplyGameLevelAndTrack / Lights - adv-render[{advancedRendering}]\n";
			Transform transform = (base.app.model.game.level.root ? base.app.model.game.level.root.transform : null);
			if (!transform)
			{
				Debug.LogWarning("SettingsController> ApplyGameLevelAndTrack / level.root is null");
				m_level_lights.Clear();
			}
			else if (m_current_level_light_root != transform)
			{
				m_current_level_light_root = transform;
				m_level_lights.Clear();
				Hierarchy.Traverse(transform, delegate(Light it)
				{
					if (it.type == LightType.Directional)
					{
						return true;
					}
					if (it.name.Contains("fix-"))
					{
						return true;
					}
					m_level_lights.Add(it);
					return true;
				});
			}
			for (int num = 0; num < m_level_lights.Count; num++)
			{
				Light light = m_level_lights[num];
				if ((bool)light)
				{
					light.enabled = advancedRendering;
				}
			}
			transform = (base.app.model.game.level.track.root ? base.app.model.game.level.track.root.transform : null);
			if (!transform)
			{
				Debug.LogWarning("SettingsController> ApplyGameLevelAndTrack / level.track.root is null");
				m_track_lights.Clear();
			}
			else if (m_current_track_light_root != transform)
			{
				m_current_track_light_root = transform;
				m_track_lights.Clear();
				Hierarchy.Traverse(transform, delegate(Light it)
				{
					if (it.type == LightType.Directional)
					{
						return true;
					}
					if (it.name.Contains("fix-"))
					{
						return true;
					}
					m_track_lights.Add(it);
					return true;
				});
			}
			for (int num2 = 0; num2 < m_track_lights.Count; num2++)
			{
				Light light2 = m_track_lights[num2];
				if ((bool)light2)
				{
					light2.enabled = advancedRendering;
				}
			}
			Debug.Log(message);
		}

		private void ApplyPhysicsTime(int p_FCMode, int p_diameter, bool p_default = false)
		{
		}

		public void ApplyGame()
		{
			GameModel g = base.app.model.game;
			if (!g)
			{
				return;
			}
			GameStateModel d = base.app.model.storage.state.player.settings.game;
			UIHUD hud = base.app.view.ui.game.hud;
			hud.race.timeContainer.SetActive(d.raceStats);
			hud.race.lapContainer.SetActive(d.raceStats && g.level.track.laps > 1);
			bool positionEnabled = d.raceStats && g.racerCount > 1;
			bool gateMarkers = d.gateMarkers;
			hud.race.positionEnabled = positionEnabled;
			hud.race.raceStatsEnabled = d.raceStats;
			hud.race.speed.gameObject.SetActive(value: false);
			base.app.model.game.level.track.pathTrace.SetRendererColor(DRLColor.raceLineColors[d.raceLineColor]);
			if ((bool)hud.standingsFade && base.app.view.ui.screens.current == null)
			{
				hud.standingsFade.gameObject.SetActive(value: true);
				hud.standingsFade.alpha = 0f;
				if (d.raceAutoStandings && (g.multiplayer || g.racerCount > 1))
				{
					hud.standingsFade.FadeIn(0.12f);
				}
			}
			hud.lowFPSWarning.gameObject.SetActive(d.fpsWarning);
			this.TimerRunOnce(delegate
			{
				RefreshCrosshairVisibility(g.camera.mode);
			}, 0.5f);
			if (g.type != GameFlag.Mission)
			{
				RectTransform obj = hud.controller.transform as RectTransform;
				obj.anchorMin = new Vector2(0.5f, 0f);
				obj.anchorMax = new Vector2(0.5f, 0f);
				obj.anchoredPosition = new Vector2(0f, -150f);
				hud.controller.gameObject.SetActive(d.controllerOverlay);
				hud.controller.SetAnimation(UIControllerAnimationType.UserInput);
				switch (g.type)
				{
				case GameFlag.Race:
				{
					hud.marker.gameObject.SetActive(gateMarkers);
					bool flag = base.app.controller.game.GetMode<RaceController>().model.raceActive && d.controllerOverlay;
					hud.controller.fade.Fade(flag ? 1f : (-0.1f));
					break;
				}
				case GameFlag.Freestyle:
					hud.controller.fade.Fade(d.controllerOverlay ? 1f : (-0.1f));
					break;
				}
				if (g.mode == GameFlag.NetworkMultiplayer)
				{
					NetworkRoom room = base.app.model.network.room;
					if (room != null && room.IsSpectator)
					{
						hud.damage.Show(p_flag: false);
						hud.controller.gameObject.SetActive(value: false);
					}
				}
			}
			base.app.model.game.level.track.pathTrace.rendererEnabled = d.raceGuide;
			DroneSimulation simulation = g.simulation;
			if ((bool)simulation)
			{
				for (int num = 0; num < simulation.drones.list.Count; num++)
				{
					Drone it = simulation.drones.Get(num);
					if (!it)
					{
						continue;
					}
					it.SetPropwash(d.propwash);
					if (g.IsPlayer(it) && !it.isGhost && !it.isRemote)
					{
						if (!it.physics)
						{
							continue;
						}
						this.TimerRunOnce(delegate
						{
							string username = base.app.model.storage.state.player.profile.username;
							it.ResetBatteryResistance();
							string text = "";
							if (base.app.model.network.room != null)
							{
								text = base.app.model.network.room.RaceId;
							}
							else if (base.app.inTournament)
							{
								text = base.app.tournament.guid;
							}
							if (!string.IsNullOrEmpty(text))
							{
								UnityEngine.Random.InitState(text.GetHashCode());
							}
							if (base.app.model.storage.state.player.garage.CanUseDamage(it.rig))
							{
								d.batteryResistance = (base.app.inVirtualSeason ? (Mathf.Round(UnityEngine.Random.Range(d.batteryResistanceMin, d.batteryResistanceMax) * 100f) / 100f) : 18f);
								it.SetBatteryResistance(p_sag: true, base.app.inVirtualSeason, d.batteryCapacity, d.batteryResistance);
								Debug.Log($"SettingsController> ApplyGame / <color=#ff0>Battery Physics for [{username}] / battery | sag[{true}] drain[{true}] capacity[{d.batteryCapacity}] resistance[{d.batteryResistance}]</color>");
								if (base.app.model.storage.state.player.activeFCMode == FCMode.DRLPilot)
								{
									base.app.model.service.GetCrashSettings(delegate(DRLCrashPenaltyData p_crashData)
									{
										if (base.validContext && p_crashData != null)
										{
											RefreshCrashPenaltyData(p_crashData);
											Debug.Log("SettingsController> Refreshed crash settings data.");
										}
									});
								}
							}
						}, 0.1f);
					}
					else
					{
						it.renderer.SetTrailsEnabled(d.trails);
					}
				}
			}
			if (hud.marker.isActiveAndEnabled)
			{
				hud.marker.markerColor = DRLColor.checkPointColors[d.checkPointColor];
			}
		}

		private void OnVisibilityChanged(bool p_shadowsOnly)
		{
			if (p_shadowsOnly && !(base.app.model.game == null) && (bool)base.app.model.game.simulation)
			{
				base.app.model.game.playerDrone.renderer.propsVisible = base.app.model.storage.state.player.settings.game.propsVisible;
			}
		}

		private void RefreshCrosshairVisibility(DroneCameraModeType p_mode)
		{
			if ((bool)base.app.controller.game)
			{
				base.app.controller.game.RefreshCrosshairVisibility(p_mode);
			}
		}

		private void RefreshCrosshairVisibility()
		{
			if ((bool)base.app.controller.game)
			{
				base.app.controller.game.RefreshCrosshairVisibility();
			}
		}

		public void OpenRigEdit(bool p_show_store)
		{
			if ((base.app.inGame && base.app.arguments.game.tryouts) || !DRLBootController.ready || (base.app.view.ui.screens.current != null && base.app.view.ui.screens.current.name == "garage-rig-selection-screen"))
			{
				return;
			}
			bool openedFromBrackets = base.app.view.ui.screens.current != null && base.app.view.ui.screens.current.name == "tournament-brackets-screen";
			UIGarageRigSelectionView uIGarageRigSelectionView = base.app.view.ui.screens.Open<UIGarageRigSelectionView>("garage-rig-selection-screen");
			uIGarageRigSelectionView.screen.title = base.app.model.storage.locale.Get("multiplayer.select-drone-screen.title", "Select your Drone");
			uIGarageRigSelectionView.openStoreOnSelection = p_show_store;
			uIGarageRigSelectionView.allowCustomPhysics = true;
			uIGarageRigSelectionView.SetCreationEnabled(p_flag: false);
			uIGarageRigSelectionView.selectionOnly = true;
			uIGarageRigSelectionView.SetDroneClassEnabled(true);
			uIGarageRigSelectionView.overrideList = null;
			uIGarageRigSelectionView.overrideSizes = null;
			uIGarageRigSelectionView.openedFromBrackets = openedFromBrackets;
			if (base.app.inTournament)
			{
				uIGarageRigSelectionView.allowCustomPhysics = false;
				int droneClass = base.app.arguments.tournament.data.droneClass;
				if (droneClass == 1)
				{
					if (!string.IsNullOrEmpty(base.app.arguments.tournament.data.droneGuid))
					{
						int p_index;
						DroneRigData rigByGUID = base.app.model.storage.state.player.garage.GetRigByGUID(base.app.arguments.tournament.data.droneGuid, out p_index);
						uIGarageRigSelectionView.overrideList = ((rigByGUID != null) ? new List<DroneRigData> { rigByGUID } : null);
					}
					return;
				}
				if (base.app.tournament.drlPilotMode)
				{
					DroneRigData droneRigData = base.app.model.storage.state.player.garage.officialRigs[0];
					uIGarageRigSelectionView.overrideList = ((droneRigData != null) ? new List<DroneRigData> { droneRigData } : null);
					return;
				}
				uIGarageRigSelectionView.overrideList = null;
				if (droneClass == 2)
				{
					uIGarageRigSelectionView.overrideSizes = new List<int>(1) { 0 };
				}
				else if (droneClass > 2)
				{
					uIGarageRigSelectionView.overrideSizes = new List<int>(1) { droneClass };
				}
				else
				{
					uIGarageRigSelectionView.overrideSizes = null;
				}
			}
			else
			{
				if (base.app.model.network.room == null)
				{
					return;
				}
				if (base.app.model.network.room.DroneClass != 100 && base.app.model.network.room.DroneClass != 101)
				{
					uIGarageRigSelectionView.overrideSizes = new List<int>();
					uIGarageRigSelectionView.overrideSizes.Add((base.app.model.network.room.DroneClass < 100) ? base.app.model.network.room.DroneClass : 0);
					uIGarageRigSelectionView.allowCustomPhysics = false;
				}
				uIGarageRigSelectionView.allowCustomPhysics = base.app.model.network.room.GameMode == NetworkRoom.GameType.Freestyle;
				string mapId = base.app.model.network.room.MapId;
				string trackId = base.app.model.network.room.TrackId;
				if (base.app.model.network.room.UsingCustomMap || string.IsNullOrEmpty(mapId))
				{
					return;
				}
				StorageModel storage = base.app.model.storage;
				if (storage == null)
				{
					return;
				}
				GameFlag gameFlag = GameFlag.Freestyle;
				NetworkRoom.GameType gameMode = base.app.model.network.room.GameMode;
				if ((uint)(gameMode - 1) <= 1u)
				{
					gameFlag = GameFlag.Race;
				}
				List<DRLMap> list = ((gameFlag == GameFlag.Freestyle) ? storage.GetMaps() : storage.GetRaceMaps());
				int num = -1;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].guid == mapId)
					{
						num = i;
						break;
					}
				}
				if (num < 0)
				{
					Debug.LogWarning("SettingsController> UpdateMap - Invalid Map - guid[" + mapId + "]");
					return;
				}
				DRLMap dRLMap = list[num];
				List<DRLMapTrack> mapTracks = storage.GetMapTracks(dRLMap, gameFlag);
				int num2 = -1;
				for (int j = 0; j < mapTracks.Count; j++)
				{
					if (mapTracks[j].guid == trackId)
					{
						num2 = j;
						break;
					}
				}
				if (num2 < 0)
				{
					Debug.LogWarning("UIMultiplayerRoomView> UpdateTrack  - Invalid Track - guid[" + trackId + "]");
					return;
				}
				DRLMapTrack dRLMapTrack = mapTracks[num2];
				if (dRLMapTrack.promoDrones != null && dRLMapTrack.promoDrones.Length != 0)
				{
					if (dRLMapTrack.promoDronesOnly)
					{
						uIGarageRigSelectionView.overrideList = new List<DroneRigData>(dRLMapTrack.promoDrones);
					}
					else
					{
						uIGarageRigSelectionView.promoList = new List<DroneRigData>(dRLMapTrack.promoDrones);
					}
				}
				else if (dRLMap.promoDrones != null && dRLMap.promoDrones.Length != 0)
				{
					if (dRLMap.promoDronesOnly)
					{
						uIGarageRigSelectionView.overrideList = new List<DroneRigData>(dRLMap.promoDrones);
					}
					else
					{
						uIGarageRigSelectionView.promoList = new List<DroneRigData>(dRLMap.promoDrones);
					}
				}
				if (dRLMapTrack.droneSizes != null && dRLMapTrack.droneSizes.Length != 0)
				{
					uIGarageRigSelectionView.overrideSizes = new List<int>(dRLMapTrack.droneSizes);
				}
				else if (dRLMap.droneSizes != null && dRLMap.droneSizes.Length != 0)
				{
					uIGarageRigSelectionView.overrideSizes = new List<int>(dRLMap.droneSizes);
				}
			}
		}

		private void RefreshCrashPenaltyData(DRLCrashPenaltyData p_data)
		{
			damageTier1 = p_data.damageTier1;
			damageTier2 = p_data.damageTier2;
			damageTier3 = p_data.damageTier3;
			speedReduction1 = p_data.speedReduction1;
			speedReduction2 = p_data.speedReduction2;
			speedReduction3 = p_data.speedReduction3;
			lineDeviation1 = p_data.lineDeviation1;
			lineDeviation2 = p_data.lineDeviation2;
			lineDeviation3 = p_data.lineDeviation3;
			Drone.CrashEnergy = p_data.crashEnergy;
			Drone.DamageEnergy = p_data.damageEnergy;
			Drone.CrashEnergyTransferRate = p_data.energyTransferRate;
		}

		public static float[] GetDamagePenalty(float p_damage)
		{
			float[] result = new float[2];
			if (p_damage <= 0f)
			{
				return result;
			}
			if (p_damage <= damageTier1)
			{
				result = new float[2] { speedReduction1, lineDeviation1 };
			}
			else if (p_damage <= damageTier2)
			{
				result = new float[2] { speedReduction2, lineDeviation2 };
			}
			else if (p_damage > damageTier2)
			{
				result = new float[2] { speedReduction3, lineDeviation3 };
			}
			return result;
		}
	}
}
