using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl.sim
{
	[Serializable]
	public class CrashData
	{
		public DroneEventType type;

		public float crashEnergy;

		public Vector3 contactNormal;

		public Vector3 impactVelocity;

		public Vector3 contactPoint;

		public bool isBroken;

		public List<DroneCrashNode> nodes;

		public float[] propsDamage;

		public float bodyDamage;

		public CrashData(DroneEventType p_type, float p_crashEnergy, Vector3 p_contactNormal, Vector3 p_impactVelocity, Vector3 p_contactPoint, bool p_isBroken, List<DroneCrashNode> p_nodes, float p_bodyDamage = 0f, float[] p_propsDamage = null)
		{
			type = p_type;
			crashEnergy = p_crashEnergy;
			contactNormal = p_contactNormal;
			impactVelocity = p_impactVelocity;
			contactPoint = p_contactPoint;
			nodes = p_nodes;
			isBroken = p_isBroken;
			propsDamage = p_propsDamage;
			bodyDamage = p_bodyDamage;
		}
	}
}
