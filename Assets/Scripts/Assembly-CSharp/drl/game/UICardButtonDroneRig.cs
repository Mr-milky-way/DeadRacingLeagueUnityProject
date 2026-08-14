using System;
using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class UICardButtonDroneRig : UICardView
	{
		public Text title0Field;

		public Text title1Field;

		public Text infoLeftField;

		public Text infoRightField;

		public RawImage previewField;

		public RawImage imageField;

		public GameObject customPhysicsMarker;

		public UIStatusView status;

		private FadeComponent m_preview_fade;

		private FadeComponent m_image_fade;

		public DRLDroneRig asset;

		public bool defaultRig;

		private DroneRigData m_data;

		private WebAsyncRequest m_photo_loader;

		public GarageStateModel model;

		public override UICardType type => UICardType.ButtonDroneRig;

		public string title0
		{
			set
			{
				UIReflection.Set(title0Field, value);
			}
		}

		public string title1
		{
			set
			{
				UIReflection.Set(title1Field, value);
			}
		}

		public string infoLeft
		{
			set
			{
				UIReflection.Set(infoLeftField, value);
			}
		}

		public string infoRight
		{
			set
			{
				UIReflection.Set(infoRightField, value);
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

		public FadeComponent previewFade
		{
			get
			{
				if (!m_preview_fade)
				{
					if (!previewField)
					{
						return null;
					}
					return m_preview_fade = previewField.GetComponent<FadeComponent>();
				}
				return m_preview_fade;
			}
		}

		public FadeComponent imageFade
		{
			get
			{
				if (!m_image_fade)
				{
					if (!imageField)
					{
						return null;
					}
					return m_image_fade = imageField.GetComponent<FadeComponent>();
				}
				return m_image_fade;
			}
		}

		public RectTransform imageRT
		{
			get
			{
				if (!imageField)
				{
					return null;
				}
				return imageField.transform as RectTransform;
			}
		}

		public Vector2 imageSize
		{
			get
			{
				if (!imageRT)
				{
					return Vector2.zero;
				}
				return imageRT.sizeDelta;
			}
			set
			{
				if ((bool)imageRT)
				{
					imageRT.sizeDelta = value;
				}
			}
		}

		public float imageWidth
		{
			get
			{
				return imageSize.x;
			}
			set
			{
				Vector2 vector = imageSize;
				vector.x = value;
				imageSize = vector;
			}
		}

		public float imageHeight
		{
			get
			{
				return imageSize.y;
			}
			set
			{
				Vector2 vector = imageSize;
				vector.y = value;
				imageSize = vector;
			}
		}

		public new DroneRigData data
		{
			get
			{
				if (m_data != null)
				{
					return m_data;
				}
				if (!asset)
				{
					return null;
				}
				return asset.rig;
			}
			set
			{
				m_data = value;
			}
		}

		public void Set(DRLDroneRig p_data)
		{
			if ((bool)p_data)
			{
				title1 = p_data.label.ToUpper();
				image = (p_data.preview ? p_data.preview : p_data.image);
				asset = p_data;
				title0Field.gameObject.SetActive(value: false);
			}
		}

		public void Set(DroneRigData p_data, Action p_callback = null)
		{
			if (p_data == null)
			{
				return;
			}
			title1 = p_data.name.ToUpper();
			title0Field.gameObject.SetActive(value: false);
			if (data != null && p_data.guid != data.guid)
			{
				image = null;
			}
			if ((bool)previewFade)
			{
				previewFade.alpha = 0.1f;
			}
			if ((bool)imageFade)
			{
				imageFade.alpha = 0f;
			}
			model.GetRigThumbnail(p_data, 320, 0, delegate(Texture2D p_result)
			{
				if (p_result != null && p_result.width > 128)
				{
					image = p_result;
					if ((bool)previewFade)
					{
						previewFade.FadeOut(0.8f, 0.2f);
					}
					if ((bool)imageFade)
					{
						imageFade.FadeIn(0.8f, 0.2f);
					}
					if (p_callback != null)
					{
						p_callback();
					}
				}
			});
			data = p_data;
			if (p_data.hasCustomPhysics)
			{
				infoLeft = "CUSTOM PHYSICS";
				infoLeftField.transform.parent.gameObject.SetActive(value: true);
				infoLeftField.color = Color.yellow;
				customPhysicsMarker.SetActive(value: true);
			}
			else
			{
				infoLeftField.transform.parent.gameObject.SetActive(value: false);
				customPhysicsMarker.SetActive(value: false);
			}
		}

		public override void Build()
		{
			base.Build();
		}
	}
}
