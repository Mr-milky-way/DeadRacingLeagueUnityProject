namespace drl.sim
{
	public class FCManeauverProcess : FCProcess
	{
		public float[] layout = new float[4] { 1f, 1f, 1f, 1f };

		public virtual float rate => 0f;

		public virtual float irate
		{
			get
			{
				if (!(rate <= 0f))
				{
					return 1f / rate;
				}
				return 0f;
			}
		}

		public override float[] signals
		{
			get
			{
				float[] array = base.signals;
				float num = irate;
				float num2 = ((base.pid == null) ? 0f : base.pid.control);
				for (int i = 0; i < layout.Length; i++)
				{
					array[i] = num2 * layout[i] * num;
				}
				return array;
			}
		}
	}
}
