using System;

namespace Epic.OnlineServices.Metrics
{
	public sealed class MetricsInterface : Handle
	{
		public const int BeginplayersessionApiLatest = 1;

		public const int EndplayersessionApiLatest = 1;

		public MetricsInterface()
		{
		}

		public MetricsInterface(IntPtr innerHandle)
			: base(innerHandle)
		{
		}

		public Result BeginPlayerSession(BeginPlayerSessionOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<BeginPlayerSessionOptionsInternal, BeginPlayerSessionOptions>(ref target, options);
			Result result = Bindings.EOS_Metrics_BeginPlayerSession(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}

		public Result EndPlayerSession(EndPlayerSessionOptions options)
		{
			IntPtr target = IntPtr.Zero;
			Helper.TryMarshalSet<EndPlayerSessionOptionsInternal, EndPlayerSessionOptions>(ref target, options);
			Result result = Bindings.EOS_Metrics_EndPlayerSession(base.InnerHandle, target);
			Helper.TryMarshalDispose(ref target);
			return result;
		}
	}
}
