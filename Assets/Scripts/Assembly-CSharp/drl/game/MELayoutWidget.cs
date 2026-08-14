using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class MELayoutWidget : MEControlsWidget
	{
		public enum InstanceWrapMode
		{
			Clamp = 0,
			Repeat = 1
		}

		public enum LayoutMode
		{
			Renderer = 0,
			Spline = 1
		}

		public MEControlLayoutLayerController controller;

		public DRLNumberFieldView instanceCountField;

		public DRLInputFieldView instancePatternField;

		public DRLToggleView instancePatternWrapToggle;

		public DRLVectorFieldView layoutScaleRangeField;

		public DRLVectorFieldView layoutMarginRangeField;

		public DRLNumberFieldView layoutSpacingField;

		public DRLVectorFieldView layouOffsetPositionField;

		public DRLVectorFieldView layouDitherPositionField;

		public DRLVectorFieldView layouOrientOffsetField;

		public DRLVectorFieldView layouOrientStepField;

		public DRLToggleView layoutOrientToggle;

		public List<GameObject> entityLayoutList;

		public List<GameObject> splineLayoutList;

		public LayoutGroup widgetLayout;

		public ContentSizeFitter widgetContentFitter;

		public List<MARenderer> templates;

		public Transform container;

		public List<MARenderer> instances;

		public MARenderer anchor;

		public MELayoutSurface surface;

		public LayoutMode layoutMode;

		private Activity m_update_loop;

		private Activity m_layout_timer;

		public int instanceCount
		{
			get
			{
				return (int)instanceCountField.value;
			}
			set
			{
				instanceCountField.value = value;
			}
		}

		public string instancePattern
		{
			get
			{
				return instancePatternField.text;
			}
			set
			{
				instancePatternField.text = value;
			}
		}

		public char[] instancePatternTokens => instancePattern.ToCharArray();

		public InstanceWrapMode instancePatternWrap
		{
			get
			{
				if (!instancePatternWrapToggle.toggle.isOn)
				{
					return InstanceWrapMode.Repeat;
				}
				return InstanceWrapMode.Clamp;
			}
			set
			{
				instancePatternWrapToggle.toggle.isOn = value == InstanceWrapMode.Clamp;
				instancePatternWrapToggle.SetState(instancePatternWrapToggle.toggle.isOn);
			}
		}

		public Vector2 layoutScaleRange
		{
			get
			{
				return layoutScaleRangeField.Get<Vector2>();
			}
			set
			{
				layoutScaleRangeField.Set(value);
			}
		}

		public Vector2 layoutMarginRange
		{
			get
			{
				return layoutMarginRangeField.Get<Vector2>();
			}
			set
			{
				layoutMarginRangeField.Set(value);
			}
		}

		public float layoutSpacing
		{
			get
			{
				return layoutSpacingField.value;
			}
			set
			{
				layoutSpacingField.value = value;
			}
		}

		public Vector2 layoutOffsetPosition
		{
			get
			{
				return layouOffsetPositionField.Get<Vector2>();
			}
			set
			{
				layouOffsetPositionField.Set(value);
			}
		}

		public Vector3 layoutDitherPosition
		{
			get
			{
				return layouDitherPositionField.Get<Vector3>();
			}
			set
			{
				layouDitherPositionField.Set(value);
			}
		}

		public Vector3 layoutOrientOffset
		{
			get
			{
				return layouOrientOffsetField.Get<Vector3>();
			}
			set
			{
				layouOrientOffsetField.Set(value);
			}
		}

		public Vector3 layoutOrientStep
		{
			get
			{
				return layouOrientStepField.Get<Vector3>();
			}
			set
			{
				layouOrientStepField.Set(value);
			}
		}

		public bool layoutOrientEnabled
		{
			get
			{
				return layoutOrientToggle.toggle.isOn;
			}
			set
			{
				layoutOrientToggle.toggle.isOn = value;
				layoutOrientToggle.SetState(layoutOrientToggle.toggle.isOn);
			}
		}

		protected override void Awake()
		{
			base.Awake();
			layoutSpacingField.input.text = "";
		}

		public void Set(MARenderer p_anchor, List<MARenderer> p_templates)
		{
			anchor = p_anchor;
			templates = new List<MARenderer>(p_templates);
			if ((bool)anchor)
			{
				if ((bool)surface)
				{
					Object.Destroy(surface);
				}
				switch (anchor.data.type)
				{
				case MapAssetType.Spline:
					surface = anchor.gameObject.AddComponent<MELayoutSpline>();
					SetLayoutMode(LayoutMode.Spline);
					break;
				case MapAssetType.Renderer:
					surface = anchor.gameObject.AddComponent<MELayoutMesh>();
					SetLayoutMode(LayoutMode.Renderer);
					break;
				}
			}
			if (m_update_loop != null)
			{
				m_update_loop.Stop();
			}
			m_update_loop = Activity.Run(OnUpdate, 0f, false);
		}

		public void Clear()
		{
			anchor = null;
			templates.Clear();
			if (m_update_loop != null)
			{
				m_update_loop.Stop();
			}
		}

		public void SetLayoutMode(LayoutMode p_mode)
		{
			layoutMode = p_mode;
			List<GameObject> list = new List<GameObject>();
			list.AddRange(entityLayoutList);
			list.AddRange(splineLayoutList);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].SetActive(value: false);
			}
			list = null;
			switch (p_mode)
			{
			case LayoutMode.Renderer:
				list = entityLayoutList;
				break;
			case LayoutMode.Spline:
				list = splineLayoutList;
				break;
			}
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					list[j].SetActive(value: true);
				}
			}
			RefreshUILayout();
		}

		protected void RefreshUILayout()
		{
			widgetContentFitter.enabled = true;
			widgetLayout.enabled = true;
		}

		public bool OnUpdate()
		{
			if (!anchor)
			{
				return false;
			}
			return true;
		}

		protected void OnEnable()
		{
			if ((bool)container)
			{
				container.gameObject.SetActive(value: true);
			}
		}

		protected void OnDisable()
		{
			if ((bool)container)
			{
				container.gameObject.SetActive(value: false);
			}
		}

		protected void OnDestroy()
		{
			if ((bool)container)
			{
				Object.Destroy(container.gameObject);
			}
		}

		public void Generate(bool p_rebuild)
		{
			if (!anchor)
			{
				Debug.LogWarning("MELayoutWidget> Generate / No Anchor Found");
				return;
			}
			if (!surface)
			{
				Debug.LogWarning("MELayoutWidget> Generate / No Surface Found - type[" + anchor.data.type.ToString() + "]");
				return;
			}
			string obj = (string.IsNullOrEmpty(instancePattern) ? "1" : instancePattern);
			int count = templates.Count;
			List<char> list = new List<char>(obj.ToCharArray());
			string text = "0123456789#";
			for (int i = 0; i < list.Count; i++)
			{
				if (!text.Contains(list[i].ToString() ?? ""))
				{
					list.RemoveAt(i--);
				}
			}
			List<int> list2 = new List<int>();
			List<string> list3 = new List<string>();
			int num = 0;
			for (int j = 0; j < list.Count; j++)
			{
				if (num++ > 1000)
				{
					Debug.LogWarning("Infinite LOOP!");
					break;
				}
				string text2 = list[j].ToString();
				switch (text2)
				{
				case "*":
				{
					string s = ((list3.Count <= 0) ? "-1" : list3[list3.Count - 1]);
					string text3 = ((j < list.Count - 1) ? list[j + 1].ToString() : "-1");
					int result = -1;
					int result2 = -1;
					int.TryParse(s, out result);
					int.TryParse(text3, out result2);
					if (result >= 0 && (!(text3 != "#") || result2 >= 0))
					{
						int num2 = Mathf.Min(10, result);
						for (int k = 0; k < num2; k++)
						{
							string item2 = ((text3 == "#") ? Random.Range(1, count + 1).ToString() : text3);
							list3.Add(item2);
						}
					}
					break;
				}
				case "#":
				{
					string item = Random.Range(1, count + 1).ToString();
					list3.Add(item);
					break;
				}
				default:
					list3.Add(text2);
					break;
				}
			}
			num = 0;
			for (int l = 0; l < list3.Count; l++)
			{
				if (num++ > 1000)
				{
					Debug.LogWarning("Infinite LOOP!");
					break;
				}
				string s2 = list3[l];
				int result3 = -1;
				int.TryParse(s2, out result3);
				if (result3 >= 0)
				{
					result3 = Mathf.Clamp(result3, 0, count);
				}
				list2.Add(result3);
			}
			surface.offsetPosition = layoutOffsetPosition;
			surface.ditherPosition = layoutDitherPosition;
			surface.orientEnabled = layoutOrientEnabled;
			surface.orientOffset = layoutOrientOffset;
			if (p_rebuild)
			{
				instances = new List<MARenderer>();
			}
			instances.RemoveAll((MARenderer it) => it == null);
			if (!container)
			{
				container = new GameObject("layout-widget-container$" + base.gameObject.GetInstanceID().ToString("x6")).transform;
				container.transform.parent = null;
				container.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			}
			switch (anchor.data.type)
			{
			case MapAssetType.Spline:
				Generate(surface as MELayoutSpline, p_rebuild, list2);
				break;
			case MapAssetType.Renderer:
				Generate(surface as MELayoutMesh, p_rebuild, list2);
				break;
			}
		}

		public void Generate(MELayoutMesh p_surface, bool p_rebuild, List<int> p_template_indexes)
		{
			if (!p_surface)
			{
				return;
			}
			int instance_count = instanceCount;
			InstanceWrapMode instance_wrap = instancePatternWrap;
			Vector2 scaleRange = layoutScaleRange;
			Transform c = container;
			p_surface.orientStep = layoutOrientStep;
			p_surface.Generate(instance_count, delegate
			{
				bool flag = p_rebuild || instance_count != instances.Count;
				if (flag)
				{
					List<GameObject> list = new List<GameObject>();
					for (int i = 0; i < c.childCount; i++)
					{
						Transform child = c.GetChild(i);
						list.Add(child.gameObject);
					}
					while (list.Count > 0)
					{
						Object.Destroy(list[0]);
						list.RemoveAt(0);
					}
				}
				int num = 0;
				int num2 = 0;
				int num3 = instance_count;
				for (int j = 0; j < num3; j++)
				{
					if (num2++ > 1000)
					{
						Debug.LogWarning("Infinite LOOP!");
						break;
					}
					int num4 = ((num >= p_template_indexes.Count) ? (-1) : (p_template_indexes[num] - 1));
					MARenderer mARenderer = null;
					MapEditorController editor = controller.ui.editor;
					if (templates.Count > 0 && num4 >= 0)
					{
						MARenderer mARenderer2 = templates[num4];
						TransformVector transformVector = new TransformVector(mARenderer2.transform);
						if (flag)
						{
							mARenderer = (MARenderer)editor.view.factory.Instantiate(mARenderer2.data, c);
							mARenderer.name = mARenderer.name.Replace("(Clone)", "");
							instances.Add(mARenderer);
						}
						else
						{
							mARenderer = instances[j];
						}
						mARenderer.transform.SetSiblingIndex(instances.Count);
						TransformVector transformVector2 = p_surface.Get(j);
						mARenderer.transform.position = transformVector2.position;
						mARenderer.transform.localRotation = transformVector.rotation * transformVector2.rotation;
						mARenderer.transform.localScale = transformVector.scale * Mathf.Lerp(scaleRange.x, scaleRange.y, p_surface.GetRandom(j).x);
					}
					switch (instance_wrap)
					{
					case InstanceWrapMode.Clamp:
						num = Mathf.Min(num + 1, p_template_indexes.Count - 1);
						break;
					case InstanceWrapMode.Repeat:
						num = (num + 1) % p_template_indexes.Count;
						break;
					}
				}
			});
		}

		public void Generate(MELayoutSpline p_surface, bool p_rebuild, List<int> p_template_indexes)
		{
			if (!p_surface)
			{
				return;
			}
			int instance_count = instanceCount;
			InstanceWrapMode instance_wrap = instancePatternWrap;
			float x = layoutMarginRange.x;
			float y = layoutMarginRange.y;
			Vector2 scaleRange = layoutScaleRange;
			Transform c = container;
			p_surface.spacing = layoutSpacing;
			p_surface.orientStep = layoutOrientStep;
			p_surface.Generate(instance_count, x, y, delegate
			{
				bool flag = p_rebuild || instance_count != instances.Count;
				if (flag)
				{
					List<GameObject> list = new List<GameObject>();
					for (int i = 0; i < c.childCount; i++)
					{
						Transform child = c.GetChild(i);
						list.Add(child.gameObject);
					}
					while (list.Count > 0)
					{
						Object.Destroy(list[0]);
						list.RemoveAt(0);
					}
				}
				int num = 0;
				int num2 = 0;
				int num3 = instance_count;
				for (int j = 0; j < num3; j++)
				{
					if (num2++ > 1000)
					{
						Debug.LogWarning("Infinite LOOP!");
						break;
					}
					int num4 = ((num >= p_template_indexes.Count) ? (-1) : (p_template_indexes[num] - 1));
					MARenderer mARenderer = null;
					MapEditorController editor = controller.ui.editor;
					if (templates.Count > 0 && num4 >= 0)
					{
						MARenderer mARenderer2 = templates[num4];
						TransformVector transformVector = new TransformVector(mARenderer2.transform);
						if (!flag)
						{
							mARenderer = instances[j];
							if (!mARenderer)
							{
								flag = true;
								instances.RemoveAt(j--);
							}
						}
						if (flag)
						{
							mARenderer = (MARenderer)editor.view.factory.Instantiate(mARenderer2.data, c);
							mARenderer.name = mARenderer.name.Replace("(Clone)", "");
							instances.Add(mARenderer);
						}
						else
						{
							mARenderer = instances[j];
						}
						mARenderer.transform.SetSiblingIndex(instances.Count);
						TransformVector transformVector2 = p_surface.Get(j);
						mARenderer.transform.position = transformVector2.position;
						mARenderer.transform.localRotation = transformVector.rotation * transformVector2.rotation;
						mARenderer.transform.localScale = transformVector.scale * Mathf.Lerp(scaleRange.x, scaleRange.y, p_surface.GetRandom(j).x);
					}
					switch (instance_wrap)
					{
					case InstanceWrapMode.Clamp:
						num = Mathf.Min(num + 1, p_template_indexes.Count - 1);
						break;
					case InstanceWrapMode.Repeat:
						num = (num + 1) % p_template_indexes.Count;
						break;
					}
				}
			});
		}
	}
}
