using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using thelab.core;

namespace drl.game
{
	public class UICardButtonLarge : UICardView
	{
		public UICardButtonLargeType subType;

		public UICardButtonLargeListType listType;

		[SerializeField]
		private Text m_labelField;

		public Text subtitle;

		[SerializeField]
		private RawImage m_previewField;

		[SerializeField]
		private RawImage m_imageField;

		[SerializeField]
		private FadeComponent m_imageFade;

		[SerializeField]
		private FadeComponent m_previewFade;

		[SerializeField]
		private VideoPlayer m_videoField;

		private QualityGroup m_imageFadeQualityGroup;

		public bool fadeField;

		public override UICardType type => UICardType.ButtonLarge;

		public Text labelField
		{
			get
			{
				if (!m_labelField)
				{
					return m_labelField = Find<Text>("content.body.title-3");
				}
				return m_labelField;
			}
		}

		public RawImage previewField
		{
			get
			{
				if (!m_previewField)
				{
					return m_previewField = Find<RawImage>("backgrounds.preview");
				}
				return m_previewField;
			}
		}

		public RawImage imageField
		{
			get
			{
				if (!m_imageField)
				{
					return m_imageField = Find<RawImage>("backgrounds.image");
				}
				return m_imageField;
			}
		}

		public FadeComponent imageFade
		{
			get
			{
				if (!m_imageFade)
				{
					return m_imageFade = (imageField ? imageField.GetComponent<FadeComponent>() : null);
				}
				return m_imageFade;
			}
		}

		public FadeComponent previewFade
		{
			get
			{
				if (!m_previewFade)
				{
					return m_previewFade = (previewField ? previewField.GetComponent<FadeComponent>() : null);
				}
				return m_previewFade;
			}
		}

		public VideoPlayer videoField
		{
			get
			{
				if (!m_videoField)
				{
					return m_videoField = (imageField ? imageField.GetComponent<VideoPlayer>() : null);
				}
				return m_videoField;
			}
		}

		private QualityGroup imageFadeQualityGroup
		{
			get
			{
				if (!m_imageFadeQualityGroup)
				{
					return m_imageFadeQualityGroup = imageFade.gameObject.GetComponent<QualityGroup>();
				}
				return m_imageFadeQualityGroup;
			}
		}

		public string label
		{
			set
			{
				UIReflection.Set(labelField, value);
			}
		}

		public Texture preview
		{
			set
			{
				UIReflection.Set(previewField, value);
				if ((bool)previewField)
				{
					previewField.enabled = value;
				}
			}
		}

		public Texture image
		{
			set
			{
				UIReflection.Set(imageField, value);
				if ((bool)imageField)
				{
					imageField.enabled = value;
				}
			}
		}

		protected void Start()
		{
			if ((bool)videoField)
			{
				QualityGroup qualityGroup = videoField.gameObject.AddComponent<QualityGroup>();
				qualityGroup.targets = new List<QualityGroup.Element>();
				QualityGroup.Element element = new QualityGroup.Element();
				element.target = videoField;
				element.flags = new List<bool>(new bool[5] { false, false, true, true, true });
				qualityGroup.targets.Add(element);
				if ((bool)imageField)
				{
					element = new QualityGroup.Element();
					element.target = imageField;
					element.flags = new List<bool>(new bool[5] { false, false, true, true, true });
					qualityGroup.targets.Add(element);
				}
				qualityGroup.Apply();
			}
			if ((bool)imageField && (bool)imageField.texture && (bool)videoField && (bool)imageFade && (bool)imageFadeQualityGroup)
			{
				int num = imageFadeQualityGroup.targets.FindIndex(delegate(QualityGroup.Element x)
				{
					RawImage rawImage = x.target as RawImage;
					return rawImage != null && rawImage == imageField;
				});
				if (num > -1 && imageFadeQualityGroup.currentQualityLevel > -1 && !imageFadeQualityGroup.targets[num].flags[imageFadeQualityGroup.currentQualityLevel])
				{
					imageField.enabled = false;
				}
			}
		}

		public override void Build()
		{
			base.Build();
			FocusResize focusResize = GetComponent<FocusResize>();
			if (!focusResize)
			{
				focusResize = base.gameObject.AddComponent<FocusResize>();
			}
			focusResize.enabled = true;
			focusResize.min = new Vector2(420f, 540f);
			focusResize.max = new Vector2(500f, 650f);
			focusResize.duration = 0.1f;
			label = "LARGE BUTTON";
			preview = null;
			image = null;
			((RectTransform)base.transform).sizeDelta = focusResize.min;
		}

		public override void OnFocus()
		{
			base.OnFocus();
			if (!imageField || !imageField.texture)
			{
				return;
			}
			if ((bool)videoField && (bool)imageFade && (bool)imageFadeQualityGroup)
			{
				int num = imageFadeQualityGroup.targets.FindIndex(delegate(QualityGroup.Element x)
				{
					RawImage rawImage = x.target as RawImage;
					return rawImage != null && rawImage == imageField;
				});
				if (num > -1 && !imageFadeQualityGroup.targets[num].flags[imageFadeQualityGroup.currentQualityLevel])
				{
					return;
				}
			}
			FadeComponent fadeComponent = ((!fadeField) ? null : (labelField ? labelField.GetComponent<FadeComponent>() : null));
			if ((bool)fadeComponent)
			{
				fadeComponent.FadeOut(0.2f);
			}
			fadeComponent = imageFade;
			if ((bool)fadeComponent)
			{
				fadeComponent.FadeIn(0.2f);
			}
		}

		public override void OnUnfocus()
		{
			base.OnUnfocus();
			if (!imageField || !imageField.texture)
			{
				return;
			}
			if ((bool)videoField && (bool)imageFade && (bool)imageFadeQualityGroup)
			{
				int num = imageFadeQualityGroup.targets.FindIndex(delegate(QualityGroup.Element x)
				{
					RawImage rawImage = x.target as RawImage;
					return rawImage != null && rawImage == imageField;
				});
				if (num > -1 && !imageFadeQualityGroup.targets[num].flags[imageFadeQualityGroup.currentQualityLevel])
				{
					return;
				}
			}
			FadeComponent fadeComponent = ((!fadeField) ? null : (labelField ? labelField.GetComponent<FadeComponent>() : null));
			if ((bool)fadeComponent)
			{
				fadeComponent.FadeIn(0.2f);
			}
			fadeComponent = imageFade;
			if ((bool)fadeComponent)
			{
				fadeComponent.FadeOut(0.2f);
			}
		}
	}
}
