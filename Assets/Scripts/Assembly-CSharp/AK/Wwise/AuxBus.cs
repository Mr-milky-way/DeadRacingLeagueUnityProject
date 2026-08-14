using System;

namespace AK.Wwise
{
	[Serializable]
	public class AuxBus : BaseType
	{
		public WwiseAuxBusReference WwiseObjectReference;

		public override WwiseObjectReference ObjectReference
		{
			get
			{
				return WwiseObjectReference;
			}
			set
			{
				WwiseObjectReference = value as WwiseAuxBusReference;
			}
		}

		public override WwiseObjectType WwiseObjectType => WwiseObjectType.AuxBus;
	}
}
