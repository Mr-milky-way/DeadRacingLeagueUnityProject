using System.Collections.Generic;
using UnityEngine;
using drl.network;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIVoteTrackController : Controller<DRLApp>
	{
		public NetworkModel model => base.app.model.network;

		public UIVoteTrackView view => AssertLocal<UIVoteTrackView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!view.isActiveAndEnabled)
			{
				return;
			}
			switch (p_event)
			{
			case "network.player.voted.track":
			{
				NetworkRoom room = base.app.model.network.room;
				if (room != null)
				{
					Dictionary<string, int> voteTrackTable = room.VoteTrackTable;
					view.Refresh(voteTrackTable);
					NetworkActor networkActor = Reflection<object>.Get<NetworkActor>(p_data, 0);
					if (networkActor != null && networkActor.IsLocal)
					{
						string votedTrackGUID = networkActor.VotedTrackGUID;
						view.HilightByGUID(votedTrackGUID);
					}
				}
				break;
			}
			case "ui.game.vote-track.card@click":
			{
				UICardButtonVoteTrack uICardButtonVoteTrack = p_target as UICardButtonVoteTrack;
				if ((bool)uICardButtonVoteTrack)
				{
					Debug.Log("UIVotaTrackController> Vote Click - track[" + uICardButtonVoteTrack.trackNameField?.ToString() + "] guid[" + uICardButtonVoteTrack.guid + "]");
					model.SendTrackVote(uICardButtonVoteTrack.guid);
				}
				break;
			}
			}
		}
	}
}
