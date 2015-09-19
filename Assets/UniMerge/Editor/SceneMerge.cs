//Matt Schoen
//5-29-2013
//
// This software is the copyrighted material of its author, Matt Schoen, and his company Defective Studios.
// It is available for sale on the Unity Asset store and is subject to their restrictions and limitations, as well as
// the following: You shall not reproduce or re-distribute this software without the express written (e-mail is fine)
// permission of the author. If permission is granted, the code (this file and related files) must bear this license 
// in its entirety. Anyone who purchases the script is welcome to modify and re-use the code at their personal risk 
// and under the condition that it not be included in any distribution builds. The software is provided as-is without 
// warranty and the author bears no responsibility for damages or losses caused by the software.  
// This Agreement becomes effective from the day you have installed, copied, accessed, downloaded and/or otherwise used
// the software.

#define DEV			//Comment this out to not auto-populate scene merge

using System.Collections;
using UniMerge;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SceneMerge : EditorWindow {
	const string messagePath = "Assets/merges.txt";
	public static Object mine, theirs;
	private static bool merged;
	//If these names end up conflicting with names within your scene, change them here
	public const string mineContainerName = "mine", theirsContainerName = "theirs";
	public static GameObject mineContainer, theirsContainer;

	public static float colWidth;

	static SceneData mySceneData, theirSceneData;
	Vector2 scroll;

	private bool renderSettingsFoldout, lightmapsFoldout;
	private static bool loading;

	[MenuItem("Window/UniMerge/Scene Merge %&m")]
	static void Init() {
		GetWindow(typeof(SceneMerge));
	}
#if DEV
	//If these names end up conflicting with names within your project, change them here
	public const string mineSceneName = "Mine", theirsSceneName = "Theirs";
	void OnEnable() {
	//Get path
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		//Unity 3 path stuff?
#else
		string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
		UniMergeConfig.DEFAULT_PATH = scriptPath.Substring(0, scriptPath.IndexOf("Editor") - 1);
#endif
		if(Directory.Exists(UniMergeConfig.DEFAULT_PATH + "/Demo/Scene Merge")){
			string[] assets = Directory.GetFiles(UniMergeConfig.DEFAULT_PATH + "/Demo/Scene Merge");
			foreach(var asset in assets) {
				if(asset.EndsWith(".unity")) {
					if(asset.Contains(mineSceneName)) {
						mine = AssetDatabase.LoadAssetAtPath(asset.Replace('\\', '/'), typeof(Object));
					}
					if(asset.Contains(theirsSceneName)) {
						theirs = AssetDatabase.LoadAssetAtPath(asset.Replace('\\', '/'), typeof(Object));
					}
				}
			}
		}
		if (EditorPrefs.HasKey(ObjectMerge.RowHeightKey)) {
			ObjectMerge.selectedRowHeight = EditorPrefs.GetInt(ObjectMerge.RowHeightKey);
		}
		loading = false;
	}
#endif
	void OnDestroy() {
		EditorPrefs.SetInt("RowHeight", ObjectMerge.selectedRowHeight);
	}

	private void OnGUI() {
		if (loading) {
			GUILayout.Label("Loading...");
			return;
		}
#if  UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
	//Layout fix for older versions?
#else
		EditorGUIUtility.labelWidth = 100;
#endif
		//Ctrl + w to close
		if (Event.current.Equals(Event.KeyboardEvent("^w"))) {
			Close();
			GUIUtility.ExitGUI();
		}
		/*
		 * SETUP
		 */
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		EditorGUIUtility.LookLikeControls();
#endif
		ObjectMerge.alt = false;
		//Adjust colWidth as the window resizes
		colWidth = (position.width - UniMergeConfig.midWidth * 2 - UniMergeConfig.margin) / 2;
#if UNITY_5
		if (mine == null || theirs == null)					//TODO: check if valid scene object
#else
		if (mine == null || theirs == null
			|| mine.GetType() != typeof (Object) || mine.GetType() != typeof (Object)
			) //|| !AssetDatabase.GetAssetPath(mine).Contains(".unity") || !AssetDatabase.GetAssetPath(theirs).Contains(".unity"))
#endif
			merged = GUI.enabled = false;
		if (GUILayout.Button("Merge")) {
			loading = true;
			Merge(mine, theirs);
			GUIUtility.ExitGUI();
		}
		GUI.enabled = merged;
		GUILayout.BeginHorizontal();
		{
			GUI.enabled = mineContainer;
			if (!GUI.enabled)
				merged = false;
			if (GUILayout.Button("Unpack Mine")) {
				DestroyImmediate(theirsContainer);
				List<Transform> tmp = new List<Transform>();
				foreach (Transform t in mineContainer.transform)
					tmp.Add(t);
				foreach (Transform t in tmp)
					t.parent = null;
				DestroyImmediate(mineContainer);
				mySceneData.ApplySettings();
			}
			GUI.enabled = theirsContainer;
			if (!GUI.enabled)
				merged = false;
			if (GUILayout.Button("Unpack Theirs")) {
				DestroyImmediate(mineContainer);
				List<Transform> tmp = new List<Transform>();
				foreach (Transform t in theirsContainer.transform)
					tmp.Add(t);
				foreach (Transform t in tmp)
					t.parent = null;
				DestroyImmediate(theirsContainer);
				theirSceneData.ApplySettings();
			}
		}
		GUILayout.EndHorizontal();

		GUI.enabled = true;
		ObjectMerge.DrawRowHeight();

		GUILayout.BeginHorizontal();
		{
			GUILayout.BeginVertical(GUILayout.Width(colWidth));
			{
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				mine = EditorGUILayout.ObjectField("Mine", mine, typeof(Object));
#else
				mine = EditorGUILayout.ObjectField("Mine", mine, typeof (Object), true);
#endif
			}
			GUILayout.EndVertical();
			GUILayout.Space(UniMergeConfig.midWidth * 2);
			GUILayout.BeginVertical(GUILayout.Width(colWidth));
			{
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				theirs = EditorGUILayout.ObjectField("Theirs", theirs, typeof(Object));
#else
				theirs = EditorGUILayout.ObjectField("Theirs", theirs, typeof (Object), true);
#endif
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
		if (mine == null || theirs == null)
			merged = false;
		if (merged) {
			scroll = GUILayout.BeginScrollView(scroll);
			ObjectMerge.StartRow(RenderSettingsCompare(mySceneData, theirSceneData));
#if UNITY_5
			renderSettingsFoldout = EditorGUILayout.Foldout(renderSettingsFoldout, "Lighting");
#else
			renderSettingsFoldout = EditorGUILayout.Foldout(renderSettingsFoldout, "Render Settings");
#endif
			ObjectMerge.EndRow();
			if (renderSettingsFoldout) {
				//Fog
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.fog == theirSceneData.fog,
					left = delegate { mySceneData.fog = EditorGUILayout.Toggle("Fog", mySceneData.fog); },
					leftButton = delegate { mySceneData.fog = theirSceneData.fog; },
					rightButton = delegate { theirSceneData.fog = mySceneData.fog; },
					right = delegate { theirSceneData.fog = EditorGUILayout.Toggle("Fog", theirSceneData.fog); },
					drawButtons = true
				});
				//Fog Color
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.fogColor == theirSceneData.fogColor,
					left = delegate { mySceneData.fogColor = EditorGUILayout.ColorField("Fog Color", mySceneData.fogColor); },
					leftButton = delegate { mySceneData.fogColor = theirSceneData.fogColor; },
					rightButton = delegate { theirSceneData.fogColor = mySceneData.fogColor; },
					right = delegate { theirSceneData.fogColor = EditorGUILayout.ColorField("Fog Color", theirSceneData.fogColor); },
					drawButtons = true
				});
				//Fog Mode
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.fogMode == theirSceneData.fogMode,
					left = delegate { mySceneData.fogMode = (FogMode) EditorGUILayout.EnumPopup("Fog Mode", mySceneData.fogMode); },
					leftButton = delegate { mySceneData.fogMode = theirSceneData.fogMode; },
					rightButton = delegate { theirSceneData.fogMode = mySceneData.fogMode; },
					right = delegate { theirSceneData.fogMode = (FogMode) EditorGUILayout.EnumPopup("Fog Mode", theirSceneData.fogMode); },
					drawButtons = true
				});
				//Fog Density
