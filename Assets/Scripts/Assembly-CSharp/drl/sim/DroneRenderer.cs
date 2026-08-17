using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using thelab.core;

namespace drl.sim
{
	public class DroneRenderer : MonoBehaviour
	{
		[SerializeField]
		protected List<Renderer> m_renderers;

		[SerializeField]
		[HideInInspector]
		private Color m_color;

		[SerializeField]
		[HideInInspector]
		private Color m_color_0;

		[SerializeField]
		[HideInInspector]
		private Color m_color_1;

		[SerializeField]
		[HideInInspector]
		private Color m_color_2;

		[SerializeField]
		[HideInInspector]
		private Color m_emissive;

		[SerializeField]
		private Light m_light;

		public List<DroneTrail> trails;

		[SerializeField]
		protected List<Renderer> m_props;

		public Material[] _skin;

		private float m_trail_scale;

		private float m_trail_duration;

		private float m_trail_width_multiplier = 1f;

		public Action<bool> VisibilityChanged;

		private Dictionary<Renderer, bool> m_visibility = new Dictionary<Renderer, bool>();

		private Dictionary<Renderer, ShadowCastingMode> m_shadowing = new Dictionary<Renderer, ShadowCastingMode>();

		protected bool m_visible = true;

		protected bool m_shadowsOnly;

		public List<Renderer> renderers
		{
			get
			{
				if (m_renderers != null)
				{
					return m_renderers;
				}
				return m_renderers = new List<Renderer>();
			}
		}

		public Color color
		{
			get
			{
				return m_color;
			}
			set
			{
				m_color = value;
				SetColor("_Color", value);
			}
		}

		public Color color0
		{
			get
			{
				return m_color_0;
			}
			set
			{
				m_color_0 = value;
				SetColor("_Color1", value);
				emissive = m_color_0 * 1.5f;
			}
		}

		public Color color1
		{
			get
			{
				return m_color_1;
			}
			set
			{
				m_color_1 = value;
				SetColor("_Color2", value);
			}
		}

		public Color color2
		{
			get
			{
				return m_color_2;
			}
			set
			{
				m_color_2 = value;
				SetColor("_Color3", value);
			}
		}

		public Color emissive
		{
			get
			{
				return m_emissive;
			}
			set
			{
				m_emissive = value;
				SetColor("_ColorEmission", value);
				if ((bool)light)
				{
					light.color = value;
				}
			}
		}

		public Color trailsColor
		{
			get
			{
				return GetTrailColor("_Color")[0];
			}
			set
			{
				SetTrailColor("_Color", value);
			}
		}

		public Color playerColor
		{
			get
			{
				return trailsColor;
			}
			set
			{
				trailsColor = value;
			}
		}

		public Light light
		{
			get
			{
				if (!m_light)
				{
					return m_light = (base.gameObject ? Hierarchy.Find<Light>(base.transform) : null);
				}
				return m_light;
			}
		}

		public List<Renderer> props
		{
			get
			{
				if (m_props != null)
				{
					return m_props;
				}
				return m_props = new List<Renderer>();
			}
		}

		internal Texture _skinAlbedo
		{
			get
			{
				if (_skin == null)
				{
					return null;
				}
				if (_skin.Length < 1)
				{
					return null;
				}
				return _skin[0].GetTexture("_SkinAlbedoTex");
			}
			set
			{
				for (int i = 0; i < _skin.Length; i++)
				{
					_skin[i].SetTexture("_SkinAlbedoTex", value);
				}
			}
		}

		internal Texture _skinMask
		{
			get
			{
				if (_skin == null)
				{
					return null;
				}
				if (_skin.Length < 1)
				{
					return null;
				}
				return _skin[0].GetTexture("_SkinMaskTex");
			}
			set
			{
				for (int i = 0; i < _skin.Length; i++)
				{
					_skin[i].SetTexture("_SkinMaskTex", value);
				}
			}
		}

		public float trailScale
		{
			get
			{
				return m_trail_scale;
			}
			set
			{
				m_trail_scale = value;
				ApplyTrailDuration();
			}
		}

		public float trailDuration
		{
			get
			{
				return m_trail_duration;
			}
			set
			{
				m_trail_duration = value;
				ApplyTrailDuration();
			}
		}

