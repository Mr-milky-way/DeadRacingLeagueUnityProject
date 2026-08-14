using System;
using UnityEngine;

namespace thelab.core
{
	public class AudioComponent<T> : AudioComponent
	{
		public new T type
		{
			get
			{
				return Reflection<object>.GetEnum<T>(base.type);
			}
			set
			{
				base.type = (int)(object)value;
			}
		}

		public AudioComponent Find(T p_type)
		{
			AudioComponent<T> res = null;
			Hierarchy.Traverse(base.transform, delegate(AudioComponent c)
			{
				if (!res && c is AudioComponent<T>)
				{
					AudioComponent<T> audioComponent = (AudioComponent<T>)c;
					int num = Reflection<object>.GetEnum(p_type as Enum);
					int typeFlag = audioComponent.GetTypeFlag();
					if (num == typeFlag)
					{
						res = audioComponent;
					}
				}
			});
			return res;
		}

		public void Play(T p_type, float p_time = -1f, float p_volume = -1f)
		{
			AudioComponent audioComponent = Find(p_type);
			if ((bool)audioComponent)
			{
				audioComponent.Play(p_time, p_volume);
			}
		}

		public void PlayInstance(T p_type, float p_time = -1f, float p_volume = -1f)
		{
			PlayInstance(p_type, new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity), p_time, p_volume);
		}

		public void PlayInstance(T p_type, Vector3 p_position, float p_time = -1f, float p_volume = -1f)
		{
			AudioComponent audioComponent = Find(p_type);
			if (!audioComponent)
			{
				return;
			}
			Transform transform = audioComponent.transform;
			string text = audioComponent.name;
			audioComponent = UnityEngine.Object.Instantiate(audioComponent);
			audioComponent.transform.SetParent(transform.parent);
			audioComponent.name = text + "-instance";
			if ((bool)audioComponent.source)
			{
				audioComponent.source.loop = false;
				if (!p_position.Equals(new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity)))
				{
					audioComponent.transform.position = p_position;
					audioComponent.source.spatialBlend = 1f;
				}
			}
			float t = (audioComponent.source ? (audioComponent.source.clip.length + 0.05f) : 0f);
			audioComponent.Play(p_time, p_volume);
			UnityEngine.Object.Destroy(audioComponent.gameObject, t);
		}

		public void Stop(T p_type)
		{
			AudioComponent audioComponent = Find(p_type);
			if ((bool)audioComponent)
			{
				audioComponent.Stop();
			}
		}

		public void Pause(T p_type)
		{
			AudioComponent audioComponent = Find(p_type);
			if ((bool)audioComponent)
			{
				audioComponent.Pause();
			}
		}

		public void Fade(T p_type, float p_volume, float p_time, Easing p_easing = null)
		{
			AudioComponent audioComponent = Find(p_type);
			if ((bool)audioComponent)
			{
				audioComponent.Fade(p_volume, p_time, p_easing);
			}
		}

		public void Fade(T p_type, float p_volume, Easing p_easing = null)
		{
			Fade(p_type, p_volume, 0.8f, p_easing);
		}

		public void FadePitch(T p_type, float p_pitch, float p_time, Easing p_easing = null)
		{
			AudioComponent audioComponent = Find(p_type);
			if ((bool)audioComponent)
			{
				audioComponent.FadePitch(p_pitch, p_time, p_easing);
			}
		}

		public void FadePitch(T p_type, float p_pitch, Easing p_easing = null)
		{
			FadePitch(p_type, p_pitch, 0.8f, p_easing);
		}
	}
	public class AudioComponent : MonoBehaviour
	{
		public int type;

		public AudioSource source => GetComponent<AudioSource>();

		internal int GetTypeFlag()
		{
			return type;
		}

		public AudioComponent Find(string p_path)
		{
			return Hierarchy.Find<AudioComponent>(base.transform, p_path);
		}

		public void Play(string p_path, float p_time = -1f, float p_volume = -1f)
		{
			AudioComponent audioComponent = Find(p_path);
			if ((bool)audioComponent)
			{
				audioComponent.Play(p_time, p_volume);
			}
		}

		public void Play(float p_time = -1f, float p_volume = -1f)
		{
			AudioSource audioSource = source;
			if (audioSource == null)
			{
				Debug.LogWarning("AudioComponent> Tried to play null sound - [" + base.name + "]");
				return;
			}
			audioSource.Play();
			if (p_time >= 0f)
			{
				audioSource.time = p_time;
			}
			if (p_volume >= 0f)
			{
				audioSource.volume = p_volume;
			}
		}

		public void Stop(string p_path)
		{
			AudioComponent audioComponent = Find(p_path);
			if ((bool)audioComponent)
			{
				audioComponent.Stop();
			}
		}

		public void Stop()
		{
			if ((bool)source)
			{
				source.Stop();
			}
		}

		public void Pause(string p_path)
		{
			AudioComponent audioComponent = Find(p_path);
			if ((bool)audioComponent)
			{
				audioComponent.Pause();
			}
		}

		public void Pause()
		{
			if ((bool)source)
			{
				source.Pause();
			}
		}

		public void Fade(string p_path, float p_volume, float p_time, Easing p_easing = null)
		{
			AudioComponent audioComponent = Find(p_path);
			if ((bool)audioComponent)
			{
				audioComponent.Fade(p_volume, p_time, p_easing);
			}
		}

		public void Fade(string p_path, float p_volume, Easing p_easing = null)
		{
			Fade(p_path, p_volume, 0.8f, p_easing);
		}

		public void Fade(float p_volume, float p_time, Easing p_easing = null)
		{
			if ((bool)source)
			{
				Tween.Add(source, "volume", p_volume, p_time, (p_easing == null) ? null : p_easing);
			}
		}

		public void Fade(float p_volume, Easing p_easing = null)
		{
			Fade(p_volume, 0.8f, p_easing);
		}

		public void FadePitch(string p_path, float p_pitch, float p_time, Easing p_easing = null)
		{
			AudioComponent audioComponent = Find(p_path);
			if ((bool)audioComponent)
			{
				audioComponent.FadePitch(p_pitch, p_time, p_easing);
			}
		}

		public void FadePitch(string p_path, float p_pitch, Easing p_easing = null)
		{
			FadePitch(p_path, p_pitch, 0.8f, p_easing);
		}

		public void FadePitch(float p_pitch, Easing p_easing = null)
		{
			Fade(p_pitch, 0.8f, p_easing);
		}

		public void FadePitch(float p_pitch, float p_time = 0.8f, Easing p_easing = null)
		{
			if ((bool)source)
			{
				if (p_time <= 0f)
				{
					source.pitch = p_pitch;
				}
				else
				{
					Tween.Add(source, "pitch", p_pitch, p_time, (p_easing == null) ? new Easing(Cubic.Out) : p_easing);
				}
			}
		}
	}
}
