using System;
using System.Collections;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class GhostDrone : Drone
	{
		public override IEnumerator InitializeAsync()
		{
			yield return null;
			if (!base.body)
			{
				Debug.LogWarning("Drone> Missing 'body' at [" + Hierarchy.Path(base.transform) + "]");
			}
			else
			{
				if (!base.body.frame)
				{
					throw new NullReferenceException("no drone frame");
				}
				base.body.Build();
				yield return null;
				base.renderer.Build();
				yield return null;
				base.body.LinkSkins();
				yield return null;
				base.rigidbody.Build();
				yield return null;
				if ((bool)base.simulation)
				{
					base.simulation.Initialize();
				}
				yield return null;
			}
			if (!base.fc)
			{
				Debug.LogWarning("Drone> Missing 'flight-controller' at [" + Hierarchy.Path(base.transform) + "]");
			}
			else
			{
				base.fc.Boot();
				base.fc.SetLayout(FrameLayoutType.QuadX);
				yield return null;
			}
			if (!base.renderer)
			{
				Debug.LogWarning("Drone> Missing 'renderer' at [" + Hierarchy.Path(base.transform) + "]");
			}
			else
			{
				base.renderer.ClearTrails();
			}
			Validate();
			base.rigidbody.ResetBacktrace();
			base.rigidbody.rb.maxAngularVelocity = 125.663704f;
			m_ready = true;
			Activity.RunOnce(delegate
			{
				Dispatch(DroneEventType.Ready);
			}, 1f / 60f);
		}

		protected override void Update()
		{
			if (base.ready)
			{
				_ = base.fc.armed;
			}
		}

		protected override void FixedUpdate()
		{
			if (base.ready && !isRemote && base.fc.armed && base.rigidbody.backtraceTriggers)
			{
				base.rigidbody.BackTraceTriggers();
			}
		}
	}
}