		public float trailWidthMultiplier
		{
			get
			{
				return m_trail_width_multiplier;
			}
			set
			{
				m_trail_width_multiplier = value;
				ApplyTrailWidth();
			}
		}

		public bool visible
		{
			get
			{
				return m_visible;
			}
			set
			{
				m_visible = value;
				if (!value)
				{
					foreach (Renderer renderer in renderers)
					{
						if (renderer != null)
						{
							renderer.enabled = false;
						}
					}
					return;
				}
				if (m_visibility.Count == renderers.Count)
				{
					foreach (Renderer renderer2 in renderers)
					{
						if (renderer2 != null)
						{
							if (m_visibility.ContainsKey(renderer2))
							{
								renderer2.enabled = m_visibility[renderer2];
							}
							else
							{
								renderer2.enabled = true;
							}
						}
					}
					return;
				}
				foreach (Renderer renderer3 in renderers)
				{
					if (renderer3 != null)
					{
						renderer3.enabled = true;
					}
				}
			}
		}

		public bool shadowsOnly
		{
			get
			{
				return m_shadowsOnly;
			}
			set
			{
				m_shadowsOnly = value;
				if (m_shadowsOnly)
				{
					foreach (Renderer renderer in renderers)
					{
						if (renderer != null)
						{
							renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
						}
					}
				}
				else if (m_visibility.Count == renderers.Count)
				{
					foreach (Renderer renderer2 in renderers)
					{
						if (renderer2 != null)
						{
							if (m_shadowing.ContainsKey(renderer2))
							{
								renderer2.shadowCastingMode = m_shadowing[renderer2];
							}
							else
							{
								renderer2.shadowCastingMode = ShadowCastingMode.On;
							}
						}
					}
				}
				else
				{
					foreach (Renderer renderer3 in renderers)
					{
						if (renderer3 != null)
						{
							renderer3.shadowCastingMode = ShadowCastingMode.On;
						}
					}
				}
				if (!m_shadowsOnly)
				{
					propsVisible = true;
				}
				if (VisibilityChanged != null)
				{
					VisibilityChanged(m_shadowsOnly);
				}
			}
		}

		public bool propsVisible
		{
			set
			{
				foreach (Renderer prop in props)
				{
					if (prop != null)
					{
						if (!m_shadowsOnly)
						{
							prop.shadowCastingMode = ShadowCastingMode.On;
						}
						else
						{
							prop.shadowCastingMode = (value ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly);
						}
					}
				}
			}
		}

		public void Build()
		{
			renderers.Clear();
			props.Clear();
			renderers.AddRange(Hierarchy.FindAll<Renderer>(base.transform));
			m_visibility.Clear();
			m_shadowing.Clear();
			foreach (Renderer renderer2 in renderers)
			{
				m_visibility.Add(renderer2, renderer2.enabled);
				m_shadowing.Add(renderer2, renderer2.shadowCastingMode);
			}
			if (trails == null)
			{
				trails = new List<DroneTrail>();
			}
			Material material = null;
			Dictionary<int, Material> dictionary = new Dictionary<int, Material>();
			for (int i = 0; i < renderers.Count; i++)
			{
				Renderer renderer = renderers[i];
				if (renderer is TrailRenderer)
				{
					if (!material)
					{
						material = UnityEngine.Object.Instantiate(renderer.sharedMaterial);
						material.name = material.name.Replace("(Clone)", "") + GetHashCode().ToString("X6") + "-copy";
					}
					renderer.sharedMaterial = material;
					renderers.RemoveAt(i--);
					continue;
				}
				Material[] sharedMaterials = renderer.sharedMaterials;
				if (sharedMaterials.Length == 0)
				{
					continue;
				}
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					Material material2 = sharedMaterials[j];
					if (!material2)
					{
						continue;
					}
					int num = -1;
					for (int k = 0; k < _skin.Length; k++)
					{
						if (_skin[k] == material2)
						{
							num = k;
							break;
						}
					}
					Material material3 = (material2.name.Contains("-copy") ? sharedMaterials[j] : null);
					int instanceID = material2.GetInstanceID();
					bool flag = material2.name.IndexOf("propeller") >= 0;
					bool flag2 = dictionary.ContainsKey(instanceID);
					bool flag3 = false;
					if (!flag2)
					{
						flag3 = true;
					}
					if (flag)
					{
						flag3 = true;
					}
					if (flag3)
					{
						if ((bool)material3)
						{
							UnityEngine.Object.Destroy(material3);
						}
						material2 = (sharedMaterials[j] = UnityEngine.Object.Instantiate(material2));
						material2.name = material2.name.Replace("(Clone)", "") + material2.GetHashCode().ToString("x6") + "-copy";
						if (!flag)
						{
							dictionary[instanceID] = material2;
						}
					}
					if (flag2)
					{
						sharedMaterials[j] = dictionary[instanceID];
					}
					if (num >= 0)
					{
						_skin[num] = sharedMaterials[j];
					}
				}
				renderer.sharedMaterials = sharedMaterials;
			}
			if (trails.Count > 0)
			{
				m_trail_duration = trails[0].renderer.time;
				m_trail_scale = 1f;
			}
			for (int l = 0; l < renderers.Count; l++)
			{
				GameFlagTag component = renderers[l].GetComponent<GameFlagTag>();
				DroneProp componentInParent = renderers[l].GetComponentInParent<DroneProp>();
				if ((component != null && component.tags.Contains(GameFlag.DroneKeepVisible)) || componentInParent != null)
				{
					if (componentInParent != null)
					{
						props.Add(renderers[l]);
					}
					renderers.RemoveAt(l--);
				}
			}
			ResetAllColors();
		}

