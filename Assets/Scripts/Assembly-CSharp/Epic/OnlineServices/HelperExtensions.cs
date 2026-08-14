namespace Epic.OnlineServices
{
	public static class HelperExtensions
	{
		public static bool IsOperationComplete(this Result result)
		{
			return Common.IsOperationComplete(result);
		}

		public static string ToHexString(this byte[] byteArray)
		{
			return Common.ToString(byteArray);
		}
	}
}
