using UnityEngine;

public class TrainingElement : MonoBehaviour, IActivable
{
	public bool IsRunning { get; protected set; }

	public virtual void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}

	public virtual void Run()
	{
		IsRunning = true;
	}

	public virtual void Reset(bool setActive = false)
	{
	}
}
