namespace drl
{
	public enum NetworkByteStreamEventType
	{
		None = 0,
		Data = 1,
		Decode = 2,
		Connect = 3,
		Disconnect = 4,
		Listening = 5,
		Send = 6,
		Error = 7
	}
}
