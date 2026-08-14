using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class OSInputMiddleware : MonoBehaviour
	{
		public enum Rules
		{
			None = 0,
			AllowAltTab = 1,
			AllowWindowsKey = 2,
			AllowAltTabAndWindows = 3
		}

		public Rules rules;

		public List<KeyCode> modifiers;

		private const int WH_KEYBOARD_LL = 13;

		private const int WM_KEYUP = 257;

		private const int WM_SYSKEYUP = 261;

		private const int WM_KEYDOWN = 256;

		private const int WM_SYSKEYDOWN = 260;

		private const int VK_SHIFT = 16;

		private const int VK_CONTROL = 17;

		private const int VK_MENU = 18;

		private const int VK_CAPITAL = 20;

		private const int VK_ESCAPE = 27;

		private const int VK_TAB = 9;

		private const int VK_LWIN = 91;

		private const int VK_RWIN = 92;

		private IntPtr hookID = IntPtr.Zero;

		private OS.HookHandlerDelegate proc;

		private bool isEditor;

		public bool GetModifier(KeyCode k)
		{
			return Get(k, modifiers);
		}

		protected bool Get(KeyCode k, List<KeyCode> l)
		{
			return l.Contains(k);
		}

		protected void Awake()
		{
			isEditor = Application.isEditor;
		}

		protected IntPtr HookCallback(int nCode, IntPtr wParam, ref OS.KeyboardHookStruct lParam)
		{
			bool flag = base.enabled;
			if (isEditor)
			{
				flag = false;
			}
			if (nCode < 0)
			{
				flag = false;
			}
			if (!flag)
			{
				return OS.CallNextHookEx(hookID, nCode, wParam, ref lParam);
			}
			bool num = rules == Rules.AllowAltTabAndWindows;
			bool flag2 = num || rules == Rules.AllowWindowsKey;
			bool flag3 = num || rules == Rules.AllowAltTab;
			bool flag4 = true;
			if (wParam == (IntPtr)257 || wParam == (IntPtr)261 || wParam == (IntPtr)256 || wParam == (IntPtr)260)
			{
				if (lParam.vkCode < 160 || lParam.vkCode > 164)
				{
					UpdateModifiers();
				}
				switch (lParam.flags)
				{
				case 0:
					if (lParam.vkCode == 27 && GetModifier(KeyCode.LeftControl))
					{
						flag4 = flag4 && flag2;
					}
					break;
				case 1:
					if (lParam.vkCode == 91 || lParam.vkCode == 92)
					{
						flag4 = flag4 && flag2;
					}
					break;
				case 32:
					if (lParam.vkCode == 9)
					{
						flag4 = flag4 && flag3;
					}
					break;
				}
			}
			if (!flag4)
			{
				return (IntPtr)1;
			}
			return OS.CallNextHookEx(hookID, nCode, wParam, ref lParam);
		}

		protected void UpdateModifiers()
		{
			modifiers.Clear();
			if ((OS.GetKeyState(20) & 1) != 0)
			{
				modifiers.Add(KeyCode.CapsLock);
			}
			if ((OS.GetKeyState(16) & 0x8000) != 0)
			{
				modifiers.Add(KeyCode.LeftShift);
				modifiers.Add(KeyCode.RightShift);
			}
			if ((OS.GetKeyState(17) & 0x8000) != 0)
			{
				modifiers.Add(KeyCode.LeftControl);
				modifiers.Add(KeyCode.RightControl);
			}
			if ((OS.GetKeyState(18) & 0x8000) != 0)
			{
				modifiers.Add(KeyCode.LeftAlt);
				modifiers.Add(KeyCode.RightAlt);
			}
		}

		protected void OnDestroy()
		{
			if (hookID != IntPtr.Zero)
			{
				OS.UnhookWindowsHookEx(hookID);
			}
		}
	}
}
