namespace drl.analytics
{
	public class GATryouts : IGAModule
	{
		public string Id => "tryouts";

		public void Registered()
		{
			CompletedStep("registration");
		}

		public void CompletedStep(string stepIdentifier)
		{
			if (!string.IsNullOrEmpty(stepIdentifier))
			{
				GADesign.DesignEvent(Id + ":" + stepIdentifier);
			}
		}
	}
}
