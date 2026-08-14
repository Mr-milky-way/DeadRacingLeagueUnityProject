using System.Collections;
using UnityEngine;

namespace drl
{
	public class CameraShake : MonoBehaviour
	{
		public AnimationCurve fade;

		private float m_duration = 1f;

		private Vector3 m_shakeVector = Vector3.zero;

		public float duration;

		public float intensityMultiplier = 1f;

		public void Shake(float p_intensity = 1f, float p_duration = 0.5f)
		{
			if (duration <= 0f)
			{
				duration = 0.1f;
			}
			m_duration = p_duration + duration;
			StartCoroutine(ShakeCoroutine(p_intensity));
			D.Log("CameraShake> Called - intensity: " + intensityMultiplier + " duration: " + duration);
		}

		private IEnumerator ShakeCoroutine(float p_intensity)
		{
			while (m_duration > 0f)
			{
				m_duration -= Time.deltaTime;
				float num = fade.Evaluate(1f - m_duration / duration);
				float p_intensity2 = p_intensity * num;
				base.transform.localEulerAngles = UpdateShake(p_intensity2);
				yield return null;
			}
			base.transform.localRotation = Quaternion.identity;
		}

		private Vector3 UpdateShake(float p_intensity = 1f)
		{
			float num = Random.Range(-100f, 100f);
			m_shakeVector.x = (Mathf.PerlinNoise(num, 0f) - 0.5f) * 2f;
			m_shakeVector.y = Mathf.PerlinNoise(0f, num) - 0.5f;
			m_shakeVector.z = 0f;
			return m_shakeVector * p_intensity * 10f * intensityMultiplier;
		}
	}
}
