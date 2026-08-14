using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OnLoginCallbackInternal(IntPtr data);
}
