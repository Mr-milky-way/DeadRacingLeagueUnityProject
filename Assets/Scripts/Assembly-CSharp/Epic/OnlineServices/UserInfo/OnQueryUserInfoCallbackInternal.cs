using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UserInfo
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OnQueryUserInfoCallbackInternal(IntPtr data);
}
