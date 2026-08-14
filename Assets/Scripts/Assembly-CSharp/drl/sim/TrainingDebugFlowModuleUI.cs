using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class TrainingDebugFlowModuleUI : FlowModuleUI
	{
		[Serializable]
		public class TrainingInfo
		{
			public string name;

			public string missionType;

			public string scene;

			public bool available;
		}

		public Transform uiScrollBackgound;

		public DebugTrainingHolder trainingHolderPrefab;

		public DebugQuestHolder questHolderPrefab;

		public List<TrainingInfo> trainingsMissionsList = new List<TrainingInfo>();

		public LevelManager levelManager;

		private void Start()
		{
			Vector2 vector = new Vector2(760f, -300f);
			Vector2 vector2 = new Vector2(0f, -425f);
			Vector2 vector3 = new Vector2(-695f, 0f);
			Vector2 vector4 = new Vector2(265f, 0f);
			levelManager = GameObject.Find("level-load").GetComponent<LevelManager>();
			int num = 0;
			foreach (IGrouping<string, TrainingInfo> item in from tm in trainingsMissionsList
				group tm by tm.missionType)
			{
				DebugQuestHolder debugQuestHolder = UnityEngine.Object.Instantiate(questHolderPrefab);
				debugQuestHolder.transform.SetParent(uiScrollBackgound);
				debugQuestHolder.transform.localScale = Vector3.one;
				debugQuestHolder.transform.localPosition = vector + vector2 * num++;
				debugQuestHolder.title.text = item.Key;
				int num2 = 0;
				foreach (TrainingInfo item2 in item)
				{
					DebugTrainingHolder debugTrainingHolder = UnityEngine.Object.Instantiate(trainingHolderPrefab);
					debugTrainingHolder.transform.SetParent(debugQuestHolder.holder);
					debugTrainingHolder.transform.localScale = Vector3.one;
					debugTrainingHolder.transform.localPosition = vector3 + vector4 * num2++;
					debugTrainingHolder.title.text = item2.name.Substring(0, item2.name.IndexOf(':') + 1);
					debugTrainingHolder.subTitle.text = item2.name.Substring(item2.name.IndexOf(':') + 2);
					if (item2.available)
					{
						string missionTitle = item2.name.Substring(0, item2.name.IndexOf(':') + 1);
						string sceneName = item2.scene;
						debugTrainingHolder.button.onClick.AddListener(delegate
						{
							Debug.Log("Starting training mission: " + missionTitle);
							levelManager.LoadLevel(sceneName);
						});
					}
					else
					{
						debugTrainingHolder.button.interactable = false;
						debugTrainingHolder.buttonText.text = "Under construction";
					}
				}
			}
		}

		public void QuitApplication()
		{
			Application.Quit();
		}
	}
}
