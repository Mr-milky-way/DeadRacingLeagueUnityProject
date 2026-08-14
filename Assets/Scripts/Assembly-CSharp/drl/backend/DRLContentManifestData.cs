using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using thelab.core;

namespace drl.backend
{
	public class DRLContentManifestData : SerializedData
	{
		private List<DRLManifestActionData> m_actions;

		private List<DRLManifestOperation> m_operations;

		public string id
		{
			get
			{
				return Get("id", "");
			}
			set
			{
				Set("id", value);
			}
		}

		public string name
		{
			get
			{
				return Get("name", "");
			}
			set
			{
				Set("name", value);
			}
		}

		public string branch
		{
			get
			{
				return Get("branch", "");
			}
			set
			{
				Set("branch", value);
			}
		}

		public string platform
		{
			get
			{
				return Get("platform", "");
			}
			set
			{
				Set("platform", value);
			}
		}

		public string version
		{
			get
			{
				return Get("version", "");
			}
			set
			{
				Set("version", value);
			}
		}

		public string createdAtString
		{
			get
			{
				return Get("created-at", "");
			}
			set
			{
				Set("created-at", value);
			}
		}

		public DateTime createdAt => DateTime.Parse(createdAtString);

		public List<DRLManifestActionData> actions
		{
			get
			{
				if (m_actions != null)
				{
					return m_actions;
				}
				m_actions = new List<DRLManifestActionData>();
				JArray jArray = Get<JArray>("actions", null);
				if (jArray != null)
				{
					DRLManifestActionData[] collection = jArray.ToObject<DRLManifestActionData[]>();
					m_actions.AddRange(collection);
				}
				return m_actions;
			}
			set
			{
				if (m_actions == null)
				{
					m_actions = new List<DRLManifestActionData>();
				}
				m_actions.Clear();
				m_actions.AddRange(value);
				Set("actions", m_actions);
			}
		}

		public List<DRLManifestOperation> GetOperations()
		{
			if (m_operations != null)
			{
				return m_operations;
			}
			List<DRLManifestOperation> list = (m_operations = new List<DRLManifestOperation>());
			List<DRLManifestActionData> list2 = actions;
			for (int i = 0; i < list2.Count; i++)
			{
				DRLManifestActionData dRLManifestActionData = list2[i];
				if (dRLManifestActionData.type != ManifestActionType.Invalid)
				{
					List<DRLManifestFileData> files = dRLManifestActionData.files;
					for (int j = 0; j < files.Count; j++)
					{
						DRLManifestFileData file = files[j];
						DRLManifestOperation item = new DRLManifestOperation
						{
							type = dRLManifestActionData.type,
							file = file
						};
						list.Add(item);
					}
				}
			}
			return list;
		}
	}
}
