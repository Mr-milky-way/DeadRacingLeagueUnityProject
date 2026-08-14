using System.Collections.Generic;

namespace drl.game
{
	public class MEInspectorFieldIds
	{
		public const string TransformPosition = "transform-position";

		public const string TransformRotation = "transform-rotation";

		public const string TransformScale = "transform-scale";

		public const string TransformModularScale = "transform-modular-scale";

		public static string[] TransformFields = new string[4] { "transform-position", "transform-rotation", "transform-scale", "transform-modular-scale" };

		public const string TransformSnapGround = "transform-snap-ground";

		public const string TransformSnapCamera = "transform-snap-camera";

		public const string LayoutAligment = "layout-aligment";

		public const string LayoutAlign = "layout-align";

		public const string LayoutAlignNX = "layout-align-nx";

		public const string LayoutAlignNY = "layout-align-ny";

		public const string LayoutAlignNZ = "layout-align-nz";

		public const string LayoutAlignPX = "layout-align-px";

		public const string LayoutAlignPY = "layout-align-py";

		public const string LayoutAlignPZ = "layout-align-pz";

		public const string LayoutAlignXC = "layout-align-xc";

		public const string LayoutAlignYC = "layout-align-yc";

		public const string LayoutAlignZC = "layout-align-zc";

		public const string LayoutDistribute = "layout-distribute";

		public const string LayoutDistributeApply = "layout-distribute-apply";

		public const string LayoutDistributeSpacing = "layout-distribute-spacing";

		public const string LayoutDistributeApplyX = "layout-distribute-apply-x";

		public const string LayoutDistributeApplyY = "layout-distribute-apply-y";

		public const string LayoutDistributeApplyZ = "layout-distribute-apply-z";

		public const string LayoutDistributeApplyD = "layout-distribute-apply-d";

		public const string LayoutOrient = "layout-orient";

		public const string LayoutOrientApply = "layout-orient-apply";

		public const string LayoutOrientOffset = "layout-orient-offset";

		public const string LayoutOrientSmooth = "layout-orient-smooth";

		public const string LayoutOrientSmoothApply = "layout-orient-smooth-apply";

		public const string LayoutOrientFlat = "layout-orient-flat";

		public const string LayoutOrientFlatApply = "layout-orient-flat-apply";

		public const string LayoutGeometryAttribs0 = "lgt-attribs-0";

		public const string LayoutGeometryType = "lgt-type";

		public const string LayoutGeometryFill = "lgt-fill";

		public const string LayoutGeometryVisibility = "lgt-visibility";

		public const string LayoutGeometryStats = "lgt-stats";

		public const string LayoutGeometryAttribsAsset = "lgt-attribs-asset";

		public const string LayoutGeometryAssetSize = "lgt-asset-size";

		public const string LayoutGeometryAssetMargin = "lgt-asset-margin";

		public const string LayoutGeometryAssetDensity = "lgt-asset-density";

		public const string LayoutGeometryAttribsShape = "lgt-attribs-shape";

		public const string LayoutGeometryShapeRadius = "lgt-shape-radius";

		public const string LayoutGeometryShapeHeight = "lgt-shape-height";

		public const string LayoutGeometryShapeAperture = "lgt-shape-aperture";

		public const string LayoutGeometryGridSize = "lgt-grid-size";

		public const string LayoutGeometryRandom = "lgt-random";

		public const string LayoutGeometryAttribs1 = "lgt-attribs-1";

		public const string LayoutGeometrySliceOffset = "lgt-slices-offset";

		public const string LayoutGeometrySliceSize = "lgt-slices-size";

		public const string LayoutGeometryApply = "lgt-apply";

		public static string[] LayoutGeometryShapeStats = new string[7] { "lgt-stats", "lgt-shape-radius", "lgt-shape-height", "lgt-shape-aperture", "lgt-grid-size", "lgt-attribs-shape", "lgt-apply" };

		public const string AttribRuler = "attrib-ruler";

		public const string AttribLayout = "attrib-layout";

		public const string GateEnabled = "gate-enabled";

		public const string GateMode = "gate-mode";

		public const string GateAttribs0 = "gate-attribs-0";

		public const string GateFinish = "gate-finish";

		public const string GateLapStart = "gate-lap-start";

