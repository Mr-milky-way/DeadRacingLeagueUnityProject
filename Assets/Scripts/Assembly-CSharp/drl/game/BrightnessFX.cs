using UnityEngine;
using UnityEngine.PostProcessing;
using thelab.mvc;

namespace drl.game
{
	public class BrightnessFX : View<DRLApp>
	{
		[SerializeField]
		private protected Camera m_camera;

		protected PostProcessingBehaviour m_ppb;

		[SerializeField]
		private float m_exposure;

		public Camera camera
		{
			get
			{
				if (!m_camera)
				{
					return m_camera = GetComponent<Camera>();
				}
				return m_camera;
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
				m_ppb = (this ? GetComponent<PostProcessingBehaviour>() : null);
				return m_ppb;
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

		protected void Awake()
		{
			if ((bool)base.app && !base.app.brightness)
			{
				base.app.brightness = this;
			}
			if ((bool)ppb)
			{
				if ((bool)ppb.profile)
				{
					PostProcessingProfile postProcessingProfile = Object.Instantiate(ppb.profile);
					ppb.profile = postProcessingProfile;
					postProcessingProfile.name = "app.brightness.profile";
				}
				else
				{
					Debug.LogWarning("BrightnessFx> PostProcessing Profile is <null>");
				}
			}
		}

		public virtual void WritePPP()
		{
			if ((bool)ppb)
			{
				camera.depth = 300f;
				ColorGradingModel colorGradingModel = (ppb.profile ? ppb.profile.colorGrading : null);
				if (colorGradingModel != null)
				{
					ColorGradingModel.Settings settings = colorGradingModel.settings;
					settings.basic.postExposure = m_exposure;
					colorGradingModel.settings = settings;
				}
				ppb.enabled = Mathf.Abs(exposure) > 0f;
			}
		}

		public void OnPersistency()
		{
			base.app.brightness = this;
		}
	}
}
