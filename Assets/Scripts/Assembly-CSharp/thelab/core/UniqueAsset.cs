using UnityEngine;

namespace thelab.core
{
	[ExecuteInEditMode]
	public class UniqueAsset : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		public string m_guid;

		public string guid
		{
			get
			{
				if (string.IsNullOrEmpty(m_guid))
				{
					m_guid = GetGUID();
				}
				return m_guid;
			}
			set
			{
				m_guid = value;
				if (string.IsNullOrEmpty(m_guid))
				{
					m_guid = GetGUID();
				}
			}
		}

		public bool destroyed
		{
			get
			{
				if (!this)
				{
					return true;
				}
				if (!base.gameObject)
				{
					return true;
				}
				return false;
			}
		}

		private void Start()
		{
			if (string.IsNullOrEmpty(m_guid))
			{
				guid = "";
			}
		}

		public bool Is<T>()
		{
			if (this is T)
			{
				return true;
			}
			return Reflection<object>.InheritFrom<T>(GetType());
		}

		protected virtual string GetGUID()
		{
			return GUID.Create(24, "", 200, 0, 15, "x1");
		}
	}
}
