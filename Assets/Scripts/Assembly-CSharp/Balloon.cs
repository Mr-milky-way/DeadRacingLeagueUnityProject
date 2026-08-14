using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Balloon : TrainingElement, IPoppable
{
	[SerializeField]
	public List<GameObject> m_activateOnPop;

	private Animator m_animator;

	private PodiumMoveTrigger m_podiumMoveTrigger;

	[HideInInspector]
	public bool playSpawnAnimation = true;

	private void OnEnable()
	{
		if (!m_animator)
		{
			m_animator = GetComponent<Animator>();
		}
		if (playSpawnAnimation)
		{
			Spawn();
		}
	}

	public void Disable()
	{
		base.gameObject.SetActive(value: false);
	}

	private void Start()
	{
		m_podiumMoveTrigger = GetComponent<PodiumMoveTrigger>();
	}

	public void Pop()
	{
		if ((bool)m_podiumMoveTrigger)
		{
			m_podiumMoveTrigger.TriggerPodiumMove(base.transform);
		}
		if ((bool)m_animator)
		{
			m_animator.enabled = true;
			m_animator.Play("Pop");
		}
		if (m_activateOnPop == null)
		{
			return;
		}
		foreach (GameObject item in m_activateOnPop)
		{
			item.SetActive(value: true);
		}
	}

	public void Spawn()
	{
		if ((bool)m_animator)
		{
			m_animator.enabled = true;
			m_animator.Play("Spawn");
		}
	}

	public override void Reset(bool setActive = false)
	{
		if ((bool)m_animator)
		{
			m_animator.enabled = true;
		}
		Transform transform = base.transform.Find("lods");
		if ((bool)transform)
		{
			transform.gameObject.SetActive(value: true);
		}
		Transform transform2 = base.transform.Find("fx/explosion");
		if ((bool)transform2)
		{
			transform2.gameObject.SetActive(value: false);
		}
		base.gameObject.SetActive(setActive);
	}
}
