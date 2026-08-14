using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct s_mappedCh
{
	public int chType;

	public s_mappedButtonCh button;

	public s_mappedJoystickCh joystick;
}
