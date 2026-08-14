namespace Epic.OnlineServices.Ecom
{
	public class KeyImageInfo : ISettable
	{
		public string Type { get; set; }

		public string Url { get; set; }

		public uint Width { get; set; }

		public uint Height { get; set; }

		internal void Set(KeyImageInfoInternal? other)
		{
			if (other.HasValue)
			{
				Type = other.Value.Type;
				Url = other.Value.Url;
				Width = other.Value.Width;
				Height = other.Value.Height;
			}
		}

		public void Set(object other)
		{
			Set(other as KeyImageInfoInternal?);
		}
	}
}
