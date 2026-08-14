using System;

namespace Epic.OnlineServices.TitleStorage
{
	public sealed class TitleStorageFileTransferRequest : Handle
	{
		public TitleStorageFileTransferRequest()
		{
		}

		public TitleStorageFileTransferRequest(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CancelRequest()
		{
			return Bindings.EOS_TitleStorageFileTransferRequest_CancelRequest(base.InnerHandle);
		}

		public Result GetFileRequestState()
		{
			return Bindings.EOS_TitleStorageFileTransferRequest_GetFileRequestState(base.InnerHandle);
		}

		public Result GetFilename(out string outStringBuffer)
		{
			IntPtr target = IntPtr.Zero;
			int outStringLength = 64;
			Helper.TryMarshalAllocate(ref target, outStringLength, out var _);
			Result result = Bindings.EOS_TitleStorageFileTransferRequest_GetFilename(base.InnerHandle, (uint)outStringLength, target, ref outStringLength);
			Helper.TryMarshalGet(target, out outStringBuffer);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void Release()
		{
			Bindings.EOS_TitleStorageFileTransferRequest_Release(base.InnerHandle);
		}
	}
}
