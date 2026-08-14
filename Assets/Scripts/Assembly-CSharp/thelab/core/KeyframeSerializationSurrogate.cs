using System.Runtime.Serialization;
using UnityEngine;

namespace thelab.core
{
	internal sealed class KeyframeSerializationSurrogate : ISerializationSurrogate
	{
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Keyframe keyframe = (Keyframe)obj;
			info.AddValue("time", keyframe.time);
			info.AddValue("value", keyframe.value);
			info.AddValue("inTangent", keyframe.inTangent);
			info.AddValue("outTangent", keyframe.outTangent);
			info.AddValue("tangentMode", keyframe.tangentMode);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Keyframe keyframe = (Keyframe)obj;
			keyframe.time = (float)info.GetValue("time", typeof(float));
			keyframe.value = (float)info.GetValue("value", typeof(float));
			keyframe.inTangent = (float)info.GetValue("inTangent", typeof(float));
			keyframe.outTangent = (float)info.GetValue("outTangent", typeof(float));
			keyframe.tangentMode = (int)info.GetValue("tangentMode", typeof(int));
			obj = keyframe;
			return obj;
		}
	}
}
