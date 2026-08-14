using System;

namespace Epic.OnlineServices.Sessions
{
	public sealed class SessionModification : Handle
	{
		public const int SessionmodificationAddattributeApiLatest = 1;

		public const int SessionmodificationMaxSessionAttributeLength = 64;

		public const int SessionmodificationMaxSessionAttributes = 64;

		public const int SessionmodificationMaxSessionidoverrideLength = 64;

		public const int SessionmodificationMinSessionidoverrideLength = 16;

		public const int SessionmodificationRemoveattributeApiLatest = 1;

		public const int SessionmodificationSetbucketidApiLatest = 1;

		public const int SessionmodificationSethostaddressApiLatest = 1;

		public const int SessionmodificationSetinvitesallowedApiLatest = 1;

		public const int SessionmodificationSetjoininprogressallowedApiLatest = 1;

		public const int SessionmodificationSetmaxplayersApiLatest = 1;

		public const int SessionmodificationSetpermissionlevelApiLatest = 1;

		public SessionModification()
		{
		}

		public SessionModification(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result AddAttribute(SessionModificationAddAttributeOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionModificationAddAttributeOptionsInternal, SessionModificationAddAttributeOptions>(ref target, options);
			Result result = Bindings.EOS_SessionModification_AddAttribute(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void Release()
		{
			Bindings.EOS_SessionModification_Release(base.InnerHandle);
		}

		public Result RemoveAttribute(SessionModificationRemoveAttributeOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionModificationRemoveAttributeOptionsInternal, SessionModificationRemoveAttributeOptions>(ref target, options);
			Result result = Bindings.EOS_SessionModification_RemoveAttribute(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetBucketId(SessionModificationSetBucketIdOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionModificationSetBucketIdOptionsInternal, SessionModificationSetBucketIdOptions>(ref target, options);
			Result result = Bindings.EOS_SessionModification_SetBucketId(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetHostAddress(SessionModificationSetHostAddressOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionModificationSetHostAddressOptionsInternal, SessionModificationSetHostAddressOptions>(ref target, options);
			Result result = Bindings.EOS_SessionModification_SetHostAddress(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetInvitesAllowed(SessionModificationSetInvitesAllowedOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionModificationSetInvitesAllowedOptionsInternal, SessionModificationSetInvitesAllowedOptions>(ref target, options);
			Result result = Bindings.EOS_SessionModification_SetInvitesAllowed(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetJoinInProgressAllowed(SessionModificationSetJoinInProgressAllowedOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionModificationSetJoinInProgressAllowedOptionsInternal, SessionModificationSetJoinInProgressAllowedOptions>(ref target, options);
			Result result = Bindings.EOS_SessionModification_SetJoinInProgressAllowed(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetMaxPlayers(SessionModificationSetMaxPlayersOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionModificationSetMaxPlayersOptionsInternal, SessionModificationSetMaxPlayersOptions>(ref target, options);
			Result result = Bindings.EOS_SessionModification_SetMaxPlayers(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetPermissionLevel(SessionModificationSetPermissionLevelOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SessionModificationSetPermissionLevelOptionsInternal, SessionModificationSetPermissionLevelOptions>(ref target, options);
			Result result = Bindings.EOS_SessionModification_SetPermissionLevel(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}
	}
}
