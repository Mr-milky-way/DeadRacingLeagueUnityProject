using System;

namespace Epic.OnlineServices.Lobby
{
	public sealed class LobbyModification : Handle
	{
		public const int LobbymodificationAddattributeApiLatest = 1;

		public const int LobbymodificationAddmemberattributeApiLatest = 1;

		public const int LobbymodificationMaxAttributeLength = 64;

		public const int LobbymodificationMaxAttributes = 64;

		public const int LobbymodificationRemoveattributeApiLatest = 1;

		public const int LobbymodificationRemovememberattributeApiLatest = 1;

		public const int LobbymodificationSetbucketidApiLatest = 1;

		public const int LobbymodificationSetinvitesallowedApiLatest = 1;

		public const int LobbymodificationSetmaxmembersApiLatest = 1;

		public const int LobbymodificationSetpermissionlevelApiLatest = 1;

		public LobbyModification()
		{
		}

		public LobbyModification(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result AddAttribute(LobbyModificationAddAttributeOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationAddAttributeOptionsInternal, LobbyModificationAddAttributeOptions>(ref target, options);
			Result result = Bindings.EOS_LobbyModification_AddAttribute(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result AddMemberAttribute(LobbyModificationAddMemberAttributeOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationAddMemberAttributeOptionsInternal, LobbyModificationAddMemberAttributeOptions>(ref target, options);
			Result result = Bindings.EOS_LobbyModification_AddMemberAttribute(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void Release()
		{
			Bindings.EOS_LobbyModification_Release(base.InnerHandle);
		}

		public Result RemoveAttribute(LobbyModificationRemoveAttributeOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationRemoveAttributeOptionsInternal, LobbyModificationRemoveAttributeOptions>(ref target, options);
			Result result = Bindings.EOS_LobbyModification_RemoveAttribute(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result RemoveMemberAttribute(LobbyModificationRemoveMemberAttributeOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationRemoveMemberAttributeOptionsInternal, LobbyModificationRemoveMemberAttributeOptions>(ref target, options);
			Result result = Bindings.EOS_LobbyModification_RemoveMemberAttribute(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetBucketId(LobbyModificationSetBucketIdOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationSetBucketIdOptionsInternal, LobbyModificationSetBucketIdOptions>(ref target, options);
			Result result = Bindings.EOS_LobbyModification_SetBucketId(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetInvitesAllowed(LobbyModificationSetInvitesAllowedOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationSetInvitesAllowedOptionsInternal, LobbyModificationSetInvitesAllowedOptions>(ref target, options);
			Result result = Bindings.EOS_LobbyModification_SetInvitesAllowed(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetMaxMembers(LobbyModificationSetMaxMembersOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationSetMaxMembersOptionsInternal, LobbyModificationSetMaxMembersOptions>(ref target, options);
			Result result = Bindings.EOS_LobbyModification_SetMaxMembers(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetPermissionLevel(LobbyModificationSetPermissionLevelOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyModificationSetPermissionLevelOptionsInternal, LobbyModificationSetPermissionLevelOptions>(ref target, options);
			Result result = Bindings.EOS_LobbyModification_SetPermissionLevel(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}
	}
}
