using System;

namespace drl.sim.thread
{
	[Serializable]
	public struct PIDVector
	{
		public byte P;

		public byte I;

		public byte D;

		public ushort F;

		public static PIDVector zero => new PIDVector(0, 0, 0, 0);

		public PIDVector(byte _p, byte _i, byte _d, ushort _f)
		{
			P = _p;
			I = _i;
			D = _d;
			F = _f;
		}

		public void Set(byte _p, byte _i, byte _d, ushort _f)
		{
			P = _p;
			I = _i;
			D = _d;
			F = _f;
		}
	}
}
