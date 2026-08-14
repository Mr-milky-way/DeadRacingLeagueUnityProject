using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	[RequireComponent(typeof(Image))]
	public class WebImage : MonoBehaviour
	{
		public string url;

		public WebImageCallback OnEvent;
	}
}
