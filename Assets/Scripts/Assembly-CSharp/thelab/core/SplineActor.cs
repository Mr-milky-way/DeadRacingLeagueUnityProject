using UnityEngine;

namespace thelab.core
{
	public class SplineActor : MonoBehaviour
	{
		public enum Mode
		{
			Position = 1,
			Rotation = 2,
			Scale = 4,
			PositionRotation = 3,
			PositionScale = 5,
			RotationScale = 6,
			All = 7
		}

		public enum SnapMode
		{
			None = 0,
			Start = 1,
			End = 2,
			Closest = 3
		}

		public SplineComponent spline;

		public WrapMode wrap = WrapMode.Once;

		public Mode mode = Mode.Position;

		public bool reverse;

		public bool orient = true;

		[HideInInspector]
		public Transform lookAtObject;

		public bool useForward;

		public float speed = 1f;

		public float angularSpeed = 1f;

		public float smoothPosition = 0.1f;

		public float smoothRotation = 0.1f;

		public float threshold = 0.25f;

		public SnapMode snap;

		public bool auto;

		public bool wrapAngles;

		private float m_startingProgress;

		private float m_accumulativeProgress;

		public AnimationCurve moveSmooth = AnimationCurve.Constant(0f, 1f, 1f);

		public float moveIndex;

		public float position
		{
			get
			{
				return moveIndex;
			}
			set
			{
				moveIndex = value;
				MoveNormalized(value / ((float)spline.positions.values.Length - 1f));
			}
		}

		public float progress
		{
			get
			{
				if ((bool)spline)
				{
					if (!((float)spline.positions.values.Length <= 0f))
					{
						return moveIndex / ((float)spline.positions.values.Length - 1f);
					}
					return 0f;
				}
				return 0f;
			}
			set
			{
				if ((bool)spline)
				{
					position = (float)spline.positions.values.Length * Mathf.Clamp01(value);
				}
			}
		}

		public void Move(float p_position)
		{
			if ((bool)spline)
			{
				if ((mode & Mode.Position) != 0)
				{
					base.transform.position = spline.positions.Get(p_position);
				}
				if ((mode & Mode.Rotation) != 0)
				{
					Vector3 upwards = spline.ups.Get(p_position);
					Vector3 forward = spline.forwads.Get(p_position);
					base.transform.localRotation = Quaternion.LookRotation(forward, upwards);
				}
				if ((mode & Mode.Scale) != 0)
				{
					base.transform.localScale = spline.scales.Get(p_position);
				}
			}
		}

		public void MoveNormalized(float p_ratio)
		{
			if ((bool)spline)
			{
				if ((mode & Mode.Position) != 0)
				{
					base.transform.position = spline.positions.GetNormalized(p_ratio);
				}
				if ((mode & Mode.Rotation) != 0)
				{
					Vector3 normalized = spline.ups.GetNormalized(p_ratio);
					Vector3 normalized2 = spline.forwads.GetNormalized(p_ratio);
					base.transform.localRotation = Quaternion.LookRotation(normalized2, normalized);
				}
				if ((mode & Mode.Scale) != 0)
				{
					base.transform.localScale = spline.scales.GetNormalized(p_ratio);
				}
			}
		}

		public void Run()
		{
			if (reverse)
			{
				progress = 1f;
			}
			else
			{
				progress = 0f;
			}
			auto = true;
			m_accumulativeProgress = 0f;
		}

		public void Pause()
		{
			auto = false;
		}

		public void Resume()
		{
			auto = true;
		}

		private void Start()
		{
			Snap();
		}

		public void Snap()
		{
			switch (snap)
			{
			case SnapMode.Start:
				progress = 0f;
				break;
			case SnapMode.End:
				progress = 1f;
				break;
			case SnapMode.Closest:
			{
				Vector3 p_value = base.transform.position;
				float p_length = 0f;
				float length = spline.positions.length;
				float p_precision = 0.2f / length;
				spline.positions.GetClosestValue(p_value, ref p_length, p_precision);
				progress = ((length <= 0f) ? 0f : p_length);
				break;
			}
			}
			m_startingProgress = progress;
			m_accumulativeProgress = 0f;
		}

		public void SetReverse(bool p_reverse)
		{
			reverse = p_reverse;
		}

		private void Update()
		{
			if (!auto)
			{
				return;
			}
			float num = spline.positions.values.Length - 1;
			float time = Mathf.Clamp01(moveIndex / num);
			float num2 = moveSmooth.Evaluate(time) * speed * Time.deltaTime;
			if (reverse)
			{
				num2 = 0f - num2;
			}
			Vector3 vector = spline.positions.LerpDeriv(moveIndex);
			if (vector.magnitude > 0f)
			{
				num2 /= vector.magnitude;
			}
			moveIndex += num2;
			switch (wrap)
			{
			case WrapMode.Once:
			case WrapMode.ClampForever:
				moveIndex = Mathf.Clamp(moveIndex, 0f, num);
				break;
			case WrapMode.Loop:
				if (moveIndex > num)
				{
					moveIndex -= num;
				}
				if (moveIndex < 0f)
				{
					moveIndex = num + moveIndex;
				}
				break;
			case WrapMode.PingPong:
				if (moveIndex > num)
				{
					reverse = true;
					moveIndex = num;
				}
				if (moveIndex < 0f)
				{
					reverse = false;
					moveIndex = 0f;
				}
				break;
			}
			MoveNormalized(Mathf.Clamp01(moveIndex / num));
		}

		private void OnDrawGizmos()
		{
			if ((bool)spline)
			{
				Vector3 center = spline.positions.Get(position);
				center.y += 1f;
				Gizmos.color = Color.magenta;
				Gizmos.DrawSphere(center, 0.1f);
				center = base.transform.position;
				Gizmos.color = new Color(1f, 0.5f, 0.5f);
				Gizmos.DrawLine(center, center + base.transform.right * 0.5f);
				Gizmos.color = new Color(0.5f, 1f, 0.5f);
				Gizmos.DrawLine(center, center + base.transform.up * 0.5f);
				Gizmos.color = new Color(0.5f, 0.5f, 1f);
				Gizmos.DrawLine(center, center + base.transform.forward * 0.5f);
			}
		}
	}
}
