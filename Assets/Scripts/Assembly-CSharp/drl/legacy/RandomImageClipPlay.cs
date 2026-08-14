using UnityEngine;
using thelab.core;

namespace drl.legacy
{
	public class RandomImageClipPlay : MonoBehaviour
	{
		public ImageClip clip;

		public float minWait;

		public float maxWait;

		protected void Awake()
		{
			clip = GetComponent<ImageClip>();
			if ((bool)clip)
			{
				Activity.RunOnce(OnTimeOut, Random.Range(minWait, maxWait));
			}
		}

		protected void OnTimeOut()
		{
			if ((bool)clip)
			{
				clip.Stop();
				clip.Play();
			}
			Activity.RunOnce(OnTimeOut, Random.Range(minWait, maxWait));
		}
	}
}
