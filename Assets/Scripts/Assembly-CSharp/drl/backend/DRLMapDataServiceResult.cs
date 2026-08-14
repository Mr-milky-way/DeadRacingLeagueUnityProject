using System.Runtime.Serialization;

namespace drl.backend
{
	public class DRLMapDataServiceResult
	{
		[OptionalField]
		public string id;

		public bool success;

		public bool encoded = true;

		[OptionalField]
		public string message;

		[OptionalField]
		public string token;

		[OptionalField]
		public string webtoken;

		public DRLMapDataResult data;
	}
}
