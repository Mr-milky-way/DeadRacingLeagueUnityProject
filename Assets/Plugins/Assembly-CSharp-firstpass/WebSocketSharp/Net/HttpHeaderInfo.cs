namespace WebSocketSharp.Net
{
	internal class HttpHeaderInfo
	{
		private string _headerName;

		private HttpHeaderType _headerType;

		internal bool IsMultiValueInRequest => (_headerType & HttpHeaderType.MultiValueInRequest) == HttpHeaderType.MultiValueInRequest;

		internal bool IsMultiValueInResponse => (_headerType & HttpHeaderType.MultiValueInResponse) == HttpHeaderType.MultiValueInResponse;

		public string HeaderName => _headerName;

		public HttpHeaderType HeaderType => _headerType;

		public bool IsRequest => (_headerType & HttpHeaderType.Request) == HttpHeaderType.Request;

		public bool IsResponse => (_headerType & HttpHeaderType.Response) == HttpHeaderType.Response;

		internal HttpHeaderInfo(string headerName, HttpHeaderType headerType)
		{
			_headerName = headerName;
			_headerType = headerType;
		}

		public bool IsMultiValue(bool response)
		{
			if ((_headerType & HttpHeaderType.MultiValue) != HttpHeaderType.MultiValue)
			{
				if (!response)
				{
					return IsMultiValueInRequest;
				}
				return IsMultiValueInResponse;
			}
			if (!response)
			{
				return IsRequest;
			}
			return IsResponse;
		}

		public bool IsRestricted(bool response)
		{
			if ((_headerType & HttpHeaderType.Restricted) != HttpHeaderType.Restricted)
			{
				return false;
			}
			if (!response)
			{
				return IsRequest;
			}
			return IsResponse;
		}
	}
}
