using UnityEngine;
using UnityEngine.Rendering;

namespace thelab.core
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	public class GizmoRenderer : MonoBehaviour
	{
		public Material material;

		[HideInInspector]
		public MeshFilter mfilter;

		[HideInInspector]
		public Renderer renderer;

		public Color color = Color.white;

		public float alpha
		{
			get
			{
				return color.a;
			}
			set
			{
				color.a = value;
			}
		}

		protected virtual void Awake()
		{
			Refresh(p_force: true);
		}

		protected virtual void OnEnable()
		{
			Refresh(p_force: true);
		}

		public virtual void Refresh(bool p_force = false)
		{
			bool flag = IsDirty();
			if (!mfilter)
			{
				mfilter = GetComponent<MeshFilter>();
				if ((bool)mfilter)
				{
					mfilter.hideFlags = HideFlags.HideInInspector;
					flag = true;
				}
			}
			if (!renderer)
			{
				renderer = GetComponent<MeshRenderer>();
				if ((bool)renderer)
				{
					renderer.hideFlags = HideFlags.HideInInspector;
					renderer.sharedMaterials = new Material[0];
					renderer.shadowCastingMode = ShadowCastingMode.Off;
					renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
					flag = true;
				}
			}
			if (flag || p_force)
			{
				OnRefresh();
			}
		}

		protected virtual void OnRenderObject()
		{
			Camera current = Camera.current;
			if (!current)
			{
				return;
			}
			if (current.name != "SceneCamera")
			{
				int layer = base.gameObject.layer;
				if (((1 << layer) & current.cullingMask) == 0)
				{
					return;
				}
			}
			Refresh();
			OnRender();
		}

		protected virtual bool IsDirty()
		{
			return false;
		}

		protected virtual void OnRefresh()
		{
		}

		protected virtual void OnRender()
		{
		}

		protected Material AssertMaterial(Material v, Material t)
		{
			if ((bool)v)
			{
				return v;
			}
			v = (t ? Object.Instantiate(t) : null);
			if ((bool)v)
			{
				v.hideFlags = HideFlags.HideAndDontSave;
				v.name = v.name.Replace("(Clone)", "");
			}
			return v;
		}

		protected Mesh AssertMesh(Mesh v, string n)
		{
			if ((bool)v)
			{
				return v;
			}
			v = new Mesh();
			v.name = n;
			v.hideFlags = HideFlags.HideAndDontSave;
			return v;
		}

		protected void DestroyProper(Object v)
		{
			if ((bool)v)
			{
				if (Application.isPlaying)
				{
					Object.Destroy(v);
				}
				else
				{
					Object.DestroyImmediate(v);
				}
			}
		}
	}
}
