namespace thelab.core
{
	public class Cubic
	{
		public static float In(float p_r)
		{
			return p_r * p_r * p_r;
		}

		public static float Out(float p_r)
		{
			return p_r * (p_r * (p_r - 3f) + 3f);
		}

		public static float InOut(float p_r)
		{
			return -2f * p_r * (p_r * (p_r - 1.5f));
		}

		public static float OutIn(float p_r)
		{
			return p_r * (p_r * (4f * p_r - 6f) + 3f);
		}

		public static float BackIn(float p_r)
		{
			return p_r * (p_r * (4f * p_r - 3f));
		}

		public static float OutBack(float p_r)
		{
			return p_r * (p_r * (4f * p_r - 9f) + 6f);
		}
	}
}
