using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class FNCollector : FlowNode
	{
		public SerializedMethod onSucccess;

		public SerializedMethod onFail;

		public int collectablesCount;

		public int minToCollect;

		public bool autoEnd;

		private List<Collectable> mCollectablesCollected = new List<Collectable>();

		public int Collected
		{
			get
			{
				if (mCollectablesCollected == null)
				{
					return 0;
				}
				return mCollectablesCollected.Count;
			}
		}

		internal override void OnInitialize()
		{
			base.OnInitialize();
			status = FlowStatus.Running;
			Collectable.OnCollected += OnCollected;
			if (mCollectablesCollected != null && mCollectablesCollected.Count > 0)
			{
				ResetCollectables();
			}
		}

		internal override FlowStatus OnUpdate()
		{
			return status;
		}

		private void OnCollected(Collectable collectable, bool end)
		{
			if (!mCollectablesCollected.Contains(collectable) && !end)
			{
				mCollectablesCollected.Add(collectable);
			}
			if (!end && (!autoEnd || mCollectablesCollected.Count < collectablesCount))
			{
				return;
			}
			status = FlowStatus.Complete;
			Collectable.OnCollected -= OnCollected;
			Debug.Log("Total collected: " + mCollectablesCollected.Count);
			if (mCollectablesCollected.Count >= minToCollect)
			{
				if (onSucccess != null)
				{
					onSucccess.Invoke();
				}
				return;
			}
			ResetCollectables();
			collectable.Reset();
			if (onFail != null)
			{
				onFail.Invoke();
			}
		}

		public void ResetCollectables()
		{
			foreach (Collectable item in mCollectablesCollected)
			{
				item.Reset();
			}
			mCollectablesCollected.Clear();
		}

		private void OnDestroy()
		{
			mCollectablesCollected.Clear();
			Collectable.OnCollected -= OnCollected;
		}
	}
}
