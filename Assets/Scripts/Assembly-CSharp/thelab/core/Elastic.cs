namespace thelab.core
{
	public class Elastic
	{
		public static float OutBig(float p_r)
		{
			return p_r * (p_r * (p_r * (p_r * (56f * p_r + -175f) + 200f) + -100f) + 20f);
		}

		public static float OutSmall(float p_r)
		{
			return p_r * (p_r * (p_r * (p_r * (33f * p_r + -106f) + 126f) + -67f) + 15f);
		}

		public static float InBig(float p_r)
		{
			return p_r * (p_r * (p_r * (p_r * (33f * p_r + -59f) + 32f) + -5f));
		}

		public static float InSmall(float p_r)
		{
			return p_r * (p_r * (p_r * (p_r * (56f * p_r + -105f) + 60f) + -10f));
		}
	}
}
