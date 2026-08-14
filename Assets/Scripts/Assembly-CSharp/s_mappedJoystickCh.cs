using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct s_mappedJoystickCh
{
	public int reportStartBit;

	public int reportStopBit;

	public int logicalMin;

	public int logicalMax;

	public int usage;

	public int val;
}
