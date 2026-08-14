namespace Epic.OnlineServices.Logging
{
	public class LogMessage : ISettable
	{
		public string Category { get; private set; }

		public string Message { get; private set; }

		public LogLevel Level { get; private set; }

		internal void Set(LogMessageInternal? other)
		{
			if (other.HasValue)
			{
				Category = other.Value.Category;
				Message = other.Value.Message;
				Level = other.Value.Level;
			}
		}

		public void Set(object other)
		{
			Set(other as LogMessageInternal?);
		}
	}
}
