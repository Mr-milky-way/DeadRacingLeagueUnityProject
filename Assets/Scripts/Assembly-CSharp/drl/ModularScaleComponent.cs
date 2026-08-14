using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl
{
	[ExecuteInEditMode]
	public class ModularScaleComponent : MonoBehaviour
	{
		[SerializeField]
		private Vector3 m_scale = Vector3.one;

		public Vector3 min = Vector3.one;

		public Vector3 max = Vector3.one;

		public string swizzle = "xyz";

		public ModularScaleVariant current;

		public GameObject baseAsset;

		public List<ModularScaleVariant> variants;

		public Action<ModularScaleVariant, ModularScaleVariant> OnVariantChange;

		private Transform m_transform;

		public Vector3 scale
		{
			get
			{
				return m_scale;
			}
			set
			{
				if (IsDifferent(value, m_scale))
				{
					Set(value);
				}
			}
		}

		public new Transform transform
		{
			get
			{
				if (!m_transform)
				{
					return m_transform = base.transform;
				}
				return m_transform;
			}
		}

		public ModularScaleVariant Set(Vector3 p_scale)
		{
			m_scale = GetVectorSizzle(p_scale);
			Vector3 p_scale2 = m_scale;
			p_scale2.x = Mathf.Clamp(p_scale2.x, min.x, max.x);
			p_scale2.y = Mathf.Clamp(p_scale2.y, min.y, max.y);
			p_scale2.z = Mathf.Clamp(p_scale2.z, min.z, max.z);
			Vector3 vector = p_scale;
			vector.x = Mathf.Clamp(vector.x, min.x, max.x);
			vector.y = Mathf.Clamp(vector.y, min.y, max.y);
			vector.z = Mathf.Clamp(vector.z, min.z, max.z);
			m_scale = p_scale2;
			ModularScaleVariant modularScaleVariant = Get(p_scale2);
			if (modularScaleVariant != current)
			{
				ModularScaleVariant modularScaleVariant2 = null;
				if ((bool)current)
				{
					modularScaleVariant2 = current;
				}
				if ((bool)modularScaleVariant)
				{
					modularScaleVariant = UnityEngine.Object.Instantiate(modularScaleVariant, transform);
					modularScaleVariant.name = modularScaleVariant.name.Replace("(Clone)", "$variant");
					modularScaleVariant.transform.localPosition = Vector3.zero;
					modularScaleVariant.transform.localEulerAngles = Vector3.zero;
				}
				if (OnVariantChange != null)
				{
					OnVariantChange(current, modularScaleVariant);
				}
				current = modularScaleVariant;
				if ((bool)modularScaleVariant2)
				{
					UnityEngine.Object.Destroy(modularScaleVariant2.gameObject);
				}
				modularScaleVariant2 = null;
			}
			if ((bool)baseAsset)
			{
				baseAsset.SetActive(current == null);
			}
			return current;
		}

		public Vector3 GetVectorSizzle(Vector3 p_vector)
		{
			Vector3 vector = m_scale;
			Vector3 vector2 = vector - p_vector;
			List<Vector3> swizzleVector = GetSwizzleVector(vector2, swizzle.ToLower().Split(','));
			Vector3 vector3 = ((swizzleVector.Count <= 0) ? vector2 : swizzleVector[0]);
			float num = vector3.magnitude;
			for (int i = 1; i < swizzleVector.Count; i++)
			{
				Vector3 vector4 = swizzleVector[i];
				float magnitude = vector4.magnitude;
				if (!(magnitude <= num))
				{
					vector3 = vector4;
					num = magnitude;
				}
			}
			vector -= vector3;
			Vector3 result = vector;
			result.x = Mathf.Clamp(result.x, min.x, max.x);
			result.y = Mathf.Clamp(result.y, min.y, max.y);
			result.z = Mathf.Clamp(result.z, min.z, max.z);
			return result;
		}

		public ModularScaleVariant Get(Vector3 p_scale)
		{
			List<ModularScaleVariant> list = ((variants == null) ? new List<ModularScaleVariant>() : variants);
			variants = list;
			Vector3 vector = Abs(current ? (p_scale - current.scale) : (Vector3.one * 999f));
			float num = vector.x + vector.y + vector.z;
			ModularScaleVariant result = current;
			for (int i = 0; i < variants.Count; i++)
			{
				ModularScaleVariant modularScaleVariant = variants[i];
				if ((bool)modularScaleVariant && !(modularScaleVariant == current))
				{
					Vector3 vector2 = modularScaleVariant.scale;
					Vector3 vector3 = Abs(p_scale - vector2);
					float num2 = vector3.x + vector3.y + vector3.z;
					if (num2 < num)
					{
						num = num2;
						result = modularScaleVariant;
					}
				}
			}
			return result;
		}

		public void ResetScaleLimits()
		{
			List<ModularScaleVariant> list = (variants = ((variants == null) ? new List<ModularScaleVariant>() : variants));
			if (list.Count > 0)
			{
				min = list[0].scale;
				max = list[0].scale;
				for (int i = 1; i < list.Count; i++)
				{
					min.x = Mathf.Min(list[i].scale.x, min.x);
					min.y = Mathf.Min(list[i].scale.y, min.y);
					min.z = Mathf.Min(list[i].scale.z, min.z);
					max.x = Mathf.Max(list[i].scale.x, max.x);
					max.y = Mathf.Max(list[i].scale.y, max.y);
					max.z = Mathf.Max(list[i].scale.z, max.z);
				}
				Set(m_scale);
			}
		}

		protected void Awake()
		{
			if (base.enabled)
			{
				Set(m_scale);
			}
		}

		protected void LateUpdate()
		{
			if (transform.hasChanged)
			{
				Vector3 vector = transform.localScale - Vector3.one;
				if (IsDifferent(Vector3.zero, vector, 0.01f))
				{
					Vector3 p_scale = m_scale + vector;
					Set(p_scale);
					transform.localScale = Vector3.one;
				}
				transform.hasChanged = false;
			}
		}

		private bool IsDifferent(Vector3 a, Vector3 b, float p_bias = 0.0001f)
		{
			if (Mathf.Abs(a.x - b.x) >= p_bias)
			{
				return true;
			}
			if (Mathf.Abs(a.y - b.y) >= p_bias)
			{
				return true;
			}
			if (Mathf.Abs(a.z - b.z) >= p_bias)
			{
				return true;
			}
			return false;
		}

		private Vector3 Abs(Vector3 a)
		{
			a.x = Mathf.Abs(a.x);
			a.y = Mathf.Abs(a.y);
			a.z = Mathf.Abs(a.z);
			return a;
		}

		private List<Vector3> GetSwizzleVector(Vector3 v, string[] sl)
		{
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < sl.Length; i++)
			{
				list.Add(GetSwizzleVector(v, sl[i]));
			}
			return list;
		}

		private Vector3 GetSwizzleVector(Vector3 v, string s)
		{
			string text = s.ToLower().Trim();
			if (string.IsNullOrEmpty(text))
			{
				return v;
			}
			float[] array = new float[3] { v.x, v.y, v.z };
			List<char> list = new List<char> { 'x', 'y', 'z' };
			int num = ((text.Length >= 1) ? list.IndexOf(text[0]) : 0);
			int num2 = ((text.Length < 2) ? 1 : list.IndexOf(text[1]));
			int num3 = ((text.Length >= 3) ? list.IndexOf(text[2]) : 2);
			v.x = array[num];
			v.y = array[num2];
			v.z = array[num3];
			return v;
		}
	}
}
