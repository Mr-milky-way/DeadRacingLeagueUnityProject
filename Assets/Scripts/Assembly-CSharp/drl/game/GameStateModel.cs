using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class GameStateModel : Model<DRLApp>
	{
		public SettingsStateModel parent => AssertParent<SettingsStateModel>("parent");

		public DataFlow data => parent.data;

		public bool raceGuide
		{
			get
			{
				return data.Get("settings-game-race-guide", d: true);
			}
			set
			{
				data.Set("settings-game-race-guide", value);
				Refresh();
			}
		}

		public bool racePath
		{
			get
			{
				return data.Get("settings-game-race-path", d: false);
			}
			set
			{
				data.Set("settings-game-race-path", value);
				Refresh();
			}
		}

		public bool raceStats
		{
			get
			{
				return data.Get("settings-game-race-stats", d: true);
			}
			set
			{
				data.Set("settings-game-race-stats", value);
				Refresh();
			}
		}

		public bool raceFastReset
		{
			get
			{
				return data.Get("settings-game-race-fast-reset", d: false);
			}
			set
			{
				data.Set("settings-game-race-fast-reset", value);
				Refresh();
			}
		}

		public bool radioNoise
		{
			get
			{
				return data.Get("settings-radio-noise", d: false);
			}
			set
			{
				data.Set("settings-radio-noise", value);
				Refresh();
			}
		}

		public bool raceAutoStandings
		{
			get
			{
				return data.Get("settings-game-race-auto-standings", d: true);
			}
			set
			{
				data.Set("settings-game-race-auto-standings", value);
				Refresh();
			}
		}

		public bool gateMarkers
		{
			get
			{
				return data.Get("settings-game-gate-markers", d: true);
			}
			set
			{
				data.Set("settings-game-gate-markers", value);
				Refresh();
			}
		}

		public bool fpsWarning
		{
			get
			{
				if (Application.platform == RuntimePlatform.XboxOne)
				{
					return false;
				}
				if (Application.platform == RuntimePlatform.PS4)
				{
					return false;
				}
				return data.Get("settings-game-fps-warning", d: true);
			}
			set
			{
				data.Set("settings-game-fps-warning", value);
				Refresh();
			}
		}

		public bool controllerOverlay
		{
			get
			{
				return data.Get("settings-game-controller-overlay", d: false);
			}
			set
			{
				data.Set("settings-game-controller-overlay", value);
				Refresh();
			}
		}

		public bool trails
		{
			get
			{
				return data.Get("settings-game-trails", d: true);
			}
			set
			{
				data.Set("settings-game-trails", value);
				Refresh();
			}
		}

		public float batteryResistanceMin => data.Get("settings-battery-resistance-min", 18f);

		public float batteryResistanceMax => data.Get("settings-battery-resistance-max", 24f);

		public float batteryResistance
		{
			get
			{
				return data.Get("settings-battery-resistance", 18f);
			}
			set
			{
				data.Set("settings-battery-resistance", value);
				Refresh();
			}
		}

		public float batteryCapacity => data.Get("settings-battery-capacity", 2300f);

		public float trailsDuration
		{
			get
			{
				return data.Get("settings-game-trails-duration", 0.2f);
			}
			set
			{
				data.Set("settings-game-trails-duration", value);
				Refresh();
			}
		}

		public bool lensDistortion
		{
			get
			{
				return data.Get("settings-game-lens-distortion", d: false);
			}
			set
			{
				data.Set("settings-game-lens-distortion", value);
				Refresh();
			}
		}

		public bool propsVisible
		{
			get
			{
				return data.Get("settings-game-props-visibility", d: true);
			}
			set
			{
				data.Set("settings-game-props-visibility", value);
				Refresh();
			}
		}

		public bool armAndTurtle
		{
			get
			{
				return data.Get("settings-game-arm-and-turtle", d: false);
			}
			set
			{
				data.Set("settings-game-arm-and-turtle", value);
				Refresh();
			}
		}

		public bool tuningPromode
		{
			get
			{
				return data.Get("settings-game-tuning-promode", d: false);
			}
			set
			{
				data.Set("settings-game-tuning-promode", value);
				Refresh();
			}
		}

		public int propwash
		{
			get
			{
				return data.Get("settings-game-propwash", 2);
			}
			set
			{
				data.Set("settings-game-propwash", value);
				Refresh();
			}
		}

		public bool crosshair
		{
			get
			{
				return data.Get("settings-game-crosshair", d: false);
			}
			set
			{
				data.Set("settings-game-crosshair", value);
				Refresh();
			}
		}

		public bool crossplay
		{
			get
			{
				return data.Get("settings-game-crossplay", d: true);
			}
			set
			{
				data.Set("settings-game-crossplay", value);
				Refresh();
			}
		}

		public bool chat
		{
			get
			{
				return data.Get("settings-game-chat", d: true);
			}
			set
			{
				data.Set("settings-game-chat", value);
				Refresh();
			}
		}

		public bool damage
		{
			get
			{
				return data.Get("settings-game-damage", d: false);
			}
			set
			{
				data.Set("settings-game-damage", value);
				Refresh();
			}
		}

		public bool hotkeys
		{
			get
			{
				return data.Get("settings-game-hotkeys", d: true);
			}
			set
			{
				data.Set("settings-game-hotkeys", value);
				Refresh();
			}
		}

		public int raceLineColor
		{
			get
			{
				return data.Get("settings-game-race-line-color", 4);
			}
			set
			{
				data.Set("settings-game-race-line-color", value);
				Refresh();
			}
		}

		public int raceMarkerColor
		{
			get
			{
				return data.Get("settings-game-check-point-color", 0);
			}
			set
			{
				data.Set("settings-game-check-point-color", value);
				Refresh();
			}
		}

		public int checkPointColor
		{
			get
			{
				return data.Get("settings-game-check-point-color", 0);
			}
			set
			{
				data.Set("settings-game-check-point-color", value);
				Refresh();
			}
		}

		public void Refresh()
		{
			if ((bool)parent)
			{
				parent.Refresh();
			}
		}
	}
}
