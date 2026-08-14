using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class DroneCrashNode : MonoBehaviour
	{
		[SerializeField]
		private DroneCrashBody m_body;

		[SerializeField]
		private Renderer m_renderer;

		public List<DroneCrashNode> siblings;

		public List<DroneCrashNode> children;

		public List<CrashNodeType> tags;

		public List<CrashNodeType> links;

		public bool hideOnCrash;

		public Vector3 localCenterOfMass;

		public Vector3 worldCenterOfMass;

		public float integrity = 1f;

		public CrashNodeSturdiness sturdiness;

		public Transform nodeTransform;

		private Rigidbody rb;

		private bool hadRigidbody;

		private Collider outline;

		private bool hadCollider;

		private bool colliderWasEnabled;

		[SerializeField]
		private Transform parent;

		[SerializeField]
		private Vector3 position;

		[SerializeField]
		private Quaternion rotation;

		private bool isBroken;

		private static float m_totalSturdinessReduction;

		private bool m_reducedDamage;

		public DroneCrashBody body
		{
			get
			{
				if (!m_body)
				{
					return m_body = Hierarchy.FindReverse<DroneCrashBody>(base.transform);
				}
				return m_body;
			}
		}

		public Renderer renderer
		{
			get
			{
				if (!m_renderer)
				{
					return m_renderer = Hierarchy.Find<Renderer>(base.transform);
				}
				return m_renderer;
			}
		}

		public bool isDamaged => integrity <= 0f;

		public bool Match(CrashNodeType p_type)
		{
			if (tags == null)
			{
				return false;
			}
			return tags.IndexOf(p_type) >= 0;
		}

		public bool MatchAny(List<CrashNodeType> p_types)
		{
			for (int i = 0; i < p_types.Count; i++)
			{
				CrashNodeType p_type = p_types[i];
				if (Match(p_type))
				{
					return true;
				}
			}
			return false;
		}

		public void CalculateCenterOfMass()
		{
			if (!renderer)
			{
				localCenterOfMass = Vector3.zero;
				return;
			}
			Vector3 vector = (worldCenterOfMass = renderer.bounds.center);
			GameObject gameObject = new GameObject("node-com");
			gameObject.transform.parent = base.transform;
			gameObject.transform.position = vector;
			nodeTransform = gameObject.transform;
			vector = base.transform.InverseTransformPoint(vector);
			localCenterOfMass = vector;
		}

		private void Start()
		{
			SetFixData();
		}

		public void SetFixData()
		{
			parent = base.transform.parent;
			position = base.transform.localPosition;
			rotation = base.transform.localRotation;
		}

		public void Break(float p_crashEnergy, Vector3 p_velocityVector, float p_droneCrashThreshold, float p_transferRate, float p_forceFactor, Vector3 p_centerOfMass)
		{
			if (isBroken || !this || GetComponent<Drone>() != null || GetComponent<DroneFrame>() != null)
			{
				return;
			}
			isBroken = true;
			integrity = 1f - p_crashEnergy / p_droneCrashThreshold;
			SetFixData();
			float p_crashEnergy2;
			if (p_transferRate >= 1f)
			{
				p_crashEnergy2 = p_crashEnergy;
				if (p_transferRate > 1f)
				{
					integrity = 0.45f;
					p_crashEnergy2 = p_crashEnergy * Random.Range(0.5f, 1.5f);
				}
			}
			else
			{
				p_crashEnergy2 = p_crashEnergy - p_crashEnergy * (1f - p_transferRate);
			}
			foreach (DroneCrashNode sibling in siblings)
			{
				if (!(sibling == null))
				{
					sibling.Break(p_crashEnergy2, p_velocityVector, p_droneCrashThreshold, p_transferRate, p_forceFactor, p_centerOfMass);
				}
			}
			if (integrity > 0f && Random.Range(0.3f, 1f) > integrity)
			{
				integrity = 0f;
			}
			if (integrity > 0f)
			{
				return;
			}
			if (children != null)
			{
				for (int i = 0; i < children.Count; i++)
				{
					if (children[i] != null && !children[i].isDamaged)
					{
						children[i].transform.parent = base.transform;
					}
				}
			}
			if (rb == null)
			{
				rb = GetComponent<Rigidbody>();
				if (rb != null)
				{
					hadRigidbody = true;
				}
				else
				{
					hadRigidbody = false;
					rb = base.gameObject.AddComponent<Rigidbody>();
					DronePart component = GetComponent<DronePart>();
					if (component != null)
					{
						rb.mass = component.weight * 0.001f;
					}
					else
					{
						rb.mass = 0.1f;
					}
				}
			}
			else
			{
				hadRigidbody = true;
			}
			rb.useGravity = true;
			rb.interpolation = RigidbodyInterpolation.Interpolate;
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			if (outline == null)
			{
				outline = GetComponentInChildren<Collider>(includeInactive: true);
				if (outline != null)
				{
					hadCollider = true;
					colliderWasEnabled = outline.enabled;
				}
				else
				{
					BoxCollider boxCollider = base.gameObject.AddComponent<BoxCollider>();
					boxCollider.size = Vector3.one * 0.05f;
					hadCollider = false;
					outline = boxCollider;
				}
			}
			else
			{
				hadCollider = true;
				colliderWasEnabled = outline.enabled;
			}
			base.transform.parent = null;
			outline.enabled = false;
			this.TimerRunOnce(delegate
			{
				outline.enabled = true;
			}, 1f / 12f);
			p_crashEnergy = Mathf.Clamp(p_crashEnergy, 0f, 30f);
			float num = 3f * rb.mass * Mathf.Log(p_crashEnergy + 1f) * p_forceFactor;
			Vector3 force = (new Vector3(Random.Range(0f - num, num), Random.Range(0f - num, num), Random.Range(0f - num, num)) + (p_centerOfMass - base.transform.position) + p_velocityVector) * Random.Range(0.5f, num);
			rb.AddForce(force);
			rb.AddTorque(nodeTransform.position * (p_velocityVector.magnitude * Random.Range(-3f, 3f) * Random.Range(0.5f, num / 10f)));
		}

		public float CalculateTotalSturdiness(float p_totalNodes, float p_transferRate = 1f)
		{
			if (m_reducedDamage)
			{
				return m_totalSturdinessReduction;
			}
			m_totalSturdinessReduction += GetSturdinessFactor(sturdiness) / p_totalNodes;
			m_reducedDamage = true;
			foreach (DroneCrashNode sibling in siblings)
			{
				if (!(sibling == null))
				{
					sibling.CalculateTotalSturdiness(p_totalNodes, p_transferRate - p_transferRate / p_totalNodes);
				}
			}
			return m_totalSturdinessReduction;
		}

		public void ResetSturdinessReduction()
		{
			m_totalSturdinessReduction = 0f;
			m_reducedDamage = false;
		}

		private float GetSturdinessFactor(CrashNodeSturdiness p_sturdiness)
		{
			return p_sturdiness switch
			{
				CrashNodeSturdiness.Light => Drone.PropSturdiness, 
				CrashNodeSturdiness.Medium => Drone.ArmSturdiness, 
				CrashNodeSturdiness.Strong => Drone.BodySturdiness, 
				_ => Drone.ArmSturdiness, 
			};
		}

		public void Fix()
		{
			if (isBroken && (bool)this && !(GetComponent<Drone>() != null) && !(GetComponent<DroneFrame>() != null))
			{
				if (!hadRigidbody && rb != null)
				{
					Object.DestroyImmediate(rb);
					rb = null;
				}
				if (!hadCollider && outline != null)
				{
					Object.DestroyImmediate(outline);
					outline = null;
				}
				if (hadCollider && outline != null)
				{
					outline.enabled = colliderWasEnabled;
				}
				base.transform.parent = parent;
				base.transform.localPosition = position;
				base.transform.localRotation = rotation;
				isBroken = false;
				integrity = 1f;
			}
		}

		public void FixSnap()
		{
			base.transform.parent = parent;
			base.transform.localPosition = position;
			base.transform.localRotation = rotation;
		}
	}
}
