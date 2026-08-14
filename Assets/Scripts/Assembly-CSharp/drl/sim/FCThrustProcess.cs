namespace drl.sim
{
	public class FCThrustProcess : FCManeauverProcess
	{
		public float throttle;

		public override float[] signals
		{
			get
			{
				float[] array = base.signals;
				for (int i = 0; i < layout.Length; i++)
				{
					array[i] = throttle;
				}
				return array;
			}
		}

		protected override void OnUpdate()
		{
			if (base.fc.mode == FlightControllerMode.AcroClassic)
			{
				throttle = base.fc.signal.throttle;
				return;
			}
			throttle = base.fc.signal.throttle;
			for (int i = 0; i < base.fc.inputs.Count; i++)
			{
				base.fc.inputs[i] += throttle;
			}
		}

		public override void SetLayout(FrameLayoutType p_type)
		{
			if (p_type == FrameLayoutType.QuadX)
			{
				layout = new float[4] { 1f, 1f, 1f, 1f };
			}
		}
	}
}
