using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityStandardAssets.ImageEffects;
using thelab.core;

namespace drl
{
	public class CameraFX : MonoBehaviour
	{
		private Camera m_camera;

		private BlurOptimized m_blurFilter;

		private float m_blur;

		private Grayscale m_grayscaleFilter;

		private float m_grayscale;

		protected postVHSPro m_postVHS;

		public bool radioEnabled = true;

		public bool radioLock;

		private float m_radio;

		protected PostProcessingBehaviour m_ppb;

		protected SunShafts m_sunshafts;

		public bool sunshaftsAllowed = true;

		[SerializeField]
		private float m_aoIntensity;

		[SerializeField]
		private float m_aoRadius;

		[SerializeField]
		private float m_saturation;

		private float m_saturation_default;

		[SerializeField]
		private float m_exposure;

		private float m_exposure_default;

		[SerializeField]
		private float m_brightness;

		private Activity m_exposure_anim;

		private Activity m_saturation_anim;

		private Activity m_exposure_timer;

		public bool depthOfFieldEnabled = true;

		public DepthOfFieldModel.KernelSize depthOfFieldKernel = DepthOfFieldModel.KernelSize.VeryLarge;

		private Activity m_dof_activity;

		protected CameraShake m_shake;

		private DistortPP m_distortFilter;

		public Camera camera
		{
			get
			{
				if ((bool)m_camera)
				{
					return m_camera;
				}
				m_camera = GetComponent<Camera>();
				if ((bool)m_camera)
				{
					return m_camera;
				}
				m_camera = GetComponentInChildren<Camera>();
				return m_camera;
			}
		}

		public BlurOptimized blurFilter
		{
			get
			{
				if (!m_blurFilter)
				{
					return m_blurFilter = Hierarchy.Find<BlurOptimized>(base.transform);
				}
				return m_blurFilter;
			}
		}

		public float blur
		{
			get
			{
				return m_blur;
			}
			set
			{
				m_blur = Mathf.Clamp01(value);
				BlurOptimized blurOptimized = blurFilter;
				if ((bool)blurOptimized)
				{
					blurOptimized.blurSize = Mathf.Lerp(0f, 10f, m_blur);
					bool flag = blurOptimized.blurSize <= 0.3f;
					blurOptimized.blurIterations = (flag ? 1 : 2);
					blurOptimized.downsample = ((!flag) ? 2 : 0);
					blurOptimized.enabled = m_blur > 0f;
				}
			}
		}

		public Grayscale grayscaleFilter
		{
			get
			{
				if (!camera)
				{
					return null;
				}
				return Reflection<object>.Assert(ref m_grayscaleFilter, camera.gameObject);
			}
		}

		public float grayscale
		{
			get
			{
				return m_grayscale;
			}
			set
			{
				m_grayscale = Mathf.Clamp01(value);
				Grayscale grayscale = grayscaleFilter;
				if ((bool)grayscale)
				{
					grayscale.intensity = m_grayscale;
					grayscale.enabled = m_grayscale > 0f;
				}
			}
		}

		public postVHSPro postVHS
		{
			get
			{
				if ((bool)m_postVHS)
				{
					return m_postVHS;
				}
				return m_postVHS = Hierarchy.Find<postVHSPro>(base.transform);
			}
		}

		public float radio
		{
			get
			{
				return m_radio;
			}
			set
			{
				m_radio = Mathf.Clamp01(value);
				postVHSPro postVHSPro2 = postVHS;
				if ((bool)postVHSPro2)
				{
					float t = m_radio;
					postVHSPro2.bleedAmount = Mathf.Lerp(4.6f, 0f, t);
					postVHSPro2.filmGrainAmount = Mathf.Lerp(0.035f, 0f, t);
					postVHSPro2.signalNoiseAmount = Mathf.Lerp(0.33f, 0f, t);
					postVHSPro2.lineNoiseAmount = Mathf.Lerp(10f, 0f, t);
					postVHSPro2.tapeNoiseTH = Mathf.Lerp(0.2f, 1f, t);
					postVHSPro2.tapeNoiseAmount = Mathf.Lerp(0.2f, 0f, t);
					postVHSPro2.signalAdjustY = Mathf.Lerp(-0.015f, 0f, t);
					postVHSPro2.signalAdjustI = Mathf.Lerp(-0.035f, 0f, t);
					postVHSPro2.signalAdjustQ = Mathf.Lerp(0.002f, 0f, t);
					postVHSPro2.signalShiftY = Mathf.Lerp(1.22f, 1f, t);
					postVHSPro2.signalShiftI = Mathf.Lerp(1.22f, 1f, t);
					postVHSPro2.signalShiftQ = Mathf.Lerp(0.8f, 1f, t);
					postVHSPro2.gammaCorection = Mathf.Lerp(1.15f, 1f, t);
					postVHSPro2.enabled = m_radio < 0.98f && radioEnabled && !radioLock;
				}
			}
		}

