using UnityEngine;
using UnityEngine.UI;
using drl.game;

public class UICircuitMapItemStripeView : UICardView
{
	public Image stripe;

	public Color incompleteRaceColor;

	public Color completedRaceColor;

	public void SetComplete()
	{
		stripe.color = completedRaceColor;
	}

	public void Reset()
	{
		stripe.color = incompleteRaceColor;
	}
}
