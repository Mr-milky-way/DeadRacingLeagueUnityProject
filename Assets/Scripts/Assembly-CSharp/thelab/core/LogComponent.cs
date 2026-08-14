using UnityEngine;

namespace thelab.core
{
	public class LogComponent : MonoBehaviour
	{
		public void Log(string v)
		{
			Debug.Log(v);
		}

		public void LogWarning(string v)
		{
			Debug.LogWarning(v);
		}

		public void LogError(string v)
		{
			Debug.LogError(v);
		}
	}
}