		protected void SetColor(string p_name, Color p_color)
		{
			SetColor(p_name, p_color, renderers);
			SetColor(p_name, p_color, props);
		}

		protected void SetColor(string p_name, Color p_color, List<Renderer> p_renderers)
		{
			bool flag = p_name == "_Color3";
			for (int i = 0; i < p_renderers.Count; i++)
			{
				Renderer renderer = p_renderers[i];
				if (!renderer)
				{
					continue;
				}
				Material[] sharedMaterials = renderer.sharedMaterials;
				if (sharedMaterials.Length == 0)
				{
					continue;
				}
				foreach (Material material in sharedMaterials)
				{
					if (!material)
					{
						Debug.LogError("DroneRenderer> null material on renderer " + renderer.name);
						continue;
					}
					bool flag2 = false;
					if (flag && material.name.IndexOf("fx-propeller") >= 0)
					{
						flag2 = true;
					}
					material.SetColor(flag2 ? "_Color" : p_name, p_color);
				}
				renderer.sharedMaterials = sharedMaterials;
			}
		}

		protected void ResetAllColors()
		{
			SetColor("_Color1", color0);
			SetColor("_Color2", color1);
			SetColor("_Color3", color2);
		}

		public bool GetTrailsEnabled()
		{
			if (trails.Count <= 0)
			{
				return false;
			}
			DroneTrail droneTrail = trails[0];
			if (!droneTrail)
			{
				return false;
			}
			return droneTrail.renderer.enabled;
		}

		public void SetTrailsEnabled(bool p_flag)
		{
			for (int i = 0; i < trails.Count; i++)
			{
				DroneTrail droneTrail = trails[i];
				if ((bool)droneTrail)
				{
					droneTrail.renderer.enabled = p_flag;
					if (!p_flag)
					{
						droneTrail.renderer.Clear();
					}
				}
			}
		}

		public void SetTrailsActive(bool p_flag)
		{
			for (int i = 0; i < trails.Count; i++)
			{
				DroneTrail droneTrail = trails[i];
				if ((bool)droneTrail)
				{
					droneTrail.renderer.gameObject.SetActive(p_flag);
					if (!p_flag)
					{
						droneTrail.renderer.Clear();
					}
				}
			}
		}

		public void SetTrailsDuration(float p_time)
		{
			trailDuration = p_time;
		}

		protected void ApplyTrailDuration()
		{
			float trail_duration = m_trail_duration;
			float trail_scale = m_trail_scale;
			trail_scale = ((trail_scale <= 0f) ? 0f : (1f / trail_scale));
			for (int i = 0; i < trails.Count; i++)
			{
				DroneTrail droneTrail = trails[i];
				if ((bool)droneTrail)
				{
					droneTrail.renderer.time = trail_duration * trail_scale;
					droneTrail.renderer.startWidth = 0.5f;
					droneTrail.renderer.endWidth = 0.5f;
				}
			}
		}

