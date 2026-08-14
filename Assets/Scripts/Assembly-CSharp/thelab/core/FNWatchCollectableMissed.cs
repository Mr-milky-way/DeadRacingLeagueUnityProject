using UnityEngine;

namespace thelab.core
{
	public class FNWatchCollectableMissed : FlowNode
	{
		public int missedThreshold;

		private int totalMissed;

		internal override void OnInitialize()
		{
			base.OnInitialize();
			status = FlowStatus.Running;
			MissedCollectableChecker.OnMissedCollectable += OnMissedCollectable;
			Collectable.OnCollected += OnCollected;
		}

		private void OnCollected(Collectable collectable, bool isEnd)
		{
			if (isEnd)
			{
				totalMissed = 0;
				MissedCollectableChecker.OnMissedCollectable -= OnMissedCollectable;
				Collectable.OnCollected -= OnCollected;
			}
		}

		private void OnMissedCollectable(Collectable missedCol)
		{
			Debug.Log(missedCol?.ToString() + " missed");
			totalMissed++;
			if (totalMissed > missedThreshold)
			{
				status = FlowStatus.Complete;
			}
		}

		internal override FlowStatus OnUpdate()
		{
			return status;
		}
	}
}
