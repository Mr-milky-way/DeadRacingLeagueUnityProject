using UnityEngine;

namespace drl.game
{
	public class MAGuide : MARenderer
	{
		[SerializeField]
		protected MeshRenderer m_asset;

		public new MDGuide data
		{
			get
			{
				return base.data as MDGuide;
			}
			set
			{
				base.data = value;
			}
		}

		public MeshRenderer asset
		{
			get
			{
				if ((bool)m_asset)
				{
					return m_asset;
				}
				Transform transform = base.transform.Find("asset");
				return m_asset = transform.GetComponent<MeshRenderer>();
			}
		}

		public virtual void SetEnabled(bool p_flag)
		{
			Transform transform = base.transform.Find("asset");
			if ((bool)transform)
			{
				transform.gameObject.SetActive(p_flag);
			}
			transform = base.transform.Find("icon");
			if ((bool)transform)
			{
				transform.gameObject.SetActive(p_flag);
			}
		}

		public override void Write()
		{
			base.Write();
			_ = data;
		}

		public override void Read()
		{
			_ = m_data is MDGuide;
			base.Read();
		}

		protected override MDObject NewData()
		{
			return new MDGuide();
		}

		public void SetColor(Color p_color)
		{
			if ((bool)asset)
			{
				Material sharedMaterial = asset.sharedMaterial;
				if (sharedMaterial.HasProperty("_Color"))
				{
					sharedMaterial.SetColor("_Color", p_color);
				}
				else if (sharedMaterial.HasProperty("_Tint"))
				{
					sharedMaterial.SetColor("_Tint", p_color);
				}
				asset.sharedMaterial = sharedMaterial;
			}
		}

		public void SetAssetActive(string p_name, bool p_flag)
		{
			if ((bool)base.gameObject)
			{
				Transform transform = base.transform.Find(p_name);
				if ((bool)transform)
				{
					transform.gameObject.SetActive(p_flag);
				}
			}
		}

		public override void OnEditorSelect()
		{
			SetAssetMode(p_flag: true);
		}

		public override void OnEditorUnselect()
		{
			SetIconMode(p_flag: true);
		}

		public void SetIconMode(bool p_flag)
		{
			Transform transform = base.transform.Find("icon");
			Transform transform2 = base.transform.Find("asset");
			if (!transform && (bool)transform2)
			{
				SetAssetMode(p_flag: true);
				return;
			}
			if ((bool)transform)
			{
				transform.gameObject.SetActive(p_flag);
			}
			if ((bool)transform2)
			{
				transform2.gameObject.SetActive(!p_flag);
			}
		}

		public void SetAssetMode(bool p_flag)
		{
			Transform transform = base.transform.Find("icon");
			Transform transform2 = base.transform.Find("asset");
			if (!transform2 && (bool)transform)
			{
				SetIconMode(p_flag: true);
				return;
			}
			if ((bool)transform)
			{
				transform.gameObject.SetActive(!p_flag);
			}
			if ((bool)transform2)
			{
				transform2.gameObject.SetActive(p_flag);
			}
		}
	}
}
