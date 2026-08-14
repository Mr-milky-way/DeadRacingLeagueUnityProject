using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class FNBranch : FlowNode
	{
		[Serializable]
		public class Statement
		{
			public UnityEngine.Object target;

			public string statement = "";

			public string label = "";

			public bool IsTrue()
			{
				return Reflection<object>.ParseStatement(target, statement);
			}

			public bool IsFalse()
			{
				return !IsTrue();
			}
		}

		[SerializeField]
		private List<Statement> m_statements;

		public List<Statement> statements
		{
			get
			{
				if (m_statements != null)
				{
					return m_statements;
				}
				return m_statements = new List<Statement>();
			}
		}

		internal override bool hasContent => true;

		internal override FlowStatus OnUpdate()
		{
			for (int i = 0; i < statements.Count; i++)
			{
				Statement statement = statements[i];
				if (statement.IsTrue())
				{
					flow.Branch(statement.label);
					return FlowStatus.Complete;
				}
			}
			return FlowStatus.Running;
		}
	}
}
