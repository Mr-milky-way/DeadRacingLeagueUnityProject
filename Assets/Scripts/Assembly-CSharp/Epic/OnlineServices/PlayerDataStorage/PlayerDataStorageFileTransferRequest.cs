using System;

namespace Epic.OnlineServices.PlayerDataStorage
{
	public sealed class PlayerDataStorageFileTransferRequest : Handle
	{
		public PlayerDataStorageFileTransferRequest()
		{
		}

		public PlayerDataStorageFileTransferRequest(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CancelRequest()
		{
			return Bindings.EOS_PlayerDataStorageFileTransferRequest_CancelRequest(base.InnerHandle);
		}

		public Result GetFileRequestState()
		{
			return Bindings.EOS_PlayerDataStorageFileTransferRequest_GetFileRequestState(base.InnerHandle);
		}

		public Result GetFilename(out string outStringBuffer)
		{
			IntPtr target = IntPtr.Zero;
			int outStringLength = 64;
			Helper.TryMarshalAllocate(ref target, outStringLength, out var _);
			Result result = Bindings.EOS_PlayerDataStorageFileTransferRequest_GetFilename(base.InnerHandle, (uint)outStringLength, target, ref outStringLength);
			Helper.TryMarshalGet(target, out outStringBuffer);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void Release()
		{
			Bindings.EOS_PlayerDataStorageFileTransferRequest_Release(base.InnerHandle);
		}
	}
}
