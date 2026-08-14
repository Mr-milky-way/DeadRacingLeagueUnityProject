using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;

namespace thelab.core
{
	public class Serialize
	{
		private static bool m_accept_datetime = true;

		private static JsonSerializerSettings m_date_format_stg;

		private static BinaryFormatter m_bfmt;

		private static Dictionary<Type, bool> _sstable;

		public static bool jsonParseDateTime
		{
			get
			{
				return m_accept_datetime;
			}
			set
			{
				m_accept_datetime = value;
			}
		}

		private static Dictionary<Type, bool> m_sstable
		{
			get
			{
				if (_sstable != null)
				{
					return _sstable;
				}
				_sstable = new Dictionary<Type, bool>();
				_sstable.Add(typeof(Vector2), value: true);
				_sstable.Add(typeof(Vector3), value: true);
				_sstable.Add(typeof(Vector4), value: true);
				_sstable.Add(typeof(Quaternion), value: true);
				_sstable.Add(typeof(Color), value: true);
				_sstable.Add(typeof(Rect), value: true);
				_sstable.Add(typeof(Keyframe), value: true);
				_sstable.Add(typeof(AnimationCurve), value: true);
				_sstable.Add(typeof(Color32), value: true);
				_sstable.Add(typeof(GradientColorKey), value: true);
				_sstable.Add(typeof(GradientAlphaKey), value: true);
				_sstable.Add(typeof(Gradient), value: true);
				_sstable.Add(typeof(Bounds), value: true);
				return _sstable;
			}
		}

		public static T FromBase64<T>(string p_data, Encoding p_encoding)
		{
			byte[] array = Convert.FromBase64String(p_data);
			if (typeof(T) == typeof(string))
			{
				return (T)(object)((p_encoding == null) ? Encoding.UTF8 : p_encoding).GetString(array);
			}
			if (typeof(T) == typeof(byte[]))
			{
				return (T)(object)array;
			}
			return FromBytes<T>(array);
		}

		public static T FromBase64<T>(string p_data)
		{
			return FromBase64<T>(p_data, null);
		}

		public static byte[] FromBase64(string p_data)
		{
			return Convert.FromBase64String(p_data);
		}

		public static string ToBase64(object p_data, Encoding p_encoding)
		{
			return Convert.ToBase64String(ToBytes(p_data, p_encoding));
		}

		public static string ToBase64(object p_data)
		{
			return ToBase64(p_data, null);
		}

		public static bool IsSerializable(Type p_type)
		{
			if (p_type == null)
			{
				return false;
			}
			if (m_sstable.ContainsKey(p_type))
			{
				return m_sstable[p_type];
			}
			return p_type.IsSerializable;
		}

		public static bool IsSerializable<T>()
		{
			return IsSerializable(typeof(T));
		}

		public static string BytesToASCII(byte[] p_data)
		{
			if (p_data == null)
			{
				return "";
			}
			if (p_data.Length == 0)
			{
				return "";
			}
			return Encoding.ASCII.GetString(p_data);
		}

		public static string BytesToUTF8(byte[] p_data)
		{
			if (p_data == null)
			{
				return "";
			}
			if (p_data.Length == 0)
			{
				return "";
			}
			return Encoding.UTF8.GetString(p_data);
		}

		public static T FromStream<T>(string p_fileLocation, bool p_unsafe)
		{
			if (string.IsNullOrEmpty(p_fileLocation) || !File.Exists(p_fileLocation))
			{
				Debug.Log("Serialize> FromStream missing file location.");
				return default(T);
			}
			BinaryFormatter binaryFormatter = GetBinaryFormatter();
			using Stream stream = new FileStream(p_fileLocation, FileMode.Open);
			if (stream.CanSeek)
			{
				stream.Position = 0L;
			}
			object obj = null;
			if (p_unsafe)
			{
				obj = binaryFormatter.Deserialize(stream);
			}
			else
			{
				try
				{
					obj = binaryFormatter.Deserialize(stream);
				}
				catch (Exception ex)
				{
					Debug.LogWarning("Serialize> Error [" + ex.Message + "] length[" + ((stream == null) ? "" : stream.Length.ToString()) + "]");
				}
			}
			T result = (T)obj;
			stream.Close();
			stream.Dispose();
			new FileInfo(p_fileLocation).Delete();
			return result;
		}

