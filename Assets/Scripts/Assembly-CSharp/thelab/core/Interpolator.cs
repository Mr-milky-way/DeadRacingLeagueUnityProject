using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class Interpolator<T>
	{
		public class Sample
		{
			public T value;

			public float time;

			public Sample()
			{
			}

			public Sample(T v, float t)
			{
				value = v;
				time = t;
			}

			public void Set(T v, float t)
			{
				value = v;
				time = t;
			}
		}

		public class Estimation
		{
			public float delay;

			public float predictionScale;

			public List<Sample> buffer;

			public List<T> smoothed;

			public List<float> weights;

			public float deviationExp;

			public float speedBlendDuration;

			public float maxDeviation;

			public float deviation;

			internal T smoothVelocity;

			internal List<Sample> pool;

			public Interpolator<T> interpolator { get; internal set; }

			public int samples { get; internal set; }

			public float smoothExp { get; internal set; }

			public Estimation()
			{
				delay = 0f;
				smoothVelocity = default(T);
				speedBlendDuration = 0.016f;
				maxDeviation = 1f;
				deviationExp = 6f;
				predictionScale = 1.5f;
				buffer = new List<Sample>();
				pool = new List<Sample>();
				smoothed = new List<T>();
				SetSampling(25, 0.5f);
			}

			public void SetSampling(int p_count, float p_exponent)
			{
				weights = new List<float>();
				samples = p_count;
				smoothExp = p_exponent;
				for (int i = 0; i < samples; i++)
				{
					float f = (float)i / (float)(samples - 1);
					f = Mathf.Clamp01(Mathf.Pow(f, smoothExp));
					weights.Add(f);
				}
			}

			public void SetSampling(float p_exponent)
			{
				SetSampling(samples, p_exponent);
			}

			public T Evaluate(T p_value, float p_time)
			{
				int count = pool.Count;
				Sample sample = ((count > 0) ? pool[0] : new Sample());
				if (count > 0)
				{
					pool.RemoveAt(0);
				}
				sample.Set(p_value, p_time);
				float num = ((interpolator.deltatime <= 0f) ? Time.unscaledDeltaTime : interpolator.deltatime);
				buffer.Add(sample);
				int count2 = weights.Count;
				while (buffer.Count > count2)
				{
					Sample item = buffer[0];
					buffer.RemoveAt(0);
					pool.Add(item);
				}
				int count3 = buffer.Count;
				Sample sample2 = Tail(0);
				Sample sample3 = Tail(1);
				T val = ((sample2 == null) ? p_value : sample2.value);
				T b = ((sample3 == null) ? val : sample3.value);
				interpolator.Distance(val, b);
				float num2 = sample2?.time ?? 0f;
				float num3 = sample3?.time ?? num2;
				float num4 = Mathf.Max(0f, num2 - num3);
				float n = ((num4 <= 0f) ? 0f : (1f / num4));
				float num5 = Mathf.Round(Mathf.Max(1f, delay) / 10f) * 10f * 0.001f;
				float num6 = speedBlendDuration;
				num6 = ((num6 <= 0f) ? 1f : (num6 * num));
				T val2 = interpolator.Zero();
				float num7 = 0f;
				float num8 = 0f;
				List<float> list = weights;
				int num9 = 0;
				T val3 = val;
				T v = interpolator.Sub(val3, b);
				v = interpolator.Mul(v, n);
				smoothVelocity = interpolator.Lerp(smoothVelocity, v, 0.01f);
				float p_step = num5 * predictionScale;
				T val4 = interpolator.Move(val3, smoothVelocity, p_step);
				smoothed.Add(val4);
				val2 = interpolator.Zero();
				num7 = 0f;
				num9 = Mathf.Min(list.Count, buffer.Count);
				for (int i = 0; i < num9; i++)
				{
					int num10 = smoothed.Count - 1;
					int num11 = list.Count - 1;
					int index = num10 - i;
					int index2 = num11 - i;
					T b2 = interpolator.Mul(smoothed[index], list[index2]);
					val2 = interpolator.Add(val2, b2);
					num7 += list[index2];
				}
				num8 = ((num7 <= 0f) ? 0f : (1f / num7));
				val2 = ((num7 <= 0f) ? val4 : interpolator.Mul(val2, num8));
				deviation = interpolator.Distance(val3, val2);
				float f = ((maxDeviation <= 0f) ? 1f : Mathf.Clamp01(deviation / maxDeviation));
				if (count3 < count2)
				{
					return val4;
				}
				return interpolator.Lerp(val2, val4, Mathf.Pow(f, deviationExp));
			}

			public Sample Tail(int p_offset)
			{
				int count = buffer.Count;
				if (count <= 0)
				{
					return null;
				}
				int index = Mathf.Clamp(count - 1 - p_offset, 0, count - 1);
				return buffer[index];
			}

			public void Clear()
			{
				buffer.Clear();
				smoothed.Clear();
				smoothVelocity = default(T);
			}
		}

		public float deltatime;

		public T current;

		public T next;

		public MathType data { get; private set; }

		public InterpolationType type { get; set; }

		public Estimation estimate { get; internal set; }

		public Interpolator(InterpolationType p_type)
		{
			current = Zero();
			next = Zero();
			deltatime = 0f;
			type = p_type;
			estimate = new Estimation();
			estimate.interpolator = this;
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
			else if (typeof(T) == typeof(Vector4))
			{
				data = MathType.Vector4;
			}
			else if (typeof(T) == typeof(Quaternion))
			{
				data = MathType.Quaternion;
			}
			else if (typeof(T) == typeof(Transform))
			{
				Debug.LogWarning("Interpolator> Type [" + typeof(T).Name + "] not supported!");
			}
			else if (typeof(T) == typeof(Rect))
			{
				Debug.LogWarning("Interpolator> Type [" + typeof(T).Name + "] not supported!");
			}
			else if (typeof(T) == typeof(int))
			{
				Debug.LogWarning("Interpolator> Type [" + typeof(T).Name + "] not supported!");
			}
		}

		public Interpolator()
			: this(InterpolationType.Lerp)
		{
		}

		public void Clear()
		{
			next = Zero();
			current = Zero();
			estimate.Clear();
		}

		public T Evaluate(T p_value, float p_time)
		{
			next = p_value;
			float num = ((deltatime <= 0f) ? Time.unscaledDeltaTime : deltatime);
			switch (type)
			{
			case InterpolationType.None:
				current = next;
				break;
			case InterpolationType.Lerp:
				current = Lerp(current, p_value, (p_time <= 0f) ? 1f : (num / p_time));
				break;
			case InterpolationType.Predictive:
				current = estimate.Evaluate(next, p_time);
				break;
			case InterpolationType.PID:
				current = next;
				break;
			}
			return current;
		}

		public T Evaluate(T p_value)
		{
			return Evaluate(p_value, 0f);
		}

		protected V C<V>(object n)
		{
			return (V)n;
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
			default:
				return 0f;
			}
		}

		protected virtual T Move(T p_value, T p_vector, float p_step)
		{
			T result = default(T);
			switch (data)
			{
			case MathType.Float:
			{
				float num = C<float>(p_value);
				float num2 = C<float>(p_vector);
				return C<T>(num + num2 * p_step);
			}
			case MathType.Vector2:
			{
				Vector2 vector7 = C<Vector2>(p_value);
				Vector2 vector8 = C<Vector2>(p_vector);
				return C<T>(vector7 + vector8 * p_step);
			}
			case MathType.Vector3:
			{
				Vector3 vector5 = C<Vector3>(p_value);
				Vector3 vector6 = C<Vector3>(p_vector);
				return C<T>(vector5 + vector6 * p_step);
			}
			case MathType.Vector4:
			{
				Vector4 vector3 = C<Vector4>(p_value);
				Vector4 vector4 = C<Vector4>(p_vector);
				return C<T>(vector3 + vector4 * p_step);
			}
			case MathType.Quaternion:
			{
				Quaternion quaternion = C<Quaternion>(p_value);
				Quaternion quaternion2 = C<Quaternion>(p_vector);
				Vector4 vector = new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
				Vector4 vector2 = new Vector4(quaternion2.x, quaternion2.y, quaternion2.z, quaternion2.w);
				vector += vector2 * p_step;
				vector.Normalize();
				quaternion.Set(vector.x, vector.y, vector.z, vector.w);
				return C<T>(quaternion);
			}
			default:
				return result;
			}
		}

		protected virtual T Lerp(T a, T b, float r)
		{
			return Spline.Lerp<T>(data, SplineType.Linear, r, a, b);
		}

		protected virtual T Mul(T v, float n)
		{
			T result = Zero();
			switch (data)
			{
			case MathType.Float:
			{
				float num = C<float>(v);
				result = C<T>(num * n);
				break;
			}
			case MathType.Vector2:
			{
				Vector2 vector3 = C<Vector2>(v);
				result = C<T>(vector3 * n);
				break;
			}
			case MathType.Vector3:
			{
				Vector3 vector2 = C<Vector3>(v);
				result = C<T>(vector2 * n);
				break;
			}
			case MathType.Vector4:
			{
				Vector4 vector = C<Vector4>(v);
				result = C<T>(vector * n);
				break;
			}
			case MathType.Quaternion:
			{
				Quaternion quaternion = C<Quaternion>(v);
				quaternion.Set(quaternion.x * n, quaternion.y * n, quaternion.z * n, quaternion.w * n);
				result = C<T>(quaternion);
				break;
			}
			}
			return result;
		}

		protected virtual T Add(T a, T b)
		{
			T result = Zero();
			switch (data)
			{
			case MathType.Float:
			{
				float num = C<float>(a);
				float num2 = C<float>(b);
				result = C<T>(num + num2);
				break;
			}
			case MathType.Vector2:
			{
				Vector2 vector5 = C<Vector2>(a);
				Vector2 vector6 = C<Vector2>(b);
				result = C<T>(vector5 + vector6);
				break;
			}
			case MathType.Vector3:
			{
				Vector3 vector3 = C<Vector3>(a);
				Vector3 vector4 = C<Vector3>(b);
				result = C<T>(vector3 + vector4);
				break;
			}
			case MathType.Vector4:
			{
				Vector4 vector = C<Vector4>(a);
				Vector4 vector2 = C<Vector4>(b);
				result = C<T>(vector + vector2);
				break;
			}
			case MathType.Quaternion:
			{
				Quaternion quaternion = C<Quaternion>(a);
				Quaternion quaternion2 = C<Quaternion>(b);
				quaternion.Set(quaternion.x + quaternion2.x, quaternion.y + quaternion2.y, quaternion.z + quaternion2.z, quaternion.w + quaternion2.w);
				result = C<T>(quaternion);
				break;
			}
			}
			return result;
		}

		protected virtual T Sub(T a, T b)
		{
			T result = Zero();
			switch (data)
			{
			case MathType.Float:
			{
				float num = C<float>(a);
				float num2 = C<float>(b);
				result = C<T>(num - num2);
				break;
			}
			case MathType.Vector2:
			{
				Vector2 vector5 = C<Vector2>(a);
				Vector2 vector6 = C<Vector2>(b);
				result = C<T>(vector5 - vector6);
				break;
			}
			case MathType.Vector3:
			{
				Vector3 vector3 = C<Vector3>(a);
				Vector3 vector4 = C<Vector3>(b);
				result = C<T>(vector3 - vector4);
				break;
			}
			case MathType.Vector4:
			{
				Vector4 vector = C<Vector4>(a);
				Vector4 vector2 = C<Vector4>(b);
				result = C<T>(vector - vector2);
				break;
			}
			case MathType.Quaternion:
			{
				Quaternion quaternion = C<Quaternion>(a);
				Quaternion quaternion2 = C<Quaternion>(b);
				quaternion.Set(quaternion.x - quaternion2.x, quaternion.y - quaternion2.y, quaternion.z - quaternion2.z, quaternion.w - quaternion2.w);
				result = C<T>(quaternion);
				break;
			}
			}
			return result;
		}

		protected virtual T Zero()
		{
			T val = default(T);
			return data switch
			{
				MathType.Float => C<T>(0f), 
				MathType.Vector2 => C<T>(Vector2.zero), 
				MathType.Vector3 => C<T>(Vector3.zero), 
				MathType.Vector4 => C<T>(Vector4.zero), 
				MathType.Quaternion => C<T>(new Quaternion(0f, 0f, 0f, 0f)), 
				_ => val, 
			};
		}
	}
}
