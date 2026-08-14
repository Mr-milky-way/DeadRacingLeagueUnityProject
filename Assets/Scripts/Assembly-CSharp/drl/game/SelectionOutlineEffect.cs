using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.ImageEffects;
using thelab.core;

namespace drl.game
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[ImageEffectAllowedInSceneView]
	public class SelectionOutlineEffect : PostEffectsBase
	{
		public enum State
		{
			SelectedCapture = 0,
			EdgeCapture = 1,
			Compose = 2
		}

		[HideInInspector]
		[Range(0f, 2f)]
		public float edgeSensitivityDepth = 1f;

		[HideInInspector]
		[Range(0f, 2f)]
		public float edgeSensitivityNormal = 1f;

		[Range(0f, 4f)]
		public float edgeSampleDistance = 1f;

		[Range(0f, 1f)]
		public float fillIntensity = 0.2f;

		[Range(0f, 1f)]
		public float blurIntensity = 0.5f;

		public Color edgeColor = Colorf.RGBToColor(16777056u);

		public Material selectionFillMaterial;

		public Texture2D edgePattern;

		private Camera m_camera;

		[HideInInspector]
		public State state = State.Compose;

		public LayerMask layer;

		private int m_layer_flag = -1;

		public Shader selectionOutlineShader;

		private Material selectionOutlineMaterial;

		[SerializeField]
		private List<Renderer> m_targets;

		private List<Renderer> l_remove;

		private Dictionary<Renderer, int> m_lut_layers;

		private Dictionary<Renderer, Material[]> m_lut_materials;

		private Dictionary<Renderer, ShadowCastingMode> m_lut_scm;

		private Dictionary<Renderer, int> m_lut_visible;

		public BlurOptimized blur;

		public RenderTexture edgeMaskRT;

		public RenderTexture edgeSmoothRT;

		public Camera camera => m_camera ?? (m_camera = GetComponent<Camera>());

		public int layerFlag
		{
			get
			{
				if (m_layer_flag != -1)
				{
					return m_layer_flag;
				}
				int num = layer;
				m_layer_flag = 0;
				while (num > 1)
				{
					num >>= 1;
					m_layer_flag++;
				}
				return m_layer_flag;
			}
		}

		public List<Renderer> targets
		{
			get
			{
				return m_targets;
			}
			set
			{
				List<Renderer> list = m_targets;
				List<Renderer> list2 = new List<Renderer>();
				if (value != null)
				{
					list2.AddRange(value);
				}
				if (l_remove == null)
				{
					l_remove = new List<Renderer>();
				}
				else
				{
					l_remove.Clear();
				}
				for (int i = 0; i < list.Count; i++)
				{
					Renderer renderer = list[i];
					if ((bool)renderer && !list2.Contains(renderer))
					{
						l_remove.Add(renderer);
					}
				}
				for (int j = 0; j < l_remove.Count; j++)
				{
					Renderer renderer2 = l_remove[j];
					if ((bool)renderer2)
					{
						if (lut_layers.ContainsKey(renderer2))
						{
							renderer2.gameObject.layer = lut_layers[renderer2];
							lut_layers.Remove(renderer2);
						}
						if (lut_materials.ContainsKey(renderer2))
						{
							renderer2.sharedMaterials = lut_materials[renderer2];
							lut_materials.Remove(renderer2);
						}
						if (lut_scm.ContainsKey(renderer2))
						{
							renderer2.shadowCastingMode = lut_scm[renderer2];
							lut_scm.Remove(renderer2);
						}
						if (lut_visible.ContainsKey(renderer2))
						{
							lut_visible.Remove(renderer2);
						}
					}
				}
				m_targets = list2;
			}
		}

		protected Dictionary<Renderer, int> lut_layers
		{
			get
			{
				if (m_lut_layers != null)
				{
					return m_lut_layers;
				}
				return m_lut_layers = new Dictionary<Renderer, int>();
			}
		}

		protected Dictionary<Renderer, Material[]> lut_materials
		{
			get
			{
				if (m_lut_materials != null)
				{
					return m_lut_materials;
				}
				return m_lut_materials = new Dictionary<Renderer, Material[]>();
			}
		}

		protected Dictionary<Renderer, ShadowCastingMode> lut_scm
		{
			get
			{
				if (m_lut_scm != null)
				{
					return m_lut_scm;
				}
				return m_lut_scm = new Dictionary<Renderer, ShadowCastingMode>();
			}
		}

		protected Dictionary<Renderer, int> lut_visible
		{
			get
			{
				if (m_lut_visible != null)
				{
					return m_lut_visible;
				}
				return m_lut_visible = new Dictionary<Renderer, int>();
			}
		}

		protected void Awake()
		{
			if (Application.isPlaying)
			{
				selectionFillMaterial = Object.Instantiate(selectionFillMaterial);
				selectionFillMaterial.name = selectionFillMaterial.name.Replace("(Clone)", "-" + selectionFillMaterial.GetInstanceID().ToString("x6"));
			}
		}

		public override bool CheckResources()
		{
			CheckSupport(needDepth: true);
			selectionOutlineMaterial = CheckShaderAndCreateMaterial(selectionOutlineShader, selectionOutlineMaterial);
			SetCameraFlag();
			if (!isSupported)
			{
				ReportAutoDisable();
			}
			return isSupported;
		}

		protected void SetCameraFlag()
		{
			GetComponent<Camera>().depthTextureMode |= DepthTextureMode.DepthNormals;
		}

		protected void OnEnable()
		{
			SetCameraFlag();
		}

		protected bool IsVisible(Renderer p_item)
		{
			if (p_item.isVisible)
			{
				return true;
			}
			if (lut_visible.ContainsKey(p_item))
			{
				return lut_visible[p_item] <= 1000;
			}
			return true;
		}

		protected void SetTargetsTint(Color p_color)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				Renderer renderer = targets[i];
				if ((bool)renderer)
				{
					Material sharedMaterial = renderer.sharedMaterial;
					if ((bool)sharedMaterial && sharedMaterial.HasProperty("_Color"))
					{
						sharedMaterial.SetColor("_Color", p_color);
					}
				}
			}
		}

		protected void PushTargetsState()
		{
			for (int i = 0; i < targets.Count; i++)
			{
				PushTargetState(targets[i]);
			}
		}

		protected void PushTargetState(Renderer r)
		{
			if ((bool)r)
			{
				lut_layers[r] = r.gameObject.layer;
				lut_materials[r] = r.sharedMaterials;
				lut_scm[r] = r.shadowCastingMode;
				r.gameObject.layer = layerFlag;
				r.sharedMaterial = selectionFillMaterial;
				r.shadowCastingMode = ShadowCastingMode.Off;
			}
		}

		protected void PopTargetsState()
		{
			for (int i = 0; i < targets.Count; i++)
			{
				PopTargetState(targets[i]);
			}
		}

		protected void PopTargetState(Renderer r)
		{
			if ((bool)r)
			{
				if (lut_layers.ContainsKey(r))
				{
					r.gameObject.layer = lut_layers[r];
				}
				if (lut_materials.ContainsKey(r))
				{
					r.sharedMaterials = lut_materials[r];
				}
				if (lut_scm.ContainsKey(r))
				{
					r.shadowCastingMode = lut_scm[r];
				}
			}
		}

		protected void RefreshVisibility(Renderer r)
		{
			if ((bool)r)
			{
				int num = (lut_visible.ContainsKey(r) ? lut_visible[r] : 0);
				lut_visible[r] = ((!r.isVisible) ? Mathf.Min(1001, num + 1) : 0);
			}
		}

		protected void RefreshVisibility()
		{
			for (int i = 0; i < targets.Count; i++)
			{
				RefreshVisibility(targets[i]);
			}
		}

		protected void LateUpdate()
		{
			if ((bool)camera)
			{
				FieldInfo field = typeof(Canvas).GetField("willRenderCanvases", BindingFlags.Static | BindingFlags.NonPublic);
				object value = field.GetValue(null);
				field.SetValue(null, null);
				edgeMaskRT = GetRT("soe-edge-mask", edgeMaskRT);
				edgeSmoothRT = GetRT("soe-edge-smooth", edgeSmoothRT);
				PushTargetsState();
				state = State.EdgeCapture;
				camera.targetTexture = edgeMaskRT;
				camera.clearFlags = CameraClearFlags.Color;
				camera.backgroundColor = Color.white;
				blur.enabled = false;
				SetTargetsTint(Color.white);
				camera.Render();
				state = State.SelectedCapture;
				camera.targetTexture = edgeSmoothRT;
				camera.clearFlags = CameraClearFlags.Color;
				camera.backgroundColor = Color.white;
				blur.enabled = true;
				float num = 1f - fillIntensity;
				SetTargetsTint(new Color(num, num, num, 1f));
				camera.Render();
				state = State.Compose;
				camera.targetTexture = null;
				camera.clearFlags = CameraClearFlags.Depth;
				blur.enabled = false;
				PopTargetsState();
				field.SetValue(null, value);
			}
		}

		[ImageEffectOpaque]
		protected void OnRenderImage(RenderTexture p_src, RenderTexture p_dst)
		{
			if (!CheckResources())
			{
				Graphics.Blit(p_src, p_dst);
				return;
			}
			Material material = selectionOutlineMaterial;
			switch (state)
			{
			case State.EdgeCapture:
			{
				Vector4 value2 = new Vector4(edgeSensitivityDepth, edgeSensitivityNormal, 1f, edgeSensitivityNormal);
				material.SetVector("_EdgeSensitivity", value2);
				material.SetFloat("_EdgeBackground", 0f);
				material.SetFloat("_EdgeBlur", 1f);
				material.SetFloat("_EdgeSampleDistance", edgeSampleDistance * 0.5f);
				Graphics.Blit(p_src, p_dst, selectionOutlineMaterial, 0);
				break;
			}
			case State.SelectedCapture:
			{
				Vector4 value = new Vector4(edgeSensitivityDepth, edgeSensitivityNormal, 1f, edgeSensitivityNormal);
				material.SetVector("_EdgeSensitivity", value);
				material.SetFloat("_EdgeBackground", 1f);
				material.SetFloat("_EdgeSampleDistance", edgeSampleDistance);
				material.SetFloat("_EdgeBlur", blurIntensity);
				Graphics.Blit(p_src, p_dst, selectionOutlineMaterial, 0);
				break;
			}
			case State.Compose:
				material.SetTexture("_EdgeMaskTex", edgeMaskRT);
				material.SetTexture("_EdgeSmoothTex", edgeSmoothRT);
				material.SetTexture("_EdgePatternTex", edgePattern ? edgePattern : Texture2D.whiteTexture);
				material.SetColor("_EdgeColor", edgeColor);
				Graphics.Blit(p_src, p_dst, selectionOutlineMaterial, 1);
				break;
			}
		}

		protected RenderTexture GetRT(string p_id, RenderTexture p_current)
		{
			int num = (p_current ? p_current.width : 0);
			int num2 = (p_current ? p_current.height : 0);
			if (num == Screen.width && num2 == Screen.height && !(p_current == null))
			{
				return p_current;
			}
			int width = Screen.width;
			num2 = Screen.height;
			if ((bool)p_current)
			{
				RenderTexture.ReleaseTemporary(p_current);
			}
			RenderTexture temporary = RenderTexture.GetTemporary(width, num2, 24, RenderTextureFormat.ARGBFloat);
			temporary.name = p_id;
			if (!Application.isPlaying)
			{
				temporary.hideFlags = HideFlags.HideAndDontSave;
			}
			return temporary;
		}
	}
}
