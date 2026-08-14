using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using drl.sim;

namespace thelab.core
{
	public class Flow : MonoBehaviour
	{
		private static List<Type> m_fn_types;

		private static List<string> m_fn_type_names;

		public bool run;

		public bool complete;

		public bool active;

		public SkipHandler skipHandler;

		public int pointer;

		public int repeat = -1;

		public int count;

		public float speed;

		[SerializeField]
		private List<FlowNode> m_nodes;

		private bool m_has_branch;

		private float m_pointer_elapsed;

		public Action ProgressUpdate;

		[SerializeField]
		[HideInInspector]
		private string m_guid;

		public FlowNode current
		{
			get
			{
				if (nodes.Count > 0)
				{
					if (pointer >= 0)
					{
						if (pointer < nodes.Count)
						{
							return nodes[pointer];
						}
						return null;
					}
					return null;
				}
				return null;
			}
		}

		public List<FlowNode> nodes
		{
			get
			{
				if (m_nodes != null)
				{
					return m_nodes;
				}
				return m_nodes = new List<FlowNode>();
			}
		}

		internal string guid
		{
			get
			{
				if (!string.IsNullOrEmpty(m_guid))
				{
					return m_guid;
				}
				return m_guid = GetHashCode().ToString("x6");
			}
		}

		public static List<Type> GetFlowNodeTypes()
		{
			if (m_fn_types != null)
			{
				return m_fn_types;
			}
			List<Type> list = new List<Type>();
			Assembly clientAssembly = Reflection<object>.clientAssembly;
			if (clientAssembly != null)
			{
				Type[] types = clientAssembly.GetTypes();
				foreach (Type type in types)
				{
					object[] customAttributes = type.GetCustomAttributes(inherit: false);
					bool flag = false;
					for (int j = 0; j < customAttributes.Length; j++)
					{
						if (customAttributes[j] is HideInInspector)
						{
							flag = true;
							break;
						}
					}
					if (!flag && Reflection<object>.InheritFrom<FlowNode>(type))
					{
						list.Add(type);
					}
				}
			}
			clientAssembly = Reflection<object>.unityEngineAssembly;
			if (clientAssembly != null)
			{
				Type[] types = clientAssembly.GetTypes();
				foreach (Type type2 in types)
				{
					object[] customAttributes2 = type2.GetCustomAttributes(inherit: false);
					bool flag2 = false;
					for (int l = 0; l < customAttributes2.Length; l++)
					{
						if (customAttributes2[l] is HideInInspector)
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2 && Reflection<object>.InheritFrom<FlowNode>(type2))
					{
						list.Add(type2);
					}
				}
			}
			return m_fn_types = list;
		}

		public static List<string> GetFlowNodeTypeNames()
		{
			if (m_fn_type_names != null)
			{
				return m_fn_type_names;
			}
			m_fn_types = GetFlowNodeTypes();
			m_fn_type_names = new List<string>();
			for (int i = 0; i < m_fn_types.Count; i++)
			{
				string item = m_fn_types[i].Name;
				m_fn_type_names.Add(item);
			}
			return m_fn_type_names;
		}

		protected virtual void Awake()
		{
			skipHandler = new SkipHandler();
			if (run)
			{
				Run();
			}
		}

		protected virtual void OnDestroy()
		{
			if (skipHandler != null)
			{
				skipHandler.OnSkipStopHandler();
			}
			for (int i = 0; i < nodes.Count; i++)
			{
				if ((bool)nodes[i])
				{
					UnityEngine.Object.Destroy(nodes[i]);
				}
			}
		}

		public void Run()
		{
			if (!active)
			{
				active = true;
				m_has_branch = false;
				m_pointer_elapsed = 0f;
			}
		}

		public void Reset()
		{
			pointer = 0;
			count = 0;
			complete = false;
			m_has_branch = false;
			m_pointer_elapsed = 0f;
			if (pointer < nodes.Count)
			{
				nodes[pointer].status = FlowStatus.Idle;
			}
		}

		public void Refresh()
		{
			FNSimulationModule[] componentsInChildren = GetComponentsInChildren<FNSimulationModule>();
			foreach (FNSimulationModule obj in componentsInChildren)
			{
				obj.SetMode(obj.mode);
			}
		}

		public void Restart()
		{
			Reset();
			Run();
		}

		public void Stop()
		{
			active = false;
			pointer = nodes.Count;
			complete = false;
			m_has_branch = false;
			m_pointer_elapsed = 0f;
			foreach (Transform item in base.transform)
			{
				if (!(item.GetComponent<Flow>() == null))
				{
					item.GetComponent<Flow>().Stop();
				}
			}
		}

		public void End()
		{
			active = false;
			pointer = nodes.Count;
			complete = true;
			m_has_branch = false;
			m_pointer_elapsed = 0f;
			foreach (Transform item in base.transform)
			{
				if (!(item.GetComponent<Flow>() == null))
				{
					item.GetComponent<Flow>().End();
				}
			}
		}

