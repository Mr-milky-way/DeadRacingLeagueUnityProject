using System.Runtime.InteropServices;

namespace thelab
{
	public static class GCTool
	{
		[DllImport("__Internal")]
		public static extern void GC_disable();

		[DllImport("__Internal")]
		public static extern void GC_enable();
	}
}
