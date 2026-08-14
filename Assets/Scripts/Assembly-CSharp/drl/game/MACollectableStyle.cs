using System.Collections.Generic;
using UnityEngine;

namespace drl.game
{
	public class MACollectableStyle : MonoBehaviour
	{
		public string id;

		public MapCollectableMode mode;

		public List<Collider> hits;

		public List<Renderer> renderers;
	}
}
