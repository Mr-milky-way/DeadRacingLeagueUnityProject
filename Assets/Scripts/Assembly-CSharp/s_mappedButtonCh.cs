using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct s_mappedButtonCh
{
	public int reportStartBit;

	public int reportStopBit;

	public int numberOfBtns;

	public int val;
}
