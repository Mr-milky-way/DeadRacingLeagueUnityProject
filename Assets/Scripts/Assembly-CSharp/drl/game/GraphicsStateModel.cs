using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class GraphicsStateModel : Model<DRLApp>
	{
		public struct HardwareScoreData
		{
			public int processorCount;

			public int systemMemorySize;

			public int graphicsMemorySize;

			public string deviceModel;

			public string deviceName;

			public string graphicsDeviceVendor;

			public string graphicsDeviceName;

			public string os;
		}

		private GraphicsQualityPreset m_cached_preset;

		private float m_cached_preset_score;

		private int m_default_vsync = 1;

		private int m_cached_hw_quality = -100;

		[NonReorderable]
		public List<GraphicsQualityPreset> presets;

		[NonReorderable]
		public List<TextureQualitySettings> textureQualityPresets;

		[NonReorderable]
		public List<ShadowQualitySettings> shadowQualityPresets;

		[NonReorderable]
		public List<AntiAliasingQualitySettings> antiAliasingQualityPresets;

		[NonReorderable]
		public List<DepthOfFieldQualitySettings> depthOfFieldQualityPresets;

		[NonReorderable]
		public List<AmbientOcclusionQualitySettings> ambientOcclusionQualityPresets;

		[NonReorderable]
		public List<PostProcessingQualitySettings> postProcessingQualityPresets;

		[NonReorderable]
		public List<EffectsQualitySettings> effectsQualityPresets;

		[NonReorderable]
		public List<DetailsQualitySettings> detailsQualityPresets;

		[NonReorderable]
		public List<WaterReflectionQualitySettings> waterReflectionQualityPresets;

		public SettingsStateModel parent => AssertParent<SettingsStateModel>("parent");

		public DataFlow data => parent.data;

		public float[] resolution
		{
			get
			{
				float d = Screen.currentResolution.width;
				float d2 = Screen.currentResolution.height;
				bool flag = data.Contains("settings-graphics-resolution-x") && data.Contains("settings-graphics-resolution-y");
				float num = data.Get("settings-graphics-resolution-x", d);
				float num2 = data.Get("settings-graphics-resolution-y", d2);
				float[] array = new float[2] { num, num2 };
				if (!flag)
				{
					Resolution resolutionByHardwareScore = GetResolutionByHardwareScore(GetHardwareScore());
					num = (array[0] = resolutionByHardwareScore.width);
					num2 = (array[1] = resolutionByHardwareScore.height);
					data.Set("settings-graphics-resolution-x", num);
					data.Set("settings-graphics-resolution-y", num2);
					Refresh();
				}
				return array;
			}
			set
			{
				float num = Screen.currentResolution.width;
				float num2 = Screen.currentResolution.height;
				float[] array = ((value != null) ? value : new float[2] { num, num2 });
				data.Set("settings-graphics-resolution-x", array[0]);
				data.Set("settings-graphics-resolution-y", array[1]);
				Refresh();
			}
		}

		public float minimumRenderScale
		{
			get
			{
				float[] array = resolution;
				float num = 720f;
				float num2 = array[1];
				return Mathf.Round(((num2 <= 0f) ? 1f : (num / num2)) * 10f) / 10f;
			}
		}

		public float renderScale
		{
			get
			{
				return Mathf.Max(minimumRenderScale, data.Get("settings-graphics-render-scale", 1f));
			}
			set
			{
				data.Set("settings-graphics-render-scale", value);
				Refresh();
			}
		}

		public float[] renderScaleResolution
		{
			get
			{
				float[] array = resolution;
				float num = renderScale;
				if (num >= 1f)
				{
					return array;
				}
				array[0] *= num;
				array[1] *= num;
				return array;
			}
		}

		public bool fullscreen
		{
			get
			{
				return data.Get("settings-graphics-fullscreen", d: true);
			}
			set
			{
				data.Set("settings-graphics-fullscreen", value);
				Refresh();
			}
		}

		public int vsync
		{
			get
			{
				return data.Get("settings-graphics-vsync", m_default_vsync);
			}
			set
			{
				data.Set("settings-graphics-vsync", value);
				Refresh();
			}
		}

		public int fpsLimit
		{
			get
			{
				return data.Get("settings-graphics-fps-limit", m_default_fps);
			}
			set
			{
				data.Set("settings-graphics-fps-limit", value);
				Refresh();
			}
		}

		private int m_default_fps => Screen.currentResolution.refreshRate;

		public int targetScreen
		{
			get
			{
				return data.Get("settings-graphics-mode", 1);
			}
			set
			{
				data.Set("settings-graphics-mode", value);
				Refresh();
			}
		}

		public bool exclusiveMode
		{
			get
			{
				return data.Get("settings-graphics-exclusive-mode", d: false);
			}
			set
			{
				data.Set("settings-graphics-exclusive-mode", value);
				Refresh();
			}
		}

		public float brightness
		{
			get
			{
				return data.Get("settings-graphics-brightness", 0f);
			}
			set
			{
				data.Set("settings-graphics-brightness", value);
				Refresh();
			}
		}

		public bool advancedRendering
		{
			get
			{
				return data.Get("settings-graphics-advanced-rendering", GetPresetByHardware().advancedRendering);
			}
			set
			{
				data.Set("settings-graphics-advanced-rendering", value);
				Refresh();
			}
		}

		public int quality
		{
			get
			{
				if (m_cached_hw_quality <= -100)
				{
					m_cached_hw_quality = GetQualityByHardware();
				}
				return data.Get("settings-graphics-quality", m_cached_hw_quality);
			}
			set
			{
				data.Set("settings-graphics-quality", value);
				Refresh();
			}
		}

		public bool hasQuality => data.Contains("settings-graphics-quality");

		public int texture
		{
			get
			{
				return data.Get("settings-graphics-texture", (int)GetPresetByHardware().texture);
			}
			set
			{
				data.Set("settings-graphics-texture", value);
				Refresh();
			}
		}

		public int antialias
		{
			get
			{
				return data.Get("settings-graphics-antialias", (int)GetPresetByHardware().antialias);
			}
			set
			{
				data.Set("settings-graphics-antialias", value);
				Refresh();
			}
		}

		public int shadow
		{
			get
			{
				return data.Get("settings-graphics-shadows", (int)GetPresetByHardware().shadow);
			}
			set
			{
				data.Set("settings-graphics-shadows", value);
				Refresh();
			}
		}

		public int ambientOcclusion
		{
			get
			{
				return data.Get("settings-graphics-ambient-occlusion", (int)GetPresetByHardware().ambientOcclusion);
			}
			set
			{
				data.Set("settings-graphics-ambient-occlusion", value);
				Refresh();
			}
		}

		public int depthOfField
		{
			get
			{
				int num = 0;
				try
				{
					num = data.Get("settings-graphics-dof", (int)GetPresetByHardware().depthOfField);
				}
				catch
				{
					data.Set("settings-graphics-dof", num);
					Refresh();
				}
				return num;
			}
			set
			{
				data.Set("settings-graphics-dof", value);
				Refresh();
			}
		}

		public int postProcessing
		{
			get
			{
				return data.Get("settings-graphics-post-processing", (int)GetPresetByHardware().postProcessing);
			}
			set
			{
				data.Set("settings-graphics-post-processing", value);
				Refresh();
			}
		}

		public int tier
		{
			get
			{
				return data.Get("settings-graphics-tier", (int)GetPresetByHardware().tier);
			}
			set
			{
				data.Set("settings-graphics-tier", value);
				Refresh();
			}
		}

		public bool motionBlur
		{
			get
			{
				return data.Get("settings-graphics-motion-blur", GetPresetByHardware().motionBlur);
			}
			set
			{
				data.Set("settings-graphics-motion-blur", value);
				Refresh();
			}
		}

		public bool waterReflection
		{
			get
			{
				return data.Get("settings-graphics-water-reflection", GetPresetByHardware().waterReflection);
			}
			set
			{
				data.Set("settings-graphics-water-reflection", value);
				Refresh();
			}
		}

		public int effectsQuality
		{
			get
			{
				return data.Get("settings-graphics-effects-quality", (int)GetPresetByHardware().effectsQuality);
			}
			set
			{
				data.Set("settings-graphics-effects-quality", value);
				Refresh();
			}
		}

		public int details
		{
			get
			{
				return data.Get("settings-graphics-details-quality", (int)GetPresetByHardware().details);
			}
			set
			{
				data.Set("settings-graphics-details-quality", value);
				Refresh();
			}
		}

		public bool eyeAdaptation => postProcessingQualityPresets[postProcessing].eyeAdaptation;

		public bool colorGrading => postProcessingQualityPresets[postProcessing].colorGrading;

		public bool bloom => postProcessingQualityPresets[postProcessing].bloom;

		public bool chromaticAberration => postProcessingQualityPresets[postProcessing].chromaticAberration;

		public bool grain => postProcessingQualityPresets[postProcessing].grain;

		public bool radioFx => postProcessingQualityPresets[postProcessing].radioFx;

		public bool sunShafts => postProcessingQualityPresets[postProcessing].sunShafts;

		public bool screenSpaceReflection => postProcessingQualityPresets[postProcessing].screenSpaceReflection;

		public static HardwareScoreData GetHardwareScoreData(AppSystemInfo p_info = null)
		{
			HardwareScoreData result = default(HardwareScoreData);
			if (p_info == null)
			{
				result.deviceModel = SystemInfo.deviceModel;
				result.deviceName = SystemInfo.deviceName;
				result.processorCount = SystemInfo.processorCount;
				result.systemMemorySize = SystemInfo.systemMemorySize;
				result.graphicsDeviceVendor = SystemInfo.graphicsDeviceVendor;
				result.graphicsDeviceName = SystemInfo.graphicsDeviceName;
				result.graphicsMemorySize = SystemInfo.graphicsMemorySize;
				result.os = OS.prefix;
			}
			else
			{
				result.deviceModel = p_info.deviceModel;
				result.deviceName = p_info.deviceName;
				result.processorCount = p_info.GetProcessorCount();
				result.systemMemorySize = p_info.GetSystemMemorySize();
				result.graphicsMemorySize = p_info.GetGraphicsMemorySize();
				result.graphicsDeviceVendor = p_info.graphicsDeviceVendor;
				result.graphicsDeviceName = p_info.graphicsDeviceName;
				result.os = p_info.GetOperatingSystemPrefix();
			}
			return result;
		}

		private static string GPUVendorNameToShortVendorString(string non_formatted_gpu_vendor_name)
		{
			string result = "null";
			non_formatted_gpu_vendor_name = non_formatted_gpu_vendor_name.ToLower();
			if (non_formatted_gpu_vendor_name.Contains("intel"))
			{
				result = "intel";
			}
			if (non_formatted_gpu_vendor_name.Contains("nvidia"))
			{
				result = "nvidia";
			}
			if (non_formatted_gpu_vendor_name.Contains("amd"))
			{
				result = "amd";
			}
			if (non_formatted_gpu_vendor_name.Contains("ati"))
			{
				result = "ati";
			}
			if (non_formatted_gpu_vendor_name.Contains("radeon"))
			{
				result = "amd";
			}
			return result;
		}

		public static float GetHardwareScore(HardwareScoreData p_data)
		{
			string text = "";
			HardwareScoreData hardwareScoreData = p_data;
			float[] array = new float[4];
			float[] array2 = new float[4] { 0.5f, 0.25f, 0.5f, 2.5f };
			float num = 0f;
			for (int i = 0; i < array2.Length; i++)
			{
				num += array2[i];
			}
			int num2 = 0;
			array[num2] = 1f;
			switch (hardwareScoreData.processorCount)
			{
			case 1:
				array[num2] = 0f;
				break;
			case 2:
				array[num2] = 0.5f;
				break;
			}
			text = text + "cpu-count: " + array[num2].ToString("0.0") + "\n";
			num2++;
			int systemMemorySize = hardwareScoreData.systemMemorySize;
			array[num2] = 1f;
			if (systemMemorySize <= 12800)
			{
				array[num2] = 1f;
			}
			if (systemMemorySize <= 8800)
			{
				array[num2] = 1f;
			}
			if (systemMemorySize <= 4400)
			{
				array[num2] = 0.5f;
			}
			if (systemMemorySize <= 2200)
			{
				array[num2] = 0.2f;
			}
			if (systemMemorySize <= 1100)
			{
				array[num2] = 0f;
			}
			text = text + "system-memory: " + array[num2].ToString("0.0") + "\n";
			num2++;
			int graphicsMemorySize = hardwareScoreData.graphicsMemorySize;
			array[num2] = 1f;
			if (graphicsMemorySize <= 4400)
			{
				array[num2] = 1f;
			}
			if (graphicsMemorySize <= 2200)
			{
				array[num2] = 1f;
			}
			if (graphicsMemorySize <= 1100)
			{
				array[num2] = 0.5f;
			}
			if (graphicsMemorySize <= 550)
			{
				array[num2] = 0f;
			}
			text = text + "graphics-memory: " + array[num2].ToString("0.0") + "\n";
			num2++;
			string gpu_vendor = GPUVendorNameToShortVendorString(hardwareScoreData.graphicsDeviceVendor + " " + hardwareScoreData.graphicsDeviceName);
			array[num2] = GetGPUHardwareScore(gpu_vendor, hardwareScoreData.graphicsDeviceName);
			text = text + "gpu-vendor: " + array[num2].ToString("0.0") + "\n";
			float num3 = 0f;
			int num4 = array.Length;
			for (int j = 0; j < num4; j++)
			{
				num3 += array[j] * array2[j] / num;
			}
			text = text + "final-score: " + num3.ToString("0.0") + "\n";
			if (hardwareScoreData.os == "osx")
			{
				num3 *= 0.9f;
			}
			string text2 = hardwareScoreData.deviceModel.ToLower() + " " + hardwareScoreData.deviceName.ToLower();
			if (text2.Contains("macbook") || text2.Contains("notebook"))
			{
				num3 *= 0.85f;
			}
			return num3;
		}

		public static float GetHardwareScore()
		{
			return GetHardwareScore(GetHardwareScoreData());
		}

		public static bool HasLowSpec()
		{
			return GetHardwareScore() < 0.25f;
		}

		public static float GetGPUHardwareScore(string gpu_vendor, string gpu_name)
		{
			string text = gpu_vendor.ToLower();
			string text2 = gpu_name.ToLower();
			string[] array = new string[20]
			{
				"Titan", "1080", "1070", "2080", "2070", "3090", "3080", "3070", "3060", "3060 Ti",
				"3070 Ti", "3080 Ti", "P5200", "P5000", "M5500", "P4000", "M4000", "M5000", "P3200", "P3000"
			};
			string[] array2 = new string[32]
			{
				"660 Ti", "580", "590", "670", "760", "770", "780", "1050", "1060", "1070",
				"950", "960", "970", "980", "GTX 460", "GTX 560", "GTX 650", "GTX 750", "GTX 470", "GTX 560",
				"GTX 750", "GTX 480", "GTX 570", "GTX 660", "GTX 880", "GTX 680", "M3000", "M2200", "P500", "M2000",
				"K5000", "K4100"
			};
			string[] array3 = new string[17]
			{
				"Vega", "R9", "Fury", "7970", "7950", "5970", "R7 370", "WX 7100", "8970", "6700",
				"6700XT", "6600", "6600XT", "6800", "6800XT", "6900", "6900XT"
			};
			string[] array4 = new string[20]
			{
				"RX", "7850", "6970", "260X", "6950", "5870", "7790", "6870", "5850", "R7 360",
				"R7 260", "R7", "7770", "6850", "7170", "WX 4150", "Pro", "R6", "R5", "FirePro"
			};
			string[] array5 = new string[3] { "Iris Plus", "Iris Pro", "6200" };
			string text3 = "null";
			if (text.Contains("nvidia"))
			{
				if (text2.Contains("rtx 40") || text2.Contains("rtx 50"))
				{
					return 1f;
				}
				for (int i = 0; i < array.Length; i++)
				{
					text3 = array[i].ToLower();
					if (text2.Contains(text3))
					{
						return 1f;
					}
				}
				for (int j = 0; j < array2.Length; j++)
				{
					text3 = array2[j].ToLower();
					if (text2.Contains(text3))
					{
						return 0.5f;
					}
				}
			}
			else if (text.Contains("amd") || text.Contains("ati"))
			{
				for (int k = 0; k < array3.Length; k++)
				{
					text3 = array3[k].ToLower();
					if (text2.Contains(text3))
					{
						return 1f;
					}
				}
				for (int l = 0; l < array4.Length; l++)
				{
					text3 = array4[l].ToLower();
					if (text2.Contains(text3))
					{
						return 0.5f;
					}
				}
			}
			else if (text.Contains("intel"))
			{
				for (int m = 0; m < array5.Length; m++)
				{
					text3 = array5[m].ToLower();
					if (text2.Contains(text3))
					{
						return 0.5f;
					}
				}
			}
			return 0f;
		}

		public static bool IsAdvancedRenderingEnabled(float gpu_score)
		{
			if (gpu_score < 0.5f)
			{
				return false;
			}
			return true;
		}

		public static int GetQualityByHardware(out float p_score, out float p_gpu_score)
		{
			float hardwareScore = GetHardwareScore();
			string gpu_vendor = GPUVendorNameToShortVendorString(SystemInfo.graphicsDeviceVendor);
			p_gpu_score = GetGPUHardwareScore(gpu_vendor, SystemInfo.graphicsDeviceName);
			p_score = hardwareScore;
			int num = Mathf.FloorToInt(100f * hardwareScore);
			if (num <= 40)
			{
				return 0;
			}
			if (num <= 50)
			{
				return 1;
			}
			if (num <= 60)
			{
				return 2;
			}
			if (num <= 70)
			{
				return 3;
			}
			if (num <= 100)
			{
				return 4;
			}
			return 1;
		}

		public static int GetQualityByHardware()
		{
			float p_score = 0f;
			float p_gpu_score = 0f;
			return GetQualityByHardware(out p_score, out p_gpu_score);
		}

		public static float[] FindLowestResolution()
		{
			Resolution resolutionByPixelCount = GetResolutionByPixelCount(921600);
			return new float[2] { resolutionByPixelCount.width, resolutionByPixelCount.height };
		}

		public void InitializeQualityByScore(float p_score)
		{
			bool num = data.Contains("settings-graphics-resolution-x") && data.Contains("settings-graphics-resolution-y");
			bool flag = data.Contains("settings-graphics-quality");
			if (!num)
			{
				Resolution resolutionByHardwareScore = GetResolutionByHardwareScore(p_score);
				resolution = ((p_score <= 0f) ? FindLowestResolution() : new float[2] { resolutionByHardwareScore.width, resolutionByHardwareScore.height });
			}
			if (!flag)
			{
				int fromPreset = (quality = ((!(p_score <= 0f)) ? GetQualityByHardware() : 0));
				SetFromPreset(fromPreset);
			}
			Debug.Log($"GraphicsStateModel> InitializeQualityByScore / Quality is [{quality}]");
			ApplyGraphics();
		}

		public void ApplyGraphics()
		{
			Notify("settings.startup.graphics.apply");
		}

		public static Resolution GetResolutionByHardwareScore(float p_score, out int p_index)
		{
			p_index = 0;
			if (Application.isEditor)
			{
				return Screen.currentResolution;
			}
			List<Resolution> list = new List<Resolution>();
			list.AddRange(Screen.resolutions);
			float a = Screen.currentResolution.width;
			float a2 = Screen.currentResolution.height;
			if (Display.main != null)
			{
				a = Display.main.systemWidth;
				a2 = Display.main.systemHeight;
			}
			float[] array = FindLowestResolution();
			a = Mathf.Max(a, array[0]);
			a2 = Mathf.Max(a2, array[1]);
			float num = a / a2;
			List<Resolution> list2 = new List<Resolution>(list);
			for (int i = 0; i < list2.Count; i++)
			{
				Resolution resolution = list2[i];
				float num2 = resolution.width;
				float num3 = resolution.height;
				if (num2 < 800f)
				{
					list2.RemoveAt(i--);
				}
				else if (num3 < 600f)
				{
					list2.RemoveAt(i--);
				}
				else if (Mathf.Abs(((num3 <= 0f) ? 1f : (num2 / num3)) - num) > 0.05f)
				{
					list2.RemoveAt(i--);
				}
			}
			if (list2.Count <= 0)
			{
				list2 = new List<Resolution>(list);
			}
			list = list2;
			int num4 = (p_index = Mathf.FloorToInt((float)(list.Count - 1) * p_score));
			if (num4 < 0)
			{
				return Screen.currentResolution;
			}
			if (num4 >= list.Count)
			{
				return Screen.currentResolution;
			}
			return list[num4];
		}

		public static Resolution GetResolutionByHardwareScore(float p_score)
		{
			int p_index = 0;
			return GetResolutionByHardwareScore(Mathf.Pow(Mathf.Clamp01(p_score), 1.35f), out p_index);
		}

		public static Resolution GetResolutionByPixelCount(int p_pixel_count)
		{
			if (p_pixel_count <= 0)
			{
				return Screen.currentResolution;
			}
			List<Resolution> list = new List<Resolution>();
			list.AddRange(Screen.resolutions);
			Display main = Display.main;
			float num = main?.systemWidth ?? Screen.currentResolution.width;
			float num2 = main?.systemHeight ?? Screen.currentResolution.height;
			float aspect = ((Mathf.Abs(num2) <= 0.01f) ? 1.7777778f : (num / num2));
			list.RemoveAll((Resolution it) => Mathf.Abs((float)(it.width / it.height) - aspect) >= 0.1f);
			if (list.Count <= 0)
			{
				list.AddRange(Screen.resolutions);
			}
			list.Sort(delegate(Resolution a, Resolution b)
			{
				int num3 = a.width * a.height;
				int num4 = b.width * b.height;
				int num5 = Mathf.Abs(num3 - p_pixel_count);
				int num6 = Mathf.Abs(num4 - p_pixel_count);
				return (num5 >= num6) ? 1 : (-1);
			});
			if (list.Count > 0)
			{
				return list[0];
			}
			return Screen.currentResolution;
		}

		public GraphicsQualityPreset GetPresetByHardware(out float p_score)
		{
			p_score = m_cached_preset_score;
			if (m_cached_preset != null)
			{
				return m_cached_preset;
			}
			float p_gpu_score = 0f;
			int qualityByHardware = GetQualityByHardware(out p_score, out p_gpu_score);
			qualityByHardware = Mathf.Clamp(qualityByHardware, 0, presets.Count - 1);
			m_cached_preset_score = p_score;
			return m_cached_preset = presets[qualityByHardware];
		}

		public void SetFromPreset(int p_index)
		{
			Debug.Log($"GraphicsStateModel> SetFromPreset / index[{p_index}]");
			p_index = Mathf.Clamp(p_index, 0, presets.Count - 1);
			if (p_index < 0)
			{
				Debug.Log("GraphicsStateModel> SetFromPreset / Failed, no preset found!");
				return;
			}
			GraphicsQualityPreset graphicsQualityPreset = presets[p_index];
			texture = (int)graphicsQualityPreset.texture;
			ambientOcclusion = (int)graphicsQualityPreset.ambientOcclusion;
			antialias = (int)graphicsQualityPreset.antialias;
			depthOfField = (int)graphicsQualityPreset.depthOfField;
			shadow = (int)graphicsQualityPreset.shadow;
			postProcessing = (int)graphicsQualityPreset.postProcessing;
			tier = (int)graphicsQualityPreset.tier;
			effectsQuality = (int)graphicsQualityPreset.effectsQuality;
			details = (int)graphicsQualityPreset.details;
			motionBlur = graphicsQualityPreset.motionBlur;
			advancedRendering = graphicsQualityPreset.advancedRendering;
			waterReflection = graphicsQualityPreset.waterReflection;
			Debug.Log($"GraphicsStateModel> SetFromPreset / name[{base.name}] id[{graphicsQualityPreset.id}] label[{graphicsQualityPreset.label}] index[{p_index}] texture[{graphicsQualityPreset.texture}] adv-render[{graphicsQualityPreset.advancedRendering}] water-rfl[{graphicsQualityPreset.waterReflection}]");
		}

		public GraphicsQualityPreset GetPresetByHardware()
		{
			float p_score = 0f;
			return GetPresetByHardware(out p_score);
		}

		public GraphicsQualityPreset GetPresetByIndex(int p_index)
		{
			int count = presets.Count;
			if (count > 0)
			{
				if (p_index >= 0)
				{
					if (p_index < count)
					{
						return presets[p_index];
					}
					return presets[count - 1];
				}
				return presets[0];
			}
			return null;
		}

		public void Refresh()
		{
			if ((bool)parent)
			{
				parent.Refresh();
			}
		}

		public string GetSystemFieldId(string p_field)
		{
			string text = OS.prefix + "_" + SystemInfo.graphicsDeviceID;
			return p_field + "@" + text;
		}
	}
}
