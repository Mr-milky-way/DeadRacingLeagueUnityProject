namespace thelab.core
{
	public class TextFloat : TextNumber<float>
	{
		public override string GetStringValue()
		{
			return base.value.ToString(format);
		}
	}
}
