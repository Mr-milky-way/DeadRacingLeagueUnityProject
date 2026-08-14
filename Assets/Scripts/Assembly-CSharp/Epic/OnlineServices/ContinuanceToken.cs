using System;

namespace Epic.OnlineServices
{
	public sealed class ContinuanceToken : Handle
	{
		public ContinuanceToken()
		{
		}

		public ContinuanceToken(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result ToString(out string outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			int inOutBufferLength = 1024;
			Helper.TryMarshalAllocate(ref target, inOutBufferLength, out var _);
			Result result = Bindings.EOS_ContinuanceToken_ToString(base.InnerHandle, target, ref inOutBufferLength);
			Helper.TryMarshalGet(target, out outBuffer);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public override string ToString()
		{
			ToString(out var outBuffer);
			return outBuffer;
		}
	}
}
