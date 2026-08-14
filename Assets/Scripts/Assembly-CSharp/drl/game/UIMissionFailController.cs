using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMissionFailController : Controller<DRLApp>
	{
		public bool feedbackAlreadyGiven;

		public UIMissionFailView view => AssertLocal<UIMissionFailView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!view.current)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					view.Set(view.quest, view.mission);
					feedbackAlreadyGiven = IsFeedbackAlreadyGiven();
					view.feedbackContainer.gameObject.SetActive(!feedbackAlreadyGiven);
				}
				break;
			case "missions.mission-complete.exit@click":
				ProcessFeedbackPoll();
				base.enabled = false;
				base.app.view.audio.PlayUIGenericSuccess();
				base.app.controller.game.Exit();
				break;
			case "missions.mission-complete.quests@click":
				ProcessFeedbackPoll();
				base.app.view.ui.screens.Open<UIQuestsView>("train-menu-screen");
				break;
			case "missions.mission-complete.next@click":
				ProcessFeedbackPoll();
				if (view.mission == view.quest.missions.Last())
				{
					List<DRLQuest> list = base.app.model.storage.library.FindAll<DRLQuest>();
					for (int i = 0; i < list.Count - 1; i++)
					{
						if (list[i] == view.quest)
						{
							view.quest = list[i + 1];
							break;
						}
					}
					base.app.controller.game.NextQuest(view.quest);
				}
				else
				{
					DRLQuest quest = view.quest;
					DRLMission mission = view.mission;
					base.app.controller.game.NextMission(quest, mission);
				}
				break;
			case "missions.mission-complete.restart@click":
				ProcessFeedbackPoll();
				base.enabled = false;
				base.app.view.audio.PlayUIGenericSuccess();
				base.app.controller.game.Restart();
				break;
			}
		}

		private bool IsFeedbackAlreadyGiven()
		{
			List<PollResultModel> polls = base.app.model.storage.state.player.polls;
			DRLMission mission = view.mission;
			foreach (PollResultModel item in polls)
			{
				if (item != null && item.playerId == base.app.model.service.backend.playerId && item.mission == mission.guid)
				{
					return true;
				}
			}
			return false;
		}

		private bool HasValidFeedbackAnswers()
		{
			string text = "<please select>";
			if (view.feedbackImprovedSkills.value.ToLower() == text)
			{
				return false;
			}
			if (view.feedbackEnjoyedMission.value.ToLower() == text)
			{
				return false;
			}
			if (view.feedbackPilotExp.value.ToLower() == text)
			{
				return false;
			}
			return true;
		}

		protected void ProcessFeedbackPoll()
		{
			if (HasValidFeedbackAnswers() && !feedbackAlreadyGiven)
			{
				feedbackAlreadyGiven = true;
				PollResultModel pollResultModel = new PollResultModel();
				DRLMission mission = view.mission;
				pollResultModel.type = GameFlag.Mission;
				pollResultModel.mode = GameFlag.SinglePlayer;
				pollResultModel.mission = mission.guid;
				pollResultModel.playerId = base.app.model.service.backend.playerId;
				pollResultModel.timestamp = base.app.model.storage.state.server.GetTime().ToString();
				pollResultModel.score = Mathf.FloorToInt(view.score * 1000f);
				List<PollResultModel.Entry> entries = pollResultModel.entries;
				entries.Add(new PollResultModel.Entry("are-you-an-experienced-pilot", view.feedbackPilotExp.value));
				entries.Add(new PollResultModel.Entry("did-you-enjoy-this-mission", view.feedbackEnjoyedMission.value));
				entries.Add(new PollResultModel.Entry("did-this-help-improve-your-skills", view.feedbackImprovedSkills.value));
				entries.Add(new PollResultModel.Entry("feedback-message", view.feedbackMessage.inputText.text));
				pollResultModel.entries = entries;
				List<PollResultModel> polls = base.app.model.storage.state.player.polls;
				polls.Add(pollResultModel);
				base.app.model.storage.state.player.polls = polls;
			}
		}
	}
}
