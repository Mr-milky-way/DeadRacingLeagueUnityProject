using System;
using UnityEngine;

[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
public class AkVertex
{
	private Vector3 Vector = Vector3.zero;

	public float X
	{
		get
		{
			return Vector.x;
		}
		set
		{
			Vector.x = value;
		}
	}

	public float Y
	{
		get
		{
			return Vector.y;
		}
		set
		{
			Vector.y = value;
		}
	}

	public float Z
	{
		get
		{
			return Vector.z;
		}
		set
		{
			Vector.z = value;
		}
	}

	public void Zero()
	{
		Vector.Set(0f, 0f, 0f);
	}

	public static implicit operator Vector3(AkVertex vector)
	{
		return vector.Vector;
	}

	public AkVertex()
	{
	}

	public AkVertex(float x, float y, float z)
	{
		Vector.Set(x, y, z);
	}

	public void Clear()
	{
		Vector.Set(0f, 0f, 0f);
	}

	public static int GetSizeOf()
	{
		return 12;
	}

	public void Clone(AkVertex other)
	{
		Vector = other.Vector;
	}
}
