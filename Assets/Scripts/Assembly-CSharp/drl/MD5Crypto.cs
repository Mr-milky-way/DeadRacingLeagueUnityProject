using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace drl
{
	public static class MD5Crypto
	{
		public static string CalculateChecksum(string p_file)
		{
			if (string.IsNullOrEmpty(p_file) || !File.Exists(p_file))
			{
				Debug.LogError("MD5Crypto> Valid filepath not provided.");
				return "";
			}
			string text = "";
			using MD5 mD = MD5.Create();
			using FileStream inputStream = File.OpenRead(p_file);
			return BitConverter.ToString(mD.ComputeHash(inputStream)).Replace("-", "").ToLowerInvariant();
		}

		public static async void CalculateChecksumAsync(string p_filename, Action<string> p_callback)
		{
			if (string.IsNullOrEmpty(p_filename) || !File.Exists(p_filename))
			{
				Debug.LogError("MD5Crypto> Valid filepath not provided.");
				p_callback?.Invoke("");
				return;
			}
			using MD5 md5 = MD5.Create();
			using FileStream stream = new FileStream(p_filename, FileMode.Open, FileAccess.Read, FileShare.Read, 131072, useAsync: true);
			byte[] buffer = new byte[131072];
			int num;
			while ((num = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
			{
				if (stream.Position == stream.Length)
				{
					md5.TransformFinalBlock(buffer, 0, num);
				}
				else
				{
					md5.TransformBlock(buffer, 0, num, buffer, 0);
				}
			}
			if (stream.Length == 0)
			{
				md5.TransformFinalBlock(buffer, 0, 0);
			}
			string obj = BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
			p_callback?.Invoke(obj);
		}
	}
}
