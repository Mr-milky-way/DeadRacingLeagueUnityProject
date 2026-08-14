namespace foo
{
	public class ScriptGuide
	{
		public enum InternalEnum
		{
			ValueA = 0,
			OtherValueB = 1
		}

		public static int globalCounter;

		private static int m_hiddenCounter;

		public int classCounter;

		protected int m_classHiddenCounter;

		internal int scopeOnlyCounter;

		public int[] coords;

		static ScriptGuide()
		{
		}

		public static void StaticMethod()
		{
		}

		public void ScriptMethod(string p_name, int p_count, int p_another_var)
		{
		}

		private void ScriptHiddenMethod()
		{
		}
	}
}
