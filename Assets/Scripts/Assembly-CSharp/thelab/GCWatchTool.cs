using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace thelab
{
	public class GCWatchTool : MonoBehaviour
	{
		public Text gcCountText;

		public Text toggleButtonText;

		public Button toggleButton;

		public Text logText;

		private bool isGcDisabled;

		private List<byte[]> storage;

		private void Awake()
		{
			storage = new List<byte[]>();
			toggleButtonText.text = "Disable GC";
			toggleButton.onClick.AddListener(delegate
			{
				try
				{
					OnToggle();
				}
				catch (Exception)
				{
					logText.text = "FAIL";
					Activity.RunOnce(delegate
					{
						logText.text = "";
					}, 5f);
				}
			});
		}

		private void Update()
		{
			int num = GC.CollectionCount(0);
			gcCountText.text = num.ToString();
			storage.Add(new byte[512000]);
		}

		public void OnToggle()
		{
			isGcDisabled = !isGcDisabled;
			if (isGcDisabled)
			{
				GCTool.GC_disable();
				toggleButtonText.text = "Enable GC";
			}
			else
			{
				GCTool.GC_enable();
				toggleButtonText.text = "Disable GC";
			}
		}

		private void OnApplicationQuit()
		{
			if (isGcDisabled)
			{
				GCTool.GC_enable();
			}
		}
	}
}
