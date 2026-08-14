using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using thelab.core;

namespace drl.backend
{
	public class DRLManifestActionData : SerializedData
	{
		private List<DRLManifestFileData> m_files;

		public string typeString
		{
			get
			{
				return Get("type", "invalid");
			}
			set
			{
				Set("type", value);
			}
		}

		public ManifestActionType type => typeString.ToLower() switch
		{
			"added" => ManifestActionType.Add, 
			"updated" => ManifestActionType.Update, 
			"deleted" => ManifestActionType.Remove, 
			_ => ManifestActionType.Invalid, 
		};

		public List<DRLManifestFileData> files
		{
			get
			{
				if (m_files != null)
				{
					return m_files;
				}
				m_files = new List<DRLManifestFileData>();
				JArray jArray = Get<JArray>("files", null);
				if (jArray != null)
				{
					DRLManifestFileData[] collection = jArray.ToObject<DRLManifestFileData[]>();
					m_files.AddRange(collection);
				}
				return m_files;
			}
			set
			{
				if (m_files == null)
				{
					m_files = new List<DRLManifestFileData>();
				}
				m_files.Clear();
				m_files.AddRange(value);
				Set("files", m_files);
			}
		}
	}
}