		public void SetTrailsWidth(float p_widthMultiplier)
		{
			trailWidthMultiplier = p_widthMultiplier;
		}

		protected void ApplyTrailWidth()
		{
			for (int i = 0; i < trails.Count; i++)
			{
				trails[i].renderer.widthMultiplier = trailWidthMultiplier;
			}
		}

		public void ClearTrails()
		{
			for (int i = 0; i < trails.Count; i++)
			{
				DroneTrail it = trails[i];
				if (!it || !it.renderer)
				{
					continue;
				}
				it.renderer.Clear();
				Activity.Run(delegate(float t)
				{
					if (t > 1f / 30f)
					{
						return false;
					}
					if ((bool)it && (bool)it.renderer)
					{
						it.renderer.Clear();
					}
					return true;
				});
			}
		}

		protected void SetTrailColor(string p_name, Color p_color)
		{
			for (int i = 0; i < trails.Count; i++)
			{
				DroneTrail droneTrail = trails[i];
				if ((bool)droneTrail && (bool)droneTrail.renderer.sharedMaterial)
				{
					Color value = p_color;
					value.a = 0.5f;
					Material sharedMaterial = droneTrail.renderer.sharedMaterial;
					if (sharedMaterial.HasProperty(p_name))
					{
						sharedMaterial.SetColor(p_name, value);
					}
					string text = ((p_name == "_Color") ? "_TintColor" : "_Color");
					if (sharedMaterial.HasProperty(text))
					{
						sharedMaterial.SetColor(text, value);
					}
				}
			}
		}

		protected void SetTrailColor(Color[] p_colors)
		{
			Color color = p_colors[0];
			Color color2 = p_colors[1];
			float alpha = 1f;
			Gradient gradient = new Gradient();
			float r = ((color.r > color2.r) ? (color.r - Mathf.Abs((color2.r - color.r) * 0.5f)) : (color.r + Mathf.Abs((color2.r - color.r) * 0.5f)));
			float g = ((color.g > color2.g) ? (color.g - Mathf.Abs((color2.g - color.g) * 0.5f)) : (color.g + Mathf.Abs((color2.g - color.g) * 0.5f)));
			float b = ((color.b > color2.b) ? (color.b - Mathf.Abs((color2.b - color.b) * 0.5f)) : (color.b + Mathf.Abs((color2.b - color.b) * 0.5f)));
			Color color3 = new Color(r, g, b);
			string[] obj = new string[6] { "DronRenderer> SetTrailColor> Primary Color: ", null, null, null, null, null };
			Color color4 = color;
			obj[1] = color4.ToString();
			obj[2] = " Secondary Color:";
			color4 = color2;
			obj[3] = color4.ToString();
			obj[4] = " gradient: ";
			color4 = color3;
			obj[5] = color4.ToString();
			Debug.Log(string.Concat(obj));
			gradient.SetKeys(new GradientColorKey[5]
			{
				new GradientColorKey(color, 0f),
				new GradientColorKey(color, 0.15f),
				new GradientColorKey(color3, 0.3f),
				new GradientColorKey(color2, 0.45f),
				new GradientColorKey(color2, 1f)
			}, new GradientAlphaKey[2]
			{
				new GradientAlphaKey(alpha, 0f),
				new GradientAlphaKey(alpha, 1f)
			});
			for (int i = 0; i < trails.Count; i++)
			{
				trails[i].GetComponent<TrailRenderer>().colorGradient = gradient;
			}
		}

		protected Color[] GetTrailColor(string p_name)
		{
			Color[] array = new Color[Mathf.Max(2, trails.Count)];
			for (int i = 0; i < trails.Count; i++)
			{
				DroneTrail droneTrail = trails[i];
				if ((bool)droneTrail && (bool)droneTrail.renderer.sharedMaterial)
				{
					Material sharedMaterial = droneTrail.renderer.sharedMaterial;
					string text = ((p_name == "_Color") ? "_TintColor" : "_Color");
					if (sharedMaterial.HasProperty(p_name))
					{
						array[i] = sharedMaterial.GetColor(p_name);
					}
					else if (sharedMaterial.HasProperty(text))
					{
						array[i] = sharedMaterial.GetColor(text);
					}
				}
			}
			return array;
		}
	}
}
