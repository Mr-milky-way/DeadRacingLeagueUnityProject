using System;
using UnityEngine;
using drl.sim.gatech;

namespace drl.sim
{
	public class AeroModelGATech : AeroModel
	{
		private drl.sim.gatech.AeroModel model;

		private GATechLookupData lookup;

		private float time;

		private bool useCrossFlow;

		private bool useUnsteady;

		private bool useShedding;

		private int recovery;

		private Vector3 localVelocity;

		private Vector3 localAngularVelocity;

		private float angleOfAttack;

		private float sine;

		private float _drag;

		private Vector3 dragDirection;

		private float _lift;

		private Vector3 liftDirection;

		public AeroModelGATech(GATechLookupData p_data)
		{
			lookup = p_data;
			model = new drl.sim.gatech.AeroModel(lookup, 1.225f);
			time = 0f;
		}

		public override void RecalculateForces(Drone p_drone, float p_dt, float p_mass, Vector3 p_transformUp, Vector3 p_velocity, Vector3 p_angularVelocity, Quaternion p_orientation)
		{
			if (!(p_drone == null) && p_drone.hasPhysics && p_drone.hasBody && p_drone.body.hasFrame)
			{
				if (p_drone.physics.airDensity <= 0f)
				{
					p_drone.physics.airDensity = 1.225f;
				}
				localVelocity = p_velocity;
				localAngularVelocity = p_angularVelocity;
				model.RecalculateForces(localVelocity, localAngularVelocity, p_drone.body.centerOfMass, useCrossFlow, use_unsteady: false, useShedding, 20f, p_dt, time, p_drone.physics.dragScale, p_drone.physics.liftScale, p_drone.physics.sideScale);
				m_totalForce = p_orientation * model.faB;
				m_moment = model.maB;
				angleOfAttack = 90f - Vector3.Angle(p_velocity, Vector3.up);
				if (recovery > 0 || !useUnsteady)
				{
					_drag = 0.5f * p_drone.physics.airDensity * p_velocity.sqrMagnitude * base.Cd * p_drone.physics.surfaceArea;
					dragDirection = p_orientation * p_velocity.normalized;
					m_dragForce = dragDirection * _drag;
					_lift = 0.5f * p_drone.physics.airDensity * p_velocity.sqrMagnitude * base.Cl * p_drone.physics.surfaceArea;
					liftDirection = p_orientation * (Quaternion.AngleAxis(90f, p_velocity) * Vector3.Cross(p_velocity, Vector3.up) * Mathf.Sign(angleOfAttack)).normalized;
					m_liftForce = liftDirection * _lift;
					m_totalForce = m_dragForce + m_liftForce;
				}
				else
				{
					dragDirection = p_orientation * p_velocity.normalized;
					m_dragForce = dragDirection * model.faW.x;
					liftDirection = p_orientation * (Quaternion.AngleAxis(90f, p_velocity) * Vector3.Cross(p_velocity, Vector3.up) * Mathf.Sign(angleOfAttack)).normalized;
					m_liftForce = liftDirection * model.faW.z;
				}
				m_moment = Vector3.zero;
			}
		}

