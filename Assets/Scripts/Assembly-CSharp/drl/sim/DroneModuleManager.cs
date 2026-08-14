namespace drl.sim
{
	public class DroneModuleManager : DroneSimulationManager<DroneSimulationModule>
	{
		public override string GetContainerName()
		{
			return "modules";
		}

		public void OnInitialize()
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				base.list[i].simulation = base.simulation;
				base.list[i].Initialize();
			}
		}

		public void OnUpdate()
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if (base.list[i].enabled)
				{
					base.list[i].OnUpdate();
				}
			}
		}

		public void OnFixedUpdate()
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if (base.list[i].enabled)
				{
					base.list[i].OnFixedUpdate();
				}
			}
		}

		public void OnCountStart()
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if (base.list[i].enabled)
				{
					base.list[i].OnCountStart();
				}
			}
		}

		public void OnCountStep()
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if (base.list[i].enabled)
				{
					base.list[i].OnCountStep();
				}
			}
		}

		public void OnCountComplete()
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if (base.list[i].enabled)
				{
					base.list[i].OnCountComplete();
				}
			}
		}

		public void OnPauseChange(DroneSimulationPauseMode p_from, DroneSimulationPauseMode p_to)
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if (base.list[i].enabled)
				{
					base.list[i].OnPauseChange(p_from, p_to);
				}
			}
		}

		public void OnStop()
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if (base.list[i].enabled)
				{
					base.list[i].OnStop();
				}
			}
		}

		public void OnStart()
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if (base.list[i].enabled)
				{
					base.list[i].OnStart();
				}
			}
		}
	}
}
