using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.AntiCheatServer
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OnClientActionRequiredCallbackInternal(IntPtr data);
}