		public const string GateLapEnd = "gate-lap-end";

		public static string[] GateLapLogicFields = new string[3] { "gate-finish", "gate-lap-start", "gate-lap-end" };

		public const string GateRespawnGuideVisible = "gate-respawn-visible";

		public const string SplineRaceLine = "spline-race-line";

		public const string SplineCategory = "spline-category";

		public const string SplineSmooth = "spline-smooth";

		public const string SplineLoop = "spline-loop";

		public const string SplineAlpha = "spline-alpha";

		public const string SplineThickness = "spline-thickness";

		public const string SplineStartWidth = "spline-start-width";

		public const string SplineEndWidth = "spline-end-width";

		public const string SplineSnapGates = "spline-snap-gates";

		public const string SplineSnapClosestGate = "spline-snap-closest-gate";

		public const string SplineControlPointIndex = "spline-control-point-index";

		public const string SplineCourseCameraSpeed = "spline-course-camera-speed";

		public const string SplineCourseCameraFOV = "spline-course-camera-fov";

		public const string SplineCourseCameraIndex = "spline-course-camera-index";

		public const string SplineSnapSelectNext = "spline-snap-select-next";

		public const string SplineAttribs0 = "spline-attribs-0";

		public const string SplineAttribs1 = "spline-attribs-1";

		public const string SplineAttribs2 = "spline-attribs-2";

		public const string SplineAttribs3 = "spline-attribs-3";

		public const string SplineAttribs4 = "spline-attribs-4";

		public const string SplineCourseCameraPreview = "spl-course-camera-preview";

		public const string SplineCourseCameraToggle = "spl-course-camera-toggle";

		public const string SplineCourseCameraPreviewExpand = "spl-course-camera-preview-expand";

		public static string[] SplineFields = new string[21]
		{
			"spline-race-line", "spline-category", "spline-smooth", "spline-loop", "spline-alpha", "spline-thickness", "spline-start-width", "spline-end-width", "spline-snap-gates", "spline-snap-closest-gate",
			"spline-control-point-index", "spline-attribs-0", "spline-attribs-1", "spline-attribs-2", "spline-attribs-3", "spline-attribs-4", "spl-course-camera-preview", "spl-course-camera-preview-expand", "spline-course-camera-speed", "spline-course-camera-fov",
			"spline-course-camera-index"
		};

		public const string PhysicsSimulationToggle = "physics-simulation-toggle";

		public const string CameraToolControlPointAttribs0 = "ctcp-attribs-0";

		public const string CameraToolControlPointCameraTrackingMode = "ctcp-camera-tracking-mode";

		public const string CameraToolControlPointCameraTrackingDelay = "ctcp-camera-tracking-delay";

		public const string CameraToolControlPointCameraOrbitAngle = "ctcp-camera-orbit-angle";

		public const string CameraToolControlPointAttribs1 = "ctcp-attribs-1";

		public const string CameraToolControlPointCameraDistance = "ctcp-camera-distance";

		public const string CameraToolControlPointCameraFOV = "ctcp-camera-fov";

		public const string CameraToolControlPointCameraOffset = "ctcp-camera-offset";

		public const string CameraToolAttribs0 = "ct-attribs-0";

		public const string CameraToolCameraEasing = "ct-camera-easing";

		public const string CameraToolCameraEasingTest = "ct-camera-easing-test";

		public const string CameraToolCameraEasingHelp = "ct-camera-easing-help";

		public const string CameraToolPreview = "ct-preview";

		public const string CameraToolPreviewExpand = "ct-preview-expand";

		public const string CameraToolIndex = "ct-index";

		public const string CollectableAttribs0 = "collectable-attribs-0";

		public const string CollectableAttribs1 = "collectable-attribs-1";

		public const string CollectableMode = "collectable-mode";

		public const string CollectableStyle0 = "collectable-style-0";

		public const string CollectableSize = "collectable-size";

		public const string CollectableScore = "collectable-score";

		public const string MaterialColorEmission = "material-color-emission";

		public const string MaterialColorIntensity = "material-color-intensity";

		public const string MaterialColor0 = "material-color-0";

		public const string MaterialColor1 = "material-color-1";

		public const string MaterialColor2 = "material-color-2";

		public static string[] MaterialColorFields = new string[5] { "material-color-intensity", "material-color-emission", "material-color-0", "material-color-1", "material-color-2" };

