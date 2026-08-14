using System;
using AK.Wwise;
using UnityEngine;

[Obsolete("This functionality is deprecated as of Wwise v2019.2.0 and will be removed in a future release.")]
public class AkSpatialAudioEmitter : MonoBehaviour
{
	[Header("Early Reflections")]
	[Tooltip("(DEPRECATED) As of 2019.2, the early reflections auxiliary bus can be set per sound, in the Authoring tool, or per game object, with the AkEarlyReflections component.")]
	public AuxBus reflectAuxBus = new AuxBus();

	[Tooltip("(DEPRECATED) As of 2019.2, the Reflection Max Path Length is set by the sound's Attenuation Max Distance value in the Authoring tool.")]
	public float reflectionMaxPathLength = 1000f;

	[Range(0f, 1f)]
	[Tooltip("(DEPRECATED) As of 2019.2, the early reflections send volume can be set per sound, in the Authoring tool, or for all sunds playing on a game object, with the AkEarlyReflections component.")]
	public float reflectionsAuxBusGain = 1f;

	[Tooltip("(DEPRECATED) As of 2019.2, the Reflection Order is set in the Spatial Audio Initialization Settings.")]
	public uint reflectionsOrder = 1u;

	[Header("Rooms")]
	[Tooltip("(DEPRECATED) As of 2019.2, the Room Reverb Aux Bus Gain is set by the Game-Defined Auxiliary Sends Volume in the Sound Property Editor in the Authoring tool.")]
	public float roomReverbAuxBusGain = 1f;

	[Header("Geometric Diffraction")]
	[Tooltip("(DEPRECATED) As of 2019.2, diffraction is enabled in the Sound Property Editor in the Authoring tool.")]
	public uint diffractionMaxEdges;

	[Tooltip("(DEPRECATED) As of 2019.2, diffraction is enabled in the Sound Property Editor in the Authoring tool.")]
	public uint diffractionMaxPaths;

	[Tooltip("(DEPRECATED) As of 2019.2, diffraction is enabled in the Sound Property Editor in the Authoring tool.")]
	public uint diffractionMaxPathLength;
}
