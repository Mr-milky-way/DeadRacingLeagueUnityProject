using UnityEngine;
using thelab.core;

public class Capture : MonoBehaviour
{
	public string textureName = "ss-capture";

	public int count;

	public SerializedMethod[] callback;

	public void CaptureSnapshot()
	{
		Debug.Log("CAPTURE - " + textureName);
		ScreenCapture.CaptureScreenshot("Assets/" + textureName + count + ".png", 2);
		count++;
	}

	public void RefreshAssets()
	{
	}
}
