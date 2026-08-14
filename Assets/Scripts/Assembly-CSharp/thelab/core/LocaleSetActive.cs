using UnityEngine;

namespace thelab.core
{
	public class LocaleSetActive : LocaleElement
	{
		public enum Criteria
		{
			Any = 0,
			All = 1
		}

		[SerializeField]
		private GameObject m_target;

		public Criteria criteria;

		public bool flag;

		public GameObject target
		{
			get
			{
				return m_target ?? (m_target = base.gameObject);
			}
			set
			{
				m_target = value;
			}
		}

		public override void OnLocaleRefresh()
		{
			if (!target || keys.Count <= 0)
			{
				return;
			}
			bool flag = criteria == Criteria.All;
			bool flag2 = flag;
			for (int i = 0; i < keys.Count; i++)
			{
				string p_key = keys[i];
				bool flag3 = base.manager.Get(p_key, flag);
				switch (criteria)
				{
				case Criteria.All:
					flag2 = flag2 && flag3;
					break;
				case Criteria.Any:
					flag2 = flag2 || flag3;
					break;
				}
			}
			if (flag2)
			{
				target.SetActive(this.flag);
			}
		}
	}
}
