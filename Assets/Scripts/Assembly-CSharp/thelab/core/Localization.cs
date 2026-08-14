using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace thelab.core
{
	public class Localization : MonoBehaviour
	{
		private static readonly Dictionary<SystemLanguage, string> CountryCodes = new Dictionary<SystemLanguage, string>
		{
			{
				SystemLanguage.Afrikaans,
				"af-ZA"
			},
			{
				SystemLanguage.Arabic,
				"ar-SA"
			},
			{
				SystemLanguage.Basque,
				"eu-ES"
			},
			{
				SystemLanguage.Belarusian,
				"be-BY"
			},
			{
				SystemLanguage.Bulgarian,
				"bg-BG"
			},
			{
				SystemLanguage.Catalan,
				"ca-ES"
			},
			{
				SystemLanguage.Chinese,
				"zh-CN"
			},
			{
				SystemLanguage.Czech,
				"cs-CZ"
			},
			{
				SystemLanguage.Danish,
				"da-DK"
			},
			{
				SystemLanguage.Dutch,
				"nl-NL"
			},
			{
				SystemLanguage.English,
				"en-US"
			},
			{
				SystemLanguage.Estonian,
				"et-EE"
			},
			{
				SystemLanguage.Faroese,
				"fo-FO"
			},
			{
				SystemLanguage.Finnish,
				"fi-FI"
			},
			{
				SystemLanguage.French,
				"fr-FR"
			},
			{
				SystemLanguage.German,
				"de-DE"
			},
			{
				SystemLanguage.Greek,
				"el-GR"
			},
			{
				SystemLanguage.Hebrew,
				"he-IL"
			},
			{
				SystemLanguage.Hungarian,
				"hu-HU"
			},
			{
				SystemLanguage.Icelandic,
				"is-IS"
			},
			{
				SystemLanguage.Indonesian,
				"id-ID"
			},
			{
				SystemLanguage.Italian,
				"it-IT"
			},
			{
				SystemLanguage.Japanese,
				"ja-JP"
			},
			{
				SystemLanguage.Korean,
				"ko-KR"
			},
			{
				SystemLanguage.Latvian,
				"lv-LV"
			},
			{
				SystemLanguage.Lithuanian,
				"lt-LT"
			},
			{
				SystemLanguage.Norwegian,
				"no-NO"
			},
			{
				SystemLanguage.Polish,
				"pl-PL"
			},
			{
				SystemLanguage.Portuguese,
				"pt-BR"
			},
			{
				SystemLanguage.Romanian,
				"ro-RO"
			},
			{
				SystemLanguage.Russian,
				"ru-RU"
			},
			{
				SystemLanguage.SerboCroatian,
				"sr-RS"
			},
			{
				SystemLanguage.Slovak,
				"sk-SK"
			},
			{
				SystemLanguage.Slovenian,
				"sl-SI"
			},
			{
				SystemLanguage.Spanish,
				"es-ES"
			},
			{
				SystemLanguage.Swedish,
				"sv-SE"
			},
			{
				SystemLanguage.Thai,
				"th-TH"
			},
			{
				SystemLanguage.Turkish,
				"tr-TR"
			},
			{
				SystemLanguage.Ukrainian,
				"uk-UA"
			},
			{
				SystemLanguage.Vietnamese,
				"vi-VN"
			},
			{
				SystemLanguage.Unknown,
				"un-UN"
			}
		};

		private static Localization m_instance;

		[SerializeField]
		private DictionaryStringString m_texts;

		[SerializeField]
		private DictionaryStringObject m_assets;

		public string language = "en-us";

		public bool runOnAwake;

		public bool debug;

		public List<string> sources;

		private List<ILocaleElement> m_elements;

		public UnityEvent OnLoad;

		private string m_log;

		public static Localization instance
		{
			get
			{
				return m_instance;
			}
			set
			{
				m_instance = value;
			}
		}

		public DictionaryStringString texts => m_texts ?? (m_texts = base.gameObject.AddComponent<DictionaryStringString>());

		public DictionaryStringObject assets => m_assets ?? (m_assets = base.gameObject.AddComponent<DictionaryStringObject>());

		public List<ILocaleElement> elements
		{
			get
			{
				if (m_elements != null)
				{
					return m_elements;
				}
				return m_elements = new List<ILocaleElement>();
			}
			set
			{
				m_elements = value;
			}
		}

		public static string LanguageToCountryISO2(SystemLanguage p_type)
		{
			return LanguageToCountryISO(p_type).Split('-')[1].ToLower();
		}

		public static string LanguageToCountryISO(SystemLanguage p_type)
		{
			SystemLanguage systemLanguage = p_type;
			systemLanguage = (CountryCodes.ContainsKey(systemLanguage) ? systemLanguage : SystemLanguage.Unknown);
			return CountryCodes[systemLanguage];
		}

		public static void Add(ILocaleElement p_element)
		{
			if (p_element != null)
			{
				Localization localization = m_instance;
				if (!localization)
				{
					Debug.LogWarning("Localization> Add - Localization Manager not found!");
				}
				else if (!localization.elements.Contains(p_element))
				{
					localization.elements.Add(p_element);
					OnLocaleRefresh(p_element);
				}
			}
		}

		public static void Remove(ILocaleElement p_element)
		{
			if (p_element != null)
			{
				Localization localization = m_instance;
				if (!localization)
				{
					Debug.LogWarning("Localization> Remove - Localization Manager not found!");
				}
				else if (localization.elements.Contains(p_element))
				{
					localization.elements.Remove(p_element);
				}
			}
		}

		private static bool OnLocaleRefresh(ILocaleElement p_element)
		{
			if (p_element == null)
			{
				return false;
			}
			if (p_element is Object && (Object)p_element == null)
			{
				return false;
			}
			if (p_element is Behaviour && !((Behaviour)p_element).enabled)
			{
				return true;
			}
			p_element.OnLocaleRefresh();
			return true;
		}

		protected void Awake()
		{
			if (!m_instance)
			{
				m_instance = this;
			}
			if (runOnAwake)
			{
				Load();
			}
		}

		public void Load(string p_language)
		{
			if (debug)
			{
				m_log = "Localization> [" + base.name + "] Load - language[" + p_language + "]";
			}
			string text = (language = (string.IsNullOrEmpty(p_language) ? "en-us" : p_language));
			string[] array = new string[2] { "csv", "tsv" };
			texts.Clear();
			assets.Clear();
			for (int i = 0; i < sources.Count; i++)
			{
				string text2 = sources[i];
				text2 = text2 + text + "/";
				foreach (string text3 in array)
				{
					string[] array2 = (Directory.Exists(text2) ? Directory.GetFiles(text2, "*." + text3) : new string[0]);
					foreach (string text4 in array2)
					{
						if (debug)
						{
							m_log = m_log + "\n[" + text4 + "]";
						}
						switch (text3)
						{
						case "csv":
							Parse_SV(text4, ',');
							break;
						case "tsv":
							Parse_SV(text4, '\t');
							break;
						}
					}
				}
			}
			if (debug)
			{
				Debug.Log(m_log);
			}
			if (OnLoad != null)
			{
				OnLoad.Invoke();
			}
			Refresh();
		}

		public void Load()
		{
			Load(language);
		}

		public void Refresh()
		{
			for (int i = 0; i < elements.Count; i++)
			{
				if (!OnLocaleRefresh(elements[i]))
				{
					elements.RemoveAt(i--);
				}
			}
		}

		public T Get<T>(string p_key, T p_default)
		{
			object obj = null;
			bool flag = false;
			if (typeof(T) == typeof(Texture2D))
			{
				obj = (assets.ContainsKey(p_key) ? assets[p_key] : ((object)p_default));
				flag = true;
			}
			if (typeof(T) == typeof(Texture))
			{
				obj = (assets.ContainsKey(p_key) ? assets[p_key] : ((object)p_default));
				flag = true;
			}
			if (!flag)
			{
				bool num = texts.ContainsKey(p_key);
				obj = p_default;
				if (num)
				{
					string text = texts[p_key];
					if (typeof(T) == typeof(string))
					{
						obj = text;
					}
					if (typeof(T) == typeof(int))
					{
						int result = 0;
						int.TryParse(text, out result);
						obj = result;
					}
					if (typeof(T) == typeof(float))
					{
						float result2 = 0f;
						float.TryParse(text, out result2);
						obj = result2;
					}
					if (typeof(T) == typeof(bool))
					{
						bool result3 = false;
						bool.TryParse(text, out result3);
						obj = result3;
					}
				}
			}
			if (!(obj is T))
			{
				Debug.LogWarning("Localization> Failed to cast [" + p_key + "][" + typeof(T).Name + "][" + obj?.ToString() + "]");
				return default(T);
			}
			return (T)obj;
		}

		public T Get<T>(string p_key)
		{
			return Get(p_key, default(T));
		}

		public string Get(string p_key, string p_default)
		{
			return this.Get<string>(p_key, p_default);
		}

		protected void Parse_SV(string p_path, char p_char)
		{
			char[] array = File.ReadAllText(p_path).ToCharArray();
			bool flag = true;
			for (int i = 0; i < array.Length; i++)
			{
				char c = array[i];
				if (c == '"')
				{
					flag = !flag;
				}
				if (flag && c == p_char)
				{
					array[i] = '\u0003';
				}
				if (!flag && c == '\n')
				{
					array[i] = '\u0004';
				}
				if (!flag && c == '\r')
				{
					array[i] = '\u0005';
				}
			}
			string[] array2 = new string(array).Split('\n');
			for (int j = 0; j < array2.Length; j++)
			{
				string[] array3 = array2[j].Split('\u0003');
				if (array3.Length <= 2)
				{
					continue;
				}
				string text = array3[0].Trim();
				string text2 = array3[1].Trim();
				text2 = text2.Replace('\u0004', '\n');
				text2 = text2.Replace('\u0005', '\r');
				if (text2.Length >= 2 && text2[0] == '"')
				{
					text2 = text2.Substring(1, Mathf.Max(0, text2.Length - 2));
				}
				text2 = text2.Replace("''", "\"").Replace("\"\"", "\"");
				switch (array3[2].Trim().ToLower())
				{
				case "text":
					texts.Add(text, text2);
					break;
				case "image":
				{
					Texture2D texture2D = LoadAsset<Texture2D>(text2);
					if ((bool)texture2D)
					{
						texture2D.name = "locale." + text;
					}
					assets.Add(text, texture2D);
					break;
				}
				}
			}
		}

		protected T LoadAsset<T>(string p_path) where T : Object
		{
			T result = null;
			string text = (string.IsNullOrEmpty(language) ? "en-us" : language);
			for (int i = 0; i < sources.Count; i++)
			{
				string text2 = string.Concat(sources[i] + text + "/", p_path);
				if (debug)
				{
					m_log = m_log + "\n   [asset][" + text2 + "]";
				}
				if (File.Exists(text2))
				{
					byte[] data = File.ReadAllBytes(text2);
					if (typeof(T) == typeof(Texture2D))
					{
						Texture2D texture2D = new Texture2D(1, 1);
						texture2D.LoadImage(data, markNonReadable: true);
						return (T)(Object)texture2D;
					}
				}
			}
			return result;
		}
	}
}
