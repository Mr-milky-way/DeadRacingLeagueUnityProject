using thelab.core;

public class DRLCounterUAVData : SerializedData
{
	public float x
	{
		get
		{
			return Get("vector-x", 0f);
		}
		set
		{
			Set("vector-x", value);
		}
	}

	public float y
	{
		get
		{
			return Get("vector-y", 0f);
		}
		set
		{
			Set("vector-y", value);
		}
	}

	public string mode
	{
		get
		{
			return Get("mode", "net");
		}
		set
		{
			Set("mode", value);
		}
	}

	public float duration
	{
		get
		{
			return Get("duration", 0f);
		}
		set
		{
			Set("duration", value);
		}
	}
}
