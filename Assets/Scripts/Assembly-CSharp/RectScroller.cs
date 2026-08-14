using System.Collections;
using UnityEngine;

public class RectScroller : MonoBehaviour
{
	private RectTransform rect;

	private RectTransform rectParent;

	private Coroutine worker;

	private Vector2 spectatorOffset = new Vector2(53f, 0f);

	public bool ScrollActive => worker != null;

	public RectTransform Rect => rect;

	private void Awake()
	{
		rect = GetComponent<RectTransform>();
		rectParent = rect.parent as RectTransform;
	}

	private void OnDisable()
	{
		if (worker != null)
		{
			StopCoroutine(worker);
			worker = null;
		}
	}

	public void SetScrollState(bool _scrollActive, float p_offset, bool isSpectator)
	{
		if (!_scrollActive && worker != null)
		{
			StopCoroutine(worker);
			worker = null;
			rect.anchoredPosition = Vector2.zero;
		}
		else if (_scrollActive && worker == null)
		{
			worker = StartCoroutine(ScrollerRoutine(p_offset));
			if (isSpectator)
			{
				rectParent.SetAsFirstSibling();
			}
		}
	}

	private IEnumerator ScrollerRoutine(float p_offsetDelta, float p_durationSeconds = 5f)
	{
		p_offsetDelta = Mathf.Abs(p_offsetDelta);
		bool isReverse = false;
		while (true)
		{
			float done = 0f;
			float _startTime = Time.time;
			while (done < 1f)
			{
				done = (Time.time - _startTime) / p_durationSeconds;
				yield return null;
				rect.anchoredPosition = Vector2.left * Mathf.Lerp(isReverse ? p_offsetDelta : 0f, isReverse ? 0f : p_offsetDelta, done);
			}
			isReverse = !isReverse;
		}
	}
}
