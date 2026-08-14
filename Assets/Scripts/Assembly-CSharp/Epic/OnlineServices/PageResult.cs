namespace Epic.OnlineServices
{
	public class PageResult : ISettable
	{
		public int StartIndex { get; set; }

		public int Count { get; set; }

		public int TotalCount { get; set; }

		internal void Set(PageResultInternal? other)
		{
			if (other.HasValue)
			{
				StartIndex = other.Value.StartIndex;
				Count = other.Value.Count;
				TotalCount = other.Value.TotalCount;
			}
		}

		public void Set(object other)
		{
			Set(other as PageResultInternal?);
		}
	}
}
