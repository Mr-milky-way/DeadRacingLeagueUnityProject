using System.Runtime.Serialization;
using UnityEngine;

namespace thelab.core
{
	internal sealed class BoundsSerializationSurrogate : ISerializationSurrogate
	{
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Bounds bounds = (Bounds)obj;
			info.AddValue("min", bounds.min);
			info.AddValue("max", bounds.max);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Bounds bounds = (Bounds)obj;
			bounds.min = (Vector3)info.GetValue("min", typeof(Vector3));
			bounds.max = (Vector3)info.GetValue("max", typeof(Vector3));
			return obj = bounds;
		}
	}
}
