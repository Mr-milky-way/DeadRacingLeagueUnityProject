using System;

namespace Epic.OnlineServices.Mods
{
	public sealed class ModsInterface : Handle
	{
		public const int CopymodinfoApiLatest = 1;

		public const int EnumeratemodsApiLatest = 1;

		public const int InstallmodApiLatest = 1;

		public const int ModIdentifierApiLatest = 1;

		public const int ModinfoApiLatest = 1;

		public const int UninstallmodApiLatest = 1;

		public const int UpdatemodApiLatest = 1;

		public ModsInterface()
		{
		}

		public ModsInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result CopyModInfo(CopyModInfoOptions options, out ModInfo outEnumeratedMods)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CopyModInfoOptionsInternal, CopyModInfoOptions>(ref target, options);
			IntPtr outEnumeratedMods2 = IntPtr.Zero;
			Result result = Bindings.EOS_Mods_CopyModInfo(base.InnerHandle, target, ref outEnumeratedMods2);
			Helper.TryMarshalDispose(ref target);
			if (Helper.TryMarshalGet<ModInfoInternal, ModInfo>(outEnumeratedMods2, out outEnumeratedMods))
			{
				Bindings.EOS_Mods_ModInfo_Release(outEnumeratedMods2);
			}
			return result;
		}

		public void EnumerateMods(EnumerateModsOptions options, object clientData, OnEnumerateModsCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<EnumerateModsOptionsInternal, EnumerateModsOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnEnumerateModsCallbackInternal onEnumerateModsCallbackInternal = OnEnumerateModsCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onEnumerateModsCallbackInternal);
			Bindings.EOS_Mods_EnumerateMods(base.InnerHandle, target, clientDataAddress, onEnumerateModsCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void InstallMod(InstallModOptions options, object clientData, OnInstallModCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<InstallModOptionsInternal, InstallModOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnInstallModCallbackInternal onInstallModCallbackInternal = OnInstallModCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onInstallModCallbackInternal);
			Bindings.EOS_Mods_InstallMod(base.InnerHandle, target, clientDataAddress, onInstallModCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void UninstallMod(UninstallModOptions options, object clientData, OnUninstallModCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<UninstallModOptionsInternal, UninstallModOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnUninstallModCallbackInternal onUninstallModCallbackInternal = OnUninstallModCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onUninstallModCallbackInternal);
			Bindings.EOS_Mods_UninstallMod(base.InnerHandle, target, clientDataAddress, onUninstallModCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public void UpdateMod(UpdateModOptions options, object clientData, OnUpdateModCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<UpdateModOptionsInternal, UpdateModOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnUpdateModCallbackInternal onUpdateModCallbackInternal = OnUpdateModCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onUpdateModCallbackInternal);
			Bindings.EOS_Mods_UpdateMod(base.InnerHandle, target, clientDataAddress, onUpdateModCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		[MonoPInvokeCallback(typeof(OnEnumerateModsCallbackInternal))]
		internal static void OnEnumerateModsCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnEnumerateModsCallback, EnumerateModsCallbackInfoInternal, EnumerateModsCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnInstallModCallbackInternal))]
		internal static void OnInstallModCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnInstallModCallback, InstallModCallbackInfoInternal, InstallModCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnUninstallModCallbackInternal))]
		internal static void OnUninstallModCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnUninstallModCallback, UninstallModCallbackInfoInternal, UninstallModCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnUpdateModCallbackInternal))]
		internal static void OnUpdateModCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnUpdateModCallback, UpdateModCallbackInfoInternal, UpdateModCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
