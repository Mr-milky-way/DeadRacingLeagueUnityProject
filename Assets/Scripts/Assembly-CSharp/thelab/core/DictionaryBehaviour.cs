using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace thelab.core
{
	public class DictionaryBehaviour<K, V> : DictionaryBehaviour
	{
		public List<K> keys;

		public List<V> values;

		public V this[K k]
		{
			get
			{
				int num = keys.IndexOf(k);
				if (num >= 0)
				{
					return values[num];
				}
				return (V)(object)null;
			}
			set
			{
				int num = keys.IndexOf(k);
				if (num >= 0)
				{
					if (value == null)
					{
						RemoveAt(num);
					}
					else
					{
						values[num] = value;
					}
				}
				else
				{
					Add(k, value);
				}
			}
		}

		public List<KeyValuePair<K, V>> pairs
		{
			get
			{
				List<KeyValuePair<K, V>> list = new List<KeyValuePair<K, V>>();
				for (int i = 0; i < Count; i++)
				{
					list.Add(new KeyValuePair<K, V>(keys[i], values[i]));
				}
				return list;
			}
			set
			{
				if (value == null)
				{
					Clear();
					return;
				}
				for (int i = 0; i < values.Count; i++)
				{
					Add(value[i].Key, value[i].Value);
				}
			}
		}

		public Dictionary<K, V> instance
		{
			get
			{
				Dictionary<K, V> dictionary = new Dictionary<K, V>();
				for (int i = 0; i < Count; i++)
				{
					dictionary[keys[i]] = values[i];
				}
				return dictionary;
			}
			set
			{
				if (value != null)
				{
					Clear();
					pairs = value.ToList();
				}
			}
		}

		public override int Count
		{
			get
			{
				if (keys != null)
				{
					return keys.Count;
				}
				return 0;
			}
		}

		public bool ContainsKey(K p_key)
		{
			return keys.IndexOf(p_key) >= 0;
		}

		public override void Clear()
		{
			if (keys == null)
			{
				keys = new List<K>();
			}
			else
			{
				keys.Clear();
			}
			if (values == null)
			{
				values = new List<V>();
			}
			else
			{
				values.Clear();
			}
		}

		public override void Add()
		{
			Add(default(K), default(V));
		}

		public void Add(K p_key, V p_value)
		{
			if (keys == null)
			{
				keys = new List<K>();
			}
			if (values == null)
			{
				values = new List<V>();
			}
			int num = keys.IndexOf(p_key);
			if (num < 0)
			{
				keys.Add(p_key);
				values.Add(p_value);
			}
			else
			{
				keys[num] = p_key;
				values[num] = p_value;
			}
		}

		public override void RemoveAt(int p_index)
		{
			if (p_index >= 0)
			{
				if (keys != null && p_index < keys.Count)
				{
					keys.RemoveAt(p_index);
				}
				if (values != null && p_index < values.Count)
				{
					values.RemoveAt(p_index);
				}
			}
		}

		private void Awake()
		{
			if (keys == null)
			{
				keys = new List<K>();
			}
			if (values == null)
			{
				values = new List<V>();
			}
		}
	}
	public class DictionaryBehaviour : MonoBehaviour
	{
		public virtual int Count => 0;

		public virtual void Add()
		{
		}

		public virtual void RemoveAt(int p_index)
		{
		}

		public virtual void Clear()
		{
		}
	}
}
