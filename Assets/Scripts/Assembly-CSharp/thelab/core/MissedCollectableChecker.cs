using System;
using UnityEngine;

namespace thelab.core
{
	public class MissedCollectableChecker : MonoBehaviour
	{
		private Collectable mCollectableToCheck;

		private bool mHasBeenCollected;

		private int mCollidersOnTrigger;

		public static event Action<Collectable> OnMissedCollectable;

		private void Start()
		{
			mCollectableToCheck = GetComponentInChildren<Collectable>();
		}

		private void OnTriggerEnter(Collider other)
		{
			if (mCollidersOnTrigger == 0)
			{
				Collectable.OnCollected += OnCollectableCollected;
				mHasBeenCollected = false;
				mCollidersOnTrigger++;
			}
		}

		private void OnCollectableCollected(Collectable col, bool isEnd)
		{
			if (col == mCollectableToCheck)
			{
				mHasBeenCollected = true;
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (mCollidersOnTrigger != 1)
			{
				return;
			}
			Collectable.OnCollected -= OnCollectableCollected;
			if (!mHasBeenCollected)
			{
				if (MissedCollectableChecker.OnMissedCollectable != null)
				{
					MissedCollectableChecker.OnMissedCollectable(mCollectableToCheck);
				}
				Debug.Log("Collectable " + mCollectableToCheck.name + " has been missed");
			}
			mCollidersOnTrigger--;
		}
	}
}
