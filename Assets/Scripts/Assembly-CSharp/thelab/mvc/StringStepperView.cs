namespace thelab.mvc
{
	public class StringStepperView : ListStepperView<string>
	{
		public enum Format
		{
			Default = 0,
			Upper = 1,
			Lower = 2
		}

		public Format textFormat;

		protected override string GetValueString()
		{
			return textFormat switch
			{
				Format.Lower => value.ToLower(), 
				Format.Upper => value.ToUpper(), 
				_ => value, 
			};
		}
	}
}
