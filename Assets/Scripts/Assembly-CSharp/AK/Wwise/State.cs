using System;

namespace AK.Wwise
{
	[Serializable]
	public class State : BaseGroupType
	{
		public WwiseStateReference WwiseObjectReference;

		public override WwiseObjectReference ObjectReference
		{
			get
			{
				return WwiseObjectReference;
			}
			set
			{
				WwiseObjectReference = value as WwiseStateReference;
			}
		}

		public override WwiseObjectType WwiseObjectType => WwiseObjectType.State;

		public override WwiseObjectType WwiseObjectGroupType => WwiseObjectType.StateGroup;

		public void SetValue()
		{
			if (IsValid())
			{
				AKRESULT result = AkSoundEngine.SetState(base.GroupId, base.Id);
				Verify(result);
			}
		}
	}
}
