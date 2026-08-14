using System;
using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIGarageChartsController : Controller<DRLApp>
	{
		private List<DroneMotor> m_motors;

		private Dictionary<string, int> m_motorGuids;

		private UIGraph[] m_graphs;

		private string m_lastUrl;

		private DroneMotor m_lastMotor;

		private int m_lastBenchtest;

		private bool m_reviewMode;

		public UIGarageChartsView view => AssertLocal<UIGarageChartsView>("view");

		public List<DroneMotor> motors
		{
			get
			{
				if (m_motors == null)
				{
					m_motors = base.app.model.storage.library.FindAll<DroneMotor>();
				}
				return m_motors;
			}
		}

		public Dictionary<string, int> motorGuids
		{
			get
			{
				if (m_motorGuids == null)
				{
					m_motorGuids = new Dictionary<string, int>(motors.Count);
					for (int i = 0; i < motors.Count; i++)
					{
						if (!m_motorGuids.ContainsKey(motors[i].guid))
						{
							m_motorGuids.Add(motors[i].guid, i);
						}
						else
						{
							Debug.LogError("UIGarageChartsController> duplicate motor guid for motor " + motors[i].name);
						}
					}
				}
				return m_motorGuids;
			}
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "garage.charts.datasheet@click":
				break;
			case "ui.screen@close":
				if ((bool)base.app.view.ui.header)
				{
					base.app.view.ui.header.logoContainer.gameObject.SetActive(value: false);
				}
				break;
			case "ui.screen@open":
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				if ((bool)base.app.view.ui.header)
				{
					base.app.view.ui.header.logoContainer.gameObject.SetActive(value: true);
				}
				m_reviewMode = false;
				if (m_graphs == null || m_graphs.Length == 0)
				{
					m_graphs = new UIGraph[view.graphs.Length];
					for (int i = 0; i < m_graphs.Length; i++)
					{
						m_graphs[i] = view.graphs[i].GetComponentInChildren<UIGraph>();
					}
				}
				if (!(view.drone == null) && !(view.drone.body == null) && !(view.drone.body.frame == null) && view.drone.body.frame.escs != null && view.drone.body.frame.escs.Count != 0)
				{
					DroneESC droneESC = view.drone.body.frame.escs[0];
					if (!(droneESC.motor == null) && !(droneESC.motor.spec == null) && droneESC.motor.spec.data != null)
					{
						UpdateCharts(droneESC.motor);
					}
				}
				break;
			case "garage.charts.review@click":
				m_reviewMode = true;
				m_lastBenchtest = 0;
				m_lastMotor = motors[0];
				if (m_lastMotor.spec.measurements.Count != 0)
				{
					UpdateCharts(m_lastMotor.spec.measurements[m_lastBenchtest]);
					view.caption.text = "REVIEWING: " + m_lastMotor.info.name + " " + m_lastMotor.spec.statorWidth.ToString("00") + m_lastMotor.spec.statorHeight.ToString("00") + " " + m_lastMotor.spec.kv + "kv with " + m_lastMotor.spec.measurements[m_lastBenchtest].name;
					if ((bool)view.debugCaption)
					{
						view.debugCaption.text = "line " + m_lastMotor.spec.measurements[m_lastBenchtest].verificationLine + " of " + m_lastMotor.spec.measurements[m_lastBenchtest].verificationFilename;
					}
				}
				break;
			case "garage.charts.review.next@click":
				m_reviewMode = true;
				m_lastBenchtest++;
				if (m_lastBenchtest >= m_lastMotor.spec.measurements.Count)
				{
					m_lastBenchtest = 0;
					int num2 = motorGuids[m_lastMotor.guid] + 1;
					if (num2 >= motors.Count)
					{
						num2 = 0;
					}
					m_lastMotor = motors[num2];
				}
				if (m_lastMotor.spec.measurements.Count != 0)
				{
					UpdateCharts(m_lastMotor.spec.measurements[m_lastBenchtest]);
					view.caption.text = "REVIEWING: " + m_lastMotor.info.name + " " + m_lastMotor.spec.statorWidth.ToString("00") + m_lastMotor.spec.statorHeight.ToString("00") + " " + m_lastMotor.spec.kv + "kv with " + m_lastMotor.spec.measurements[m_lastBenchtest].name;
					if ((bool)view.debugCaption)
					{
						view.debugCaption.text = "line " + m_lastMotor.spec.measurements[m_lastBenchtest].verificationLine + " of " + m_lastMotor.spec.measurements[m_lastBenchtest].verificationFilename;
					}
				}
				break;
			case "garage.charts.review.previous@click":
				m_reviewMode = true;
				m_lastBenchtest--;
				if (m_lastBenchtest < 0)
				{
					int num = motorGuids[m_lastMotor.guid] - 1;
					if (num < 0)
					{
						num = motors.Count - 1;
					}
					m_lastMotor = motors[num];
					m_lastBenchtest = m_lastMotor.spec.measurements.Count - 1;
				}
				if (m_lastMotor.spec.measurements.Count != 0)
				{
					UpdateCharts(m_lastMotor.spec.measurements[m_lastBenchtest]);
					view.caption.text = "REVIEWING: " + m_lastMotor.info.name + " " + m_lastMotor.spec.statorWidth.ToString("00") + m_lastMotor.spec.statorHeight.ToString("00") + " " + m_lastMotor.spec.kv + "kv with " + m_lastMotor.spec.measurements[m_lastBenchtest].name;
					if ((bool)view.debugCaption)
					{
						view.debugCaption.text = "line " + m_lastMotor.spec.measurements[m_lastBenchtest].verificationLine + " of " + m_lastMotor.spec.measurements[m_lastBenchtest].verificationFilename;
					}
				}
				break;
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			}
		}

		private void UpdateCharts(DroneMotor p_motor)
		{
			m_lastMotor = p_motor;
			UpdateCharts(p_motor.spec.data);
			view.caption.text = p_motor.info.name + " " + p_motor.spec.statorWidth.ToString("00") + p_motor.spec.statorHeight.ToString("00") + " " + p_motor.spec.kv + "kv  with " + p_motor.prop.info.name;
		}

		private void UpdateCharts(DroneMotorSpec.BenchData p_data)
		{
			m_lastUrl = null;
			if (p_data != null)
			{
				m_lastUrl = p_data.dataUrl;
				int num = 0;
				DrawChart(m_graphs[num++], 100, "SIGNAL TO THRUST", "signal", "0.0", 1f, 0.1f, "gF", "0", p_data.GetMaxThrust(), 500f, (float x) => p_data.thrust.Evaluate(p_data.rpm.Evaluate(p_data.watts.Evaluate(p_data.amperes.Evaluate(x)))));
				DrawChart(m_graphs[num++], 100, "SIGNAL TO TORQUE", "signal", "0.0", 1f, 0.1f, "Nm", "0.000", p_data.GetMaxTorque(), 0.001f, (float x) => p_data.torque.Evaluate(p_data.watts.Evaluate(p_data.amperes.Evaluate(x))));
				DrawChart(m_graphs[num++], 100, "SIGNAL TO RPM", "signal", "0.0", 1f, 0.1f, "rpm", "0", p_data.GetMaxRPM(), 5000f, (float x) => p_data.rpm.Evaluate(p_data.watts.Evaluate(p_data.amperes.Evaluate(x))));
				DrawChart(m_graphs[num++], 100, "RPM TO THRUST", "rpm", "0", p_data.GetMaxRPM(), 5000f, "gF", "0", p_data.GetMaxThrust(), 500f, (float x) => p_data.thrust.Evaluate(x));
			}
		}

		private void DrawChart(UIGraph p_g, int p_points, string p_caption, string p_labelX, string p_xFormat, float p_maxX, float p_xGranularity, string p_labelY, string p_yFormat, float p_maxY, float p_yGranularity, Func<float, float> Calculate)
		{
			float num = RoundTo(p_maxX, p_xGranularity);
			float num2 = RoundTo(p_maxY, p_yGranularity) + p_yGranularity;
			Vector2[] array = new Vector2[p_points];
			for (int i = 0; i < array.Length; i++)
			{
				float num3 = 1f * (float)i / (float)array.Length * p_maxX;
				array[i] = new Vector2(num3, Calculate(num3));
			}
			p_g.inputFormat = p_xFormat;
			p_g.outputFormat = p_yFormat;
			p_g.SetCaption(p_caption);
			p_g.SetBounds(0f, p_maxX, 0f, num2 + p_yGranularity * 0.2f);
			p_g.SetLabels(p_labelX, p_labelY, new float[3]
			{
				num * 0.333334f,
				num * 0.666667f,
				num
			}, new float[3]
			{
				0f,
				num2 * 0.5f,
				num2
			});
			if (m_reviewMode)
			{
				p_g.UpdateGraph(array);
			}
			else
			{
				p_g.UpdateGraph(array, new Vector2(-0.05f, -0.01f), new Vector2(1.05f, 1.05f));
			}
			p_g.SetEndpointLabel(p_maxY);
		}

		private float RoundTo(float p_value, float p_scale)
		{
			return (float)(int)((p_value + 1E-06f) / p_scale) * p_scale;
		}
	}
}
