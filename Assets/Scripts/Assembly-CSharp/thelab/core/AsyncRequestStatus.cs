namespace thelab.core
{
	public enum AsyncRequestStatus
	{
		Idle = 0,
		Created = 1,
		Active = 2,
		Error = 3,
		Cancelled = 4,
		Pending = 5,
		Complete = 6
	}
}
