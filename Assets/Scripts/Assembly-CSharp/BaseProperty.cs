using System;
using UnityEngine;

public class BaseProperty<T> : ScriptableObject, ISerializationCallbackReceiver
{
	[SerializeField]
	protected T m_DefaultValue;

	[SerializeField]
	protected T m_Value;

	public Action<T> OnChanged;

	public T Value
	{
		get
		{
			return m_Value;
		}
		set
		{
			m_Value = value;
			OnChanged?.Invoke(m_Value);
		}
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		SetDefault();
	}

	protected virtual void SetDefault()
	{
		m_Value = m_DefaultValue;
	}
}
