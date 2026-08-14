using UnityEngine;

namespace drl.sim
{
	public class DroneInputTransmitter : MonoBehaviour
	{
		public Drone drone;

		public int order;

		public int channel;

		protected virtual void Start()
		{
		}

		public void Step(float p_dt)
		{
			if ((bool)base.gameObject && (bool)drone && drone.hasReceiver && !(drone.receiver == null) && drone.receiver.channel == channel && drone.receiver.enabled)
			{
				drone.receiver.signal = default(SignalVector);
				if (base.enabled)
				{
					OnUpdate(p_dt);
				}
			}
		}

		public virtual ControllerStateType GetControllerType()
		{
			return ControllerStateType.Taranis;
		}

		protected virtual void OnUpdate(float p_dt)
		{
		}

		public virtual string GetPrefix()
		{
			return "it";
		}
	}
}
