using System;

namespace Epic.OnlineServices
{
	public static class Common
	{
		public const ulong InvalidNotificationid = 0uL;

		public const int PagequeryApiLatest = 1;

		public const int PagequeryMaxcountDefault = 10;

		public const int PagequeryMaxcountMaximum = 100;

		public const int PaginationApiLatest = 1;

		public static bool IsOperationComplete(Result result)
		{
			Helper.TryMarshalGet(Bindings.EOS_EResult_IsOperationComplete(result), out var target);
			return target;
		}

		public static string ToString(Result result)
		{
			Helper.TryMarshalGet(Bindings.EOS_EResult_ToString(result), out string target);
			return target;
		}

		public static Result ToString(byte[] byteArray, out string outBuffer)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet(ref target, byteArray, out var arrayLength);
			IntPtr target2 = IntPtr.Zero;
			uint inOutBufferLength = 1024u;
			Helper.TryMarshalAllocate(ref target2, inOutBufferLength, out var _);
			Result result = Bindings.EOS_ByteArray_ToString(target, arrayLength, target2, ref inOutBufferLength);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(target2, out outBuffer);
			Helper.TryMarshalDispose(ref target2);
			return result;
		}

		public static string ToString(byte[] byteArray)
		{
			ToString(byteArray, out var outBuffer);
			return outBuffer;
		}
	}
}
