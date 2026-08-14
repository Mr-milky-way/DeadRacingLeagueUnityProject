using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace drl
{
	public static class AESCrypto
	{
		private static bool m_initialized = false;

		[Obfuscation(Exclude = false, Feature = "default", StripAfterObfuscation = false)]
		private static string key = "09e027edfde3212431a8758576807083";

		[Obfuscation(Exclude = false, Feature = "default", StripAfterObfuscation = false)]
		private static string iv = "ZdIyKkDJEqx1/mkBr2Gwng==";

		public static byte[] Encrypt(string p_message, string ivString)
		{
			byte[] array = null;
			using Aes aes = Aes.Create();
			byte[] bytes = Encoding.UTF8.GetBytes(key);
			byte[] bytes2 = Encoding.UTF8.GetBytes(ivString);
			aes.Key = bytes;
			aes.IV = bytes2;
			ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);
			using MemoryStream memoryStream = new MemoryStream();
			using (CryptoStream stream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
			{
				using StreamWriter streamWriter = new StreamWriter(stream);
				streamWriter.Write(p_message);
			}
			return memoryStream.ToArray();
		}

		public static byte[] Encrypt(string p_message)
		{
			byte[] array = null;
			using Aes aes = Aes.Create();
			byte[] array2 = Convert.FromBase64String(key);
			byte[] iV = Convert.FromBase64String(iv);
			aes.Key = array2;
			aes.IV = iV;
			ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);
			using MemoryStream memoryStream = new MemoryStream();
			using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
				{
					streamWriter.Write(p_message);
					streamWriter.Flush();
					streamWriter.Close();
				}
				cryptoStream.Flush();
				cryptoStream.Close();
			}
			array = memoryStream.ToArray();
			memoryStream.Flush();
			memoryStream.Close();
			return array;
		}

		public static string Decrypt(byte[] p_message, string ivBase64)
		{
			string text = null;
			using Aes aes = Aes.Create();
			byte[] array = Convert.FromBase64String(key);
			byte[] iV = Convert.FromBase64String(ivBase64);
			aes.Key = array;
			aes.IV = iV;
			ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
			using MemoryStream stream = new MemoryStream(p_message);
			using CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
			using StreamReader streamReader = new StreamReader(stream2);
			return streamReader.ReadToEnd();
		}
	}
}
