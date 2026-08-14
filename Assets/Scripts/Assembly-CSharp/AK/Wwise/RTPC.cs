using System;
using UnityEngine;

namespace AK.Wwise
{
	[Serializable]
	public class RTPC : BaseType
	{
		public WwiseRtpcReference WwiseObjectReference;

		public override WwiseObjectReference ObjectReference
		{
			get
			{
				return WwiseObjectReference;
			}
			set
			{
				WwiseObjectReference = value as WwiseRtpcReference;
			}
		}

		public override WwiseObjectType WwiseObjectType => WwiseObjectType.GameParameter;

		public void SetValue(GameObject gameObject, float value)
		{
			if (IsValid())
			{
				AKRESULT result = AkSoundEngine.SetRTPCValue(base.Id, value, gameObject);
				Verify(result);
			}
		}

		public float GetValue(GameObject gameObject)
		{
			float out_rValue = 0f;
			if (IsValid())
			{
				int io_rValueType = ((!gameObject) ? 1 : 2);
				AKRESULT rTPCValue = AkSoundEngine.GetRTPCValue(base.Id, gameObject, 0u, out out_rValue, ref io_rValueType);
				Verify(rTPCValue);
			}
			return out_rValue;
		}

		public void SetGlobalValue(float value)
		{
			if (IsValid())
			{
				AKRESULT result = AkSoundEngine.SetRTPCValue(base.Id, value);
				Verify(result);
			}
		}

		public float GetGlobalValue()
		{
			return GetValue(null);
		}
	}
}
