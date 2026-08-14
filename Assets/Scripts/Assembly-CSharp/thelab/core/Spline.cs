using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class Spline<T>
	{
		private T[] m_values;

		private float[] m_weights;

		private float[] m_lengths;

		private T[] m_samples;

		public T[] values
		{
			get
			{
				return m_values;
			}
			set
			{
				T[] array = ((value == null) ? new T[0] : value);
				m_values = new T[array.Length];
				m_lengths = new float[array.Length];
				m_weights = new float[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					m_values[i] = array[i];
				}
				Refresh();
			}
		}

		public float[] weights => m_weights;

		public float[] lengths => m_lengths;

		public float length { get; private set; }

		public MathType data { get; private set; }

		public SplineType type { get; set; }

		public Spline(SplineType p_type, int p_length, float p_precision)
		{
			m_values = new T[p_length];
			m_weights = new float[p_length];
			m_lengths = new float[p_length];
			m_samples = new T[8];
			type = p_type;
			if (typeof(T) == typeof(float))
			{
				data = MathType.Float;
			}
			else if (typeof(T) == typeof(Color))
			{
				data = MathType.Color;
			}
			else if (typeof(T) == typeof(Vector3))
			{
				data = MathType.Vector3;
			}
			else if (typeof(T) == typeof(Vector2))
			{
				data = MathType.Vector2;
			}
			else if (typeof(T) == typeof(Quaternion))
			{
				data = MathType.Quaternion;
			}
			else if (typeof(T) == typeof(Transform))
			{
				data = MathType.Transform;
			}
			else if (typeof(T) == typeof(Rect))
			{
				data = MathType.Rect;
			}
			else if (typeof(T) == typeof(Vector4))
			{
				data = MathType.Vector4;
			}
			else if (typeof(T) == typeof(int))
			{
				data = MathType.Int;
			}
		}

		public Spline(SplineType p_type, int p_length)
			: this(p_type, p_length, 0.01f)
		{
		}

		public Spline(Spline<T> p_src)
			: this(p_src.type, p_src.values.Length, 0.1f)
		{
			Array.Copy(p_src.values, m_values, m_values.Length);
			Array.Copy(p_src.m_weights, m_weights, m_weights.Length);
			Array.Copy(p_src.m_lengths, m_lengths, m_lengths.Length);
		}

		public T GetNormalized(float p_ratio)
		{
			T[] array = m_values;
			if (array.Length == 0)
			{
				return default(T);
			}
			if (array.Length == 1)
			{
				return array[0];
			}
			float samplesAndRatio = (float)(values.Length - 1) * p_ratio;
			float r = SetSamplesAndRatio(samplesAndRatio);
			return Lerp(r, m_samples);
		}

		protected virtual T Lerp(float r, T[] vl)
		{
			return Spline.Lerp(data, type, r, vl);
		}

		public T Lerp(float p_position)
		{
			float r = SetSamplesAndRatio(p_position);
			return Lerp(r, m_samples);
		}

		public T LerpDeriv(float p_position)
		{
			float r = SetSamplesAndRatio(p_position);
			return Spline.LerpDeriv(data, type, r, m_samples);
		}

		public T GetNormalized(float p_ratio, bool p_use_weights)
		{
			return GetNormalized(p_ratio);
		}

		public T Get(float p_distance)
		{
			float num = Mathf.Clamp(p_distance, 0f, length);
			return GetNormalized((length <= 0f) ? 0f : (num / length));
		}

		public T GetClosestNode(float p_distance, out int p_index)
		{
			p_index = -1;
			if (m_values.Length == 0)
			{
				return default(T);
			}
			T a = Get(p_distance);
			T val = m_values[0];
			T result = val;
			p_index = 0;
			float num = Distance(a, val);
			for (int i = 1; i < m_values.Length; i++)
			{
				val = m_values[i];
				float num2 = Distance(a, val);
				if (num2 < num)
				{
					result = val;
					num = num2;
					p_index = i;
				}
			}
			return result;
		}

		public T GetClosestValue(T p_value, ref float p_length, float p_precision = 0.0001f)
		{
			if (values.Length == 0)
			{
				return default(T);
			}
			T p_closest = values[0];
			float p_step = 0.5f;
			p_length = 0.5f;
			BinarySearchClosest(p_value, ref p_closest, p_length, p_step, ref p_length, p_precision);
			return p_closest;
		}

		public T GetClosestValue(T p_value, float p_precision = 0.0001f)
		{
			float p_length = 0f;
			return GetClosestValue(p_value, ref p_length, p_precision);
		}

		private void BinarySearchClosest(T p_target, ref T p_closest, float p_pos, float p_step, ref float p_length, float p_precision = 0.005f)
		{
			if (p_step <= 0.0001f)
			{
				return;
			}
			float num = Distance(p_closest, p_target);
			if (!(num <= p_precision))
			{
				float num2 = Mathf.Clamp01(p_pos - p_step);
				T normalized = GetNormalized(num2);
				if (Distance(normalized, p_target) < num)
				{
					p_closest = normalized;
					p_length = num2;
				}
				float num3 = Mathf.Clamp01(p_pos + p_step);
				T normalized2 = GetNormalized(num3);
				if (Distance(normalized2, p_target) < num)
				{
					p_closest = normalized2;
					p_length = num3;
				}
				BinarySearchClosest(p_target, ref p_closest, num2, p_step * 0.5f, ref p_length, p_precision);
				BinarySearchClosest(p_target, ref p_closest, num3, p_step * 0.5f, ref p_length, p_precision);
			}
		}

		public T GetClosestNode(float p_distance)
		{
			int p_index = 0;
			return GetClosestNode(p_distance, out p_index);
		}

		public bool IsControl(int p_index)
		{
			int num = p_index;
			switch (type)
			{
			case SplineType.Linear:
				return true;
			case SplineType.Catmull:
				return true;
			case SplineType.Bezier2:
				num %= 2;
				return num == 0;
			case SplineType.Bezier3:
				num %= 3;
				return num == 0;
			default:
				return true;
			}
		}

		public bool IsTangent(int p_index)
		{
			return !IsControl(p_index);
		}

		public T GetControl(int p_index)
		{
			int controlIndex = GetControlIndex(p_index);
			return values[controlIndex];
		}

		public int GetControlIndex(int p_index)
		{
			int num = Mathf.Clamp(p_index, 0, values.Length - 1);
			switch (type)
			{
			case SplineType.Bezier2:
				if (num % 2 == 1)
				{
					num = Mathf.Max(num - 1, 0);
				}
				break;
			case SplineType.Bezier3:
				if (num % 3 == 1)
				{
					num = Mathf.Max(num - 1, 0);
				}
				if (num % 3 == 2)
				{
					num = Mathf.Min(num + 1, values.Length - 1);
				}
				break;
			}
			return num;
		}

		public int[] GetTangentsIndex(int p_index)
		{
			List<int> list = new List<int>();
			if (IsTangent(p_index))
			{
				return list.ToArray();
			}
			switch (type)
			{
			case SplineType.Bezier2:
				if (p_index < values.Length - 1)
				{
					list.Add(p_index + 1);
				}
				break;
			case SplineType.Bezier3:
				if (p_index > 0)
				{
					list.Add(p_index - 1);
				}
				if (p_index < values.Length - 1)
				{
					list.Add(p_index + 1);
				}
				break;
			}
			return list.ToArray();
		}

		public float MoveTowards(ref T p_current, float p_position, float p_speed, float p_threshold = 0.1f)
		{
			T val = p_current;
			T val2 = Get(Mathf.Clamp(p_position, 0f, length));
			float num = Distance(val, val2);
			float result = Mathf.Max(p_threshold - num, 0f);
			p_current = Move(val, val2, p_speed);
			return result;
		}

		protected virtual T Move(T p0, T p1, float s)
		{
			switch (data)
			{
			case MathType.Float:
			{
				float num = C<float>(p0);
				C<float>(p1);
				return C<T>(num + s);
			}
			case MathType.Vector2:
			{
				Vector2 vector7 = C<Vector2>(p0);
				Vector2 vector8 = C<Vector2>(p1);
				return C<T>(vector7 + (vector8 - vector7).normalized * s);
			}
			case MathType.Vector3:
			{
				Vector3 vector5 = C<Vector3>(p0);
				Vector3 vector6 = C<Vector3>(p1);
				return C<T>(vector5 + (vector6 - vector5).normalized * s);
			}
			case MathType.Vector4:
			{
				Vector4 vector3 = C<Vector4>(p0);
				Vector4 vector4 = C<Vector4>(p1);
				return C<T>(vector3 + (vector4 - vector3).normalized * s);
			}
			case MathType.Color:
			{
				Vector4 vector = C<Vector4>(p0);
				Vector4 vector2 = C<Vector4>(p1);
				return C<T>(vector + (vector2 - vector).normalized * s);
			}
			case MathType.Quaternion:
			{
				Quaternion quaternion = C<Quaternion>(p0);
				Quaternion quaternion2 = C<Quaternion>(p1);
				Quaternion quaternion3 = C<Quaternion>(p0);
				quaternion3.x = quaternion.x + (quaternion2.x - quaternion.x) * s;
				quaternion3.y = quaternion.y + (quaternion2.y - quaternion.y) * s;
				quaternion3.z = quaternion.z + (quaternion2.z - quaternion.z) * s;
				quaternion3.w = quaternion.w + (quaternion2.w - quaternion.w) * s;
				return C<T>(quaternion3);
			}
			default:
				return default(T);
			}
		}

		public void Resize(int p_length)
		{
			T[] array = new T[p_length];
			int num = Mathf.Min(array.Length, m_values.Length);
			for (int i = 0; i < num; i++)
			{
				array[i] = m_values[i];
			}
			values = array;
		}

		public void Refresh()
		{
			length = 0f;
			T[] array = m_values;
			if (array.Length == 0)
			{
				return;
			}
			float[] array2 = m_lengths;
			float[] array3 = m_weights;
			array2[0] = (array3[0] = 0f);
			if (array.Length > 1)
			{
				float num = array.Length;
				float num2 = 1f / (num - 1f);
				for (int i = 1; i < array.Length; i++)
				{
					float num3 = i;
					float ra = (num3 - 1f) * num2;
					float rb = num3 * num2;
					float num4 = MeasureStep(ra, rb, 0.05f);
					length += num4;
					array2[i] = length;
				}
				float num5 = 0f;
				for (int j = 0; j < array.Length; j++)
				{
					num5 = ((length <= 0f) ? 0f : (array2[j] / length));
					array3[j] = num5;
				}
			}
		}

		public float SetSamplesAndRatio(float p_position)
		{
			int num = m_values.Length;
			T[] array = m_values;
			T[] samples = m_samples;
			float result = 0f;
			float num2 = p_position;
			int value = 0;
			switch (type)
			{
			case SplineType.Linear:
				value = Mathf.FloorToInt(num2);
				break;
			case SplineType.Catmull:
				value = Mathf.FloorToInt(num2);
				break;
			case SplineType.Bezier2:
				value = Mathf.FloorToInt(num2 / 2f) * 2;
				break;
			case SplineType.Bezier3:
				value = Mathf.FloorToInt(num2 / 3f) * 3;
				break;
			}
			value = Mathf.Clamp(value, 0, num - 1);
			int num3 = Mathf.Min(value + 1, num - 1);
			int num4 = Mathf.Max(value - 1, 0);
			int num5 = Mathf.Min(value + 2, num - 1);
			int num6 = Mathf.Min(value + 3, num - 1);
			switch (type)
			{
			case SplineType.Linear:
				samples[0] = array[value];
				samples[1] = array[num3];
				break;
			case SplineType.Catmull:
				samples[0] = array[num4];
				samples[1] = array[value];
				samples[2] = array[num3];
				samples[3] = array[num5];
				break;
			case SplineType.Bezier2:
				samples[0] = array[value];
				samples[1] = array[num3];
				samples[2] = array[num5];
				break;
			case SplineType.Bezier3:
				samples[0] = array[value];
				samples[1] = array[num3];
				samples[2] = array[num5];
				samples[3] = array[num6];
				break;
			}
			switch (type)
			{
			case SplineType.Linear:
				result = num2 - Mathf.Floor(num2);
				break;
			case SplineType.Catmull:
				result = num2 - Mathf.Floor(num2);
				break;
			case SplineType.Bezier2:
				num2 /= 2f;
				result = num2 - Mathf.Floor(num2);
				break;
			case SplineType.Bezier3:
				num2 /= 3f;
				result = num2 - Mathf.Floor(num2);
				break;
			}
			return result;
		}

		private float MeasureStep(float ra, float rb, float prec)
		{
			if (rb - ra > prec)
			{
				float num = (rb - ra) * 0.5f;
				float num2 = MeasureStep(ra, rb - num, prec);
				float num3 = MeasureStep(ra + num, rb, prec);
				return num2 + num3;
			}
			T normalized = GetNormalized(ra, p_use_weights: false);
			T normalized2 = GetNormalized(rb, p_use_weights: false);
			return Distance(normalized, normalized2);
		}

		protected virtual float Distance(T a, T b)
		{
			switch (data)
			{
			case MathType.Float:
			{
				float num = C<float>(a);
				return Mathf.Abs(C<float>(b) - num);
			}
			case MathType.Vector2:
			{
				Vector2 b4 = C<Vector2>(a);
				return Vector2.Distance(C<Vector2>(b), b4);
			}
			case MathType.Vector3:
			{
				Vector3 b3 = C<Vector3>(a);
				return Vector3.Distance(C<Vector3>(b), b3);
			}
			case MathType.Vector4:
			{
				Vector4 b2 = C<Vector4>(a);
				return Vector4.Distance(C<Vector4>(b), b2);
			}
			case MathType.Quaternion:
			{
				Quaternion quaternion = C<Quaternion>(a);
				Quaternion quaternion2 = C<Quaternion>(b);
				return Vector4.Distance(b: new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w), a: new Vector4(quaternion2.x, quaternion2.y, quaternion2.z, quaternion2.w));
			}
			case MathType.Color:
				return Vector4.Distance(C<Color>(a), C<Color>(b));
			default:
				return 0f;
			}
		}

		private V C<V>(object n)
		{
			return (V)n;
		}
	}
	public class Spline
	{
		private static double Polynom2(double a, double b, double c, double t)
		{
			double num = t * t;
			return a * num + b * t + c;
		}

		private static float Polynom2(float a, float b, float c, float t)
		{
			float num = t * t;
			return a * num + b * t + c;
		}

		private static Vector2 Polynom2(Vector2 a, Vector2 b, Vector2 c, float t)
		{
			float num = t * t;
			return a * num + b * t + c;
		}

		private static Vector3 Polynom2(Vector3 a, Vector3 b, Vector3 c, float t)
		{
			float num = t * t;
			return a * num + b * t + c;
		}

		private static Vector4 Polynom2(Vector4 a, Vector4 b, Vector4 c, float t)
		{
			float num = t * t;
			return a * num + b * t + c;
		}

		private static double PolynomDeriv2(double a, double b, double t)
		{
			return 2.0 * a * t + b;
		}

		private static float PolynomDeriv2(float a, float b, float t)
		{
			return 2f * a * t + b;
		}

		private static Vector2 PolynomDeriv2(Vector2 a, Vector2 b, float t)
		{
			return 2f * a * t + b;
		}

		private static Vector3 PolynomDeriv2(Vector3 a, Vector3 b, float t)
		{
			return 2f * a * t + b;
		}

		private static Vector4 PolynomDeriv2(Vector4 a, Vector4 b, float t)
		{
			return 2f * a * t + b;
		}

		private static double Polynom3(double a, double b, double c, double d, double t)
		{
			double num = t * t;
			double num2 = num * t;
			return a * num2 + b * num + c * t + d;
		}

		private static float Polynom3(float a, float b, float c, float d, float t)
		{
			float num = t * t;
			float num2 = num * t;
			return a * num2 + b * num + c * t + d;
		}

		private static Vector2 Polynom3(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
		{
			float num = t * t;
			float num2 = num * t;
			return a * num2 + b * num + c * t + d;
		}

		private static Vector3 Polynom3(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
		{
			float num = t * t;
			float num2 = num * t;
			return a * num2 + b * num + c * t + d;
		}

		private static Vector4 Polynom3(Vector4 a, Vector4 b, Vector4 c, Vector4 d, float t)
		{
			float num = t * t;
			float num2 = num * t;
			return a * num2 + b * num + c * t + d;
		}

		private static double PolynomDeriv3(double a, double b, double c, double t)
		{
			double num = t * t;
			return 3.0 * a * num + 2.0 * b * t + c;
		}

		private static float PolynomDeriv3(float a, float b, float c, float t)
		{
			float num = t * t;
			return 3f * a * num + 2f * b * t + c;
		}

		private static Vector2 PolynomDeriv3(Vector2 a, Vector2 b, Vector2 c, float t)
		{
			float num = t * t;
			return 3f * a * num + 2f * b * t + c;
		}

		private static Vector3 PolynomDeriv3(Vector3 a, Vector3 b, Vector3 c, float t)
		{
			float num = t * t;
			return 3f * a * num + 2f * b * t + c;
		}

		private static Vector4 PolynomDeriv3(Vector4 a, Vector4 b, Vector4 c, float t)
		{
			float num = t * t;
			return 3f * a * num + 2f * b * t + c;
		}

		public static float Linear(float v0, float v1, float r)
		{
			return v0 + (v1 - v0) * r;
		}

		public static Vector2 Linear(Vector2 v0, Vector2 v1, float r)
		{
			return v0 + (v1 - v0) * r;
		}

		public static Vector3 Linear(Vector3 v0, Vector3 v1, float r)
		{
			return v0 + (v1 - v0) * r;
		}

		public static Vector4 Linear(Vector4 v0, Vector4 v1, float r)
		{
			return v0 + (v1 - v0) * r;
		}

		public static Color Linear(Color v0, Color v1, float r)
		{
			return v0 + (v1 - v0) * r;
		}

		public static Quaternion Linear(Quaternion v0, Quaternion v1, float r)
		{
			return Quaternion.Slerp(v0, v1, r);
		}

		public static float Catmull(float v0, float v1, float v2, float v3, float r)
		{
			float num = r * r;
			float num2 = num * r;
			return 0.5f * (2f * v1 + (0f - v0 + v2) * r + (2f * v0 - 5f * v1 + 4f * v2 - v3) * num + (0f - v0 + 3f * v1 - 3f * v2 + v3) * num2);
		}

		public static Vector2 Catmull(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, float r)
		{
			float num = r * r;
			float num2 = num * r;
			return 0.5f * (2f * v1 + (-v0 + v2) * r + (2f * v0 - 5f * v1 + 4f * v2 - v3) * num + (-v0 + 3f * v1 - 3f * v2 + v3) * num2);
		}

		public static Vector3 Catmull(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float r)
		{
			float num = r * r;
			float num2 = num * r;
			return 0.5f * (2f * v1 + (-v0 + v2) * r + (2f * v0 - 5f * v1 + 4f * v2 - v3) * num + (-v0 + 3f * v1 - 3f * v2 + v3) * num2);
		}

		public static Vector4 Catmull(Vector4 v0, Vector4 v1, Vector4 v2, Vector4 v3, float r)
		{
			float num = r * r;
			float num2 = num * r;
			return 0.5f * (2f * v1 + (-v0 + v2) * r + (2f * v0 - 5f * v1 + 4f * v2 - v3) * num + (-v0 + 3f * v1 - 3f * v2 + v3) * num2);
		}

		public static Color Catmull(Color v0, Color v1, Color v2, Color v3, float r)
		{
			return Catmull((Vector4)v0, (Vector4)v1, (Vector4)v2, (Vector4)v3, r);
		}

		public static Quaternion Catmull(Quaternion v0, Quaternion v1, Quaternion v2, Quaternion v3, float r)
		{
			Quaternion result = default(Quaternion);
			result.x = Catmull(v0.x, v1.x, v2.x, v3.x, r);
			result.y = Catmull(v0.y, v1.y, v2.y, v3.y, r);
			result.z = Catmull(v0.z, v1.z, v2.z, v3.z, r);
			result.w = Catmull(v0.w, v1.w, v2.w, v3.w, r);
			return result;
		}

		public static Vector3 CatmullDeriv(float t, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float m)
		{
			return PolynomDeriv3(v0 * (0f - m) + v1 * (0f - m + 2f) + v2 * (m - 2f) + v3 * m, v0 * m * 2f + v1 * (m - 3f) + v2 * (-2f * m + 3f) + v3 * (0f - m), v0 * (0f - m) + v2 * m, t);
		}

		public static float Bezier3(float v0, float v1, float v2, float v3, float r)
		{
			float num = 1f - r;
			float num2 = num * num;
			float num3 = num2 * num;
			float num4 = r * r;
			float num5 = num4 * r;
			return num3 * v0 + 3f * num2 * r * v1 + 3f * num * num4 * v2 + num5 * v3;
		}

		public static Vector2 Bezier3(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, float r)
		{
			float num = 1f - r;
			float num2 = num * num;
			float num3 = num2 * num;
			float num4 = r * r;
			float num5 = num4 * r;
			return num3 * v0 + 3f * num2 * r * v1 + 3f * num * num4 * v2 + num5 * v3;
		}

		public static Vector3 Bezier3(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float r)
		{
			float num = 1f - r;
			float num2 = num * num;
			float num3 = num2 * num;
			float num4 = r * r;
			float num5 = num4 * r;
			return num3 * v0 + 3f * num2 * r * v1 + 3f * num * num4 * v2 + num5 * v3;
		}

		public static Vector4 Bezier3(Vector4 v0, Vector4 v1, Vector4 v2, Vector4 v3, float r)
		{
			float num = 1f - r;
			float num2 = num * num;
			float num3 = num2 * num;
			float num4 = r * r;
			float num5 = num4 * r;
			return num3 * v0 + 3f * num2 * r * v1 + 3f * num * num4 * v2 + num5 * v3;
		}

		public static Color Bezier3(Color v0, Color v1, Color v2, Color v3, float r)
		{
			return Bezier3((Vector4)v0, (Vector4)v1, (Vector4)v2, (Vector4)v3, r);
		}

		public static Quaternion Bezier3(Quaternion v0, Quaternion v1, Quaternion v2, Quaternion v3, float r)
		{
			Quaternion result = default(Quaternion);
			result.x = Bezier3(v0.x, v1.x, v2.x, v3.x, r);
			result.y = Bezier3(v0.y, v1.y, v2.y, v3.y, r);
			result.z = Bezier3(v0.z, v1.z, v2.z, v3.z, r);
			result.w = Bezier3(v0.w, v1.w, v2.w, v3.w, r);
			return result;
		}

		public static float Bezier2(float v0, float v1, float v2, float v3, float r)
		{
			float num = 1f - r;
			return num * num * v0 + 2f * num * r * v1 + r * r * v2;
		}

		public static Vector2 Bezier2(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, float r)
		{
			float num = 1f - r;
			return num * num * v0 + 2f * num * r * v1 + r * r * v2;
		}

		public static Vector3 Bezier2(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float r)
		{
			float num = 1f - r;
			return num * num * v0 + 2f * num * r * v1 + r * r * v2;
		}

		public static Vector4 Bezier2(Vector4 v0, Vector4 v1, Vector4 v2, Vector4 v3, float r)
		{
			float num = 1f - r;
			return num * num * v0 + 2f * num * r * v1 + r * r * v2;
		}

		public static Color Bezier2(Color v0, Color v1, Color v2, Color v3, float r)
		{
			return Bezier2((Vector4)v0, (Vector4)v1, (Vector4)v2, (Vector4)v3, r);
		}

		public static Quaternion Bezier2(Quaternion v0, Quaternion v1, Quaternion v2, Quaternion v3, float r)
		{
			Quaternion result = default(Quaternion);
			result.x = Bezier2(v0.x, v1.x, v2.x, v3.x, r);
			result.y = Bezier2(v0.y, v1.y, v2.y, v3.y, r);
			result.z = Bezier2(v0.z, v1.z, v2.z, v3.z, r);
			result.w = Bezier2(v0.w, v1.w, v2.w, v3.w, r);
			return result;
		}

		public static float Lerp(SplineType p_type, float r, params float[] v)
		{
			return p_type switch
			{
				SplineType.Catmull => Catmull(v[0], v[1], v[2], v[3], r), 
				SplineType.Bezier3 => Bezier3(v[0], v[1], v[2], v[3], r), 
				SplineType.Bezier2 => Bezier2(v[0], v[1], v[2], v[3], r), 
				_ => Linear(v[0], v[1], r), 
			};
		}

		public static Vector2 Lerp(SplineType p_type, float r, params Vector2[] v)
		{
			return p_type switch
			{
				SplineType.Catmull => Catmull(v[0], v[1], v[2], v[3], r), 
				SplineType.Bezier3 => Bezier3(v[0], v[1], v[2], v[3], r), 
				SplineType.Bezier2 => Bezier2(v[0], v[1], v[2], v[3], r), 
				_ => Linear(v[0], v[1], r), 
			};
		}

		public static Vector3 Lerp(SplineType p_type, float r, params Vector3[] v)
		{
			return p_type switch
			{
				SplineType.Catmull => Catmull(v[0], v[1], v[2], v[3], r), 
				SplineType.Bezier3 => Bezier3(v[0], v[1], v[2], v[3], r), 
				SplineType.Bezier2 => Bezier2(v[0], v[1], v[2], v[3], r), 
				_ => Linear(v[0], v[1], r), 
			};
		}

		public static Vector4 Lerp(SplineType p_type, float r, params Vector4[] v)
		{
			return p_type switch
			{
				SplineType.Catmull => Catmull(v[0], v[1], v[2], v[3], r), 
				SplineType.Bezier3 => Bezier3(v[0], v[1], v[2], v[3], r), 
				SplineType.Bezier2 => Bezier2(v[0], v[1], v[2], v[3], r), 
				_ => Linear(v[0], v[1], r), 
			};
		}

		public static Color Lerp(SplineType p_type, float r, params Color[] v)
		{
			return p_type switch
			{
				SplineType.Catmull => Catmull(v[0], v[1], v[2], v[3], r), 
				SplineType.Bezier3 => Bezier3(v[0], v[1], v[2], v[3], r), 
				SplineType.Bezier2 => Bezier2(v[0], v[1], v[2], v[3], r), 
				_ => Linear(v[0], v[1], r), 
			};
		}

		public static Quaternion Lerp(SplineType p_type, float r, params Quaternion[] v)
		{
			return p_type switch
			{
				SplineType.Catmull => Catmull(v[0], v[1], v[2], v[3], r), 
				SplineType.Bezier3 => Bezier3(v[0], v[1], v[2], v[3], r), 
				SplineType.Bezier2 => Bezier2(v[0], v[1], v[2], v[3], r), 
				_ => Linear(v[0], v[1], r), 
			};
		}

		public static T Lerp<T>(MathType p_data, SplineType p_type, float r, params T[] v)
		{
			T val = default(T);
			T val2 = ((v.Length != 0) ? v[0] : val);
			T val3 = ((v.Length > 1) ? v[1] : val);
			T val4 = ((v.Length > 2) ? v[2] : val);
			T val5 = ((v.Length > 3) ? v[3] : val);
			return p_data switch
			{
				MathType.Float => (T)(object)Lerp(p_type, r, (float)(object)val2, (float)(object)val3, (float)(object)val4, (float)(object)val5), 
				MathType.Vector2 => (T)(object)Lerp(p_type, r, (Vector2)(object)val2, (Vector2)(object)val3, (Vector2)(object)val4, (Vector2)(object)val5), 
				MathType.Vector3 => (T)(object)Lerp(p_type, r, (Vector3)(object)val2, (Vector3)(object)val3, (Vector3)(object)val4, (Vector3)(object)val5), 
				MathType.Vector4 => (T)(object)Lerp(p_type, r, (Vector4)(object)val2, (Vector4)(object)val3, (Vector4)(object)val4, (Vector4)(object)val5), 
				MathType.Color => (T)(object)Lerp(p_type, r, (Color)(object)val2, (Color)(object)val3, (Color)(object)val4, (Color)(object)val5), 
				MathType.Quaternion => (T)(object)Lerp(p_type, r, (Quaternion)(object)val2, (Quaternion)(object)val3, (Quaternion)(object)val4, (Quaternion)(object)val5), 
				_ => default(T), 
			};
		}

		public static Vector3 LerpDeriv(SplineType p_type, float r, params Vector3[] v)
		{
			if (p_type == SplineType.Catmull)
			{
				return CatmullDeriv(r, v[0], v[1], v[2], v[3], 0.5f);
			}
			return Linear(v[0], v[1], r);
		}

		public static T LerpDeriv<T>(MathType p_data, SplineType p_type, float r, params T[] v)
		{
			T val = default(T);
			T val2 = ((v.Length != 0) ? v[0] : val);
			T val3 = ((v.Length > 1) ? v[1] : val);
			T val4 = ((v.Length > 2) ? v[2] : val);
			T val5 = ((v.Length > 3) ? v[3] : val);
			if (p_data == MathType.Vector3)
			{
				return (T)(object)LerpDeriv(p_type, r, (Vector3)(object)val2, (Vector3)(object)val3, (Vector3)(object)val4, (Vector3)(object)val5);
			}
			return default(T);
		}
	}
}
