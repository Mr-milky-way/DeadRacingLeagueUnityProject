using System;

namespace Epic.OnlineServices.P2P
{
	public sealed class P2PInterface : Handle
	{
		public const int AcceptconnectionApiLatest = 1;

		public const int AddnotifyincomingpacketqueuefullApiLatest = 1;

		public const int AddnotifypeerconnectionclosedApiLatest = 1;

		public const int AddnotifypeerconnectionrequestApiLatest = 1;

		public const int CloseconnectionApiLatest = 1;

		public const int CloseconnectionsApiLatest = 1;

		public const int GetnattypeApiLatest = 1;

		public const int GetnextreceivedpacketsizeApiLatest = 2;

		public const int GetpacketqueueinfoApiLatest = 1;

		public const int GetportrangeApiLatest = 1;

		public const int GetrelaycontrolApiLatest = 1;

		public const int MaxConnections = 32;

		public const int MaxPacketSize = 1170;

		public const int MaxQueueSizeUnlimited = 0;

		public const int QuerynattypeApiLatest = 1;

		public const int ReceivepacketApiLatest = 2;

		public const int SendpacketApiLatest = 2;

		public const int SetpacketqueuesizeApiLatest = 1;

		public const int SetportrangeApiLatest = 1;

		public const int SetrelaycontrolApiLatest = 1;

		public const int SocketidApiLatest = 1;

		public P2PInterface()
		{
		}

		public P2PInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result AcceptConnection(AcceptConnectionOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AcceptConnectionOptionsInternal, AcceptConnectionOptions>(ref target, options);
			Result result = Bindings.EOS_P2P_AcceptConnection(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public ulong AddNotifyIncomingPacketQueueFull(AddNotifyIncomingPacketQueueFullOptions options, object clientData, OnIncomingPacketQueueFullCallback incomingPacketQueueFullHandler)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyIncomingPacketQueueFullOptionsInternal, AddNotifyIncomingPacketQueueFullOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnIncomingPacketQueueFullCallbackInternal onIncomingPacketQueueFullCallbackInternal = OnIncomingPacketQueueFullCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, incomingPacketQueueFullHandler, onIncomingPacketQueueFullCallbackInternal);
			ulong num = Bindings.EOS_P2P_AddNotifyIncomingPacketQueueFull(base.InnerHandle, target, clientDataAddress, onIncomingPacketQueueFullCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifyPeerConnectionClosed(AddNotifyPeerConnectionClosedOptions options, object clientData, OnRemoteConnectionClosedCallback connectionClosedHandler)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyPeerConnectionClosedOptionsInternal, AddNotifyPeerConnectionClosedOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnRemoteConnectionClosedCallbackInternal onRemoteConnectionClosedCallbackInternal = OnRemoteConnectionClosedCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, connectionClosedHandler, onRemoteConnectionClosedCallbackInternal);
			ulong num = Bindings.EOS_P2P_AddNotifyPeerConnectionClosed(base.InnerHandle, target, clientDataAddress, onRemoteConnectionClosedCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public ulong AddNotifyPeerConnectionRequest(AddNotifyPeerConnectionRequestOptions options, object clientData, OnIncomingConnectionRequestCallback connectionRequestHandler)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<AddNotifyPeerConnectionRequestOptionsInternal, AddNotifyPeerConnectionRequestOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnIncomingConnectionRequestCallbackInternal onIncomingConnectionRequestCallbackInternal = OnIncomingConnectionRequestCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, connectionRequestHandler, onIncomingConnectionRequestCallbackInternal);
			ulong num = Bindings.EOS_P2P_AddNotifyPeerConnectionRequest(base.InnerHandle, target, clientDataAddress, onIncomingConnectionRequestCallbackInternal);
			Helper.TryMarshalDispose(ref target);
			Helper.TryAssignNotificationIdToCallback(clientDataAddress, num);
			return num;
		}

