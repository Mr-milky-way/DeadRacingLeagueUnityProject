using System.Collections.Generic;
using System.IO;
using UnityEngine;
using drl.game;
using thelab.core;

namespace drl.sim
{
	public class DebugReplayInputAndFlightpath : MonoBehaviour
	{
		private Drone drone;

		private bool isPlaying;

		public bool arm;

		public bool startPlayback;

		public Vector3 scaleSignals = Vector3.one;

		private List<Vector3> positions = new List<Vector3>(10000);

		private List<Quaternion> rotations = new List<Quaternion>(10000);

		private List<Quaternion> signals = new List<Quaternion>(10000);

		private List<float> timings = new List<float>(10000);

		private List<Vector3> angularVelocity = new List<Vector3>(10000);

		private float startTime;

		public string recordingFilename;

		private List<Vector3> pathPositions = new List<Vector3>(10000);

		private List<Quaternion> pathRotations = new List<Quaternion>(10000);

		private int frame;

		private LineRenderer original;

		private LineRenderer recreated;

		private Transform startPosition;

		private AnimationCurve throttle;

		private AnimationCurve pitch;

		private AnimationCurve roll;

		private AnimationCurve yaw;

		public bool useCurves;

		public bool drawPaths = true;

		public Transform startTarget;

		public string commands = "";

		public bool useOptitrackReplay;

		public bool IsPlaying => isPlaying;

		private void Start()
		{
			drone = GetComponent<Drone>();
			if (useOptitrackReplay)
			{
				GameObject obj = new GameObject("OptiTrack");
				DebugOptitrackReplay debugOptitrackReplay = obj.AddComponent<DebugOptitrackReplay>();
				obj.transform.position = base.transform.position;
				obj.transform.rotation = base.transform.rotation;
				debugOptitrackReplay.droneSim = drone;
			}
		}

		public void StartPlayback(string filename = "")
		{
			recordingFilename = filename;
			if (recordingFilename == "")
			{
				FileInfo[] files = new DirectoryInfo(Application.dataPath + "/..").GetFiles("*.rec", SearchOption.TopDirectoryOnly);
				if (files.Length == 0)
				{
					return;
				}
				FileInfo fileInfo = files[0];
				for (int i = 1; i < files.Length; i++)
				{
					if (files[i].LastWriteTime > fileInfo.LastWriteTime)
					{
						fileInfo = files[i];
					}
				}
				recordingFilename = fileInfo.Name;
			}
			if (!recordingFilename.EndsWith(".rec"))
			{
				recordingFilename += ".rec";
			}
			startPlayback = false;
			positions.Clear();
			rotations.Clear();
			signals.Clear();
			timings.Clear();
			pathPositions.Clear();
			LoadFile();
			List<Keyframe> list = new List<Keyframe>(timings.Count);
			List<Keyframe> list2 = new List<Keyframe>(timings.Count);
			List<Keyframe> list3 = new List<Keyframe>(timings.Count);
			List<Keyframe> list4 = new List<Keyframe>(timings.Count);
			for (int j = 0; j < timings.Count && j < signals.Count; j++)
			{
				list.Add(new Keyframe(timings[j], signals[j].x));
				list4.Add(new Keyframe(timings[j], signals[j].y));
				list2.Add(new Keyframe(timings[j], signals[j].z));
				list3.Add(new Keyframe(timings[j], signals[j].w));
			}
			throttle = new AnimationCurve(list.ToArray());
			pitch = new AnimationCurve(list2.ToArray());
			roll = new AnimationCurve(list3.ToArray());
			yaw = new AnimationCurve(list4.ToArray());
			isPlaying = true;
			startTime = Time.time;
			frame = 0;
			if (startTarget != null)
			{
				drone.position = startTarget.position;
				drone.transform.rotation = startTarget.rotation;
			}
			if (startPosition == null)
			{
				startPosition = new GameObject("replay start position").transform;
			}
			startPosition.position = drone.position;
			startPosition.rotation = drone.transform.rotation;
			drone.receiver.channel = -1;
			if (commands != null)
			{
				string[] array = commands.Split(' ');
				for (int k = 1; k < array.Length; k++)
				{
					string[] array2 = array[k].Split('=');
					switch (array2[0])
					{
					case "groundeffect":
						drone.GetComponent<DronePhysics>();
						drone.physics.groundEffectStrength = float.Parse(array2[1]);
						drone.physics.groundeffectDistance = float.Parse(array2[2]);
						break;
					case "windload":
						drone.physics.gravityFactor = float.Parse(array2[1]);
						break;
					case "starthovering":
						drone.position += Vector3.up * float.Parse(array2[1]);
						drone.rigidbody.rb.constraints = RigidbodyConstraints.FreezePositionY;
						Activity.RunOnce(delegate
						{
							drone.rigidbody.rb.constraints = RigidbodyConstraints.None;
						}, float.Parse(array2[2]));
						break;
					case "lock":
						switch (array2[1])
						{
						case "none":
							drone.rigidbody.rb.constraints = RigidbodyConstraints.None;
							break;
						case "pos":
							drone.rigidbody.rb.constraints = RigidbodyConstraints.FreezePosition;
							break;
						case "posy":
							drone.rigidbody.rb.constraints = RigidbodyConstraints.FreezePositionY;
							break;
						case "posxz":
							drone.rigidbody.rb.constraints = (RigidbodyConstraints)10;
							break;
						case "rot":
							drone.rigidbody.rb.constraints = RigidbodyConstraints.FreezeRotation;
							break;
						case "roty":
							drone.rigidbody.rb.constraints = RigidbodyConstraints.FreezeRotationY;
							break;
						case "rotxz":
							drone.rigidbody.rb.constraints = (RigidbodyConstraints)80;
							break;
						}
						break;
					case "startpos":
						drone.position = new Vector3(float.Parse(array2[1]), float.Parse(array2[2]), float.Parse(array2[3]));
						break;
					case "startoffset":
						drone.position += new Vector3(float.Parse(array2[1]), float.Parse(array2[2]), float.Parse(array2[3]));
						break;
					case "startrot":
						drone.transform.eulerAngles = new Vector3(float.Parse(array2[1]), float.Parse(array2[2]), float.Parse(array2[3]));
						break;
					case "startrotoffset":
						drone.transform.eulerAngles += new Vector3(float.Parse(array2[1]), float.Parse(array2[2]), float.Parse(array2[3]));
						break;
					case "thrust":
						drone.physics.thrust = float.Parse(array2[1]);
						break;
					case "torque":
						drone.physics.torque = float.Parse(array2[1]);
						break;
					}
				}
			}
			DRLApp dRLApp = Object.FindObjectOfType<DRLApp>();
			dRLApp.view.ui.game.hud.controller.SetController(ControllerStateType.Taranis);
			dRLApp.view.ui.game.hud.controller.SetAnimation(UIControllerAnimationType.DroneInput, drone);
		}

