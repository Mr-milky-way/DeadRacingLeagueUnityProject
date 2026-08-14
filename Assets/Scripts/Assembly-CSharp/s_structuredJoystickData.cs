public struct s_structuredJoystickData
{
	public int usage;

	public double normVal;

	public bool doubleBytePrecision;

	public s_structuredJoystickData(int usg, double max, double min, double val)
	{
		usage = usg;
		if (max <= min)
		{
			min = -1.0 * min;
		}
		if (min == max)
		{
			normVal = 0.0;
		}
		else
		{
			normVal = (val - min) / (max - min) * 2.0 - 1.0;
		}
		doubleBytePrecision = max > 255.0;
	}
}
