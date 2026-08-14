using System;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class SocialModel : Model<DRLApp>
	{
		public ServiceModel service => base.app.model.service;

		public SocialFriendsModel friends => Assert<SocialFriendsModel>("friends");

		public void GetTwitchLiveStatus(Action<bool> p_callback)
		{
			service.backend.GetTwitchLiveStatus(delegate(DRLServiceResult p_result)
			{
				SerializedData serializedData = (p_result.success ? p_result.GetData<SerializedData>() : null);
				string text = ((serializedData == null) ? "" : serializedData.Get("type", "offline"));
				bool obj = !string.IsNullOrEmpty(text) && text == "live";
				if (p_callback != null)
				{
					p_callback(obj);
				}
			});
		}

		public void GetUserOnlineCount(Action<int> p_callback)
		{
			service.backend.GetOnlineUserCount(delegate(DRLServiceResult p_result)
			{
				int obj = (p_result.success ? p_result.GetData<SerializedData>() : null)?.Get("count", 0) ?? (-1);
				if (p_callback != null)
				{
					p_callback(obj);
				}
			});
		}
	}
}
