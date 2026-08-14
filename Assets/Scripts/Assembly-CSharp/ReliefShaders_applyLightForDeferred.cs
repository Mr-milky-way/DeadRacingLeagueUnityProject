using UnityEngine;

[ExecuteInEditMode]
public class ReliefShaders_applyLightForDeferred : MonoBehaviour
{
	public Light lightForSelfShadowing;

	private Renderer _renderer;

	private int m_renderer_retry;

	private void Reset()
	{
		lightForSelfShadowing = GetComponent<Light>();
		m_renderer_retry = 180;
	}

	private void Update()
	{
		if (!lightForSelfShadowing)
		{
			return;
		}
		if (m_renderer_retry > 0 && !_renderer)
		{
			m_renderer_retry--;
			_renderer = GetComponent<Renderer>();
		}
		Transform transform = lightForSelfShadowing.transform;
		if ((bool)_renderer)
		{
			Material[] sharedMaterials = _renderer.sharedMaterials;
			Vector3 vector;
			if (lightForSelfShadowing.type == LightType.Directional)
			{
				vector = -transform.forward;
				for (int i = 0; i < sharedMaterials.Length; i++)
				{
					sharedMaterials[i].SetVector("_WorldSpaceLightPosCustom", vector);
				}
				return;
			}
			vector = transform.position;
			Vector4 value = new Vector4(vector.x, vector.y, vector.z, 1f);
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				sharedMaterials[j].SetVector("_WorldSpaceLightPosCustom", value);
			}
		}
		else if (lightForSelfShadowing.type == LightType.Directional)
		{
			Vector3 vector = -lightForSelfShadowing.transform.forward;
			Shader.SetGlobalVector("_WorldSpaceLightPosCustom", vector);
		}
		else
		{
			Vector3 vector = transform.position;
			Vector4 value2 = new Vector4(vector.x, vector.y, vector.z, 1f);
			Shader.SetGlobalVector("_WorldSpaceLightPosCustom", value2);
		}
	}
}
