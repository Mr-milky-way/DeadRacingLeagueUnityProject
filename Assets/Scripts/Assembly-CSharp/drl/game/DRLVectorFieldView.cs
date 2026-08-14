using System.Collections.Generic;
using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class DRLVectorFieldView : NotificationView<DRLApp>
	{
		public List<DRLNumberFieldView> fields;

		protected void Awake()
		{
			for (int i = 0; i < fields.Count; i++)
			{
				DRLNumberFieldView dRLNumberFieldView = fields[i];
				if ((bool)dRLNumberFieldView)
				{
					dRLNumberFieldView.OnEvent.AddListener(OnNotification);
				}
			}
		}

		public T Get<T>()
		{
			float[] array = new float[fields.Count];
			for (int i = 0; i < array.Length; i++)
			{
				if ((bool)fields[i])
				{
					array[i] = fields[i].value;
				}
			}
			if (typeof(T) == typeof(float[]))
			{
				return (T)(object)array;
			}
			if (typeof(T) == typeof(int[]))
			{
				int[] array2 = new int[array.Length];
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j] = (int)array[j];
				}
				return (T)(object)array2;
			}
			float num = ((array.Length == 0) ? 0f : array[0]);
			float num2 = ((array.Length <= 1) ? 0f : array[1]);
			float num3 = ((array.Length <= 2) ? 0f : array[2]);
			float num4 = ((array.Length <= 3) ? 0f : array[3]);
			if (typeof(T) == typeof(Vector2))
			{
				return (T)(object)new Vector2(num, num2);
			}
			if (typeof(T) == typeof(Vector3))
			{
				return (T)(object)new Vector3(num, num2, num3);
			}
			if (typeof(T) == typeof(Vector4))
			{
				return (T)(object)new Vector4(num, num2, num3, num4);
			}
			if (typeof(T) == typeof(Quaternion))
			{
				return (T)(object)new Quaternion(num, num2, num3, num4);
			}
			if (typeof(T) == typeof(Color))
			{
				return (T)(object)new Color(num, num2, num3, num4);
			}
			return default(T);
		}

		public void Set(Vector2 v)
		{
			Set(new float[2] { v.x, v.y });
		}

		public void Set(Vector3 v)
		{
			Set(new float[3] { v.x, v.y, v.z });
		}

		public void Set(Vector4 v)
		{
			Set(new float[4] { v.x, v.y, v.z, v.w });
		}

		public void Set(Quaternion v)
		{
			Set(new float[4] { v.x, v.y, v.z, v.w });
		}

		public void Set(Color v)
		{
			Set(new float[4] { v.r, v.g, v.b, v.a });
		}

		public void Set(int[] v)
		{
			if (v != null)
			{
				float[] array = new float[v.Length];
				for (int i = 0; i < v.Length; i++)
				{
					array[i] = v[i];
				}
				Set(array);
			}
		}

		public void Set(float[] v)
		{
			if (v == null)
			{
				return;
			}
			int num = Mathf.Min(v.Length, fields.Count);
			for (int i = 0; i < num; i++)
			{
				if ((bool)fields[i])
				{
					fields[i].value = v[i];
				}
			}
		}

		public void SetPrecision(float[] p_precision)
		{
			int num = Mathf.Min(p_precision.Length, fields.Count);
			for (int i = 0; i < num; i++)
			{
				fields[i].precision = p_precision[i];
			}
		}

		public void SetPrecision(Vector4 p_precision)
		{
			SetPrecision(new float[4] { p_precision.x, p_precision.y, p_precision.z, p_precision.w });
		}

		public void SetPrecision(Vector3 p_precision)
		{
			SetPrecision(new float[3] { p_precision.x, p_precision.y, p_precision.z });
		}

		public void SetPrecision(Vector2 p_precision)
		{
			SetPrecision(new float[2] { p_precision.x, p_precision.y });
		}

		public void SetPrecision(float p_precision)
		{
			float[] array = new float[fields.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = p_precision;
			}
			SetPrecision(array);
		}

		public void SetSnap(float[] p_snap)
		{
			int num = Mathf.Min(p_snap.Length, fields.Count);
			for (int i = 0; i < num; i++)
			{
				fields[i].snap = p_snap[i];
			}
		}

		public void SetSnap(Vector4 p_snap)
		{
			SetSnap(new float[4] { p_snap.x, p_snap.y, p_snap.z, p_snap.w });
		}

		public void SetSnap(Vector3 p_snap)
		{
			SetSnap(new float[3] { p_snap.x, p_snap.y, p_snap.z });
		}

		public void SetSnap(Vector2 p_snap)
		{
			SetSnap(new float[2] { p_snap.x, p_snap.y });
		}

		public void SetSnap(float p_snap)
		{
			float[] array = new float[fields.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = p_snap;
			}
			SetSnap(array);
		}

		public void SetMinValue(float[] p_v)
		{
			int num = Mathf.Min(p_v.Length, fields.Count);
			for (int i = 0; i < num; i++)
			{
				fields[i].minValue = p_v[i];
			}
		}

		public void SetMinValue(Vector4 p_v)
		{
			SetMinValue(new float[4] { p_v.x, p_v.y, p_v.z, p_v.w });
		}

		public void SetMinValue(Vector3 p_v)
		{
			SetMinValue(new float[3] { p_v.x, p_v.y, p_v.z });
		}

		public void SetMinValue(Vector2 p_v)
		{
			SetMinValue(new float[2] { p_v.x, p_v.y });
		}

		public void SetMaxValue(float[] p_v)
		{
			int num = Mathf.Min(p_v.Length, fields.Count);
			for (int i = 0; i < num; i++)
			{
				fields[i].maxValue = p_v[i];
			}
		}

		public void SetMaxValue(Vector4 p_v)
		{
			SetMaxValue(new float[4] { p_v.x, p_v.y, p_v.z, p_v.w });
		}

		public void SetMaxValue(Vector3 p_v)
		{
			SetMaxValue(new float[3] { p_v.x, p_v.y, p_v.z });
		}

		public void SetMaxValue(Vector2 p_v)
		{
			SetMaxValue(new float[2] { p_v.x, p_v.y });
		}

		public void Invalidate()
		{
			for (int i = 0; i < fields.Count; i++)
			{
				if ((bool)fields[i])
				{
					fields[i].Invalidate();
				}
			}
		}

		protected void OnNotification(NotificationEvent p_event)
		{
			if (base.enabled)
			{
				if (p_event.notification.Contains("@change"))
				{
					Notify(notification + "@change");
				}
				if (p_event.notification.Contains("@end-edit"))
				{
					Notify(notification + "@end-edit");
				}
			}
		}
	}
}