		public const string MaterialColorReset = "material-color-reset";

		public const string MaterialStyle0 = "material-style-0";

		public const string MaterialStyle1 = "material-style-1";

		public const string MaterialStyle2 = "material-style-2";

		public static string[] MaterialStyleFields = new string[3] { "material-style-0", "material-style-1", "material-style-2" };

		public const string MaterialStyleReset = "material-style-reset";

		public const string MapAttribs0 = "map-attribs-0";

		public const string MapAttribs1 = "map-attribs-1";

		public const string MapAttribs2 = "map-attribs-2";

		public const string MapAttribs3 = "map-attribs-3";

		public const string MapAttribs4 = "map-attribs-4";

		public const string MapTitle = "map-title";

		public const string MapTrackId = "map-track-id";

		public const string MapCategory = "map-category";

		public const string MapDifficulty = "map-difficulty";

		public const string MapVisiblity = "map-visibility";

		public const string MapAllowCopy = "map-allow-copy";

		public const string MapThumb = "map-thumb";

		public const string MapLighting = "map-lighting";

		public const string MapAssetLayer0 = "map-asset-layer-0";

		public const string MapAssetLayer1 = "map-asset-layer-1";

		public const string MapAssetLayer2 = "map-asset-layer-2";

		public const string MapStyle0 = "map-style-0";

		public const string MapStyle1 = "map-style-1";

		public const string MapStyle2 = "map-style-2";

		public const string MapBaseAssets = "map-base-assets";

		public const string MapCollabs = "map-collabs";

		public const string MapCollabList = "map-collab-list";

		public const string MapLaps = "map-laps";

		public const string PrefsMapSave = "prefs-map-save";

		public const string MapAutoSave = "map-auto-save";

		public const string MapSave = "map-save";

		public const string PrefsReplayCache = "prefs-replay-cache";

		public const string ReplayCacheDelete = "replay-cache-delete";

		public const string PhysicsLabel = "physics-label";

		public const string PhysicsDropVelocity = "physics-drop-velocity";

		public const string PhysicsDropVelocityUp = "physics-drop-v-up";

		public const string PhysicsDropVelocityForward = "physics-drop-v-forward";

		public const string PhysicsDropSpin = "physics-drop-spin";

		public const string PhysicsDropTiming = "physics-drop-timing";

		public const string PhysicsDropDelay = "physics-drop-delay";

		public const string PhysicsDropDuration = "physics-drop-duration";

		public static List<string> PropertiesUndoFields = new List<string>
		{
			"spline-race-line", "spline-category", "spline-smooth", "spline-loop", "spline-alpha", "spline-thickness", "spline-start-width", "spline-end-width", "spline-snap-gates", "spline-control-point-index",
			"spline-course-camera-speed", "spline-course-camera-fov", "spline-course-camera-index", "spline-snap-select-next", "ctcp-camera-fov", "ctcp-camera-distance", "ctcp-camera-offset", "ctcp-camera-orbit-angle", "ctcp-camera-tracking-delay", "ctcp-camera-tracking-mode",
			"ct-camera-easing", "material-color-reset", "material-color-emission", "material-color-intensity", "material-color-0", "material-color-1", "material-color-2", "material-style-0", "material-style-1", "material-style-2",
			"attrib-ruler", "gate-enabled", "gate-mode", "gate-finish", "gate-lap-start", "gate-lap-end", "gate-respawn-visible", "ct-index", "lgt-type", "lgt-fill",
			"lgt-visibility", "lgt-stats", "lgt-asset-size", "lgt-asset-margin", "lgt-asset-density", "lgt-attribs-shape", "lgt-shape-radius", "lgt-shape-height", "lgt-shape-aperture", "lgt-grid-size",
			"lgt-random", "lgt-slices-offset", "lgt-slices-size", "collectable-mode", "collectable-style-0", "collectable-size", "collectable-score"
		};

		public static List<string> TransformUndoFields = new List<string>
		{
			"layout-orient", "layout-distribute", "layout-align", "transform-snap-ground", "transform-snap-camera", "spline-snap-closest-gate", "transform-position", "transform-rotation", "transform-scale", "transform-modular-scale",
			"physics-simulation-toggle"
		};
	}
}
