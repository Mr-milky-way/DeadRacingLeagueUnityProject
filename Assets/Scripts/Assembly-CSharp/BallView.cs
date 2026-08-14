using UnityEngine;
using thelab.mvc;

public class BallView : View<BounceApplication>
{
	public void OnCollisionEnter(Collision p_collision)
	{
		Notify("ball.hit", "ground");
	}
}
