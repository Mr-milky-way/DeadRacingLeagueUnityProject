using UnityEngine;
using drl.sim;

namespace drl.network
{
	public interface INetworkObservable
	{
		INetworkObject NetworkObject { get; set; }

		bool IsReady { get; }

		GameObject gameObject { get; }

		Transform NetworkTransform { get; }

		Rigidbody NetworkRigidbody { get; }

		DroneBatteryPowerData BatteryPowerData { get; }

		long PackedInputAndRPM { get; set; }

		float[] NetworkRPMs { get; set; }

		bool CanSync { get; }

		void OnTeleport(float squaredDeltaDistance);
	}
}
