using System.Collections;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace drl.sim.thread
{
	public class ThreadManager : MonoBehaviour
	{
		[SerializeField]
		private bool _physicsThreadPaused;

		[SerializeField]
		private bool _stepThread;

		[SerializeField]
		private int _framesPassedUpdate;

		[SerializeField]
		private int _framesPassedFixedUpdate;

		[SerializeField]
		[Tooltip("In milliseconds!")]
		private int _stepTime = 1000;

		[Space(20f)]
		public DroneThreaded DF;

		private Thread threadDroneForces;

		private Thread threadStep;

		private Stopwatch stopwatchStep;

		private bool threadStepRunning;

		public bool PhysicsThreadPaused => _physicsThreadPaused;

		public bool StepThread => _stepThread;

		public int StepTime
		{
			get
			{
				return _stepTime;
			}
			set
			{
				_stepTime = value;
			}
		}

		private void Start()
		{
		}

		public void TogglePhysics()
		{
			SetPhysics(!_physicsThreadPaused);
		}

		public void SetPhysics(bool paused)
		{
			_physicsThreadPaused = paused;
			_stepThread = false;
		}

		public void ToggleStep()
		{
			_stepThread = (_physicsThreadPaused ? true : false);
			_physicsThreadPaused = true;
			_framesPassedUpdate = 0;
			_framesPassedFixedUpdate = 0;
		}

		private void FixedUpdate()
		{
			if (_stepThread)
			{
				_framesPassedFixedUpdate++;
			}
		}

		private IEnumerator Initalize()
		{
			while (!DF)
			{
				yield return null;
				DF = GetComponent<DroneThreaded>();
			}
			threadStep = new Thread(StepLoop);
			threadStep.Name = "TManager.StepLoop";
			threadStepRunning = true;
			threadStep.Start();
			StartThreads();
		}

		private void StartThreads()
		{
			threadDroneForces = new Thread(DF.DFThread);
			threadDroneForces.Name = "TManager.DF";
			DF.AllowThreadRun = true;
			threadDroneForces.Start();
		}

		private void StepLoop()
		{
			stopwatchStep = new Stopwatch();
			stopwatchStep.Start();
			long num = 0L;
			long num2 = 0L;
			while (threadStepRunning)
			{
				num = stopwatchStep.ElapsedMilliseconds;
				while (_stepThread)
				{
					_physicsThreadPaused = false;
					if (num2 >= _stepTime)
					{
						_physicsThreadPaused = true;
						_stepThread = false;
						num2 = 0L;
					}
					else
					{
						num2 = stopwatchStep.ElapsedMilliseconds - num;
					}
				}
			}
			stopwatchStep.Stop();
		}

		private void OnDestroy()
		{
			if (DF != null)
			{
				DF.AllowThreadRun = false;
			}
			threadStepRunning = false;
			if (threadDroneForces != null && threadDroneForces.IsAlive)
			{
				threadDroneForces.Abort();
			}
			if (threadStep != null && threadStep.IsAlive)
			{
				threadStep.Abort();
			}
		}
	}
}
