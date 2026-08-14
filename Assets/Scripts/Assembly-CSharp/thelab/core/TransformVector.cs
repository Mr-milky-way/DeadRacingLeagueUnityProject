using System;
using System.Text;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public struct TransformVector
	{
		public Transform target;

		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;

		private static StringBuilder _sb;

		public static TransformVector identity => new TransformVector(Vector3.zero);

		public Vector3 forward => rotation * Vector3.forward;

		public static TransformVector Lerp(TransformVector a, TransformVector b, float r)
		{
			return new TransformVector
			{
				position = Vector3.LerpUnclamped(a.position, b.position, r),
				rotation = Quaternion.SlerpUnclamped(a.rotation, b.rotation, r),
				scale = Vector3.LerpUnclamped(a.scale, b.scale, r)
			};
		}

		public static TransformVector Lerp(TransformVector a, TransformVector b, float r, Transform p_target, bool p_local)
		{
			TransformVector result = Lerp(a, b, r);
			result.Get(p_target, p_local);
			return result;
		}

		public static TransformVector Lerp(TransformVector a, TransformVector b, float r, Transform p_target)
		{
			return Lerp(a, b, r, p_target, p_local: true);
		}

		public TransformVector(Vector3 p_position, Quaternion p_rotation, Vector3 p_scale)
		{
			target = null;
			position = p_position;
			rotation = p_rotation;
			scale = p_scale;
		}

		public TransformVector(Vector3 p_position, Quaternion p_rotation)
			: this(p_position, p_rotation, Vector3.one)
		{
		}

		public TransformVector(Vector3 p_position)
			: this(p_position, Quaternion.identity, Vector3.one)
		{
		}

		public TransformVector(Transform p_target, bool p_local)
		{
			target = p_target;
			position = Vector3.zero;
			rotation = Quaternion.identity;
			scale = Vector3.one;
			Set(p_target, p_local);
		}

		public TransformVector(Transform p_target)
			: this(p_target, p_local: true)
		{
		}

		public void Set(Transform p_target, bool p_local)
		{
			if ((bool)p_target)
			{
				target = p_target;
				position = (p_local ? p_target.localPosition : p_target.position);
				rotation = (p_local ? p_target.localRotation : p_target.rotation);
				scale = p_target.localScale;
			}
		}

		public void Set(Transform p_target)
		{
			Set(p_target, p_local: true);
		}

		public void Set(Vector3 p_position, Quaternion p_rotation, Vector3 p_scale)
		{
			target = null;
			position = p_position;
			rotation = p_rotation;
			scale = p_scale;
		}

		public void Set(Vector3 p_position, Quaternion p_rotation)
		{
			Set(p_position, p_rotation, Vector3.one);
		}

		public void Set(Vector3 p_position)
		{
			Set(p_position, Quaternion.identity, Vector3.one);
		}

		public bool Get(Transform p_target, bool p_local)
		{
			if (!p_target)
			{
				return false;
			}
			if ((bool)target && p_target != target)
			{
				Debug.LogWarning("TransformVector> Target [" + target?.ToString() + "] not matching [" + p_target?.ToString() + "]");
				return false;
			}
			if (p_local)
			{
				p_target.localPosition = position;
			}
			else
			{
				p_target.position = position;
			}
			if (p_local)
			{
				p_target.localRotation = rotation;
			}
			else
			{
				p_target.rotation = rotation;
			}
			if (p_local)
			{
				p_target.localScale = scale;
			}
			else
			{
				p_target.localScale = scale;
			}
			return true;
		}

		public bool Get(Transform p_target)
		{
			return Get(p_target, p_local: true);
		}

		public override string ToString()
		{
			if (_sb == null)
			{
				_sb = new StringBuilder();
			}
			_sb.Clear();
			if ((bool)target)
			{
				_sb.Append("[" + target.name + "] / ");
			}
			Vector3 vector = position;
			_sb.Append("position[");
			_sb.Append(vector.x.ToString("0.0"));
			_sb.Append(",");
			_sb.Append(vector.y.ToString("0.0"));
			_sb.Append(",");
			_sb.Append(vector.z.ToString("0.0"));
			_sb.Append("] ");
			vector = rotation.eulerAngles;
			_sb.Append("rotation[");
			_sb.Append(vector.x.ToString("0.0"));
			_sb.Append(",");
			_sb.Append(vector.y.ToString("0.0"));
			_sb.Append(",");
			_sb.Append(vector.z.ToString("0.0"));
			_sb.Append("] ");
			vector = scale;
			_sb.Append("scale[");
			_sb.Append(vector.x.ToString("0.0"));
			_sb.Append(",");
			_sb.Append(vector.y.ToString("0.0"));
			_sb.Append(",");
			_sb.Append(vector.z.ToString("0.0"));
			_sb.Append("] ");
			return _sb.ToString();
		}
	}
}
