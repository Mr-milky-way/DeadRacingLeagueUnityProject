using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Gate : TrainingElement, IPoppable
{
	[SerializeField]
	private bool m_playSpawnOnEnable;

	[SerializeField]
	private bool m_popOnHit;

	[SerializeField]
	private bool m_playPopOnHit;

	private Animator m_animator;

	private PodiumMoveTrigger m_podiumMoveTrigger;

	private void OnEnable()
	{
		if (!m_animator)
		{
			m_animator = GetComponent<Animator>();
		}
		if (m_playSpawnOnEnable)
		{
			Spawn();
		}
	}

	public void Disable()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Spawn()
	{
		if ((bool)m_animator)
		{
			m_animator.enabled = true;
			m_animator.Play("Spawn");
		}
	}

	public void Pop()
	{
		if (m_popOnHit)
		{
			if (m_playPopOnHit && (bool)m_animator)
			{
				m_animator.enabled = true;
				m_animator.Play("Pop");
			}
			else
			{
				SetActive(active: false);
			}
		}
	}

	public override void Reset(bool setActive = false)
	{
		base.Reset(setActive);
		if ((bool)m_animator)
		{
			m_animator.enabled = true;
			m_animator.Play("Reset");
		}
	}
}
