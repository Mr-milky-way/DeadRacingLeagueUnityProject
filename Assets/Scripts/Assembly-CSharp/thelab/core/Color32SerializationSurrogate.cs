using System.Runtime.Serialization;
using UnityEngine;

namespace thelab.core
{
	internal sealed class Color32SerializationSurrogate : ISerializationSurrogate
	{
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Color32 color = (Color32)obj;
			info.AddValue("r", color.r);
			info.AddValue("g", color.g);
			info.AddValue("b", color.b);
			info.AddValue("a", color.a);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Color32 color = (Color32)obj;
			color.r = (byte)info.GetValue("r", typeof(byte));
			color.g = (byte)info.GetValue("g", typeof(byte));
			color.b = (byte)info.GetValue("b", typeof(byte));
			color.a = (byte)info.GetValue("a", typeof(byte));
			obj = color;
			return obj;
		}
	}
}