#if UNITY_5
				string label = "Density";
#else
				string label = "Linear Density";
#endif
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.fogDensity == theirSceneData.fogDensity,
					left = delegate { mySceneData.fogDensity = EditorGUILayout.FloatField(label, mySceneData.fogDensity); },
					leftButton = delegate { mySceneData.fogDensity = theirSceneData.fogDensity; },
					rightButton = delegate { theirSceneData.fogDensity = mySceneData.fogDensity; },
					right = delegate { theirSceneData.fogDensity = EditorGUILayout.FloatField(label, theirSceneData.fogDensity); },
					drawButtons = true
				});
				//Linear Fog Start
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.fogStartDistance == theirSceneData.fogStartDistance,
					left = delegate { mySceneData.fogStartDistance = EditorGUILayout.FloatField("Linear Fog Start", mySceneData.fogStartDistance); },
					leftButton = delegate { mySceneData.fogStartDistance = theirSceneData.fogStartDistance; },
					rightButton = delegate { theirSceneData.fogStartDistance = mySceneData.fogStartDistance; },
					right = delegate { theirSceneData.fogStartDistance = EditorGUILayout.FloatField("Linear Fog Start", theirSceneData.fogStartDistance); },
					drawButtons = true
				});
				//Linear Fog End
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.fogEndDistance == theirSceneData.fogEndDistance,
					left = delegate { mySceneData.fogEndDistance = EditorGUILayout.FloatField("Linear Fog End", mySceneData.fogEndDistance); },
					leftButton = delegate { mySceneData.fogEndDistance = theirSceneData.fogEndDistance; },
					rightButton = delegate { theirSceneData.fogEndDistance = mySceneData.fogEndDistance; },
					right = delegate { theirSceneData.fogEndDistance = EditorGUILayout.FloatField("Linear Fog End", theirSceneData.fogEndDistance); },
					drawButtons = true
				});
				//Ambient Light
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.ambientLight == theirSceneData.ambientLight,
					left = delegate { mySceneData.ambientLight = EditorGUILayout.ColorField("Ambient Light", mySceneData.ambientLight); },
					leftButton = delegate { mySceneData.ambientLight = theirSceneData.ambientLight; },
					rightButton = delegate { theirSceneData.ambientLight = mySceneData.ambientLight; },
					right = delegate { theirSceneData.ambientLight = EditorGUILayout.ColorField("Ambient Light", theirSceneData.ambientLight); },
					drawButtons = true
				});
				//Skybox
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.skybox == theirSceneData.skybox,
					left = delegate {
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
						mySceneData.skybox = (Material)EditorGUILayout.ObjectField("Skybox Material", mySceneData.skybox, typeof(Material));
#else
						mySceneData.skybox = (Material) EditorGUILayout.ObjectField("Skybox Material", mySceneData.skybox, typeof (Material), false);
#endif
					},
					leftButton = delegate { mySceneData.skybox = theirSceneData.skybox; },
					rightButton = delegate { theirSceneData.skybox = mySceneData.skybox; },
					right = delegate {
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
						theirSceneData.skybox = (Material)EditorGUILayout.ObjectField("Skybox Material", theirSceneData.skybox, typeof(Material));
#else
						theirSceneData.skybox = (Material) EditorGUILayout.ObjectField("Skybox Material", theirSceneData.skybox, typeof (Material), false);
#endif
					},
					drawButtons = true
				});
				//Halo Strength
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.haloStrength == theirSceneData.haloStrength,
					left = delegate { mySceneData.haloStrength = EditorGUILayout.FloatField("Halo Strength", mySceneData.haloStrength); },
					leftButton = delegate { mySceneData.haloStrength = theirSceneData.haloStrength; },
					rightButton = delegate { theirSceneData.haloStrength = mySceneData.haloStrength; },
					right = delegate { theirSceneData.haloStrength = EditorGUILayout.FloatField("Halo Strength", theirSceneData.haloStrength); },
					drawButtons = true
				});
				//Flare Strength
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.flareStrength == theirSceneData.flareStrength,
					left = delegate { mySceneData.flareStrength = EditorGUILayout.FloatField("Flare Strength", mySceneData.flareStrength); },
					leftButton = delegate { mySceneData.flareStrength = theirSceneData.flareStrength; },
					rightButton = delegate { theirSceneData.flareStrength = mySceneData.flareStrength; },
					right = delegate { theirSceneData.flareStrength = EditorGUILayout.FloatField("Flare Strength", theirSceneData.flareStrength); },
					drawButtons = true
				});
				//Flare Fade Speed
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.flareFadeSpeed == theirSceneData.flareFadeSpeed,
					left = delegate { mySceneData.flareFadeSpeed = EditorGUILayout.FloatField("Flare Fade Speed", mySceneData.flareFadeSpeed); },
					leftButton = delegate { mySceneData.flareFadeSpeed = theirSceneData.flareFadeSpeed; },
					rightButton = delegate { theirSceneData.flareFadeSpeed = mySceneData.flareFadeSpeed; },
					right = delegate { theirSceneData.flareFadeSpeed = EditorGUILayout.FloatField("Flare Fade Speed", theirSceneData.flareFadeSpeed); },
					drawButtons = true
				});
			}
			ObjectMerge.StartRow(LightmapSettingsCompare(mySceneData, theirSceneData));
			lightmapsFoldout = EditorGUILayout.Foldout(lightmapsFoldout, "Lightmap Settings");
			ObjectMerge.EndRow();
			if (lightmapsFoldout) {
#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
				//BakedColorSpace
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.bakedColorSpace == theirSceneData.bakedColorSpace,
					left = delegate { mySceneData.bakedColorSpace = (ColorSpace) EditorGUILayout.EnumPopup("Baked Color Space", mySceneData.bakedColorSpace); },
					leftButton = delegate { mySceneData.bakedColorSpace = theirSceneData.bakedColorSpace; },
					rightButton = delegate { theirSceneData.bakedColorSpace = mySceneData.bakedColorSpace; },
					right = delegate { theirSceneData.bakedColorSpace = (ColorSpace)EditorGUILayout.EnumPopup("Baked Color Space", theirSceneData.bakedColorSpace); },
					drawButtons = true
				});
				//LightProbes
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.lightProbes == theirSceneData.lightProbes,
					left = delegate { mySceneData.lightProbes = (LightProbes)EditorGUILayout.ObjectField("Light Probes", mySceneData.lightProbes, typeof(LightProbes), false); },
					leftButton = delegate { mySceneData.lightProbes = theirSceneData.lightProbes; },
					rightButton = delegate { theirSceneData.lightProbes = mySceneData.lightProbes; },
					right = delegate { theirSceneData.lightProbes = (LightProbes)EditorGUILayout.ObjectField("Light Probes", theirSceneData.lightProbes, typeof(LightProbes), false); },
					drawButtons = true
				});
