using System;
using Rewired;
using drl.sim.rci;
using thelab.mvc;

namespace drl.game
{
	public class UISettingsRCCustomController : Controller<DRLApp>
	{
		private class FoundJoystick
		{
			public readonly string name;

			public readonly Guid guid;

			public readonly int id;

			public RCDeviceData deviceData;

			public Joystick joystick => ReInput.controllers.GetJoystick(id);

			public FoundJoystick(string name, Guid guid, int id)
			{
				this.name = name;
				this.guid = guid;
				this.id = id;
			}
		}
	}
}
