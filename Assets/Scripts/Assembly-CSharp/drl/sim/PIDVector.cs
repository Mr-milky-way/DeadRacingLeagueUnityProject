using System;

namespace drl.sim
{
	[Serializable]
	public struct PIDVector
	{
		public float p;

		public float i;

		public float d;

		public static PIDVector zero => new PIDVector(0f, 0f, 0f);

		public PIDVector(float p_p, float p_i, float p_d)
		{
			p = p_p;
			i = p_i;
			d = p_d;
		}

		public PIDVector(PIDVector p_pid)
		{
			p = p_pid.p;
			i = p_pid.i;
			d = p_pid.d;
		}

		public void Set(PIDVector p_pid)
		{
			p = p_pid.p;
			i = p_pid.i;
			d = p_pid.d;
		}

		public void Set(float p_p, float p_i, float p_d)
		{
			p = p_p;
			i = p_i;
			d = p_d;
		}

		public string ToString(string fmt = "0.0000")
		{
			string text = p.ToString(fmt);
			string text2 = i.ToString(fmt);
			string text3 = d.ToString(fmt);
			return "[" + text + "," + text2 + "," + text3 + "]";
		}

		public static bool operator ==(PIDVector a, PIDVector b)
		{
			if (a.p == b.p && a.i == b.i)
			{
				return a.d == b.d;
			}
			return false;
		}

		public static bool operator !=(PIDVector a, PIDVector b)
		{
			if (a.p == b.p && a.i == b.i)
			{
				return a.d != b.d;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			return obj is PIDVector other && this == other;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = p.GetHashCode();
				hashCode = (hashCode * 397) ^ i.GetHashCode();
				return (hashCode * 397) ^ d.GetHashCode();
			}
		}
	}
}