#endif
				//Lightmaps--do this later
				//Lightmaps mode
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.lightmapsMode == theirSceneData.lightmapsMode,
					left = delegate { mySceneData.lightmapsMode = (LightmapsMode)EditorGUILayout.EnumPopup("Lightmaps Mode", mySceneData.lightmapsMode); },
					leftButton = delegate { mySceneData.lightmapsMode = theirSceneData.lightmapsMode; },
					rightButton = delegate { theirSceneData.lightmapsMode = mySceneData.lightmapsMode; },
					right = delegate { theirSceneData.lightmapsMode = (LightmapsMode)EditorGUILayout.EnumPopup("Lightmaps Mode", theirSceneData.lightmapsMode); },
					drawButtons = true
				});
#if !UNITY_5
				//Quality
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.quality == theirSceneData.quality,
					left = delegate { mySceneData.quality = (LightmapBakeQuality)EditorGUILayout.EnumPopup("Quality", mySceneData.quality); },
					leftButton = delegate { mySceneData.quality = theirSceneData.quality; },
					rightButton = delegate { theirSceneData.quality = mySceneData.quality; },
					right = delegate { theirSceneData.quality = (LightmapBakeQuality)EditorGUILayout.EnumPopup("Quality", theirSceneData.quality); },
					drawButtons = true
				});
				//Bounces
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.bounces == theirSceneData.bounces,
					left = delegate { mySceneData.bounces = EditorGUILayout.IntField("Bounces", mySceneData.bounces); },
					leftButton = delegate { mySceneData.bounces = theirSceneData.bounces; },
					rightButton = delegate { theirSceneData.bounces = mySceneData.bounces; },
					right = delegate { theirSceneData.bounces = EditorGUILayout.IntField("Bounces", theirSceneData.bounces); },
					drawButtons = true
				});
