using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public abstract class LocaleElement : MonoBehaviour, ILocaleElement
	{
		[SerializeField]
		private Localization m_manager;

		public List<string> keys;

		private bool warn_manager;

		public Localization manager
		{
			get
			{
				return m_manager ?? (m_manager = Localization.instance);
			}
			set
			{
				m_manager = value;
			}
		}

		protected void Start()
		{
			Init();
		}

		public void Init()
		{
			Localization localization = manager;
			if (!localization)
			{
				if (!warn_manager)
				{
					warn_manager = true;
					Debug.LogWarning(GetType().Name + "> Localization Manager not found!");
				}
				Invoke("Init", 0.1f);
			}
			else if (!localization.elements.Contains(this))
			{
				localization.elements.Add(this);
				if (base.enabled)
				{
					OnLocaleRefresh();
				}
			}
		}

		public virtual void OnLocaleRefresh()
		{
		}
	}
}
