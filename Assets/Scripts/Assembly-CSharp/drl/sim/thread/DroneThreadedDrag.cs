using System.Collections;
using UnityEngine;

namespace drl.sim.thread
{
	public class DroneThreadedDrag : MonoBehaviour
	{
		public enum AerodynamicsModelType
		{
			Traditional = 0,
			GATech = 1
		}

		[SerializeField]
		private DronePhysicsData _dronePhysicsData;

		[SerializeField]
		private AerodynamicsModelType aerodynamicsModelType;

		[SerializeField]
		private AeroModel _aeroModel;

		[SerializeField]
		private DroneBody _droneBody;

		[SerializeField]
		private Drone _drone;

		private Vector3 _currentTotalDragForce;

		private bool runPhysics;

		public Vector3 DragForce => _currentTotalDragForce;

		private void Start()
		{
			StartCoroutine(FindParts());
		}

		private IEnumerator FindParts()
		{
			yield return null;
			while (!_droneBody)
			{
				_droneBody = base.transform.parent.parent.GetComponent<DroneBody>();
				yield return null;
			}
			while (!_drone)
			{
				_drone = base.transform.parent.parent.GetComponent<Drone>();
				yield return null;
			}
			_dronePhysicsData = _drone.physics;
			SetAerodynamics(aerodynamicsModelType);
			runPhysics = true;
		}

		private void SetAerodynamics(AerodynamicsModelType aerodynamics)
		{
			switch (aerodynamics)
			{
			case AerodynamicsModelType.Traditional:
				_aeroModel = new AeroModelTraditional();
				break;
			case AerodynamicsModelType.GATech:
				_aeroModel = new AeroModelGATech(_droneBody.frame.gatechDragData);
				break;
			}
		}

		private IEnumerator WaitForChangesOnPhysicsData()
		{
			while (true)
			{
				yield return new WaitForSeconds(1f);
				if (_dronePhysicsData.aerodynamicsType == DronePhysicsData.AerodynamicsModelType.GATech)
				{
					if (!(_aeroModel is AeroModelGATech))
					{
						runPhysics = false;
						aerodynamicsModelType = AerodynamicsModelType.GATech;
						SetAerodynamics(aerodynamicsModelType);
						runPhysics = true;
					}
				}
				else if (_dronePhysicsData.aerodynamicsType == DronePhysicsData.AerodynamicsModelType.Traditional && !(_aeroModel is AeroModelTraditional))
				{
					runPhysics = false;
					aerodynamicsModelType = AerodynamicsModelType.Traditional;
					SetAerodynamics(aerodynamicsModelType);
					runPhysics = true;
				}
			}
		}

		public void Step(float deltaTime, float rigidbodyMass, Vector3 rigidbodyTransformUp, Vector3 rigidbodyVelocity, Vector3 rigidbodyAngularVelocity, Quaternion rigidbodyRotation)
		{
			if (runPhysics)
			{
				_aeroModel.Step(_drone, deltaTime, rigidbodyMass, rigidbodyTransformUp, rigidbodyVelocity, rigidbodyAngularVelocity, rigidbodyRotation);
				_currentTotalDragForce = _aeroModel.totalForce;
			}
		}

		public void RecalculateForces(float deltaTime, float rigidbodyMass, Vector3 rigidbodyTransformUp, Vector3 rigidbodyVelocity, Vector3 rigidbodyAngularVelocity, Quaternion rigidbodyRotation)
		{
			if (runPhysics)
			{
				_aeroModel.RecalculateForces(_drone, deltaTime, rigidbodyMass, rigidbodyTransformUp, rigidbodyVelocity, rigidbodyAngularVelocity, rigidbodyRotation);
				_currentTotalDragForce = _aeroModel.totalForce;
			}
		}
	}
}
