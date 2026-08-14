using System;
using System.Collections.Generic;
using UnityEngine;
using drl.sim;

namespace thelab.core
{
	[Serializable]
	public class FNMissGateDetector : FlowNode
	{
		public enum FailType
		{
			MissGate = 0,
			IncorrectAngle = 1
		}

		[Serializable]
		public class MissData
		{
			public FNColliderElementHolder gate;

			public bool incomingFromFwd = true;

			public bool checkAttackAngle;

			public Vector3 expectedUpVector = Vector3.up;

			public bool checkMirroredUp;

			public float thresholdAngle = 45f;
		}

		public const string MISS_GATE_SCORE_KEY = "miss-gates-negative-score";

		[Tooltip("Used as a placeholder for dragging multiple objects at once")]
		public List<FNColliderElementHolder> colliderHolders;

		[SerializeField]
		private List<MissData> m_gatesToCheck;

		[SerializeField]
		private SimulationFlowModule m_module;

		[SerializeField]
		private bool m_isTest;

		[SerializeField]
		private Flow m_onFailFlow;

		private Drone mDrone;

		private float mPrevDistToTarget;

		private Vector3 mExpectedIncommingDir;

		private Vector3 mCenterPos;

		private Flow mFlow;

		private MissData mCurrentTargetData;

		private int mCurrentTargetIdx;

		private bool mWaitForStartingApproach;

		private bool m_negativeAngle;

		private float mNegativeScoreOnGateFail;

		private float mAccumNegativeScore;

		internal override bool hasContent => true;

		internal override void OnInitialize()
		{
			if (colliderHolders.Count > 0)
			{
				for (int i = 0; i < m_gatesToCheck.Count; i++)
				{
					m_gatesToCheck[i].gate = colliderHolders[i];
				}
			}
			base.OnInitialize();
			mPrevDistToTarget = 0f;
			mDrone = m_module.simulation.drones.Get(0);
			mFlow = GetComponent<Flow>();
			mCurrentTargetIdx = -1;
			mNegativeScoreOnGateFail = 0f - 1f / (float)m_gatesToCheck.Count;
			mAccumNegativeScore = 0f;
			mWaitForStartingApproach = true;
			SetNextTarget();
		}

		private bool SetNextTarget()
		{
			mCurrentTargetIdx++;
			mPrevDistToTarget = 0f;
			m_negativeAngle = false;
			if (mCurrentTargetIdx < m_gatesToCheck.Count)
			{
				mCurrentTargetData = m_gatesToCheck[mCurrentTargetIdx];
				mExpectedIncommingDir = mCurrentTargetData.gate.transform.forward * (mCurrentTargetData.incomingFromFwd ? 1f : (-1f));
				mCenterPos = mCurrentTargetData.gate.transform.position + -mExpectedIncommingDir * 0.2f;
				float num = Vector3.Distance(mDrone.transform.position, mCenterPos);
				Vector3 vector = mDrone.transform.position - mCenterPos;
				float num2 = Vector3.Dot(mExpectedIncommingDir, vector.normalized);
				mWaitForStartingApproach = num2 <= 0.5f && num >= 1f;
				return true;
			}
			mCurrentTargetData = null;
			return false;
		}

		internal override FlowStatus OnUpdate()
		{
			if (mDrone == null)
			{
				return FlowStatus.Fail;
			}
			if (mCurrentTargetData != null)
			{
				float num = Vector3.Distance(mDrone.transform.position, mCenterPos);
				if (mWaitForStartingApproach)
				{
					Vector3 vector = mDrone.transform.position - mCenterPos;
					float num2 = Vector3.Dot(mExpectedIncommingDir, vector.normalized);
					mWaitForStartingApproach = num2 <= 0.5f && num >= 1f;
				}
				else
				{
					if (mCurrentTargetData.gate.targetCollider.hit)
					{
						Vector3 to = mCurrentTargetData.gate.transform.InverseTransformDirection(mDrone.transform.up);
						if (((mCurrentTargetData.checkAttackAngle && Vector3.Angle(mCurrentTargetData.expectedUpVector, to) > mCurrentTargetData.thresholdAngle) || (mCurrentTargetData.checkMirroredUp && Vector3.Angle(mCurrentTargetData.expectedUpVector * -1f, to) > mCurrentTargetData.thresholdAngle)) && m_isTest)
						{
							m_onFailFlow.Restart();
							return FlowStatus.Complete;
						}
						if (!SetNextTarget())
						{
							return FlowStatus.Complete;
						}
					}
					else
					{
						Vector3 dir = mDrone.transform.position - mCenterPos;
						Debug.DrawRay(mCenterPos, dir, Color.red, 50f);
						Debug.DrawRay(mCenterPos, mExpectedIncommingDir * 5f, Color.blue, 50f);
						if (Vector3.Dot(mExpectedIncommingDir, dir.normalized) < 0f)
						{
							if (m_isTest)
							{
								m_onFailFlow.Restart();
								return FlowStatus.Complete;
							}
							if (m_module.data != null)
							{
								mAccumNegativeScore += mNegativeScoreOnGateFail;
								m_module.data.Set("miss-gates-negative-score", mAccumNegativeScore);
							}
							mWaitForStartingApproach = true;
						}
					}
					mPrevDistToTarget = num;
				}
				return FlowStatus.Running;
			}
			return FlowStatus.Complete;
		}
	}
}
