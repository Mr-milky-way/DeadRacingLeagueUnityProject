using UnityEngine;

namespace drl.game
{
	public class MECameraPreview : MonoBehaviour
	{
		[SerializeField]
		private Camera m_camera;

		public MapEditorView editor;

		public Camera camera
		{
			get
			{
				if (!m_camera)
				{
					return m_camera = GetComponent<Camera>();
				}
				return m_camera;
			}
		}

		protected void Awake()
		{
		}

		protected void OnPreRender()
		{
			if ((bool)editor)
			{
				editor.scene.SetGuidesVisible(p_flag: false);
			}
		}

		protected void OnPostRender()
		{
			if ((bool)editor)
			{
				editor.scene.SetGuidesVisible(p_flag: true);
			}
		}
	}
}
