using UnityEngine;
using UnityEngine.Audio;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class AudioStateModel : Model<DRLApp>
	{
		public AudioMixer masterAudioMixer;

		public SettingsStateModel parent => AssertParent<SettingsStateModel>("parent");

		public DataFlow data => parent.data;

		public float volumeMain
		{
			get
			{
				return data.Get("settings-audio-volume-main", GetVolumeMain());
			}
			set
			{
				data.Set("settings-audio-volume-main", Mathf.Clamp(value, 0.0001f, 1f));
				masterAudioMixer.SetFloat("MasterVolume", 20f * Mathf.Log10(value + 0.0001f));
				PlayerPrefs.SetFloat("VolumeMain", Mathf.Clamp(value, 0.0001f, 1f));
				Refresh();
			}
		}

		public float volumeMusic
		{
			get
			{
				return data.Get("settings-audio-volume-music", GetVolumeMusic());
			}
			set
			{
				data.Set("settings-audio-volume-music", Mathf.Clamp(value, 0.0001f, 1f));
				masterAudioMixer.SetFloat("MusicVolume", 20f * Mathf.Log10(value + 0.0001f));
				PlayerPrefs.SetFloat("VolumeMusic", Mathf.Clamp(value, 0.0001f, 1f));
				Refresh();
			}
		}

		public float volumeSFX
		{
			get
			{
				return data.Get("settings-audio-volume-sfx", GetVolumeSFX());
			}
			set
			{
				data.Set("settings-audio-volume-sfx", Mathf.Clamp(value, 0.0001f, 1f));
				masterAudioMixer.SetFloat("EffectsVolume", 20f * Mathf.Log10(value + 0.0001f));
				PlayerPrefs.SetFloat("VolumeSFX", Mathf.Clamp(value, 0.0001f, 1f));
				Refresh();
			}
		}

		public bool audioUIEnabled
		{
			get
			{
				return data.Get("settings-audio-ui-enabled", d: true);
			}
			set
			{
				data.Set("settings-audio-ui-enabled", value);
				Refresh();
			}
		}

		public bool audioMotorEnabled
		{
			get
			{
				return data.Get("settings-audio-motor-enabled", d: true);
			}
			set
			{
				data.Set("settings-audio-motor-enabled", value);
				Refresh();
			}
		}

		private float GetVolumeMain()
		{
			if (PlayerPrefs.HasKey("VolumeMain"))
			{
				return PlayerPrefs.GetFloat("VolumeMain");
			}
			return 1f;
		}

		private float GetVolumeMusic()
		{
			if (PlayerPrefs.HasKey("VolumeMusic"))
			{
				return PlayerPrefs.GetFloat("VolumeMusic");
			}
			return 0.2f;
		}

		private float GetVolumeSFX()
		{
			if (PlayerPrefs.HasKey("VolumeSFX"))
			{
				return PlayerPrefs.GetFloat("VolumeSFX");
			}
			return 0.2f;
		}

		public void Refresh()
		{
			if ((bool)parent)
			{
				parent.Refresh();
			}
		}

		private void Start()
		{
			volumeMain = volumeMain;
			volumeMusic = volumeMusic;
			volumeSFX = volumeSFX;
		}
	}
}
