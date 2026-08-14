using System;
using System.Runtime.InteropServices;

public abstract class AkBaseArray<T> : IDisposable
{
	private IntPtr m_Buffer;

	public int Capacity { get; private set; }

	protected abstract int StructureSize { get; }

	public T this[int index]
	{
		get
		{
			return CreateNewReferenceFromIntPtr(GetObjectPtr(index));
		}
		set
		{
			CloneIntoReferenceFromIntPtr(GetObjectPtr(index), value);
		}
	}

	public AkBaseArray(int capacity)
	{
		m_Buffer = Marshal.AllocHGlobal(capacity * StructureSize);
		if (m_Buffer != IntPtr.Zero)
		{
			Capacity = capacity;
			for (int i = 0; i < capacity; i++)
			{
				DefaultConstructAtIntPtr(GetObjectPtr(i));
			}
		}
	}

	public void Dispose()
	{
		if (m_Buffer != IntPtr.Zero)
		{
			for (int i = 0; i < Capacity; i++)
			{
				ReleaseAllocatedMemoryFromReferenceAtIntPtr(GetObjectPtr(i));
			}
			Marshal.FreeHGlobal(m_Buffer);
			m_Buffer = IntPtr.Zero;
			Capacity = 0;
		}
	}

	~AkBaseArray()
	{
		Dispose();
	}

	public virtual int Count()
	{
		return Capacity;
	}

	protected virtual void DefaultConstructAtIntPtr(IntPtr address)
	{
	}

	protected virtual void ReleaseAllocatedMemoryFromReferenceAtIntPtr(IntPtr address)
	{
	}

	protected abstract T CreateNewReferenceFromIntPtr(IntPtr address);

	protected abstract void CloneIntoReferenceFromIntPtr(IntPtr address, T other);

	public IntPtr GetBuffer()
	{
		return m_Buffer;
	}

	protected IntPtr GetObjectPtr(int index)
	{
		if (index >= Capacity)
		{
			throw new IndexOutOfRangeException("Out of range access in " + GetType().Name);
		}
		return (IntPtr)(m_Buffer.ToInt64() + StructureSize * index);
	}
}
