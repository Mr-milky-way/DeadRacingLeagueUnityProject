using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MEGraphLayer : View<DRLApp>
	{
		public enum SplineMode
		{
			None = 0,
			Screen = 1,
			Scene = 2
		}

		[Serializable]
		public class SplineCanvas
		{
			public SplineMode mode;

			public SplineRenderer screen;

			public SplineRenderer scene;

			public SplineRenderer current
			{
				get
				{
					if (mode != SplineMode.None)
					{
						if (mode != SplineMode.Screen)
						{
							return scene;
						}
						return screen;
					}
					return null;
				}
			}

			public bool active
			{
				get
				{
					bool num = (bool)screen && screen.gameObject.activeInHierarchy;
					bool flag = (bool)scene && scene.gameObject.activeInHierarchy;
					return num || flag;
				}
			}

			public float alpha
			{
				get
				{
					if (mode == SplineMode.Scene && (bool)scene)
					{
						return scene.alpha;
					}
					if (mode == SplineMode.Screen && (bool)screen)
					{
						return screen.alpha;
					}
					return 0f;
				}
				set
				{
					if (mode == SplineMode.Scene && (bool)scene)
					{
						scene.alpha = value;
					}
					if (mode == SplineMode.Screen && (bool)screen)
					{
						screen.alpha = value;
					}
				}
			}

			public void Init()
			{
				if ((bool)scene)
				{
					scene.name = "me-graph-scene$" + scene.transform.parent.parent.name;
					scene.transform.parent = null;
					scene.transform.position = Vector3.zero;
					scene.transform.eulerAngles = Vector3.zero;
					scene.transform.localScale = Vector3.one;
				}
			}

			public void Set(IList p_list)
			{
				if (!active)
				{
					return;
				}
				for (int i = 0; i < p_list.Count; i++)
				{
					switch (mode)
					{
					case SplineMode.Screen:
					{
						RectTransform obj2 = ((i >= screen.transform.childCount) ? ((RectTransform)new GameObject("", typeof(RectTransform)).transform) : ((RectTransform)screen.transform.GetChild(i)));
						obj2.name = i.ToString("00");
						Vector2 anchorMin = (obj2.anchorMax = new Vector2(0f, 1f));
						obj2.anchorMin = anchorMin;
						obj2.SetParent(screen.transform, worldPositionStays: true);
						break;
					}
					case SplineMode.Scene:
					{
						Transform obj = ((i >= scene.transform.childCount) ? new GameObject().transform : scene.transform.GetChild(i));
						obj.name = i.ToString("00");
						obj.SetParent(scene.transform, worldPositionStays: true);
						obj.localScale = Vector3.one;
						break;
					}
					}
				}
				Refresh(p_list);
			}

			public void SetEnabled(bool p_flag)
			{
				if ((bool)screen)
				{
					screen.gameObject.SetActive(p_flag);
				}
				if ((bool)scene)
				{
					scene.gameObject.SetActive(p_flag);
				}
			}

			public void Clear()
			{
				SplineRenderer splineRenderer = scene;
				if ((bool)splineRenderer)
				{
					splineRenderer.Clear();
					Transform transform = splineRenderer.transform;
					for (int i = 0; i < transform.childCount; i++)
					{
						UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
					}
				}
				splineRenderer = screen;
				if ((bool)splineRenderer)
				{
					splineRenderer.Clear();
					Transform transform = splineRenderer.transform;
					for (int j = 0; j < transform.childCount; j++)
					{
						UnityEngine.Object.Destroy(transform.GetChild(j).gameObject);
					}
				}
			}

			public void Refresh(IList p_list)
			{
				if (p_list == null)
				{
					return;
				}
				bool flag = false;
				for (int i = 0; i < p_list.Count; i++)
				{
					switch (mode)
					{
					case SplineMode.Screen:
					{
						RectTransform rectTransform = (RectTransform)p_list[i];
						if ((bool)rectTransform)
						{
							RectTransform rectTransform2 = (RectTransform)screen.transform.GetChild(i);
							if ((bool)rectTransform2)
							{
								rectTransform2.anchoredPosition = rectTransform.anchoredPosition;
								flag = true;
							}
						}
						break;
					}
					case SplineMode.Scene:
					{
						Component component = (Component)p_list[i];
						Transform transform = (component ? component.transform : null);
						if ((bool)transform)
						{
							Transform child = scene.transform.GetChild(i);
							Vector3 position = transform.position;
							if (component is MAGate)
							{
								position = ((MAGate)component).triggerCenter;
							}
							child.position = position;
							child.rotation = transform.rotation;
							child.localScale = Vector3.one;
							flag = true;
						}
						break;
					}
					}
				}
				if (flag)
				{
					switch (mode)
					{
					case SplineMode.Screen:
						screen.spline.Refresh();
						screen.Refresh();
						break;
					case SplineMode.Scene:
						scene.spline.Refresh();
						scene.Refresh();
						break;
					case SplineMode.None:
						break;
					}
				}
			}

			public void Destroy()
			{
				SplineRenderer splineRenderer = scene;
				if ((bool)splineRenderer)
				{
					splineRenderer.Clear();
					UnityEngine.Object.Destroy(splineRenderer.gameObject);
				}
				splineRenderer = screen;
				if ((bool)splineRenderer)
				{
					splineRenderer.Clear();
					UnityEngine.Object.Destroy(splineRenderer.gameObject);
				}
			}
		}

		public MapEditorController editor;

		public ListComponent nodes;

		public SplineRenderer screenSpline;

		public SplineCanvas spline;

		public List<Component> targets;

		public RectOffset margin;

		public Vector2 screenOffset;

		public float groupRadius = 47.5f;

		public float groupHeight = 32f;

		private Vector3[] m_nlist;

		private List<List<int>> m_ngroups;

		private List<int> m_nbuffer;

		private MonoActivity m_clear_timer;

		public float alpha
		{
			get
			{
				return fade.alpha;
			}
			set
			{
				fade.alpha = value;
				spline.alpha = value;
			}
		}

		public Canvas canvas => Assert<Canvas>("canvas");

		public Camera camera => editor.view.camera.main;

		public FadeComponent fade => Assert<FadeComponent>("fade");

		protected void Awake()
		{
			m_nlist = new Vector3[0];
			m_ngroups = new List<List<int>>();
			m_nbuffer = new List<int>();
			spline.Init();
		}

		public void Set<T>(List<T> p_targets) where T : Component
		{
			Clear();
			List<Component> list = (targets = new List<Component>(p_targets));
			for (int i = 0; i < list.Count; i++)
			{
				nodes.Push();
			}
			spline.Set(list);
			m_nlist = new Vector3[nodes.Count];
			m_ngroups = new List<List<int>>();
			m_nbuffer = new List<int>();
			Refresh(p_force: true);
		}

		public List<T> GetNodes<T>() where T : Component
		{
			return nodes.GetList<T>();
		}

		public int GetNodeIndex<T>(T p_target) where T : Component
		{
			return GetNodes<T>().IndexOf(p_target);
		}

		public void Clear(float p_delay = 0f)
		{
			if (m_clear_timer != null)
			{
				m_clear_timer.Stop();
			}
			if (p_delay <= 0f)
			{
				targets.Clear();
				nodes.Clear();
				spline.Clear();
				return;
			}
			m_clear_timer = RunOnce(delegate
			{
				targets.Clear();
				nodes.Clear();
				spline.Clear();
			}, p_delay);
		}

		public void Refresh(bool p_force = false)
		{
			bool flag = false || camera.transform.hasChanged;
			for (int i = 0; i < targets.Count; i++)
			{
				flag = flag || targets[i].transform.hasChanged;
			}
			if (!p_force && !flag)
			{
				return;
			}
			Vector3[] nlist = m_nlist;
			List<List<int>> ngroups = m_ngroups;
			List<int> nbuffer = m_nbuffer;
			float num = groupRadius * 2f;
			nbuffer.Clear();
			ngroups.Clear();
			_ = Screen.width;
			_ = Screen.height;
			for (int j = 0; j < targets.Count; j++)
			{
				Component component = targets[j];
				if (!component)
				{
					continue;
				}
				MAEntity mAEntity = component as MAEntity;
				Vector3 p_position = component.transform.position;
				if ((bool)mAEntity)
				{
					switch (mAEntity.data.type)
					{
					case MapAssetType.Gate:
						p_position = ((MAGate)component).triggerCenter;
						break;
					case MapAssetType.Spline:
						if (component.transform.childCount > 0)
						{
							p_position = component.transform.GetChild(0).position;
						}
						break;
					}
				}
				Vector2 vector = WorldToAnchorPosition(p_position);
				vector.x += screenOffset.x;
				vector.y += screenOffset.y;
				nlist[j] = vector;
				nbuffer.Add(j);
			}
			int num2 = 0;
			while (nbuffer.Count > 0)
			{
				num2++;
				if (num2 >= 500)
				{
					Debug.LogWarning(">>> Infinite Loop AGAIN!");
					break;
				}
				List<int> list = new List<int>();
				int num3 = 0;
				Vector3 a = Vector3.zero;
				if (nbuffer.Count > 0)
				{
					a = nlist[nbuffer[0]];
					list.Add(nbuffer[0]);
					nbuffer.RemoveAt(0);
				}
				do
				{
					_ = list[num3];
					for (int k = 0; k < nbuffer.Count; k++)
					{
						int num4 = nbuffer[k];
						Vector3 b = nlist[num4];
						if (!(Vector3.Distance(a, b) > num))
						{
							a = Vector3.Lerp(a, b, 0.6f);
							list.Add(num4);
							nbuffer.RemoveAt(k--);
						}
					}
					num3++;
				}
				while (num3 < list.Count);
				if (list.Count > 0)
				{
					ngroups.Add(list);
				}
			}
			float num5 = groupHeight;
			float num6 = 10f;
			float num7 = 0f;
			float num8 = 0f;
			for (int l = 0; l < ngroups.Count; l++)
			{
				List<int> list2 = ngroups[l];
				if (list2.Count <= 1)
				{
					continue;
				}
				float num9 = 1f / (float)list2.Count;
				Vector3 vector2 = nlist[list2[0]];
				for (int m = 1; m < list2.Count; m++)
				{
					vector2 += nlist[list2[m]];
				}
				vector2 *= num9;
				float num10 = 0f;
				float num11 = 9999999f;
				float num12 = 0f;
				for (int n = 0; n < list2.Count; n++)
				{
					for (int num13 = n + 1; num13 < list2.Count; num13++)
					{
						int num14 = list2[n];
						int num15 = list2[num13];
						float num16 = Vector3.Distance(nlist[num14], nlist[num15]);
						num10 += num16;
						num12 += 1f;
						num11 = Mathf.Min(num16, num11);
					}
				}
				if (num12 > 0f)
				{
					num10 /= num12;
				}
				num10 *= 0.5f;
				num7 = 0f;
				num8 = ((num5 + num6) * (float)list2.Count - num6) * 0.5f;
				float f = 1f - Mathf.Clamp01(num11 / (num * 0.5f));
				f = Mathf.Clamp01(Mathf.Pow(f, 0.2f) / 0.85f);
				for (int num17 = 0; num17 < list2.Count; num17++)
				{
					int num18 = list2[num17];
					Vector3 a2 = nlist[num18];
					Vector3 b2 = vector2;
					b2.y += 0f - num8 + num7;
					nlist[num18] = Vector3.Lerp(a2, b2, f);
					num7 += num5 + num6;
				}
			}
			int num19 = Mathf.Min(nlist.Length, nodes.Count);
			for (int num20 = 0; num20 < num19; num20++)
			{
				RectTransform rectTransform = nodes.Get<RectTransform>(num20);
				MEControlsWidget component2 = Hierarchy.GetComponent<MEControlsWidget>(rectTransform.gameObject);
				if (!component2 || component2.follow)
				{
					rectTransform.anchoredPosition = nlist[num20];
				}
			}
			if (!spline.active)
			{
				return;
			}
			switch (spline.mode)
			{
			case SplineMode.Screen:
				spline.Refresh(nodes.GetList<RectTransform>());
				break;
			case SplineMode.Scene:
				if (p_force)
				{
					spline.Refresh(targets);
				}
				break;
			case SplineMode.None:
				break;
			}
		}

		protected void LateUpdate()
		{
			if (alpha > 0f)
			{
				Refresh();
			}
		}

		public Vector2 WorldToAnchorPosition(Vector3 p_position)
		{
			return Hierarchy.WorldToAnchorPosition((RectTransform)base.transform, canvas, p_position, margin);
		}

		public void Fade(float p_alpha, float p_duration, float p_delay)
		{
			Tween.Kill(this, "alpha");
			Tween.Add(this, "alpha", p_alpha, p_delay, p_duration, Cubic.Out);
		}

		public void Fade(float p_alpha, float p_duration)
		{
			Fade(p_alpha, p_duration, 0f);
		}

		public void Fade(float p_alpha)
		{
			Fade(p_alpha, 0f, 0.2f);
		}

		public void FadeIn(float p_duration, float p_delay)
		{
			Fade(1f, p_delay, p_duration);
		}

		public void FadeIn(float p_duration)
		{
			Fade(1f, 0f, p_duration);
		}

		public void FadeOut(float p_duration, float p_delay)
		{
			Fade(-0.1f, p_delay, p_duration);
		}

		public void FadeOut(float p_duration)
		{
			Fade(-0.1f, 0f, p_duration);
		}

		protected void OnDestroy()
		{
			if (spline != null)
			{
				spline.Destroy();
			}
		}
	}
}
