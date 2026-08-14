using System.Runtime.Serialization;
using UnityEngine;

namespace thelab.core
{
	internal sealed class GradientColorKeySerializationSurrogate : ISerializationSurrogate
	{
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			GradientColorKey gradientColorKey = (GradientColorKey)obj;
			info.AddValue("time", gradientColorKey.time);
			info.AddValue("color", Colorf.ColorToARGB(gradientColorKey.color));
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			GradientColorKey gradientColorKey = (GradientColorKey)obj;
			gradientColorKey.time = (float)info.GetValue("time", typeof(float));
			gradientColorKey.color = Colorf.ARGBToColor((uint)info.GetValue("color", typeof(uint)));
			obj = gradientColorKey;
			return obj;
		}
	}
}
