using UnityEngine;

namespace drl.sim
{
	public class DroneSimulationModule : MonoBehaviour
	{
		public DroneSimulation simulation;

		public void Initialize()
		{
			Debug.Log(GetType().Name + "> Initialize");
			OnInitialize();
		}

		public virtual void OnInitialize()
		{
		}

		public virtual void OnUpdate()
		{
		}

		public virtual void OnFixedUpdate()
		{
		}

		public virtual void OnPauseChange(DroneSimulationPauseMode p_from, DroneSimulationPauseMode p_to)
		{
		}

		public virtual void OnStart()
		{
		}

		public virtual void OnStop()
		{
		}

		public virtual void OnTimeout()
		{
		}

		public virtual void OnCountStart()
		{
		}

		public virtual void OnCountStep()
		{
		}

		public virtual void OnCountComplete()
		{
		}
	}
}
