using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OnLobbyInviteReceivedCallbackInternal(IntPtr data);
}
