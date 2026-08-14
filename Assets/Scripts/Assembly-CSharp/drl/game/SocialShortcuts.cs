using System;
using UnityEngine;
using drl.sim.rci;

namespace drl.game
{
	[Serializable]
	public struct SocialShortcuts
	{
		public KeyCode keyboard;

		public KeyCode chatFocus;

		public ConsoleButtons chatFocusGamepad;
	}
}
