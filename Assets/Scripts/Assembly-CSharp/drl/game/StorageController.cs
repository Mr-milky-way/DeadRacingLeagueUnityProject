using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class StorageController : Controller<DRLApp>
	{
		private static bool m_localization_loaded;

		public StorageModel model => AssertLocal<StorageModel>("model");

		protected override void Start()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "boot@complete":
				model.state.server.NotifyTime();
				LoadLocalization();
				break;
			case "service.state.game@refresh":
			{
				Dictionary<string, string> data2 = (p_data[0] as DRLServiceResult).GetData<Dictionary<string, string>>();
				Dictionary<string, string> hash2 = model.state.server.data.hash;
				hash2.Merge(data2);
				model.state.server.data.hash = hash2;
				Notify("storage.game.state@parse");
				break;
			}
			case "service.state@refresh":
			{
				Dictionary<string, string> data3 = (p_data[0] as DRLServiceResult).GetData<Dictionary<string, string>>();
				_ = model.state.player.data.hash;
				model.state.player.data.hash = data3;
				model.state.player.profile.photo = base.app.model.service.platform.playerThumbBig;
				Notify("storage.state@parse");
				break;
			}
			case "storage.state@refresh":
				if (!DRLApp.offline)
				{
					List<string> dataKeyChangeList = model.state.player.dataKeyChangeList;
					Dictionary<string, string> hash = model.state.player.data.GetHash(dataKeyChangeList);
					Debug.Log(string.Format("StorageController> Storage.StateRefresh / key-count[{0}]\n{1}", dataKeyChangeList.Count, string.Join("\n", dataKeyChangeList.ToArray())));
					base.app.model.service.State(delegate(DRLServiceResult p_result)
					{
						if (p_result.success)
						{
							Notify("storage.state@write");
						}
					}, hash);
				}
				CacheStateLocally();
				Notify("storage.state@parse");
				break;
			case "storage.license@refresh":
				base.app.model.service.License(delegate(DRLLicenseResult p_result)
				{
					if (p_result != null)
					{
						bool exists = p_result.exists;
						base.app.model.storage.state.license.exists = exists;
					}
				});
				break;
			case "storage.license@change":
				Debug.LogWarning("StorageController> License has changed!");
				base.app.model.storage.SetLibraryByLicense(base.app.model.storage.state.license.exists);
				base.app.view.ui.fade.FadeIn(1.5f);
				base.app.arguments.Clear();
				Activity.RunOnce(delegate
				{
					base.app.scene.LoadMain(p_force: true);
				}, 1.7f);
				break;
			case "service.time@refresh":
			{
				DRLServiceResult dRLServiceResult = p_data[0] as DRLServiceResult;
				if (!dRLServiceResult.success)
				{
					base.app.view.ui.footer.timeField.text = "-- : -- : --";
					break;
				}
				Dictionary<string, string> data = dRLServiceResult.GetData<Dictionary<string, string>>();
				if (data == null)
				{
					base.app.view.ui.footer.timeField.text = "-- : -- : --";
					break;
				}
				string text = data["time"];
				text = text.Substring(0, text.LastIndexOf("-"));
				text = text.Replace("-", "/");
				text = text.Replace("T", " ");
				DateTime result = DateTime.Now;
				if (!DateTime.TryParseExact(text, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
				{
					Debug.LogError("StorageController:: TimeRefresh:: invalid date format [" + text + "] expected [yyyy/MM/dd HH:mm:ss]");
					break;
				}
				model.state.server.time = result;
				if ((bool)base.app.view.ui && (bool)base.app.view.ui.footer)
				{
					base.app.view.ui.footer.date = result;
				}
				break;
			}
			case "state.time@refresh":
				if ((bool)base.app.view.ui && (bool)base.app.view.ui.footer)
				{
					base.app.view.ui.footer.date = model.state.server.GetTime();
				}
				break;
			}
		}

		public void OnPersistency()
		{
			base.app.controller.storage = this;
		}

		protected void LoadLocalization()
		{
			if (m_localization_loaded)
			{
				return;
			}
			m_localization_loaded = true;
			List<Transform> list = model.library.FindAll<Transform>();
			for (int i = 0; i < list.Count; i++)
			{
				_ = (bool)list[i].gameObject.GetComponent<DRLQuest>();
			}
			int num = 0;
			for (int j = 0; j < list.Count; j++)
			{
				ILocaleElement component = list[j].gameObject.GetComponent<ILocaleElement>();
				if (component != null)
				{
					if (component is LocaleElement)
					{
						(component as LocaleElement).manager = null;
					}
					num++;
					Localization.Add(component);
				}
			}
			Debug.Log("StorageController> Found [" + num + "] Locale Elements");
		}

		protected void CacheStateLocally()
		{
			Dictionary<string, object> d = model.state.player.data.data;
			if (!d.ContainsKey("player-id") || string.IsNullOrEmpty((string)d["player-id"]))
			{
				return;
			}
			string playerStateFileLocation = DRLPaths.Storage.offlinePlayerStateFile;
			new Thread((ThreadStart)delegate
			{
				string text = Serialize.ToJson(d);
				if (!string.IsNullOrEmpty(text) && !(text.Trim() == "{}"))
				{
					File.WriteAllText(playerStateFileLocation, text);
				}
			}).Start();
		}
	}
}
