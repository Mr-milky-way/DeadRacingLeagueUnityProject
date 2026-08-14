using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class SerializedData : Dictionary<string, object>
	{
		private static System.Random float_encode_rnd = new System.Random(16711422);

		public static SerializedData FromBytes(byte[] p_data, SerializedData p_target = null)
		{
			SerializedData serializedData = ((p_target == null) ? new SerializedData() : p_target);
			Dictionary<string, object> dictionary = null;
			try
			{
				dictionary = Serialize.FromBytes<Dictionary<string, object>>(p_data);
				serializedData.Set(dictionary);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("SerializedData> Failed to parse Bytes!\n" + ex.Message);
				return null;
			}
			serializedData.Set(dictionary);
			return serializedData;
		}

		public static T FromJson<T>(string p_data, T p_target = null) where T : SerializedData, new()
		{
			T val = ((p_target == null) ? new T() : p_target);
			Dictionary<string, object> dictionary = null;
			try
			{
				dictionary = Serialize.FromJson<Dictionary<string, object>>(p_data);
				val.Set(dictionary);
				return val;
			}
			catch (Exception ex)
			{
				Debug.LogWarning("SerializedData> Failed to parse Json!\n" + ex.Message);
				return null;
			}
		}

		public static SerializedData FromBase64(string p_data, SerializedData p_target = null)
		{
			SerializedData serializedData = ((p_target == null) ? new SerializedData() : p_target);
			Dictionary<string, object> dictionary = null;
			try
			{
				dictionary = Serialize.FromBase64<Dictionary<string, object>>(p_data);
				serializedData.Set(dictionary);
				return serializedData;
			}
			catch (Exception ex)
			{
				Debug.LogWarning("SerializedData> Failed to parse Base64!\n" + ex.Message);
				return null;
			}
		}

		public static ulong FloatEncode(float v, float a, float b, byte shf = 0, byte g1 = 0, byte g2 = 0)
		{
			double num = v;
			double num2 = (((double)Math.Abs(b - a) <= 1E-13) ? 0.0 : (1.0 / (double)(b - a)));
			num = (num - (double)a) * num2;
			if (num < 0.0)
			{
				num = 0.0;
			}
			if (num > 1.0)
			{
				num = 1.0;
			}
			ulong num3 = ulong.MaxValue;
			num3 >>= (int)shf;
			num *= (double)num3;
			return ((ulong)num << (int)shf) + (ulong)float_encode_rnd.Next(g1, g2);
		}

		public static float FloatDecode(ulong v, float a, float b, byte shf = 0)
		{
			ulong num = v >> (int)shf;
			ulong num2 = ulong.MaxValue;
			num2 >>= (int)shf;
			double num3 = num;
			num3 /= (double)num2;
			if (num3 < 0.0)
			{
				num3 = 0.0;
			}
			if (num3 > 1.0)
			{
				num3 = 1.0;
			}
			num3 = (double)a + num3 * (double)(b - a);
			return (float)num3;
		}

		public T Get<T>(string k, T d)
		{
			if (ContainsKey(k))
			{
				return Reflection<object>.AssertCast<T>(base[k]);
			}
			return d;
		}

		public T GetCast<T>(string k, T d, bool p_add = false)
		{
			object obj = Get(k, (object)d);
			if (obj is T)
			{
				return (T)obj;
			}
			if (obj is JArray)
			{
				obj = (obj as JArray).ToObject<T>();
			}
			if (obj is JObject)
			{
				obj = (obj as JObject).ToObject<T>();
			}
			if (obj == null)
			{
				obj = d;
			}
			if (!(obj is T))
			{
				obj = d;
			}
			if (p_add)
			{
				Set(k, obj);
			}
			return (T)obj;
		}

		public T Get<T>(string k)
		{
			return Get(k, default(T));
		}

		public virtual void Set(string k, object v)
		{
			base[k] = v;
		}

		public void Set(int p, string k, object v)
		{
			Set(k + "[" + p + "]", v);
		}

		public void SetWebArray(string k, object[] v)
		{
			if (v != null)
			{
				for (int i = 0; i < v.Length; i++)
				{
					Set(i, k, v[i]);
				}
			}
		}

		public void SetWebArray<T>(string k, T[] v)
		{
			if (v != null)
			{
				for (int i = 0; i < v.Length; i++)
				{
					Set(i, k, v[i]);
				}
			}
		}

		public void SetVector2(string k, Vector2 v)
		{
			Set(k, new float[2] { v.x, v.y });
		}

		public Vector2 GetVector2(string k, Vector2 d)
		{
			float[] cast = GetCast<float[]>(k, null);
			if (cast != null)
			{
				return new Vector2(cast[0], cast[1]);
			}
			return d;
		}

		public void SetVector3(string k, Vector3 v)
		{
			Set(k, new float[3] { v.x, v.y, v.z });
		}

		public Vector3 GetVector3(string k, Vector3 d)
		{
			float[] cast = GetCast<float[]>(k, null);
			if (cast != null)
			{
				return new Vector3(cast[0], cast[1], cast[2]);
			}
			return d;
		}

		public void SetVector4(string k, Vector4 v)
		{
			Set(k, new float[4] { v.x, v.y, v.z, v.w });
		}

		public Vector4 GetVector4(string k, Vector4 d)
		{
			float[] cast = GetCast<float[]>(k, null);
			if (cast != null)
			{
				return new Vector4(cast[0], cast[1], cast[2], cast[3]);
			}
			return d;
		}

		public void SetQuaternion(string k, Quaternion v)
		{
			Set(k, new float[4] { v.x, v.y, v.z, v.w });
		}

		public Quaternion GetQuaternion(string k, Quaternion d)
		{
			float[] cast = GetCast<float[]>(k, null);
			if (cast != null)
			{
				return new Quaternion(cast[0], cast[1], cast[2], cast[3]);
			}
			return d;
		}

		public void SetColor(string k, Color v)
		{
			Set(k, new float[4] { v.r, v.g, v.b, v.a });
		}

		public Color GetColor(string k, Color d)
		{
			float[] cast = GetCast<float[]>(k, null);
			if (cast != null)
			{
				return new Color(cast[0], cast[1], cast[2], cast[3]);
			}
			return d;
		}

		public void SetColorARGBHex(string k, Color v)
		{
			Set(k, Colorf.ToARGBHex(v));
		}

		public Color GetColorARGBHex(string k, Color d)
		{
			string text = Get<string>(k, null);
			if (!string.IsNullOrEmpty(text))
			{
				return Colorf.ParseARGB(text, d);
			}
			return d;
		}

		public void SetColorRGBHex(string k, Color v)
		{
			Set(k, Colorf.ToARGBHex(v));
		}

		public Color GetColorRGBHex(string k, Color d)
		{
			string text = Get<string>(k, null);
			if (!string.IsNullOrEmpty(text))
			{
				return Colorf.ParseRGB(text, d);
			}
			return d;
		}

		public void SetColorARGB(string k, Color v)
		{
			Set(k, Colorf.ColorToARGB(v));
		}

		public Color GetColorARGB(string k, Color d)
		{
			return Colorf.ARGBToColor(Get(k, Colorf.ColorToARGB(d)));
		}

		public void SetColorRGB(string k, Color v)
		{
			Set(k, Colorf.ColorToRGB(v));
		}

		public Color GetColorRGB(string k, Color d)
		{
			return Colorf.RGBToColor(Get(k, Colorf.ColorToRGB(d)));
		}

		public void SetVector2Int(string k, Vector2Int v)
		{
			Set(k, new int[2] { v.x, v.y });
		}

		public Vector2Int GetVector2Int(string k, Vector2Int d)
		{
			int[] cast = GetCast<int[]>(k, null);
			if (cast != null)
			{
				return new Vector2Int(cast[0], cast[1]);
			}
			return d;
		}

		public void SetVector3Int(string k, Vector3Int v)
		{
			Set(k, new int[3] { v.x, v.y, v.z });
		}

		public Vector3Int GetVector3Int(string k, Vector3Int d)
		{
			int[] cast = GetCast<int[]>(k, null);
			if (cast != null)
			{
				return new Vector3Int(cast[0], cast[1], cast[2]);
			}
			return d;
		}

		public void Set(object[] p_pairs)
		{
			if (p_pairs == null || p_pairs.Length <= 1)
			{
				return;
			}
			for (int i = 1; i < p_pairs.Length; i++)
			{
				object obj = p_pairs[i - 1];
				object obj2 = p_pairs[i];
				if (obj != null)
				{
					string k = obj.ToString();
					if (obj2 is Vector2)
					{
						SetVector2(k, (Vector2)obj2);
					}
					else if (obj2 is Vector3)
					{
						SetVector3(k, (Vector3)obj2);
					}
					else if (obj2 is Vector4)
					{
						SetVector4(k, (Vector4)obj2);
					}
					else if (obj2 is Color)
					{
						SetColor(k, (Color)obj2);
					}
					else if (obj2 is Quaternion)
					{
						SetQuaternion(k, (Quaternion)obj2);
					}
					else if (obj2 is Vector2Int)
					{
						SetVector2Int(k, (Vector2Int)obj2);
					}
					else if (obj2 is Vector3Int)
					{
						SetVector3Int(k, (Vector3Int)obj2);
					}
					else
					{
						Set(k, obj2);
					}
				}
			}
		}

		public void Set(Dictionary<string, object> p_data, bool p_clear)
		{
			if (p_clear)
			{
				Clear();
			}
			if (p_data == null)
			{
				return;
			}
			foreach (KeyValuePair<string, object> p_datum in p_data)
			{
				Set(p_datum.Key, p_datum.Value);
			}
			RefreshCached();
		}

		public void Set(Dictionary<string, object> p_data)
		{
			Set(p_data, p_clear: true);
		}

		public void Match(Dictionary<string, object> p_data)
		{
			if (p_data == null)
			{
				return;
			}
			foreach (KeyValuePair<string, object> p_datum in p_data)
			{
				if (ContainsKey(p_datum.Key))
				{
					Set(p_datum.Key, p_datum.Value);
				}
			}
			RefreshCached();
		}

		public bool Set(byte[] p_data, bool p_clear)
		{
			Dictionary<string, object> dictionary = null;
			try
			{
				dictionary = Serialize.FromBytes<Dictionary<string, object>>(p_data);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("SerializedData> Failed to parse! " + ex.Message);
			}
			if (dictionary != null)
			{
				Set(dictionary, p_clear);
			}
			return dictionary != null;
		}

		public bool Set(byte[] p_data)
		{
			return Set(p_data, p_clear: true);
		}

		public bool Set(TextAsset p_data)
		{
			return Set(p_data.bytes);
		}

		public virtual void RefreshCached()
		{
		}

		public byte[] ToBytes()
		{
			RefreshStored();
			return Serialize.ToBytes(new Dictionary<string, object>(this));
		}

		public string ToBase64()
		{
			RefreshStored();
			return Serialize.ToBase64(new Dictionary<string, object>(this));
		}

		public string ToJson(bool p_indented = false)
		{
			RefreshStored();
			return Serialize.ToJson(new Dictionary<string, object>(this), p_indented);
		}

		public Dictionary<string, string> ToHashTable()
		{
			RefreshStored();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, object> current = enumerator.Current;
				string key = current.Key;
				object value = current.Value;
				if (value != null)
				{
					dictionary[key] = ((value is bool) ? value.ToString().ToLower() : value.ToString());
				}
			}
			return dictionary;
		}

		public WWWForm ToForm()
		{
			RefreshStored();
			WWWForm wWWForm = new WWWForm();
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, object> current = enumerator.Current;
				string key = current.Key;
				object value = current.Value;
				if (value != null && !string.IsNullOrEmpty(key))
				{
					if (value is byte[])
					{
						wWWForm.AddBinaryData(key, value as byte[]);
					}
					else
					{
						wWWForm.AddField(key, value.ToString());
					}
				}
			}
			return wWWForm;
		}

		public override string ToString()
		{
			RefreshStored();
			string text = "";
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, object> current = enumerator.Current;
				if (!string.IsNullOrEmpty(text))
				{
					text += "\n";
				}
				text = text + current.Key + ": " + ((current.Value == null) ? "" : current.Value.ToString());
			}
			return text;
		}

		public virtual void RefreshStored()
		{
		}
	}
}