		public override void Step(Drone p_drone, float p_dt, float p_mass, Vector3 p_transformUp, Vector3 p_velocity, Vector3 p_angularVelocity, Quaternion p_orientation)
		{
			if (p_drone == null || !p_drone.hasPhysics || !p_drone.hasBody || !p_drone.body.hasFrame)
			{
				base.Step(p_drone, p_dt, p_mass, p_transformUp, p_velocity, p_angularVelocity, p_orientation);
				return;
			}
			if (useCrossFlow != p_drone.physics.gatechUseCrossflow || useUnsteady != p_drone.physics.gatechUseUnsteady || useShedding != p_drone.physics.gatechUseShedding)
			{
				useCrossFlow = p_drone.physics.gatechUseCrossflow;
				useUnsteady = p_drone.physics.gatechUseUnsteady;
				useShedding = p_drone.physics.gatechUseShedding;
				Reset();
			}
			if (p_drone.physics.surfaceArea < 0f)
			{
				p_drone.physics.surfaceArea = p_drone.body.frame.surfaceArea.y;
			}
			if (p_drone.physics.dragScale < 0f)
			{
				p_drone.physics.dragScale = p_drone.body.frame.dragScaling.x;
			}
			if (p_drone.physics.liftScale < 0f)
			{
				p_drone.physics.liftScale = p_drone.body.frame.dragScaling.y;
			}
			if (p_drone.physics.sideScale < 0f)
			{
				p_drone.physics.sideScale = p_drone.body.frame.dragScaling.z;
			}
			if (p_drone.physics.airDensity <= 0f)
			{
				p_drone.physics.airDensity = 1.225f;
			}
			if (!Mathf.Approximately(EffectiveSurface(p_drone), model.Aref) || !Mathf.Approximately(p_drone.body.frame.size * 0.001f, model.Lref) || !Mathf.Approximately(p_drone.physics.airDensity, model.airDensity))
			{
				model.Aref = EffectiveSurface(p_drone);
				model.Lref = p_drone.body.frame.size * 0.001f;
				model.airDensity = p_drone.physics.airDensity;
				Reset();
			}
			localVelocity = p_velocity;
			localAngularVelocity = p_angularVelocity;
			bool flag = false;
			if (float.IsNaN(localVelocity.x))
			{
				localVelocity.x = 0f;
				flag = true;
			}
			if (float.IsNaN(localVelocity.y))
			{
				localVelocity.y = 0f;
				flag = true;
			}
			if (float.IsNaN(localVelocity.z))
			{
				localVelocity.z = 0f;
				flag = true;
			}
			if (float.IsNaN(localAngularVelocity.x))
			{
				localAngularVelocity.x = 0f;
				flag = true;
			}
			if (float.IsNaN(localAngularVelocity.y))
			{
				localAngularVelocity.y = 0f;
				flag = true;
			}
			if (float.IsNaN(localAngularVelocity.z))
			{
				localAngularVelocity.z = 0f;
				flag = true;
			}
			if (flag)
			{
				p_drone.FixNaN();
			}
			model.Calculate(localVelocity, localAngularVelocity, p_drone.body.centerOfMass, useCrossFlow, use_unsteady: false, useShedding, 20f, p_dt, time, p_drone.physics.dragScale, p_drone.physics.liftScale, p_drone.physics.sideScale);
			m_totalForce = p_orientation * model.faB;
			m_moment = model.maB;
			base.terminalVelocity = Vector3.zero;
			base.Cd = Mathf.Abs(model.CD);
			base.Cl = Mathf.Abs(model.CL);
			angleOfAttack = 90f - Vector3.Angle(p_velocity, Vector3.up);
			if ((float.IsNaN(base.Cd) || base.Cd > 35f || float.IsNaN(base.Cl) || base.Cl > 35f) && recovery <= 0)
			{
				Debug.LogError("GATech aermodel coefficients NaN, recovering...");
				p_drone.FixNaN();
				Debug.LogError("Drone state dumped OK");
				recovery = 30;
				Reset();
			}
			if (recovery > 0)
			{
				recovery--;
				if (p_drone.physics.ClMin < 0f)
				{
					p_drone.physics.ClMin = p_drone.body.frame.cL.x;
				}
				if (p_drone.physics.ClMax < 0f)
				{
					p_drone.physics.ClMax = p_drone.body.frame.cL.y;
				}
				if (p_drone.physics.CdMin < 0f)
				{
					p_drone.physics.CdMin = p_drone.body.frame.cD.x;
				}
				if (p_drone.physics.CdMax < 0f)
				{
					p_drone.physics.CdMax = p_drone.body.frame.cD.y;
				}
				sine = Mathf.Sin((float)Math.PI / 180f * angleOfAttack);
				base.Cd = p_drone.physics.CdMin + 2f * (p_drone.physics.CdMax - p_drone.physics.CdMin) * sine * sine;
				base.Cl = p_drone.physics.ClMin + (p_drone.physics.ClMax - p_drone.physics.ClMin) * sine;
				_drag = 0.5f * p_drone.physics.airDensity * p_velocity.sqrMagnitude * base.Cd * p_drone.physics.surfaceArea;
				dragDirection = p_orientation * p_velocity.normalized;
				m_dragForce = dragDirection * _drag;
				_lift = 0.5f * p_drone.physics.airDensity * p_velocity.sqrMagnitude * base.Cl * p_drone.physics.surfaceArea;
				liftDirection = p_orientation * (Quaternion.AngleAxis(90f, p_velocity) * Vector3.Cross(p_velocity, Vector3.up) * Mathf.Sign(angleOfAttack)).normalized;
				m_liftForce = liftDirection * _lift;
				m_totalForce = m_dragForce + m_liftForce;
			}
			else if (!useUnsteady)
			{
				_drag = 0.5f * p_drone.physics.airDensity * p_velocity.sqrMagnitude * base.Cd * p_drone.physics.surfaceArea;
				dragDirection = p_orientation * p_velocity.normalized;
				m_dragForce = dragDirection * _drag;
				_lift = 0.5f * p_drone.physics.airDensity * p_velocity.sqrMagnitude * base.Cl * p_drone.physics.surfaceArea;
				liftDirection = p_orientation * (Quaternion.AngleAxis(90f, p_velocity) * Vector3.Cross(p_velocity, Vector3.up) * Mathf.Sign(angleOfAttack)).normalized;
				m_liftForce = liftDirection * _lift;
				m_totalForce = m_dragForce + m_liftForce;
			}
			else
			{
				dragDirection = p_orientation * p_velocity.normalized;
				m_dragForce = dragDirection * model.faW.x;
				liftDirection = p_orientation * (Quaternion.AngleAxis(90f, p_velocity) * Vector3.Cross(p_velocity, Vector3.up) * Mathf.Sign(angleOfAttack)).normalized;
				m_liftForce = liftDirection * model.faW.z;
			}
			m_moment = Vector3.zero;
			time += p_dt;
			if (time > 10f)
			{
				Reset();
			}
		}

