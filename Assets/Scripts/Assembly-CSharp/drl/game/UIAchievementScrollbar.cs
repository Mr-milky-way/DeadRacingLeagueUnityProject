using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	public class UIAchievementScrollbar : MonoBehaviour
	{
		[SerializeField]
		private GridLayoutGroup gridLayoutGroup;

		[SerializeField]
		private int enabledSpacing = 8;

		[SerializeField]
		private int disabledSpacing = 8;

		[SerializeField]
		private int enabledCellSize = 542;

		[SerializeField]
		private int disabledCellSize = 550;

		private void OnEnable()
		{
			gridLayoutGroup.spacing = new Vector2(enabledSpacing, gridLayoutGroup.spacing.y);
			gridLayoutGroup.cellSize = new Vector2(enabledCellSize, gridLayoutGroup.cellSize.y);
		}

		private void OnDisable()
		{
			gridLayoutGroup.spacing = new Vector2(disabledSpacing, gridLayoutGroup.spacing.y);
			gridLayoutGroup.cellSize = new Vector2(disabledCellSize, gridLayoutGroup.cellSize.y);
		}
	}
}