#endif
				//Sky Light Color
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.skyLightColor == theirSceneData.skyLightColor,
					left = delegate { mySceneData.skyLightColor = EditorGUILayout.ColorField("Sky Light Color", mySceneData.skyLightColor); },
					leftButton = delegate { mySceneData.skyLightColor = theirSceneData.skyLightColor; },
					rightButton = delegate { theirSceneData.skyLightColor = mySceneData.skyLightColor; },
					right = delegate { theirSceneData.skyLightColor = EditorGUILayout.ColorField("Sky Light Color", theirSceneData.skyLightColor); },
					drawButtons = true
				});
				//Sky Light Intensity
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.skyLightIntensity == theirSceneData.skyLightIntensity,
					left = delegate { mySceneData.skyLightIntensity = EditorGUILayout.FloatField("Sky Light Intensity", mySceneData.skyLightIntensity); },
					leftButton = delegate { mySceneData.skyLightIntensity = theirSceneData.skyLightIntensity; },
					rightButton = delegate { theirSceneData.skyLightIntensity = mySceneData.skyLightIntensity; },
					right = delegate { theirSceneData.skyLightIntensity = EditorGUILayout.FloatField("Sky Light Intensity", theirSceneData.skyLightIntensity); },
					drawButtons = true
				});
#if !UNITY_5
				//Bounce Boost
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.bounceBoost == theirSceneData.bounceBoost,
					left = delegate { mySceneData.bounceBoost = EditorGUILayout.FloatField("Bounce Boost", mySceneData.bounceBoost); },
					leftButton = delegate { mySceneData.bounceBoost = theirSceneData.bounceBoost; },
					rightButton = delegate { theirSceneData.bounceBoost = mySceneData.bounceBoost; },
					right = delegate { theirSceneData.bounceBoost = EditorGUILayout.FloatField("Bounce Boost", theirSceneData.bounceBoost); },
					drawButtons = true
				});
				//Bounce Intensity
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.bounceIntensity == theirSceneData.bounceIntensity,
					left = delegate { mySceneData.bounceIntensity = EditorGUILayout.FloatField("Bounce Boost", mySceneData.bounceIntensity); },
					leftButton = delegate { mySceneData.bounceIntensity = theirSceneData.bounceIntensity; },
					rightButton = delegate { theirSceneData.bounceIntensity = mySceneData.bounceIntensity; },
					right = delegate { theirSceneData.bounceIntensity = EditorGUILayout.FloatField("Bounce Boost", theirSceneData.bounceIntensity); },
					drawButtons = true
				});
				//Final Gather Rays
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.finalGatherRays == theirSceneData.finalGatherRays,
					left = delegate { mySceneData.finalGatherRays = EditorGUILayout.IntField("Final Gather Rays", mySceneData.finalGatherRays); },
					leftButton = delegate { mySceneData.finalGatherRays = theirSceneData.finalGatherRays; },
					rightButton = delegate { theirSceneData.finalGatherRays = mySceneData.finalGatherRays; },
					right = delegate { theirSceneData.finalGatherRays = EditorGUILayout.IntField("Final Gather Rays", theirSceneData.finalGatherRays); },
					drawButtons = true
				});
				//Final Gather Contrast Threshold
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.finalGatherContrastThreshold == theirSceneData.finalGatherContrastThreshold,
					left = delegate { mySceneData.finalGatherContrastThreshold = EditorGUILayout.FloatField("FG Contrast Threshold", mySceneData.finalGatherContrastThreshold); },
					leftButton = delegate { mySceneData.finalGatherContrastThreshold = theirSceneData.finalGatherContrastThreshold; },
					rightButton = delegate { theirSceneData.finalGatherContrastThreshold = mySceneData.finalGatherContrastThreshold; },
					right = delegate { theirSceneData.finalGatherContrastThreshold = EditorGUILayout.FloatField("FG Contrast Threshold", theirSceneData.finalGatherContrastThreshold); },
					drawButtons = true
				});
				//Final Gather Gradient Threshold
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.finalGatherGradientThreshold == theirSceneData.finalGatherGradientThreshold,
					left = delegate { mySceneData.finalGatherGradientThreshold = EditorGUILayout.FloatField("FG Gradient Threshold", mySceneData.finalGatherGradientThreshold); },
					leftButton = delegate { mySceneData.finalGatherGradientThreshold = theirSceneData.finalGatherGradientThreshold; },
					rightButton = delegate { theirSceneData.finalGatherGradientThreshold = mySceneData.finalGatherGradientThreshold; },
					right = delegate { theirSceneData.finalGatherGradientThreshold = EditorGUILayout.FloatField("FG Gradient Threshold", theirSceneData.finalGatherGradientThreshold); },
					drawButtons = true
				});
				//Final Gather Interpolation Points
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.finalGatherInterpolationPoints == theirSceneData.finalGatherInterpolationPoints,
					left = delegate { mySceneData.finalGatherInterpolationPoints = EditorGUILayout.IntField("FG Interpolation Points", mySceneData.finalGatherInterpolationPoints); },
					leftButton = delegate { mySceneData.finalGatherInterpolationPoints = theirSceneData.finalGatherInterpolationPoints; },
					rightButton = delegate { theirSceneData.finalGatherInterpolationPoints = mySceneData.finalGatherInterpolationPoints; },
					right = delegate { theirSceneData.finalGatherInterpolationPoints = EditorGUILayout.IntField("FG Interpolation Points", theirSceneData.finalGatherInterpolationPoints); },
					drawButtons = true
				});
				//AO Amount
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.aoAmount == theirSceneData.aoAmount,
					left = delegate { mySceneData.aoAmount = EditorGUILayout.FloatField("Ambient Occlusion", mySceneData.aoAmount); },
					leftButton = delegate { mySceneData.aoAmount = theirSceneData.aoAmount; },
					rightButton = delegate { theirSceneData.aoAmount = mySceneData.aoAmount; },
					right = delegate { theirSceneData.aoAmount = EditorGUILayout.FloatField("Ambient Occlusion", theirSceneData.aoAmount); },
					drawButtons = true
				});
				//AO Contrast
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.aoContrast == theirSceneData.aoContrast,
					left = delegate { mySceneData.aoContrast = EditorGUILayout.FloatField("AO Contrast", mySceneData.aoContrast); },
					leftButton = delegate { mySceneData.aoContrast = theirSceneData.aoContrast; },
					rightButton = delegate { theirSceneData.aoContrast = mySceneData.aoContrast; },
					right = delegate { theirSceneData.aoContrast = EditorGUILayout.FloatField("AO Contrast", theirSceneData.aoContrast); },
					drawButtons = true
				});		
				//AO Max Distance
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.aoMaxDistance == theirSceneData.aoMaxDistance,
					left = delegate { mySceneData.aoMaxDistance = EditorGUILayout.FloatField("AO Max Distance", mySceneData.aoMaxDistance); },
					leftButton = delegate { mySceneData.aoMaxDistance = theirSceneData.aoMaxDistance; },
					rightButton = delegate { theirSceneData.aoMaxDistance = mySceneData.aoMaxDistance; },
					right = delegate { theirSceneData.aoMaxDistance = EditorGUILayout.FloatField("AO Contrast", theirSceneData.aoMaxDistance); },
					drawButtons = true
				});
				//Lock Atlas
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.lockAtlas == theirSceneData.lockAtlas,
					left = delegate { mySceneData.lockAtlas = EditorGUILayout.Toggle("Lock Atlas", mySceneData.lockAtlas); },
					leftButton = delegate { mySceneData.lockAtlas = theirSceneData.lockAtlas; },
					rightButton = delegate { theirSceneData.lockAtlas = mySceneData.lockAtlas; },
					right = delegate { theirSceneData.lockAtlas = EditorGUILayout.Toggle("Lock Atlas", theirSceneData.lockAtlas); },
					drawButtons = true
				});
				//Last Used Resolution
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.lastUsedResolution == theirSceneData.lastUsedResolution,
					left = delegate { mySceneData.lastUsedResolution = EditorGUILayout.FloatField("Last Used Resolution", mySceneData.lastUsedResolution); },
					leftButton = delegate { mySceneData.lastUsedResolution = theirSceneData.lastUsedResolution; },
					rightButton = delegate { theirSceneData.lastUsedResolution = mySceneData.lastUsedResolution; },
					right = delegate { theirSceneData.lastUsedResolution = EditorGUILayout.FloatField("Last Used Resolution", theirSceneData.lastUsedResolution); },
					drawButtons = true
				});
