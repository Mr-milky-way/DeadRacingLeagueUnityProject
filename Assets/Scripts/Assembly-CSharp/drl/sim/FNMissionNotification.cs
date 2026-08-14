namespace drl.sim
{
	public class FNMissionNotification
	{
		public const string MissionComplete = "fn.mission@complete";

		public const string MissionFail = "fn.mission@fail";

		public const string TargetHit = "fn.mission.target@hit";

		public const string DroneSpawn = "fn.mission.drone.spawn";

		public const string DroneRescue = "fn.mission.drone.rescue";

		public const string PrecisionStart = "fn.mission.precision@start";

		public const string PrecisionUpdate = "fn.mission.precision@update";

		public const string PrecisionStop = "fn.mission.precision@stop";

		public const string BalloonRadarStart = "fn.mission.balloonradar@start";

		public const string BalloonRadarStop = "fn.mission.balloonradar@stop";

		public const string PhysicsIntroStep1Start = "fn.mission.physicsintrostep1@start";

		public const string PhysicsIntroStep2Start = "fn.mission.physicsintrostep2@start";

		public const string PhysicsIntroStep3Start = "fn.mission.physicsintrostep3@start";

		public const string PhysicsIntroStep4Start = "fn.mission.physicsintrostep4@start";

		public const string DroneCollision = "fn.mission.drone@collision";

		public const string VideoFinished = "fn.mission.video-player@end";
	}
}
