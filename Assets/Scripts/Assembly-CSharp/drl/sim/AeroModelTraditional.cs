using System;
using UnityEngine;

namespace drl.sim
{
	public class AeroModelTraditional : AeroModel
	{
		private float m;

		private float g;

		private float mg;

		private float angleOfAttack;

		private float sine;

		private float _drag;

		private Vector3 dragDirection;

		private float _lift;

		private Vector3 liftDirection;

		public override void RecalculateForces(Drone p_drone, float p_dt, float p_mass, Vector3 p_transformUp, Vector3 p_velocity, Vector3 p_angularVelocity, Quaternion p_orientation)
		{
			if (!(p_drone == null) && p_drone.hasPhysics && p_drone.hasBody && p_drone.body.hasFrame)
			{
				if (p_drone.physics.airDensity <= 0f)
				{
					p_drone.physics.airDensity = 1.225f;
				}
				angleOfAttack = 90f - Vector3.Angle(-p_velocity, Vector3.up);
				_drag = 0.5f * p_drone.physics.airDensity * p_velocity.sqrMagnitude * base.Cd * p_drone.physics.surfaceArea;
				dragDirection = p_orientation * p_velocity.normalized;
				m_dragForce = dragDirection * _drag;
				_lift = 0.5f * p_drone.physics.airDensity * p_velocity.sqrMagnitude * base.Cl * ((p_drone.physics.surfaceArea > 0f) ? p_drone.physics.surfaceArea : p_drone.body.frame.surfaceArea.y);
				liftDirection = p_orientation * (Quaternion.AngleAxis(90f, -p_velocity) * Vector3.Cross(-p_velocity, Vector3.up) * Mathf.Sign(angleOfAttack)).normalized;
				m_liftForce = liftDirection * _lift;
				m_totalForce = m_dragForce + m_liftForce;
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
			if (p_drone.physics.CdMin < 0f)
			{
				p_drone.physics.CdMin = p_drone.body.frame.cD.x;
			}
			if (p_drone.physics.CdMax < 0f)
			{
				p_drone.physics.CdMax = p_drone.body.frame.cD.y;
			}
			if (p_drone.physics.surfaceArea < 0f)
			{
				p_drone.physics.surfaceArea = p_drone.body.frame.surfaceArea.y;
			}
			if (p_drone.physics.airDensity <= 0f)
			{
				p_drone.physics.airDensity = 1.225f;
			}
			angleOfAttack = 90f - Vector3.Angle(-p_velocity, Vector3.up);
			sine = Mathf.Sin((float)Math.PI / 180f * angleOfAttack);
			base.Cd = p_drone.physics.CdMin + 2f * (p_drone.physics.CdMax - p_drone.physics.CdMin) * sine * sine;
			_drag = 0.5f * p_drone.physics.airDensity * p_velocity.sqrMagnitude * base.Cd * p_drone.physics.surfaceArea;
			dragDirection = p_orientation * p_velocity.normalized;
			m_dragForce = dragDirection * _drag;
			if (p_drone.physics.legacyDrag)
			{
				m_dragForce = p_drone.fc.sensor.inertial.velocityX.sqrMagnitude * 0.05f * -p_drone.fc.sensor.inertial.velocityX.normalized + p_drone.fc.sensor.inertial.velocityY.sqrMagnitude * 0.078f * -p_drone.fc.sensor.inertial.velocityY.normalized + p_drone.fc.sensor.inertial.velocityZ.sqrMagnitude * 0.04f * -p_drone.fc.sensor.inertial.velocityZ.normalized;
			}
			if (p_drone.physics.legacyDrag)
			{
				m_liftForce = Vector3.zero;
			}
			else
			{
				if (p_drone.physics.ClMin < 0f)
				{
					p_drone.physics.ClMin = p_drone.body.frame.cL.x;
				}
				if (p_drone.physics.ClMax < 0f)
				{
					p_drone.physics.ClMax = p_drone.body.frame.cL.y;
				}
				base.Cl = p_drone.physics.ClMin + (p_drone.physics.ClMax - p_drone.physics.ClMin) * sine;
				_lift = 0.5f * p_drone.physics.airDensity * p_velocity.sqrMagnitude * base.Cl * p_drone.physics.surfaceArea;
				liftDirection = p_orientation * (Quaternion.AngleAxis(90f, -p_velocity) * Vector3.Cross(-p_velocity, Vector3.up) * Mathf.Sign(angleOfAttack)).normalized;
				m_liftForce = liftDirection * _lift;
			}
			m_totalForce = m_dragForce + m_liftForce;
			m_moment = Vector3.zero;
			base.terminalVelocity = Vector3.zero;
		}
	}
}
