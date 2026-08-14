using UnityEngine;

namespace thelab.mvc
{
	public class Controller : Element
	{
		protected virtual void Start()
		{
			if ((bool)base.app)
			{
				base.app.CacheController(this);
			}
		}

		public virtual void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
		}
	}
	public class Controller<T> : Controller where T : BaseApplication
	{
		public new T app => (T)base.app;
	}
}
