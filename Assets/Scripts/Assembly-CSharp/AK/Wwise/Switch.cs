using System;
using UnityEngine;

namespace AK.Wwise
{
	[Serializable]
	public class Switch : BaseGroupType
	{
		public WwiseSwitchReference WwiseObjectReference;

		public override WwiseObjectReference ObjectReference
		{
			get
			{
				return WwiseObjectReference;
			}
			set
			{
				WwiseObjectReference = value as WwiseSwitchReference;
			}
		}

		public override WwiseObjectType WwiseObjectType => WwiseObjectType.Switch;

		public override WwiseObjectType WwiseObjectGroupType => WwiseObjectType.SwitchGroup;

		public void SetValue(GameObject gameObject)
		{
			if (IsValid())
			{
				AKRESULT result = AkSoundEngine.SetSwitch(base.GroupId, base.Id, gameObject);
				Verify(result);
			}
		}
	}
}
