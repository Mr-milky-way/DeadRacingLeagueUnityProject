using System;

namespace Epic.OnlineServices.Reports
{
	public sealed class ReportsInterface : Handle
	{
		public const int ReportcontextMaxLength = 4096;

		public const int ReportmessageMaxLength = 512;

		public const int SendplayerbehaviorreportApiLatest = 2;

		public ReportsInterface()
		{
		}

		public ReportsInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public void SendPlayerBehaviorReport(SendPlayerBehaviorReportOptions options, object clientData, OnSendPlayerBehaviorReportCompleteCallback completionDelegate)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<SendPlayerBehaviorReportOptionsInternal, SendPlayerBehaviorReportOptions>(ref target, options);
			IntPtr clientDataAddress = IntPtr.Zero;
			OnSendPlayerBehaviorReportCompleteCallbackInternal onSendPlayerBehaviorReportCompleteCallbackInternal = OnSendPlayerBehaviorReportCompleteCallbackInternalImplementation;
			Helper.AddCallback(ref clientDataAddress, clientData, completionDelegate, onSendPlayerBehaviorReportCompleteCallbackInternal);
			Bindings.EOS_Reports_SendPlayerBehaviorReport(base.InnerHandle, target, clientDataAddress, onSendPlayerBehaviorReportCompleteCallbackInternal);
			Helper.TryMarshalDispose(ref target);
		}

		[MonoPInvokeCallback(typeof(OnSendPlayerBehaviorReportCompleteCallbackInternal))]
		internal static void OnSendPlayerBehaviorReportCompleteCallbackInternalImplementation(IntPtr data)
		{
			if (Helper.TryGetAndRemoveCallback<OnSendPlayerBehaviorReportCompleteCallback, SendPlayerBehaviorReportCompleteCallbackInfoInternal, SendPlayerBehaviorReportCompleteCallbackInfo>(data, out var callback, out var callbackInfo))
			{
				callback(callbackInfo);
			}
		}
	}
}
