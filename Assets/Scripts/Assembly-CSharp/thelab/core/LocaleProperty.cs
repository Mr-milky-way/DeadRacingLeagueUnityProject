using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	public class LocaleProperty : LocaleElement
	{
		public enum Case
		{
			None = 0,
			Upper = 1,
			Lower = 2
		}

		[Serializable]
		public class Target
		{
			public UnityEngine.Object target;

			public string property;

			public Case textCase;
		}

		public List<Target> targets;

		public override void OnLocaleRefresh()
		{
			int num = Mathf.Min(targets.Count, keys.Count);
			if (num <= 0)
			{
				return;
			}
			for (int i = 0; i < num; i++)
			{
				string p_key = keys[i];
				Target target = targets[i];
				if (!target.target)
				{
					continue;
				}
				string property = target.property;
				object obj = Reflection<object>.Traverse<object>(target.target, property);
				if (obj == null)
				{
					continue;
				}
				string text;
				if (obj.GetType().IsArray)
				{
					text = base.manager.Get<string>(p_key, "");
					switch (target.textCase)
					{
					case Case.Upper:
						text = text.ToUpper();
						break;
					case Case.Lower:
						text = text.ToLower();
						break;
					}
					string[] array = text.Split(',');
					Array array2 = obj as Array;
					int num2 = Mathf.Min(array2.Length, array.Length);
					for (int j = 0; j < num2; j++)
					{
						array2.SetValue(array[j], j);
					}
					continue;
				}
				if (obj.GetType() == typeof(List<string>))
				{
					text = base.manager.Get<string>(p_key, "");
					switch (target.textCase)
					{
					case Case.Upper:
						text = text.ToUpper();
						break;
					case Case.Lower:
						text = text.ToLower();
						break;
					}
					string[] array3 = text.Split(',');
					List<string> list = obj as List<string>;
					int num3 = Mathf.Min(list.Count, array3.Length);
					for (int k = 0; k < num3; k++)
					{
						list[k] = array3[k];
					}
					continue;
				}
				if (obj.GetType() == typeof(List<Dropdown.OptionData>))
				{
					text = base.manager.Get<string>(p_key, "");
					switch (target.textCase)
					{
					case Case.Upper:
						text = text.ToUpper();
						break;
					case Case.Lower:
						text = text.ToLower();
						break;
					}
					string[] array4 = text.Split(',');
					List<Dropdown.OptionData> list2 = obj as List<Dropdown.OptionData>;
					int num4 = Mathf.Min(list2.Count, array4.Length);
					for (int l = 0; l < num4; l++)
					{
						list2[l].text = array4[l];
					}
					continue;
				}
				string p_default = "";
				if (Application.isEditor)
				{
					try
					{
						p_default = Reflection<object>.Traverse<string>(target.target, property);
					}
					catch (Exception ex)
					{
						Debug.LogError("LocaleProperty> Property Get Failed @ target[" + target.target?.ToString() + "] p[" + property + "]\n" + ex.Message);
					}
				}
				else
				{
					p_default = Reflection<object>.Traverse<string>(target.target, property);
				}
				text = base.manager.Get<string>(p_key, p_default);
				switch (target.textCase)
				{
				case Case.Upper:
					text = text.ToUpper();
					break;
				case Case.Lower:
					text = text.ToLower();
					break;
				}
				if (Application.isEditor)
				{
					try
					{
						Reflection<object>.Traverse<object>(target.target, property, text);
					}
					catch (Exception ex2)
					{
						Debug.LogError("LocaleProperty> Property Set Failed @ target[" + target.target?.ToString() + "] p[" + property + "] v[" + text + "]\n" + ex2.Message);
					}
				}
				else
				{
					Reflection<object>.Traverse<object>(target.target, property, text);
				}
			}
		}
	}
}
