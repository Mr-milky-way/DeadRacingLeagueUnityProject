using System;
using UnityEngine;
using thelab.core;

public class TweenExample01 : MonoBehaviour
{
	public Transform target;

	public Transform position;

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			DoTween();
		}
		float num = 2.5f;
		float num2 = Mathf.Sin(Time.time * 360f * ((float)Math.PI / 180f) * 0.5f) * num * 0.25f;
		float num3 = Mathf.Cos(Time.time * 360f * ((float)Math.PI / 180f) * 0.3f) * num * 1f;
		float num4 = Mathf.Sin(Time.time * 360f * ((float)Math.PI / 180f) * 0.2f) * num * 2f;
		Vector3 vector = new Vector3(num2 + num3, num3 + num4, num2 + num4);
		position.position = vector;
	}

	public void DoTween()
	{
		Tween.Add(target, "position", position.position, 0.5f, Elastic.OutBig);
	}
}
