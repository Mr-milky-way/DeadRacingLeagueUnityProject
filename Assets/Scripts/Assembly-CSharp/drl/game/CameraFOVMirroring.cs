using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class CameraFOVMirroring : ActivityBehaviour, ILateUpdateable
	{
		public Camera source;

		public Camera destination;

		public float rate = 1f / 12f;

		public float bias = 0.5f;

		private float m_elapsed;

		protected void Start()
		{
			Sync();
		}

		public void OnLateUpdate()
		{
			if (!source || !destination)
			{
				return;
			}
			m_elapsed += Time.deltaTime;
			if (!(m_elapsed < rate))
			{
				m_elapsed = 0f;
				if (IsDirty())
				{
					Sync();
				}
			}
		}

		public bool IsDirty()
		{
			if (!source)
			{
				return false;
			}
			if (!destination)
			{
				return false;
			}
			float fieldOfView = source.fieldOfView;
			float fieldOfView2 = destination.fieldOfView;
			return Mathf.Abs(fieldOfView - fieldOfView2) > bias;
		}

		public void Sync()
		{
			if ((bool)source && (bool)destination)
			{
				destination.fieldOfView = source.fieldOfView;
				m_elapsed = 0f;
			}
		}
	}
}
