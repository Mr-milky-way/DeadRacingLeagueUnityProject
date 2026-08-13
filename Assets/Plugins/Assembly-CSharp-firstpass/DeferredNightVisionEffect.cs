using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DeferredNightVisionEffect : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The main color of the NV effect")]
	public Color m_NVColor = new Color(0f, 1f, 0.1724138f, 0f);

	[SerializeField]
	[Tooltip("The color that the NV effect will 'bleach' towards (white = default)")]
	public Color m_TargetBleachColor = new Color(1f, 1f, 1f, 0f);

	[Range(0f, 0.1f)]
	[Tooltip("How much base lighting does the NV effect pick up")]
	public float m_baseLightingContribution = 0.025f;

	[Range(0f, 128f)]
	[Tooltip("The higher this value, the more bright areas will get 'bleached out'")]
	public float m_LightSensitivityMultiplier = 100f;

	private Material m_Material;

	public Shader Shader;

	[Tooltip("Do we want to apply a vignette to the edges of the screen?")]
	public bool useVignetting = true;

	private void Start()
	{
		m_Material = new Material(Shader);
		UpdateShaderValues();
	}

	[ContextMenu("UpdateShaderValues")]
	public void UpdateShaderValues()
	{
		if (!(m_Material == null))
		{
			m_Material.SetVector("_NVColor", m_NVColor);
			m_Material.SetVector("_TargetWhiteColor", m_TargetBleachColor);
			m_Material.SetFloat("_BaseLightingContribution", m_baseLightingContribution);
			m_Material.SetFloat("_LightSensitivityMultiplier", m_LightSensitivityMultiplier);
			m_Material.shaderKeywords = null;
			if (useVignetting)
			{
				Shader.EnableKeyword("USE_VIGNETTE");
			}
			else
			{
				Shader.DisableKeyword("USE_VIGNETTE");
			}
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (m_Material != null && base.enabled)
		{
			Graphics.Blit(source, destination, m_Material);
		}
	}
}
