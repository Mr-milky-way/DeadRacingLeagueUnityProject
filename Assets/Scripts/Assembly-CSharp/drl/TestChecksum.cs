using UnityEngine;

namespace drl
{
	public class TestChecksum : MonoBehaviour
	{
		public string filepath;

		[ContextMenu("TestChecksum")]
		public void Checksum()
		{
			MD5Crypto.CalculateChecksumAsync(filepath, delegate(string s)
			{
				Debug.Log("CHECKSUM: " + s);
			});
		}
	}
}
