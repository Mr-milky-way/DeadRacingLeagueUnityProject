using System;

namespace Epic.OnlineServices.Sessions
{
	public sealed class SessionDetails : Handle
	{
		public const int SessiondetailsAttributeApiLatest = 1;

		public const int SessiondetailsCopyinfoApiLatest = 1;

		public const int SessiondetailsCopysessionattributebyindexApiLatest = 1;

		public const int SessiondetailsCopysessionattributebykeyApiLatest = 1;

		public const int SessiondetailsGetsessionattributecountApiLatest = 1;

		public const int SessiondetailsInfoApiLatest = 1;

		public const int SessiondetailsSettingsApiLatest = 2;

		public SessionDetails()
		{
		}

		public SessionDetails(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CopyInfo(SessionDetailsCopyInfoOptions options, out SessionDetailsInfo outSessionInfo)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionDetailsCopyInfoOptionsInternal, SessionDetailsCopyInfoOptions>(ref target, options);
			IntPtr outSessionInfo2 = IntPtr.Zero;
			Result result = Bindings.EOS_SessionDetails_CopyInfo(base.InnerHandle, target, ref outSessionInfo2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<SessionDetailsInfoInternal, SessionDetailsInfo>(outSessionInfo2, out outSessionInfo))
			{
				Bindings.EOS_SessionDetails_Info_Release(outSessionInfo2);
			}
			return result;
		}

		public Result CopySessionAttributeByIndex(SessionDetailsCopySessionAttributeByIndexOptions options, out SessionDetailsAttribute outSessionAttribute)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionDetailsCopySessionAttributeByIndexOptionsInternal, SessionDetailsCopySessionAttributeByIndexOptions>(ref target, options);
			IntPtr outSessionAttribute2 = IntPtr.Zero;
			Result result = Bindings.EOS_SessionDetails_CopySessionAttributeByIndex(base.InnerHandle, target, ref outSessionAttribute2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<SessionDetailsAttributeInternal, SessionDetailsAttribute>(outSessionAttribute2, out outSessionAttribute))
			{
				Bindings.EOS_SessionDetails_Attribute_Release(outSessionAttribute2);
			}
			return result;
		}

		public Result CopySessionAttributeByKey(SessionDetailsCopySessionAttributeByKeyOptions options, out SessionDetailsAttribute outSessionAttribute)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionDetailsCopySessionAttributeByKeyOptionsInternal, SessionDetailsCopySessionAttributeByKeyOptions>(ref target, options);
			IntPtr outSessionAttribute2 = IntPtr.Zero;
			Result result = Bindings.EOS_SessionDetails_CopySessionAttributeByKey(base.InnerHandle, target, ref outSessionAttribute2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<SessionDetailsAttributeInternal, SessionDetailsAttribute>(outSessionAttribute2, out outSessionAttribute))
			{
				Bindings.EOS_SessionDetails_Attribute_Release(outSessionAttribute2);
			}
			return result;
		}

		public uint GetSessionAttributeCount(SessionDetailsGetSessionAttributeCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionDetailsGetSessionAttributeCountOptionsInternal, SessionDetailsGetSessionAttributeCountOptions>(ref target, options);
			uint result = Bindings.EOS_SessionDetails_GetSessionAttributeCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void Release()
		{
			Bindings.EOS_SessionDetails_Release(base.InnerHandle);
		}
	}
}
