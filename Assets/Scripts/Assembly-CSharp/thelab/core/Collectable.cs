using System;
using UnityEngine;

namespace thelab.core
{
	public class Collectable : MonoBehaviour
	{
		[SerializeField]
		private bool m_endOfCollection;

		[SerializeField]
		private bool m_popOnCollection = true;

		private bool mCollected;

		private Balloon mBalloonPopEffect;

		public static event Action<Collectable, bool> OnCollected;

		private void Awake()
		{
			if (m_popOnCollection)
			{
				mBalloonPopEffect = GetComponent<Balloon>();
				if (!mBalloonPopEffect)
				{
					mBalloonPopEffect = base.gameObject.AddComponent<Balloon>();
				}
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (Collectable.OnCollected != null && !mCollected)
			{
				mCollected = true;
				Collectable.OnCollected(this, m_endOfCollection);
				if (m_popOnCollection)
				{
					mBalloonPopEffect.Pop();
				}
			}
		}

		public void Reset()
		{
			mCollected = false;
			if (m_popOnCollection)
			{
				mBalloonPopEffect.Reset();
			}
		}
	}
}
