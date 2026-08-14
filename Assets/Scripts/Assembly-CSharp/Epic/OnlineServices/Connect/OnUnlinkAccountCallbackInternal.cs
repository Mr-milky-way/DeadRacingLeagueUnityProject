using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OnUnlinkAccountCallbackInternal(IntPtr data);
}
