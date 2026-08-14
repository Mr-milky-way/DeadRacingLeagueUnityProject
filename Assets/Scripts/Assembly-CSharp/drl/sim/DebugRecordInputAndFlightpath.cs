using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace drl.sim
{
	public class DebugRecordInputAndFlightpath : MonoBehaviour
	{
		public KeyCode toggleHotkey = KeyCode.KeypadDivide;

		private Drone drone;

		private bool isRecording;

		private List<Vector3> positions = new List<Vector3>(10000);

		private List<Quaternion> rotations = new List<Quaternion>(10000);

		private List<Quaternion> signals = new List<Quaternion>(10000);

		private List<float> timings = new List<float>(10000);

		private List<Vector3> angularVelocity = new List<Vector3>(10000);

		private float startTime;

		private string filename;

		private Vector3 startPosition;

		private Quaternion startRotation;

		public bool useDebugInput;

		private void Start()
		{
			drone = GetComponent<Drone>();
		}

		private void Update()
		{
			if (toggleHotkey != KeyCode.None && Input.GetKeyDown(toggleHotkey))
			{
				StartRecording();
			}
		}

		public void StartRecording(string filename = "")
		{
			if (isRecording)
			{
				FinishRecording();
				return;
			}
			if (filename == "")
			{
				filename = "rec_" + DateTime.Now.ToString("MMdd_HHmmss");
			}
			if (!filename.EndsWith(".rec"))
			{
				filename += ".rec";
			}
			this.filename = filename;
			isRecording = true;
			positions.Clear();
			rotations.Clear();
			signals.Clear();
			timings.Clear();
			startTime = Time.time;
			startPosition = drone.position;
			startRotation = drone.transform.rotation;
		}

		public void FinishRecording()
		{
			if (!isRecording)
			{
				return;
			}
			isRecording = false;
			StreamWriter streamWriter = File.CreateText(filename);
			streamWriter.WriteLine("rates " + drone.fc.profile.rcRate.pitch + " " + drone.fc.profile.superRate.pitch + " " + drone.fc.profile.expo.pitch + " " + drone.fc.profile.rcRate.roll + " " + drone.fc.profile.superRate.roll + " " + drone.fc.profile.expo.roll + " " + drone.fc.profile.rcRate.yaw + " " + drone.fc.profile.superRate.yaw + " " + drone.fc.profile.expo.yaw);
			streamWriter.WriteLine("pid " + drone.fc.profile.pid.pitch.p + " " + drone.fc.profile.pid.pitch.i + " " + drone.fc.profile.pid.pitch.d + " " + drone.fc.profile.pid.roll.p + " " + drone.fc.profile.pid.roll.i + " " + drone.fc.profile.pid.roll.d + " " + drone.fc.profile.pid.yaw.p + " " + drone.fc.profile.pid.yaw.i + " " + drone.fc.profile.pid.yaw.d);
			streamWriter.WriteLine("positions");
			streamWriter.WriteLine(positions.Count.ToString());
			foreach (Vector3 position in positions)
			{
				string[] array = new string[5];
				float x = position.x;
				array[0] = x.ToString("0.000000");
				array[1] = " ";
				x = position.y;
				array[2] = x.ToString("0.000000");
				array[3] = " ";
				x = position.z;
				array[4] = x.ToString("0.000000");
				streamWriter.WriteLine(string.Concat(array));
			}
			streamWriter.WriteLine("rotations");
			streamWriter.WriteLine(rotations.Count.ToString());
			foreach (Quaternion rotation in rotations)
			{
				string[] array2 = new string[7];
				float x = rotation.x;
				array2[0] = x.ToString("0.000000");
				array2[1] = " ";
				x = rotation.y;
				array2[2] = x.ToString("0.000000");
				array2[3] = " ";
				x = rotation.z;
				array2[4] = x.ToString("0.000000");
				array2[5] = " ";
				x = rotation.w;
				array2[6] = x.ToString("0.000000");
				streamWriter.WriteLine(string.Concat(array2));
			}
			streamWriter.WriteLine("signals");
			streamWriter.WriteLine(signals.Count.ToString());
			foreach (Quaternion signal in signals)
			{
				string[] array3 = new string[7];
				float x = signal.x;
				array3[0] = x.ToString("0.000000");
				array3[1] = " ";
				x = signal.y;
				array3[2] = x.ToString("0.000000");
				array3[3] = " ";
				x = signal.z;
				array3[4] = x.ToString("0.000000");
				array3[5] = " ";
				x = signal.w;
				array3[6] = x.ToString("0.000000");
				streamWriter.WriteLine(string.Concat(array3));
			}
			streamWriter.WriteLine("timings");
			streamWriter.WriteLine(timings.Count.ToString());
			foreach (float timing in timings)
			{
				streamWriter.WriteLine(timing.ToString("0.000000"));
			}
			streamWriter.Close();
			drone.ResetPosition(startPosition);
			drone.transform.rotation = startRotation;
			drone.fc.Reset();
		}

		private void FixedUpdate()
		{
			if (isRecording)
			{
				positions.Add(drone.position);
				rotations.Add(base.transform.rotation);
				timings.Add(Time.time - startTime);
				if (useDebugInput)
				{
					signals.Add(new Quaternion(drone.fc.debugThrottle, drone.fc.debugYaw, drone.fc.debugPitch, drone.fc.debugRoll));
				}
				else
				{
					signals.Add(new Quaternion(drone.receiver.signal.throttle, drone.receiver.signal.yaw, drone.receiver.signal.pitch, drone.receiver.signal.roll));
				}
			}
		}
	}
}