#endif
				//Resolution
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.resolution == theirSceneData.resolution,
					left = delegate { mySceneData.resolution = EditorGUILayout.FloatField("Resolution", mySceneData.resolution); },
					leftButton = delegate { mySceneData.resolution = theirSceneData.resolution; },
					rightButton = delegate { theirSceneData.resolution = mySceneData.resolution; },
					right = delegate { theirSceneData.resolution = EditorGUILayout.FloatField("Resolution", theirSceneData.resolution); },
					drawButtons = true
				});
#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
				//Padding
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.padding == theirSceneData.padding,
					left = delegate { mySceneData.padding = EditorGUILayout.IntField("Padding", mySceneData.padding); },
					leftButton = delegate { mySceneData.padding = theirSceneData.padding; },
					rightButton = delegate { theirSceneData.padding = mySceneData.padding; },
					right = delegate { theirSceneData.padding = EditorGUILayout.IntField("Padding", theirSceneData.padding); },
					drawButtons = true
				});
#endif
				//Max Atlas Height
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.maxAtlasHeight == theirSceneData.maxAtlasHeight,
					left = delegate { mySceneData.maxAtlasHeight = EditorGUILayout.IntField("Max Atlas Height", mySceneData.maxAtlasHeight); },
					leftButton = delegate { mySceneData.maxAtlasHeight = theirSceneData.maxAtlasHeight; },
					rightButton = delegate { theirSceneData.maxAtlasHeight = mySceneData.maxAtlasHeight; },
					right = delegate { theirSceneData.maxAtlasHeight = EditorGUILayout.IntField("Max Atlas Height", theirSceneData.maxAtlasHeight); },
					drawButtons = true
				});
				//Max Atlas Width
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.maxAtlasWidth == theirSceneData.maxAtlasWidth,
					left = delegate { mySceneData.maxAtlasWidth = EditorGUILayout.IntField("Max Atlas Width", mySceneData.maxAtlasWidth); },
					leftButton = delegate { mySceneData.maxAtlasWidth = theirSceneData.maxAtlasWidth; },
					rightButton = delegate { theirSceneData.maxAtlasWidth = mySceneData.maxAtlasWidth; },
					right = delegate { theirSceneData.maxAtlasWidth = EditorGUILayout.IntField("Max Atlas Width", theirSceneData.maxAtlasWidth); },
					drawButtons = true
				});
				//Texture Compression
				ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
					indent = UniMerge.Util.TAB_SIZE,
					colWidth = colWidth,
					compare = () => mySceneData.textureCompression == theirSceneData.textureCompression,
					left = delegate { mySceneData.textureCompression = EditorGUILayout.Toggle("Tex Compression", mySceneData.textureCompression); },
					leftButton = delegate { mySceneData.fog = theirSceneData.textureCompression; },
					rightButton = delegate { theirSceneData.textureCompression = mySceneData.textureCompression; },
					right = delegate { theirSceneData.fog = EditorGUILayout.Toggle("Tex Compression", theirSceneData.textureCompression); },
					drawButtons = true
				});
			}
			GUILayout.EndScrollView();
		}
	}

	static bool RenderSettingsCompare(SceneData mySceneData, SceneData theirSceneData) {
		if (mySceneData.fog != theirSceneData.fog)
			return false;
		if (mySceneData.fogColor != theirSceneData.fogColor)
			return false;
		if (mySceneData.fogMode != theirSceneData.fogMode)
			return false;
		if (mySceneData.fogDensity != theirSceneData.fogDensity)
			return false;
		if (mySceneData.fogStartDistance != theirSceneData.fogStartDistance)
			return false;
		if (mySceneData.fogEndDistance != theirSceneData.fogEndDistance)
			return false;
		if (mySceneData.ambientLight != theirSceneData.ambientLight)
			return false;
		if (mySceneData.skybox != theirSceneData.skybox)
			return false;
		if (mySceneData.haloStrength != theirSceneData.haloStrength)
			return false;
		if (mySceneData.flareStrength != theirSceneData.flareStrength)
			return false;
		if (mySceneData.flareFadeSpeed != theirSceneData.flareFadeSpeed)
			return false;
		return true;
	}
	static bool LightmapSettingsCompare(SceneData mySceneData, SceneData theirSceneData) {
#if !UNITY_5
		if (mySceneData.aoAmount != theirSceneData.aoAmount)
			return false;
		if (mySceneData.aoContrast != theirSceneData.aoContrast)
			return false;
		if (mySceneData.bounceBoost != theirSceneData.bounceBoost)
			return false;
		if (mySceneData.bounceIntensity != theirSceneData.bounceIntensity)
			return false;
		if (mySceneData.finalGatherContrastThreshold != theirSceneData.finalGatherContrastThreshold)
			return false;
		if (mySceneData.lastUsedResolution != theirSceneData.lastUsedResolution)
			return false;
		if (mySceneData.bounces != theirSceneData.bounces)
			return false;
		if (mySceneData.finalGatherInterpolationPoints != theirSceneData.finalGatherInterpolationPoints)
			return false;
		if (mySceneData.finalGatherRays != theirSceneData.finalGatherRays)
			return false;
		if (mySceneData.lockAtlas != theirSceneData.lockAtlas)
			return false;
#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
		if (mySceneData.padding != theirSceneData.padding)
			return false;
#endif
#endif
#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
		if (mySceneData.bakedColorSpace != theirSceneData.bakedColorSpace)
			return false;
		if (mySceneData.lightProbes != theirSceneData.lightProbes)
			return false;
#endif
		//if (mySceneData.lightmaps != theirSceneData.lightmaps)
		//	return false;
		if (mySceneData.lightmapsMode != theirSceneData.lightmapsMode)
			return false;
		if (mySceneData.aoMaxDistance != theirSceneData.aoMaxDistance)
			return false;
		if (mySceneData.resolution != theirSceneData.resolution)
			return false;
		if (mySceneData.skyLightIntensity != theirSceneData.skyLightIntensity)
			return false;
		if (mySceneData.maxAtlasHeight != theirSceneData.maxAtlasHeight)
			return false;
		if (mySceneData.maxAtlasWidth != theirSceneData.maxAtlasWidth)
			return false;
		if (mySceneData.textureCompression != theirSceneData.textureCompression)
			return false;
		if (mySceneData.skyLightColor != theirSceneData.skyLightColor)
			return false;
		return true;
	}
	public static void CLIIn() {
		string[] args = System.Environment.GetCommandLineArgs();
		foreach(string arg in args)
			Debug.Log(arg);
		Merge(
			args[args.Length - 2].Substring(args[args.Length - 2].IndexOf("Assets")).Replace("\\", "/").Trim(),
			args[args.Length - 1].Substring(args[args.Length - 1].IndexOf("Assets")).Replace("\\", "/").Trim());
	}
	void Update() {
		if (cancelRefresh) {
			refresh = null;
		}
		if (refresh != null) {
			if (!refresh.MoveNext())
				refresh = null;
			Repaint();
		}
		cancelRefresh = false;

		TextAsset mergeFile = (TextAsset)AssetDatabase.LoadAssetAtPath(messagePath, typeof(TextAsset));
		if (mergeFile) {
			string[] files = mergeFile.text.Split('\n');
			AssetDatabase.DeleteAsset(messagePath);
			for (int i = 0; i < files.Length; i++) {
				if (!files[i].StartsWith("Assets")) {
					if (files[i].IndexOf("Assets") > -1)
						files[i] = files[i].Substring(files[i].IndexOf("Assets")).Replace("\\", "/").Trim();
				}
			}
			DoMerge(files);
		}
	}
	public static void PrefabMerge(string myPath, string theirPath) {
		GetWindow(typeof(ObjectMerge));
		ObjectMerge.mine = (GameObject)AssetDatabase.LoadAssetAtPath(myPath, typeof(GameObject));
		ObjectMerge.theirs = (GameObject)AssetDatabase.LoadAssetAtPath(theirPath, typeof(GameObject));
	}
	public static IEnumerator refresh;
	public static bool cancelRefresh = false;
	public static void DoMerge(string[] paths) {
		if(paths.Length > 2) {
			Merge(paths[0], paths[1]);
		} else Debug.LogError("need at least 2 paths, " + paths.Length + " given");
	}
	public static void Merge(Object myScene, Object theirScene) {
		if(myScene == null || theirScene == null)
			return;
		Merge(AssetDatabase.GetAssetPath(myScene), AssetDatabase.GetAssetPath(theirScene));
	}

	public static void Merge(string myPath, string theirPath) {
		refresh = MergeAsync(myPath, theirPath);
	}

	public static IEnumerator MergeAsync(string myPath, string theirPath){
		if(string.IsNullOrEmpty(myPath) || string.IsNullOrEmpty(theirPath))
			yield break;
		if (myPath.EndsWith("prefab") || theirPath.EndsWith("prefab")) {
			PrefabMerge(myPath, theirPath);
			yield break;
		}
		if(AssetDatabase.LoadAssetAtPath(myPath, typeof(Object)) && AssetDatabase.LoadAssetAtPath(theirPath, typeof(Object))) {
			if(EditorApplication.SaveCurrentSceneIfUserWantsTo()) {
				/*
				 * Get scene data (render settings, lightmaps)
				 */
				//Load "theirs" to get RenderSettings, etc.
				yield return null;
				EditorApplication.OpenScene(theirPath);
				theirSceneData.CaptureSettings();

				//Load "mine" to start the merge
				yield return null;
				EditorApplication.OpenScene(myPath);
				mySceneData.CaptureSettings();

				/*
				 * Start Merge
				 */
				mineContainer = new GameObject {name = mineContainerName};
				GameObject[] allObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				foreach(GameObject obj in allObjects) {
					if(obj.transform.parent == null
						&& EditorUtility.GetPrefabType(obj) != PrefabType.Prefab
						&& EditorUtility.GetPrefabType(obj) != PrefabType.ModelPrefab
						&& obj.hideFlags == 0)		//Want a better way to filter out "internal" objects
						obj.transform.parent = mineContainer.transform;
				}
#else
				foreach(GameObject obj in allObjects) {
					if(obj.transform.parent == null
						&& PrefabUtility.GetPrefabType(obj) != PrefabType.Prefab
						&& PrefabUtility.GetPrefabType(obj) != PrefabType.ModelPrefab
						&& obj.hideFlags == 0)		//Want a better way to filter out "internal" objects
						obj.transform.parent = mineContainer.transform;
				}
#endif
				EditorApplication.OpenSceneAdditive(theirPath);
				theirsContainer = new GameObject {name = theirsContainerName};
				allObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				foreach(GameObject obj in allObjects) {
					if(obj.transform.parent == null && obj.name != mineContainerName
						&& EditorUtility.GetPrefabType(obj) != PrefabType.Prefab
						&& EditorUtility.GetPrefabType(obj) != PrefabType.ModelPrefab
						&& obj.hideFlags == 0)		//Want a better way to filter out "internal" objects
						obj.transform.parent = theirsContainer.transform;
				}
#else
				foreach(GameObject obj in allObjects) {
					if(obj.transform.parent == null && obj.name != mineContainerName
						&& PrefabUtility.GetPrefabType(obj) != PrefabType.Prefab
						&& PrefabUtility.GetPrefabType(obj) != PrefabType.ModelPrefab
						&& obj.hideFlags == 0)		//Want a better way to filter out "internal" objects
						obj.transform.parent = theirsContainer.transform;
				}
#endif
				yield return null;
				ObjectMerge.root = null;
				GetWindow(typeof(ObjectMerge));
				ObjectMerge.mine = mineContainer;
				ObjectMerge.theirs = theirsContainer;
				yield return null;
				merged = true;
			}
		}
		loading = false;
		yield break;
	}
}
public struct SceneData {
	/*
	 * Render Settings
	 */
	public Color	ambientLight, 
					fogColor;

