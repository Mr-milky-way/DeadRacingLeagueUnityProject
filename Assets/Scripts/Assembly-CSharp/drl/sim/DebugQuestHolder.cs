using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace drl.sim
{
	public class DebugQuestHolder : MonoBehaviour
	{
		[Serializable]
		public class TestEvent : UnityEvent<string>
		{
		}

		public Transform holder;

		public Text title;
	}
}
