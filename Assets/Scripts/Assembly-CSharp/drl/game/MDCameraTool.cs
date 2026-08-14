namespace drl.game
{
	public class MDCameraTool : MDRenderer
	{
		public string easingMode
		{
			get
			{
				return Get("ct-easing-mode", CameraToolEasingMode.Linear);
			}
			set
			{
				Set("ct-easing-mode", value);
			}
		}

		public int index
		{
			get
			{
				return Get("ct-index", 999);
			}
			set
			{
				Set("ct-index", value);
			}
		}

		public MDCameraTool()
		{
			base.type = MapAssetType.CameraTool;
		}
	}
}
