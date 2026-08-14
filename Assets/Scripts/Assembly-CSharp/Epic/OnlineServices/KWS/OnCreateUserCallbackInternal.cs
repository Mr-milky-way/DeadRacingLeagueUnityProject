using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.KWS
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OnCreateUserCallbackInternal(IntPtr data);
}
