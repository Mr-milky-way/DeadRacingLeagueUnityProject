using System.Collections.Generic;
using UnityEngine;
using drl.sim;

namespace drl
{
	public class RadioQuality : MonoBehaviour
	{
		public CameraFX target;

		public AnimationCurve decay = AnimationCurve.Linear(0f, 1f, 50f, 0f);

		public float height = 155f;

		public List<Collider> ranges;

		[SerializeField]
		protected List<float> m_signals;

		public float minReception = 0.1f;

		public float maxReception = 0.75f;

		public float receptionRangeDistance = 6.6f;

		public float receptionRangeExp = 4f;

		public bool receptionEnabled;

		public float boundsSignal;

		public float receptionSignal;

		public List<Vector3> receptionDirections = new List<Vector3>
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(-1f, 0f, 0f),
			new Vector3(0f, 1f, 1f),
			new Vector3(0f, 1f, -1f),
			new Vector3(0f, 1f, 0f),
			new Vector3(-1f, 1f, 0f),
			new Vector3(1f, 1f, 0f)
		};

		[SerializeField]
		protected List<float> m_receptions;

		protected void Awake()
		{
			Initialize();
		}

		public void Initialize()
		{
			if (ranges == null)
			{
				ranges = new List<Collider>();
			}
			m_signals = new List<float>();
			m_receptions = new List<float>();
			for (int i = 0; i < receptionDirections.Count; i++)
			{
				receptionDirections[i].Normalize();
				m_receptions.Add(1f);
			}
			decay.preWrapMode = WrapMode.Once;
			decay.postWrapMode = WrapMode.Once;
			ranges.Clear();
			for (int j = 0; j < base.transform.childCount; j++)
			{
				Collider component = base.transform.GetChild(j).GetComponent<Collider>();
				if ((bool)component)
				{
					component.enabled = false;
					component.isTrigger = true;
					ranges.Add(component);
				}
			}
		}

		public float GetSignal()
		{
			if (!target)
			{
				return 0f;
			}
			return GetSignal(target.transform);
		}

		public float GetSignal(Transform p_target)
		{
			float result = 1f;
			if (ranges == null)
			{
				return result;
			}
			if (!p_target)
			{
				return result;
			}
			Vector3 position = p_target.position;
			m_signals.Clear();
			float num = 0f;
			bool flag = false;
			float num2 = 0f;
			num = Mathf.Max(0f, Mathf.Min(position.y, 2000f) - height);
			result = decay.Evaluate(num);
			if (result < 1f)
			{
				m_signals.Add(result);
			}
			num2 = result;
			for (int i = 0; i < ranges.Count; i++)
			{
				num = GetDistance(position, ranges[i]);
				num = Mathf.Max(num, 0f);
				if (num <= 0f)
				{
					flag = true;
					break;
				}
				result = decay.Evaluate(num);
				m_signals.Add(result);
			}
			result = 0f;
			for (int j = 0; j < m_signals.Count; j++)
			{
				result += m_signals[j] * m_signals[j];
			}
			result = Mathf.Sqrt(result);
			result = Mathf.Clamp01(result);
			if (!(num2 < 1f))
			{
				if (!flag)
				{
					return result;
				}
				return 1f;
			}
			return num2;
		}

		public float GetDistance(Vector3 p_position, Collider p_collider)
		{
			float result = 0f;
			if (p_collider is SphereCollider)
			{
				SphereCollider sphereCollider = p_collider as SphereCollider;
				result = Vector3.Distance(sphereCollider.transform.InverseTransformPoint(p_position), Vector3.zero);
				result = Mathf.Max(0f, result - sphereCollider.radius);
			}
			if (p_collider is BoxCollider)
			{
				BoxCollider boxCollider = p_collider as BoxCollider;
				Vector3 rhs = boxCollider.transform.InverseTransformPoint(p_position) - boxCollider.center;
				float num = 0f;
				float num2 = 0f;
				float num3 = 0f;
				float num4 = boxCollider.size.x * 0.5f;
				float num5 = boxCollider.size.y * 0.5f;
				float num6 = boxCollider.size.z * 0.5f;
				num = Mathf.Abs(Vector3.Dot(Vector3.right, rhs));
				num2 = Mathf.Abs(Vector3.Dot(Vector3.up, rhs));
				num3 = Mathf.Abs(Vector3.Dot(Vector3.forward, rhs));
				float x = Mathf.Max(0f, num - num4);
				float y = Mathf.Max(0f, num2 - num5);
				float z = Mathf.Max(0f, num3 - num6);
				return new Vector3(x, y, z).magnitude;
			}
			return result;
		}

		public float GetReception(Transform p_target)
		{
			Vector3 position = p_target.position;
			RaycastHit[] array = new RaycastHit[10];
			float num = ((m_receptions.Count <= 0) ? 1f : (1f / (float)m_receptions.Count));
			float num2 = 0f;
			for (int i = 0; i < receptionDirections.Count; i++)
			{
				Vector3 direction = receptionDirections[i];
				float t = 1f;
				if (Physics.RaycastNonAlloc(position, direction, array, receptionRangeDistance, DRLPhysics.Layers.Raycast_IgnoreDrone) > 0)
				{
					RaycastHit raycastHit = array[0];
					float num3 = Vector3.Distance(position, raycastHit.point);
					t = Mathf.Pow((receptionRangeDistance <= 0f) ? 1f : (num3 / receptionRangeDistance), 4f);
				}
				m_receptions[i] = Mathf.Lerp(minReception, maxReception, t);
				num2 += m_receptions[i] * num;
			}
			return num2;
		}

		protected void Update()
		{
			UpdateTarget(target);
		}

		public float UpdateTarget(CameraFX p_camera, float p_exp = 1f)
		{
			if (!p_camera)
			{
				return 1f;
			}
			float signal = GetSignal(p_camera.transform);
			signal = Mathf.Pow(signal, p_exp);
			float b = 1f;
			if (receptionEnabled)
			{
				b = GetReception(p_camera.transform);
			}
			boundsSignal = signal;
			receptionSignal = b;
			float b2 = Mathf.Min(signal, b);
			p_camera.radio = Mathf.Lerp(p_camera.radio, b2, Time.deltaTime * 3f);
			return p_camera.radio;
		}

		public float UpdateTarget(float p_exp = 1f)
		{
			return UpdateTarget(target, p_exp);
		}
	}
}
