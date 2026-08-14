using thelab.mvc;

namespace drl.game
{
	public class DRLView : View<DRLApp>
	{
		private static UIView m_ui;

		private static NetworkView m_network;

		private static ChatView m_chat;

		private static AudioView m_audio;

		public UIView ui
		{
			get
			{
				if (!m_ui)
				{
					return m_ui = Assert<UIView>("ui");
				}
				return m_ui;
			}
		}

		public NetworkView network
		{
			get
			{
				return m_network;
			}
			set
			{
				m_network = value;
			}
		}

		public ChatView chat
		{
			get
			{
				return m_chat;
			}
			set
			{
				m_chat = value;
			}
		}

		public AudioView audio
		{
			get
			{
				if (!m_audio)
				{
					return m_audio = Assert<AudioView>("audio");
				}
				return m_audio;
			}
		}
	}
}
