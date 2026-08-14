using System;

namespace UnityEngine.PostProcessing
{
	[Serializable]
	public class FogModel : PostProcessingModel
	{
		[Serializable]
		public struct Settings
		{
			[Tooltip("Should the fog affect the skybox?")]
			public bool excludeSkybox;

			[Range(0f, 1f)]
			[Tooltip("Texture blending factor.")]
			public float textureBlend;

			[Tooltip("Replace fog solid color with a texture.")]
			public Texture texture;

			public static Settings defaultSettings => new Settings
			{
				excludeSkybox = true,
				textureBlend = 1f,
				texture = null
			};
		}

		[SerializeField]
		private Settings m_Settings = Settings.defaultSettings;

		public Settings settings
		{
			get
			{
				return m_Settings;
			}
			set
			{
				m_Settings = value;
			}
		}

		public override void Reset()
		{
			m_Settings = Settings.defaultSettings;
		}
	}
}
