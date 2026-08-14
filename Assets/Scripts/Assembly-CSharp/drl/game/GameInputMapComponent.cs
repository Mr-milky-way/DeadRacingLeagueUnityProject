using System.Collections.Generic;
using UnityEngine;
using drl.sim.rci;

namespace drl.game
{
	public class GameInputMapComponent : MonoBehaviour
	{
		public List<GameCommand> commands;

		public bool leftAlt => Input.GetKey(KeyCode.LeftAlt);

		public bool rightAlt => Input.GetKey(KeyCode.RightAlt);

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

		public bool leftCtrl
		{
			get
			{
				if (!Input.GetKey(KeyCode.LeftControl))
				{
					return Input.GetKey(KeyCode.LeftCommand);
				}
				return true;
			}
		}

		public bool rightCtrl
		{
			get
			{
				if (!Input.GetKey(KeyCode.RightControl))
				{
					return Input.GetKey(KeyCode.RightCommand);
				}
				return true;
			}
		}

		public bool leftShift => Input.GetKey(KeyCode.LeftShift);

		public bool rightShift => Input.GetKey(KeyCode.RightShift);

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

		public bool modified
		{
			get
			{
				if (!alt)
				{
					return ctrl;
				}
				return true;
			}
		}

		protected void Start()
		{
		}

		public GameCommand GetCommand()
		{
			for (int i = 0; i < commands.Count; i++)
			{
				GameCommand gameCommand = commands[i];
				bool flag = false || GetInput(gameCommand.key, gameCommand.down) || GetInput(gameCommand.button, gameCommand.down);
				if ((gameCommand.button == ConsoleButtons.LeftShoulder1 || gameCommand.button == ConsoleButtons.RightShoulder1) && RCI.UsingToggles())
				{
					flag = false;
				}
				if (gameCommand.useAxis)
				{
					flag = flag || GetInput(gameCommand.axis);
				}
				bool num = gameCommand.key == KeyCode.LeftAlt;
				bool flag2 = gameCommand.key == KeyCode.RightAlt;
				bool flag3 = gameCommand.key == KeyCode.LeftControl;
				bool flag4 = gameCommand.key == KeyCode.RightControl;
				bool flag5 = gameCommand.key == KeyCode.LeftShift;
				bool flag6 = gameCommand.key == KeyCode.RightShift;
				bool flag7 = gameCommand.leftAlt;
				bool flag8 = !num && leftAlt;
				flag = flag && flag7 == flag8;
				flag7 = gameCommand.rightAlt;
				flag8 = !flag2 && rightAlt;
				flag = flag && flag7 == flag8;
				flag7 = gameCommand.leftCtrl;
				flag8 = !flag3 && leftCtrl;
				flag = flag && flag7 == flag8;
				flag7 = gameCommand.rightCtrl;
				flag8 = !flag4 && rightCtrl;
				flag = flag && flag7 == flag8;
				flag7 = gameCommand.leftShift;
				flag8 = !flag5 && leftShift;
				flag = flag && flag7 == flag8;
				flag7 = gameCommand.rightShift;
				flag8 = !flag6 && rightShift;
				if (flag && flag7 == flag8)
				{
					return gameCommand;
				}
			}
			return null;
		}

		protected bool GetInput(KeyCode k, bool d)
		{
			if (k == KeyCode.None)
			{
				return false;
			}
			if (!d)
			{
				return Input.GetKeyUp(k);
			}
			return Input.GetKeyDown(k);
		}

		protected bool GetInput(ConsoleButtons k, bool d)
		{
			if (k < (ConsoleButtons)0)
			{
				return false;
			}
			if (!d)
			{
				return RCI.GetButtonUp(k);
			}
			return RCI.GetButtonDown(k);
		}

		protected bool GetInput(RawAxis k)
		{
			return RCI.GetAxisToggle(k);
		}
	}
}
