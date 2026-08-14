using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
	private static T m_instance;

	public static T Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = Object.FindObjectOfType<T>();
				if (m_instance == null)
				{
					m_instance = new GameObject(typeof(T).Name).AddComponent<T>();
				}
				if (!m_instance.Initialized)
				{
					m_instance.Initialize();
					m_instance.Initialized = true;
				}
			}
			return m_instance;
		}
	}

	protected bool Initialized { get; set; }

	private void Awake()
	{
		Debug.Log("SingletonMonoBehaviour> Awake");
		if (m_instance != null)
		{
			Object.DestroyImmediate(base.gameObject);
		}
		else
		{
			m_instance = Instance;
		}
	}

	protected virtual void Initialize()
	{
	}
}