		public static T FromBytes<T>(byte[] p_data, Encoding p_encoding, bool p_unsafe)
		{
			T result = default(T);
			if (typeof(T) == typeof(string))
			{
				Encoding encoding = ((p_encoding == null) ? Encoding.UTF8 : p_encoding);
				try
				{
					result = (T)(object)encoding.GetString(p_data);
					return result;
				}
				catch (Exception ex)
				{
					Debug.LogWarning("Serialize> Error [" + ex.Message + "]");
				}
				return result;
			}
			using MemoryStream memoryStream = new MemoryStream(p_data);
			BinaryFormatter binaryFormatter = GetBinaryFormatter();
			memoryStream.Position = 0L;
			object obj = null;
			if (p_unsafe)
			{
				obj = binaryFormatter.Deserialize(memoryStream);
			}
			else
			{
				try
				{
					obj = binaryFormatter.Deserialize(memoryStream);
				}
				catch (Exception ex2)
				{
					Debug.LogWarning("Serialize> Error [" + ex2.Message + "] length[" + ((p_data == null) ? "" : p_data.Length.ToString()) + "]");
				}
			}
			result = (T)obj;
			memoryStream.Close();
			memoryStream.Dispose();
			return result;
		}

		public static T FromBytes<T>(byte[] p_data, Encoding p_encoding)
		{
			return FromBytes<T>(p_data, p_encoding, p_unsafe: false);
		}

		public static T FromBytes<T>(byte[] p_data, bool p_unsafe)
		{
			return FromBytes<T>(p_data, null, p_unsafe);
		}

		public static T FromBytes<T>(byte[] p_data)
		{
			return FromBytes<T>(p_data, null, p_unsafe: false);
		}

		public static byte[] ToBytes(object p_data, Encoding p_encoding)
		{
			if (p_data == null)
			{
				return new byte[0];
			}
			if (p_data.GetType() == typeof(byte[]))
			{
				return (byte[])p_data;
			}
			if (p_data.GetType() == typeof(string))
			{
				return ((p_encoding == null) ? Encoding.UTF8 : p_encoding).GetBytes((string)p_data);
			}
			if (!IsSerializable(p_data.GetType()))
			{
				Debug.LogWarning("Serialization> Data [" + p_data?.ToString() + "] not serializable!");
				return new byte[0];
			}
			using MemoryStream memoryStream = new MemoryStream();
			GetBinaryFormatter().Serialize(memoryStream, p_data);
			byte[] result = memoryStream.ToArray();
			memoryStream.Close();
			return result;
		}

		public static void ToBytes(object p_data, Stream p_stream, bool p_close = true)
		{
			if (p_data == null)
			{
				return;
			}
			if (!IsSerializable(p_data.GetType()))
			{
				Debug.LogWarning("Serialization> Data [" + p_data?.ToString() + "] not serializable!");
				return;
			}
			GetBinaryFormatter().Serialize(p_stream, p_data);
			p_stream.Flush();
			if (p_close)
			{
				p_stream.Close();
			}
		}

		public static byte[] ToBytes(object p_data)
		{
			return ToBytes(p_data, (Encoding)null);
		}

		public static void EncryptXOR(byte p_mask, Stream p_data)
		{
			byte[] array = new byte[4096];
			while (p_data.Position < p_data.Length)
			{
				long num = Math.Min(array.Length, p_data.Length - p_data.Position);
				if (num <= 0)
				{
					break;
				}
				p_data.Read(array, 0, (int)num);
				EncryptXOR(p_mask, array, 0, (int)num);
				p_data.Seek(-num, SeekOrigin.Current);
				p_data.Write(array, 0, (int)num);
			}
			p_data.Flush();
		}

		public static void EncryptXOR(byte p_mask, byte[] p_data, int p_offset = 0, int p_count = 0)
		{
			int num = Math.Min(p_count, p_data.Length);
			for (int i = 0; i < num; i++)
			{
				p_data[i + p_offset] = (byte)(p_data[i + p_offset] ^ p_mask);
			}
		}

