using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISettingsSystemController : Controller<DRLApp>
	{
		protected bool m_ignore_quality;

		protected float m_brightness_value;

		private Activity m_notify_graphics_timer;

		public UISettingsSystemView view => AssertLocal<UISettingsSystemView>("view");

		public StateModel model => base.app.model.storage.state;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					view.RefreshStates();
				}
				break;
			case "settings.system.form.event@change":
				OnFormNotification(p_target, p_is_change: true);
				break;
			case "settings.system.form.event@click":
				OnFormNotification(p_target, p_is_change: false);
				break;
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				this.TimerRunOnce(delegate
				{
					if (base.validContext)
					{
						view.EnableDiscardButton(p_enable: false);
					}
				}, 0.2f);
				break;
			case "ui.screen.nav-right@click":
			{
				Debug.Log("UISettingsSystemController> Apply");
				GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
				base.app.view.audio.PlayUIGenericSuccess();
				ApplyBrightness();
				graphics.vsync = view.vsync;
				int refreshRate = Screen.currentResolution.refreshRate;
				graphics.fpsLimit = refreshRate;
				view.RefreshVSyncFPS();
				view.RefreshRenderScale();
				int index = view.graphicsModeStepper.index;
				if (index == graphics.quality)
				{
					Notify("ui.screen.return@click");
					view.EnableDiscardButton(p_enable: false);
					break;
				}
				view.EnableDiscardButton();
				SetQuality(index);
				graphics.quality = index;
				ApplyResolutionByQuality(index);
				NotifyGraphicsApply();
				Notify("ui.screen.return@click");
				break;
			}
			}
		}

		protected void OnFormNotification(Object p_target, bool p_is_change)
		{
			if (view.notificationLock)
			{
				return;
			}
			bool flag = p_is_change;
			string text = p_target.name;
			GraphicsStateModel graphics = model.player.settings.graphics;
			if (text == null)
			{
				return;
			}
			switch (text)
			{
			case "graphics-fullscreen":
			case "graphics-resolution":
				if (flag)
				{
					bool fullscreen = view.fullscreen;
					float[] graphicsResolution = view.graphicsResolution;
					graphics.resolution = graphicsResolution;
					graphics.fullscreen = fullscreen;
					view.RefreshRenderScale();
					NotifyGraphicsApply();
				}
				break;
			case "graphics-advanced":
				graphics.advancedRendering = view.advancedRendering;
				SetCustomQuality();
				NotifyGraphicsApply();
				break;
			case "graphics-vsync":
			{
				graphics.vsync = view.vsync;
				int refreshRate = Screen.currentResolution.refreshRate;
				graphics.fpsLimit = refreshRate;
				view.RefreshVSyncFPS();
				NotifyGraphicsApply();
				break;
			}
			case "graphics-fps-limit":
				graphics.fpsLimit = view.fpsLimit;
				NotifyGraphicsApply();
				break;
			case "graphics-mode":
				RefreshFullscreenMode();
				NotifyGraphicsApply();
				break;
			case "graphics-quality":
			{
				int tier = view.quality;
				SetQuality(tier);
				graphics.quality = tier;
				ApplyResolutionByQuality(tier);
				NotifyGraphicsApply();
				break;
			}
			case "quality-detect":
			{
				int p_quality = ApplyQualityByHardware();
				ApplyResolutionByQuality(p_quality);
				NotifyGraphicsApply();
				break;
			}
			case "graphics-texture":
			{
				int tier = view.texture;
				graphics.texture = tier;
				SetCustomQuality();
				NotifyGraphicsApply();
				break;
			}
			case "graphics-antialias":
			{
				int tier = view.antialias;
				graphics.antialias = tier;
				SetCustomQuality();
				NotifyGraphicsApply();
				break;
			}
			case "graphics-shadow":
			{
				int tier = view.shadow;
				graphics.shadow = tier;
				SetCustomQuality();
				NotifyGraphicsApply();
				break;
			}
			case "graphics-ambient-occlusion":
			{
				int tier = view.ambientOcclusion;
				graphics.ambientOcclusion = tier;
				SetCustomQuality();
				NotifyGraphicsApply();
				break;
			}
			case "graphics-dof":
			{
				int tier = view.depthOfField;
				graphics.depthOfField = tier;
				SetCustomQuality();
				NotifyGraphicsApply();
				break;
			}
			case "graphics-motion-blur":
			{
				bool waterReflection = view.motionBlur;
				graphics.motionBlur = waterReflection;
				SetCustomQuality();
				NotifyGraphicsApply();
				break;
			}
			case "graphics-water-reflection":
			{
				bool waterReflection = view.waterReflection;
				graphics.waterReflection = waterReflection;
				SetCustomQuality();
				NotifyGraphicsApply();
				break;
			}
			case "graphics-post-processing":
			{
				int tier = view.postProcessing;
				graphics.postProcessing = tier;
				SetCustomQuality();
				NotifyGraphicsApply();
				break;
			}
			case "graphics-effects-quality":
			{
				int tier = view.effectsQuality;
				graphics.effectsQuality = tier;
				SetCustomQuality();
				NotifyGraphicsApply();
				break;
			}
			case "graphics-details":
			{
				int tier = view.details;
				graphics.details = tier;
				SetCustomQuality();
				NotifyGraphicsApply();
				break;
			}
			case "graphics-tier":
			{
				int tier = view.tier;
				graphics.tier = tier;
				SetCustomQuality();
				NotifyGraphicsApply();
				view.RefreshShaderWarning();
				break;
			}
			case "graphics-render-scale":
			{
				float renderScale = view.renderScale;
				graphics.renderScale = renderScale;
				NotifyGraphicsApply();
				break;
			}
			case "graphics-console-mode":
				if (base.app.model.storage.state.player.settings.graphics.quality != view.graphicsModeStepper.index)
				{
					view.EnableDiscardButton();
				}
				view.SetPresetConsole();
				break;
			case "brightness-slider":
				if (base.app.model.storage.state.player.settings.graphics.brightness != view.brightnessSlider.value)
				{
					view.EnableDiscardButton();
				}
				SetBrightness(view.brightnessSlider.value);
				break;
			case "discard":
			{
				GraphicsStateModel graphics2 = base.app.model.storage.state.player.settings.graphics;
				SetBrightness(graphics2.brightness);
				view.vsync = graphics2.vsync;
				Notify("ui.screen.return@click");
				break;
			}
			}
		}

		private void SetBrightness(float p_value)
		{
			p_value = Mathf.Round(p_value * 10f) / 10f;
			base.app.brightness.exposure = p_value;
			m_brightness_value = p_value;
		}

		public int ApplyQualityByHardware()
		{
			int num = -1;
			float p_score = 0f;
			float p_gpu_score = 0f;
			GraphicsStateModel graphics = model.player.settings.graphics;
			num = GraphicsStateModel.GetQualityByHardware(out p_score, out p_gpu_score);
			view.quality = num;
			SetQuality(num);
			graphics.quality = num;
			if (p_score <= 0.5f)
			{
				view.targetScreen = view.screenStepper.max;
				RefreshFullscreenMode();
			}
			int refreshRate = GraphicsStateModel.GetLowLatencyFrameLimit();
			graphics.vsync = 0;
			graphics.fpsLimit = refreshRate;
			view.vsync = 0;
			view.fpsLimit = graphics.fpsLimit;
			return num;
		}

		public void ApplyResolutionByQuality(int p_quality = -1)
		{
			GraphicsStateModel graphics = model.player.settings.graphics;
			float p_score = 0f;
			float p_gpu_score = 0f;
			int num = p_quality;
			if (num < 0)
			{
				num = GraphicsStateModel.GetQualityByHardware(out p_score, out p_gpu_score);
			}
			if (num < 0)
			{
				num = 0;
			}
			int num2 = (new int[5] { 921600, 921600, 921600, 1440000, 2073600 })[num];
			Resolution resolutionByPixelCount = GraphicsStateModel.GetResolutionByPixelCount(num2);
			Debug.Log($"UISettingsController> ApplyResolutionByQuality / param[{p_quality}] applied[{num}] resolution[{resolutionByPixelCount.width},{resolutionByPixelCount.height}] pixel-count[{num2}]");
			view.graphicsResolution = new float[2] { resolutionByPixelCount.width, resolutionByPixelCount.height };
			bool fullscreen = view.fullscreen;
			float[] graphicsResolution = view.graphicsResolution;
			graphics.resolution = graphicsResolution;
			graphics.fullscreen = fullscreen;
			resolutionByPixelCount = GraphicsStateModel.GetResolutionByPixelCount((new int[5] { 921600, 921600, 921600, 1440000, 2073600 })[num]);
			float num3 = graphics.resolution[1];
			float num4 = resolutionByPixelCount.height;
			float num5 = Mathf.Clamp(num4 / num3, 0.3f, 1f);
			Debug.Log($"UISettingsController> ApplyResolutionByQuality / current-height[{num3}] target-height[{num4}] render-scale[{num5}]");
			graphics.renderScale = num5;
			view.renderScale = num5;
			view.RefreshRenderScale();
		}

		public void SetQuality(int p_quality)
		{
			if (p_quality >= 0)
			{
				GraphicsQualityPreset preset = model.player.settings.graphics.presets[p_quality];
				m_ignore_quality = true;
				view.SetPreset(preset);
				GraphicsStateModel graphics = model.player.settings.graphics;
				graphics.advancedRendering = view.advancedRendering;
				graphics.tier = view.tier;
				graphics.texture = view.texture;
				graphics.antialias = view.antialias;
				graphics.shadow = view.shadow;
				graphics.ambientOcclusion = view.ambientOcclusion;
				graphics.depthOfField = view.depthOfField;
				graphics.motionBlur = view.motionBlur;
				graphics.waterReflection = view.waterReflection;
				graphics.postProcessing = view.postProcessing;
				graphics.effectsQuality = view.effectsQuality;
				graphics.details = view.details;
				view.RefreshShaderWarning();
				view.RefreshRenderScale();
				m_ignore_quality = false;
			}
		}

		private void ApplyBrightness()
		{
			model.player.settings.graphics.brightness = ((view.brightnessSlider != null) ? view.brightnessSlider.value : 0f);
		}

		public void SetCustomQuality()
		{
			if (!m_ignore_quality)
			{
				GraphicsStateModel graphics = model.player.settings.graphics;
				view.notificationLock = true;
				view.quality = -1;
				graphics.quality = -1;
				view.notificationLock = false;
			}
		}

		public void RefreshFullscreenMode()
		{
			GraphicsStateModel graphics = model.player.settings.graphics;
			bool fullscreen = view.fullscreen;
			graphics.targetScreen = view.targetScreen;
			graphics.exclusiveMode = view.exclusiveMode;
			graphics.fullscreen = fullscreen;
		}

		protected void NotifyGraphicsApply()
		{
			if (m_notify_graphics_timer != null)
			{
				m_notify_graphics_timer.Stop();
			}
			m_notify_graphics_timer = Activity.RunOnce(delegate
			{
				Notify("settings.system.screen.apply");
			}, 1f);
		}
	}
}
