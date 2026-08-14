using System.Runtime.Serialization;
using UnityEngine;

namespace thelab.core
{
	internal sealed class GradientAlphaKeySerializationSurrogate : ISerializationSurrogate
	{
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			GradientAlphaKey gradientAlphaKey = (GradientAlphaKey)obj;
			info.AddValue("time", gradientAlphaKey.time);
			info.AddValue("alpha", gradientAlphaKey.alpha);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			GradientAlphaKey gradientAlphaKey = (GradientAlphaKey)obj;
			gradientAlphaKey.time = (float)info.GetValue("time", typeof(float));
			gradientAlphaKey.alpha = (float)info.GetValue("alpha", typeof(float));
			obj = gradientAlphaKey;
			return obj;
		}
	}
}
