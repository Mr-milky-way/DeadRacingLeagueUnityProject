using UnityEngine;
using UnityEngine.UI;
using drl.backend;

namespace drl.game
{
	public class UICircuitItemView : UICardView
	{
		public DRLCircuitData circuitData;

		public RawImage thumb;

		public Text circuitTitle;

		public Text circuitSubTitle;

		public GameObject spinnerLoader;

		public void Set(DRLCircuitData p_data)
		{
			circuitData = p_data;
			Texture circuitThumb = null;
			spinnerLoader.SetActive(value: true);
			thumb.gameObject.SetActive(value: false);
			circuitTitle.text = "";
			circuitSubTitle.text = "";
			base.app.model.storage.state.player.circuits.GetCircuitThumbnail(p_data, delegate(Texture2D texture2D)
			{
				if (base.validContext)
				{
					if (texture2D != null)
					{
						circuitThumb = texture2D;
						thumb.texture = circuitThumb;
						thumb.gameObject.SetActive(value: true);
					}
					spinnerLoader.SetActive(value: false);
					circuitTitle.text = p_data.name.ToUpper();
					circuitSubTitle.text = p_data.description.ToUpper();
				}
			});
		}
	}
}
