using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct s_simpleMsg
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
	public byte[] data;

	public int length;
}
