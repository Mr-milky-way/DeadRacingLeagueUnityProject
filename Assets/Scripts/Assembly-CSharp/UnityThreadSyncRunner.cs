using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityThreadSyncRunner : MonoBehaviour
{
	public static UnityThreadSyncRunner Instance;

	private static Queue<Action> queue;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			queue = new Queue<Action>();
		}
		else
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (queue.Count > 0)
		{
			queue.Dequeue()();
		}
	}

	public void EnqueueAction(Action action)
	{
		if (action != null)
		{
			queue.Enqueue(action);
		}
	}
}
