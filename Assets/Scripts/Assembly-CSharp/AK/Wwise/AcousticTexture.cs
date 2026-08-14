using System;

namespace AK.Wwise
{
	[Serializable]
	public class AcousticTexture : BaseType
	{
		public WwiseAcousticTextureReference WwiseObjectReference;

		public override WwiseObjectReference ObjectReference
		{
			get
			{
				return WwiseObjectReference;
			}
			set
			{
				WwiseObjectReference = value as WwiseAcousticTextureReference;
			}
		}

		public override WwiseObjectType WwiseObjectType => WwiseObjectType.AcousticTexture;
	}
}
