using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct s_outputMapExp
{
	public int numButtons;

	public int numJoysticks;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	public s_mappedCh[] channels;
}
