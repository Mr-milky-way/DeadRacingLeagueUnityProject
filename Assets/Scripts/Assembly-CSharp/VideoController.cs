using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
	private VideoPlayer videoPlayer;

	private void Awake()
	{
		videoPlayer = GetComponent<VideoPlayer>();
	}

	private void OnEnable()
	{
	}
}
