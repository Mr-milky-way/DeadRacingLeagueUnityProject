using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbySearchSetParameterOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Parameter;

		private ComparisonOp m_ComparisonOp;

		public AttributeData Parameter
		{
			set
			{
				Helper.TryMarshalSet<AttributeDataInternal, AttributeData>(ref m_Parameter, value);
			}
		}

		public ComparisonOp ComparisonOp
		{
			set
			{
				m_ComparisonOp = value;
			}
		}

		public void Set(LobbySearchSetParameterOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Parameter = other.Parameter;
				ComparisonOp = other.ComparisonOp;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbySearchSetParameterOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Parameter);
		}
	}
}
