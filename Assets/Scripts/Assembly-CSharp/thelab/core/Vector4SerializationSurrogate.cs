using System.Runtime.Serialization;
using UnityEngine;

namespace thelab.core
{
	internal sealed class Vector4SerializationSurrogate : ISerializationSurrogate
	{
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Vector4 vector = (Vector4)obj;
			info.AddValue("x", vector.x);
			info.AddValue("y", vector.y);
			info.AddValue("z", vector.z);
			info.AddValue("w", vector.w);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Vector4 vector = (Vector4)obj;
			vector.x = (float)info.GetValue("x", typeof(float));
			vector.y = (float)info.GetValue("y", typeof(float));
			vector.z = (float)info.GetValue("z", typeof(float));
			vector.w = (float)info.GetValue("w", typeof(float));
			return obj = vector;
		}
	}
}
