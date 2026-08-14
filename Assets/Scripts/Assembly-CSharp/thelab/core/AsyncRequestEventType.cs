namespace thelab.core
{
	public enum AsyncRequestEventType
	{
		Create = 0,
		Start = 1,
		Progress = 2,
		UploadProgress = 3,
		Pending = 4,
		Complete = 5,
		Cancel = 6,
		Error = 7
	}
}
