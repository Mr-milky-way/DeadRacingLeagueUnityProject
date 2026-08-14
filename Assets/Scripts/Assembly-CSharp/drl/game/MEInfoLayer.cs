using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MEInfoLayer : View<DRLApp>
	{
		public FadeResizeComponent fade;

		[Header("Help")]
		public MEInfoHelpLayer help;
	}
}
