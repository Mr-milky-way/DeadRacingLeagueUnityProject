using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class BundleModel : Model<DRLApp>
	{
		public DRLBundleEntry[] entries;

		public DRLBundleEntry[] updates;

		public DRLBundleEntry[] deletions;

		public static string entriesCachePath => DRLPaths.Storage.libraryRoot + "bundles.entries";

		public DRLBundleEntry[] LoadCache()
		{
			bool flag = false;
			DRLBundleEntry[] result = new DRLBundleEntry[0];
			if (File.Exists(entriesCachePath))
			{
				flag = true;
				byte[] p_data = File.ReadAllBytes(entriesCachePath);
				try
				{
					result = Serialize.FromBytes<DRLBundleEntry[]>(p_data);
				}
				catch
				{
				}
			}
			Debug.Log("BundleModel> LoadCache - path[" + entriesCachePath + "] exists[" + flag + "]");
			return result;
		}

		public void SaveCache()
		{
			DRLBundleEntry[] p_data = ((entries == null) ? new DRLBundleEntry[0] : entries);
			try
			{
				byte[] bytes = Serialize.ToBytes(p_data);
				File.WriteAllBytes(entriesCachePath, bytes);
				Debug.Log("BundleModel> SaveCache [" + entriesCachePath + "]");
			}
			catch (Exception ex)
			{
				Debug.Log("BundleModel> SaveCache / error[" + ex.Message + "]");
			}
		}

		public void UpdateVersioning()
		{
			DRLBundleEntry[] array = LoadCache();
			List<DRLBundleEntry> list = new List<DRLBundleEntry>();
			list = new List<DRLBundleEntry>();
			DRLBundleEntry[] array2 = entries;
			DRLBundleEntry[] array3 = array;
			foreach (DRLBundleEntry dRLBundleEntry in array2)
			{
				bool flag = false;
				bool flag2 = false;
				foreach (DRLBundleEntry dRLBundleEntry2 in array3)
				{
					flag = dRLBundleEntry.id == dRLBundleEntry2.id;
					if (flag)
					{
						flag2 = dRLBundleEntry.version != dRLBundleEntry2.version;
						break;
					}
				}
				if (flag2 || !flag)
				{
					list.Add(dRLBundleEntry);
				}
			}
			updates = list.ToArray();
			list.Clear();
			list = new List<DRLBundleEntry>();
			array2 = array;
			array3 = entries;
			foreach (DRLBundleEntry dRLBundleEntry3 in array2)
			{
				bool flag = false;
				foreach (DRLBundleEntry dRLBundleEntry4 in array3)
				{
					flag = dRLBundleEntry3.id == dRLBundleEntry4.id;
					if (flag)
					{
						break;
					}
				}
				if (!flag)
				{
					list.Add(dRLBundleEntry3);
				}
			}
			deletions = list.ToArray();
			list.Clear();
			Debug.Log("BundleModel> UpdateVersioning - updates[" + updates.Length + "] deletions[" + deletions.Length + "]");
		}
	}
}
