using UnityEngine;

public static class TransformExtensions
{
	public static bool IsChild(this Transform child, Transform parent)
	{
		if (child == null || child.parent == null || parent == null)
		{
			return false;
		}
		if (child.parent == parent)
		{
			return true;
		}
		return child.parent.IsChild(parent);
	}
}
