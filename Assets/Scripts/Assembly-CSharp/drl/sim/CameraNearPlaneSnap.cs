using UnityEngine;

namespace drl.sim
{
	public class CameraNearPlaneSnap : MonoBehaviour
	{
		[SerializeField]
		private Camera m_camera;

		public LayerMask ignoreLayer;

		public float delay = 1f / 30f;

		public bool hit;

		public RaycastHit raycast;

		public float nearClipCamera;

		public float nearClipMin = 0.1f;

		public bool debug;

		private float m_elapsed;

		private Vector3[] m_vpp;

		private Ray m_ray;

		private Collider[] m_cast_results = new Collider[20];

		public Camera camera
		{
			get
			{
				if ((bool)m_camera)
				{
					return m_camera;
				}
				m_camera = GetComponent<Camera>();
				if ((bool)m_camera)
				{
					return m_camera;
				}
				m_camera = Camera.main;
				return m_camera;
			}
			set
			{
				m_camera = value;
			}
		}

		public int ignoreLayerBits => ignoreLayer.value;

		protected void Awake()
		{
			m_vpp = new Vector3[7]
			{
				Vector3.zero,
				Vector3.zero,
				Vector3.zero,
				Vector3.zero,
				Vector3.zero,
				Vector3.zero,
				Vector3.zero
			};
			float num = -0.1f;
			float num2 = 1.1f;
			m_vpp[0] = new Vector3(num, num2, 0.2f);
			m_vpp[1] = new Vector3(num2, num2, 0.2f);
			m_vpp[2] = new Vector3(num2, num, 0.2f);
			m_vpp[3] = new Vector3(num, num, 0.2f);
			m_vpp[4] = new Vector3(0.5f, 0.5f, 0.2f);
			m_vpp[5] = new Vector3(num2, -0.2f, 0.2f);
			m_vpp[6] = new Vector3(num, -0.2f, 0.2f);
			m_ray = default(Ray);
			nearClipCamera = (camera ? camera.nearClipPlane : 0f);
			m_elapsed = 0f;
		}

		protected void LateUpdate()
		{
			Camera camera = this.camera;
			float unscaledDeltaTime = Time.unscaledDeltaTime;
			float num = nearClipCamera;
			if (hit)
			{
				num = nearClipMin;
			}
			if (Mathf.Abs(camera.nearClipPlane - num) > 0.0005f)
			{
				camera.nearClipPlane = Mathf.Lerp(camera.nearClipPlane, num, 1f);
			}
			m_elapsed += unscaledDeltaTime;
			if (m_elapsed < delay)
			{
				return;
			}
			m_elapsed = 0f;
			if ((bool)camera)
			{
				bool flag = false;
				float radius = nearClipCamera * 1.5f;
				_ = m_ray;
				if (Physics.OverlapSphereNonAlloc(camera.transform.position, radius, m_cast_results, ~ignoreLayerBits) > 0)
				{
					flag = true;
				}
				if (hit && !flag)
				{
					hit = flag;
					OnNearExit();
				}
				if (!hit && flag)
				{
					hit = flag;
					OnNearEnter();
				}
			}
		}

		protected virtual void OnNearEnter()
		{
		}

		protected virtual void OnNearExit()
		{
		}
	}
}
