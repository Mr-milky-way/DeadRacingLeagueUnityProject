using System;

namespace thelab.core
{
	[Serializable]
	public class WebImageEvent
	{
		public WebImage target;

		public float progress;

		public string error;
	}
}