	public float	flareFadeSpeed,
					flareStrength,
					fogDensity,
					fogEndDistance,
					fogStartDistance,
					haloStrength;

	public bool		fog;

	public FogMode	fogMode;

	public Material skybox;

	//Hmm... can't get at haloTexture or spotCookie
	//public Texture haloTexture, spotCookie;

	/*
	 * Lightmap Settings
	 */
#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
	public ColorSpace bakedColorSpace;
	public LightProbes lightProbes;
#endif
	public LightmapData[] lightmaps;
	public LightmapsMode lightmapsMode;

	/*
	 * Lightmap Editor Settings
	 */
	//Where is "use in forward rendering?"
	//TODO: Use serialized object representation
#if UNITY_5
#else
	public float	aoAmount,
					aoContrast,
					bounceBoost,
					bounceIntensity,
					finalGatherContrastThreshold,
					finalGatherGradientThreshold,
					lastUsedResolution;

	public int		bounces,
					finalGatherInterpolationPoints,
					finalGatherRays;
	public bool		lockAtlas;
#endif
#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
	public int padding;
#endif
	public float	aoMaxDistance,
					resolution,
					skyLightIntensity;
					
	public int		maxAtlasHeight,
					maxAtlasWidth;

	public bool		textureCompression;

	public Color	skyLightColor;

#if UNITY_5

#else
	public LightmapBakeQuality quality;
#endif
	//Nav mesh settings can't be accessed via script :(

