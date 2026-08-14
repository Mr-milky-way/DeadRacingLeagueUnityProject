using System;
using UnityEngine;

public static class D
{
	public static void Log(string p_message, bool p_devOnly = true)
	{
		if (!p_devOnly)
		{
			Debug.Log(p_message);
		}
	}

	public static void Warning(string p_message, bool p_devOnly = true)
	{
		if (!p_devOnly)
		{
			Debug.LogWarning(p_message);
		}
	}

	public static void Error(string p_message, bool p_devOnly = true)
	{
		if (!p_devOnly)
		{
			Debug.LogError(p_message);
		}
	}

	public static void Exception(Exception p_exception, bool p_devOnly = true)
	{
		if (!p_devOnly)
		{
			Debug.LogException(p_exception);
		}
	}
}
