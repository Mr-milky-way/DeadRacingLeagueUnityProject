using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class LicenseStateModel : Model<DRLApp>
	{
		internal static bool m_exists;

		private Activity m_refresh_timer;

		public bool exists
		{
			get
			{
				return m_exists;
			}
			set
			{
				if (m_exists != value)
				{
					m_exists = value;
					Notify(1f / 30f, "storage.license@change");
				}
			}
		}

		public void Poll()
		{
		}
	}
}
