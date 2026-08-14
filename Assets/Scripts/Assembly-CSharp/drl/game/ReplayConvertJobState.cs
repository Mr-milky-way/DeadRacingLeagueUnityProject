namespace drl.game
{
	public enum ReplayConvertJobState
	{
		Idle = 0,
		Init = 1,
		Download = 2,
		DeserializeStart = 3,
		Deserializing = 4,
		ConvertStart = 5,
		Converting = 6,
		Upload = 7,
		Uploading = 8,
		Complete = 9,
		Error = 10
	}
}
