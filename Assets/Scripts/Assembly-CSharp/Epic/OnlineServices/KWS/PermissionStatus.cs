namespace Epic.OnlineServices.KWS
{
	public class PermissionStatus : ISettable
	{
		public string Name { get; set; }

		public KWSPermissionStatus Status { get; set; }

		internal void Set(PermissionStatusInternal? other)
		{
			if (other.HasValue)
			{
				Name = other.Value.Name;
				Status = other.Value.Status;
			}
		}

		public void Set(object other)
		{
			Set(other as PermissionStatusInternal?);
		}
	}
}
