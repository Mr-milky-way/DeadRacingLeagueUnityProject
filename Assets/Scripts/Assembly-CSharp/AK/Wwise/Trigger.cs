using System;
using UnityEngine;

namespace AK.Wwise
{
	[Serializable]
	public class Trigger : BaseType
	{
		public WwiseTriggerReference WwiseObjectReference;

		public override WwiseObjectReference ObjectReference
		{
			get
			{
				return WwiseObjectReference;
			}
			set
			{
				WwiseObjectReference = value as WwiseTriggerReference;
			}
		}

		public override WwiseObjectType WwiseObjectType => WwiseObjectType.Trigger;

		public void Post(GameObject gameObject)
		{
			if (IsValid())
			{
				AKRESULT result = AkSoundEngine.PostTrigger(base.Id, gameObject);
				Verify(result);
			}
		}
	}
}
