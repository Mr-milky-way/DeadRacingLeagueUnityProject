using UnityEngine;

namespace drl
{
	[ExecuteInEditMode]
	[ImageEffectAllowedInSceneView]
	[AddComponentMenu("Image Effects/DRL/SpeedFX")]
	public class CameraSpeedFX : MonoBehaviour
	{
		[SerializeField]
		private Material m_material;

		public Texture2D noiseTexture;

		public float minSpeed = 30f;

		public float maxSpeed = 100f;

		public float minSampleDistance;

		public float maxSampleDistance = 0.6f;

		public float samples = 10f;

		public float sampleDistance;

		public float sampleMult = 1f;

		public float sampleExp = 1f;

		public Vector2 centerOffset = new Vector2(0.2f, 0.2f);

		public Vector2 center = new Vector2(0f, 0f);

		private Vector3 m_lastPosition;

		private Vector3 m_delayVelocity;

		public Material material
		{
			get
			{
				if ((bool)m_material)
				{
					return m_material;
				}
				Shader shader = Shader.Find("DRL/Image Effects/Speed FX");
				if (!shader)
				{
					Debug.LogError("CameraSpeedFX: shader not found:DRL/Image Effects/Speed FX");
					return null;
				}
				m_material = new Material(shader);
				if (!m_material)
				{
					return null;
				}
				m_material.name = "CameraSpeedFXMaterial";
				m_material.hideFlags = HideFlags.HideAndDontSave;
				return m_material;
			}
		}

		protected void Start()
		{
			m_lastPosition = base.transform.position;
		}

		protected void LateUpdate()
		{
			if (!(material == null))
			{
				material.SetFloat("_SampleMult", sampleMult);
				material.SetFloat("_SampleExp", sampleExp);
				material.SetTexture("_NoiseTex", noiseTexture);
				if (!Application.isPlaying)
				{
					material.SetFloat("_SampleDist", sampleDistance);
					material.SetFloat("_CenterX", center.x);
					material.SetFloat("_CenterY", center.y);
					return;
				}
				Vector3 b = base.transform.position - m_lastPosition;
				m_lastPosition = base.transform.position;
				m_delayVelocity = Vector3.Lerp(m_delayVelocity, b, Time.deltaTime * 1f);
				float t = (((Time.deltaTime <= 0f) ? 0f : (b.magnitude * 3.6f / Time.deltaTime)) - minSpeed) / (maxSpeed - minSpeed);
				Vector3 normalized = m_delayVelocity.normalized;
				float num = Vector3.Dot(base.transform.right, normalized);
				float num2 = Vector3.Dot(base.transform.up, normalized);
				float value = Vector3.Dot(base.transform.forward, m_delayVelocity);
				float x = centerOffset.x * num * Mathf.Clamp01(value);
				float y = centerOffset.y * num2 * Mathf.Clamp01(value);
				float b2 = Mathf.Lerp(minSampleDistance, maxSampleDistance, t);
				sampleDistance = Mathf.Lerp(sampleDistance, b2, Time.deltaTime * 20f);
				center.x = x;
				center.y = y;
				material.SetFloat("_SampleDist", sampleDistance);
				material.SetFloat("_CenterX", center.x);
				material.SetFloat("_CenterY", center.y);
			}
		}

		protected void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			Graphics.Blit(source, destination, material);
		}
	}
}
