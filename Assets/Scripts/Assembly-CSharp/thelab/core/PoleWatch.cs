using UnityEngine;
using drl.sim.rci;

namespace thelab.core
{
	public class PoleWatch : TrainingElement
	{
		public enum poleTags
		{
			Pole = 0,
			FirstPole = 1,
			LastPole = 2
		}

		public Transform target;

		public float goalAngle;

		public float distToEngage = 10f;

		public bool showGauge;

		public Gauge gauge;

		public poleTags poleTag;

		private LineRenderer mLineRenderer;

		public override void Run()
		{
			Flow component = GetComponent<Flow>();
			if ((bool)component)
			{
				component.Restart();
				base.IsRunning = true;
			}
		}

		private void Awake()
		{
			mLineRenderer = target.GetComponentInChildren<LineRenderer>();
			Flow component = GetComponent<Flow>();
			if ((bool)component)
			{
				component.Stop();
				component.Reset();
				component.run = false;
			}
		}

		[ContextMenu("SetupLineRenderer")]
		private void SetupLineRenderer()
		{
			mLineRenderer = target.GetComponentInChildren<LineRenderer>();
			if ((bool)mLineRenderer)
			{
				mLineRenderer.positionCount = 20;
				Vector3 vector = Quaternion.AngleAxis(70f * Mathf.Sign(goalAngle), target.up) * target.forward;
				Vector3[] array = new Vector3[20];
				float num = 0f;
				float num2 = Mathf.Abs(goalAngle) / 20f;
				Mathf.Sign(goalAngle);
				for (int i = 0; i < 20; i++)
				{
					vector = Quaternion.AngleAxis(num2 * Mathf.Sign(goalAngle), target.up) * vector;
					array[i] = target.position + vector + Vector3.up * num;
					num += 0.01f;
				}
				mLineRenderer.SetPositions(array);
			}
		}
	}
}
