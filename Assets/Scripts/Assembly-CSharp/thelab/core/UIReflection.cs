using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace thelab.core
{
	public class UIReflection
	{
		public static T Get<T>(UIBehaviour p_target)
		{
			if (!p_target)
			{
				return default(T);
			}
			Type typeFromHandle = typeof(T);
			if (typeFromHandle == typeof(RectTransform))
			{
				return (T)(object)(RectTransform)p_target.transform;
			}
			if (typeFromHandle == typeof(string))
			{
				Text text = Get<Text>(p_target);
				if ((bool)text)
				{
					return (T)(object)text.text;
				}
				InputField inputField = Get<InputField>(p_target);
				if ((bool)inputField)
				{
					return (T)(object)inputField.text;
				}
				return (T)(object)"";
			}
			if (typeFromHandle == typeof(int))
			{
				Slider slider = Get<Slider>(p_target);
				if ((bool)slider)
				{
					return (T)(object)Mathf.FloorToInt(slider.value);
				}
				string text2 = Get<string>(p_target);
				int result = 0;
				if (!string.IsNullOrEmpty(text2))
				{
					int.TryParse(text2, out result);
				}
				return (T)(object)result;
			}
			if (typeFromHandle == typeof(float))
			{
				Slider slider2 = Get<Slider>(p_target);
				if ((bool)slider2)
				{
					return (T)(object)slider2.normalizedValue;
				}
				string text3 = Get<string>(p_target);
				float result2 = 0f;
				if (!string.IsNullOrEmpty(text3))
				{
					float.TryParse(text3, out result2);
				}
				return (T)(object)result2;
			}
			if (typeFromHandle == typeof(bool))
			{
				Toggle toggle = Get<Toggle>(p_target);
				if ((bool)toggle)
				{
					return (T)(object)toggle.isOn;
				}
				return (T)(object)false;
			}
			if (typeFromHandle == typeof(Color))
			{
				Graphic graphic = Get<Graphic>(p_target);
				if ((bool)graphic)
				{
					return (T)(object)graphic.color;
				}
				return (T)(object)Color.black;
			}
			if (typeFromHandle == typeof(Texture))
			{
				RawImage rawImage = Get<RawImage>(p_target);
				if ((bool)rawImage)
				{
					return (T)(object)rawImage.texture;
				}
				Image image = Get<Image>(p_target);
				if ((bool)image)
				{
					return (T)(object)image.sprite.texture;
				}
				return default(T);
			}
			if (typeFromHandle == typeof(Sprite))
			{
				Image image2 = Get<Image>(p_target);
				if ((bool)image2)
				{
					return (T)(object)image2.sprite;
				}
				return default(T);
			}
			if (typeFromHandle != p_target.GetType())
			{
				return default(T);
			}
			return (T)(object)p_target;
		}

		public static void Set<T>(UIBehaviour p_target, T p_value)
		{
			if (!p_target)
			{
				return;
			}
			Type typeFromHandle = typeof(T);
			if ((object)p_target != null)
			{
				GameObject gameObject = p_target.gameObject;
				if (typeFromHandle != typeof(Texture))
				{
					gameObject.SetActive(p_value != null);
				}
			}
			if (typeFromHandle == typeof(string))
			{
				Text text = Get<Text>(p_target);
				if ((bool)text)
				{
					text.text = ((p_value == null) ? "" : p_value.ToString());
				}
				InputField inputField = Get<InputField>(p_target);
				if ((bool)inputField)
				{
					inputField.text = ((p_value == null) ? "" : p_value.ToString());
				}
			}
			if (typeFromHandle == typeof(int))
			{
				Slider slider = Get<Slider>(p_target);
				if ((bool)slider)
				{
					slider.value = (int)Reflection<object>.Cast<float>(p_value);
				}
			}
			if (typeFromHandle == typeof(float))
			{
				Slider slider2 = Get<Slider>(p_target);
				if ((bool)slider2)
				{
					slider2.value = Reflection<object>.Cast<float>(p_value);
				}
			}
			if (typeFromHandle == typeof(bool))
			{
				Toggle toggle = Get<Toggle>(p_target);
				if ((bool)toggle)
				{
					toggle.isOn = Reflection<object>.Cast<bool>(p_value);
				}
				else
				{
					p_target.enabled = Reflection<object>.Cast<bool>(p_value);
				}
			}
			if (typeFromHandle == typeof(Color))
			{
				Graphic graphic = Get<Graphic>(p_target);
				if ((bool)graphic)
				{
					graphic.color = Reflection<object>.Cast<Color>(p_value);
				}
			}
			if (typeFromHandle == typeof(Texture))
			{
				RawImage rawImage = Get<RawImage>(p_target);
				if ((bool)rawImage)
				{
					rawImage.texture = Reflection<object>.Cast<Texture>(p_value);
				}
			}
			if (typeFromHandle == typeof(Sprite))
			{
				Image image = Get<Image>(p_target);
				if ((bool)image)
				{
					image.sprite = Reflection<object>.Cast<Sprite>(p_value);
				}
			}
		}
	}
}