		public override void Reset()
		{
			model.Reset();
			time = 0f;
		}

		public float GetMaxCd(Drone p_drone)
		{
			if (p_drone.physics.surfaceArea < 0f)
			{
				p_drone.physics.surfaceArea = p_drone.body.frame.surfaceArea.y;
			}
			if (p_drone.physics.dragScale < 0f)
			{
				p_drone.physics.dragScale = p_drone.body.frame.dragScaling.x;
			}
			if (p_drone.physics.liftScale < 0f)
			{
				p_drone.physics.liftScale = p_drone.body.frame.dragScaling.y;
			}
			if (p_drone.physics.sideScale < 0f)
			{
				p_drone.physics.sideScale = p_drone.body.frame.dragScaling.z;
			}
			if (p_drone.physics.airDensity <= 0f)
			{
				p_drone.physics.airDensity = 1.225f;
			}
			if (!Mathf.Approximately(EffectiveSurface(p_drone), model.Aref) || !Mathf.Approximately(p_drone.body.frame.size * 0.001f, model.Lref))
			{
				model.Aref = EffectiveSurface(p_drone);
				model.Lref = p_drone.body.frame.size * 0.001f;
				model.airDensity = p_drone.physics.airDensity;
				Reset();
			}
			model.Calculate(Vector3.down, Vector3.zero, Vector3.zero, use_crossflow: false, use_unsteady: false, use_shedding: false, 10f, Time.deltaTime, 0f, p_drone.physics.dragScale, p_drone.physics.liftScale, p_drone.physics.sideScale);
			return Mathf.Abs(model.CD);
		}

		public float GetCdAtAngle(Drone p_drone, float p_angle)
		{
			if (p_drone.physics.surfaceArea < 0f)
			{
				p_drone.physics.surfaceArea = p_drone.body.frame.surfaceArea.y;
			}
			if (p_drone.physics.dragScale < 0f)
			{
				p_drone.physics.dragScale = p_drone.body.frame.dragScaling.x;
			}
			if (p_drone.physics.liftScale < 0f)
			{
				p_drone.physics.liftScale = p_drone.body.frame.dragScaling.y;
			}
			if (p_drone.physics.sideScale < 0f)
			{
				p_drone.physics.sideScale = p_drone.body.frame.dragScaling.z;
			}
			if (p_drone.physics.airDensity <= 0f)
			{
				p_drone.physics.airDensity = 1.225f;
			}
			if (!Mathf.Approximately(EffectiveSurface(p_drone), model.Aref) || !Mathf.Approximately(p_drone.body.frame.size * 0.001f, model.Lref))
			{
				model.Aref = EffectiveSurface(p_drone);
				model.Lref = p_drone.body.frame.size * 0.001f;
				model.airDensity = p_drone.physics.airDensity;
				Reset();
			}
			model.Calculate(Mathf.Cos((float)Math.PI / 180f * p_angle) * Vector3.forward + Mathf.Sin((float)Math.PI / 180f * p_angle) * Vector3.up, Vector3.zero, Vector3.zero, p_drone.physics.gatechUseCrossflow, use_unsteady: false, p_drone.physics.gatechUseShedding, 10f, Time.deltaTime, 0f, p_drone.physics.dragScale, p_drone.physics.liftScale, p_drone.physics.sideScale);
			base.Cd = model.CD;
			base.Cl = model.CL;
			return Mathf.Abs(model.CD);
		}

		public float EffectiveSurface(Drone p_drone)
		{
			if (lookup.areaReference < 0.1f)
			{
				return p_drone.physics.surfaceArea * 0.3f;
			}
			return p_drone.physics.surfaceArea * lookup.areaReference;
		}
	}
}
