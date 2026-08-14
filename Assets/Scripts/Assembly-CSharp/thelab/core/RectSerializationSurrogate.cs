using System.Runtime.Serialization;
using UnityEngine;

namespace thelab.core
{
	internal sealed class RectSerializationSurrogate : ISerializationSurrogate
	{
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Rect rect = (Rect)obj;
			info.AddValue("xMin", rect.xMin);
			info.AddValue("xMax", rect.xMax);
			info.AddValue("yMin", rect.yMin);
			info.AddValue("yMax", rect.yMax);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Rect rect = (Rect)obj;
			rect.xMin = (float)info.GetValue("xMin", typeof(float));
			rect.xMax = (float)info.GetValue("xMax", typeof(float));
			rect.yMin = (float)info.GetValue("yMin", typeof(float));
			rect.yMax = (float)info.GetValue("yMax", typeof(float));
			return obj = rect;
		}
	}
}
