using System;

namespace Epic.OnlineServices.Lobby
{
	public sealed class LobbyDetails : Handle
	{
		public const int LobbydetailsCopyattributebyindexApiLatest = 1;

		public const int LobbydetailsCopyattributebykeyApiLatest = 1;

		public const int LobbydetailsCopyinfoApiLatest = 1;

		public const int LobbydetailsCopymemberattributebyindexApiLatest = 1;

		public const int LobbydetailsCopymemberattributebykeyApiLatest = 1;

		public const int LobbydetailsGetattributecountApiLatest = 1;

		public const int LobbydetailsGetlobbyownerApiLatest = 1;

		public const int LobbydetailsGetmemberattributecountApiLatest = 1;

		public const int LobbydetailsGetmemberbyindexApiLatest = 1;

		public const int LobbydetailsGetmembercountApiLatest = 1;

		public const int LobbydetailsInfoApiLatest = 1;

		public LobbyDetails()
		{
		}

		public LobbyDetails(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CopyAttributeByIndex(LobbyDetailsCopyAttributeByIndexOptions options, out Attribute outAttribute)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyDetailsCopyAttributeByIndexOptionsInternal, LobbyDetailsCopyAttributeByIndexOptions>(ref target, options);
			IntPtr outAttribute2 = IntPtr.Zero;
			Result result = Bindings.EOS_LobbyDetails_CopyAttributeByIndex(base.InnerHandle, target, ref outAttribute2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<AttributeInternal, Attribute>(outAttribute2, out outAttribute))
			{
				Bindings.EOS_Lobby_Attribute_Release(outAttribute2);
			}
			return result;
		}

		public Result CopyAttributeByKey(LobbyDetailsCopyAttributeByKeyOptions options, out Attribute outAttribute)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyDetailsCopyAttributeByKeyOptionsInternal, LobbyDetailsCopyAttributeByKeyOptions>(ref target, options);
			IntPtr outAttribute2 = IntPtr.Zero;
			Result result = Bindings.EOS_LobbyDetails_CopyAttributeByKey(base.InnerHandle, target, ref outAttribute2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<AttributeInternal, Attribute>(outAttribute2, out outAttribute))
			{
				Bindings.EOS_Lobby_Attribute_Release(outAttribute2);
			}
			return result;
		}

		public Result CopyInfo(LobbyDetailsCopyInfoOptions options, out LobbyDetailsInfo outLobbyDetailsInfo)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyDetailsCopyInfoOptionsInternal, LobbyDetailsCopyInfoOptions>(ref target, options);
			IntPtr outLobbyDetailsInfo2 = IntPtr.Zero;
			Result result = Bindings.EOS_LobbyDetails_CopyInfo(base.InnerHandle, target, ref outLobbyDetailsInfo2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<LobbyDetailsInfoInternal, LobbyDetailsInfo>(outLobbyDetailsInfo2, out outLobbyDetailsInfo))
			{
				Bindings.EOS_LobbyDetails_Info_Release(outLobbyDetailsInfo2);
			}
			return result;
		}

		public Result CopyMemberAttributeByIndex(LobbyDetailsCopyMemberAttributeByIndexOptions options, out Attribute outAttribute)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyDetailsCopyMemberAttributeByIndexOptionsInternal, LobbyDetailsCopyMemberAttributeByIndexOptions>(ref target, options);
			IntPtr outAttribute2 = IntPtr.Zero;
			Result result = Bindings.EOS_LobbyDetails_CopyMemberAttributeByIndex(base.InnerHandle, target, ref outAttribute2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<AttributeInternal, Attribute>(outAttribute2, out outAttribute))
			{
				Bindings.EOS_Lobby_Attribute_Release(outAttribute2);
			}
			return result;
		}

		public Result CopyMemberAttributeByKey(LobbyDetailsCopyMemberAttributeByKeyOptions options, out Attribute outAttribute)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyDetailsCopyMemberAttributeByKeyOptionsInternal, LobbyDetailsCopyMemberAttributeByKeyOptions>(ref target, options);
			IntPtr outAttribute2 = IntPtr.Zero;
			Result result = Bindings.EOS_LobbyDetails_CopyMemberAttributeByKey(base.InnerHandle, target, ref outAttribute2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<AttributeInternal, Attribute>(outAttribute2, out outAttribute))
			{
				Bindings.EOS_Lobby_Attribute_Release(outAttribute2);
			}
			return result;
		}

		public uint GetAttributeCount(LobbyDetailsGetAttributeCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyDetailsGetAttributeCountOptionsInternal, LobbyDetailsGetAttributeCountOptions>(ref target, options);
			uint result = Bindings.EOS_LobbyDetails_GetAttributeCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public ProductUserId GetLobbyOwner(LobbyDetailsGetLobbyOwnerOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyDetailsGetLobbyOwnerOptionsInternal, LobbyDetailsGetLobbyOwnerOptions>(ref target, options);
			IntPtr source = Bindings.EOS_LobbyDetails_GetLobbyOwner(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(source, out ProductUserId target2);
			return target2;
		}

		public uint GetMemberAttributeCount(LobbyDetailsGetMemberAttributeCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyDetailsGetMemberAttributeCountOptionsInternal, LobbyDetailsGetMemberAttributeCountOptions>(ref target, options);
			uint result = Bindings.EOS_LobbyDetails_GetMemberAttributeCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public ProductUserId GetMemberByIndex(LobbyDetailsGetMemberByIndexOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyDetailsGetMemberByIndexOptionsInternal, LobbyDetailsGetMemberByIndexOptions>(ref target, options);
			IntPtr source = Bindings.EOS_LobbyDetails_GetMemberByIndex(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(source, out ProductUserId target2);
			return target2;
		}

		public uint GetMemberCount(LobbyDetailsGetMemberCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<LobbyDetailsGetMemberCountOptionsInternal, LobbyDetailsGetMemberCountOptions>(ref target, options);
			uint result = Bindings.EOS_LobbyDetails_GetMemberCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void Release()
		{
			Bindings.EOS_LobbyDetails_Release(base.InnerHandle);
		}
	}
}
