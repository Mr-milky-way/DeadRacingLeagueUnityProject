using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class MEPreviewPhysics : MonoBehaviour
	{
		public MAEntity target;

		public Mesh meshBasic;

		public List<Collider> colliders;

		public float delay;

		public float duration;

		public float bounce;

		public Vector3 velocity;

		public Vector3 angularVelocity;

		public Rigidbody rigidbody;

		public float elapsed;

		private Dictionary<Collider, bool> m_collider_convex_lut;

		private Dictionary<Collider, Mesh> m_collider_mesh_lut;

		public void Set(float p_delay, float p_duration, Vector3 p_velocity, Vector3 p_angular_velocity)
		{
			delay = p_delay;
			duration = p_duration;
			velocity = p_velocity;
			angularVelocity = p_angular_velocity;
		}

		public void Run()
		{
			m_collider_convex_lut = new Dictionary<Collider, bool>();
			m_collider_mesh_lut = new Dictionary<Collider, Mesh>();
			elapsed = 0f - (delay + 0.05f);
			target = GetComponent<MAEntity>();
			if ((bool)target)
			{
				MARenderer mARenderer = target as MARenderer;
				if ((bool)mARenderer)
				{
					meshBasic = mARenderer.GetMeshSimple();
				}
			}
			colliders = (target ? Hierarchy.FindAll<Collider>(target.transform) : new List<Collider>());
		}

		public void Apply()
		{
			if (!rigidbody)
			{
				SetCollidersReady(p_flag: true);
				Invoke("CreateRigidbody", 1f / 30f);
			}
		}

		private void CreateRigidbody()
		{
			try
			{
				Rigidbody obj = (rigidbody = base.gameObject.AddComponent<Rigidbody>());
				obj.velocity = velocity;
				obj.angularVelocity = angularVelocity;
				obj.isKinematic = false;
			}
			catch (Exception)
			{
				Clear();
			}
		}

		public void SetCollidersReady(bool p_flag)
		{
			for (int i = 0; i < colliders.Count; i++)
			{
				Collider collider = colliders[i];
				if (!collider)
				{
					continue;
				}
				MeshCollider meshCollider = collider as MeshCollider;
				if (!meshCollider)
				{
					continue;
				}
				if (!p_flag)
				{
					meshCollider.convex = m_collider_convex_lut.ContainsKey(collider) && m_collider_convex_lut[collider];
					meshCollider.sharedMesh = (m_collider_mesh_lut.ContainsKey(collider) ? m_collider_mesh_lut[collider] : meshCollider.sharedMesh);
					continue;
				}
				m_collider_convex_lut[collider] = meshCollider.convex;
				m_collider_mesh_lut[collider] = meshCollider.sharedMesh;
				try
				{
					if ((bool)meshBasic)
					{
						meshCollider.sharedMesh = meshBasic;
					}
					meshCollider.convex = true;
				}
				catch (Exception)
				{
					Clear();
					break;
				}
			}
		}

		public void Clear()
		{
			if ((bool)rigidbody)
			{
				UnityEngine.Object.Destroy(rigidbody);
			}
			Invoke("DelayClear", 1f / 30f);
		}

		private void DelayClear()
		{
			SetCollidersReady(p_flag: false);
			UnityEngine.Object.Destroy(this);
		}

		protected void FixedUpdate()
		{
			if (!this)
			{
				return;
			}
			float fixedUnscaledDeltaTime = Time.fixedUnscaledDeltaTime;
			if (elapsed < 0f && elapsed + fixedUnscaledDeltaTime >= 0f)
			{
				Apply();
			}
			elapsed += fixedUnscaledDeltaTime;
			if (!(elapsed < 0f))
			{
				if (duration > 0f && elapsed >= duration)
				{
					Clear();
				}
				else if (elapsed >= 40f)
				{
					Clear();
				}
				else if ((bool)rigidbody && rigidbody.IsSleeping())
				{
					Clear();
				}
			}
		}
	}
}
