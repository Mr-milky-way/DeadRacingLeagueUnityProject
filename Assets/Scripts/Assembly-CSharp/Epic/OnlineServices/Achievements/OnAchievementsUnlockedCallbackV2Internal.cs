using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OnAchievementsUnlockedCallbackV2Internal(IntPtr data);
}
