using UnityEngine;

public class FPSmeter : MonoBehaviour
{
	public float updateInterval = 0.5f;

	private float lastInterval;

	private int frames;

	public static float fps;

	public bool showFPS;

	private void Start()
	{
		lastInterval = Time.realtimeSinceStartup;
		frames = 0;
	}

	private void OnGUI()
	{
		if (showFPS)
		{
			GUI.Label(new Rect(10f, 10f, 100f, 20f), (Mathf.Round(fps * 100f) / 100f).ToString() ?? "");
		}
	}

	private void Update()
	{
		frames++;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup > lastInterval + updateInterval)
		{
			fps = (float)frames / (realtimeSinceStartup - lastInterval);
			frames = 0;
			lastInterval = realtimeSinceStartup;
		}
	}
}
