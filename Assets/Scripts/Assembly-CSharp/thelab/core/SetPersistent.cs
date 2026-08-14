using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace thelab.core
{
	public class SetPersistent : MonoBehaviour
	{
		private static Dictionary<string, object> m_table;

		public bool unique = true;

		public bool runOnAwake = true;

		public string id;

		public UnityEvent OnEvent;

		internal static Dictionary<string, object> table
		{
			get
			{
				if (m_table != null)
				{
					return m_table;
				}
				return m_table = new Dictionary<string, object>();
			}
		}

		public static void Clear()
		{
			table.Clear();
		}

		protected void Awake()
		{
			if (runOnAwake)
			{
				Apply();
			}
		}

		public void Apply()
		{
			if (unique && table.ContainsKey(id))
			{
				if ((bool)base.gameObject)
				{
					Object.Destroy(base.gameObject);
				}
				return;
			}
			table[id] = base.gameObject;
			base.transform.SetParent(null, worldPositionStays: true);
			base.name = id;
			Object.DontDestroyOnLoad(base.gameObject);
			if (OnEvent != null)
			{
				OnEvent.Invoke();
			}
		}
	}
}
