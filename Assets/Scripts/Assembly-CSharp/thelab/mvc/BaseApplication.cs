using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace thelab.mvc
{
	public class BaseApplication<M, V, C> : BaseApplication where M : Element where V : Element where C : Element
	{
		public new M model => (M)(Element)base.model;

		public new V view => (V)(Element)base.view;

		public new C controller => (C)(Element)base.controller;
	}
	[ExecuteInEditMode]
	public class BaseApplication : Element
	{
		private static List<string> __args;

		public int verbose;

		public bool profilerEnabled;

		private Model m_model;

		private View m_view;

		private Controller m_controller;

		private List<AsyncOperation> __async_loads;

		private List<string> __async_args;

		[SerializeField]
		private List<Controller> m_controllers;

		public List<string> args
		{
			get
			{
				if (__args != null)
				{
					return __args;
				}
				return new List<string>();
			}
		}

		public Model model
		{
			get
			{
				if (!m_model)
				{
					return m_model = Assert(m_model);
				}
				return m_model;
			}
		}

		public View view
		{
			get
			{
				if (!m_view)
				{
					return m_view = Assert(m_view);
				}
				return m_view;
			}
		}

		public Controller controller
		{
			get
			{
				if (!m_controller)
				{
					return m_controller = Assert(m_controller);
				}
				return m_controller;
			}
		}

		public int levelId => SceneManager.GetActiveScene().buildIndex;

		public string levelName => SceneManager.GetActiveScene().name;

		private List<AsyncOperation> m_async_loads
		{
			get
			{
				if (__async_loads != null)
				{
					return __async_loads;
				}
				return __async_loads = new List<AsyncOperation>();
			}
		}

		private List<string> m_async_args
		{
			get
			{
				if (__async_args != null)
				{
					return __async_args;
				}
				return __async_args = new List<string>();
			}
		}

		public List<Controller> controllers
		{
			get
			{
				if (m_controllers != null)
				{
					return m_controllers;
				}
				return m_controllers = new List<Controller>();
			}
		}

		protected virtual void Awake()
		{
			SceneManager.sceneLoaded += OnLevelLoaded;
		}

		protected void Start()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			__async_loads = new List<AsyncOperation>();
			__async_args = new List<string>();
			Traverse(delegate(Transform it)
			{
				Controller[] components = it.GetComponents<Controller>();
				for (int i = 0; i < components.Length; i++)
				{
					CacheController(components[i]);
				}
				return true;
			});
			Notify(0.01f, "scene.start", levelName, levelId);
		}

		internal void CacheController(Controller p_target)
		{
			if (!controllers.Contains(p_target))
			{
				controllers.Add(p_target);
			}
		}

		protected virtual void OnLevelLoaded(Scene p_scene, LoadSceneMode p_mode)
		{
			if (Application.isPlaying)
			{
				Notify(0.01f, "scene.load", this, levelName, levelId);
			}
		}

		protected void OnLevelLoaded()
		{
			OnLevelLoaded(default(Scene), LoadSceneMode.Single);
		}

		public void Notify(string p_event, Object p_target, params object[] p_data)
		{
			for (int i = 0; i < controllers.Count; i++)
			{
				Controller controller = controllers[i];
				if (!controller)
				{
					controllers.RemoveAt(i--);
				}
				else if (controller.gameObject.activeInHierarchy && controller.enabled)
				{
					if (profilerEnabled)
					{
						_ = Profiler.enabled;
					}
					controller.OnNotification(p_event, p_target, p_data);
					if (profilerEnabled)
					{
						_ = Profiler.enabled;
					}
				}
			}
		}

		public void Notify(float p_delay, string p_event, Object p_target, params object[] p_data)
		{
			StartCoroutine(TimedNotify(p_delay, p_event, p_target, p_data));
		}

		private IEnumerator TimedNotify(float p_delay, string p_event, Object p_target, params object[] p_data)
		{
			float num = ((Time.timeScale <= 0f) ? 0f : Time.timeScale);
			yield return new WaitForSeconds(p_delay * num);
			Notify(p_event, p_target, p_data);
		}

		public void SceneAdd(string p_name, bool p_async, params string[] p_args)
		{
			if (p_async)
			{
				StartCoroutine(SceneLoadAsync(p_name, p_additive: true, p_args));
				return;
			}
			__args = new List<string>(p_args);
			SceneManager.LoadScene(p_name, LoadSceneMode.Additive);
		}

		public void SceneAdd(string p_name, params string[] p_args)
		{
			SceneAdd(p_name, p_async: false, p_args);
		}

		public void SceneLoad(string p_name, bool p_async, params string[] p_args)
		{
			if (p_async)
			{
				StartCoroutine(SceneLoadAsync(p_name, p_additive: false, p_args));
				return;
			}
			__args = new List<string>(p_args);
			SceneManager.LoadScene(p_name, LoadSceneMode.Single);
		}

		public void SceneLoad(string p_name, params string[] p_args)
		{
			SceneLoad(p_name, p_async: false, p_args);
		}

		private IEnumerator SceneLoadAsync(string p_name, bool p_additive, params string[] p_args)
		{
			__args = new List<string>(p_args);
			string text;
			AsyncOperation asyncOperation;
			if (p_additive)
			{
				text = "scene.add.progress";
				asyncOperation = SceneManager.LoadSceneAsync(p_name, LoadSceneMode.Additive);
			}
			else
			{
				text = "scene.load.progress";
				asyncOperation = SceneManager.LoadSceneAsync(p_name, LoadSceneMode.Single);
			}
			m_async_loads.Add(asyncOperation);
			m_async_args.Add(p_name + "~" + text);
			yield return asyncOperation;
		}

		private void Update()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			for (int i = 0; i < m_async_loads.Count; i++)
			{
				AsyncOperation asyncOperation = m_async_loads[i];
				if (asyncOperation != null)
				{
					string text = m_async_args[i];
					string text2 = text.Split('~')[0];
					string text3 = text.Split('~')[1];
					float num = Mathf.Clamp01(asyncOperation.progress / 0.9f);
					if (text3 != "")
					{
						Notify(text3, text2, num);
					}
					if ((double)asyncOperation.progress >= 1.0)
					{
						m_async_loads[i] = null;
					}
				}
				else
				{
					if (i < m_async_loads.Count)
					{
						m_async_loads.RemoveAt(i--);
					}
					if (i < m_async_args.Count)
					{
						m_async_args.RemoveAt(i--);
					}
				}
			}
		}

		public virtual void Quit()
		{
		}

		protected void OnDestroy()
		{
			SceneManager.sceneLoaded -= OnLevelLoaded;
		}
	}
}
