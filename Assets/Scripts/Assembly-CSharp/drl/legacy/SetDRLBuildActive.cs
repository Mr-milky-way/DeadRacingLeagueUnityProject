using UnityEngine;
using drl.game;
using thelab.mvc;

namespace drl.legacy
{
	public class SetDRLBuildActive : View<DRLApp>
	{
		public enum Mode
		{
			Release = 0,
			Development = 1,
			Paywall = 2,
			Offline = 3,
			CustomEvent = 4,
			Xbox = 5,
			PS4 = 6,
			Console = 7
		}

		public Mode mode = Mode.Development;

		public bool flag;

		public bool destroy;

		public Object[] targets;

		protected void Awake()
		{
			bool flag = true;
			bool flag2 = true;
			bool flag3 = false;
			flag = mode == Mode.Release;
			if (mode != Mode.Development && mode != Mode.Release && mode != Mode.Offline && mode != Mode.CustomEvent && mode != Mode.Xbox && mode != Mode.PS4 && mode != Mode.Console)
			{
				flag = true;
			}
			if (mode == Mode.Paywall && base.validContext)
			{
				if (!base.app.model.service.platform.ready)
				{
					flag2 = false;
				}
				if (base.app.model.storage.state.license.exists)
				{
					flag2 = false;
				}
			}
			if (!((flag && flag2) || flag3))
			{
				return;
			}
			Object[] array = targets;
			foreach (Object obj in array)
			{
				if (obj is Behaviour)
				{
					((Behaviour)obj).enabled = this.flag;
				}
				if (obj is GameObject)
				{
					((GameObject)obj).SetActive(this.flag);
				}
				if (destroy)
				{
					Object.Destroy(obj);
				}
			}
		}
	}
}
