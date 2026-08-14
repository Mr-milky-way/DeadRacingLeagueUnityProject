namespace drl.backend
{
	public class DRLSocketPacket
	{
		public string eventName = "unknown";

		public string message;

		public int attachments;

		public string nsp = "/";

		public int id = -1;

		public EnginePacketType enginePacketType = EnginePacketType.UNKNOWN;

		public SocketPacketType socketPacketType = SocketPacketType.UNKNOWN;

		private SocketPacketType p_stype;

		public DRLSocketPacket()
		{
		}

		public DRLSocketPacket(EnginePacketType p_enginePacketType)
		{
			enginePacketType = p_enginePacketType;
		}

		public DRLSocketPacket(string p_event, string p_message = null)
		{
			eventName = p_event;
			message = p_message;
			enginePacketType = EnginePacketType.MESSAGE;
			socketPacketType = SocketPacketType.EVENT;
		}

		public DRLSocketPacket(EnginePacketType p_enginePacketType, SocketPacketType p_socketPacketType)
		{
			enginePacketType = p_enginePacketType;
			socketPacketType = p_socketPacketType;
		}

		public DRLSocketPacket(EnginePacketType p_enginePacketType, SocketPacketType p_socketPacketType, string p_event, string p_message = null, int p_attachements = 0, string p_namespace = "/", int p_id = -1)
		{
			enginePacketType = p_enginePacketType;
			socketPacketType = p_socketPacketType;
			attachments = p_attachements;
			nsp = p_namespace;
			id = p_id;
			message = p_message;
			eventName = p_event;
		}

		public override string ToString()
		{
			if (message != null)
			{
				return "DRLSocketPacket: " + eventName + " - " + message;
			}
			return "DRLSocketPacket: " + eventName;
		}
	}
}