		public void StopPlayback()
		{
			isPlaying = false;
			drone.receiver.channel = 0;
			drone.receiver.signal.throttle = 0f;
			drone.receiver.signal.yaw = 0f;
			drone.receiver.signal.pitch = 0f;
			drone.receiver.signal.roll = 0f;
			drone.ResetPosition(startPosition.position);
			drone.transform.rotation = startPosition.rotation;
			drone.fc.Reset();
			DrawPaths();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F11))
			{
				StartPlayback(recordingFilename);
			}
			if (arm)
			{
				arm = false;
				GameObject gameObject = GameObject.FindGameObjectWithTag("Respawn");
				if ((bool)gameObject)
				{
					drone.position = gameObject.transform.position;
					drone.transform.rotation = gameObject.transform.rotation;
				}
			}
			if (startPlayback)
			{
				StartPlayback(recordingFilename);
			}
			if (!isPlaying)
			{
				return;
			}
			while (isPlaying && timings[frame] < Time.time - startTime)
			{
				frame++;
				if (frame >= timings.Count)
				{
					StopPlayback();
					continue;
				}
				if (useCurves)
				{
					drone.receiver.signal.throttle = throttle.Evaluate(Time.time - startTime);
					drone.receiver.signal.yaw = yaw.Evaluate(Time.time - startTime);
					drone.receiver.signal.pitch = pitch.Evaluate(Time.time - startTime);
					drone.receiver.signal.roll = roll.Evaluate(Time.time - startTime);
				}
				else
				{
					drone.receiver.signal.throttle = signals[frame].x;
					drone.receiver.signal.yaw = signals[frame].y;
					drone.receiver.signal.pitch = signals[frame].z;
					drone.receiver.signal.roll = signals[frame].w;
				}
				pathPositions.Add(drone.position);
				pathRotations.Add(drone.transform.rotation);
			}
		}

		private void DrawPaths()
		{
			if (drawPaths)
			{
				if (original == null)
				{
					original = new GameObject("originalPath").AddComponent<LineRenderer>();
					original.startColor = Color.red;
					original.endColor = Color.red;
					original.widthMultiplier = 0.1f;
					original.material = new Material(Shader.Find("Sprites/Default"));
					original.material.color = new Color(1f, 1f, 1f, 0.5f);
				}
				if (recreated == null)
				{
					recreated = Object.Instantiate(original);
					recreated.startColor = Color.blue;
					recreated.endColor = Color.blue;
				}
				original.positionCount = positions.Count;
				Transform transform = new GameObject("converter").transform;
				transform.position = positions[0];
				transform.rotation = rotations[0];
				for (int i = 0; i < positions.Count; i++)
				{
					original.SetPosition(i, startPosition.TransformPoint(transform.InverseTransformPoint(positions[i])));
				}
				recreated.positionCount = pathPositions.Count;
				recreated.SetPositions(pathPositions.ToArray());
				Object.Destroy(transform.gameObject);
				Debug.Log("drew paths from " + positions.Count + " into " + original.positionCount + " points");
			}
		}

		private void LoadFile()
		{
			StreamReader streamReader = File.OpenText(recordingFilename);
			string text = "";
			startTarget = null;
			commands = null;
			while (text != "positions")
			{
				text = streamReader.ReadLine();
				if (text.StartsWith("rates"))
				{
					string[] array = text.Split(' ');
					int num = 1;
					drone.fc.profile.rcRate.pitch = float.Parse(array[num++]);
					drone.fc.profile.superRate.pitch = float.Parse(array[num++]);
					drone.fc.profile.expo.pitch = float.Parse(array[num++]);
					drone.fc.profile.rcRate.roll = float.Parse(array[num++]);
					drone.fc.profile.superRate.roll = float.Parse(array[num++]);
					drone.fc.profile.expo.roll = float.Parse(array[num++]);
					drone.fc.profile.rcRate.yaw = float.Parse(array[num++]);
					drone.fc.profile.superRate.yaw = float.Parse(array[num++]);
					drone.fc.profile.expo.yaw = float.Parse(array[num++]);
				}
				else if (text.StartsWith("pid"))
				{
					string[] array2 = text.Split(' ');
					int num2 = 1;
					drone.fc.profile.pid.pitch = new PIDVector(float.Parse(array2[num2++]), float.Parse(array2[num2++]), float.Parse(array2[num2++]));
					drone.fc.profile.pid.roll = new PIDVector(float.Parse(array2[num2++]), float.Parse(array2[num2++]), float.Parse(array2[num2++]));
					drone.fc.profile.pid.yaw = new PIDVector(float.Parse(array2[num2++]), float.Parse(array2[num2++]), float.Parse(array2[num2++]));
				}
				else if (text.StartsWith("curves"))
				{
					string[] array3 = text.Split(' ');
					drawPaths = array3[1] == "1";
				}
				else if (text.StartsWith("enable"))
				{
					string[] array4 = text.Split(' ');
					GameObject gameObject = GameObject.Find(array4[1]);
					Transform transform = null;
					if ((bool)gameObject)
					{
						transform = gameObject.transform;
					}
					if ((bool)transform)
					{
						transform = transform.Find(array4[2]);
					}
					if ((bool)transform)
					{
						transform.gameObject.SetActive(value: true);
					}
				}
				else if (text.StartsWith("start"))
				{
					GameObject gameObject2 = GameObject.Find(text.Split(' ')[1]);
					if ((bool)gameObject2)
					{
						startTarget = gameObject2.transform;
					}
				}
				else if (text.StartsWith("commands"))
				{
					commands = text;
				}
			}
			int num3 = int.Parse(streamReader.ReadLine());
			for (int i = 0; i < num3; i++)
			{
				positions.Add(parseVec(streamReader.ReadLine()));
			}
			streamReader.ReadLine();
			int num4 = int.Parse(streamReader.ReadLine());
			for (int j = 0; j < num4; j++)
			{
				rotations.Add(parseQuat(streamReader.ReadLine()));
			}
			streamReader.ReadLine();
			int num5 = int.Parse(streamReader.ReadLine());
			for (int k = 0; k < num5; k++)
			{
				Quaternion item = parseQuat(streamReader.ReadLine());
				item.y /= scaleSignals.x;
				item.z /= scaleSignals.y;
				item.w /= scaleSignals.z;
				signals.Add(item);
			}
			streamReader.ReadLine();
			int num6 = int.Parse(streamReader.ReadLine());
			for (int l = 0; l < num6; l++)
			{
				timings.Add(float.Parse(streamReader.ReadLine()));
			}
			streamReader.Close();
			Debug.Log("file " + recordingFilename + " read OK, with " + positions.Count + " positions and " + timings.Count + " timings");
		}

		private Vector3 parseVec(string s)
		{
			string[] array = s.Split(' ');
			if (array.Length < 3)
			{
				return Vector3.zero;
			}
			return new Vector3(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]));
		}

		private Quaternion parseQuat(string s)
		{
			string[] array = s.Split(' ');
			if (array.Length < 4)
			{
				return Quaternion.identity;
			}
			return new Quaternion(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
		}
	}
}
