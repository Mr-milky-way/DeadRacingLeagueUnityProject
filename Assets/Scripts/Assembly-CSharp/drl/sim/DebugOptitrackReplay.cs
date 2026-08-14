using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace drl.sim
{
	public class DebugOptitrackReplay : MonoBehaviour
	{
		[Serializable]
		public class SyncCurves
		{
			public AnimationCurve accelX = new AnimationCurve();

			public AnimationCurve accelY = new AnimationCurve();

			public AnimationCurve accelZ = new AnimationCurve();

			public AnimationCurve bbAccelX = new AnimationCurve();

			public AnimationCurve bbAccelY = new AnimationCurve();

			public AnimationCurve bbAccelZ = new AnimationCurve();
		}

		private List<Vector3> positions = new List<Vector3>(10000);

		private List<Quaternion> rotations = new List<Quaternion>(10000);

		private List<Quaternion> signals = new List<Quaternion>(10000);

		private List<float> timings = new List<float>(10000);

		private List<Vector3> speed = new List<Vector3>(10000);

		private List<Vector3> angular = new List<Vector3>(10000);

		private List<Vector3> acceleration = new List<Vector3>(10000);

		private List<Vector3> localAcceleration = new List<Vector3>(10000);

		private List<Vector3> torque = new List<Vector3>(10000);

		private AnimationCurve posX = new AnimationCurve();

		private AnimationCurve posY = new AnimationCurve();

		private AnimationCurve posZ = new AnimationCurve();

		private AnimationCurve rotX = new AnimationCurve();

		private AnimationCurve rotY = new AnimationCurve();

		private AnimationCurve rotZ = new AnimationCurve();

		private AnimationCurve rotW = new AnimationCurve();

		public SyncCurves syncCurves = new SyncCurves();

		private LineRenderer pathOptitrack;

		private LineRenderer pathSim;

		private Transform startPosition;

		public Transform droneOptitrack;

		public Drone droneSim;

		public int currentFrame;

		public float currentTime;

		public float blackboxTime;

		public int blackboxFrame;

		public bool paused = true;

		private Transform converter;

		private DroneCamera cam;

		public bool fullOptitrackPath;

		public string optitrackFile;

		public string blackboxFile;

		public float deltaTime;

		public bool trim;

		public float overrideMotorInputs = 1f;

		public float overrideRcInputs;

		public bool trigger_syncSimToOptitrack;

		public float activeSyncRotation;

		public float activeSyncPosition;

		public float activeSyncSpeed;

		public float stopAt;

		private int pathStartFrame;

		public bool showOptitrack = true;

		public bool showSimulated = true;

		public string[] bb_files;

		public string[] optitrack_files;

		public float[] timestamp_deltas;

		private int currentRecording;

		private List<float> bbTime = new List<float>(10000);

		private List<float> motorFL = new List<float>(10000);

		private List<float> motorFR = new List<float>(10000);

		private List<float> motorBL = new List<float>(10000);

		private List<float> motorBR = new List<float>(10000);

		private List<float> signalThrottle = new List<float>(10000);

		private List<float> signalPitch = new List<float>(10000);

		private List<float> signalRoll = new List<float>(10000);

		private List<float> signalYaw = new List<float>(10000);

		public float bbStartTime;

		private void Start()
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			string[] files = Directory.GetFiles(Application.streamingAssetsPath + "/Physics");
			foreach (string text in files)
			{
				string fileName = Path.GetFileName(text);
				if (Path.GetExtension(text).ToLower() == ".csv")
				{
					if (fileName.ToLower().StartsWith("bb"))
					{
						list.Add(text);
					}
					else if (fileName.ToLower().StartsWith("optitrack"))
					{
						list2.Add(text);
					}
				}
			}
			List<string> list3 = new List<string>();
			List<string> list4 = new List<string>();
			List<float> list5 = new List<float>();
			foreach (string item in list2)
			{
				string value = Path.GetFileName(item).Substring(10).ToLower()
					.Replace(".csv", "");
				string text2 = null;
				foreach (string item2 in list)
				{
					if (Path.GetFileName(item2).Substring(3).ToLower()
						.StartsWith(value))
					{
						text2 = item2;
						break;
					}
				}
				if (string.IsNullOrEmpty(text2))
				{
					Debug.LogError("BB file not found for " + item);
					continue;
				}
				list3.Add(text2);
				list4.Add(item);
				list5.Add(float.Parse(Path.GetFileName(text2).ToLower().Replace(".csv", "")
					.Split(new string[1] { "_delta" }, StringSplitOptions.None)[1], NumberStyles.Number, CultureInfo.InvariantCulture));
			}
			bb_files = list3.ToArray();
			optitrack_files = list4.ToArray();
			timestamp_deltas = list5.ToArray();
			droneSim.renderer.SetTrailsEnabled(p_flag: false);
			droneSim.SetMotorRPM(10000f);
			droneSim.renderer.color0 = Color.blue;
			droneSim.renderer.color1 = Color.blue;
			droneSim.renderer.color2 = Color.blue;
			droneOptitrack = UnityEngine.Object.Instantiate(droneSim.body.frame.gameObject).transform;
			UnityEngine.Object.Destroy(droneOptitrack.Find("colliders").gameObject);
			UnityEngine.Object.Destroy(droneOptitrack.Find("captures").gameObject);
			droneOptitrack.position = base.transform.position;
			droneOptitrack.rotation = base.transform.rotation;
			droneOptitrack.parent = base.transform;
			Renderer[] componentsInChildren = droneOptitrack.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_ = componentsInChildren[i].materials;
			}
			droneSim.renderer.color0 = Color.red;
			droneSim.renderer.color1 = Color.red;
			droneSim.renderer.color2 = Color.red;
			optitrackFile = optitrack_files[0];
			blackboxFile = bb_files[0];
			LoadOptitrack(optitrackFile);
			LoadBlackbox(blackboxFile);
			deltaTime = timestamp_deltas[0];
			if (pathOptitrack == null)
			{
				pathOptitrack = new GameObject("optitrackPath").AddComponent<LineRenderer>();
			}
			pathOptitrack.startColor = Color.blue;
			pathOptitrack.endColor = Color.blue;
			pathOptitrack.widthMultiplier = 0.02f;
			pathOptitrack.material = new Material(Shader.Find("Sprites/Default"));
			pathOptitrack.material.color = new Color(1f, 1f, 1f, 0.5f);
			pathOptitrack.positionCount = 0;
			if (pathSim == null)
			{
				pathSim = new GameObject("simulatorPath").AddComponent<LineRenderer>();
			}
			pathSim.startColor = Color.red;
			pathSim.endColor = Color.red;
			pathSim.widthMultiplier = 0.02f;
			pathSim.material = new Material(Shader.Find("Sprites/Default"));
			pathSim.material.color = new Color(1f, 1f, 1f, 0.5f);
			pathSim.positionCount = 0;
			if (converter == null)
			{
				converter = new GameObject("converter").transform;
			}
			converter.position = positions[0];
			converter.rotation = rotations[0];
		}

		private void SetFrame(int frame)
		{
			if (frame < 0)
			{
				frame = 0;
			}
			if (frame >= timings.Count)
			{
				frame = timings.Count - 1;
			}
			droneOptitrack.position = base.transform.TransformPoint(converter.InverseTransformPoint(positions[currentFrame]));
			droneOptitrack.localRotation = rotations[currentFrame];
			currentTime = timings[frame];
		}

		private void SetTime(float time)
		{
			Vector3 position = new Vector3(posX.Evaluate(time), posY.Evaluate(time), posZ.Evaluate(time));
			Quaternion localRotation = new Quaternion(rotX.Evaluate(time), rotY.Evaluate(time), rotZ.Evaluate(time), rotW.Evaluate(time));
			droneOptitrack.position = base.transform.TransformPoint(converter.InverseTransformPoint(position));
			droneOptitrack.localRotation = localRotation;
			while (currentFrame < timings.Count - 2 && timings[currentFrame + 1] < time)
			{
				currentFrame++;
			}
			while (currentFrame > 0 && timings[currentFrame] > time)
			{
				currentFrame--;
			}
			while (blackboxFrame < bbTime.Count - 2 && bbTime[blackboxFrame + 1] - bbStartTime < time + deltaTime)
			{
				blackboxFrame++;
			}
			while (blackboxFrame > 0 && bbTime[blackboxFrame] - bbStartTime > time + deltaTime)
			{
				blackboxFrame--;
			}
			droneSim.simulation.externalOverrideEsc = overrideMotorInputs > 0f;
			if (overrideRcInputs > 0f)
			{
				droneSim.fc.allowPitch = false;
				droneSim.fc.allowRoll = false;
				droneSim.fc.allowYaw = false;
				droneSim.fc.allowThrottle = false;
				droneSim.fc.debugThrottle = signalThrottle[blackboxFrame] * overrideRcInputs;
				droneSim.fc.debugPitch = signalPitch[blackboxFrame] * overrideRcInputs;
				droneSim.fc.debugYaw = signalYaw[blackboxFrame] * overrideRcInputs;
				droneSim.fc.debugRoll = signalRoll[blackboxFrame] * overrideRcInputs;
			}
			else
			{
				droneSim.fc.allowPitch = true;
				droneSim.fc.allowRoll = true;
				droneSim.fc.allowYaw = true;
				droneSim.fc.allowThrottle = true;
			}
			if (overrideMotorInputs > 0f)
			{
				droneSim.simulation.currentInput[3] = motorFL[blackboxFrame] * overrideMotorInputs;
				droneSim.simulation.currentInput[0] = motorFR[blackboxFrame] * overrideMotorInputs;
				droneSim.simulation.currentInput[2] = motorBL[blackboxFrame] * overrideMotorInputs;
				droneSim.simulation.currentInput[1] = motorBR[blackboxFrame] * overrideMotorInputs;
			}
		}

		public void SetPause(bool flag)
		{
			if (paused != flag)
			{
				droneSim.SetPaused(flag);
				paused = flag;
			}
		}

		private void Update()
		{
			if (droneOptitrack.gameObject.activeSelf != showOptitrack)
			{
				droneOptitrack.gameObject.SetActive(showOptitrack);
				pathOptitrack.gameObject.SetActive(showOptitrack);
			}
			if (droneSim.gameObject.activeSelf != showSimulated)
			{
				droneSim.gameObject.SetActive(showSimulated);
				pathSim.gameObject.SetActive(showSimulated);
			}
			if (stopAt > 0f && currentTime > stopAt && !paused)
			{
				SetPause(flag: true);
			}
			if (Input.GetKeyDown(KeyCode.F1))
			{
				currentRecording++;
				if (currentRecording >= bb_files.Length)
				{
					currentRecording = bb_files.Length - 1;
				}
			}
			if (Input.GetKeyDown(KeyCode.F2))
			{
				currentRecording--;
				if (currentRecording < 0)
				{
					currentRecording = 0;
				}
			}
			if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.F2))
			{
				optitrackFile = optitrack_files[currentRecording];
				blackboxFile = bb_files[currentRecording];
				LoadOptitrack(optitrackFile);
				LoadBlackbox(blackboxFile);
				deltaTime = timestamp_deltas[currentRecording];
			}
			if (Input.GetKeyDown(KeyCode.Space))
			{
				if (paused)
				{
					stopAt = -1f;
				}
				SetPause(!paused);
				if (cam == null)
				{
					cam = UnityEngine.Object.FindObjectOfType<DroneCamera>();
				}
				if (cam != null)
				{
					cam.SetNone();
				}
			}
			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				if (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
				{
					currentFrame = 0;
					currentTime = timings[0];
					if (paused)
					{
						SetFrame(0);
					}
					else
					{
						SetTime(currentTime);
					}
					pathOptitrack.positionCount = 0;
					pathSim.positionCount = 0;
					pathStartFrame = 0;
					stopAt = -1f;
				}
				else
				{
					stopAt = currentTime;
					currentTime -= 2f;
					if (currentTime < timings[0])
					{
						currentTime = timings[0];
					}
					SetTime(currentTime);
					trigger_syncSimToOptitrack = true;
					SetPause(flag: false);
					pathOptitrack.positionCount = 0;
					pathSim.positionCount = 0;
					pathStartFrame = currentFrame;
				}
			}
			if (paused)
			{
				if (Input.GetKeyDown(KeyCode.RightArrow) || (Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.RightControl)))
				{
					if (currentFrame < timings.Count - 2)
					{
						stopAt = timings[currentFrame + 1];
					}
					SetPause(flag: false);
				}
				if (Input.GetKeyDown(KeyCode.LeftArrow) || (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightControl)))
				{
					currentFrame--;
					if (currentFrame < 0)
					{
						currentFrame = 0;
					}
					SetFrame(currentFrame);
					trigger_syncSimToOptitrack = true;
				}
			}
			else
			{
				currentTime += Time.deltaTime;
				SetTime(currentTime);
				if (activeSyncRotation > 0f)
				{
					droneSim.transform.rotation = Quaternion.Slerp(droneSim.transform.rotation, droneOptitrack.rotation, activeSyncRotation);
				}
				if (activeSyncPosition > 0f)
				{
					droneSim.transform.position = Vector3.Lerp(droneSim.transform.position, droneOptitrack.position, activeSyncPosition);
				}
			}
			if (Input.GetKeyDown(KeyCode.Return) || trigger_syncSimToOptitrack)
			{
				trigger_syncSimToOptitrack = false;
				droneSim.position = droneOptitrack.position;
				droneSim.transform.rotation = droneOptitrack.rotation;
				droneSim.rigidbody.rb.velocity = base.transform.TransformVector(converter.InverseTransformVector(speed[currentFrame]));
				droneSim.rigidbody.rb.angularVelocity = base.transform.TransformVector(converter.InverseTransformVector(angular[currentFrame]));
			}
			if (currentFrame - pathStartFrame > pathOptitrack.positionCount)
			{
				pathOptitrack.positionCount++;
				pathSim.positionCount = pathOptitrack.positionCount;
				pathOptitrack.SetPosition(pathOptitrack.positionCount - 1, droneOptitrack.position);
				pathSim.SetPosition(pathSim.positionCount - 1, droneSim.position);
			}
		}

		private void FixedUpdate()
		{
			if (!paused && activeSyncSpeed > 0f)
			{
				droneSim.rigidbody.rb.velocity = Vector3.Lerp(droneSim.rigidbody.rb.velocity, base.transform.TransformVector(converter.InverseTransformVector(speed[currentFrame])), activeSyncSpeed);
			}
		}

		public void LoadOptitrack(string filename)
		{
			if (!File.Exists(filename))
			{
				return;
			}
			Debug.Log("OptitrackReplay> loaded recording [" + Path.GetFileName(filename).Replace(".csv", "") + "]");
			paused = true;
			string[] array = File.ReadAllLines(filename);
			int i = 7;
			List<Keyframe> list = new List<Keyframe>(10000);
			List<Keyframe> list2 = new List<Keyframe>(10000);
			List<Keyframe> list3 = new List<Keyframe>(10000);
			List<Keyframe> list4 = new List<Keyframe>(10000);
			List<Keyframe> list5 = new List<Keyframe>(10000);
			List<Keyframe> list6 = new List<Keyframe>(10000);
			List<Keyframe> list7 = new List<Keyframe>(10000);
			positions.Clear();
			rotations.Clear();
			timings.Clear();
			speed.Clear();
			angular.Clear();
			acceleration.Clear();
			localAcceleration.Clear();
			torque.Clear();
			string[] array2 = array[i].Split(',');
			float num = float.Parse(array2[1], NumberStyles.Number, CultureInfo.InvariantCulture);
			float x = (0f - float.Parse(array2[6], NumberStyles.Number, CultureInfo.InvariantCulture)) * 0.001f;
			float y = float.Parse(array2[7], NumberStyles.Number, CultureInfo.InvariantCulture) * 0.001f;
			float z = float.Parse(array2[8], NumberStyles.Number, CultureInfo.InvariantCulture) * 0.001f;
			float x2 = 0f - float.Parse(array2[2], NumberStyles.Number, CultureInfo.InvariantCulture);
			float y2 = float.Parse(array2[3], NumberStyles.Number, CultureInfo.InvariantCulture);
			float z2 = float.Parse(array2[4], NumberStyles.Number, CultureInfo.InvariantCulture);
			float w = 0f - float.Parse(array2[5], NumberStyles.Number, CultureInfo.InvariantCulture);
			Vector3 vector = new Vector3(x, y, z);
			Quaternion b = new Quaternion(x2, y2, z2, w);
			bool flag = false;
			while (i < array.Length && !flag)
			{
				if (array[i].Length > 50)
				{
					string[] array3 = array[i].Split(',');
					num = float.Parse(array3[1], NumberStyles.Number, CultureInfo.InvariantCulture);
					x = (0f - float.Parse(array3[6], NumberStyles.Number, CultureInfo.InvariantCulture)) * 0.001f;
					y = float.Parse(array3[7], NumberStyles.Number, CultureInfo.InvariantCulture) * 0.001f;
					z = float.Parse(array3[8], NumberStyles.Number, CultureInfo.InvariantCulture) * 0.001f;
					x2 = 0f - float.Parse(array3[2], NumberStyles.Number, CultureInfo.InvariantCulture);
					y2 = float.Parse(array3[3], NumberStyles.Number, CultureInfo.InvariantCulture);
					z2 = float.Parse(array3[4], NumberStyles.Number, CultureInfo.InvariantCulture);
					w = 0f - float.Parse(array3[5], NumberStyles.Number, CultureInfo.InvariantCulture);
					if ((new Vector3(x, y, z) - vector).magnitude > 0.01f)
					{
						i -= 100;
						flag = true;
					}
					else if (Quaternion.Angle(new Quaternion(x2, y2, z2, w), b) > 2f)
					{
						i -= 100;
						flag = true;
					}
					else
					{
						i += 100;
					}
				}
				else
				{
					i++;
				}
			}
			if (!flag)
			{
				Debug.LogError("cant find recording start");
				return;
			}
			for (; i < array.Length; i++)
			{
				if (array[i].Length > 50)
				{
					string[] array4 = array[i].Split(',');
					num = float.Parse(array4[1], NumberStyles.Number, CultureInfo.InvariantCulture);
					x = (0f - float.Parse(array4[6], NumberStyles.Number, CultureInfo.InvariantCulture)) * 0.001f;
					y = float.Parse(array4[7], NumberStyles.Number, CultureInfo.InvariantCulture) * 0.001f;
					z = float.Parse(array4[8], NumberStyles.Number, CultureInfo.InvariantCulture) * 0.001f;
					x2 = 0f - float.Parse(array4[2], NumberStyles.Number, CultureInfo.InvariantCulture);
					y2 = float.Parse(array4[3], NumberStyles.Number, CultureInfo.InvariantCulture);
					z2 = float.Parse(array4[4], NumberStyles.Number, CultureInfo.InvariantCulture);
					w = 0f - float.Parse(array4[5], NumberStyles.Number, CultureInfo.InvariantCulture);
					timings.Add(num);
					list.Add(new Keyframe(num, x2));
					list2.Add(new Keyframe(num, y2));
					list3.Add(new Keyframe(num, z2));
					list4.Add(new Keyframe(num, w));
					list5.Add(new Keyframe(num, x));
					list6.Add(new Keyframe(num, y));
					list7.Add(new Keyframe(num, z));
					positions.Add(new Vector3(x, y, z));
					rotations.Add(new Quaternion(x2, y2, z2, w));
				}
			}
			speed.Add(Vector3.zero);
			angular.Add(Vector3.zero);
			acceleration.Add(Vector3.zero);
			torque.Add(Vector3.zero);
			localAcceleration.Add(Vector3.zero);
			List<Keyframe> list8 = new List<Keyframe>(1000);
			List<Keyframe> list9 = new List<Keyframe>(1000);
			List<Keyframe> list10 = new List<Keyframe>(1000);
			for (i = 1; i < timings.Count; i++)
			{
				speed.Add((positions[i] - positions[i - 1]) / (timings[i] - timings[i - 1]));
				acceleration.Add((speed[i] - speed[i - 1]) / (timings[i] - timings[i - 1]));
				Quaternion quaternion = rotations[i] * Quaternion.Inverse(rotations[i - 1]);
				float angle = 0f;
				Vector3 axis = Vector3.zero;
				quaternion.ToAngleAxis(out angle, out axis);
				angle *= (float)Math.PI / 180f;
				angular.Add(axis * angle / (timings[i] - timings[i - 1]));
				torque.Add((angular[i] - angular[i - 1]) / (timings[i] - timings[i - 1]));
				localAcceleration.Add(Quaternion.Inverse(rotations[i]) * acceleration[i]);
				Keyframe item = new Keyframe(timings[i], Mathf.Clamp(localAcceleration[i].x, -100f, 100f), 0f, 0f);
				list8.Add(item);
				item = new Keyframe(timings[i], Mathf.Clamp(localAcceleration[i].y, -100f, 100f), 0f, 0f);
				list9.Add(item);
				item = new Keyframe(timings[i], Mathf.Clamp(localAcceleration[i].z, -100f, 100f), 0f, 0f);
				list10.Add(item);
			}
			syncCurves.accelX.keys = list8.ToArray();
			syncCurves.accelY.keys = list9.ToArray();
			syncCurves.accelZ.keys = list10.ToArray();
			posX.keys = list5.ToArray();
			posY.keys = list6.ToArray();
			posZ.keys = list7.ToArray();
			rotX.keys = list.ToArray();
			rotY.keys = list2.ToArray();
			rotZ.keys = list3.ToArray();
			rotW.keys = list4.ToArray();
			currentTime = timings[0];
			if (fullOptitrackPath)
			{
				if (pathOptitrack == null)
				{
					pathOptitrack = new GameObject("originalPath").AddComponent<LineRenderer>();
				}
				pathOptitrack.startColor = Color.blue;
				pathOptitrack.endColor = Color.blue;
				pathOptitrack.widthMultiplier = 0.02f;
				pathOptitrack.material = new Material(Shader.Find("Sprites/Default"));
				pathOptitrack.material.color = new Color(1f, 1f, 1f, 0.5f);
				pathOptitrack.positionCount = positions.Count;
				if (converter == null)
				{
					converter = new GameObject("converter").transform;
				}
				converter.position = positions[0];
				converter.rotation = rotations[0];
				for (i = 0; i < positions.Count; i++)
				{
					pathOptitrack.SetPosition(i, base.transform.TransformPoint(converter.InverseTransformPoint(positions[i])));
				}
			}
		}

		public void LoadBlackbox(string filename)
		{
			if (!File.Exists(filename))
			{
				return;
			}
			paused = true;
			string[] array = File.ReadAllLines(filename);
			bbTime.Clear();
			motorFL.Clear();
			motorFR.Clear();
			motorBL.Clear();
			motorBR.Clear();
			signalThrottle.Clear();
			signalPitch.Clear();
			signalRoll.Clear();
			signalYaw.Clear();
			bool flag = false;
			float num = float.Parse(array[1].Split(',')[1], CultureInfo.InvariantCulture);
			bbStartTime = 0f;
			List<Keyframe> list = new List<Keyframe>(1000);
			List<Keyframe> list2 = new List<Keyframe>(1000);
			List<Keyframe> list3 = new List<Keyframe>(1000);
			for (int i = 1; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(',');
				if (!flag)
				{
					if (array2[14] == "1000")
					{
						continue;
					}
					bbStartTime = float.Parse(array2[1], CultureInfo.InvariantCulture) - num;
					flag = true;
				}
				float num2 = float.Parse(array2[1], CultureInfo.InvariantCulture);
				float num3 = float.Parse(array2[23], CultureInfo.InvariantCulture);
				float num4 = float.Parse(array2[24], CultureInfo.InvariantCulture);
				float num5 = float.Parse(array2[25], CultureInfo.InvariantCulture);
				float num6 = float.Parse(array2[26], CultureInfo.InvariantCulture);
				float value = float.Parse(array2[20], CultureInfo.InvariantCulture);
				float value2 = float.Parse(array2[21], CultureInfo.InvariantCulture);
				float num7 = float.Parse(array2[22], CultureInfo.InvariantCulture);
				float num8 = float.Parse(array2[11], CultureInfo.InvariantCulture);
				float num9 = float.Parse(array2[12], CultureInfo.InvariantCulture);
				float num10 = float.Parse(array2[13], CultureInfo.InvariantCulture);
				float num11 = float.Parse(array2[14], CultureInfo.InvariantCulture);
				bbTime.Add(num2 - num);
				motorFL.Add((num6 - 1000f) * 0.001f);
				motorFR.Add((num4 - 1000f) * 0.001f);
				motorBL.Add((num5 - 1000f) * 0.001f);
				motorBR.Add((num3 - 1000f) * 0.001f);
				signalThrottle.Add((num11 - 1000f) / 1000f);
				signalPitch.Add(num9 / 500f);
				signalRoll.Add(num8 / 500f);
				signalYaw.Add(num10 / 500f);
				list.Add(new Keyframe(num2 - num, value2));
				list2.Add(new Keyframe(num2 - num, num7 - 9.81f));
				list3.Add(new Keyframe(num2 - num, value));
				if (trim && num2 - num - bbStartTime > timings[timings.Count - 1])
				{
					break;
				}
			}
			syncCurves.bbAccelX.keys = list.ToArray();
			syncCurves.bbAccelY.keys = list2.ToArray();
			syncCurves.bbAccelZ.keys = list3.ToArray();
		}
	}
}
