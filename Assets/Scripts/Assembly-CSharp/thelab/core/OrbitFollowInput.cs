using UnityEngine;

namespace thelab.core
{
	public class OrbitFollowInput : MonoBehaviour
	{
		public enum Flag
		{
			None = 0,
			PositionX = 1,
			PositionY = 2,
			PositionZ = 4,
			PositionXY = 3,
			PositionXZ = 5,
			PositionYZ = 6,
			PositionXYZ = 7,
			RotationX = 8,
			RotationY = 16,
			RotationZ = 32,
			RotationXY = 24,
			RotationXZ = 40,
			RotationYZ = 48,
			RotationXYZ = 56,
			All = 63
		}

		private OrbitTransform m_orbit;

		public Transform target;

		public Vector3 offset;

		public Flag flags;

		public OrbitTransform orbit
		{
			get
			{
				if (!m_orbit)
				{
					return m_orbit = GetComponent<OrbitTransform>();
				}
				return m_orbit;
			}
		}

		public void Snap()
		{
			if ((bool)target)
			{
				orbit.anchor = target.position;
				orbit.anchorRotation = target.rotation;
			}
		}

		public void Follow(bool p_x, bool p_y, bool p_z, bool p_rx, bool p_ry, bool p_rz)
		{
			flags = Flag.None;
			flags |= (Flag)(p_x ? 1 : 0);
			flags |= (Flag)(p_y ? 2 : 0);
			flags |= (Flag)(p_z ? 4 : 0);
			flags |= (Flag)(p_rx ? 8 : 0);
			flags |= (Flag)(p_ry ? 16 : 0);
			flags |= (Flag)(p_rz ? 32 : 0);
		}

		protected void LateUpdate()
		{
			Follow();
		}

		protected void Follow()
		{
			if (!target || !base.enabled)
			{
				return;
			}
			Vector3 position = target.position;
			Vector3 eulerAngles = target.eulerAngles;
			Vector3 vector = (orbit ? orbit.anchor : base.transform.position);
			Vector3 vector2 = (orbit ? orbit.anchorEulerAngles : base.transform.eulerAngles);
			if ((flags & Flag.PositionX) == 0)
			{
				position.x = vector.x;
			}
			if ((flags & Flag.PositionY) == 0)
			{
				position.y = vector.y;
			}
			if ((flags & Flag.PositionZ) == 0)
			{
				position.z = vector.z;
			}
			if ((flags & Flag.RotationX) == 0)
			{
				eulerAngles.x = vector2.x;
			}
			if ((flags & Flag.RotationY) == 0)
			{
				eulerAngles.y = vector2.y;
			}
			if ((flags & Flag.RotationZ) == 0)
			{
				eulerAngles.z = vector2.z;
			}
			bool num = !orbit || !orbit.IsTransitionEnabled(OrbitTransform.Transition.AnchorLock);
			bool flag = !orbit || !orbit.IsTransitionEnabled(OrbitTransform.Transition.AnchorRotationLock);
			if (num && (flags & Flag.PositionXYZ) != Flag.None)
			{
				if ((bool)orbit)
				{
					orbit.anchor = position + offset;
				}
				else
				{
					base.transform.position = position + offset;
				}
			}
			if (flag && (flags & Flag.RotationXYZ) != Flag.None)
			{
				if ((bool)orbit)
				{
					orbit.anchorRotation = Quaternion.Euler(eulerAngles);
				}
				else
				{
					base.transform.rotation = Quaternion.Euler(eulerAngles);
				}
			}
		}

		public void SetOffset(Vector3 p_offset)
		{
			offset = p_offset;
		}
	}
}
