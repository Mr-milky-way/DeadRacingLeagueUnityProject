using System;
using UnityEngine;

namespace drl.sim.thread
{
	public class GyroscopeSensor : MonoBehaviour
	{
		[Serializable]
		private class GyroSensitivitySetting
		{
			public float FromValue;

			public float ToValue;

			public float Sensitivity;
		}

		[Header("Settings")]
		[Header("_____________________________________________")]
		[SerializeField]
		private bool _flipped;

		[SerializeField]
		private float _flipThreshold = 50f;

		[SerializeField]
		private float _gyroRateNoise = 0.005f;

		[SerializeField]
		private float _maxDegreePerSecond = 2000f;

		[SerializeField]
		private GyroSensitivitySetting[] _sensitivitySetting;

		[SerializeField]
		private AnimationCurve _curveSensitivity;

		[Header("Readings")]
		[SerializeField]
		private Vector3 _velocity;

		private Quaternion previousFrame_Rotation;

		private System.Random random = new System.Random();

		private Vector3 _velocityRaw;

		private Vector3 _calculatedMistake;

		private Vector3 _sensitivity;

		public bool simulateNoise;

		private float zAngle;

		private float xAngle;

		public Vector3 Acceleration;

		public Vector3 Velocity
		{
			get
			{
				return _velocity;
			}
			set
			{
				_velocity = value;
			}
		}

		protected void Awake()
		{
			GenerateSensitivityCurve();
		}

		public void Refresh(float d_dt, Quaternion rot)
		{
			Vector3 eulerAngles = (Quaternion.Inverse(previousFrame_Rotation) * rot).eulerAngles;
			if (eulerAngles.x > 180f)
			{
				eulerAngles.x -= 360f;
			}
			if (eulerAngles.y > 180f)
			{
				eulerAngles.y -= 360f;
			}
			if (eulerAngles.z > 180f)
			{
				eulerAngles.z -= 360f;
			}
			_velocityRaw = eulerAngles / d_dt;
			if (simulateNoise)
			{
				_calculatedMistake.x = _velocityRaw.x / Mathf.Sqrt(1f / d_dt) * _gyroRateNoise * (float)random.NextDouble();
				_calculatedMistake.y = _velocityRaw.y / Mathf.Sqrt(1f / d_dt) * _gyroRateNoise * (float)random.NextDouble();
				_calculatedMistake.z = _velocityRaw.z / Mathf.Sqrt(1f / d_dt) * _gyroRateNoise * (float)random.NextDouble();
				_sensitivity.x = _curveSensitivity.Evaluate(Mathf.Abs(_velocityRaw.x));
				_sensitivity.y = _curveSensitivity.Evaluate(Mathf.Abs(_velocityRaw.y));
				_sensitivity.z = _curveSensitivity.Evaluate(Mathf.Abs(_velocityRaw.z));
				_velocityRaw.x += _calculatedMistake.x;
				_velocityRaw.y += _calculatedMistake.y;
				_velocityRaw.z += _calculatedMistake.z;
				_velocity.x = Mathf.Lerp(_velocity.x, _velocityRaw.x, d_dt * _sensitivity.x);
				_velocity.y = Mathf.Lerp(_velocity.y, _velocityRaw.y, d_dt * _sensitivity.y);
				_velocity.z = Mathf.Lerp(_velocity.z, _velocityRaw.z, d_dt * _sensitivity.z);
			}
			else
			{
				_velocity = _velocityRaw;
			}
			previousFrame_Rotation = rot;
			Acceleration = rot * Vector3.down * 9.81f;
		}

		private void GenerateSensitivityCurve()
		{
			_curveSensitivity = new AnimationCurve();
			for (int i = 0; i < _sensitivitySetting.Length; i++)
			{
				if (i == 0)
				{
					_curveSensitivity.AddKey(_sensitivitySetting[i].FromValue, _sensitivitySetting[i].Sensitivity);
					_curveSensitivity.AddKey(_sensitivitySetting[i].ToValue, _sensitivitySetting[i].Sensitivity);
				}
				else
				{
					_curveSensitivity.AddKey(_sensitivitySetting[i].ToValue, _sensitivitySetting[i].Sensitivity);
				}
			}
			for (int j = 0; j < _curveSensitivity.keys.Length; j++)
			{
				_curveSensitivity.SmoothTangents(j, 1f);
			}
		}
	}
}
