namespace thelab.core
{
	public class Quartic
	{
		public static float In(float p_r)
		{
			return p_r * p_r * p_r * p_r;
		}

		public static float Out(float p_r)
		{
			return p_r * (p_r * (p_r * (0f - p_r + 4f) - 6f) + 4f);
		}

		public static float OutIn(float p_r)
		{
			return p_r * (p_r * (p_r * (p_r + 2f) - 4f) + 2f);
		}

		public static float BackIn(float p_r)
		{
			return p_r * (p_r * (p_r * (p_r + 2f) + 1f) - 3f);
		}

		public static float OutBack(float p_r)
		{
			return p_r * (p_r * (p_r * (-2f * p_r + 10f) - 15f) + 8f);
		}
	}
}
