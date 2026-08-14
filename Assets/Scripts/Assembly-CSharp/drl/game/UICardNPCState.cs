using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	[RequireComponent(typeof(RawImage))]
	public class UICardNPCState : MonoBehaviour
	{
		private RawImage npcImage;

		public Texture npcImagePC;

		public Texture npcImageXbox;

		public Texture npcImagePS;

		private void Start()
		{
			npcImage = GetComponent<RawImage>();
			if (!(npcImage == null))
			{
				npcImage.texture = npcImagePC;
			}
		}
	}
}