	//Lightmapping is VERY version-specific. You may have to modify the settings that are compared
	public void CaptureSettings() {
#if UNITY_5
#else
		aoAmount = LightmapEditorSettings.aoAmount;
		aoContrast = LightmapEditorSettings.aoContrast;
		bounceBoost = LightmapEditorSettings.bounceBoost;	 
		bounceIntensity = LightmapEditorSettings.bounceIntensity;
		bounces = LightmapEditorSettings.bounces;
		
		quality = LightmapEditorSettings.quality;
		skyLightColor = LightmapEditorSettings.skyLightColor;
		skyLightIntensity = LightmapEditorSettings.skyLightIntensity;

		finalGatherContrastThreshold = LightmapEditorSettings.finalGatherContrastThreshold;
		finalGatherGradientThreshold = LightmapEditorSettings.finalGatherGradientThreshold;
		finalGatherInterpolationPoints = LightmapEditorSettings.finalGatherInterpolationPoints;
		finalGatherRays = LightmapEditorSettings.finalGatherRays;
		lastUsedResolution = LightmapEditorSettings.lastUsedResolution;
		lockAtlas = LightmapEditorSettings.lockAtlas;
#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
		bakedColorSpace = LightmapSettings.bakedColorSpace;
#endif
#endif
		ambientLight = RenderSettings.ambientLight;	   
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#else
		flareFadeSpeed = RenderSettings.flareFadeSpeed;
		lightProbes = LightmapSettings.lightProbes;
		padding = LightmapEditorSettings.padding;
#endif
		flareStrength = RenderSettings.flareStrength;
		fog = RenderSettings.fog;
		fogColor = RenderSettings.fogColor;
		fogDensity = RenderSettings.fogDensity;
		fogEndDistance = RenderSettings.fogEndDistance;
		fogMode = RenderSettings.fogMode;
		fogStartDistance = RenderSettings.fogStartDistance;
		haloStrength = RenderSettings.haloStrength;
		skybox = RenderSettings.skybox;

		lightmaps = LightmapSettings.lightmaps;
		lightmapsMode = LightmapSettings.lightmapsMode;

		aoMaxDistance = LightmapEditorSettings.aoMaxDistance;
		maxAtlasHeight = LightmapEditorSettings.maxAtlasHeight;
		maxAtlasWidth = LightmapEditorSettings.maxAtlasWidth;
		resolution = LightmapEditorSettings.resolution;
		textureCompression = LightmapEditorSettings.textureCompression;
	}
	public void ApplySettings() {
#if UNITY_5
#else		   
		LightmapEditorSettings.aoAmount = aoAmount;
		LightmapEditorSettings.aoContrast = aoContrast;

		LightmapEditorSettings.bounceBoost = bounceBoost;
		LightmapEditorSettings.bounceIntensity = bounceIntensity;
		LightmapEditorSettings.bounces = bounces;
		LightmapEditorSettings.finalGatherContrastThreshold = finalGatherContrastThreshold;
		LightmapEditorSettings.finalGatherGradientThreshold = finalGatherGradientThreshold;
		LightmapEditorSettings.finalGatherInterpolationPoints = finalGatherInterpolationPoints;
		LightmapEditorSettings.finalGatherRays = finalGatherRays;
		LightmapEditorSettings.lastUsedResolution = lastUsedResolution;
		LightmapEditorSettings.lockAtlas = lockAtlas;

		LightmapEditorSettings.quality = quality;
		LightmapEditorSettings.skyLightColor = skyLightColor;
		LightmapEditorSettings.skyLightIntensity = skyLightIntensity;

#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
		LightmapSettings.bakedColorSpace = bakedColorSpace;
#endif
#endif
		RenderSettings.ambientLight = ambientLight;
		RenderSettings.fogColor = fogColor;
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#else
		RenderSettings.flareFadeSpeed = flareFadeSpeed;
		LightmapSettings.lightProbes = lightProbes;
#endif
		RenderSettings.flareStrength = flareStrength;
		RenderSettings.fogDensity = fogDensity;
		RenderSettings.fogEndDistance = fogEndDistance;
		RenderSettings.fogStartDistance = fogStartDistance;
		RenderSettings.haloStrength = haloStrength;
		RenderSettings.fog = fog;
		RenderSettings.fogMode = fogMode;
		RenderSettings.skybox = skybox;

		LightmapSettings.lightmaps = lightmaps;
		LightmapSettings.lightmapsMode = lightmapsMode;

		LightmapEditorSettings.aoMaxDistance = aoMaxDistance;
		LightmapEditorSettings.maxAtlasHeight = maxAtlasHeight;
		LightmapEditorSettings.maxAtlasWidth = maxAtlasWidth;
#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
		LightmapEditorSettings.padding = padding;
#endif
		LightmapEditorSettings.resolution = resolution;
		LightmapEditorSettings.textureCompression = textureCompression;
	}
}