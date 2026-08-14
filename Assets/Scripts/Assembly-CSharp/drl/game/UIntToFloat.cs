using System.Runtime.InteropServices;

namespace drl.game
{
	[StructLayout(LayoutKind.Explicit)]
	public struct UIntToFloat
	{
		[FieldOffset(0)]
		public uint Bytes;

		[FieldOffset(0)]
		public float Value;

		[FieldOffset(0)]
		public byte Byte0;

		[FieldOffset(1)]
		public byte Byte1;

		[FieldOffset(2)]
		public byte Byte2;

		[FieldOffset(3)]
		public byte Byte3;
	}
}
