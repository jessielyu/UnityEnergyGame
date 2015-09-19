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

using UnityEditor;
using UnityEngine;

public class UniMergeConfig : EditorWindow {
	// GUI Settings
	public static string DEFAULT_GUI_SKIN_FILENAME = "Skin/UniMerge.guiskin";
	public static string DARK_GUI_SKIN_FILENAME = "Skin/UniMergeDark.guiskin";

	public static string DEFAULT_PATH = "Assets/UniMerge";

	// list normal is a list style with normal font size
	public static string LIST_STYLE_NAME = "List";
	public static string LIST_ALT_STYLE_NAME = "ListAlt";
	public static string CONFLICT_SUFFIX = "Conflict";

	public const float margin = 87;			//Amount of extra space
	public static float midWidth = 25;

	Vector2 scroll;
	[MenuItem("Window/UniMerge/UniMerge Config")]
	static void Init() {
		GetWindow(typeof(UniMergeConfig));
	}
	void OnGUI() {
		//Ctrl + w to close
		if(Event.current.Equals(Event.KeyboardEvent("^w"))){
			Close();
			GUIUtility.ExitGUI();
		}
		GUILayout.Label("To change these values, set them in UniMergeConfig.cs");
		EditorGUILayout.LabelField("Default Skin", DEFAULT_GUI_SKIN_FILENAME);
		EditorGUILayout.LabelField("Dark Skin", DARK_GUI_SKIN_FILENAME);
		EditorGUILayout.LabelField("Plugin Path", DEFAULT_PATH);
		EditorGUILayout.LabelField("List Style Name", LIST_STYLE_NAME);
		EditorGUILayout.LabelField("List Alt Style Name", LIST_ALT_STYLE_NAME);
		EditorGUILayout.LabelField("Conflit Suffix", CONFLICT_SUFFIX);
		
		GUILayout.Label(new GUIContent("Note: In order to save your changes you must make\nsome change to the skin in the editor.",
			"This is because modifying the skin via script doesn't set it \"dirty\""));
		
		scroll = EditorGUILayout.BeginScrollView(scroll);
		GUISkin defaultSkin = AssetDatabase.LoadAssetAtPath(DEFAULT_PATH + "/" + DEFAULT_GUI_SKIN_FILENAME, typeof(GUISkin)) as GUISkin;
		if(defaultSkin) {
			if(defaultSkin.customStyles.Length < 4) {
				GUIStyle[] newStyles = new GUIStyle[4];
				defaultSkin.customStyles.CopyTo(newStyles, 0);
				defaultSkin.customStyles = newStyles;
			}
			for(int i = 0; i < defaultSkin.customStyles.Length; i++)
				if(defaultSkin.customStyles[i] == null)
					defaultSkin.customStyles[i] = new GUIStyle();
			defaultSkin.customStyles[0].name = LIST_STYLE_NAME;
			defaultSkin.customStyles[1].name = LIST_ALT_STYLE_NAME;
			defaultSkin.customStyles[2].name = LIST_STYLE_NAME + CONFLICT_SUFFIX;
			defaultSkin.customStyles[3].name = LIST_ALT_STYLE_NAME + CONFLICT_SUFFIX;
			GUILayout.Label("Default Skin background colors");
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
			defaultSkin.customStyles[0].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_STYLE_NAME, defaultSkin.customStyles[0].normal.background, typeof(Texture2D));
			defaultSkin.customStyles[1].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_ALT_STYLE_NAME, defaultSkin.customStyles[1].normal.background, typeof(Texture2D));
			defaultSkin.customStyles[2].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_STYLE_NAME + CONFLICT_SUFFIX, defaultSkin.customStyles[2].normal.background, typeof(Texture2D));
			defaultSkin.customStyles[3].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_ALT_STYLE_NAME + CONFLICT_SUFFIX, defaultSkin.customStyles[3].normal.background, typeof(Texture2D));
		} else GUILayout.Label("Oops! No Light Skin found!");
#else
			defaultSkin.customStyles[0].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_STYLE_NAME, defaultSkin.customStyles[0].normal.background, typeof(Texture2D), false);
			defaultSkin.customStyles[1].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_ALT_STYLE_NAME, defaultSkin.customStyles[1].normal.background, typeof(Texture2D), false);
			defaultSkin.customStyles[2].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_STYLE_NAME + CONFLICT_SUFFIX, defaultSkin.customStyles[2].normal.background, typeof(Texture2D), false);
			defaultSkin.customStyles[3].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_ALT_STYLE_NAME + CONFLICT_SUFFIX, defaultSkin.customStyles[3].normal.background, typeof(Texture2D), false);
		} else GUILayout.Label("<color=red>Oops! No Light Skin found!</color>");
#endif

		GUISkin darkSkin = AssetDatabase.LoadAssetAtPath(DEFAULT_PATH + "/" + DARK_GUI_SKIN_FILENAME, typeof(GUISkin)) as GUISkin;
		if(darkSkin) {
			if(darkSkin.customStyles.Length < 4) {
				GUIStyle[] newStyles = new GUIStyle[4];
				darkSkin.customStyles.CopyTo(newStyles, 0);
				darkSkin.customStyles = newStyles;
			}
			for(int i = 0; i < darkSkin.customStyles.Length; i++)
				if(darkSkin.customStyles[i] == null)
					darkSkin.customStyles[i] = new GUIStyle();
			darkSkin.customStyles[0].name = LIST_STYLE_NAME;
			darkSkin.customStyles[1].name = LIST_ALT_STYLE_NAME;
			darkSkin.customStyles[2].name = LIST_STYLE_NAME + CONFLICT_SUFFIX;
			darkSkin.customStyles[3].name = LIST_ALT_STYLE_NAME + CONFLICT_SUFFIX;
			GUILayout.Label("Dark Skin background colors");
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
			darkSkin.customStyles[0].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_STYLE_NAME, darkSkin.customStyles[0].normal.background, typeof(Texture2D));
			darkSkin.customStyles[1].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_ALT_STYLE_NAME, darkSkin.customStyles[1].normal.background, typeof(Texture2D));
			darkSkin.customStyles[2].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_STYLE_NAME + CONFLICT_SUFFIX, darkSkin.customStyles[2].normal.background, typeof(Texture2D));
			darkSkin.customStyles[3].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_ALT_STYLE_NAME + CONFLICT_SUFFIX, darkSkin.customStyles[3].normal.background, typeof(Texture2D));
		} else GUILayout.Label("Oops! No Dark Skin found!");
#else
			darkSkin.customStyles[0].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_STYLE_NAME, darkSkin.customStyles[0].normal.background, typeof(Texture2D), false);
			darkSkin.customStyles[1].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_ALT_STYLE_NAME, darkSkin.customStyles[1].normal.background, typeof(Texture2D), false);
			darkSkin.customStyles[2].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_STYLE_NAME + CONFLICT_SUFFIX, darkSkin.customStyles[2].normal.background, typeof(Texture2D), false);
			darkSkin.customStyles[3].normal.background = (Texture2D)EditorGUILayout.ObjectField(LIST_ALT_STYLE_NAME + CONFLICT_SUFFIX, darkSkin.customStyles[3].normal.background, typeof(Texture2D), false);
		} else GUILayout.Label("<color=red>Oops! No Dark Skin found!</color>");
#endif
		EditorGUILayout.EndScrollView();
	}
}