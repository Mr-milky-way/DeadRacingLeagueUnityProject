using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class UISettingsSystemView : UIScreenView
	{
		public DRLResolutionDropdownView graphicsResolutionStepper;

		public DRLStepperView vsyncStepper;

		public DRLStepperView screenStepper;

		public DRLToggleView advancedRenderingToggle;

		public DRLIntStepperView qualityStepper;

		public DRLStepperView textureStepper;

		public DRLStepperView antialiasStepper;

		public DRLStepperView shadowStepper;

		public DRLStepperView tierStepper;

		public DRLStepperView ambientOcclusionStepper;

		public DRLStepperView depthOfFieldStepper;

		public DRLStepperView postProcessingStepper;

		public DRLStepperView effectsQualityStepper;

		public DRLStepperView detailsStepper;

		public DRLToggleView motionBlurToggle;

		public DRLToggleView waterReflectionToggle;

		public DRLSliderView fpsLimitSlider;

		public FadeComponent fpsLimitSliderFade;

		public DRLSliderView brightnessSlider;

		public RectTransform shaderQualityWarningRT;

		public DRLSliderView renderScaleSlider;

		public FadeComponent renderScaleSliderFade;

		public bool notificationLock;

		[Header("Consoles")]
		public DRLStepperView graphicsModeStepper;

		public GameObject resolutionModeContainer;

		public GameObject effectsModeContainer;

		public GameObject framerateModeContainer;

		public GameObject graphicsModeContainer;

		public GameObject backButton;

		public GameObject discardButton;

		private static int m_shader_quality_state = -1;

		public UINavigation fpsLimitSliderNav => fpsLimitSlider.GetComponent<UINavigation>();

		public UINavigation renderScaleSliderNav => renderScaleSlider.GetComponent<UINavigation>();

		public float[] graphicsResolution
		{
			get
			{
				Vector2 resolution = graphicsResolutionStepper.GetResolution();
				return new float[2] { resolution.x, resolution.y };
			}
			set
			{
				float p_default = Screen.currentResolution.width;
				float p_default2 = Screen.currentResolution.height;
				float x = Reflection<object>.Get(value, 0, p_default);
				float y = Reflection<object>.Get(value, 1, p_default2);
				Vector2 closestResolution = new Vector2(x, y);
				graphicsResolutionStepper.SetClosestResolution(closestResolution);
			}
		}

		public bool fullscreen
		{
			get
			{
				return screenStepper.index > 0;
			}
			set
			{
			}
		}

		public int vsync
		{
			get
			{
				return vsyncStepper.index;
			}
			set
			{
				vsyncStepper.index = value;
				vsyncStepper.Refresh();
			}
		}

		public int targetScreen
		{
			get
			{
				return screenStepper.index;
			}
			set
			{
				screenStepper.index = value;
				screenStepper.Refresh();
			}
		}

		public bool exclusiveMode => screenStepper.index >= screenStepper.max;

		public bool advancedRendering
		{
			get
			{
				return advancedRenderingToggle.toggle.isOn;
			}
			set
			{
				advancedRenderingToggle.toggle.isOn = value;
			}
		}

		public int quality
		{
			get
			{
				return qualityStepper.value;
			}
			set
			{
				qualityStepper.index = qualityStepper.values.IndexOf(value);
				qualityStepper.Refresh();
			}
		}

		public int texture
		{
			get
			{
				return textureStepper.index;
			}
			set
			{
				textureStepper.index = value;
				textureStepper.Refresh();
			}
		}

		public int antialias
		{
			get
			{
				return antialiasStepper.index;
			}
			set
			{
				antialiasStepper.index = value;
				antialiasStepper.Refresh();
			}
		}

		public int shadow
		{
			get
			{
				return shadowStepper.index;
			}
			set
			{
				shadowStepper.index = value;
				shadowStepper.Refresh();
			}
		}

		public int ambientOcclusion
		{
			get
			{
				return ambientOcclusionStepper.index;
			}
			set
			{
				ambientOcclusionStepper.index = value;
				ambientOcclusionStepper.Refresh();
			}
		}

		public int depthOfField
		{
			get
			{
				return depthOfFieldStepper.index;
			}
			set
			{
				depthOfFieldStepper.index = value;
				depthOfFieldStepper.Refresh();
			}
		}

		public int postProcessing
		{
			get
			{
				return postProcessingStepper.index;
			}
			set
			{
				postProcessingStepper.index = value;
				postProcessingStepper.Refresh();
			}
		}

		public int effectsQuality
		{
			get
			{
				return effectsQualityStepper.index;
			}
			set
			{
				effectsQualityStepper.index = value;
				effectsQualityStepper.Refresh();
			}
		}

		public int details
		{
			get
			{
				return detailsStepper.index;
			}
			set
			{
				detailsStepper.index = value;
				detailsStepper.Refresh();
			}
		}

		public bool motionBlur
		{
			get
			{
				return motionBlurToggle.toggle.isOn;
			}
			set
			{
				motionBlurToggle.toggle.isOn = value;
			}
		}

		public bool waterReflection
		{
			get
			{
				return waterReflectionToggle.toggle.isOn;
			}
			set
			{
				waterReflectionToggle.toggle.isOn = value;
			}
		}

		public int tier
		{
			get
			{
				return tierStepper.index;
			}
			set
			{
				tierStepper.index = value;
				tierStepper.Refresh();
			}
		}

		public int fpsLimit
		{
			get
			{
				return (int)fpsLimitSlider.value;
			}
			set
			{
				fpsLimitSlider.value = value;
			}
		}

		public float renderScale
		{
			get
			{
				return (renderScaleSlider ? renderScaleSlider.value : 100f) / 100f;
			}
			set
			{
				if ((bool)renderScaleSlider)
				{
					renderScaleSlider.value = value * 100f;
				}
			}
		}

		public void SetPreset(GraphicsQualityPreset p_preset)
		{
			advancedRendering = p_preset.advancedRendering;
			tier = (int)p_preset.tier;
			texture = (int)p_preset.texture;
			shadow = (int)p_preset.shadow;
			antialias = (int)p_preset.antialias;
			depthOfField = (int)p_preset.depthOfField;
			ambientOcclusion = (int)p_preset.ambientOcclusion;
			postProcessing = (int)p_preset.postProcessing;
			effectsQuality = (int)p_preset.effectsQuality;
			details = (int)p_preset.details;
			motionBlur = p_preset.motionBlur;
			waterReflection = p_preset.waterReflection;
		}

		public void SetPresetConsole()
		{
			if (!(graphicsModeStepper == null))
			{
				int index = graphicsModeStepper.index;
				resolutionModeContainer.SetActive(value: false);
				effectsModeContainer.SetActive(value: false);
				framerateModeContainer.SetActive(value: false);
				switch (index)
				{
				case 0:
					effectsModeContainer.SetActive(value: true);
					break;
				case 1:
					resolutionModeContainer.SetActive(value: true);
					break;
				case 2:
					framerateModeContainer.SetActive(value: true);
					break;
				default:
					effectsModeContainer.SetActive(value: true);
					break;
				}
			}
		}

		public void RefreshStates()
		{
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			notificationLock = true;
			if (base.app.model.storage.state.player.profile.limitFPS)
			{
				vsyncStepper.interactable = false;
				fpsLimitSlider.interactable = false;
				fpsLimitSlider.enabled = false;
			}
			graphicsResolution = graphics.resolution;
			vsync = graphics.vsync;
			int num = Display.displays.Length;
			num = 1;
			bool flag = true;
			int num2 = num + ((!flag) ? 1 : 2);
			if (screenStepper.labels.Length != num2)
			{
				List<string> list = new List<string>();
				list.Add("WINDOW");
				for (int i = 0; i < num; i++)
				{
					list.Add("FULL SCREEN" + ((num <= 1) ? "" : (" " + (i + 1))));
				}
				if (flag)
				{
					list.Add("EXCLUSIVE");
				}
				screenStepper.labels = list.ToArray();
				screenStepper.max = screenStepper.labels.Length - 1;
			}
			int num3 = ((!flag) ? 1 : 2);
			targetScreen = (graphics.fullscreen ? ((!graphics.exclusiveMode) ? 1 : num3) : 0);
			advancedRendering = graphics.advancedRendering;
			quality = graphics.quality;
			texture = graphics.texture;
			antialias = graphics.antialias;
			shadow = graphics.shadow;
			ambientOcclusion = graphics.ambientOcclusion;
			depthOfField = graphics.depthOfField;
			waterReflection = graphics.waterReflection;
			motionBlur = graphics.motionBlur;
			postProcessing = graphics.postProcessing;
			effectsQuality = graphics.effectsQuality;
			details = graphics.details;
			tier = graphics.tier;
			renderScale = graphics.renderScale;
			RefreshVSyncFPS();
			RefreshRenderScale();
			RefreshShaderWarning();
			notificationLock = false;
		}

		public void RefreshVSyncFPS(bool p_force_fps_limit = false)
		{
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			int num = Mathf.Max(60, Screen.currentResolution.refreshRate);
			int num2 = Mathf.Max(200, (num + 60) / 10 * 10);
			fpsLimitSlider.slider.minValue = 60f;
			fpsLimitSlider.slider.maxValue = num2;
			fpsLimit = ((!p_force_fps_limit) ? graphics.fpsLimit : ((vsync <= 0) ? ((int)fpsLimitSlider.slider.maxValue) : ((int)fpsLimitSlider.slider.minValue)));
			SetFPSLimitEnabled(vsync <= 0);
		}

		public void RefreshRenderScale()
		{
			if ((bool)renderScaleSlider)
			{
				GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
				if (renderScale < graphics.minimumRenderScale)
				{
					renderScale = graphics.minimumRenderScale;
				}
				int num = Mathf.FloorToInt(graphics.minimumRenderScale * 100f);
				bool flag = (float)num < renderScaleSlider.slider.maxValue;
				SetRenderScaleEnabled(flag);
				if (flag)
				{
					renderScaleSlider.slider.minValue = num;
				}
			}
		}

		public void SetRenderScaleEnabled(bool p_flag)
		{
			if ((bool)renderScaleSlider)
			{
				renderScaleSliderFade.allowMouseInput = p_flag;
				renderScaleSliderFade.Fade(p_flag ? 1f : 0.2f);
				renderScaleSliderNav.enabled = p_flag;
			}
		}

		public void SetFPSLimitEnabled(bool p_flag)
		{
			bool limitFPS = base.app.model.storage.state.player.profile.limitFPS;
			fpsLimitSliderFade.allowMouseInput = vsync <= 0;
			fpsLimitSliderFade.Fade((vsync <= 0 && !limitFPS) ? 1f : 0.2f);
			fpsLimitSliderNav.enabled = vsync <= 0;
		}

		public void EnableDiscardButton(bool p_enable = true)
		{
		}

		public void RefreshShaderWarning()
		{
			GraphicsStateModel graphics = base.app.model.storage.state.player.settings.graphics;
			if (m_shader_quality_state < 0)
			{
				m_shader_quality_state = graphics.tier;
			}
			if ((bool)shaderQualityWarningRT)
			{
				shaderQualityWarningRT.gameObject.SetActive(m_shader_quality_state != graphics.tier);
			}
		}
	}
}
