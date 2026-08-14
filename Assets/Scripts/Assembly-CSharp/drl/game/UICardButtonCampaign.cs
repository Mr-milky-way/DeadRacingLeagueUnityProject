namespace drl.game
{
	public class UICardButtonCampaign : UICardButtonLarge
	{
		public new DRLCampaign data;

		public override UICardType type => UICardType.ButtonCampaign;

		public override void Build()
		{
			base.Build();
		}

		public void Set(DRLCampaign p_data)
		{
			if ((bool)p_data)
			{
				data = p_data;
				base.label = p_data.title.ToUpper();
				base.preview = p_data.image;
				base.image = p_data.image;
			}
		}
	}
}