		public static T FromJson<T>(string p_data, T p_instance = default(T), bool p_populate = false)
		{
			if (m_date_format_stg == null)
			{
				m_date_format_stg = new JsonSerializerSettings();
			}
			m_date_format_stg.DateParseHandling = (m_accept_datetime ? DateParseHandling.DateTime : DateParseHandling.None);
			if (p_instance == null)
			{
				return JsonConvert.DeserializeObject<T>(p_data, m_date_format_stg);
			}
			if (p_populate)
			{
				JsonConvert.PopulateObject(p_data, p_instance, m_date_format_stg);
				return p_instance;
			}
			return JsonConvert.DeserializeAnonymousType(p_data, p_instance, m_date_format_stg);
		}

		public static T FromJson<T>(Stream p_data, T p_instance = default(T), bool p_populate = false)
		{
			if (m_date_format_stg == null)
			{
				m_date_format_stg = new JsonSerializerSettings();
			}
			m_date_format_stg.DateParseHandling = (m_accept_datetime ? DateParseHandling.DateTime : DateParseHandling.None);
			JsonSerializer jsonSerializer = JsonSerializer.Create(m_date_format_stg);
			StreamReader reader = new StreamReader(p_data);
			if (p_instance == null)
			{
				return (T)jsonSerializer.Deserialize(reader, typeof(T));
			}
			if (p_populate)
			{
				jsonSerializer.Populate(reader, p_instance);
				return p_instance;
			}
			return default(T);
		}

		public static string ToJson(object p_data, bool p_indented = false)
		{
			return JsonConvert.SerializeObject(p_data, p_indented ? Formatting.Indented : Formatting.None);
		}

		public static T FromPrefs<T>(string p_key)
		{
			T result = default(T);
			if (!PlayerPrefs.HasKey(p_key))
			{
				return result;
			}
			string text = PlayerPrefs.GetString(p_key);
			if (string.IsNullOrEmpty(text))
			{
				return result;
			}
			try
			{
				result = FromBase64<T>(text);
				return result;
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Serialize> Failed to Parse Prefs - key[" + p_key + "]");
				ex.GetHashCode();
			}
			return result;
		}

		public static string ToPrefs(string p_key, object p_data)
		{
			if (p_data == null)
			{
				PlayerPrefs.DeleteKey(p_key);
				return "";
			}
			string text = ToBase64(p_data);
			PlayerPrefs.SetString(p_key, text);
			return text;
		}

		public static byte[] WriteBytes(string p_path, object p_data)
		{
			byte[] array = ToBytes(p_data);
			if (array == null)
			{
				return null;
			}
			File.WriteAllBytes(p_path, array);
			return array;
		}

		public static string WriteJson(string p_path, object p_data, bool p_indented = false)
		{
			string text = ToJson(p_data, p_indented);
			File.WriteAllText(p_path, text);
			return text;
		}

		public static string WriteBase64(string p_path, object p_data)
		{
			string text = ToBase64(p_data);
			File.WriteAllText(p_path, text);
			return text;
		}

