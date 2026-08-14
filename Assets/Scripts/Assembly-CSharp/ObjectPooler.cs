using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : SingletonMonoBehaviour<ObjectPooler>
{
	[Serializable]
	public class Pool
	{
		public Component prefab;

		public int size;

		public Transform poolPlaceholder;

		public bool autoResize;
	}

	private List<GameObject> mPool = new List<GameObject>();

	public Pool poolData;

	private int mNextFreeExpectedIdx;

	private int mCyclingPoolingIdx;

	protected override void Initialize()
	{
		if (poolData.poolPlaceholder == null)
		{
			poolData.poolPlaceholder = base.transform;
		}
		for (int i = 0; i < poolData.size; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(poolData.prefab.gameObject, poolData.poolPlaceholder);
			gameObject.SetActive(value: false);
			mPool.Add(gameObject);
		}
	}

	private GameObject NewInstance()
	{
		GameObject obj = UnityEngine.Object.Instantiate(poolData.prefab.gameObject, poolData.poolPlaceholder);
		obj.name = RemoveCloneSubstring(obj.name);
		return obj;
	}

	public GameObject SpawnFromPool(Vector3 position, Quaternion rotation, bool active = true)
	{
		GameObject gameObject = null;
		if (mNextFreeExpectedIdx < mPool.Count && !mPool[mNextFreeExpectedIdx].activeSelf)
		{
			gameObject = mPool[mNextFreeExpectedIdx];
		}
		if (gameObject == null)
		{
			for (int i = 0; i < mPool.Count; i++)
			{
				GameObject gameObject2 = mPool[i];
				if (!gameObject2.activeSelf)
				{
					gameObject = gameObject2;
					mNextFreeExpectedIdx = i;
					break;
				}
			}
		}
		if (poolData.autoResize)
		{
			if (gameObject == null)
			{
				gameObject = NewInstance();
				mPool.Add(gameObject);
			}
		}
		else if (gameObject == null)
		{
			mCyclingPoolingIdx++;
			if (mCyclingPoolingIdx >= mPool.Count)
			{
				mCyclingPoolingIdx = 0;
			}
			gameObject = mPool[mCyclingPoolingIdx];
		}
		gameObject.SetActive(value: false);
		gameObject.SetActive(active);
		gameObject.transform.position = position;
		gameObject.transform.rotation = rotation;
		mNextFreeExpectedIdx++;
		if (mNextFreeExpectedIdx >= mPool.Count)
		{
			mNextFreeExpectedIdx = 0;
		}
		return gameObject;
	}

	public T SpawnFromPool<T>(Vector3 position, Quaternion rotation, bool active = true)
	{
		return SpawnFromPool(position, rotation, active).GetComponent<T>();
	}

	public void UnloadObject(GameObject go)
	{
		go.SetActive(value: false);
		if (go.transform.parent != poolData.poolPlaceholder)
		{
			go.transform.SetParent(poolData.poolPlaceholder);
		}
		go.transform.position = Vector3.zero;
		go.transform.rotation = Quaternion.identity;
	}

	public void ClearPool()
	{
		for (int i = 0; i < mPool.Count; i++)
		{
			GameObject go = mPool[i];
			UnloadObject(go);
		}
	}

	public static string RemoveCloneSubstring(string nameWithClone)
	{
		string text = nameWithClone;
		if (text.Contains("(Clone)"))
		{
			int length = text.IndexOf('(');
			text = text.Substring(0, length);
		}
		return text;
	}
}
