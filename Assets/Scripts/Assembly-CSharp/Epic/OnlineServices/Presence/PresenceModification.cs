using System;

namespace Epic.OnlineServices.Presence
{
	public sealed class PresenceModification : Handle
	{
		public const int PresencemodificationDatarecordidApiLatest = 1;

		public const int PresencemodificationDeletedataApiLatest = 1;

		public const int PresencemodificationJoininfoMaxLength = 255;

		public const int PresencemodificationSetdataApiLatest = 1;

		public const int PresencemodificationSetjoininfoApiLatest = 1;

		public const int PresencemodificationSetrawrichtextApiLatest = 1;

		public const int PresencemodificationSetstatusApiLatest = 1;

		public PresenceModification()
		{
		}

		public PresenceModification(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result DeleteData(PresenceModificationDeleteDataOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<PresenceModificationDeleteDataOptionsInternal, PresenceModificationDeleteDataOptions>(ref target, options);
			Result result = Bindings.EOS_PresenceModification_DeleteData(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void Release()
		{
			Bindings.EOS_PresenceModification_Release(base.InnerHandle);
		}

		public Result SetData(PresenceModificationSetDataOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<PresenceModificationSetDataOptionsInternal, PresenceModificationSetDataOptions>(ref target, options);
			Result result = Bindings.EOS_PresenceModification_SetData(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetJoinInfo(PresenceModificationSetJoinInfoOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<PresenceModificationSetJoinInfoOptionsInternal, PresenceModificationSetJoinInfoOptions>(ref target, options);
			Result result = Bindings.EOS_PresenceModification_SetJoinInfo(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetRawRichText(PresenceModificationSetRawRichTextOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<PresenceModificationSetRawRichTextOptionsInternal, PresenceModificationSetRawRichTextOptions>(ref target, options);
			Result result = Bindings.EOS_PresenceModification_SetRawRichText(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetStatus(PresenceModificationSetStatusOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<PresenceModificationSetStatusOptionsInternal, PresenceModificationSetStatusOptions>(ref target, options);
			Result result = Bindings.EOS_PresenceModification_SetStatus(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}
	}
}
