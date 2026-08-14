using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace thelab.core
{
	public class Reflection : Reflection<object>
	{
	}
	public class Reflection<T>
	{
		private static BindingFlags m_flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		private static Assembly m_unityEngineAssembly;

		private static Assembly m_unityEditorAssembly;

		private static Assembly m_clientEngineAssembly;

		private static Assembly m_editorEngineAssembly;

		public static Assembly unityEngineAssembly
		{
			get
			{
				if (m_unityEngineAssembly != null)
				{
					return m_unityEngineAssembly;
				}
				return m_unityEngineAssembly = Assembly.Load("UnityEngine");
			}
		}

		public static Assembly unityEditorAssembly
		{
			get
			{
				if (m_unityEditorAssembly != null)
				{
					return m_unityEditorAssembly;
				}
				return m_unityEditorAssembly = Assembly.Load("UnityEditor");
			}
		}

		public static Assembly clientAssembly
		{
			get
			{
				if (m_clientEngineAssembly != null)
				{
					return m_clientEngineAssembly;
				}
				return m_clientEngineAssembly = Assembly.Load("Assembly-CSharp");
			}
		}

		public static Assembly editorAssembly
		{
			get
			{
				if (m_editorEngineAssembly != null)
				{
					return m_editorEngineAssembly;
				}
				return m_editorEngineAssembly = Assembly.Load("Assembly-CSharp-Editor");
			}
		}

		public static bool InheritFrom<Base>(Type p_type)
		{
			Type typeFromHandle = typeof(Base);
			if (p_type == typeFromHandle)
			{
				return true;
			}
			return p_type.IsSubclassOf(typeFromHandle);
		}

		public static bool InheritFrom<Base, Type>()
		{
			return InheritFrom<Base>(typeof(Type));
		}

		public static MethodInfo[] GetMethods(T p_target)
		{
			return p_target.GetType().GetMethods(m_flags);
		}

		public static MethodInfo GetMethod(T p_target, string p_name)
		{
			return new List<MethodInfo>(p_target.GetType().GetMethods(m_flags)).Find((MethodInfo it) => it.Name == p_name);
		}

		public static U Get<U>(IList p_list, int p_index, U p_default)
		{
			if (p_index < 0)
			{
				return p_default;
			}
			if (p_index >= p_list.Count)
			{
				return p_default;
			}
			return AssertCast<U>(p_list[p_index]);
		}

		public static U Get<U>(IList p_list, int p_index)
		{
			return Get(p_list, p_index, default(U));
		}

		public static U Get<U>(T p_target, string p_property)
		{
			if (p_target == null)
			{
				return default(U);
			}
			Type type = p_target.GetType();
			FieldInfo field = type.GetField(p_property, m_flags);
			if (field != null)
			{
				return (U)field.GetValue(p_target);
			}
			PropertyInfo property = type.GetProperty(p_property, m_flags);
			if (property != null)
			{
				return (U)property.GetValue(p_target, null);
			}
			return default(U);
		}

		public static object Get(T p_target, string p_property)
		{
			if (p_target == null)
			{
				return null;
			}
			Type type = p_target.GetType();
			FieldInfo field = type.GetField(p_property, m_flags);
			if (field != null)
			{
				return field.GetValue(p_target);
			}
			PropertyInfo property = type.GetProperty(p_property, m_flags);
			if (property != null)
			{
				return property.GetValue(p_target, null);
			}
			return null;
		}

		public static U Get<U>(string p_property)
		{
			return GetStatic<U>(typeof(T), p_property);
		}

		public static U GetStatic<U>(Type p_type, string p_property)
		{
			if (p_type == null)
			{
				return default(U);
			}
			FieldInfo field = p_type.GetField(p_property, m_flags);
			if (field != null)
			{
				return (U)field.GetValue(p_type);
			}
			PropertyInfo property = p_type.GetProperty(p_property, m_flags);
			if (property != null)
			{
				return (U)property.GetValue(p_type, null);
			}
			return default(U);
		}

		public static bool Contains(T p_target, string p_property)
		{
			if (p_target == null)
			{
				return false;
			}
			Type type = p_target.GetType();
			if (type.GetField(p_property, m_flags) != null)
			{
				return true;
			}
			if (type.GetProperty(p_property, m_flags) != null)
			{
				return true;
			}
			if (type.GetMethod(p_property, m_flags) != null)
			{
				return true;
			}
			return false;
		}

		public static bool Contains(string p_property)
		{
			return ContainsStatic(typeof(T), p_property);
		}

		public static bool ContainsStatic(Type p_type, string p_property)
		{
			if (p_type == null)
			{
				return false;
			}
			if (p_type.GetField(p_property, m_flags) != null)
			{
				return true;
			}
			if (p_type.GetProperty(p_property, m_flags) != null)
			{
				return true;
			}
			if (p_type.GetMethod(p_property, m_flags) != null)
			{
				return true;
			}
			return false;
		}

		public static U Cast<U>(object p_value)
		{
			return (U)p_value;
		}

		public static U Traverse<U>(object p_target, string p_path)
		{
			List<string> list = new List<string>(p_path.Split('.'));
			for (int i = 0; i < list.Count; i++)
			{
				if (string.IsNullOrEmpty(list[i]))
				{
					list.RemoveAt(i--);
				}
			}
			if (list.Count <= 0)
			{
				return (U)p_target;
			}
			object obj = p_target;
			if (obj == null)
			{
				string text = list[0].Trim();
				if (text.IndexOf("+") == 0)
				{
					text = text.Substring(1);
					list.RemoveAt(0);
					GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
					foreach (GameObject gameObject in rootGameObjects)
					{
						if (gameObject.name == text)
						{
							obj = gameObject;
							break;
						}
					}
				}
			}
			while (list.Count > 0 && obj != null)
			{
				string text = list[0].Trim();
				list.RemoveAt(0);
				if (text.IndexOf("+") == 0 || text.IndexOf("-") == 0)
				{
					bool flag = text.IndexOf("-") == 0;
					text = text.Substring(1);
					Transform transform = null;
					if (obj is Component)
					{
						transform = ((Component)obj).transform;
					}
					if (obj is GameObject)
					{
						transform = ((GameObject)obj).transform;
					}
					if ((bool)transform)
					{
						obj = (flag ? transform.parent : transform.Find(text));
					}
				}
				else if (text.IndexOf("@") == 0)
				{
					text = text.Substring(1);
					text = text.Replace("/", ".");
					GameObject gameObject2 = null;
					if (obj is Component)
					{
						gameObject2 = ((Component)obj).gameObject;
					}
					if (obj is GameObject)
					{
						gameObject2 = (GameObject)obj;
					}
					obj = null;
					if ((bool)gameObject2)
					{
						obj = gameObject2.GetComponent(text);
					}
				}
				else if (text.IndexOf("[") >= 0)
				{
					text = text.Replace("[", "").Replace("]", "").Trim();
					int result = -1;
					if (!int.TryParse(text, out result))
					{
						obj = Reflection<object>.Get(obj, text);
						continue;
					}
					IList list2 = (IList)obj;
					obj = null;
					if (list2 != null && result < list2.Count)
					{
						obj = list2[result];
					}
				}
				else
				{
					obj = Reflection<object>.Get(obj, text);
				}
			}
			if (obj != null)
			{
				return (U)obj;
			}
			return default(U);
		}

		public static U Traverse<U>(object p_target, string p_path, object p_value)
		{
			List<string> list = new List<string>(p_path.Split('.'));
			string p_property = ((list.Count <= 0) ? "" : list[list.Count - 1]);
			list.RemoveAt(list.Count - 1);
			string p_path2 = string.Join(".", list.ToArray());
			U val = Traverse<U>(p_target, p_path2);
			if (val != null)
			{
				Reflection<object>.Set((object)val, p_property, p_value);
			}
			return val;
		}

		public static bool ParseStatement(object p_target, string p_statement)
		{
			string text = p_statement;
			bool flag = false;
			string text2 = '\u0003'.ToString();
			string text3 = '\u0004'.ToString();
			string text4 = '\u0005'.ToString();
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (c == '"')
				{
					flag = !flag;
				}
				if (flag)
				{
					if (c == ' ')
					{
						text = text.Insert(i, text2);
						text = text.Remove(i + 1, 1);
					}
					if (c == '.')
					{
						text = text.Insert(i, text3);
						text = text.Remove(i + 1, 1);
					}
					if (c == ',')
					{
						text = text.Insert(i, text4);
						text = text.Remove(i + 1, 1);
					}
				}
			}
			text = text.Replace("\n", "");
			text = text.Replace("<", " < ");
			text = text.Replace(">", " > ");
			text = text.Replace("!", " ! ");
			text = text.Replace("=", " = ");
			text = text.Replace("&", " & ");
			text = text.Replace("|", " | ");
			text = text.Replace(".", " . ");
			text = text.Replace(",", " , ");
			text = new Regex("\\<\\s+\\=").Replace(text, " <= ");
			text = new Regex("\\>\\s+\\=").Replace(text, " >= ");
			text = new Regex("\\=\\s+\\=").Replace(text, " == ");
			text = new Regex("\\!\\s+\\=").Replace(text, " != ");
			text = new Regex("\\&\\s+\\&").Replace(text, " && ");
			text = new Regex("\\|\\s+\\|").Replace(text, " || ");
			text = new Regex("\\s+\\.\\s+").Replace(text, ".");
			text = new Regex("\\.\\s+").Replace(text, ".");
			text = new Regex("\\s+\\.").Replace(text, ".");
			text = new Regex("\\s+\\,\\s+").Replace(text, ",");
			text = new Regex("\\,\\s+").Replace(text, ",");
			text = new Regex("\\s+\\,").Replace(text, ",");
			List<string> list = new List<string>(text.Split(' '));
			for (int j = 0; j < list.Count; j++)
			{
				list[j] = list[j].Trim();
				list[j] = list[j].Replace(text2, " ");
				list[j] = list[j].Replace(text3, ".");
				list[j] = list[j].Replace(text4, ",");
				if (string.IsNullOrEmpty(list[j]))
				{
					list.RemoveAt(j--);
				}
			}
			List<object> list2 = new List<object>();
			for (int k = 0; k < list.Count; k++)
			{
				object item = Parse(p_target, list[k]);
				list2.Add(item);
			}
			List<bool> list3 = new List<bool>();
			for (int l = 2; l < list2.Count; l += 4)
			{
				object a = list2[l - 2];
				ComparisonOperator comparisonOperator = ((list2[l - 1] is ComparisonOperator) ? ((ComparisonOperator)list2[l - 1]) : ComparisonOperator.Invalid);
				if (comparisonOperator != ComparisonOperator.Invalid)
				{
					object b = list2[l];
					bool item2 = Compare(a, b, comparisonOperator);
					list3.Add(item2);
				}
			}
			bool flag2 = list3.Count > 0 && list3[0];
			int num = 1;
			for (int m = 3; m < list2.Count; m += 4)
			{
				if (num >= list3.Count)
				{
					break;
				}
				LogicOperator logicOperator = ((list2[m] is LogicOperator) ? ((LogicOperator)list2[m]) : LogicOperator.Invalid);
				if (logicOperator != LogicOperator.Invalid)
				{
					bool flag3 = list3[num++];
					switch (logicOperator)
					{
					case LogicOperator.And:
						flag2 = flag2 && flag3;
						break;
					case LogicOperator.Or:
						flag2 = flag2 || flag3;
						break;
					}
				}
			}
			return flag2;
		}

		public static object Parse(object p_target, string p_token)
		{
			float result = 0f;
			int result2 = 0;
			if (p_token[0] == '"')
			{
				int length = p_token.Length;
				return p_token.Remove(length - 1, 1).Remove(0, 1);
			}
			if (p_token.ToLower() == "true")
			{
				return true;
			}
			if (p_token.ToLower() == "false")
			{
				return false;
			}
			if (p_token.Contains(","))
			{
				string[] array = p_token.Split(',');
				switch (array.Length)
				{
				case 2:
				{
					Vector2 vector3 = default(Vector2);
					float.TryParse(array[0], out vector3.x);
					float.TryParse(array[1], out vector3.y);
					return vector3;
				}
				case 3:
				{
					Vector3 vector2 = default(Vector3);
					float.TryParse(array[0], out vector2.x);
					float.TryParse(array[1], out vector2.y);
					float.TryParse(array[2], out vector2.z);
					return vector2;
				}
				case 4:
				{
					Vector4 vector = default(Vector4);
					float.TryParse(array[0], out vector.x);
					float.TryParse(array[1], out vector.y);
					float.TryParse(array[2], out vector.z);
					float.TryParse(array[3], out vector.w);
					return vector;
				}
				}
			}
			if (p_token.Contains(".") && float.TryParse(p_token, out result))
			{
				return result;
			}
			if (int.TryParse(p_token, out result2))
			{
				return result2;
			}
			return p_token switch
			{
				"<=" => ComparisonOperator.LessEqual, 
				"<" => ComparisonOperator.Less, 
				">=" => ComparisonOperator.GreaterEqual, 
				">" => ComparisonOperator.Greater, 
				"==" => ComparisonOperator.Equal, 
				"!=" => ComparisonOperator.NotEqual, 
				"&&" => LogicOperator.And, 
				"||" => LogicOperator.Or, 
				"!!" => LogicOperator.Not, 
				_ => Traverse<object>(p_target, p_token), 
			};
		}

		public static bool Compare(object a, object b, ComparisonOperator op)
		{
			bool flag = a is int;
			bool flag2 = b is int;
			bool flag3 = a is float;
			bool flag4 = b is float;
			bool flag5 = flag && flag2;
			bool flag6 = (flag3 && flag4) || (flag3 && flag2) || flag || flag4;
			bool num = a is Vector2 || a is Vector3 || a is Vector4;
			bool flag7 = b is Vector2 || b is Vector3 || b is Vector4;
			int a2 = 0;
			int b2 = 0;
			bool flag8 = num && flag7;
			int num2 = 0;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 0f;
			float num7 = 0f;
			float num8 = 0f;
			float num9 = 0f;
			float num10 = 0f;
			if (flag8)
			{
				if (a is Vector2)
				{
					a2 = 2;
				}
				if (a is Vector3)
				{
					a2 = 3;
				}
				if (a is Vector4)
				{
					a2 = 4;
				}
				if (b is Vector2)
				{
					b2 = 2;
				}
				if (b is Vector3)
				{
					b2 = 3;
				}
				if (b is Vector4)
				{
					b2 = 4;
				}
				num2 = Mathf.Min(a2, b2);
				if (num2 >= 1)
				{
					if (a is Vector2)
					{
						num3 = ((Vector2)a).x;
					}
					else if (a is Vector3)
					{
						num3 = ((Vector3)a).x;
					}
					else if (a is Vector4)
					{
						num3 = ((Vector4)a).x;
					}
					if (b is Vector2)
					{
						num7 = ((Vector2)b).x;
					}
					else if (b is Vector3)
					{
						num7 = ((Vector3)b).x;
					}
					else if (b is Vector4)
					{
						num7 = ((Vector4)b).x;
					}
				}
				if (num2 >= 2)
				{
					if (a is Vector2)
					{
						num4 = ((Vector2)a).y;
					}
					else if (a is Vector3)
					{
						num4 = ((Vector3)a).y;
					}
					else if (a is Vector4)
					{
						num4 = ((Vector4)a).y;
					}
					if (b is Vector2)
					{
						num8 = ((Vector2)b).y;
					}
					else if (b is Vector3)
					{
						num8 = ((Vector3)b).y;
					}
					else if (b is Vector4)
					{
						num8 = ((Vector4)b).y;
					}
				}
				if (num2 >= 3)
				{
					if (a is Vector3)
					{
						num5 = ((Vector3)a).z;
					}
					else if (a is Vector4)
					{
						num5 = ((Vector4)a).z;
					}
					if (b is Vector3)
					{
						num9 = ((Vector3)b).z;
					}
					else if (b is Vector4)
					{
						num9 = ((Vector4)b).z;
					}
				}
				if (num2 >= 4)
				{
					if (a is Vector4)
					{
						num6 = ((Vector4)a).w;
					}
					if (b is Vector4)
					{
						num10 = ((Vector4)b).w;
					}
				}
			}
			switch (op)
			{
			case ComparisonOperator.Equal:
				if (a == null && b == null)
				{
					return true;
				}
				if (a == null)
				{
					return false;
				}
				if (b == null)
				{
					return false;
				}
				if (a is string || b is string)
				{
					return string.Compare(a.ToString(), b.ToString()) == 0;
				}
				if (a is bool && b is bool)
				{
					return a == b;
				}
				if (flag5)
				{
					int num17 = (int)a;
					int num12 = (int)b;
					return num17 == num12;
				}
				if (flag6)
				{
					float num18 = (flag ? ((float)(int)a) : ((float)a));
					float num14 = (flag2 ? ((float)(int)b) : ((float)b));
					return num18 == num14;
				}
				if (flag8)
				{
					bool flag11 = true;
					if (num2 >= 2)
					{
						flag11 = flag11 && num3 == num7 && num4 == num8;
					}
					if (num2 >= 3)
					{
						flag11 = flag11 && num5 == num9;
					}
					if (num2 >= 4)
					{
						flag11 = flag11 && num6 == num10;
					}
					return flag11;
				}
				return a == b;
			case ComparisonOperator.NotEqual:
				if (a == null && b == null)
				{
					return false;
				}
				if (a == null)
				{
					return true;
				}
				if (b == null)
				{
					return true;
				}
				if (a is string || b is string)
				{
					return string.Compare(a.ToString(), b.ToString()) == 0;
				}
				if (a is bool && b is bool)
				{
					return a != b;
				}
				if (flag5)
				{
					int num15 = (int)a;
					int num12 = (int)b;
					return num15 != num12;
				}
				if (flag6)
				{
					float num16 = (flag ? ((float)(int)a) : ((float)a));
					float num14 = (flag2 ? ((float)(int)b) : ((float)b));
					return num16 != num14;
				}
				if (flag8)
				{
					bool flag10 = true;
					if (num2 >= 2)
					{
						flag10 = flag10 && num3 != num7 && num4 != num8;
					}
					if (num2 >= 3)
					{
						flag10 = flag10 && num5 != num9;
					}
					if (num2 >= 4)
					{
						flag10 = flag10 && num6 != num10;
					}
					return flag10;
				}
				return a != b;
			case ComparisonOperator.Less:
				if (a == null && b == null)
				{
					return true;
				}
				if (a == null)
				{
					return false;
				}
				if (b == null)
				{
					return false;
				}
				if (a is string || b is string)
				{
					return string.Compare(a.ToString(), b.ToString()) < 0;
				}
				if (flag5)
				{
					int num21 = (int)a;
					int num12 = (int)b;
					return num21 < num12;
				}
				if (flag6)
				{
					float num22 = (flag ? ((float)(int)a) : ((float)a));
					float num14 = (flag2 ? ((float)(int)b) : ((float)b));
					return num22 < num14;
				}
				if (flag8)
				{
					bool flag13 = true;
					if (num2 >= 2)
					{
						flag13 = flag13 && num3 < num7 && num4 < num8;
					}
					if (num2 >= 3)
					{
						flag13 = flag13 && num5 < num9;
					}
					if (num2 >= 4)
					{
						flag13 = flag13 && num6 < num10;
					}
					return flag13;
				}
				break;
			case ComparisonOperator.LessEqual:
				if (a == null && b == null)
				{
					return true;
				}
				if (a == null)
				{
					return false;
				}
				if (b == null)
				{
					return false;
				}
				if (a is string || b is string)
				{
					return string.Compare(a.ToString(), b.ToString()) <= 0;
				}
				if (flag5)
				{
					int num23 = (int)a;
					int num12 = (int)b;
					return num23 <= num12;
				}
				if (flag6)
				{
					float num24 = (flag ? ((float)(int)a) : ((float)a));
					float num14 = (flag2 ? ((float)(int)b) : ((float)b));
					return num24 <= num14;
				}
				if (flag8)
				{
					bool flag14 = true;
					if (num2 >= 2)
					{
						flag14 = flag14 && num3 <= num7 && num4 <= num8;
					}
					if (num2 >= 3)
					{
						flag14 = flag14 && num5 <= num9;
					}
					if (num2 >= 4)
					{
						flag14 = flag14 && num6 <= num10;
					}
					return flag14;
				}
				break;
			case ComparisonOperator.Greater:
				if (a == null && b == null)
				{
					return true;
				}
				if (a == null)
				{
					return false;
				}
				if (b == null)
				{
					return false;
				}
				if (a is string || b is string)
				{
					return string.Compare(a.ToString(), b.ToString()) > 0;
				}
				if (flag5)
				{
					int num19 = (int)a;
					int num12 = (int)b;
					return num19 > num12;
				}
				if (flag6)
				{
					float num20 = (flag ? ((float)(int)a) : ((float)a));
					float num14 = (flag2 ? ((float)(int)b) : ((float)b));
					return num20 > num14;
				}
				if (flag8)
				{
					bool flag12 = true;
					if (num2 >= 2)
					{
						flag12 = flag12 && num3 > num7 && num4 > num8;
					}
					if (num2 >= 3)
					{
						flag12 = flag12 && num5 > num9;
					}
					if (num2 >= 4)
					{
						flag12 = flag12 && num6 > num10;
					}
					return flag12;
				}
				break;
			case ComparisonOperator.GreaterEqual:
				if (a == null && b == null)
				{
					return true;
				}
				if (a == null)
				{
					return false;
				}
				if (b == null)
				{
					return false;
				}
				if (a is string || b is string)
				{
					return string.Compare(a.ToString(), b.ToString()) >= 0;
				}
				if (flag5)
				{
					int num11 = (int)a;
					int num12 = (int)b;
					return num11 >= num12;
				}
				if (flag6)
				{
					float num13 = (flag ? ((float)(int)a) : ((float)a));
					float num14 = (flag2 ? ((float)(int)b) : ((float)b));
					return num13 >= num14;
				}
				if (flag8)
				{
					bool flag9 = true;
					if (num2 >= 2)
					{
						flag9 = flag9 && num3 >= num7 && num4 >= num8;
					}
					if (num2 >= 3)
					{
						flag9 = flag9 && num5 >= num9;
					}
					if (num2 >= 4)
					{
						flag9 = flag9 && num6 >= num10;
					}
					return flag9;
				}
				break;
			}
			return false;
		}

		public static U Traverse<U>(string p_path)
		{
			return Traverse<U>(null, p_path);
		}

		public static void Set<U>(U[] p_list, int p_pos, U p_value)
		{
			if (p_list != null && p_pos >= 0 && p_pos < p_list.Length)
			{
				p_list[p_pos] = p_value;
			}
		}

		public static U Get<U>(U[] p_list, int p_pos, U p_default)
		{
			if (p_list == null)
			{
				return p_default;
			}
			if (p_pos >= 0 && p_pos < p_list.Length)
			{
				return p_list[p_pos];
			}
			return p_default;
		}

		public static U Get<U>(U[] p_list, int p_pos)
		{
			return Get(p_list, p_pos, default(U));
		}

		public static void Set<U>(IList<U> p_list, int p_pos, U p_value)
		{
			if (p_list != null && p_pos >= 0 && p_pos < p_list.Count)
			{
				p_list[p_pos] = p_value;
			}
		}

		public static U Get<U>(IList<U> p_list, int p_pos, U p_default)
		{
			if (p_list == null)
			{
				return p_default;
			}
			if (p_pos >= 0 && p_pos < p_list.Count)
			{
				return p_list[p_pos];
			}
			return p_default;
		}

		public static U Get<U>(IList<U> p_list, int p_pos)
		{
			return Get(p_list, p_pos, default(U));
		}

		public static void Set(T p_target, string p_property, object p_value, bool p_number_implicity_cast)
		{
			Type type = p_target.GetType();
			MemberInfo[] member = type.GetMember(p_property, MemberTypes.Field | MemberTypes.Property, m_flags);
			bool flag = p_target is UnityEngine.Object;
			UnityEngine.Object obj = (flag ? (p_target as UnityEngine.Object) : null);
			while (null != type && member.Length == 0)
			{
				type = type.BaseType;
				if (type == null)
				{
					return;
				}
				member = type.GetMember(p_property, MemberTypes.Field | MemberTypes.Property, m_flags);
			}
			switch (member[0].MemberType)
			{
			case MemberTypes.Property:
			{
				if (flag && !obj)
				{
					Debug.LogWarning("Reflection> Set[Property] - Invalid UnityObject");
					break;
				}
				PropertyInfo propertyInfo = (PropertyInfo)member[0];
				if (!p_number_implicity_cast)
				{
					propertyInfo.SetValue(p_target, p_value, null);
					break;
				}
				Type type3 = p_value.GetType();
				Type propertyType = propertyInfo.PropertyType;
				if (type3 == propertyType)
				{
					propertyInfo.SetValue(p_target, p_value, null);
				}
				else if (propertyType == typeof(float))
				{
					if (p_value is int)
					{
						propertyInfo.SetValue(p_target, (float)(int)p_value, null);
					}
					else if (p_value is double)
					{
						propertyInfo.SetValue(p_target, (float)(double)p_value, null);
					}
				}
				else if (p_value is float)
				{
					propertyInfo.SetValue(p_target, (int)(float)p_value, null);
				}
				else if (p_value is double)
				{
					propertyInfo.SetValue(p_target, (int)(double)p_value, null);
				}
				break;
			}
			case MemberTypes.Field:
			{
				if (flag && !obj)
				{
					Debug.LogWarning("Reflection> Set[Field] - Invalid UnityObject");
					break;
				}
				FieldInfo fieldInfo = (FieldInfo)member[0];
				if (!p_number_implicity_cast)
				{
					fieldInfo.SetValue(p_target, p_value);
					break;
				}
				Type type2 = p_value.GetType();
				Type fieldType = fieldInfo.FieldType;
				if (type2 == fieldType)
				{
					fieldInfo.SetValue(p_target, p_value);
				}
				else if (fieldType == typeof(float))
				{
					if (p_value is int)
					{
						fieldInfo.SetValue(p_target, (float)(int)p_value);
					}
					else if (p_value is double)
					{
						fieldInfo.SetValue(p_target, (float)(double)p_value);
					}
				}
				else if (p_value is float)
				{
					fieldInfo.SetValue(p_target, (int)(float)p_value);
				}
				else if (p_value is double)
				{
					fieldInfo.SetValue(p_target, (int)(double)p_value);
				}
				break;
			}
			}
		}

		public static int GetEnum(Enum p_flag)
		{
			Type type = p_flag.GetType();
			string name = Enum.GetName(type, p_flag);
			Array names = Enum.GetNames(type);
			Array values = Enum.GetValues(type);
			int num = Array.IndexOf(names, name);
			if (num < 0)
			{
				return -255;
			}
			if (num >= values.Length)
			{
				return -255;
			}
			return (int)values.GetValue(num);
		}

		public static U GetEnum<U>(int p_flag)
		{
			Array values = Enum.GetValues(typeof(U));
			for (int i = 0; i < values.Length; i++)
			{
				object value = values.GetValue(i);
				if ((int)value == p_flag)
				{
					return (U)value;
				}
			}
			return default(U);
		}

		public static U GetEnum<U>(string p_flag)
		{
			Type typeFromHandle = typeof(U);
			string[] names = Enum.GetNames(typeFromHandle);
			Array values = Enum.GetValues(typeFromHandle);
			for (int i = 0; i < names.Length; i++)
			{
				if (names[i] == p_flag)
				{
					return (U)values.GetValue(i);
				}
			}
			return default(U);
		}

		public static void Set(T p_target, string p_property, object p_value)
		{
			Set(p_target, p_property, p_value, p_number_implicity_cast: false);
		}

		public static void Set(string p_property, object p_value)
		{
			SetStatic(typeof(T), p_property, p_value);
		}

		public static void SetStatic(Type p_type, string p_property, object p_value)
		{
			FieldInfo field = p_type.GetField(p_property, m_flags);
			if (field != null)
			{
				field.SetValue(p_type, p_value);
				return;
			}
			PropertyInfo property = p_type.GetProperty(p_property, m_flags);
			if (property != null)
			{
				property.SetValue(p_type, p_value, null);
			}
		}

		public static object Invoke(T p_target, string p_method, params object[] p_args)
		{
			Type type = p_target.GetType();
			return Invoke(p_target, type, p_method, p_args);
		}

		public static object Invoke(object p_target, Type p_type, string p_method, params object[] p_args)
		{
			Type[] array = new Type[p_args.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = p_args[i].GetType();
			}
			MethodInfo method = p_type.GetMethod(p_method, m_flags, Type.DefaultBinder, array, null);
			if (method == null)
			{
				return null;
			}
			return method.Invoke(p_target, p_args);
		}

		public static U Invoke<U>(T p_target, string p_method, params object[] p_args)
		{
			return (U)Invoke(p_target, p_method, p_args);
		}

		public static object Invoke(string p_method, params object[] p_args)
		{
			return InvokeStatic(typeof(T), p_method, p_args);
		}

		public static object InvokeStatic(Type p_type, string p_method, params object[] p_args)
		{
			Type[] array = new Type[p_args.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = p_args[i].GetType();
			}
			MethodInfo method = p_type.GetMethod(p_method, m_flags, Type.DefaultBinder, array, null);
			if (method == null)
			{
				Debug.LogWarning("Reflection> Failed to locate method [" + p_method + "]");
				return null;
			}
			return method.Invoke(p_type, p_args);
		}

		public static U Invoke<U>(string p_method, params object[] p_args)
		{
			return (U)Invoke(p_method, p_args);
		}

		public static U New<U>(params object[] p_args)
		{
			Type typeFromHandle = typeof(U);
			Type[] array = new Type[p_args.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = p_args[i].GetType();
			}
			ConstructorInfo constructor = typeFromHandle.GetConstructor(array);
			if (constructor == null)
			{
				return default(U);
			}
			return (U)constructor.Invoke(p_args);
		}

		public static U Assert<U>(ref U p_var)
		{
			if (p_var != null)
			{
				return p_var;
			}
			return p_var = New<U>(Array.Empty<object>());
		}

		public static U Assert<U>(ref U p_var, GameObject p_target, bool p_add = true) where U : Component
		{
			if (!p_target)
			{
				return p_var;
			}
			if ((bool)p_var)
			{
				return p_var;
			}
			p_var = p_target.GetComponent<U>();
			if ((bool)p_var)
			{
				return p_var;
			}
			if (p_add)
			{
				p_var = p_target.AddComponent<U>();
			}
			return p_var;
		}

		public static U AssertCast<U>(object p_var, U p_default)
		{
			if (p_var == null)
			{
				return p_default;
			}
			U result = p_default;
			if (p_var is float num)
			{
				if (typeof(U) == typeof(bool))
				{
					return (U)(object)(num != 0f);
				}
				if (typeof(U) == typeof(int))
				{
					return (U)(object)(int)num;
				}
				if (typeof(U) == typeof(uint))
				{
					return (U)(object)(uint)num;
				}
				if (typeof(U) == typeof(long))
				{
					return (U)(object)(long)num;
				}
				if (typeof(U) == typeof(ulong))
				{
					return (U)(object)(ulong)num;
				}
				if (typeof(U) == typeof(double))
				{
					return (U)(object)(double)num;
				}
			}
			if (p_var is int num2)
			{
				if (typeof(U) == typeof(bool))
				{
					return (U)(object)(num2 != 0);
				}
				if (typeof(U) == typeof(float))
				{
					return (U)(object)(float)num2;
				}
				if (typeof(U) == typeof(long))
				{
					return (U)(object)(long)num2;
				}
				if (typeof(U) == typeof(ulong))
				{
					return (U)(object)(ulong)num2;
				}
				if (typeof(U) == typeof(uint))
				{
					return (U)(object)(uint)num2;
				}
				if (typeof(U) == typeof(double))
				{
					return (U)(object)(double)num2;
				}
			}
			if (p_var is long num3)
			{
				if (typeof(U) == typeof(bool))
				{
					return (U)(object)(num3 != 0);
				}
				if (typeof(U) == typeof(float))
				{
					return (U)(object)(float)num3;
				}
				if (typeof(U) == typeof(int))
				{
					return (U)(object)(int)num3;
				}
				if (typeof(U) == typeof(long))
				{
					return (U)(object)num3;
				}
				if (typeof(U) == typeof(ulong))
				{
					return (U)(object)(ulong)num3;
				}
				if (typeof(U) == typeof(uint))
				{
					return (U)(object)(uint)num3;
				}
				if (typeof(U) == typeof(double))
				{
					return (U)(object)(double)num3;
				}
			}
			if (p_var is ulong num4)
			{
				if (typeof(U) == typeof(bool))
				{
					return (U)(object)(num4 != 0);
				}
				if (typeof(U) == typeof(float))
				{
					return (U)(object)(float)num4;
				}
				if (typeof(U) == typeof(int))
				{
					return (U)(object)(int)num4;
				}
				if (typeof(U) == typeof(long))
				{
					return (U)(object)(long)num4;
				}
				if (typeof(U) == typeof(ulong))
				{
					return (U)(object)num4;
				}
				if (typeof(U) == typeof(uint))
				{
					return (U)(object)(uint)num4;
				}
				if (typeof(U) == typeof(double))
				{
					return (U)(object)(double)num4;
				}
			}
			if (p_var is double num5)
			{
				if (typeof(U) == typeof(bool))
				{
					return (U)(object)(num5 != 0.0);
				}
				if (typeof(U) == typeof(float))
				{
					return (U)(object)(float)num5;
				}
				if (typeof(U) == typeof(int))
				{
					return (U)(object)(int)num5;
				}
				if (typeof(U) == typeof(long))
				{
					return (U)(object)(long)num5;
				}
				if (typeof(U) == typeof(ulong))
				{
					return (U)(object)(ulong)num5;
				}
				if (typeof(U) == typeof(uint))
				{
					return (U)(object)(uint)num5;
				}
			}
			if (p_var is string)
			{
				string text = (string)p_var;
				if (typeof(U) == typeof(bool))
				{
					bool result2 = false;
					if (bool.TryParse(text, out result2))
					{
						return (U)(object)result2;
					}
					return result;
				}
				if (typeof(U) == typeof(float))
				{
					float result3 = 0f;
					if (float.TryParse(text, out result3))
					{
						return (U)(object)result3;
					}
					return result;
				}
				if (typeof(U) == typeof(int))
				{
					int result4 = 0;
					if (int.TryParse(text, out result4))
					{
						return (U)(object)result4;
					}
					return result;
				}
				if (typeof(U) == typeof(ulong))
				{
					ulong result5 = 0uL;
					if (ulong.TryParse(text, out result5))
					{
						return (U)(object)result5;
					}
					return result;
				}
				if (typeof(U) == typeof(long))
				{
					long result6 = 0L;
					if (long.TryParse(text, out result6))
					{
						return (U)(object)result6;
					}
					return result;
				}
				if (typeof(U) == typeof(uint))
				{
					uint result7 = 0u;
					if (uint.TryParse(text, out result7))
					{
						return (U)(object)result7;
					}
					return result;
				}
				if (typeof(U) == typeof(double))
				{
					double result8 = 0.0;
					if (double.TryParse(text, out result8))
					{
						return (U)(object)result8;
					}
					return result;
				}
				if (typeof(U) == typeof(DateTime))
				{
					DateTime result9 = DateTime.UtcNow;
					if (DateTime.TryParse(text, out result9))
					{
						return (U)(object)result9;
					}
					return result;
				}
			}
			try
			{
				result = (U)p_var;
			}
			catch
			{
				Debug.Log("Reflection> AssertCast - " + typeof(U).Name + " == " + p_var.GetType().Name);
			}
			return result;
		}

		public static U AssertCast<U>(object p_var)
		{
			return AssertCast(p_var, default(U));
		}

		public static T New(Type p_type, params object[] p_args)
		{
			Type[] array = new Type[p_args.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = p_args[i].GetType();
			}
			ConstructorInfo constructor = p_type.GetConstructor(array);
			if (constructor == null)
			{
				return default(T);
			}
			return (T)constructor.Invoke(p_args);
		}
	}
}