		public PostProcessingBehaviour ppb
		{
			get
			{
				if ((bool)m_ppb)
				{
					return m_ppb;
				}
				m_ppb = GetComponentInChildren<PostProcessingBehaviour>();
				ReadPPP();
				return m_ppb;
			}
		}

		public SunShafts sunshafts
		{
			get
			{
				if (!m_sunshafts)
				{
					return m_sunshafts = GetComponentInChildren<SunShafts>();
				}
				return m_sunshafts;
			}
		}

		public float aoIntensity
		{
			get
			{
				return m_aoIntensity;
			}
			set
			{
				m_aoIntensity = value;
				WritePPP();
			}
		}

		public float aoRadius
		{
			get
			{
				return m_aoRadius;
			}
			set
			{
				m_aoRadius = value;
				WritePPP();
			}
		}

		public float saturation
		{
			get
			{
				return m_saturation;
			}
			set
			{
				m_saturation = value;
				WritePPP();
			}
		}

		public float exposure
		{
			get
			{
				return m_exposure;
			}
			set
			{
				m_exposure = value;
				WritePPP();
			}
		}

		public float brightness
		{
			get
			{
				return m_brightness;
			}
			set
			{
				m_brightness = value;
				WritePPP();
			}
		}

		public CameraShake shake
		{
			get
			{
				if (!m_shake)
				{
					return m_shake = GetComponentInChildren<CameraShake>();
				}
				return m_shake;
			}
		}

		public DistortPP distortFilter
		{
			get
			{
				if (!m_distortFilter)
				{
					return m_distortFilter = Hierarchy.Find<DistortPP>(base.transform);
				}
				return m_distortFilter;
			}
		}

		public bool distortEnabled
		{
			get
			{
				if ((bool)distortFilter)
				{
					return distortFilter.enabled;
				}
				return false;
			}
			set
			{
				if ((bool)distortFilter)
				{
					distortFilter.enabled = value;
				}
			}
		}

		public void FadeBlur(float v, float p_duration, float p_delay = 0f)
		{
			Tween.Kill(this, "blur");
			if (p_duration <= 0f)
			{
				blur = v;
			}
			else
			{
				Tween.Add(this, "blur", v, p_duration, p_delay, Cubic.Out);
			}
		}

		public void FadeGrayscale(float v, float p_duration, float p_delay = 0f)
		{
			Tween.Kill(this, "grayscale");
			if (p_duration <= 0f)
			{
				grayscale = v;
			}
			else
			{
				Tween.Add(this, "grayscale", v, p_duration, p_delay, Cubic.Out);
			}
		}

		public void ExposureGrayscale(bool p_flag, float p_duration, float p_delay = 0f)
		{
			float p_over_exp = (p_flag ? 3.5f : 0f);
			float p_exposure = (p_flag ? 0.5f : m_exposure_default);
			float p_saturation = (p_flag ? 0f : m_saturation_default);
			ExposureSaturation(p_over_exp, p_exposure, p_saturation, p_duration, p_delay);
		}

		public void ExposureSaturation(float p_over_exp, float p_exposure, float p_saturation, float p_duration, float p_delay = 0f)
		{
			if (m_exposure_timer != null)
			{
				m_exposure_timer.Stop();
			}
			if (m_exposure_anim != null)
			{
				m_exposure_anim.Stop();
			}
			bool has_over_exp = p_over_exp > 0f;
			m_exposure_timer = Activity.RunOnce(delegate
			{
				if (has_over_exp)
				{
					exposure = p_over_exp;
				}
				m_exposure_anim = Tween.Add(this, "exposure", p_exposure, p_duration, Cubic.Out);
				m_saturation_anim = Tween.Add(this, "saturation", p_saturation, has_over_exp ? 0f : p_duration, Cubic.Out);
				if (m_exposure_anim != null)
				{
					m_exposure_anim.unscaledTime = true;
				}
				if (m_saturation_anim != null)
				{
					m_saturation_anim.unscaledTime = true;
				}
			}, p_delay);
		}

