using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using thelab.core;

namespace drl
{
	public class DRLQualityGroup : QualityGroup
	{
		public List<PostProcessingProfile> fxprofiles;

		[SerializeField]
		protected PostProcessingBehaviour[] m_postProcessing;

		public PostProcessingBehaviour[] postProcessing
		{
			get
			{
				if (m_postProcessing != null && m_postProcessing.Length > 0)
				{
					return m_postProcessing;
				}
				return m_postProcessing = Object.FindObjectsOfType<PostProcessingBehaviour>();
			}
			set
			{
				m_postProcessing = value;
			}
		}

		public override void Apply(int p_quality)
		{
			if (base.enabled)
			{
				base.Apply(p_quality);
				ApplyPPP(p_quality);
			}
		}

		public void ApplyPPP(int p_quality)
		{
			PostProcessingBehaviour[] array = postProcessing;
			if (array.Length == 0)
			{
				return;
			}
			if (fxprofiles == null || fxprofiles.Count <= 0)
			{
				Debug.Log("QualityGroup> PostProcessingBehaviour / No Profiles Found!");
				return;
			}
			int index = Mathf.Clamp(p_quality, 0, fxprofiles.Count - 1);
			PostProcessingBehaviour[] array2 = array;
			foreach (PostProcessingBehaviour postProcessingBehaviour in array2)
			{
				if (!postProcessingBehaviour)
				{
					continue;
				}
				string text = postProcessingBehaviour.name;
				if (text == null || !(text == "app.brightness"))
				{
					PostProcessingProfile postProcessingProfile = fxprofiles[index];
					string text2 = postProcessingProfile.name;
					postProcessingBehaviour.profile = Object.Instantiate(postProcessingProfile);
					postProcessingBehaviour.profile.name = text2;
					Debug.Log("QualityGroup> Using PPP [" + Hierarchy.Path(postProcessingBehaviour.transform) + "][" + index + "]");
					CameraFX component = postProcessingBehaviour.GetComponent<CameraFX>();
					if ((bool)component)
					{
						component.ReadPPP();
					}
				}
			}
		}
	}
}
