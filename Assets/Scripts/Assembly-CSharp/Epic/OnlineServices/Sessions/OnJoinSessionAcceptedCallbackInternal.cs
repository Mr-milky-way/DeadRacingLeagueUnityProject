using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OnJoinSessionAcceptedCallbackInternal(IntPtr data);
}
