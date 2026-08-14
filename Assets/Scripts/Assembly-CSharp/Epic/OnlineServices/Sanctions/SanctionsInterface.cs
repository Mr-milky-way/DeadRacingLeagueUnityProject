using System;

namespace Epic.OnlineServices.Sanctions
{
	public sealed class SanctionsInterface : Handle
	{
		public const int CopyplayersanctionbyindexApiLatest = 1;

		public const int GetplayersanctioncountApiLatest = 1;

		public const int PlayersanctionApiLatest = 1;

		public const int QueryactiveplayersanctionsApiLatest = 2;

		public SanctionsInterface()
		{
		}

		public SanctionsInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CopyPlayerSanctionByIndex(CopyPlayerSanctionByIndexOptions options, out PlayerSanction outSanction)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyPlayerSanctionByIndexOptionsInternal, CopyPlayerSanctionByIndexOptions>(ref target, options);
			IntPtr outSanction2 = IntPtr.Zero;
			Result result = Bindings.EOS_Sanctions_CopyPlayerSanctionByIndex(base.InnerHandle, target, ref outSanction2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<PlayerSanctionInternal, PlayerSanction>(outSanction2, out outSanction))
			{
				Bindings.EOS_Sanctions_PlayerSanction_Release(outSanction2);
			}
			return result;
		}

		public uint GetPlayerSanctionCount(GetPlayerSanctionCountOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetPlayerSanctionCountOptionsInternal, GetPlayerSanctionCountOptions>(ref target, options);
			uint result = Bindings.EOS_Sanctions_GetPlayerSanctionCount(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void QueryActivePlayerSanctions(QueryActivePlayerSanctionsOptions options, object clientData, OnQueryActivePlayerSanctionsCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryActivePlayerSanctionsOptionsInternal, QueryActivePlayerSanctionsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryActivePlayerSanctionsCallbackInternal onQueryActivePlayerSanctionsCallbackInternal = OnQueryActivePlayerSanctionsCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryActivePlayerSanctionsCallbackInternal);
			Bindings.EOS_Sanctions_QueryActivePlayerSanctions(base.InnerHandle, target, clientDataAddress, onQueryActivePlayerSanctionsCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		[MonoPInvokeCallback(typeof(OnQueryActivePlayerSanctionsCallbackInternal))]
		internal static void OnQueryActivePlayerSanctionsCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryActivePlayerSanctionsCallback, QueryActivePlayerSanctionsCallbackInfoInternal, QueryActivePlayerSanctionsCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
