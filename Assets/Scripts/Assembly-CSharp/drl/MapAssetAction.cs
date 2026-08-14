using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using thelab.core;

namespace drl
{
	public class MapAssetAction : ActivityBehaviour, IUpdateable
	{
		[Space(5f)]
		[Header("Attributes")]
		public MapAssetActionMode mode;

		public new GameFlag tag;

		public Collider trigger;

		public bool active;

		public List<Collider> objects;

		public UnityEvent<AssetActionEvent> OnEvent;

		[Space(5f)]
		[Header("Evaluation")]
		[Range(0f, 10f)]
		public float evaluateTimeline;

		public float evaluateStartTime;

		public float evaluateLength;

		public float evaluateSpeed = 1f;

		[Range(0f, 1f)]
		public float evaluateRatio;

		private float m_prev_evaluate_time;

		protected void OnTriggerEnter(Collider p_collider)
		{
			if (mode != MapAssetActionMode.Manual && !objects.Contains(p_collider))
			{
				objects.Add(p_collider);
				active = objects.Count > 0;
				if (objects.Count == 1)
				{
					OnActionTriggerStart();
				}
			}
		}

		protected void OnTriggerExit(Collider p_collider)
		{
			if (mode != MapAssetActionMode.Manual && objects.Contains(p_collider))
			{
				objects.Remove(p_collider);
				active = objects.Count > 0;
				if (objects.Count <= 0)
				{
					OnActionTriggerComplete();
				}
			}
		}

		public Collider GetClosestObject()
		{
			if (objects.Count <= 0)
			{
				return null;
			}
			if (!trigger)
			{
				return null;
			}
			Collider collider = objects[0];
			if (!collider)
			{
				return null;
			}
			float num = Vector3.Distance(collider.transform.position, trigger.transform.position);
			for (int i = 1; i < objects.Count; i++)
			{
				Collider collider2 = objects[i];
				float num2 = Vector3.Distance(collider2.transform.position, trigger.transform.position);
				if (!(num2 >= num))
				{
					num = num2;
					collider = collider2;
				}
			}
			return collider;
		}

		public virtual void Evaluate(float p_time)
		{
			float num = evaluateStartTime;
			float num2 = Mathf.Max(0f, p_time - num);
			float num3 = evaluateLength / evaluateSpeed;
			evaluateRatio = Mathf.Clamp01(num2 / num3);
			OnActionEvaluate(num2);
		}

		public virtual void SetActive(bool p_flag)
		{
			base.gameObject.SetActive(p_flag);
		}

		[ContextMenu("Restore")]
		public virtual void Restore()
		{
			OnActionRestore();
		}

		protected virtual void OnActionEvaluate(float p_time)
		{
		}

		protected virtual void OnActionTriggerStart()
		{
		}

		protected virtual void OnActionTriggerComplete()
		{
		}

		protected virtual bool OnActionUpdate()
		{
			return true;
		}

		protected virtual void OnActionRestore()
		{
		}

		public virtual void OnUpdate()
		{
			switch (mode)
			{
			case MapAssetActionMode.Manual:
				if (Application.isPlaying)
				{
					bool num = Mathf.Abs(m_prev_evaluate_time - evaluateTimeline) > 0f;
					m_prev_evaluate_time = evaluateTimeline;
					if (num)
					{
						float p_time = evaluateTimeline;
						Evaluate(p_time);
					}
				}
				break;
			case MapAssetActionMode.Auto:
				if (active && !OnActionUpdate())
				{
					active = false;
				}
				break;
			}
		}
	}
}
