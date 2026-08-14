using System;

namespace Epic.OnlineServices.Sessions
{
	public sealed class ActiveSession : Handle
	{
		public const int ActivesessionCopyinfoApiLatest = 1;

		public const int ActivesessionGetregisteredplayerbyindexApiLatest = 1;

		public const int ActivesessionGetregisteredplayercountApiLatest = 1;

		public const int ActivesessionInfoApiLatest = 1;

		public ActiveSession()
		{
		}

		public ActiveSession(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CopyInfo(ActiveSessionCopyInfoOptions options, out ActiveSessionInfo outActiveSessionInfo)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<ActiveSessionCopyInfoOptionsInternal, ActiveSessionCopyInfoOptions>(ref target, options);
			IntPtr outActiveSessionInfo2 = IntPtr.Zero;
			Result result = Bindings.EOS_ActiveSession_CopyInfo(base.InnerHandle, target, ref outActiveSessionInfo2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<ActiveSessionInfoInternal, ActiveSessionInfo>(outActiveSessionInfo2, out outActiveSessionInfo))
			{
				Bindings.EOS_ActiveSession_Info_Release(outActiveSessionInfo2);
			}
			return result;
		}

		public ProductUserId GetRegisteredPlayerByIndex(ActiveSessionGetRegisteredPlayerByIndexOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<ActiveSessionGetRegisteredPlayerByIndexOptionsInternal, ActiveSessionGetRegisteredPlayerByIndexOptions>(ref target, options);
			IntPtr source = Bindings.EOS_ActiveSession_GetRegisteredPlayerByIndex(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(source, out ProductUserId target2);
			return target2;
		}

		public uint GetRegisteredPlayerCount(ActiveSessionGetRegisteredPlayerCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<ActiveSessionGetRegisteredPlayerCountOptionsInternal, ActiveSessionGetRegisteredPlayerCountOptions>(ref target, options);
			uint result = Bindings.EOS_ActiveSession_GetRegisteredPlayerCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void Release()
		{
			Bindings.EOS_ActiveSession_Release(base.InnerHandle);
		}
	}
}
