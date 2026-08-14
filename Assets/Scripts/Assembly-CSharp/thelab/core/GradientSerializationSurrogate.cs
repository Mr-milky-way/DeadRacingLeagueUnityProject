using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace thelab.core
{
	internal sealed class GradientSerializationSurrogate : ISerializationSurrogate
	{
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Gradient gradient = (Gradient)obj;
			GradientColorKey[] colorKeys = gradient.colorKeys;
			List<float> list = new List<float>();
			for (int i = 0; i < colorKeys.Length; i++)
			{
				list.Add(colorKeys[i].time);
				list.Add(colorKeys[i].color.r);
				list.Add(colorKeys[i].color.g);
				list.Add(colorKeys[i].color.b);
				list.Add(colorKeys[i].color.a);
			}
			info.AddValue("$color-keys", list.ToArray());
			GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
			list = new List<float>();
			for (int j = 0; j < alphaKeys.Length; j++)
			{
				list.Add(alphaKeys[j].time);
				list.Add(alphaKeys[j].alpha);
			}
			info.AddValue("$alpha-keys", list.ToArray());
			info.AddValue("mode", gradient.mode);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Gradient gradient = new Gradient();
			int num = 0;
			num = 0;
			float[] array = (float[])info.GetValue("$color-keys", typeof(float[]));
			List<GradientColorKey> list = new List<GradientColorKey>();
			for (int i = 0; i < array.Length; i++)
			{
				GradientColorKey item = default(GradientColorKey);
				Color color = item.color;
				if (num >= array.Length)
				{
					break;
				}
				item.time = array[num++];
				if (num >= array.Length)
				{
					break;
				}
				color.r = array[num++];
				if (num >= array.Length)
				{
					break;
				}
				color.g = array[num++];
				if (num >= array.Length)
				{
					break;
				}
				color.b = array[num++];
				if (num >= array.Length)
				{
					break;
				}
				color.a = array[num++];
				item.color = color;
				list.Add(item);
			}
			num = 0;
			array = (float[])info.GetValue("$alpha-keys", typeof(float[]));
			List<GradientAlphaKey> list2 = new List<GradientAlphaKey>();
			for (int j = 0; j < array.Length; j++)
			{
				GradientAlphaKey item2 = default(GradientAlphaKey);
				if (num >= array.Length)
				{
					break;
				}
				item2.time = array[num++];
				if (num >= array.Length)
				{
					break;
				}
				item2.alpha = array[num++];
				list2.Add(item2);
			}
			gradient.mode = (GradientMode)info.GetValue("mode", typeof(GradientMode));
			gradient.SetKeys(list.ToArray(), list2.ToArray());
			obj = gradient;
			return obj;
		}
	}
}
