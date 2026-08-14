using System;
using UnityEngine;
using thelab.core;

[ExecuteInEditMode]
public class PolarLayout : MonoBehaviour
{
	public enum Type
	{
		lemniscate = 0,
		rose_curve = 1,
		archi_spiral = 2
	}

	public SplineComponent spline;

	public Type type;

	[Space]
	[Header("Settings: ")]
	public int nodes = 1;

	public Vector3 offset = Vector3.zero;

	public float amplitude = 1f;

	public int frequency = 1;

	[Header("Archi spiral: ")]
	public float angle = 360f;

	private float lastStep;

	private float lastAmplitude;

	private Vector3 lastOffset;

	private float lastFrequency;

	private float lastAngle;

	private Type lastType;

	private void Start()
	{
		lastStep = nodes;
		lastAmplitude = amplitude;
		lastOffset = offset;
		lastFrequency = frequency;
		lastType = type;
		lastAngle = angle;
	}

	private void Update()
	{
		if (nodes <= 0)
		{
			nodes = 1;
		}
		if (amplitude == 0f)
		{
			amplitude = 0.2f;
		}
		if (lastStep != (float)nodes || lastAmplitude != amplitude || lastOffset != offset || lastFrequency != (float)frequency || lastType != type || lastAngle != angle)
		{
			RefreshLayout();
		}
	}

	private void RefreshLayout()
	{
		if (!spline)
		{
			return;
		}
		float num = 0f;
		lastStep = nodes;
		lastAmplitude = amplitude;
		lastOffset = offset;
		lastFrequency = frequency;
		lastType = type;
		lastAngle = angle;
		Transform transform = spline.transform;
		float num2 = (float)Math.PI / 180f * angle / (float)nodes;
		while (transform.childCount > 0)
		{
			UnityEngine.Object.DestroyImmediate(transform.GetChild(0).gameObject);
		}
		for (int i = 0; i < nodes; i++)
		{
			num = num2 * (float)i;
			switch (type)
			{
			case Type.lemniscate:
			{
				float x3 = amplitude * Mathf.Sqrt(2f) * Mathf.Cos(num) / (Mathf.Pow(Mathf.Sin(num), 2f) + 1f);
				float z3 = amplitude * Mathf.Sqrt(2f) * Mathf.Cos(num) * Mathf.Sin(num) / (Mathf.Pow(Mathf.Sin(num), 2f) + 1f);
				Vector3 vector = new Vector3(x3, 0f, z3) + offset;
				GameObject obj3 = new GameObject((i + 1).ToString());
				obj3.transform.parent = transform;
				obj3.transform.rotation = Quaternion.identity;
				obj3.transform.localPosition = vector;
				break;
			}
			case Type.rose_curve:
			{
				float x2 = amplitude * Mathf.Cos((float)frequency * num) * Mathf.Cos(num);
				float z2 = amplitude * Mathf.Cos((float)frequency * num) * Mathf.Sin(num);
				Vector3 vector = new Vector3(x2, 0f, z2) + offset;
				GameObject obj2 = new GameObject((i + 1).ToString());
				obj2.transform.parent = transform;
				obj2.transform.rotation = Quaternion.identity;
				obj2.transform.localPosition = vector;
				break;
			}
			case Type.archi_spiral:
			{
				float x = amplitude / 10f * num * Mathf.Cos(num);
				float z = amplitude / 10f * num * Mathf.Sin(num);
				Vector3 vector = new Vector3(x, 0f, z) + offset;
				GameObject obj = new GameObject((i + 1).ToString());
				obj.transform.parent = transform;
				obj.transform.rotation = Quaternion.identity;
				obj.transform.localPosition = vector + offset;
				break;
			}
			}
		}
		if (type != Type.archi_spiral && angle == 360f)
		{
			GameObject obj4 = new GameObject((nodes + 1).ToString());
			obj4.transform.parent = transform;
			obj4.transform.localPosition = transform.GetChild(0).localPosition;
			obj4.transform.localRotation = transform.GetChild(0).localRotation;
		}
	}
}
