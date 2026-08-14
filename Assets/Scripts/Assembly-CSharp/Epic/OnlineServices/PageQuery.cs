namespace Epic.OnlineServices
{
	public class PageQuery : ISettable
	{
		public int StartIndex { get; set; }

		public int MaxCount { get; set; }

		internal void Set(PageQueryInternal? other)
		{
			if (other.HasValue)
			{
				StartIndex = other.Value.StartIndex;
				MaxCount = other.Value.MaxCount;
			}
		}

		public void Set(object other)
		{
			Set(other as PageQueryInternal?);
		}
	}
}
