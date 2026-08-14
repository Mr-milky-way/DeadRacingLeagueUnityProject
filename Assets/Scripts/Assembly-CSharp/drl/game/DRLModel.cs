using thelab.mvc;

namespace drl.game
{
	public class DRLModel : Model<DRLApp>
	{
		private static StorageModel m_storage;

		private static ServiceModel m_service;

		private static NetworkModel m_network;

		private static ChatModel m_chat;

		private static DRLOnboardingModel m_onboarding;

		private static DRLNotificationModel m_notifications;

		private static GameModel m_game;

		private static TournamentModel m_tournament;

		public StorageModel storage
		{
			get
			{
				return m_storage;
			}
			set
			{
				m_storage = value;
			}
		}

		public ServiceModel service
		{
			get
			{
				return m_service;
			}
			set
			{
				m_service = value;
			}
		}

		public NetworkModel network
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

		public ChatModel chat
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

		public DRLOnboardingModel onboarding
		{
			get
			{
				return m_onboarding;
			}
			set
			{
				m_onboarding = value;
			}
		}

		public DRLNotificationModel notifications
		{
			get
			{
				return m_notifications;
			}
			set
			{
				m_notifications = value;
			}
		}

		public GameModel game
		{
			get
			{
				if (!m_game)
				{
					return m_game = AssertFind<GameModel>("game");
				}
				return m_game;
			}
		}

		public TournamentModel tournament
		{
			get
			{
				return m_tournament;
			}
			set
			{
				m_tournament = value;
			}
		}
	}
}
