using System;
using UnityEngine;
using thelab.core;

[ExecuteInEditMode]
public class CircleLayout : MonoBehaviour
{
	public SplineComponent spline;

	[Space]
	[Header("Settings: ")]
	public float radius;

	public int numberOfNodes;

	public float heightOffset;

	public float totalAngle = 360f;

	public float spiralHeight;

	[Space]
	[Header("Setup:")]
	[SerializeField]
	private GameObject nodeGO;

	private float lastRadius;

	private float lastNoNodes;

	private float lastHeightOffset;

	private float lastTotalAngle;

	private float lastSpiralHeight;

	private void Start()
	{
		lastRadius = radius;
		lastNoNodes = numberOfNodes;
		lastHeightOffset = heightOffset;
		lastTotalAngle = totalAngle;
		lastSpiralHeight = spiralHeight;
	}

	private void Update()
	{
		if (lastNoNodes != (float)numberOfNodes || lastRadius != radius || lastHeightOffset != heightOffset || lastTotalAngle != totalAngle || lastSpiralHeight != spiralHeight)
		{
			RefreshLayout();
		}
	}

	private void RefreshLayout()
	{
		if (radius != 0f && numberOfNodes != 0 && !(spline == null))
		{
			float num = (float)Math.PI / 180f * totalAngle / (float)numberOfNodes;
			lastHeightOffset = heightOffset;
			lastRadius = radius;
			lastNoNodes = numberOfNodes;
			lastTotalAngle = totalAngle;
			lastSpiralHeight = spiralHeight;
			Transform transform = spline.transform;
			while (transform.childCount > 0)
			{
				UnityEngine.Object.DestroyImmediate(transform.GetChild(0).gameObject);
			}
			float num2 = heightOffset;
			for (int i = 0; i < numberOfNodes; i++)
			{
				GameObject obj = (nodeGO ? UnityEngine.Object.Instantiate(nodeGO) : new GameObject());
				obj.name = (i + 1).ToString();
				obj.transform.parent = transform;
				obj.transform.rotation = Quaternion.identity;
				float f = num * (float)i;
				float x = radius * Mathf.Cos(f) + transform.position.x;
				float z = radius * Mathf.Sin(f) + transform.position.z;
				obj.transform.position = new Vector3(x, num2, z);
				num2 += spiralHeight;
			}
			if (spiralHeight == 0f && Mathf.Abs(totalAngle) == 360f)
			{
				GameObject obj2 = new GameObject((numberOfNodes + 1).ToString());
				obj2.transform.parent = transform;
				obj2.transform.position = transform.GetChild(0).position;
				obj2.transform.rotation = transform.GetChild(0).rotation;
			}
		}
	}
}
