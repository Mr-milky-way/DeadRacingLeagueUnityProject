using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace thelab.core
{
	internal sealed class AnimationCurveSerializationSurrogate : ISerializationSurrogate
	{
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			AnimationCurve animationCurve = (AnimationCurve)obj;
			Keyframe[] keys = animationCurve.keys;
			List<float> list = new List<float>();
			for (int i = 0; i < keys.Length; i++)
			{
				list.Add(keys[i].time);
				list.Add(keys[i].value);
				list.Add(keys[i].inTangent);
				list.Add(keys[i].outTangent);
				list.Add(keys[i].tangentMode);
			}
			info.AddValue("$keys", list.ToArray());
			info.AddValue("preWrapMode", animationCurve.preWrapMode);
			info.AddValue("postWrapMode", animationCurve.postWrapMode);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			AnimationCurve animationCurve = new AnimationCurve();
			float[] array = (float[])info.GetValue("$keys", typeof(float[]));
			List<Keyframe> list = new List<Keyframe>();
			int num = 0;
			for (int i = 0; i < array.Length; i++)
			{
				Keyframe item = default(Keyframe);
				if (num >= array.Length)
				{
					break;
				}
				item.time = array[num++];
				if (num >= array.Length)
				{
					break;
				}
				item.value = array[num++];
				if (num >= array.Length)
				{
					break;
				}
				item.inTangent = array[num++];
				if (num >= array.Length)
				{
					break;
				}
				item.outTangent = array[num++];
				if (num >= array.Length)
				{
					break;
				}
				item.tangentMode = (int)array[num++];
				list.Add(item);
			}
			animationCurve.keys = list.ToArray();
			animationCurve.preWrapMode = (WrapMode)info.GetValue("preWrapMode", typeof(WrapMode));
			animationCurve.postWrapMode = (WrapMode)info.GetValue("postWrapMode", typeof(WrapMode));
			obj = animationCurve;
			return obj;
		}
	}
}
