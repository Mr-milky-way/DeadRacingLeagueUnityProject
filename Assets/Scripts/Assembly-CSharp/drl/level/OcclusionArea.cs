using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using thelab.core;

namespace drl.level
{
	public class OcclusionArea : MonoBehaviour
	{
		public OcclusionAreaManager manager;

		public List<Collider> colliders;

		public int activeCount;

		public bool hideOnAwake;

		public List<GameObject> targets;

		public List<GameObject> ignored;

		public List<Renderer> renderers;

		public bool includeShadows = true;

		protected void Start()
		{
			activeCount = 0;
			for (int i = 0; i < colliders.Count; i++)
			{
				Collider collider = colliders[i];
				ColliderEventComponent colliderEventComponent = collider.GetComponent<ColliderEventComponent>();
				if (!colliderEventComponent)
				{
					colliderEventComponent = collider.gameObject.AddComponent<ColliderEventComponent>();
				}
				colliderEventComponent.callback.RemoveAllListeners();
				colliderEventComponent.callback.AddListener(OnAreaEvent);
			}
			if (hideOnAwake)
			{
				Hide();
			}
		}

		protected void OnAreaEvent(ColliderEvent p_event)
		{
			switch (p_event.type)
			{
			case ColliderEvent.Type.Enter:
				if (activeCount == 0)
				{
					if ((bool)manager)
					{
						manager.OnAreaEnter(this);
					}
					else
					{
						Show();
					}
				}
				activeCount++;
				activeCount = Mathf.Clamp(activeCount, 0, colliders.Count);
				break;
			case ColliderEvent.Type.Exit:
				if (activeCount == 1)
				{
					if ((bool)manager)
					{
						manager.OnAreaExit(this);
					}
					else
					{
						Hide();
					}
				}
				activeCount--;
				activeCount = Mathf.Clamp(activeCount, 0, colliders.Count);
				break;
			}
		}

		[ContextMenu("Collect Renderers")]
		public void CollectRenderers()
		{
			renderers = new List<Renderer>();
			for (int i = 0; i < targets.Count; i++)
			{
				GameObject gameObject = targets[i];
				if (!gameObject)
				{
					continue;
				}
				List<Renderer> list = Hierarchy.FindAll<Renderer>(gameObject.transform);
				for (int j = 0; j < list.Count; j++)
				{
					if (!includeShadows && list[j].shadowCastingMode != ShadowCastingMode.Off)
					{
						continue;
					}
					GameObject gameObject2 = list[j].gameObject;
					bool flag = false;
					for (int k = 0; k < ignored.Count; k++)
					{
						GameObject gameObject3 = ignored[k];
						if (gameObject3 == gameObject2)
						{
							flag = true;
							break;
						}
						if (gameObject2.transform.IsChildOf(gameObject3.transform))
						{
							flag = true;
							break;
						}
						OcclusionArea component = gameObject3.GetComponent<OcclusionArea>();
						if ((bool)component && component.renderers.Contains(list[j]))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						renderers.Add(list[j]);
					}
				}
			}
		}

		[ContextMenu("Clear Renderers")]
		public void ClearRenderers()
		{
			renderers = new List<Renderer>();
		}

		public GameObject GetParentTarget(Renderer p_renderer)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				if ((bool)targets[i] && p_renderer.transform.IsChildOf(targets[i].transform))
				{
					return targets[i];
				}
			}
			return null;
		}

		public void SetVisible(bool p_flag)
		{
			if (renderers != null)
			{
				for (int i = 0; i < renderers.Count; i++)
				{
					renderers[i].enabled = p_flag;
				}
			}
		}

		[ContextMenu("Show")]
		public void Show()
		{
			SetVisible(p_flag: true);
		}

		[ContextMenu("Hide")]
		public void Hide()
		{
			SetVisible(p_flag: false);
		}
	}
}
