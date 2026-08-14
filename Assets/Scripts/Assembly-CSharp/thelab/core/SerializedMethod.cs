using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class SerializedMethod
	{
		public UnityEngine.Object target;

		public string method;

		public string selection;

		[SerializeField]
		internal List<string> m_methods;

		private List<object> m_arguments;

		[SerializeField]
		private byte[] m_arguments_bytes;

		[SerializeField]
		private List<bool> m_argsUnityFlags;

		[SerializeField]
		private List<UnityEngine.Object> m_argsUnity;

		[SerializeField]
		private List<string> m_argsTypes;

		[SerializeField]
		private List<string> m_argsNames;

		[SerializeField]
		internal int argsCount;

		internal MethodInfo methodInfoCache;

		public List<object> arguments
		{
			get
			{
				if (m_arguments == null)
				{
					DeserializeArgsDefault();
				}
				return m_arguments;
			}
			set
			{
				m_arguments = ((value == null) ? new List<object>() : value);
				m_arguments_bytes = Serialize.ToBytes(m_arguments);
			}
		}

		internal List<bool> argsUnityFlags
		{
			get
			{
				if (m_argsUnityFlags != null)
				{
					return m_argsUnityFlags;
				}
				return m_argsUnityFlags = new List<bool>();
			}
		}

		internal List<UnityEngine.Object> argsUnity
		{
			get
			{
				if (m_argsUnity != null)
				{
					return m_argsUnity;
				}
				return m_argsUnity = new List<UnityEngine.Object>();
			}
		}

		internal List<string> argsTypes
		{
			get
			{
				if (m_argsTypes != null)
				{
					return m_argsTypes;
				}
				return m_argsTypes = new List<string>();
			}
		}

		internal List<string> argsNames
		{
			get
			{
				if (m_argsNames != null)
				{
					return m_argsNames;
				}
				return m_argsNames = new List<string>();
			}
		}

		internal void DeserializeArgsDefault()
		{
			if (m_arguments_bytes == null)
			{
				m_arguments_bytes = Serialize.ToBytes(new List<object>());
			}
			if (m_arguments_bytes.Length == 0)
			{
				m_arguments_bytes = Serialize.ToBytes(new List<object>());
			}
			m_arguments = Serialize.FromBytes<List<object>>(m_arguments_bytes);
		}

		internal object InternalInvoke(object[] p_args)
		{
			if (string.IsNullOrEmpty(selection))
			{
				return null;
			}
			if (!target)
			{
				return null;
			}
			string[] array = selection.Replace("(", " ").Replace(")", "").Split(' ');
			if (array.Length <= 1)
			{
				return null;
			}
			string text = array[0];
			string text2 = array[1];
			if (text != null && text == "set_active" && target is GameObject)
			{
				text = "SetActive";
			}
			MethodInfo[] methods = Reflection<object>.GetMethods(target);
			bool flag = false;
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.Name != text)
				{
					continue;
				}
				string signature = GetSignature(methodInfo);
				if (signature == null || !(signature == text2))
				{
					continue;
				}
				methodInfoCache = methodInfo;
				List<object> list = new List<object>();
				int num = Mathf.Min(argsUnity.Count, arguments.Count, argsUnityFlags.Count);
				if (p_args != null)
				{
					num = p_args.Length;
				}
				for (int j = 0; j < num; j++)
				{
					if (p_args != null)
					{
						list.Add(p_args[j]);
						continue;
					}
					object item = arguments[j];
					UnityEngine.Object item2 = argsUnity[j];
					if (argsUnityFlags[j])
					{
						list.Add(item2);
					}
					else
					{
						list.Add(item);
					}
				}
				object[] parameters = list.ToArray();
				object result = methodInfoCache.Invoke(target, parameters);
				flag = true;
				return result;
			}
			Debug.Log($"SerializedMethod> InternalInvoke / target[{target}] name[{text}] args[{text2}] method-info[{methods.Length}] succ[{flag}]");
			return null;
		}

		public object Invoke()
		{
			return InternalInvoke(null);
		}

		public object Invoke(object[] p_args)
		{
			return InternalInvoke(p_args);
		}

		internal void Select(string p_method_full)
		{
			methodInfoCache = null;
			string[] array = p_method_full.Replace("(", " ").Replace(")", "").Split(' ');
			if (array.Length <= 1)
			{
				return;
			}
			string text = array[0];
			string text2 = array[1];
			MethodInfo[] methods = Reflection<object>.GetMethods(target);
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.Name != text)
				{
					continue;
				}
				string signature = GetSignature(methodInfo);
				if (signature == null || !(signature == text2))
				{
					continue;
				}
				method = text;
				argsCount = methodInfo.GetParameters().Length;
				selection = p_method_full;
				List<object> list = new List<object>();
				m_argsUnityFlags = new List<bool>();
				m_argsUnity = new List<UnityEngine.Object>();
				m_argsNames = new List<string>();
				m_argsTypes = new List<string>();
				ParameterInfo[] parameters = methodInfo.GetParameters();
				for (int j = 0; j < argsCount; j++)
				{
					list.Add(null);
					argsUnityFlags.Add(item: false);
					argsUnity.Add(null);
					argsNames.Add(parameters[j].Name);
					argsTypes.Add(parameters[j].ParameterType.FullName);
				}
				for (int k = 0; k < argsCount; k++)
				{
					ParameterInfo parameterInfo = parameters[k];
					bool flag = Reflection<object>.InheritFrom<UnityEngine.Object>(parameterInfo.ParameterType);
					argsUnityFlags[k] = flag;
					argsTypes[k] = parameterInfo.ParameterType.FullName;
					if (!flag)
					{
						Type parameterType = parameterInfo.ParameterType;
						bool flag2 = parameterType == typeof(string);
						list[k] = (parameterType.IsValueType ? Activator.CreateInstance(parameterType) : (flag2 ? "" : Reflection<object>.New(parameterType)));
					}
				}
				arguments = list;
				break;
			}
		}

		internal void RefreshMethods()
		{
			m_methods = new List<string>();
			if (!target)
			{
				return;
			}
			MethodInfo[] methods = Reflection<object>.GetMethods(target);
			foreach (MethodInfo methodInfo in methods)
			{
				if (IsValid(methodInfo))
				{
					string name = methodInfo.Name;
					string signature = GetSignature(methodInfo);
					if (signature != null)
					{
						m_methods.Add(name + "(" + signature + ")");
					}
				}
			}
		}

		internal bool IsValid(MethodInfo m)
		{
			if (!m.IsPublic)
			{
				return false;
			}
			if (m.IsGenericMethod)
			{
				return false;
			}
			if (m.IsSpecialName && m.Name.IndexOf("get_") >= 0)
			{
				return false;
			}
			return true;
		}

		internal string GetSignature(MethodInfo m)
		{
			string text = "";
			ParameterInfo[] parameters = m.GetParameters();
			bool flag = true;
			for (int i = 0; i < parameters.Length; i++)
			{
				Type parameterType = parameters[i].ParameterType;
				string text2 = parameterType.Name;
				string fullName = parameterType.FullName;
				if (fullName.IndexOf("List`") >= 0)
				{
					flag = false;
					break;
				}
				if (fullName.IndexOf("System.Type") >= 0)
				{
					flag = false;
					break;
				}
				if (fullName.IndexOf("IEnumerator") >= 0)
				{
					flag = false;
					break;
				}
				if (fullName.IndexOf("IntPtr") >= 0)
				{
					flag = false;
					break;
				}
				if (fullName.IndexOf("System.Object") >= 0)
				{
					flag = false;
					break;
				}
				if (fullName.IndexOf("UnityEngine.Coroutine") >= 0)
				{
					flag = false;
					break;
				}
				if (fullName.IndexOf("UnityEngine.RaycastHit") >= 0)
				{
					flag = false;
					break;
				}
				if (fullName.IndexOf("UnityEngine.MaterialPropertyBlock") >= 0)
				{
					flag = false;
					break;
				}
				if (fullName.IndexOf("UnityEngine.Matrix4x4") >= 0)
				{
					flag = false;
					break;
				}
				if (fullName.IndexOf("UnityEngine.ComputeBuffer") >= 0)
				{
					flag = false;
					break;
				}
				if (fullName.IndexOf("UnityEngine.AnimationEvent") >= 0)
				{
					flag = false;
					break;
				}
				if (fullName.IndexOf("UnityEngine.MatchTargetWeightMask") >= 0)
				{
					flag = false;
					break;
				}
				if (fullName.IndexOf("UnityEngine.ParticleSystem+EmitParams") >= 0)
				{
					flag = false;
					break;
				}
				if (fullName.IndexOf("UnityEngine.ParticleSystem+Particle") >= 0)
				{
					flag = false;
					break;
				}
				if (fullName.IndexOf("UnityEngine.Rendering.CommandBuffer") >= 0)
				{
					flag = false;
					break;
				}
				if (fullName.IndexOf("&") >= 0)
				{
					flag = false;
					break;
				}
				if (parameterType.IsArray)
				{
					flag = false;
					break;
				}
				if (parameterType == typeof(Delegate))
				{
					flag = false;
					break;
				}
				if (parameterType.IsSubclassOf(typeof(Delegate)))
				{
					flag = false;
					break;
				}
				switch (text2)
				{
				case "Boolean":
					text2 = "bool";
					break;
				case "Int32":
					text2 = "int";
					break;
				case "UInt32":
					text2 = "uint";
					break;
				case "Int64":
					text2 = "long";
					break;
				case "UInt64":
					text2 = "ulong";
					break;
				case "Single":
					text2 = "float";
					break;
				case "Double":
					text2 = "double";
					break;
				case "String":
					text2 = "string";
					break;
				}
				text += text2;
				if (i < parameters.Length - 1)
				{
					text += ",";
				}
			}
			if (!flag)
			{
				return null;
			}
			return text;
		}
	}
}
