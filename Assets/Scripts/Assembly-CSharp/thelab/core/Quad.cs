namespace thelab.core
{
	public class Quad
	{
		public static float In(float p_r)
		{
			return p_r * p_r;
		}

		public static float Out(float p_r)
		{
			return p_r * (0f - p_r + 2f);
		}

		public static float OutBack(float p_r)
		{
			return p_r * (-3f * p_r + 4f);
		}

		public static float BackIn(float p_r)
		{
			return p_r * (3f * p_r - 2f);
		}
	}
}
