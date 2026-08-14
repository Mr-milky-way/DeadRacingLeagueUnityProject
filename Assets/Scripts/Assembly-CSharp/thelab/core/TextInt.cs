namespace thelab.core
{
	public class TextInt : TextNumber<int>
	{
		public override string GetStringValue()
		{
			return base.value.ToString(format);
		}
	}
}