		public static Thread WriteBytesAsync(string p_path, object p_data, Action<byte[]> p_callback = null)
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				byte[] obj = WriteBytes(p_path, p_data);
				if (p_callback != null)
				{
					p_callback(obj);
				}
			});
			thread.Start();
			return thread;
		}

		public static Thread WriteJsonAsync(string p_path, object p_data, bool p_indented, Action<string> p_callback = null)
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				string obj = WriteJson(p_path, p_data, p_indented);
				if (p_callback != null)
				{
					p_callback(obj);
				}
			});
			thread.Start();
			return thread;
		}

		public static Thread WriteBase64Async(string p_path, object p_data, Action<string> p_callback = null)
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				string obj = WriteBase64(p_path, p_data);
				if (p_callback != null)
				{
					p_callback(obj);
				}
			});
			thread.Start();
			return thread;
		}

		public static T ReadBytes<T>(string p_path)
		{
			if (!File.Exists(p_path))
			{
				return default(T);
			}
			byte[] p_data = File.ReadAllBytes(p_path);
			T result = default(T);
			try
			{
				result = FromBytes<T>(p_data);
				return result;
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Serialize> Failed to read file [" + p_path + "][" + typeof(T).Name + "]\n" + ex.Message);
			}
			return result;
		}

		public static T ReadJson<T>(string p_path)
		{
			if (!File.Exists(p_path))
			{
				return default(T);
			}
			string p_data = File.ReadAllText(p_path);
			T result = default(T);
			try
			{
				result = FromJson<T>(p_data);
				return result;
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Serialize> Failed to read file [" + p_path + "][" + typeof(T).Name + "]\n" + ex.Message);
			}
			return result;
		}

		public static T ReadBase64<T>(string p_path)
		{
			if (!File.Exists(p_path))
			{
				return default(T);
			}
			string p_data = File.ReadAllText(p_path);
			T result = default(T);
			try
			{
				result = FromJson<T>(p_data);
				return result;
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Serialize> Failed to read file [" + p_path + "][" + typeof(T).Name + "]\n" + ex.Message);
			}
			return result;
		}

		public static Thread ReadBytesAsync<T>(string p_path, Action<T> p_callback = null)
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				T obj = ReadBytes<T>(p_path);
				if (p_callback != null)
				{
					p_callback(obj);
				}
			});
			thread.Start();
			return thread;
		}

		public static Thread ReadJsonAsync<T>(string p_path, Action<T> p_callback = null)
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				T obj = ReadJson<T>(p_path);
				if (p_callback != null)
				{
					p_callback(obj);
				}
			});
			thread.Start();
			return thread;
		}

		public static Thread ReadBase64Async<T>(string p_path, Action<T> p_callback = null)
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				T obj = ReadBase64<T>(p_path);
				if (p_callback != null)
				{
					p_callback(obj);
				}
			});
			thread.Start();
			return thread;
		}

		protected static byte[] GZipIO(byte[] p_data, bool p_compress, int p_buffer_size = 1024)
		{
			if (p_data == null)
			{
				return null;
			}
			byte[] array = null;
			CompressionMode mode = (p_compress ? CompressionMode.Compress : CompressionMode.Decompress);
			MemoryStream memoryStream = (p_compress ? new MemoryStream() : new MemoryStream(p_data));
			GZipStream gZipStream = new GZipStream(memoryStream, mode, leaveOpen: false);
			if (p_compress)
			{
				gZipStream.Write(p_data, 0, p_data.Length);
				gZipStream.Close();
				gZipStream.Dispose();
				array = memoryStream.ToArray();
			}
			else
			{
				MemoryStream memoryStream2 = new MemoryStream();
				byte[] array2 = new byte[p_buffer_size];
				int num = 0;
				while (true)
				{
					num = gZipStream.Read(array2, 0, array2.Length);
					if (num <= 0)
					{
						break;
					}
					memoryStream2.Write(array2, 0, num);
				}
				array = memoryStream2.ToArray();
				memoryStream2.Close();
				memoryStream2.Dispose();
				gZipStream.Close();
				gZipStream.Dispose();
			}
			try
			{
				memoryStream.Close();
				memoryStream.Dispose();
			}
			catch (Exception)
			{
			}
			return array;
		}

		public static byte[] ToGzip(object p_data)
		{
			if (p_data is byte[])
			{
				return GZipIO((byte[])p_data, p_compress: true);
			}
			return ToGzip(ToBytes(p_data));
		}

		public static byte[] FromGZip(byte[] p_data, int p_buffer_size = 1024)
		{
			return GZipIO(p_data, p_compress: false, p_buffer_size);
		}

		public static T FromGZip<T>(byte[] p_data, int p_buffer_size = 1024)
		{
			return FromBytes<T>(FromGZip(p_data, p_buffer_size));
		}

		public static BinaryAssertResult<T> BinaryAssert<T>(T v, byte[] d, T vset)
		{
			BinaryAssertResult<T> binaryAssertResult = new BinaryAssertResult<T>();
			if (vset != null)
			{
				binaryAssertResult.instance = vset;
				binaryAssertResult.data = ToBytes(vset);
				return binaryAssertResult;
			}
			bool flag = d != null && d.Length != 0;
			binaryAssertResult.instance = v;
			binaryAssertResult.data = d;
			if (flag && v != null)
			{
				return binaryAssertResult;
			}
			if (v == null)
			{
				v = (flag ? FromBytes<T>(d) : Reflection<object>.New<T>(Array.Empty<object>()));
			}
			else
			{
				d = ToBytes(v);
			}
			binaryAssertResult.instance = v;
			binaryAssertResult.data = d;
			return binaryAssertResult;
		}

		public static BinaryAssertResult<T> BinaryAssert<T>(T v, byte[] d)
		{
			return BinaryAssert(v, d, default(T));
		}

		public static T Instantiate<T>(T p_original)
		{
			return FromBytes<T>(ToBytes(p_original));
		}

		public static T Instantiate<T>()
		{
			Type typeFromHandle = typeof(T);
			return (T)(typeFromHandle.IsValueType ? Activator.CreateInstance(typeFromHandle) : ((typeFromHandle == typeof(string)) ? "" : null));
		}

		public static object Instantiate(Type p_type)
		{
			if (p_type == null)
			{
				return null;
			}
			if (!p_type.IsValueType)
			{
				if (!(p_type == typeof(string)))
				{
					return null;
				}
				return "";
			}
			return Activator.CreateInstance(p_type);
		}

		public static string ToMD5(byte[] p_data)
		{
			byte[] array = MD5.Create().ComputeHash(p_data);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}

		public static string ToMD5(string p_data)
		{
			return ToMD5(Encoding.ASCII.GetBytes(p_data));
		}

		public static string ToSHA1(byte[] p_data)
		{
			byte[] array = SHA1.Create().ComputeHash(p_data);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}

		public static string ToSHA1(string p_data)
		{
			return ToSHA1(Encoding.ASCII.GetBytes(p_data));
		}

		public static byte[] EncodeBytesDES(byte[] p_data, string p_password, int p_iterations = 1000)
		{
			DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
			dESCryptoServiceProvider.GenerateIV();
			byte[] bytes = new Rfc2898DeriveBytes(p_password, dESCryptoServiceProvider.IV, p_iterations).GetBytes(8);
			MemoryStream memoryStream = new MemoryStream();
			ICryptoTransform transform = dESCryptoServiceProvider.CreateEncryptor(bytes, dESCryptoServiceProvider.IV);
			CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
			memoryStream.Write(dESCryptoServiceProvider.IV, 0, dESCryptoServiceProvider.IV.Length);
			string s = ToBase64(p_data);
			byte[] bytes2 = Encoding.ASCII.GetBytes(s);
			cryptoStream.Write(bytes2, 0, bytes2.Length);
			cryptoStream.FlushFinalBlock();
			return memoryStream.ToArray();
		}

		public static byte[] DecodeBytesDES(byte[] p_data, string p_password, int p_iterations = 1000)
		{
			byte[] result = new byte[0];
			if (p_data == null)
			{
				return result;
			}
			if (string.IsNullOrEmpty(p_password))
			{
				p_password = "";
			}
			try
			{
				MemoryStream memoryStream = new MemoryStream(p_data);
				DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
				byte[] array = new byte[8];
				memoryStream.Read(array, 0, array.Length);
				byte[] bytes = new Rfc2898DeriveBytes(p_password, array, p_iterations).GetBytes(8);
				ICryptoTransform transform = dESCryptoServiceProvider.CreateDecryptor(bytes, array);
				result = FromBase64(new StreamReader(new CryptoStream(memoryStream, transform, CryptoStreamMode.Read)).ReadToEnd());
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Serialize> Failed to DecodeDES bytes. " + ex.ToString() + "\n" + ex.StackTrace);
			}
			return result;
		}

		public static T DecodeBytesDES<T>(byte[] p_data, string p_password, int p_iterations = 1000)
		{
			return FromBytes<T>(DecodeBytesDES(p_data, p_password, p_iterations));
		}

		public static byte[] EncodeDES(object p_data, string p_password, int p_iterations = 1000)
		{
			return EncodeBytesDES(ToBytes(p_data), p_password, p_iterations);
		}

		public static string EncodeDESToBase64(object p_data, string p_password, int p_iterations = 1000)
		{
			return ToBase64(EncodeDES(p_data, p_password, p_iterations));
		}

		public static byte[] DecodeDESFromBase64(string p_data, string p_password, int p_iterations = 1000)
		{
			return DecodeBytesDES(FromBase64(p_data), p_password, p_iterations);
		}

		public static T DecodeDESFromBase64<T>(string p_data, string p_password, int p_iterations = 1000)
		{
			return DecodeBytesDES<T>(FromBase64(p_data), p_password, p_iterations);
		}

		public static List<object> ToObjectList(IList p_list)
		{
			List<object> list = new List<object>();
			if (p_list == null)
			{
				return list;
			}
			for (int i = 0; i < p_list.Count; i++)
			{
				list.Add(p_list[i]);
			}
			return list;
		}

		public static Dictionary<string, string> ToStringHash(params object[] p_args)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (p_args.Length <= 1)
			{
				return dictionary;
			}
			for (int i = 1; i < p_args.Length; i += 2)
			{
				object obj = p_args[i - 1];
				object obj2 = p_args[i];
				if (obj != null)
				{
					string key = obj.ToString();
					string value = ((obj2 == null) ? "[null]" : obj2.ToString());
					dictionary[key] = value;
				}
			}
			return dictionary;
		}

		private static string[] FromStringHash(Dictionary<string, string> p_hash)
		{
			List<string> list = new List<string>();
			if (p_hash == null)
			{
				return list.ToArray();
			}
			foreach (KeyValuePair<string, string> item in p_hash)
			{
				string key = item.Key;
				string value = item.Value;
				list.Add(key);
				list.Add(value);
			}
			return list.ToArray();
		}

		public static string ToQueryString(params object[] p_args)
		{
			string text = "";
			if (p_args.Length <= 1)
			{
				return text;
			}
			for (int i = 1; i < p_args.Length; i += 2)
			{
				if (i > 1)
				{
					text += "&";
				}
				object obj = p_args[i - 1];
				object obj2 = p_args[i];
				if (obj != null)
				{
					string text2 = obj.ToString();
					string text3 = ((obj2 == null) ? "[null]" : ((obj2 is bool) ? obj2.ToString().ToLower() : obj2.ToString()));
					text = text + text2 + "=" + text3;
				}
			}
			return text;
		}

		public static string ToQueryString(IDictionary p_hash)
		{
			string text = "";
			if (p_hash == null)
			{
				return text;
			}
			int num = 0;
			foreach (object key in p_hash.Keys)
			{
				if (key != null)
				{
					object obj = p_hash[key];
					if (num > 0)
					{
						text += "&";
					}
					text = text + key?.ToString() + "=" + ((obj == null) ? "" : ((obj is bool) ? obj.ToString().ToLower() : obj.ToString()));
					num++;
				}
			}
			return text;
		}

		public static BinaryFormatter GetBinaryFormatter()
		{
			if (m_bfmt != null)
			{
				return m_bfmt;
			}
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			SurrogateSelector surrogateSelector = new SurrogateSelector();
			surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), new Vector2SerializationSurrogate());
			surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), new Vector3SerializationSurrogate());
			surrogateSelector.AddSurrogate(typeof(Vector4), new StreamingContext(StreamingContextStates.All), new Vector4SerializationSurrogate());
			surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), new QuaternionSerializationSurrogate());
			surrogateSelector.AddSurrogate(typeof(Color), new StreamingContext(StreamingContextStates.All), new ColorSerializationSurrogate());
			surrogateSelector.AddSurrogate(typeof(Rect), new StreamingContext(StreamingContextStates.All), new RectSerializationSurrogate());
			surrogateSelector.AddSurrogate(typeof(Keyframe), new StreamingContext(StreamingContextStates.All), new KeyframeSerializationSurrogate());
			surrogateSelector.AddSurrogate(typeof(AnimationCurve), new StreamingContext(StreamingContextStates.All), new AnimationCurveSerializationSurrogate());
			surrogateSelector.AddSurrogate(typeof(Color32), new StreamingContext(StreamingContextStates.All), new Color32SerializationSurrogate());
			surrogateSelector.AddSurrogate(typeof(GradientColorKey), new StreamingContext(StreamingContextStates.All), new GradientColorKeySerializationSurrogate());
			surrogateSelector.AddSurrogate(typeof(GradientAlphaKey), new StreamingContext(StreamingContextStates.All), new GradientAlphaKeySerializationSurrogate());
			surrogateSelector.AddSurrogate(typeof(Gradient), new StreamingContext(StreamingContextStates.All), new GradientSerializationSurrogate());
			surrogateSelector.AddSurrogate(typeof(Bounds), new StreamingContext(StreamingContextStates.All), new BoundsSerializationSurrogate());
			binaryFormatter.SurrogateSelector = surrogateSelector;
			m_bfmt = binaryFormatter;
			return binaryFormatter;
		}
	}
}
