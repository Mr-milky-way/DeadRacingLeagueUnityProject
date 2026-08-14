using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UI
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OnHideFriendsCallbackInternal(IntPtr data);
}
