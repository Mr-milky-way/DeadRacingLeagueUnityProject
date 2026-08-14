namespace thelab.core
{
	public enum AsyncRequestType
	{
		None = 0,
		Web = 16,
		HttpGet = 1,
		HttpPost = 2,
		HttpPut = 3,
		HttpDelete = 4,
		HttpHead = 5,
		HttpCreate = 6,
		BundleLoad = 7,
		BundleRead = 32
	}
}
