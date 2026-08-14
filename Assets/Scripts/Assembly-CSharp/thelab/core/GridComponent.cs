using UnityEngine;

namespace thelab.core
{
	public class GridComponent : MonoBehaviour
	{
		public GridRenderer grid;

		[SerializeField]
		private Color m_color = Color.white;

		[SerializeField]
		private float m_size = 2f;

		[SerializeField]
		private Vector4 m_interval = new Vector4(3f, 1f, 10f, 30f);

		public bool lastLODAlwaysVisible = true;

		public Color color
		{
			get
			{
				return m_color;
			}
			set
			{
				m_color = value;
				RefreshColor();
			}
		}

		public float alpha
		{
			get
			{
				return color.a;
			}
			set
			{
				Color color = this.color;
				color.a = value;
				this.color = color;
			}
		}

		public float size
		{
			get
			{
				return m_size;
			}
			set
			{
				if (!(Mathf.Abs(m_size - value) <= 1E-05f))
				{
					m_size = value;
					RefreshSize();
				}
			}
		}

		public Vector4 interval
		{
			get
			{
				return m_interval;
			}
			set
			{
				m_interval = value;
				RefreshInterval();
			}
		}

		protected void Start()
		{
			RefreshColor();
			RefreshSize();
			RefreshInterval();
		}

		public void FadeSize(float p_size, float p_duration, float p_delay = 0f)
		{
			Tween.Kill(this, "size");
			Tween.Add(this, "size", p_size, p_duration, p_delay, Cubic.Out);
		}

		public void Fade(float p_alpha, float p_duration, float p_delay = 0f)
		{
			Tween.Kill(this, "alpha");
			Tween.Add(this, "alpha", p_alpha, p_duration, p_delay, Cubic.Out);
		}

		protected void RefreshColor()
		{
			if ((bool)grid && (bool)grid.renderer)
			{
				Material[] sharedMaterials = grid.renderer.sharedMaterials;
				Color value = color;
				for (int i = 0; i < sharedMaterials.Length; i++)
				{
					sharedMaterials[i].SetColor("_Color", value);
				}
				grid.renderer.sharedMaterials = sharedMaterials;
			}
		}

		protected void RefreshSize()
		{
			if ((bool)grid && (bool)grid.renderer)
			{
				float num = Mathf.Max(2f, size);
				float num2 = 1f / num / num;
				Material[] sharedMaterials = grid.renderer.sharedMaterials;
				Vector4 vector = default(Vector4);
				vector = base.transform.localScale;
				foreach (Material obj in sharedMaterials)
				{
					obj.SetFloat("_Size", num2);
					obj.SetVector("_Scale", vector);
					num2 *= num;
				}
				grid.renderer.sharedMaterials = sharedMaterials;
			}
		}

		protected void RefreshInterval()
		{
			if (!grid || !grid.renderer)
			{
				return;
			}
			Vector4 vector = m_interval;
			float num = vector.z;
			float num2 = 0f - vector.x * 0.5f;
			Material[] sharedMaterials = grid.renderer.sharedMaterials;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				Material obj = sharedMaterials[i];
				float num3 = num2;
				float value = num3 + vector.x;
				float value2 = 1f;
				if ((i >= sharedMaterials.Length - 1 && lastLODAlwaysVisible) || num > 1000f)
				{
					num = 200f;
					value = 200f;
					value2 = 100f;
				}
				obj.SetFloat("_FadeRadius", num);
				obj.SetFloat("_FadeScale", value2);
				obj.SetFloat("_FadeMinDistance", num3);
				obj.SetFloat("_FadeMaxDistance", value);
				num2 += vector.y;
				num += vector.w;
			}
			grid.renderer.sharedMaterials = sharedMaterials;
		}
	}
}
