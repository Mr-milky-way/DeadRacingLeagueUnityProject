using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Reports
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OnSendPlayerBehaviorReportCompleteCallbackInternal(IntPtr data);
}
