namespace thelab.core
{
	public class Quintic
	{
		public static float In(float p_r)
		{
			return p_r * p_r * p_r * p_r * p_r;
		}

		public static float Out(float p_r)
		{
			return p_r * (p_r * (p_r * (p_r * (p_r - 5f) + 10f) - 10f) + 5f);
		}
	}
}
