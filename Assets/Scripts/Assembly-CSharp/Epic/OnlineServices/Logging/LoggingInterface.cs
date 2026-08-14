using System;

namespace Epic.OnlineServices.Logging
{
	public static class LoggingInterface
	{
		public static Result SetCallback(LogMessageFunc callback)
		{
			LogMessageFuncInternal logMessageFuncInternal = LogMessageFuncInternalImplementation;
			Helper.AddStaticCallback("LogMessageFuncInternalImplementation", callback, logMessageFuncInternal);
			return Bindings.EOS_Logging_SetCallback(logMessageFuncInternal);
		}

		public static Result SetLogLevel(LogCategory logCategory, LogLevel logLevel)
		{
			return Bindings.EOS_Logging_SetLogLevel(logCategory, logLevel);
		}

		[MonoPInvokeCallback(typeof(LogMessageFuncInternal))]
		internal static void LogMessageFuncInternalImplementation(IntPtr message)
		{
			if (Helper.TryGetStaticCallback<LogMessageFunc>("LogMessageFuncInternalImplementation", out var callback))
			{
				Helper.TryMarshalGet<LogMessageInternal, LogMessage>(message, out var target);
				callback(target);
			}
		}
	}
}
