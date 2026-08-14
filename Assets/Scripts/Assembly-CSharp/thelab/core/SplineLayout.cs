using UnityEngine;

namespace thelab.core
{
	[ExecuteInEditMode]
	public class SplineLayout : MonoBehaviour
	{
		public SplineComponent spline;

		public float paddingStart;

		public float paddingEnd;

		internal int m_last_rev = -1;

		public void Refresh()
		{
			if (!spline)
			{
				return;
			}
			float length = spline.positions.length;
			int childCount = base.transform.childCount;
			float a = Mathf.Min(paddingStart, length);
			float b = Mathf.Max(length - paddingEnd, 0f);
			float num = 0f;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				SplineLayoutElement component = child.GetComponent<SplineLayoutElement>();
				if ((bool)component && component.ignoreLayout)
				{
					continue;
				}
				Vector3 vector = (component ? component.position : Vector3.zero);
				Vector3 vector2 = (component ? component.rotation : Vector3.zero);
				float num2 = (component ? component.length : 0f);
				num = ((childCount <= 1) ? 0f : ((float)i / (float)(childCount - 1)));
				float num3 = Mathf.Lerp(a, b, num) + num2;
				Vector3 vector3 = spline.positions.Get(num3);
				int p_index = -1;
				if ((bool)component && component.snap)
				{
					vector3 = spline.positions.GetClosestNode(num3, out p_index);
				}
				child.position = vector3 + vector;
				if (!component || !component.ignoreRotation)
				{
					bool flag = true;
					if ((bool)component && component.snap)
					{
						flag = false;
					}
					Vector3 forward = Vector3.forward;
					if (flag)
					{
						Vector3 vector4 = spline.positions.Get(num3 - 0.05f);
						forward = (spline.positions.Get(num3 + 0.05f) - vector4).normalized;
					}
					else
					{
						p_index = Mathf.Clamp(p_index, 0, child.childCount - 1);
						forward = child.GetChild(p_index).forward;
					}
					if ((bool)component && component.groundAlign)
					{
						forward.y = 0f;
						forward.Normalize();
					}
					bool flag2 = !component || component.useGlobalUp;
					child.localRotation = Quaternion.LookRotation(forward, flag2 ? Vector3.up : child.up);
					child.localEulerAngles += vector2;
				}
			}
		}
	}
}
