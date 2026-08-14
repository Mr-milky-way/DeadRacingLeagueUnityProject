using System.Collections.Generic;
using UnityEngine;

namespace drl
{
	public class MABreakGlassAction : MapAssetAction
	{
		[Space(5f)]
		[Header("Animation")]
		public Animator animation;

		public AnimationClip clip;

		public AnimationCurve animationSpeedByVelocity;

		public List<GameObject> animationDisableList;

		[Space(5f)]
		[Header("State")]
		public bool isBroken;

		public List<Collider> sortedObjects;

		[Space(5f)]
		[Header("Playback")]
		[Range(5f, 120f)]
		public float clipObjectSpeed = 30f;

		private float m_last_cz;

		private Collider m_current_collider;

		private Rigidbody m_current_rigidbody;

		private AssetActionEvent m_event;

		protected void Start()
		{
			evaluateLength = (clip ? clip.averageDuration : 0f);
			m_event = new AssetActionEvent
			{
				target = this
			};
		}

		public override void SetActive(bool p_flag)
		{
			base.SetActive(p_flag);
			if ((bool)animation)
			{
				animation.gameObject.SetActive(p_flag);
			}
		}

		public override void Evaluate(float p_time)
		{
			float num = animationSpeedByVelocity.Evaluate(clipObjectSpeed);
			evaluateSpeed = num;
			base.Evaluate(p_time);
		}

		protected override void OnActionEvaluate(float p_time)
		{
			float num = evaluateRatio;
			if (num > 0f)
			{
				SetAnimationDisabledList(p_flag: false);
				animation.gameObject.SetActive(value: true);
				animation.Play(clip.name ?? "", 0, num);
				animation.speed = 0f;
			}
			else
			{
				animation.gameObject.SetActive(value: false);
				SetAnimationDisabledList(p_flag: true);
			}
		}

		public float GetTriggerLocalZ(Vector3 p_position)
		{
			Transform transform = (trigger ? trigger.transform : null);
			if (!transform)
			{
				return -1000f;
			}
			return transform.InverseTransformPoint(p_position).z;
		}

		protected override void OnActionTriggerStart()
		{
			m_last_cz = 1000f;
		}

		protected override void OnActionTriggerComplete()
		{
		}

		protected override bool OnActionUpdate()
		{
			if (isBroken)
			{
				return true;
			}
			sortedObjects.Clear();
			sortedObjects.AddRange(objects);
			sortedObjects.Sort(TriggerLocalZSort);
			if (sortedObjects.Count <= 0)
			{
				return true;
			}
			Collider collider = sortedObjects[0];
			if (collider != m_current_collider)
			{
				m_current_rigidbody = collider.attachedRigidbody;
			}
			m_current_collider = collider;
			float triggerLocalZ = GetTriggerLocalZ(collider.transform.position);
			bool num = m_last_cz < triggerLocalZ;
			m_last_cz = triggerLocalZ;
			if (num && !isBroken && triggerLocalZ > -0.3f)
			{
				float num2 = (m_current_rigidbody ? (m_current_rigidbody.velocity.magnitude * 3.6f) : 40f);
				float speed = animationSpeedByVelocity.Evaluate(num2);
				if ((bool)animation)
				{
					animation.speed = speed;
				}
				Break();
				if (OnEvent != null)
				{
					m_event.data = num2;
					OnEvent.Invoke(m_event);
				}
			}
			return true;
		}

		protected int TriggerLocalZSort(Collider a, Collider b)
		{
			float triggerLocalZ = GetTriggerLocalZ(a.transform.position);
			float triggerLocalZ2 = GetTriggerLocalZ(b.transform.position);
			if (!(triggerLocalZ > triggerLocalZ2))
			{
				return 1;
			}
			return -1;
		}

		public void SetAnimationEnabled(bool p_flag)
		{
			if ((bool)animation)
			{
				animation.gameObject.SetActive(p_flag);
				if (p_flag)
				{
					animation.Play(clip.name ?? "", 0, 0f);
				}
			}
			SetAnimationDisabledList(!p_flag);
		}

		protected void SetAnimationDisabledList(bool p_flag)
		{
			for (int i = 0; i < animationDisableList.Count; i++)
			{
				animationDisableList[i].SetActive(p_flag);
			}
		}

		protected override void OnActionRestore()
		{
			if (Application.isPlaying)
			{
				SetAnimationEnabled(p_flag: false);
				isBroken = false;
				m_last_cz = 1000f;
			}
		}

		[ContextMenu("Break")]
		public void Break()
		{
			if (Application.isPlaying)
			{
				SetAnimationEnabled(p_flag: true);
				isBroken = true;
			}
		}
	}
}
