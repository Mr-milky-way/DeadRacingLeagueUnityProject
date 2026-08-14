using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UICardButtonTournamentSkill : UICardView
	{
		public int skillRequired;

		public RectTransform footerEligible;

		public RectTransform footerNotEligible;

		public Text footerNotEligibleAmountText;

		public override UICardType type => UICardType.ButtonTournamentSkillItem;

		public void Refresh()
		{
			bool p_eligible = base.app.model.storage.state.player.userRank >= skillRequired;
			SetEligibility(p_eligible, skillRequired.ToString());
		}

		public bool CanEnter()
		{
			if ((bool)footerEligible)
			{
				return footerEligible.gameObject.activeInHierarchy;
			}
			return false;
		}

		public void SetEligibility(bool p_eligible, string p_amount)
		{
			if ((bool)footerEligible)
			{
				footerEligible.gameObject.SetActive(p_eligible);
			}
			if ((bool)footerNotEligible)
			{
				footerNotEligible.gameObject.SetActive(!p_eligible);
			}
			if (!p_eligible && (bool)footerNotEligibleAmountText)
			{
				footerNotEligibleAmountText.text = p_amount;
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
			((RectTransform)base.transform).sizeDelta = focusResize.min;
		}
	}
}
