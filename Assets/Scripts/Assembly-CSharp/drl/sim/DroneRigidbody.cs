using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.sim
{
	[RequireComponent(typeof(Drone))]
	[RequireComponent(typeof(Rigidbody))]
	public class DroneRigidbody : MonoBehaviour
	{
		[SerializeField]
		private Rigidbody m_rb;

		private bool m_hasRb;

		[SerializeField]
		private Drone m_drone;

		private bool m_hasDrone;

		public float[] currentThrust = new float[4];

		public float[] currentMotorThrust = new float[4];

		private bool m_frozen;

		private Vector3 m_frozenVelocity;

		private Vector3 m_frozenAngularVelocity;

		public float lastEnergy;

		public float lastEnergyTime;

		public List<Collider> colliders;

		private static PhysicMaterial m_drone_physics_mat;

		public bool backtraceTriggers = true;

		public bool backtraceCollisions;

		protected Collider m_gateCollider;

		protected float m_event_timer_collision;

		protected float m_raycastDistanceThreshold = 0.02f;

		protected float m_raycastCollisionBounceStrength = 0.2f;

		protected Vector3 m_lastPosition;

		protected Vector3 m_lastFixedPosition;

		protected List<Collider> m_holeTriggersEntered = new List<Collider>();

		public List<Collider> levelHoleTriggers = new List<Collider>();

		protected RaycastHit[] hits;

		private bool m_accumulatingEnergy;

		private float m_accumulatedDamage;

		private List<float> m_crashEnergies = new List<float>();

		private float m_crashCounter;

		private bool m_debugCrashData = true;

		private bool m_filteringScrapes;

		public Rigidbody rb
		{
			get
			{
				if (m_hasRb)
				{
					return m_rb;
				}
				if ((bool)m_rb)
				{
					m_hasRb = true;
					return m_rb;
				}
				m_rb = GetComponent<Rigidbody>();
				if ((bool)m_rb)
				{
					m_hasRb = true;
					return m_rb;
				}
				return null;
			}
			set
			{
				m_rb = value;
				m_hasRb = m_rb != null;
			}
		}

		public bool hasRb => m_hasRb;

		public Drone drone
		{
			get
			{
				if (m_hasDrone)
				{
					return m_drone;
				}
				if ((bool)m_drone)
				{
					m_hasDrone = true;
					return m_drone;
				}
				m_drone = GetComponent<Drone>();
				if ((bool)m_drone)
				{
					m_hasDrone = true;
					return m_drone;
				}
				return null;
			}
			set
			{
				m_drone = value;
				m_hasDrone = m_drone != null;
			}
		}

		public bool hasDrone => m_hasDrone;

		public float mass
		{
			get
			{
				return rb.mass;
			}
			set
			{
				rb.mass = value;
			}
		}

		public float currentTorque { get; set; }

		public Vector3 currentDragFactors { get; set; }

		public Vector3 currentDragForce { get; set; }

		public Vector3 currentLiftForce { get; set; }

		public bool isKinematic
		{
			get
			{
				return rb.isKinematic;
			}
			set
			{
				rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
				rb.isKinematic = value;
				rb.collisionDetectionMode = (value ? CollisionDetectionMode.ContinuousSpeculative : CollisionDetectionMode.ContinuousDynamic);
			}
		}

		public bool frozen
		{
			get
			{
				return m_frozen;
			}
			set
			{
				if (m_frozen != value)
				{
					if (value)
					{
						m_frozenVelocity = rb.velocity;
						m_frozenAngularVelocity = rb.angularVelocity;
					}
					else
					{
						rb.velocity = m_frozenVelocity;
						rb.angularVelocity = m_frozenAngularVelocity;
					}
				}
				isKinematic = value;
				m_frozen = value;
			}
		}

		internal static PhysicMaterial drone_physics_mat
		{
			get
			{
				if ((bool)m_drone_physics_mat)
				{
					return m_drone_physics_mat;
				}
				m_drone_physics_mat = new PhysicMaterial("drone-mat");
				m_drone_physics_mat.bounciness = 0f;
				m_drone_physics_mat.bounceCombine = PhysicMaterialCombine.Minimum;
				return m_drone_physics_mat;
			}
		}

		public void CheckMotorCount(int p_count)
		{
			if (currentThrust == null || currentThrust.Length != p_count)
			{
				currentThrust = new float[p_count];
			}
			if (currentMotorThrust == null || currentMotorThrust.Length != p_count)
			{
				currentMotorThrust = new float[p_count];
			}
		}

		public void VerifyCOG()
		{
			Vector3 centerOfMass = drone.body.centerOfMass;
			centerOfMass.x = 0f;
			centerOfMass.z = ((Mathf.Abs(centerOfMass.z) < 0.005f) ? 0f : ((Mathf.Abs(centerOfMass.z) < 0.01f) ? (centerOfMass.z / 10f) : (centerOfMass.z / 5f)));
			centerOfMass.z = 0f;
			drone.StabilizeDroneOnGround(p_flag: true);
			rb.maxAngularVelocity = 125.663704f;
			Activity.RunOnce(delegate
			{
				if (this != null && base.gameObject != null && drone != null)
				{
					drone.StabilizeDroneOnGround(p_flag: true);
				}
			}, 1f);
		}

		public void Build()
		{
			drone = GetComponent<Drone>();
			rb.interpolation = ((drone.isGhost || drone.isRemote) ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None);
			rb.useGravity = true;
			rb.drag = 0f;
			mass = drone.body.weight * 0.001f;
			colliders = new List<Collider>();
			Hierarchy.Traverse(base.transform, delegate(Collider cit)
			{
				Collider[] components = cit.transform.GetComponents<Collider>();
				foreach (Collider collider in components)
				{
					if (!collider.isTrigger && !collider.sharedMaterial)
					{
						collider.sharedMaterial = drone_physics_mat;
					}
				}
				colliders.AddRange(components);
			});
			SphereCollider[] componentsInChildren = GetComponentsInChildren<SphereCollider>();
			foreach (SphereCollider sphereCollider in componentsInChildren)
			{
				if (sphereCollider.name == "gate")
				{
					m_gateCollider = sphereCollider;
					break;
				}
			}
			VerifyCOG();
		}

		public void OnFixedUpdate()
		{
			if (Time.time > lastEnergyTime + 5f)
			{
				lastEnergy = 0f;
			}
		}

		public void ClearForces()
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			m_frozenVelocity = Vector3.zero;
			m_frozenAngularVelocity = Vector3.zero;
			drone.ClearLastState();
			ResetBacktrace();
		}

		public void SetCollisionEnabled(bool p_flag)
		{
			SetCollisionEnabled(p_flag, p_apply_triggers: false);
		}

		public void SetCollisionEnabled(bool p_flag, bool p_apply_triggers)
		{
			if (!drone || !drone.body || !drone.body.frame)
			{
				return;
			}
			Transform transform = drone.body.frame.transform;
			transform = transform.Find("colliders");
			if ((bool)transform)
			{
				transform = transform.Find("rig");
				if ((bool)transform)
				{
					transform.gameObject.SetActive(p_flag);
				}
			}
		}

		public void ResetBacktrace()
		{
			m_lastPosition = drone.position;
			m_lastFixedPosition = drone.position;
		}

		public virtual void BackTraceTriggers()
		{
			if (!hasDrone || !base.enabled || drone.isRemote || !drone.hasFc || !drone.fc.armed)
			{
				return;
			}
			if ((m_lastPosition - drone.position).sqrMagnitude > 0.0004f)
			{
				Vector3 vector = drone.position - m_lastPosition;
				float magnitude = vector.magnitude;
				if (hits == null || hits.Length < 20)
				{
					hits = new RaycastHit[20];
				}
				int num = Physics.RaycastNonAlloc(m_lastPosition, vector.normalized, hits, magnitude, DRLPhysics.Layers.Raycast_BacktraceTriggers, QueryTriggerInteraction.Collide);
				for (int i = 0; i < hits.Length && i < num; i++)
				{
					RaycastHit raycastHit = hits[i];
					if (raycastHit.transform.IsChildOf(base.transform) || !raycastHit.collider.isTrigger)
					{
						continue;
					}
					ColliderEventComponent component = raycastHit.transform.GetComponent<ColliderEventComponent>();
					if (component != null)
					{
						component.Trigger(m_gateCollider);
						continue;
					}
					TriggerView component2 = raycastHit.transform.GetComponent<TriggerView>();
					if (component2 != null)
					{
						component2.OnTriggerEnter(m_gateCollider);
						component2.OnTriggerExit(m_gateCollider);
					}
				}
			}
			m_lastPosition = drone.position;
		}

		private void Start()
		{
		}

		public virtual void BackTraceCollisions()
		{
			if (!hasDrone || !base.enabled || drone.isGhost || drone.isRemote || !drone.hasFc || !drone.fc.armed)
			{
				return;
			}
			for (int num = m_holeTriggersEntered.Count - 1; num >= 0; num--)
			{
				if (!m_holeTriggersEntered[num].bounds.Contains(drone.position))
				{
					m_holeTriggersEntered.RemoveAt(num);
				}
			}
			if ((m_lastFixedPosition - drone.position).sqrMagnitude > 0.0004f)
			{
				Vector3 lhs = drone.position - m_lastFixedPosition;
				float magnitude = lhs.magnitude;
				if (magnitude > m_raycastDistanceThreshold)
				{
					if (hits == null || hits.Length < 20)
					{
						hits = new RaycastHit[20];
					}
					int num2 = Physics.RaycastNonAlloc(m_lastFixedPosition, lhs.normalized, hits, magnitude, DRLPhysics.Layers.Raycast_BacktraceCollisions, QueryTriggerInteraction.Ignore);
					for (int i = 0; i < hits.Length && i < num2; i++)
					{
						RaycastHit raycastHit = hits[i];
						if (raycastHit.transform.IsChildOf(base.transform) || raycastHit.collider.isTrigger || (raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Terrain") && m_holeTriggersEntered.Count != 0) || !(Vector3.Dot(lhs, raycastHit.normal) < 0f))
						{
							continue;
						}
						ProcessCollision(raycastHit.point, raycastHit.normal, drone.fc.sensor.inertial.actualVelocity);
						if (raycastHit.transform.name.StartsWith("water") || raycastHit.transform.name.StartsWith("lake") || raycastHit.transform.name.StartsWith("river"))
						{
							drone.WaterImpact();
						}
						Vector3 velocity = Vector3.Reflect(rb.velocity, raycastHit.normal) * m_raycastCollisionBounceStrength;
						drone.position = raycastHit.point - lhs.normalized * m_raycastDistanceThreshold * 5f;
						rb.position = drone.position;
						rb.velocity = velocity;
						if (drone.hasThreaded && !drone.threaded.inCollision)
						{
							if (!drone.isThreaded)
							{
								drone.threaded.calculateDF = false;
								break;
							}
							drone.threaded.ResetThreadToUnityRigidbody();
							drone.threaded.inCollision = true;
							drone.threaded.wasInCollision = true;
						}
						break;
					}
				}
			}
			m_lastFixedPosition = drone.position;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (hasDrone && base.enabled && !drone.isGhost && !drone.isRemote && drone.hasFc && drone.fc.armed && levelHoleTriggers.Contains(other) && !m_holeTriggersEntered.Contains(other))
			{
				m_holeTriggersEntered.Add(other);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (hasDrone && base.enabled && !drone.isGhost && !drone.isRemote && drone.hasFc && drone.fc.armed && m_holeTriggersEntered.Contains(other))
			{
				m_holeTriggersEntered.Remove(other);
			}
		}

		protected void OnCollisionEnter(Collision p_collision)
		{
			if (!hasDrone || !base.enabled || drone.isGhost || drone.isRemote || !drone.hasFc || !drone.fc.armed || p_collision.collider.transform.IsChildOf(base.transform) || DRLPhysics.Layers.Raycast_BacktraceCollisions != (DRLPhysics.Layers.Raycast_BacktraceCollisions | (1 << p_collision.collider.gameObject.layer)))
			{
				return;
			}
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			float num = p_collision.contacts.Length;
			for (int i = 0; (float)i < num; i++)
			{
				zero += p_collision.contacts[i].normal;
				zero2 += p_collision.contacts[i].point;
			}
			num = ((num <= 0f) ? 0f : (1f / num));
			zero *= num;
			zero2 *= num;
			drone.fc.sensor.collision.active = true;
			ProcessCollision(zero2, zero, drone.fc.sensor.inertial.actualVelocity);
			if (p_collision.collider.transform.name.StartsWith("water") || p_collision.collider.transform.name.StartsWith("lake") || p_collision.collider.transform.name.StartsWith("river"))
			{
				drone.WaterImpact();
			}
			if (drone.hasThreaded && !drone.threaded.inCollision)
			{
				if (!drone.isThreaded)
				{
					drone.threaded.calculateDF = false;
					return;
				}
				drone.threaded.ResetThreadToUnityRigidbody();
				drone.threaded.inCollision = true;
				drone.threaded.wasInCollision = true;
			}
		}

		private void OnCollisionStay(Collision p_collision)
		{
			if (!hasDrone || !base.enabled || drone.isGhost || drone.isRemote || !drone.hasFc || !drone.fc.armed || p_collision.collider.transform.IsChildOf(base.transform))
			{
				return;
			}
			drone.fc.sensor.collision.active = true;
			if (drone.hasThreaded && !drone.threaded.inCollision)
			{
				if (!drone.isThreaded)
				{
					drone.threaded.calculateDF = false;
					return;
				}
				drone.threaded.ResetThreadToUnityRigidbody();
				drone.threaded.inCollision = true;
				drone.threaded.wasInCollision = true;
			}
		}

		protected void ProcessCollision(Vector3 p_contactPoint, Vector3 p_contactNormal, Vector3 p_impactVelocity)
		{
			if (drone.isBroken || !drone.hasBody || !drone.body.hasFrame || drone.body.frame == null || drone.body.frame.crash == null)
			{
				m_accumulatingEnergy = false;
				m_accumulatedDamage = 0f;
				m_crashEnergies.Clear();
				m_crashCounter = 0f;
				return;
			}
			int num = 5;
			Vector3 vector = base.transform.InverseTransformPoint(p_contactPoint);
			if (vector.magnitude > 0.05f)
			{
				num = ((Mathf.Abs(vector.z) < 0.02f) ? 4 : ((!(vector.z > 0f)) ? 1 : 7));
				if (Mathf.Abs(vector.x) < 0.02f)
				{
					num++;
				}
				else if (vector.x > 0f)
				{
					num += 2;
				}
			}
			DroneQuadrantRegion quadrant = (DroneQuadrantRegion)num;
			float dot = 0f - Vector3.Dot(p_impactVelocity.normalized, p_contactNormal.normalized);
			dot = Mathf.Clamp01(Mathf.Abs(dot));
			float num2 = 0.25f * p_impactVelocity.sqrMagnitude * mass * Mathf.Sqrt(dot);
			if (!m_filteringScrapes)
			{
				drone.Scrape(num2);
				m_filteringScrapes = true;
				this.TimerRunOnce(delegate
				{
					m_filteringScrapes = false;
				}, 1f / 60f);
			}
			if (drone.invulnerable > 0f || !drone.crashEnabled)
			{
				m_accumulatingEnergy = false;
				m_accumulatedDamage = 0f;
				m_crashEnergies.Clear();
				m_crashCounter = 0f;
			}
			else
			{
				if (num2 < Drone.DamageEnergy)
				{
					return;
				}
				m_crashCounter += 1f;
				m_crashEnergies.Add(num2);
				if (m_debugCrashData)
				{
					D.Log("DroneRigidbody> ProcessCollision:\nenergy - " + num2 + "\nimpact_velocity - " + p_impactVelocity.magnitude + "\nsensor_speed - " + drone.fc.sensor.inertial.velocity.magnitude + "\nrb_speed - " + rb.velocity.magnitude + "\ncrash_threshold - " + Drone.CrashEnergy + "\nmass - " + mass);
				}
				m_accumulatedDamage += 1f / (float)drone.body.frame.crash.nodes.Count;
				if (m_accumulatingEnergy)
				{
					return;
				}
				m_accumulatingEnergy = true;
				this.TimerRunOnce(delegate
				{
					if (drone.isBroken || drone.invulnerable > 0f || !drone.crashEnabled || !drone.hasBody || !drone.body.hasFrame || drone.body.frame == null || drone.body.frame.crash == null)
					{
						m_accumulatingEnergy = false;
						m_accumulatedDamage = 0f;
						m_crashEnergies.Clear();
						m_crashCounter = 0f;
					}
					else
					{
						m_accumulatedDamage = Mathf.Clamp01(m_accumulatedDamage);
						float num3 = m_crashEnergies.Max();
						float num4 = m_crashCounter * 5f;
						float num5 = num3 + num4;
						if (m_debugCrashData)
						{
							D.Log("Max energy: " + num3 + "  crash count: " + m_crashCounter + " acc stress: " + num4);
						}
						if (!(num5 < Drone.DamageEnergy))
						{
							if (num5 > lastEnergy)
							{
								lastEnergy = num5;
								lastEnergyTime = Time.time;
							}
							if (Drone.CrashEnergy > 0f && num5 > Drone.CrashEnergy)
							{
								if (m_debugCrashData)
								{
									D.Log("<color=#ff0000>Drone> Crash quadrant[" + quadrant.ToString() + "] energy[" + num5.ToString("0.0") + "] dot[" + dot.ToString("0.0") + "] kph spinout[" + Drone.Spinout + "]]  </color>");
								}
								if (Drone.Spinout > 0f)
								{
									drone.ApplySpinout(0.3f);
								}
								drone.Crash(num5, p_contactNormal, p_impactVelocity, p_contactPoint);
							}
							else if (Drone.DamageEnergy > 0f && num5 > Drone.DamageEnergy)
							{
								float num6 = 3f * dot / 4f + 0.25f;
								m_accumulatedDamage *= num5 / Drone.CrashEnergy;
								m_accumulatedDamage -= (1f - num6) * m_accumulatedDamage;
								if (m_debugCrashData)
								{
									D.Log("<color=#ffff00>Drone> Damage quadrant[" + quadrant.ToString() + "] energy[" + num5.ToString("0.0") + "] damage[" + drone.damage.ToString("0.0") + "] kph spinout[" + Drone.Spinout + "]] accumulated damage[" + m_accumulatedDamage + "]</color>");
								}
								drone.Damage(m_accumulatedDamage, p_contactNormal, p_impactVelocity, p_contactPoint, num5, quadrant);
							}
							m_accumulatingEnergy = false;
							m_accumulatedDamage = 0f;
							m_crashEnergies.Clear();
							m_crashCounter = 0f;
						}
					}
				}, 4f * Time.deltaTime);
			}
		}

		public Vector3 ForceToTorque(Vector3 force, Vector3 position)
		{
			Vector3 vector = Vector3.Cross(position - base.transform.position, force);
			vector *= Time.fixedDeltaTime;
			return base.transform.rotation * DRLPhysics.Div(Quaternion.Inverse(base.transform.rotation) * vector, rb.inertiaTensor);
		}
	}
}
