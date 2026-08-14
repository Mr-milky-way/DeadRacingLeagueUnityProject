using System;
using UnityEngine;

namespace drl.game
{
	public class WWISEManager : MonoBehaviour
	{
		public bool dllFound;

		public bool dllChecked;

		public string dllError = "";

		private string m_wwiseVersion;

		protected void Awake()
		{
			IsReady();
		}

		public void SetRTPC(string p_event, float p_value, GameObject p_target = null)
		{
			if (IsReady() && IsReady())
			{
				AkSoundEngine.SetRTPCValue(p_event, p_value, p_target);
			}
		}

		public void SetSwitch(string s_group, string s_state, GameObject s_target = null)
		{
			if (IsReady() && IsReady())
			{
				AkSoundEngine.SetSwitch(s_group, s_state, s_target);
			}
		}

		public void SetState(string s_group, string s_state)
		{
			if (IsReady())
			{
				AkSoundEngine.SetState(s_group, s_state);
			}
		}

		public bool PostEvent(string p_event, GameObject p_target = null)
		{
			if (!IsReady())
			{
				return false;
			}
			GameObject in_gameObjectID = (p_target ? p_target : base.gameObject);
			return AkSoundEngine.PostEvent(p_event, in_gameObjectID) != 0;
		}

		protected bool IsReady()
		{
			if (dllChecked)
			{
				return dllFound;
			}
			dllFound = true;
			try
			{
				m_wwiseVersion = AkSoundEngine.WwiseVersion;
			}
			catch (Exception ex)
			{
				dllFound = false;
				dllError = "AudioView: AkSoundEngine init failed: " + ex.GetType().ToString() + " :: " + ex.Message;
			}
			dllChecked = true;
			if (!dllFound && dllError != "")
			{
				Debug.LogError("WWISEManager> DLL Not Found!!\n" + dllError);
			}
			return dllFound;
		}
	}
}
