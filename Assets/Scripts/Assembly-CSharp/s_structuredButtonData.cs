using System.Collections;

public struct s_structuredButtonData
{
	public int numButtons;

	public BitArray vals;

	public int normVal;

	public s_structuredButtonData(int num, int data)
	{
		numButtons = num;
		vals = new BitArray(new int[1] { data });
		normVal = data;
	}
}
