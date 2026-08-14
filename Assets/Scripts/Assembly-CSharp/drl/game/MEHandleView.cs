using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MEHandleView : View<DRLApp>
	{
		public MoveHandle move;

		public RotateHandle rotate;

		public ScaleHandle scale;

		public HandleModeType mode;

		public List<TransformVector> from;

		public List<TransformVector> to;

		public bool moving
		{
			get
			{
				if (!move.gameObject.activeInHierarchy && !rotate.gameObject.activeInHierarchy && !scale.gameObject.activeInHierarchy)
				{
					return false;
				}
				if (move.moving)
				{
					return true;
				}
				if (rotate.moving)
				{
					return true;
				}
				if (scale.moving)
				{
					return true;
				}
				return false;
			}
		}

		public MapEditorView editor => AssertParent<MapEditorView>("editor");

		protected void Awake()
		{
			mode = HandleModeType.None;
			move.callback.AddListener(OnHandleEvent);
			rotate.callback.AddListener(OnHandleEvent);
			scale.callback.AddListener(OnHandleEvent);
		}

		public void Refresh()
		{
			if (move.gameObject.activeInHierarchy)
			{
				move.Refresh();
			}
			if (rotate.gameObject.activeInHierarchy)
			{
				rotate.Refresh();
			}
			if (scale.gameObject.activeInHierarchy)
			{
				scale.Refresh();
			}
		}

		public void SetHandle(HandleModeType p_type, IList p_targets)
		{
			TRSHandle tRSHandle = null;
			move.gameObject.SetActive(value: false);
			rotate.gameObject.SetActive(value: false);
			scale.gameObject.SetActive(value: false);
			switch (p_type)
			{
			case HandleModeType.Move:
				tRSHandle = move;
				break;
			case HandleModeType.Rotate:
				tRSHandle = rotate;
				break;
			case HandleModeType.Scale:
				tRSHandle = scale;
				break;
			}
			mode = p_type;
			if (!tRSHandle)
			{
				return;
			}
			List<Transform> list = new List<Transform>();
			bool flag = false;
			if (p_targets != null)
			{
				for (int i = 0; i < p_targets.Count; i++)
				{
					object obj = p_targets[i];
					if (obj is Component)
					{
						Component component = obj as Component;
						if (component is MAEntity)
						{
							MAEntity mAEntity = component as MAEntity;
							flag = flag || mAEntity.tags.Contains(MapAssetType.ModularScale);
						}
						Transform item = component.transform;
						list.Add(item);
					}
				}
			}
			if (list.Count > 0)
			{
				switch (p_type)
				{
				case HandleModeType.Scale:
					scale.rate = (flag ? 0.2f : 1f);
					scale.useDelta = flag;
					break;
				}
				tRSHandle.gameObject.SetActive(value: true);
				tRSHandle.targets = list;
			}
		}

		public void SetHandle(HandleModeType p_type)
		{
			SetHandle(p_type, null);
		}

		public void SetHandlesInputEnabled(bool p_flag)
		{
			move.SetHandlesKeyboardEnabled(p_flag);
			rotate.SetHandlesKeyboardEnabled(p_flag);
			scale.SetHandlesKeyboardEnabled(p_flag);
			move.SetHandlesMouseEnabled(p_flag);
			rotate.SetHandlesMouseEnabled(p_flag);
			scale.SetHandlesMouseEnabled(p_flag);
		}

		public void SetHandlePivot(MEHandlePivotType p_type)
		{
			bool global = p_type == MEHandlePivotType.Global;
			move.global = global;
			rotate.global = global;
			scale.global = global;
			Refresh();
		}

		protected void OnHandleEvent(GizmoHandleEvent p_event)
		{
			TRSHandle tRSHandle = null;
			if (!tRSHandle)
			{
				tRSHandle = ((p_event.target == move) ? move : (p_event.target.transform.IsChildOf(move.transform) ? move : null));
			}
			if (!tRSHandle)
			{
				tRSHandle = ((p_event.target == rotate) ? rotate : (p_event.target.transform.IsChildOf(rotate.transform) ? rotate : null));
			}
			if (!tRSHandle)
			{
				tRSHandle = ((p_event.target == scale) ? scale : (p_event.target.transform.IsChildOf(scale.transform) ? scale : null));
			}
			switch (p_event.type)
			{
			case HandleEventType.KeyDown:
				Notify("map-editor.handle@down", tRSHandle);
				break;
			case HandleEventType.Down:
				Notify("map-editor.handle@down", tRSHandle);
				break;
			case HandleEventType.DragStart:
				Notify("map-editor.handle@start", tRSHandle);
				break;
			case HandleEventType.Drag:
				Notify("map-editor.handle@update", tRSHandle);
				break;
			case HandleEventType.KeyHold:
				Notify("map-editor.handle@update", tRSHandle);
				break;
			case HandleEventType.DragEnd:
				Notify("map-editor.handle@drag-end", tRSHandle);
				break;
			case HandleEventType.KeyUp:
				Notify("map-editor.handle@drag-end", tRSHandle);
				break;
			}
		}
	}
}
