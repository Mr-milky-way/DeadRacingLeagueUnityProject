using UnityEngine;
using drl.sim.rci;
using thelab.core;

namespace drl.game
{
	public class DRLUINavigationSystem : UINavigationSystem
	{
		public static bool useWASD = false;

		public static bool controllerEnabled = true;

		public static bool controllerNavEnabled = true;

		public static bool keyboardEnabled = true;

		public static bool keyboardNavEnabled = true;

		public static UINavigation lastNavigationDown;

		public static bool IsTyping { get; set; }

		public static bool IsLoading { get; set; }

		public static bool IsButton()
		{
			bool flag = keyboardEnabled && Input.GetKeyUp(KeyCode.Return);
			if (keyboardEnabled)
			{
				flag = flag || Input.GetKeyUp(KeyCode.KeypadEnter);
			}
			if (controllerEnabled)
			{
				flag = flag || (RCI.HasNavigationController && RCI.GetButtonUp(ConsoleButtons.ActionBottomRow1));
			}
			return flag;
		}

		public static bool IsNavigation()
		{
			if (keyboardEnabled && keyboardNavEnabled)
			{
				if (Input.GetKey(KeyCode.UpArrow))
				{
					return true;
				}
				if (Input.GetKey(KeyCode.DownArrow))
				{
					return true;
				}
				if (Input.GetKey(KeyCode.LeftArrow))
				{
					return true;
				}
				if (Input.GetKey(KeyCode.RightArrow))
				{
					return true;
				}
			}
			if (!controllerEnabled || !controllerNavEnabled)
			{
				return false;
			}
			if (!RCI.HasNavigationController)
			{
				return false;
			}
			RCI.GetAxisTrigger(RawAxis.LeftStickX, isPositiveSign: true);
			if (RCI.GetAxisTrigger(RawAxis.LeftStickX, isPositiveSign: false) || RCI.GetButtonDown(ConsoleButtons.DPadRight))
			{
				return true;
			}
			if (RCI.GetAxisTrigger(RawAxis.LeftStickX, isPositiveSign: false) || RCI.GetButtonDown(ConsoleButtons.DPadLeft))
			{
				return true;
			}
			if (RCI.GetAxisTrigger(RawAxis.LeftStickY, isPositiveSign: true) || RCI.GetButtonDown(ConsoleButtons.DPadUp))
			{
				return true;
			}
			if (RCI.GetAxisTrigger(RawAxis.LeftStickY, isPositiveSign: false) || RCI.GetButtonDown(ConsoleButtons.DPadDown))
			{
				return true;
			}
			return false;
		}

		protected override bool IsClick()
		{
			return IsButton();
		}

		protected override bool IsNavigationUp()
		{
			if (keyboardEnabled && keyboardNavEnabled && Input.GetKey(KeyCode.UpArrow))
			{
				return true;
			}
			if (keyboardEnabled && keyboardNavEnabled && useWASD && Input.GetKey(KeyCode.W))
			{
				return true;
			}
			if (!controllerEnabled || !controllerNavEnabled)
			{
				return false;
			}
			if (!GetAxisTrigger(RawAxis.LeftStickY, p_directionPos: true))
			{
				return RCI.GetButtonDown(ConsoleButtons.DPadUp);
			}
			return true;
		}

		protected override bool IsNavigationDown()
		{
			if (keyboardEnabled && keyboardNavEnabled && Input.GetKey(KeyCode.DownArrow))
			{
				return true;
			}
			if (keyboardEnabled && keyboardNavEnabled && useWASD && Input.GetKey(KeyCode.S))
			{
				return true;
			}
			if (!controllerEnabled || !controllerNavEnabled)
			{
				return false;
			}
			if (!GetAxisTrigger(RawAxis.LeftStickY, p_directionPos: false))
			{
				return RCI.GetButtonDown(ConsoleButtons.DPadDown);
			}
			return true;
		}

		protected override bool IsNavigationLeft()
		{
			if (keyboardEnabled && keyboardNavEnabled && Input.GetKey(KeyCode.LeftArrow))
			{
				return true;
			}
			if (keyboardEnabled && keyboardNavEnabled && useWASD && Input.GetKey(KeyCode.A))
			{
				return true;
			}
			if (!controllerEnabled || !controllerNavEnabled)
			{
				return false;
			}
			if (!GetAxisTrigger(RawAxis.LeftStickX, p_directionPos: false))
			{
				return RCI.GetButtonDown(ConsoleButtons.DPadLeft);
			}
			return true;
		}

		protected override bool IsNavigationRight()
		{
			if (keyboardEnabled && keyboardNavEnabled && Input.GetKey(KeyCode.RightArrow))
			{
				return true;
			}
			if (keyboardEnabled && keyboardNavEnabled && useWASD && Input.GetKey(KeyCode.D))
			{
				return true;
			}
			if (!controllerEnabled || !controllerNavEnabled)
			{
				return false;
			}
			if (!GetAxisTrigger(RawAxis.LeftStickX, p_directionPos: true))
			{
				return RCI.GetButtonDown(ConsoleButtons.DPadRight);
			}
			return true;
		}

		protected bool GetAxisTrigger(RawAxis f, bool p_directionPos)
		{
			bool flag = true;
			if (!RCI.HasNavigationController)
			{
				flag = false;
			}
			bool result = RCI.GetAxisTrigger(f, p_directionPos);
			if (!flag)
			{
				result = false;
			}
			if (!controllerEnabled)
			{
				result = false;
			}
			if (!controllerNavEnabled)
			{
				result = false;
			}
			return result;
		}

		public static bool IsKeyboard()
		{
			if (Input.GetKey(KeyCode.UpArrow))
			{
				return true;
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				return true;
			}
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				return true;
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				return true;
			}
			if (Input.GetKey(KeyCode.W) && useWASD)
			{
				return true;
			}
			if (Input.GetKey(KeyCode.A) && useWASD)
			{
				return true;
			}
			if (Input.GetKey(KeyCode.S) && useWASD)
			{
				return true;
			}
			if (Input.GetKey(KeyCode.D) && useWASD)
			{
				return true;
			}
			if (Input.GetKeyUp(KeyCode.Return))
			{
				return true;
			}
			if (Input.GetKeyUp(KeyCode.KeypadEnter))
			{
				return true;
			}
			if (Input.GetKeyUp(KeyCode.Escape))
			{
				return true;
			}
			return false;
		}

		public static bool IsKeyboardButton()
		{
			if (Input.GetKeyUp(KeyCode.Return))
			{
				return true;
			}
			if (Input.GetKeyUp(KeyCode.KeypadEnter))
			{
				return true;
			}
			return false;
		}

		public static bool IsControllerButton()
		{
			if (!RCI.HasNavigationController)
			{
				return false;
			}
			return RCI.GetButtonUp(ConsoleButtons.ActionBottomRow1);
		}
	}
}
