using System;

namespace Epic.OnlineServices
{
	public sealed class ProductUserId : Handle
	{
		public const int ProductuseridMaxLength = 128;

		public ProductUserId()
		{
		}

		public ProductUserId(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public static ProductUserId FromString(string productUserIdString)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet(ref target, productUserIdString);
			IntPtr source = Bindings.EOS_ProductUserId_FromString(target);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(source, out ProductUserId target2);
			return target2;
		}

		public bool IsValid()
		{
			Helper.TryMarshalGet(Bindings.EOS_ProductUserId_IsValid(base.InnerHandle), out var target);
			return target;
		}

		public Result ToString(out string outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			int inOutBufferLength = 129;
			Helper.TryMarshalAllocate(ref target, inOutBufferLength, out var _);
			Result result = Bindings.EOS_ProductUserId_ToString(base.InnerHandle, target, ref inOutBufferLength);
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