		public void FadeSaturation(float p_value, float p_duration, float p_delay = 0f)
		{
			Tween.Kill(this, "saturation");
			Tween.Add(this, "saturation", p_value, p_duration, p_delay, Cubic.Out);
		}

		public void SetDOF(Transform p_target, float p_aperture, float p_flength)
		{
			ClearDOF();
			if (!depthOfFieldEnabled)
			{
				return;
			}
			Camera c = camera;
			Transform t = p_target;
			PostProcessingBehaviour pb = ppb;
			if (!t)
			{
				Debug.LogWarning("CameraFX> SetDOF - Target is null.");
				return;
			}
			if (!pb)
			{
				Debug.LogWarning("CameraFX> SetDOF - PPB os null.");
				return;
			}
			PostProcessingProfile ppp = pb.profile;
			ppp.depthOfField.enabled = true;
			m_dof_activity = Activity.Run((Func<bool>)delegate
			{
				if (!depthOfFieldEnabled)
				{
					ppp.depthOfField.enabled = false;
					return false;
				}
				if (!ppp.depthOfField.enabled)
				{
					ppp.depthOfField.enabled = true;
				}
				if (!pb)
				{
					return false;
				}
				ppp = pb.profile;
				float focusDistance = Vector3.Distance(c.transform.position, t.position);
				DepthOfFieldModel.Settings settings = ppp.depthOfField.settings;
				settings.aperture = p_aperture;
				settings.focalLength = p_flength;
				settings.focusDistance = focusDistance;
				ppp.depthOfField.settings = settings;
				return true;
			}, 0f, false);
		}

		public void ClearDOF()
		{
			StopDOFUpdate();
			if ((bool)ppb)
			{
				PostProcessingProfile profile = ppb.profile;
				if (profile != null)
				{
					profile.depthOfField.enabled = false;
				}
			}
		}

		private void StopDOFUpdate()
		{
			if (m_dof_activity != null)
			{
				m_dof_activity.Stop();
			}
			m_dof_activity = null;
		}

		public virtual void ReadPPP()
		{
			if ((bool)m_ppb)
			{
				AmbientOcclusionModel ambientOcclusion = m_ppb.profile.ambientOcclusion;
				m_aoRadius = ambientOcclusion?.settings.radius ?? 0f;
				m_aoIntensity = ambientOcclusion?.settings.intensity ?? 0f;
				ColorGradingModel colorGrading = m_ppb.profile.colorGrading;
				m_saturation = colorGrading?.settings.basic.saturation ?? 1f;
				m_saturation_default = m_saturation;
				m_exposure = colorGrading?.settings.basic.postExposure ?? 0.5f;
				m_exposure_default = m_exposure;
			}
		}

		public virtual void WritePPP()
		{
			if ((bool)ppb)
			{
				AmbientOcclusionModel ambientOcclusion = ppb.profile.ambientOcclusion;
				if (ambientOcclusion != null)
				{
					AmbientOcclusionModel.Settings settings = ambientOcclusion.settings;
					settings.radius = m_aoRadius;
					settings.intensity = m_aoIntensity;
					ambientOcclusion.settings = settings;
				}
				ColorGradingModel colorGrading = ppb.profile.colorGrading;
				if (colorGrading != null)
				{
					ColorGradingModel.Settings settings2 = colorGrading.settings;
					settings2.basic.saturation = m_saturation;
					settings2.basic.postExposure = m_exposure + m_brightness;
					colorGrading.settings = settings2;
				}
			}
		}

		public void SetMotionBlurEnabled(bool p_flag)
		{
			List<PostProcessingBehaviour> list = Hierarchy.FindAll<PostProcessingBehaviour>(base.transform);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].profile.motionBlur.enabled = p_flag;
			}
		}
	}
}
