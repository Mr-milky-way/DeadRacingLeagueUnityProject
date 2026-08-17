using UnityEngine;

namespace drl.sim
{
	public class DroneSensor : DronePart
	{
		protected bool m_enabled;

		public bool usePhysics;

		private GameObject m_game_object;

		private Transform m_drone_transform;

		private bool m_active_hierarchy;

		private bool m_base_enabled;

		private float m_lastFixedStepTime = float.NegativeInfinity;

		public new bool enabled => m_enabled;

		public new GameObject gameObject
		{
			get
			{
				if (!m_game_object)
				{
					return m_game_object = base.gameObject;
				}
				return m_game_object;
			}
		}

		public Transform droneTransform
		{
			get
			{
				if (!m_drone_transform)
				{
					return m_drone_transform = base.drone.transform;
				}
				return m_drone_transform;
			}
		}

		protected override void OnInitialize()
		{
			Refresh(0f);
		}

		protected virtual void Refresh(float p_dt)
		{
		}

		public virtual void Reset()
		{
			Refresh(0f);
		}

		public void FixedStep(float p_deltaTime)
		{
			if (m_lastFixedStepTime == Time.fixedTime)
			{
				return;
			}
			m_lastFixedStepTime = Time.fixedTime;
			m_enabled = m_base_enabled && m_active_hierarchy;
			if (usePhysics)
			{
				Refresh(p_deltaTime);
			}
		}

		protected virtual void FixedUpdate()
		{
			FixedStep(Time.fixedDeltaTime);
		}

		protected virtual void LateUpdate()
		{
			m_active_hierarchy = gameObject.activeInHierarchy;
			m_base_enabled = base.enabled;
			if (!usePhysics)
			{
				Refresh(Time.deltaTime);
			}
		}

		public override string GetPrefix()
		{
			return "S";
		}
	}
}
