using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace drl.game
{
	[Serializable]
	public class AppSystemInfo
	{
		[OptionalField]
		public string version;

		[OptionalField]
		public string operatingSystem;

		[OptionalField]
		public string processorType;

		[OptionalField]
		public string processorFrequency;

		[OptionalField]
		public string processorCount;

		[OptionalField]
		public string deviceModel;

		[OptionalField]
		public string deviceName;

		[OptionalField]
		public string deviceType;

		[OptionalField]
		public string graphicsDeviceID;

		[OptionalField]
		public string graphicsDeviceName;

		[OptionalField]
		public string graphicsDeviceType;

		[OptionalField]
		public string graphicsDeviceVersion;

		[OptionalField]
		public string graphicsDeviceVendor;

		[OptionalField]
		public string graphicsDeviceVendorID;

		[OptionalField]
		public string systemMemorySize;

		[OptionalField]
		public string graphicsMemorySize;

		[OptionalField]
		public string graphicsMultiThreaded;

		[OptionalField]
		public string graphicsShaderLevel;

		[OptionalField]
		public string maxTextureSize;

		[OptionalField]
		public string npotSupport;

		[OptionalField]
		public string supportedRenderTargetCount;

		[OptionalField]
		public string copyTextureSupport;

		[OptionalField]
		public string supports3DTextures;

		[OptionalField]
		public string supportsImageEffects;

		[OptionalField]
		public string supportsShadows;

		[OptionalField]
		public string currentResolutionWidth;

		[OptionalField]
		public string currentResolutionHeight;

		[OptionalField]
		public string quality;

		[OptionalField]
		public string hardwareScore;

		[OptionalField]
		public string supportSparseTexture;

		[OptionalField]
		public string displayCount;

		[OptionalField]
		public string displayResolutions;

		public int GetProcessorCount()
		{
			int result = 1;
			int.TryParse(processorCount, out result);
			return result;
		}

		public int GetSystemMemorySize()
		{
			int result = 0;
			int.TryParse(systemMemorySize, out result);
			return result;
		}

		public int GetGraphicsMemorySize()
		{
			int result = 0;
			int.TryParse(graphicsMemorySize, out result);
			return result;
		}

		public string GetOperatingSystemPrefix()
		{
			if (Application.platform == RuntimePlatform.XboxOne)
			{
				return "xbox";
			}
			if (Application.platform == RuntimePlatform.PS4)
			{
				return "ps4";
			}
			if (operatingSystem.ToLower().Contains("windows"))
			{
				return "win";
			}
			return "osx";
		}
	}
}