		public Result CloseConnection(CloseConnectionOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CloseConnectionOptionsInternal, CloseConnectionOptions>(ref target, options);
			Result result = Bindings.EOS_P2P_CloseConnection(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result CloseConnections(CloseConnectionsOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<CloseConnectionsOptionsInternal, CloseConnectionsOptions>(ref target, options);
			Result result = Bindings.EOS_P2P_CloseConnections(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result GetNATType(GetNATTypeOptions options, out NATType outNATType)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetNATTypeOptionsInternal, GetNATTypeOptions>(ref target, options);
			outNATType = Helper.GetDefault<NATType>();
			Result result = Bindings.EOS_P2P_GetNATType(base.InnerHandle, target, ref outNATType);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result GetNextReceivedPacketSize(GetNextReceivedPacketSizeOptions options, out uint outPacketSizeBytes)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetNextReceivedPacketSizeOptionsInternal, GetNextReceivedPacketSizeOptions>(ref target, options);
			outPacketSizeBytes = Helper.GetDefault<uint>();
			Result result = Bindings.EOS_P2P_GetNextReceivedPacketSize(base.InnerHandle, target, ref outPacketSizeBytes);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result GetPacketQueueInfo(GetPacketQueueInfoOptions options, out PacketQueueInfo outPacketQueueInfo)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetPacketQueueInfoOptionsInternal, GetPacketQueueInfoOptions>(ref target, options);
			PacketQueueInfoInternal outPacketQueueInfo2 = Helper.GetDefault<PacketQueueInfoInternal>();
			Result result = Bindings.EOS_P2P_GetPacketQueueInfo(base.InnerHandle, target, ref outPacketQueueInfo2);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet<PacketQueueInfoInternal, PacketQueueInfo>(outPacketQueueInfo2, out outPacketQueueInfo);
			return result;
		}

		public Result GetPortRange(GetPortRangeOptions options, out ushort outPort, out ushort outNumAdditionalPortsToTry)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetPortRangeOptionsInternal, GetPortRangeOptions>(ref target, options);
			outPort = Helper.GetDefault<ushort>();
			outNumAdditionalPortsToTry = Helper.GetDefault<ushort>();
			Result result = Bindings.EOS_P2P_GetPortRange(base.InnerHandle, target, ref outPort, ref outNumAdditionalPortsToTry);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result GetRelayControl(GetRelayControlOptions options, out RelayControl outRelayControl)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<GetRelayControlOptionsInternal, GetRelayControlOptions>(ref target, options);
			outRelayControl = Helper.GetDefault<RelayControl>();
			Result result = Bindings.EOS_P2P_GetRelayControl(base.InnerHandle, target, ref outRelayControl);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public void QueryNATType(QueryNATTypeOptions options, object clientData, OnQueryNATTypeCompleteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<QueryNATTypeOptionsInternal, QueryNATTypeOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnQueryNATTypeCompleteCallbackInternal onQueryNATTypeCompleteCallbackInternal = OnQueryNATTypeCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onQueryNATTypeCompleteCallbackInternal);
			Bindings.EOS_P2P_QueryNATType(base.InnerHandle, target, clientDataAddress, onQueryNATTypeCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		public Result ReceivePacket(ReceivePacketOptions options, out ProductUserId outPeerId, out SocketId outSocketId, out byte outChannel, out byte[] outData)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<ReceivePacketOptionsInternal, ReceivePacketOptions>(ref target, options);
			IntPtr outPeerId2 = IntPtr.Zero;
			SocketIdInternal outSocketId2 = Helper.GetDefault<SocketIdInternal>();
			outChannel = Helper.GetDefault<byte>();
			IntPtr target2 = IntPtr.Zero;
			uint outBytesWritten = 1170u;
			Helper.TryMarshalAllocate(ref target2, outBytesWritten, out var _);
			Result result = Bindings.EOS_P2P_ReceivePacket(base.InnerHandle, target, ref outPeerId2, ref outSocketId2, ref outChannel, target2, ref outBytesWritten);
			Helper.TryMarshalDispose(ref target);
			Helper.TryMarshalGet(outPeerId2, out outPeerId);
			Helper.TryMarshalGet<SocketIdInternal, SocketId>(outSocketId2, out outSocketId);
			Helper.TryMarshalGet(target2, out outData, outBytesWritten);
			Helper.TryMarshalDispose(ref target2);
			return result;
		}

		public void RemoveNotifyIncomingPacketQueueFull(ulong notificationId)
		{
			Helper.TryRemoveCallbackByNotificationId(notificationId);
			Bindings.EOS_P2P_RemoveNotifyIncomingPacketQueueFull(base.InnerHandle, notificationId);
		}

		public void RemoveNotifyPeerConnectionClosed(ulong notificationId)
		{
			Helper.TryRemoveCallbackByNotificationId(notificationId);
			Bindings.EOS_P2P_RemoveNotifyPeerConnectionClosed(base.InnerHandle, notificationId);
		}

		public void RemoveNotifyPeerConnectionRequest(ulong notificationId)
		{
			Helper.TryRemoveCallbackByNotificationId(notificationId);
			Bindings.EOS_P2P_RemoveNotifyPeerConnectionRequest(base.InnerHandle, notificationId);
		}

		public Result SendPacket(SendPacketOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SendPacketOptionsInternal, SendPacketOptions>(ref target, options);
			Result result = Bindings.EOS_P2P_SendPacket(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetPacketQueueSize(SetPacketQueueSizeOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SetPacketQueueSizeOptionsInternal, SetPacketQueueSizeOptions>(ref target, options);
			Result result = Bindings.EOS_P2P_SetPacketQueueSize(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetPortRange(SetPortRangeOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SetPortRangeOptionsInternal, SetPortRangeOptions>(ref target, options);
			Result result = Bindings.EOS_P2P_SetPortRange(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result SetRelayControl(SetRelayControlOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SetRelayControlOptionsInternal, SetRelayControlOptions>(ref target, options);
			Result result = Bindings.EOS_P2P_SetRelayControl(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		[MonoPInvokeCallback(typeof(OnIncomingConnectionRequestCallbackInternal))]
		internal static void OnIncomingConnectionRequestCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnIncomingConnectionRequestCallback, OnIncomingConnectionRequestInfoInternal, OnIncomingConnectionRequestInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnIncomingPacketQueueFullCallbackInternal))]
		internal static void OnIncomingPacketQueueFullCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnIncomingPacketQueueFullCallback, OnIncomingPacketQueueFullInfoInternal, OnIncomingPacketQueueFullInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnQueryNATTypeCompleteCallbackInternal))]
		internal static void OnQueryNATTypeCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnQueryNATTypeCompleteCallback, OnQueryNATTypeCompleteInfoInternal, OnQueryNATTypeCompleteInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}

		[MonoPInvokeCallback(typeof(OnRemoteConnectionClosedCallbackInternal))]
		internal static void OnRemoteConnectionClosedCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnRemoteConnectionClosedCallback, OnRemoteConnectionClosedInfoInternal, OnRemoteConnectionClosedInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