		public void SetPause(bool f)
		{
			active = !f;
		}

		protected void Update()
		{
			if (!active || complete)
			{
				return;
			}
			int num = Mathf.Clamp(pointer, 0, nodes.Count);
			FlowNode flowNode = ((num >= nodes.Count) ? null : nodes[num]);
			bool flag = flowNode == null;
			if (flowNode != null)
			{
				if (!flowNode.finished)
				{
					flowNode.flow = this;
					flowNode.Step();
					if (flowNode.status == FlowStatus.Complete)
					{
						OnComplete(flowNode);
					}
					if (flowNode.status == FlowStatus.Fail)
					{
						OnFail(flowNode);
					}
				}
				flag = flowNode.finished;
			}
			if (!active)
			{
				return;
			}
			if (m_has_branch)
			{
				m_pointer_elapsed = 0f;
				flag = false;
			}
			m_has_branch = false;
			if (flag)
			{
				m_pointer_elapsed = ((speed <= 0f) ? 1f : (m_pointer_elapsed + 1f / speed * Time.deltaTime));
				if (m_pointer_elapsed >= 1f)
				{
					m_pointer_elapsed = 0f;
					pointer = Mathf.Clamp(pointer + 1, 0, nodes.Count);
					if (pointer < nodes.Count)
					{
						nodes[pointer].status = FlowStatus.Idle;
					}
					UpdateProgress();
				}
			}
			if (pointer < nodes.Count)
			{
				return;
			}
			bool flag2 = false;
			if (repeat > 0 && count >= repeat)
			{
				flag2 = true;
			}
			if (repeat < 0)
			{
				flag2 = true;
			}
			if (nodes.Count <= 0)
			{
				flag2 = true;
			}
			if (flag2)
			{
				active = false;
				complete = true;
				return;
			}
			count++;
			pointer = 0;
			m_pointer_elapsed = 0f;
			if (pointer < nodes.Count)
			{
				nodes[pointer].status = FlowStatus.Idle;
			}
		}

		public void Branch(string p_label)
		{
			if (complete)
			{
				return;
			}
			for (int i = 0; i < nodes.Count; i++)
			{
				FlowNode flowNode = nodes[i];
				if (!(flowNode == null) && flowNode.label == p_label)
				{
					pointer = i;
					flowNode.status = FlowStatus.Idle;
					m_has_branch = true;
					break;
				}
			}
		}

		public void Branch(int p_line)
		{
			if (!complete && p_line >= 0 && p_line < nodes.Count)
			{
				pointer = p_line;
				nodes[p_line].status = FlowStatus.Idle;
				m_has_branch = true;
			}
		}

		public void UpdateProgress()
		{
			if (!(base.name != "steps") && ProgressUpdate != null)
			{
				ProgressUpdate();
			}
		}

		public void Message(string p_event, params object[] p_data)
		{
			OnMessage(p_event, p_data);
			foreach (Transform item in base.transform)
			{
				Flow component = item.GetComponent<Flow>();
				if (component != null)
				{
					component.Message(p_event, p_data);
				}
			}
		}

		private void OnMessage(string p_event, params object[] p_data)
		{
			foreach (FlowNode node in nodes)
			{
				node.OnMessage(p_event, p_data);
			}
		}

		public FlowNode Add(int p_index, FlowNode p_node)
		{
			if (!p_node)
			{
				return null;
			}
			if (nodes.Contains(p_node))
			{
				return p_node;
			}
			p_node.flow = this;
			nodes.Insert(p_index, p_node);
			return p_node;
		}

		public T Add<T>(int p_index, T p_node) where T : FlowNode
		{
			return (T)Add(p_index, p_node);
		}

		public T Add<T>(T p_node) where T : FlowNode
		{
			return (T)Add(nodes.Count, p_node);
		}

		public FlowNode Add(FlowNode p_node)
		{
			return Add(nodes.Count, p_node);
		}

		public T Add<T>(int p_index) where T : FlowNode
		{
			return (T)Add(p_index, typeof(T));
		}

		public T Add<T>() where T : FlowNode
		{
			return (T)Add(typeof(T));
		}

		public FlowNode Add(int p_index, Type p_type)
		{
			FlowNode p_node = (FlowNode)base.gameObject.AddComponent(p_type);
			return Add(p_index, p_node);
		}

		public FlowNode Add(Type p_type)
		{
			return Add(nodes.Count, p_type);
		}

		public void Remove(FlowNode p_node)
		{
			if (nodes.Contains(p_node))
			{
				nodes.Remove(p_node);
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(p_node);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(p_node);
				}
			}
		}

		protected virtual void OnUpdate()
		{
		}

		protected virtual void OnComplete(FlowNode p_node)
		{
		}

		protected virtual void OnFail(FlowNode p_node)
		{
		}
	}
}
