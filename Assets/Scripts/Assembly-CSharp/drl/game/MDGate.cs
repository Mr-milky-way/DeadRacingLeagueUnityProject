namespace drl.game
{
	public class MDGate : MDRenderer
	{
		public int index
		{
			get
			{
				return Get("gate-index", -1);
			}
			set
			{
				Set("gate-index", value);
			}
		}

		public MapGateMode gateMode
		{
			get
			{
				return (MapGateMode)Get("gate-mode", 1);
			}
			set
			{
				Set("gate-mode", (int)value);
			}
		}

		public bool isTrigger
		{
			get
			{
				return Get("is-trigger", d: true);
			}
			set
			{
				Set("is-trigger", value);
			}
		}

		public bool isFinish
		{
			get
			{
				return Get("is-finish", d: false);
			}
			set
			{
				Set("is-finish", value);
			}
		}

		public bool isLapStart
		{
			get
			{
				return Get("is-lap-start", d: false);
			}
			set
			{
				Set("is-lap-start", value);
			}
		}

		public bool isLapEnd
		{
			get
			{
				return Get("is-lap-end", d: false);
			}
			set
			{
				Set("is-lap-end", value);
			}
		}

		public bool isRespawnVisible
		{
			get
			{
				return Get("is-respawn-visible", d: true);
			}
			set
			{
				Set("is-respawn-visible", value);
			}
		}

		public MDGate()
		{
			base.type = MapAssetType.Gate;
		}
	}
}
