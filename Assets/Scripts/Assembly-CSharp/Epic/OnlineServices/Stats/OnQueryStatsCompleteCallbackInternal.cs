using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Stats
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OnQueryStatsCompleteCallbackInternal(IntPtr data);
}
