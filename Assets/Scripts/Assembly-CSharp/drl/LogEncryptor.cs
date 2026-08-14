using System.IO;
using System.Text;
using UnityEngine;
using drl.game;

namespace drl
{
	public class LogEncryptor : MonoBehaviour
	{
		private StringBuilder stringBuilder = new StringBuilder();

		private async void OnEnable()
		{
			Application.logMessageReceived += HandleLog;
			string path = DRLPaths.Tools.playerLogFile.Replace(".log", "-prev.log");
			if (File.Exists(path))
			{
				string text = File.ReadAllText(path);
				if (text != null)
				{
					byte[] bytes = AESCrypto.Encrypt(text);
					File.WriteAllBytes(path, bytes);
				}
			}
		}

		private async void OnDisable()
		{
			Application.logMessageReceived -= HandleLog;
			byte[] bytes = AESCrypto.Encrypt(stringBuilder.ToString());
			File.WriteAllBytes(DRLPaths.Tools.playerLogFile, bytes);
			stringBuilder = null;
		}

		private void HandleLog(string logString, string stackTrace, LogType type)
		{
			if (!string.IsNullOrEmpty(logString))
			{
				stringBuilder.Append(logString + "\n");
			}
		}
	}
}
