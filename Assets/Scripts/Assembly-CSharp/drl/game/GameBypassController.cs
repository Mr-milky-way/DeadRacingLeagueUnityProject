using System;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class GameBypassController : Controller<DRLApp>
	{
		private new void Start()
		{
			PerformCleanup(delegate
			{
				base.app.scene.Load();
			});
		}

		private void PerformCleanup(Action p_callback = null)
		{
			AsyncOperation o = Resources.UnloadUnusedAssets();
			((Component)this).TimerRun((Predicate<float>)delegate
			{
				if (!o.isDone)
				{
					return true;
				}
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
				GC.WaitForPendingFinalizers();
				p_callback?.Invoke();
				return false;
			}, 0f);
		}
	}
}
