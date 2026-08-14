using System;

namespace Epic.OnlineServices
{
	public sealed class EpicAccountId : Handle
	{
		public const int EpicaccountidMaxLength = 32;

		public EpicAccountId()
		{
		}

		public EpicAccountId(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public static EpicAccountId FromString(string accountIdString)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet(ref target, accountIdString);
			IntPtr source = Bindings.EOS_EpicAccountId_FromString(target);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(source, out EpicAccountId target2);
			return target2;
		}

		public bool IsValid()
		{
			Helper.TryMarshalGet(Bindings.EOS_EpicAccountId_IsValid(base.InnerHandle), out var target);
			return target;
		}

		public Result ToString(out string outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			int inOutBufferLength = 33;
			Helper.TryMarshalAllocate(ref target, inOutBufferLength, out var _);
			Result result = Bindings.EOS_EpicAccountId_ToString(base.InnerHandle, target, ref inOutBufferLength);
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
