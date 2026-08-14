using System;
using UnityEngine;
using drl.sim.rci;

namespace drl.game
{
	[Serializable]
	public class GameCommand
	{
		public string hash;

		public GameCommandType type;

		public KeyCode key;

		public ConsoleButtons button = ConsoleButtons.None;

		public bool useAxis;

		public RawAxis axis;

		public bool down;

		public bool leftAlt;

		public bool rightAlt;

		public bool leftCtrl;

		public bool rightCtrl;

		public bool leftShift;

		public bool rightShift;

		public bool alt
		{
			get
			{
				if (!leftAlt)
				{
					return rightAlt;
				}
				return true;
			}
		}

		public bool ctrl
		{
			get
			{
				if (!leftCtrl)
				{
					return rightCtrl;
				}
				return true;
			}
		}

		public bool shift
		{
			get
			{
				if (!leftShift)
				{
					return rightShift;
				}
				return true;
			}
		}

		public bool modified
		{
			get
			{
				if (!alt && !ctrl)
				{
					return shift;
				}
				return true;
			}
		}

		public override string ToString()
		{
			string text = "";
			text = text + type.ToString() + "> ";
			if (key != KeyCode.None)
			{
				text = text + "K[" + key.ToString() + "] ";
			}
			if (button >= (ConsoleButtons)0)
			{
				text = text + "B[" + button.ToString() + "] ";
			}
			if (useAxis)
			{
				text = text + "A[" + axis.ToString() + "] ";
			}
			string text2 = (leftAlt ? "1" : "0");
			string text3 = (rightAlt ? "1" : "0");
			string text4 = (leftCtrl ? "1" : "0");
			string text5 = (rightCtrl ? "1" : "0");
			string text6 = (leftShift ? "1" : "0");
			string text7 = (rightShift ? "1" : "0");
			return text + "alt[" + text2 + "," + text3 + "] ctrl[" + text4 + "," + text5 + "] shift[" + text6 + "," + text7 + "]";
		}
	}
}
