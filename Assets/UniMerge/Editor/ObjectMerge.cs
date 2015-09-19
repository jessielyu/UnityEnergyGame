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


//With the DEV flag defined, the ObjectMege window will, by default, search for GameObjects called mine and theirs
//in the scene and put them in mine and theirs.  This is so that when using the demo scene, I don't have to reset 
//those references after each compile.  I leave it on by default so that there is one less step when users first
//see the tool.  Comment this line out if this behavior somehow interferes with your workflow.
#define DEV

using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class ObjectMerge : EditorWindow {
	public const string mineDemoName = "Mine", theirsDemoName = "Theirs";

	public static ObjectHelper root, lastRoot;
	public static bool alt;
	public static float colWidth, winHeight, lastWinHeight;
	public static GameObject mine, theirs;
	public static int refreshCount, totalRefreshNum;
	const int PROGRESS_BAR_HEIGHT = 15;

	public const int foldoutPadding = 4;
	
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
	public const int objectPadding = -1;
	public const int basePaddingTop = 0;
	public const int basePaddingBot = 0;
#else
	public const int objectPadding = -3;
	public const int basePaddingTop = 3;
	public const int basePaddingBot = -2;
#endif

	public const string RowHeightKey = "RowHeight";
	static readonly int[] rowHeights = new[] { 10, 5, 0 };
	public static int rowPadding = 10;
	public static int selectedRowHeight;
	public const int componentsMargin = 10;

#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
	static string filters = "", lastFilters;
	public static List<System.Type> filterTypes;
	List<string> badTypes;
	List<string> notComponents;
	System.Reflection.Assembly[] assemblies;
#else
	string[][] componentTypeStrings;
	public static System.Type[][] componentTypes;
#endif

	public static bool deepCopy = true;
	public static bool log = false;
	public static bool compareAttrs = true;
	public static int[] typeMask;

	public static IEnumerator refresh;
	public static bool cancelRefresh = false;
	
	public static bool SkinSetUp;

	public static Vector2 scroll;

	//timing variables
	static Stopwatch frameTimer = new Stopwatch();
	public static int maxFrameTime = 30;

	private static int yieldCountMax = 10000;
	private static int yieldCountCurr = 3000;	//Wait this many iterations before trying yield
	private const int hammerTimeCut = 25;
	private static int yieldCountMin = 25;
	static int yieldIterCount = 0;

	private static int frameTimeWiggleRoom = 3;
	private static bool hammerTime = false;

	public static int ObjectDrawCount = 0;
	public static int ObjectDrawCursor = 0;
	public static int ObjectDrawOffset = 0;
	public static int ObjectDrawWindow = 10;
	public static float ObjectDrawHeight = 0;

	[MenuItem("Window/UniMerge/Object Merge %m")]
	static void Init() {
		GetWindow(typeof(ObjectMerge));
	}
	void OnEnable() {
#if UNITY_5
		Application.logMessageReceived += HandleLog;
#else
		Application.RegisterLogCallback(HandleLog);
#endif
		root = null;
		//Get path
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		//Unity 3 path stuff?
#else
		string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
		UniMergeConfig.DEFAULT_PATH = scriptPath.Substring(0, scriptPath.IndexOf("Editor") - 1);
#endif

		//Component filters
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
		//NB: For some reason, after a compile, filters starts out as "", though the field retains the value.  Then when it's modified the string is set... as a result sometime you see filter text with nothing being filtered
		ParseFilters();
#else
		var subclasses =
		from assembly in System.AppDomain.CurrentDomain.GetAssemblies()
		from type in assembly.GetTypes()
		where type.IsSubclassOf(typeof(Component))
		select type;
		List<List<string>> compTypeStrs = new List<List<string>> {new List<string>()};
		List<List<System.Type>> compTypes = new List<List<System.Type>> {new List<System.Type>()};
		int setCount = 0;
		foreach(System.Type t in subclasses) {
			if(compTypes[setCount].Count == 31) {
				setCount++;
				compTypeStrs.Add(new List<string>());
				compTypes.Add(new List<System.Type>());
			}
			compTypeStrs[setCount].Add(t.Name);
			compTypes[setCount].Add(t);
		}
		componentTypes = new System.Type[setCount + 1][];
		componentTypeStrings = new string[setCount + 1][];
		typeMask = new int[setCount + 1];
		for(int i = 0; i < setCount + 1; i++) {
			typeMask[i] = -1;
			componentTypes[i] = compTypes[i].ToArray();
			componentTypeStrings[i] = compTypeStrs[i].ToArray();
		}
#endif
#if DEV
		mine = GameObject.Find(mineDemoName);
		theirs = GameObject.Find(theirsDemoName);
#endif
		SkinSetUp = false;

		if (EditorPrefs.HasKey(RowHeightKey)) {
			selectedRowHeight = EditorPrefs.GetInt(RowHeightKey);
		}
	}
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
	void ParseFilters(){
		filterTypes = new List<System.Type>();
		badTypes = new List<string>();
		notComponents = new List<string>();
		string[] tmp = filters.Replace(" ", "").Split(',');
		foreach(string filter in tmp){
			if(!string.IsNullOrEmpty(filter)){
				bool found = false;
				foreach(System.Reflection.Assembly asm in assemblies){
					foreach(System.Type t in asm.GetTypes()){
						if(t.Name.ToLower() == filter.ToLower()){
							if(t.IsSubclassOf(typeof(Component))){
								filterTypes.Add(t);
							} else notComponents.Add(filter);
							found = true;
							break;
						}
					}
					if(found)
						break;
				}
				if(!found)
					badTypes.Add(filter);
			}
		}
	}
#endif
	void OnDisable() {
#if UNITY_5
		Application.logMessageReceived -= HandleLog;
#else
		Application.RegisterLogCallback(null);
#endif
	}

	void OnDestroy() {
		EditorPrefs.SetInt(RowHeightKey, selectedRowHeight);
	}
	void SetUpSkin(GUISkin builtinSkin) {
		SkinSetUp = true;
		//Set up skin. We add the styles from the custom skin because there are a bunch (467!) of built in custom styles
		string guiSkinToUse = UniMergeConfig.DEFAULT_GUI_SKIN_FILENAME;
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4
		//Alternate detection of dark skin
		if(UnityEditorInternal.InternalEditorUtility.HasPro() && EditorPrefs.GetInt("UserSkin") == 1)
			guiSkinToUse = UniMergeConfig.DARK_GUI_SKIN_FILENAME;
#else
		if(EditorGUIUtility.isProSkin)
			guiSkinToUse = UniMergeConfig.DARK_GUI_SKIN_FILENAME;
#endif
		GUISkin usedSkin = AssetDatabase.LoadAssetAtPath(UniMergeConfig.DEFAULT_PATH + "/" + guiSkinToUse, typeof(GUISkin)) as GUISkin;

		if(usedSkin) {
			//GUISkin builtinSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			List<GUIStyle> customStyles = new List<GUIStyle>(builtinSkin.customStyles);
			//Clear styles from last enable, or for light/dark switch
			for(int i = 0; i < builtinSkin.customStyles.Length; i++) {
				if(builtinSkin.customStyles[i].name == UniMergeConfig.LIST_STYLE_NAME
					|| builtinSkin.customStyles[i].name == UniMergeConfig.LIST_ALT_STYLE_NAME
					|| builtinSkin.customStyles[i].name == UniMergeConfig.LIST_STYLE_NAME + UniMergeConfig.CONFLICT_SUFFIX
					|| builtinSkin.customStyles[i].name == UniMergeConfig.LIST_ALT_STYLE_NAME + UniMergeConfig.CONFLICT_SUFFIX)
					customStyles.Remove(builtinSkin.customStyles[i]);
			}
			customStyles.AddRange(usedSkin.customStyles);
			builtinSkin.customStyles = customStyles.ToArray();
		} else Debug.LogWarning("Can't find editor skin");
	}

	void OnGUI() {
		if(!SkinSetUp)
			SetUpSkin(GUI.skin);
		//Ctrl + w to close
		if(Event.current.Equals(Event.KeyboardEvent("^w"))) {
			Close();
			GUIUtility.ExitGUI();
		}
		if (Event.current.type == EventType.ScrollWheel) {
			if (Event.current.delta.y > 0) {
				ObjectDrawOffset++;
			} else {
				ObjectDrawOffset--;
			}
			Repaint();
			return;
		}
		/*
		 * SETUP
		 */
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		EditorGUIUtility.LookLikeControls();
#endif
		alt = false;
		//Adjust colWidth as the window resizes
		colWidth = (position.width - UniMergeConfig.midWidth * 2 - UniMergeConfig.margin) / 2;

		winHeight = position.height;
		if (lastWinHeight != winHeight) {
			ObjectDrawWindow = 500;
		}
		lastWinHeight = winHeight;
		if(root == null)
			root = new ObjectHelper();
		/*
		 * BEGIN GUI
		 */
		GUILayout.BeginHorizontal();
		{
			/*
			 * Options
			 */
			GUILayout.BeginVertical();
			deepCopy = EditorGUILayout.Toggle(new GUIContent("Deep Copy", "When enabled, copying GameObjects or Components will search for references to them and try to set them.  Disable if you do not want this behavior or if the window locks up on copy (too many objects)"), deepCopy);
			log = EditorGUILayout.Toggle(new GUIContent("Log", "When enabled, non-obvious events (like deep copy reference setting) will be logged"), log);
			compareAttrs = EditorGUILayout.Toggle(new GUIContent("Compare Attributes", "When disabled, attributes will not be included in comparison algorithm.  To choose which components are included, use the drop-downs to the right."), compareAttrs);
			if(GUILayout.Button("Expand Differences"))
				root.ExpandDiffs();
			if (GUILayout.Button("Refresh"))
				root.BubbleRefresh();
			DrawRowHeight();

			GUILayout.Space(10);						//Padding between controls and merge space
			GUILayout.EndVertical();

			/*
			 * Comparison Filters
			 */
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5			
			//TODO: Better masking for U3
			GUILayout.BeginVertical();
			GUILayout.Label("Enter a list of component types to exclude, separated by commas");
			filters = EditorGUILayout.TextField("Filters", filters);
			if(filters != lastFilters){
				ParseFilters();
			}
			lastFilters = filters;
			string filt = "Filtering: ";
			if(filterTypes.Count > 0){
				foreach(System.Type bad in filterTypes)
					filt += bad.Name + ", ";
				GUILayout.Label(filt.Substring(0, filt.Length - 2));
			}
			string err = "Sorry, the following types are invalid: ";
			if(badTypes.Count > 0){
				foreach(string bad in badTypes)
					err += bad + ", ";
				GUILayout.Label(err.Substring(0, err.Length - 2));
			}
			string cerr = "Sorry, the following types aren't components: ";
			if(notComponents.Count > 0){
				foreach(string bad in notComponents)
					cerr += bad + ", ";
				GUILayout.Label(cerr.Substring(0, cerr.Length - 2));
			}
			GUILayout.EndVertical();
#else
			GUILayout.Label(new GUIContent("Comparison Filters", "Select which components should be included in the comparison.  This is a little bugged right now so its disabled.  You can't filter more than 31 things :("));
			if(componentTypeStrings != null) {
				for(int i = 0; i < componentTypeStrings.Length; i++) {
					typeMask[i] = EditorGUILayout.MaskField(typeMask[i], componentTypeStrings[i], GUILayout.Width(75));
					if(i % 3 == 2) {
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
					}
				}
			}
#endif
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		{
			GUILayout.BeginVertical(GUILayout.Width(colWidth));
			{
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				mine = root.mine = (GameObject)EditorGUILayout.ObjectField("Mine", mine, typeof(GameObject));
#else
				mine = root.mine = (GameObject)EditorGUILayout.ObjectField("Mine", mine, typeof(GameObject), true);
#endif
			}
			GUILayout.EndVertical();
			GUILayout.Space(UniMergeConfig.midWidth * 2);
			GUILayout.BeginVertical(GUILayout.Width(colWidth));
			{
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				theirs = root.theirs = (GameObject)EditorGUILayout.ObjectField("Theirs", theirs, typeof(GameObject));
#else
				theirs = root.theirs = (GameObject)EditorGUILayout.ObjectField("Theirs", theirs, typeof(GameObject), true);
#endif
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
		if(lastRoot == null) {
			lastRoot = new ObjectHelper();
		}
		if(root.mine && root.theirs
			&& (root.mine != lastRoot.mine || root.theirs != lastRoot.theirs)) {
			//This is where we initiate the merge for the first time
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				refresh = root.DoRefresh();
#else
			//Check if the objects are prefabs
			switch (PrefabUtility.GetPrefabType(root.mine)) {
				case PrefabType.ModelPrefab:
				case PrefabType.Prefab:
					switch(PrefabUtility.GetPrefabType(root.theirs)) {
						case PrefabType.ModelPrefab:
						case PrefabType.Prefab:
							if(EditorUtility.DisplayDialog(
								"Instantiate prefabs?",
								"In order to merge prefabs, you must instantiate them and merge the instances. Then you must apply the changes.",
								"Instantiate", "Cancel")){
									GameObject mine = PrefabUtility.InstantiatePrefab(root.mine) as GameObject;
									GameObject theirs = PrefabUtility.InstantiatePrefab(root.theirs) as GameObject;
									ObjectMerge.mine = mine;
									ObjectMerge.theirs = theirs;
							} else {
								ObjectMerge.mine = null;
								ObjectMerge.theirs = null;
							}
							break;
						default:
							Debug.LogWarning("Sorry, you must compare a prefab with a prefab");
							break;
					}
					break;
				case PrefabType.DisconnectedPrefabInstance:
				case PrefabType.PrefabInstance:
				case PrefabType.ModelPrefabInstance:
				case PrefabType.None:
					switch (PrefabUtility.GetPrefabType(root.theirs)) {
						case PrefabType.DisconnectedPrefabInstance:
						case PrefabType.PrefabInstance:
						case PrefabType.ModelPrefabInstance:
						case PrefabType.None:
							refresh = root.DoRefresh();	
							break;
						default:
							Debug.LogWarning("Sorry, this prefab type is not supported");
							break;
					}
					break;
				default:
					Debug.LogWarning("Sorry, this prefab type is not supported");
					break;
			}
#endif
		}
		lastRoot.mine = root.mine;
		lastRoot.theirs = root.theirs;

#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2)
		EditorGUIUtility.labelWidth = 75;			//Make labels just a bit tighter for compactness 
#endif
		if(root.mine && root.theirs) {
			//scroll = GUILayout.BeginScrollView(scroll);
			//Rect box = GUILayoutUtility.GetRect(new GUIContent("asdf"), GUIStyle.none, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			//ObjectDrawWindow = (int)(position.height - 120) / (rowPadding * 2 + 37);
			ObjectDrawCursor = 0;
			ObjectDrawCount = root.GetDrawCount();
			int lessCount = ObjectDrawCount - ObjectDrawWindow;
			if (lessCount < 0)
				lessCount = 0;
			if (ObjectDrawOffset > lessCount) {
				ObjectDrawOffset = lessCount;
			}
			startingWidth = -1;
			if (lessCount > 0) {
				GUI.SetNextControlName("UM_Slider");
				ObjectDrawOffset = EditorGUILayout.IntSlider("Row Offset: ", ObjectDrawOffset, 0, lessCount);
			}
			root.Draw();
		}
		if(refresh != null) {
			Rect pbar = GUILayoutUtility.GetRect(position.width, PROGRESS_BAR_HEIGHT);
			EditorGUI.ProgressBar(pbar, (float)refreshCount / totalRefreshNum, refreshCount + "/" + totalRefreshNum);
			if(GUILayout.Button("Cancel")) {
				cancelRefresh = true;
				GUIUtility.ExitGUI();
			}
		}
	}

	public static void DrawRowHeight() {
		selectedRowHeight = EditorGUILayout.Popup("Row height: ", selectedRowHeight, new[] {"Large", "Medium", "Small"});
		rowPadding = rowHeights[selectedRowHeight];
	}
	public static bool YieldIfNeeded() {
		if (yieldIterCount++ > yieldCountCurr) {
			if (hammerTime && yieldCountCurr > yieldCountMin) {
				yieldCountCurr -= hammerTimeCut;
			} else if(yieldCountCurr < yieldCountMax) {
				yieldCountCurr += hammerTimeCut;
			}
			long elapsed = frameTimer.ElapsedMilliseconds;
			yieldIterCount = 0;
			if (elapsed > maxFrameTime) {
				//Debug.Log(elapsed + ", " + yieldCountMax);
				if (elapsed > maxFrameTime + frameTimeWiggleRoom) {
					hammerTime = true;
				} else {
					hammerTime = false;
				}
				return true;
			}
		}
		
		return false;
	}
	void Update() {
		/*
		 * Ad-hoc editor window coroutine:  Function returns and IEnumerator, and the Update function calls MoveNext
		 * Refresh will only run when the ObjectMerge window is focused
		 */
		if(cancelRefresh) {
			refresh = null;
		}
		if(refresh != null) {
			if(!refresh.MoveNext())
				refresh = null;
			Repaint();
		} 
		cancelRefresh = false;
		displayWarning = true;

		yieldIterCount = 0;
		frameTimer.Reset();
		frameTimer.Start();
	}
	public delegate bool RowComparison();
	public struct GenericRowArguments {
		public float indent, colWidth;
		public RowComparison compare;
		public EmptyVoid left, leftButton, right, rightButton;
		public bool drawButtons;
	}
	public static void DrawGenericRow(GenericRowArguments args) {
		bool same = args.compare.Invoke();
		StartRow(same);
		GUILayout.BeginVertical(GUILayout.Width(args.colWidth + 6));
		UniMerge.Util.Indent(args.indent, args.left);
		GUILayout.EndVertical();
		if(args.drawButtons) {
			DrawMidButtons(args.leftButton, args.rightButton);
		} else GUILayout.Space(UniMergeConfig.midWidth * 2);
		GUILayout.BeginVertical(GUILayout.Width(args.colWidth));
		UniMerge.Util.Indent(args.indent, args.right);
		GUILayout.EndVertical();
		EndRow();
	}
	public delegate void EmptyVoid();
	public static void DrawMidButtons(EmptyVoid toMine, EmptyVoid toTheirs) {
		DrawMidButtons(true, true, toMine, toTheirs, null, null);
	}
	public static void DrawMidButtons(bool hasMine, bool hasTheirs, EmptyVoid toMine, EmptyVoid toTheirs, EmptyVoid delMine, EmptyVoid delTheirs) {
		GUILayout.BeginVertical(GUILayout.Width(UniMergeConfig.midWidth * 2));
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5		
		GUILayout.Space(2);
#endif
		GUILayout.BeginHorizontal();
		if(hasTheirs) {
			if(GUILayout.Button(new GUIContent("<", "Copy theirs (properties and children) to mine"), GUILayout.Width(UniMergeConfig.midWidth))) {
				toMine.Invoke();
			}
		} else {
			if(GUILayout.Button(new GUIContent("X", "Delete mine"), GUILayout.Width(UniMergeConfig.midWidth))) {
				delMine.Invoke();
			}
		}
		if(hasMine) {
			if(GUILayout.Button(new GUIContent(">", "Copy mine (properties and children) to theirs"), GUILayout.Width(UniMergeConfig.midWidth))) {
				toTheirs.Invoke();
			}
		} else {
			if(GUILayout.Button(new GUIContent("X", "Delete theirs"), GUILayout.Width(UniMergeConfig.midWidth))) {
				delTheirs.Invoke();
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();

	}
	public static void StartRow(bool same) {
		string style = alt ? UniMergeConfig.LIST_ALT_STYLE_NAME : UniMergeConfig.LIST_STYLE_NAME;
		if(!same)
			style += "Conflict";
		EditorGUILayout.BeginVertical(style);
		//Top padding
		GUILayout.Space(rowPadding + basePaddingTop);
		EditorGUILayout.BeginHorizontal();
	}
	public static void EndRow() {
		EditorGUILayout.EndHorizontal();
		//Bottom padding
		GUILayout.Space(rowPadding + basePaddingBot);
		EditorGUILayout.EndVertical();
		alt = !alt;
	}
	void HandleLog(string logString, string stackTrace, LogType type) {
		//Totally hack solution, but it works.  This situation happens a lot in conflict resolution.  You have a prefab with git markup, this error spams the console, UniMerge crashes the editor.
		//TODO: handle "too many logs" in general.
		if (logString.Contains("seems to have merge conflicts. Please open it in a text editor and fix the merge.")) {
			if (displayWarning) {
				//I can't get this to stop displaying twice for some reason.
				EditorUtility.DisplayDialog("Merge canceled for your own good", "It appears that you have a prefab in your scene with merge conflicts. Unity spits out a warning about this at every step of the merge which makes it take years.  Resolve your prefab conflicts before resolving scene conflicts.", "OK, fine");
				displayWarning = false;
				ObjectMerge.mine = null;
				ObjectMerge.theirs = null;
			}
			refresh = null;
			cancelRefresh = true;
			GUIUtility.ExitGUI();
		}
	}
	bool displayWarning = false;
	public static float startingWidth = -1;
	public static bool drawAbort = false;
	/// <summary>
	/// Check if we can draw objects yet, and increment counter
	/// </summary>
	/// <returns></returns>
	public static bool ObjectDrawCheck() {					 
		//Debug.Log(ObjectDrawCount + ", " + ObjectDrawCursor + ", " + ObjectDrawOffset + ", " + ObjectDrawWindow);
		bool check = ObjectDrawCursor >= ObjectDrawOffset;
		if (ObjectDrawCursor >= ObjectDrawOffset + ObjectDrawWindow + 5) {
			drawAbort = true;
			check = false;
		}
		if (check) {
			Rect lastRect = GUILayoutUtility.GetLastRect();
			if (lastRect.y + lastRect.height >= winHeight - 37) {
				if (GUI.GetNameOfFocusedControl() != "UM_Slider") {
					ObjectDrawWindow = ObjectDrawCursor - ObjectDrawOffset;
				}
				GUI.FocusControl("");
				drawAbort = true;
				check = false;
			}
		}
		ObjectDrawCursor++;
		return check;
	}
}
public class ObjectHelper {
	public ObjectHelper parent;
	public GameObject mine, theirs;
	public List<ObjectHelper> children = new List<ObjectHelper>();
	public List<ComponentHelper> components = new List<ComponentHelper>();
	public bool foldout, same, sameAttrs, show, showAttrs;
	public delegate void OnRefreshComplete(ObjectHelper self);

	public void BubbleRefresh() {
		if (ObjectMerge.refresh != null) {
			ObjectMerge.scroll = scroll;
		}
		if (parent != null && !parent.same)
			ObjectMerge.refresh = parent.DoRefresh();
		else if (!same) {
			ObjectMerge.refresh = DoRefresh();
		}
	}
	public IEnumerator DoRefresh() { return DoRefresh(null); }
	public IEnumerator DoRefresh(OnRefreshComplete onComplete) {
		if (mine) {
			List<GameObject> mineList = new List<GameObject>();
			UniMerge.Util.GameObjectToList(mine, mineList);
			ObjectMerge.totalRefreshNum = mineList.Count;
		}
		if (theirs) {
			List<GameObject> theirsList = new List<GameObject>();
			UniMerge.Util.GameObjectToList(theirs, theirsList);
			if (theirsList.Count > ObjectMerge.totalRefreshNum)
				ObjectMerge.totalRefreshNum = theirsList.Count;
		}
		ObjectMerge.refreshCount = 0;
		foreach (IEnumerable e in Refresh()) {
			yield return e;
		}

		if (onComplete != null)
			onComplete.Invoke(this);
	}

	private Vector2 scroll;
	public IEnumerable Refresh() {
		scroll = ObjectMerge.scroll;
		ObjectMerge.refreshCount++;
		same = true;
		List<GameObject> myChildren = new List<GameObject>();
		List<GameObject> theirChildren = new List<GameObject>();
		List<Component> myComponents = new List<Component>();
		List<Component> theirComponents = new List<Component>();
		if (mine) {
			myChildren.AddRange(from Transform t in mine.transform select t.gameObject);
			myComponents = new List<Component>(mine.GetComponents<Component>());
		}
		if (theirs) {
			theirChildren.AddRange(from Transform t in theirs.transform select t.gameObject);
			theirComponents = new List<Component>(theirs.GetComponents<Component>());
		}
		//TODO: turn these two chunks into one function... somehow
		//Merge Components
		ComponentHelper ch;
		for (int i = 0; i < myComponents.Count; i++) {
			Component match = null;
			Component myComponent = myComponents[i];
			if (myComponent != null) {
				foreach (Component g in theirComponents) {
					if (g != null) {
						if (myComponent.GetType() == g.GetType()) {
							match = g;
							break;
						}
					}
				}
				ch = components.Find(delegate(ComponentHelper helper) {
					return helper.mine == myComponent || (match != null && helper.theirs == match);
				});
				if (ch == null) {
					ch = new ComponentHelper(this, myComponent, match);
					components.Add(ch);
				}
				else {
					ch.mine = myComponent;
					ch.theirs = match;
				}
				foreach (IEnumerable e in ch.Refresh())
					yield return e;
				if (!ComponentIsFiltered(ch.type) && !ch.same) {
					same = false;
				}
				theirComponents.Remove(match);
			}
		}
		if (theirComponents.Count > 0) {
			foreach (Component g in theirComponents) {
				ch = components.Find(delegate(ComponentHelper helper) {
					return helper.theirs == g;
				});
				if (ch == null) {
					ch = new ComponentHelper(this, null, g);
					foreach (IEnumerable e in ch.Refresh())
						yield return e;
					components.Add(ch);
				}
				if (!ComponentIsFiltered(ch.type) && !ch.same)
					same = false;
			}
		}
		//Merge Children
		ObjectHelper oh;
		for (int i = 0; i < myChildren.Count; i++) {
			GameObject match = theirChildren.FirstOrDefault(g => SameObject(myChildren[i], g));
			oh = children.Find(delegate(ObjectHelper helper) {
				return helper.mine == myChildren[i] || (match != null && helper.theirs == match);
			});
			if (oh == null) {
				oh = new ObjectHelper {parent = this, mine = myChildren[i], theirs = match};
				children.Add(oh);
			}
			else {
				oh.mine = myChildren[i];
				oh.theirs = match;
			}
			theirChildren.Remove(match);
		}
		if (theirChildren.Count > 0) {
			same = false;
			foreach (GameObject g in theirChildren) {
				oh = children.Find(delegate(ObjectHelper helper) {
					return helper.theirs == g;
				});
				if (oh == null)
					children.Add(new ObjectHelper {parent = this, theirs = g});
			}
		}
		List<ObjectHelper> tmp = new List<ObjectHelper>(children);
		foreach (ObjectHelper obj in tmp) {
			if (obj.mine == null && obj.theirs == null)
				children.Remove(obj);
		}
		children.Sort(delegate(ObjectHelper a, ObjectHelper b) {
			if (a.mine && b.mine)
				return a.mine.name.CompareTo(b.mine.name);
			if (a.mine && b.theirs)
				return a.mine.name.CompareTo(b.theirs.name);
			if (a.theirs && b.mine)
				return a.theirs.name.CompareTo(b.mine.name);
			if (a.theirs && b.theirs)
				return a.theirs.name.CompareTo(b.theirs.name);
			return 0;
		});
		tmp = new List<ObjectHelper>(children);
		foreach (ObjectHelper child in tmp) {
			foreach (IEnumerable e in child.Refresh())
				yield return e;
			if (!child.same)
				same = false;
		}
		sameAttrs = CheckAttrs();
		if (!sameAttrs && ObjectMerge.compareAttrs) {
			same = false;
		}
		ObjectMerge.scroll = scroll;
	}

	bool ComponentIsFiltered(System.Type type) {
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		//TODO: Better U3 filtering
		for(int i = 0; i < ObjectMerge.filterTypes.Count; i++) {
			if(type == ObjectMerge.filterTypes[i])
				return true;
		}
#else
		for(int i = 0; i < ObjectMerge.componentTypes.Length; i++) {
			if(ObjectMerge.typeMask[i] == -1)	//This has everything, continue
				continue;
			int idx = ArrayUtility.IndexOf(ObjectMerge.componentTypes[i], type);
			if(idx != -1)
				return ((ObjectMerge.typeMask[i] >> idx) & 1) == 0;
		}
#endif
		return false;		//Assume not filtered
	}

	private static int thiscount = 0;

	/// <summary>
	/// Default draw for drawing the root.  Should only be called on root object
	/// NOTE: Why is width not 0?
	/// </summary>
	public void Draw() {
		ObjectMerge.drawAbort = false;
		Draw(1);
	}
	/// <summary>
	/// Draw this node in the tree.
	/// </summary>
	/// <param name="width"></param>
	public void Draw(float width) {
		if (ObjectMerge.drawAbort) {
			return;
		}
		thiscount++;
		if (ObjectMerge.ObjectDrawCheck()) {
			if (ObjectMerge.startingWidth < 0 && width > 1) {
				ObjectMerge.startingWidth = width;
				width = UniMerge.Util.TAB_SIZE + 1;
			}
			//This object
			ObjectMerge.StartRow(same);
			//Display mine
			GUILayout.BeginVertical();
			GUILayout.Space(ObjectMerge.objectPadding);
			DrawObject(true, width);
			GUILayout.EndVertical();
			//Swap buttons
			ObjectMerge.DrawMidButtons(mine, theirs,
				// < button
				delegate {
					//NB: This still thows a SerializedProperty error (at least in Unity 3) gonna have to do a bit more poking.
					Copy(true);
					if (mine == ObjectMerge.root.mine)
						SceneMerge.mineContainer = mine;
					BubbleRefresh();
					// > button
				}, delegate {
					Copy(false);
					if (theirs == ObjectMerge.root.theirs)
						SceneMerge.theirsContainer = theirs;
					BubbleRefresh();
					// Left X button
				}, delegate {
					DestroyAndClearRefs(mine, true);
					BubbleRefresh();
					// Right X button
				}, delegate {
					DestroyAndClearRefs(theirs, false);
					BubbleRefresh();
				});
			//Display theirs
			GUILayout.BeginVertical();
			GUILayout.Space(ObjectMerge.objectPadding);
			GUILayout.BeginHorizontal();
			DrawObject(false, width);
			if (GUILayout.Button(show ? "Hide" : "Show", GUILayout.Width(45))) {
				show = !show;
				if (Event.current.alt) {
					showAttrs = show;
					foreach (ComponentHelper component in components) {
						component.show = show;
					}
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			ObjectMerge.EndRow();
		}
		if(show) {
			DrawAttributes(width + UniMerge.Util.TAB_SIZE * 2);
			//Don't mess with same during a refresh
			if(ObjectMerge.refresh == null && !sameAttrs && ObjectMerge.compareAttrs)
				same = false;
			List<ComponentHelper> tmp = new List<ComponentHelper>(components);
			foreach(ComponentHelper component in tmp) {
				component.Draw(width + UniMerge.Util.TAB_SIZE * 2);
			}
			GUILayout.Space(ObjectMerge.componentsMargin);
		}
		//Children
		if(foldout) {
			List<ObjectHelper> tmp = new List<ObjectHelper>(children);
			foreach (ObjectHelper helper in tmp) {
				helper.Draw(width + UniMerge.Util.TAB_SIZE);
				if (ObjectMerge.startingWidth > 0) {
					width = 1;
					ObjectMerge.startingWidth = 0;
				}
			}
		}
	}

	public int GetDrawCount() {
		int selfCount = 1;		//Start with 1 because we're drawing this row
		if (show) {
			selfCount++;		//For attributes row
			if (showAttrs) {
				selfCount += 6;	//Number of attributes rendered
			}
			foreach (ComponentHelper component in components) {
				selfCount += component.GetDrawCount();
			}
		}
		if (foldout) {
			foreach (ObjectHelper helper in children)
				selfCount += helper.GetDrawCount();
		}
		return selfCount;
	}

	public void DestroyAndClearRefs(Object obj, bool isMine) {
		List<PropertyHelper> properties = new List<PropertyHelper>();
		FindRefs(obj, isMine, properties);
		foreach(PropertyHelper property in properties) {
			property.GetProperty(isMine).objectReferenceValue = null;
			if(ObjectMerge.log)
				Debug.Log("Set reference to null in " + property.GetProperty(isMine).serializedObject.targetObject
					+ "." + property.GetProperty(isMine).name, property.GetProperty(isMine).serializedObject.targetObject);
			if(property.GetProperty(!isMine).serializedObject.targetObject != null)
				property.GetProperty(isMine).serializedObject.ApplyModifiedProperties();
		}
		if (isMine)
			mine = null;
		else {
			theirs = null;
		}
		ObjectMerge.root.UnsetFlagRecursive();
		Object.DestroyImmediate(obj);
	}
	bool findAndSetFlag;
	void Copy(bool toMine) {
		if(toMine) {
			//NB: I thought I should use EditorUitlity.CopySerialized here but it don't work right
			if(parent == null) {	//Top-level object
				if(mine) {
					Object.DestroyImmediate(mine);
				}
				mine = Copy(theirs, false);
				ObjectMerge.mine = mine;
			} else if(parent.mine) {
				if(mine) {
					Object.DestroyImmediate(mine);
				}
				mine = Copy(theirs, false);
			} else Debug.LogWarning("Can't copy this object.  Destination parent doesn't exist!");
		} else {
			if(parent == null) {	//Top-level object
				if(theirs) {
					Object.DestroyImmediate(theirs);
				}
				theirs = Copy(mine, true);
				ObjectMerge.theirs = theirs;
			} else if(parent.theirs) {
				if(theirs) {
					Object.DestroyImmediate(theirs);
				}
				theirs = Copy(mine, true);
			} else Debug.LogWarning("Can't copy this object.  Destination parent doesn't exist!");
		}
	}
	/// <summary>
	/// Do the actual GameObject copy.  This is generalized because the process is totally symmetrical. Come to think of it so is the 
	/// other version of Copy.  I can probably merge these back together.
	/// </summary>
	/// <param name="original">The object to be copied</param>
	/// <param name="isMine">Whether "original" is mine or theirs</param>
	/// <returns>The copied object</returns>
	GameObject Copy(GameObject original, bool isMine) {
		GameObject copy;
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		if(UniMerge.Util.IsPrefabParent(original) && EditorUtility.GetPrefabType(original) == PrefabType.PrefabInstance){
			copy = (GameObject)EditorUtility.InstantiatePrefab(EditorUtility.GetPrefabParent(original));
#else
		if(UniMerge.Util.IsPrefabParent(original) && PrefabUtility.GetPrefabType(original) == PrefabType.PrefabInstance) {
			copy = (GameObject)PrefabUtility.InstantiatePrefab(PrefabUtility.GetPrefabParent(original));
#endif
			//Copy all properties in case they differ from prefab
			List<GameObject> sourceList = new List<GameObject>();
			List<GameObject> copyList = new List<GameObject>();
			UniMerge.Util.GameObjectToList(original, sourceList);
			UniMerge.Util.GameObjectToList(copy, copyList);
			for(int i = 0; i < sourceList.Count; i++) {
				copyList[i].name = sourceList[i].name;
				copyList[i].layer = sourceList[i].layer;
				copyList[i].tag = sourceList[i].tag;
				copyList[i].isStatic = sourceList[i].isStatic;
				copyList[i].hideFlags = sourceList[i].hideFlags;
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				copyList[i].active = sourceList[i].active;
#else
				copyList[i].SetActive(sourceList[i].activeSelf);
#endif
				Component[] sourceComps = sourceList[i].GetComponents<Component>();
				Component[] copyComps = copyList[i].GetComponents<Component>();
				for(int j = 0; j < sourceComps.Length; j++) {
					EditorUtility.CopySerialized(sourceComps[j], copyComps[j]);
				}
			}
		} else
			copy = (GameObject)Object.Instantiate(original);
		copy.name = original.name;		//because of stupid (clone)
		if(parent != null)
			copy.transform.parent = parent.GetObject(!isMine).transform;
		copy.transform.localPosition = original.transform.localPosition;
		copy.transform.localRotation = original.transform.localRotation;
		copy.transform.localScale = original.transform.localScale;
		//Set any references on their side to this object
		//Q: is this neccessary when copying top-object?
		if(ObjectMerge.deepCopy)
			FindAndSetRefs(original, copy, isMine);
		return copy;
	}
	#region ATTRIBUTES
	/* Compared attributes:
	 * Name
	 * Active
	 * Static
	 * Tag
	 * Layer
	 * HideFlags
	 */

	private void DrawAttributes(float width) {
		if (ObjectMerge.ObjectDrawCheck()) {
			//TODO: Draw GO fields as serializedProperty or something
			ObjectMerge.StartRow(CheckAttrs());
			GUILayout.BeginVertical(GUILayout.Width(ObjectMerge.colWidth + 6));
			{
				UniMerge.Util.Indent(width, delegate {
					if (GetObject(true))
						showAttrs = EditorGUILayout.Foldout(showAttrs, "Attributes");
					else
						GUILayout.Label("");
				});
			}
			GUILayout.EndVertical();
			if (GetObject(true) && GetObject(false)) {
				ObjectMerge.DrawMidButtons(
					delegate() {
						SetAttrs(true);
						BubbleRefresh();
					},
					delegate() {
						SetAttrs(false);
						BubbleRefresh();
					});
			} else GUILayout.Space(UniMergeConfig.midWidth*2);
			GUILayout.BeginVertical(GUILayout.Width(ObjectMerge.colWidth + 7));
			{
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5		
			GUILayout.Space(3);
#endif
				UniMerge.Util.Indent(width, delegate {
					if (GetObject(false))
						showAttrs = EditorGUILayout.Foldout(showAttrs, "Attributes");
					else
						GUILayout.Label("");
				});
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5		
			GUILayout.Space(-4);
#endif

			}
			GUILayout.EndVertical();
			ObjectMerge.EndRow();
		}
		//Note: I know this hardcoded stuff is stupid.  Someday I'll make it smarter.
		if(showAttrs) {
			//Name
			DrawAttribute(new ObjectMerge.GenericRowArguments {
				indent = width + UniMerge.Util.TAB_SIZE,
				colWidth = ObjectMerge.colWidth,
				compare = delegate {
					bool nameSame = GenericCompare();
					if(nameSame)
						nameSame = GetObject(true).name == GetObject(false).name;
					//Consider scenemerge container names to be equal, otherwise you'll never get all green... my OCD senses are tingling
					if(GetObject(true) && GetObject(false) && !nameSame)
						nameSame = GetObject(true).name == SceneMerge.mineContainerName && GetObject(false).name == SceneMerge.theirsContainerName;
					return nameSame;
				},
				left = delegate {
					if(GetObject(true))
						GetObject(true).name = EditorGUILayout.TextField("Name", GetObject(true).name);
				},
				leftButton = delegate {
					GetObject(true).name = GetObject(false).name;
				},
				rightButton = delegate {
					GetObject(false).name = GetObject(true).name;
				},
				right = delegate {
					if(GetObject(false))
						GetObject(false).name = EditorGUILayout.TextField("Name", GetObject(false).name);
				},
				drawButtons = mine && theirs
			});
			//Active
			DrawAttribute(new ObjectMerge.GenericRowArguments {
				indent = width + UniMerge.Util.TAB_SIZE,
				compare = delegate {
					bool activeSame = GenericCompare();
					if(activeSame)
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
						activeSame = GetObject(true).active == GetObject(false).active;
#else
						activeSame = GetObject(true).activeSelf == GetObject(false).activeSelf;
#endif
					return activeSame;
				},
				left = delegate {
					if(GetObject(true))
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				GetObject(true).active = EditorGUILayout.Toggle("Active", GetObject(true).active);
#else
						GetObject(true).SetActive(EditorGUILayout.Toggle("Active", GetObject(true).activeSelf));
#endif
				},
				leftButton = delegate {
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
					GetObject(true).active = GetObject(false).active;
#else
					GetObject(true).SetActive(GetObject(false).activeSelf);
#endif
				},
				rightButton = delegate {
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
					GetObject(false).active = GetObject(true).active;
#else
					GetObject(false).SetActive(GetObject(true).activeSelf);
#endif
				},
				right = delegate {
					if(GetObject(false))
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
						GetObject(false).active = EditorGUILayout.Toggle("Active", GetObject(false).active);
#else
						GetObject(false).SetActive(EditorGUILayout.Toggle("Active", GetObject(false).activeSelf));
#endif
				},
				drawButtons = mine && theirs
			});
			//Static
			DrawAttribute(new ObjectMerge.GenericRowArguments {
				indent = width + UniMerge.Util.TAB_SIZE,
				colWidth = ObjectMerge.colWidth,
				compare = delegate {
					bool staticSame = GenericCompare();
					if(staticSame)
						staticSame = GetObject(true).isStatic == GetObject(false).isStatic;
					return staticSame;
				},
				left = delegate {
					if(GetObject(true))
						GetObject(true).isStatic = EditorGUILayout.Toggle("Static", GetObject(true).isStatic);
				},
				leftButton = delegate {
					GetObject(true).isStatic = GetObject(false).isStatic;
				},
				rightButton = delegate {
					GetObject(false).isStatic = GetObject(true).isStatic;
				},
				right = delegate {
					if(GetObject(false))
						GetObject(false).isStatic = EditorGUILayout.Toggle("Static", GetObject(false).isStatic);
				},
				drawButtons = mine && theirs
			});
			//Tag
			DrawAttribute(new ObjectMerge.GenericRowArguments {
				indent = width + UniMerge.Util.TAB_SIZE,
				colWidth = ObjectMerge.colWidth,
				compare = delegate {
					bool tagSame = GenericCompare();
					if(tagSame)
						tagSame = GetObject(true).tag == GetObject(false).tag;
					return tagSame;
				},
				left = delegate {
					if(GetObject(true))
						GetObject(true).tag = EditorGUILayout.TextField("Tag", GetObject(true).tag);
				},
				leftButton = delegate {
					GetObject(true).tag = GetObject(false).tag;
				},
				rightButton = delegate {
					GetObject(false).tag = GetObject(true).tag;
				},
				right = delegate {
					if(GetObject(false))
						GetObject(false).tag = EditorGUILayout.TextField("Tag", GetObject(false).tag);
				},
				drawButtons = mine && theirs
			});
			//Layer
			DrawAttribute(new ObjectMerge.GenericRowArguments {
				indent = width + UniMerge.Util.TAB_SIZE,
				colWidth = ObjectMerge.colWidth,
				compare = delegate {
					bool layerSame = GenericCompare();
					if(layerSame)
						layerSame = GetObject(true).layer == GetObject(false).layer;
					return layerSame;
				},
				left = delegate {
					if(GetObject(true))
						GetObject(true).layer = EditorGUILayout.IntField("Layer", GetObject(true).layer);
				},
				leftButton = delegate {
					GetObject(true).layer = GetObject(false).layer;
				},
				rightButton = delegate {
					GetObject(false).layer = GetObject(true).layer;
				},
				right = delegate {
					if(GetObject(false))
						GetObject(false).layer = EditorGUILayout.IntField("Layer", GetObject(false).layer);
				},
				drawButtons = mine && theirs
			});
			//HideFlags
			//BUG: Should be using EnumMaskField for HideFlags, but get a flags < (1 << kHideFlagsBits) error on last two options
			DrawAttribute(new ObjectMerge.GenericRowArguments {
				indent = width + UniMerge.Util.TAB_SIZE,
				colWidth = ObjectMerge.colWidth,
				compare = delegate {
					bool flagsSame = GenericCompare();
					if(flagsSame)
						flagsSame = GetObject(true).hideFlags == GetObject(false).hideFlags;
					return flagsSame;
				},
				left = delegate {
					if(GetObject(true))
						GetObject(true).hideFlags = (HideFlags)EditorGUILayout.IntField("HideFlags", (int)GetObject(true).hideFlags);
				},
				leftButton = delegate {
					GetObject(true).hideFlags = GetObject(false).hideFlags;
				},
				rightButton = delegate {
					GetObject(false).hideFlags = GetObject(true).hideFlags;
				},
				right = delegate {
					if(GetObject(false))
						GetObject(false).hideFlags = (HideFlags)EditorGUILayout.IntField("HideFlags", (int)GetObject(false).hideFlags);
				},
				drawButtons = mine && theirs
			});
		}
	}
	bool GenericCompare() {
		bool same = GetObject(true);
		if(same)
			same = GetObject(false);
		return same;
	}
	void DrawAttribute(ObjectMerge.GenericRowArguments args) {
		if (ObjectMerge.ObjectDrawCheck()) {
			args.colWidth = ObjectMerge.colWidth;
			if (!args.compare.Invoke())
				sameAttrs = false;
			args.leftButton += delegate {
				BubbleRefresh();
			};
			args.rightButton += delegate {
				BubbleRefresh();
			};
			ObjectMerge.DrawGenericRow(args);
		}
	}
	bool CheckAttrs() {
		if(GetObject(true) == null && GetObject(false) == null)
			return true;
		if(GetObject(true) == null)
			return false;
		if(GetObject(false) == null)
			return false;
		//Consider scenemerge container names to be equal, otherwise you'll never get all green... my OCD senses are tingling
		if(GetObject(true).name == SceneMerge.mineContainerName && GetObject(false).name == SceneMerge.theirsContainerName)
			return true;
		if(GetObject(true).name != GetObject(false).name)
			return false;
		if(GetObject(true).layer != GetObject(false).layer)
			return false;
		if(GetObject(true).hideFlags != GetObject(false).hideFlags)
			return false;
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		if(GetObject(true).active != GetObject(false).active)
#else
		if(GetObject(true).activeSelf != GetObject(false).activeSelf)
#endif
			return false;
		if(GetObject(true).isStatic != GetObject(false).isStatic)
			return false;
		if(GetObject(true).tag != GetObject(false).tag)
			return false;
		return true;
	}
	void SetAttrs(bool toMine) {
		GetObject(toMine).name = GetObject(!toMine).name;
		GetObject(toMine).layer = GetObject(!toMine).layer;
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		GetObject(toMine).active = GetObject(!toMine).active;
#else
		GetObject(toMine).SetActive(GetObject(!toMine).activeSelf);
#endif
		GetObject(toMine).isStatic = GetObject(!toMine).isStatic;
		GetObject(toMine).tag = GetObject(!toMine).tag;
		sameAttrs = true;
	}
	#endregion
	/// <summary>
	/// Find references of source in Mine, and set their counterparts in Theirs to copy. This "start" function calls
	/// FindRefs which searches the whole object's heirarchy, and then calls UnsetFlagRecursive to reset the flag
	/// used to avoid searching the same object twice
	/// </summary>
	/// <param name="source">The source object being referenced</param>
	/// <param name="copy">A new copy of the source object, which will be referenced</param>
	/// <param name="isMine">Whether the source object is on the Mine (left) side</param>
	void FindAndSetRefs(GameObject source, GameObject copy, bool isMine) {
		//NOTE: There may be some possibilities of missing references going on in this function. Has not been exhaustively tested yet.
		List<PropertyHelper> properties = new List<PropertyHelper>();

		List<GameObject> sourceList = new List<GameObject>();
		List<GameObject> copyList = new List<GameObject>();
		UniMerge.Util.GameObjectToList(source, sourceList);
		UniMerge.Util.GameObjectToList(copy, copyList);

		for(int i = 0; i < sourceList.Count; i++) {
			properties.Clear();
			FindRefs(sourceList[i], isMine, properties);
			foreach(PropertyHelper property in properties) {
				//Sometimes you get an error here in older versions of Unity about using a SerializedProperty after the object has been deleted.  Don't know how else to detect this
				if(property.GetProperty(!isMine) != null) {
					property.GetProperty(!isMine).objectReferenceValue = copyList[i];
					if(ObjectMerge.log)
						Debug.Log("Set reference to " + copyList[i] + " in " + property.GetProperty(!isMine).serializedObject.targetObject
							+ "." + property.GetProperty(!isMine).name, property.GetProperty(!isMine).serializedObject.targetObject);
					if(property.GetProperty(!isMine).serializedObject.targetObject != null)
						property.GetProperty(!isMine).serializedObject.ApplyModifiedProperties();
				}
			}
			ObjectMerge.root.UnsetFlagRecursive();
			Component[] sourceComps = sourceList[i].GetComponents<Component>();
			Component[] copyComps = copyList[i].GetComponents<Component>();
			for(int j = 0; j < sourceComps.Length; j++) {
				properties.Clear();
				FindRefs(sourceComps[j], isMine, properties);
				foreach(PropertyHelper property in properties) {
					//Sometimes you get an error here in older versions of Unity about using a SerializedProperty after the object has been deleted.  Don't know how else to detect this
					if(property.GetProperty(!isMine) != null) {
						property.GetProperty(!isMine).objectReferenceValue = copyComps[j];
						if(ObjectMerge.log)
							Debug.Log("Set reference to " + copyComps[j] + " in " + property.GetProperty(!isMine).serializedObject.targetObject
								+ "." + property.GetProperty(!isMine).name, property.GetProperty(!isMine).serializedObject.targetObject);
						if(property.GetProperty(!isMine).serializedObject.targetObject != null)
							property.GetProperty(!isMine).serializedObject.ApplyModifiedProperties();
					}
				}
				ObjectMerge.root.UnsetFlagRecursive();
			}
		}
	}
	public void FindRefs(Object source, bool isMine, List<PropertyHelper> properties) {
		if(findAndSetFlag)
			return;
		findAndSetFlag = true;
		foreach(ComponentHelper component in components) {
			foreach(PropertyHelper property in component.properties) {
				if(property.GetProperty(isMine) != null && property.GetProperty(!isMine) != null) {
					if(property.GetProperty(isMine).propertyType == SerializedPropertyType.ObjectReference)
						if(property.GetProperty(isMine).objectReferenceValue == source)
							properties.Add(property);
				}
			}
		}
		if(parent != null)
			parent.FindRefs(source, isMine, properties);
		foreach(ObjectHelper helper in children)
			helper.FindRefs(source, isMine, properties);
	}
	/// <summary>
	/// Get the spouse (counterpart) of an object within this tree.  If the spouse is missing, copy the object and return the copy
	/// </summary>
	/// <param name="obj">The object we're looking for</param>
	/// <param name="isMine">Whether the object came from Mine (left)</param>
	/// <returns></returns>
	public GameObject GetObjectSpouse(GameObject obj, bool isMine) {
		if(obj == GetObject(isMine)) {
			if(GetObject(!isMine)) {
				return GetObject(!isMine);
			}
			Copy(!isMine);
			if(ObjectMerge.log && GetObject(isMine))
				Debug.Log("Copied " + GetObject(!isMine) + " in order to transfer reference");
			return GetObject(!isMine);
		}
		foreach(ObjectHelper child in children)
			if(child.GetObjectSpouse(obj, isMine))
				return child.GetObjectSpouse(obj, isMine);
		return null;
	}
	public void UnsetFlagRecursive() {
		findAndSetFlag = false;
		foreach(ObjectHelper helper in children)
			helper.UnsetFlagRecursive();
	}

	private void DrawObject(bool isMine, float width) {
		//Store foldoutstate before doing GUI to check if it changed
		bool foldoutState = foldout;
		//Create space with width = colWidth
		GUILayout.BeginVertical(GUILayout.Width(ObjectMerge.colWidth));
		UniMerge.Util.Indent(width, delegate {
			GUILayout.BeginHorizontal();
			if(GetObject(isMine)) {
				GUILayout.BeginVertical();
				{
					GUILayout.Space(ObjectMerge.foldoutPadding);			//Foldouts are too high by 4px... ok
					if(GetObject(isMine).transform.childCount > 0)
						foldout = EditorGUILayout.Foldout(foldout, GetObject(isMine).name);
					else
						GUILayout.Label(GetObject(isMine).name);
				} GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
				if(GUILayout.Button("Ping"))
					EditorGUIUtility.PingObject(GetObject(isMine));
			} else GUILayout.Label("");
			GUILayout.EndHorizontal();
		});
		GUILayout.EndVertical();
		//If foldout state changed and user was holding alt, set all child foldout states to this state
		if(Event.current.alt && foldout != foldoutState)
			SetFoldoutRecur(foldout);
	}
	void SetFoldoutRecur(bool state) {
		foldout = state;
		foreach(ObjectHelper obj in children)
			obj.SetFoldoutRecur(state);
	}
	public bool ExpandDiffs() {
		foldout = !same;
		if(children.Count(obj => obj.ExpandDiffs()) > 0) {
			foldout = true;
		}
		return foldout;
	}
	//Big ??? here.  What do we count as the same needing merge and what do we count as totally different?
	bool SameObject(GameObject mine, GameObject theirs) {
		return mine.name == theirs.name;
	}

	GameObject GetObject(bool isMine) {
		return isMine ? mine : theirs;
	}
}
public class ComponentHelper {
	public bool show, same;
	public ObjectHelper parent;
	public Component mine, theirs;
	public System.Type type;
	SerializedObject mySO, theirSO;
	public List<PropertyHelper> properties = new List<PropertyHelper>();
	public Component GetComponent(bool isMine) {
		return isMine ? mine : theirs;
	}
	public ComponentHelper(ObjectHelper parent, Component mine, Component theirs) {
		this.parent = parent;
		this.mine = mine;
		this.theirs = theirs;
		if(mine != null)
			type = mine.GetType();
		else if(theirs != null)
			type = theirs.GetType();
	}
	public IEnumerable Refresh() {
		properties.Clear();
		List<SerializedProperty> myProps = new List<SerializedProperty>();
		List<SerializedProperty> theirProps = new List<SerializedProperty>();
		if(GetComponent(true)) {
			mySO = new SerializedObject(GetComponent(true));
			GetProperties(myProps, mySO);
		}
		if (GetComponent(false)) {
			theirSO = new SerializedObject(GetComponent(false));
			GetProperties(theirProps, theirSO);
		}
		same = myProps.Count == theirProps.Count;
		if(mine && theirs && !same)
			Debug.LogWarning("not same number of props... wtf?");
		int count = Mathf.Max(myProps.Count, theirProps.Count);
		for(int i = 0; i < count; i++) {
			SerializedProperty myProp = null;
			SerializedProperty theirProp = null;
			if(i < myProps.Count)
				myProp = myProps[i];
			if(i < theirProps.Count)
				theirProp = theirProps[i];
			PropertyHelper ph = new PropertyHelper(this, myProp, theirProp);
			foreach(IEnumerable e in ph.CheckSame())
				yield return e;
			properties.Add(ph);
			if(!ph.same)
				same = false;
		}
	}

	private void GetProperties(List<SerializedProperty> myProps, SerializedObject obj) {
		SerializedProperty iterator = obj.GetIterator();
		bool enterChildren = true;
		while(iterator.NextVisible(enterChildren)) {
			myProps.Add(iterator.Copy());
			enterChildren = false;
		}
	}
	public void Draw(float width) {
		if (ObjectMerge.drawAbort) {
			return;
		}
		if(mine == null && theirs == null)		//TODO: figure out why blank componentHelpers are being created
			return;
		if (ObjectMerge.ObjectDrawCheck()) {
			ObjectMerge.StartRow(same);
			DrawComponent(true, width);
			//Swap buttons
			if (parent.mine && parent.theirs) {
				ObjectMerge.DrawMidButtons(mine, theirs, delegate { //Copy theirs to mine
					EditorUtility.CopySerialized(theirs, mine ?? parent.mine.AddComponent(theirs.GetType()));
					parent.BubbleRefresh();
				}, delegate { //Copy mine to theirs
					EditorUtility.CopySerialized(mine, theirs ?? parent.theirs.AddComponent(mine.GetType()));
					parent.BubbleRefresh();
				}, delegate { //Delete mine
					if (mine is Camera) {
						parent.DestroyAndClearRefs(mine.GetComponent<AudioListener>(), true);
						parent.DestroyAndClearRefs(mine.GetComponent<GUILayer>(), true);
						parent.DestroyAndClearRefs(mine.GetComponent("FlareLayer"), true);
					}
					parent.DestroyAndClearRefs(mine, true);
					parent.BubbleRefresh();
				}, delegate { //Delete theirs
					if (theirs is Camera) {
						parent.DestroyAndClearRefs(theirs.GetComponent<AudioListener>(), false);
						parent.DestroyAndClearRefs(theirs.GetComponent<GUILayer>(), false);
						parent.DestroyAndClearRefs(theirs.GetComponent("FlareLayer"), false);
					}
					parent.DestroyAndClearRefs(theirs, false);
					parent.BubbleRefresh();
				});
			} else GUILayout.Space(UniMergeConfig.midWidth*2);
			//Display theirs
			DrawComponent(false, width);
			ObjectMerge.EndRow();
		}
		if(show) {
			List<PropertyHelper> tmp = new List<PropertyHelper>(properties);
			foreach(PropertyHelper property in tmp) {
				property.Draw(width);
			}
		}
		if(mySO != null && mySO.targetObject != null)
			if(mySO.ApplyModifiedProperties())
				parent.BubbleRefresh();
		if(theirSO != null && theirSO.targetObject != null)
			if(theirSO.ApplyModifiedProperties())
				parent.BubbleRefresh();
	}

	private void DrawComponent(bool isMine, float width) {
		GUILayout.BeginVertical(GUILayout.Width(ObjectMerge.colWidth + 6));
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5		
		GUILayout.Space(3);
#endif
		UniMerge.Util.Indent(width, delegate {
			if(GetComponent(isMine)) {
				show = EditorGUILayout.Foldout(show, GetComponent(isMine).GetType().Name);
			} else GUILayout.Label("");
		});
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5		
		GUILayout.Space(-4);
#endif
		GUILayout.EndVertical();
	}

	internal int GetDrawCount() {
		int selfCount = 1;
		if (show) {
			selfCount += properties.Count;
		}
		return selfCount;
	}
}
public class PropertyHelper {
	public bool show, same;
	public SerializedProperty mine, theirs;
	readonly ComponentHelper parent;		//oh cool! If a field is set in the constructor, it can be readonly
	public SerializedProperty GetProperty(bool isMine) {
		return isMine ? mine : theirs;
	}
	public PropertyHelper(ComponentHelper parent, SerializedProperty mine, SerializedProperty theirs) {
		this.parent = parent;
		this.mine = mine;
		this.theirs = theirs;
	}
	public IEnumerable CheckSame() {
		if(mine == null || theirs == null) {
			same = false;
			yield break;
		}
		foreach(bool e in UniMerge.Util.PropEqual(mine, theirs, ObjectMerge.mine, ObjectMerge.theirs)) {
			if(ObjectMerge.YieldIfNeeded())
				yield return null;
			same = e;
		}
	}

	public void Draw(float width) {
		if (ObjectMerge.ObjectDrawCheck()) {
			ObjectMerge.StartRow(same);
			//Display mine
			DrawProperty(true, width);
			//Swap buttons
			if (mine != null && theirs != null) {
				ObjectMerge.DrawMidButtons(delegate { //Copy theirs to mine
					if (mine.propertyType == SerializedPropertyType.ObjectReference) {
						if (theirs.objectReferenceValue != null) {
							System.Type t = theirs.objectReferenceValue.GetType();
							if (t == typeof (GameObject)) {
								GameObject g = ObjectMerge.root.GetObjectSpouse((GameObject) theirs.objectReferenceValue, false);
								mine.objectReferenceValue = g ? g : theirs.objectReferenceValue;
							} else if (t.IsSubclassOf(typeof (Component))) {
								GameObject g = ObjectMerge.root.GetObjectSpouse(((Component) theirs.objectReferenceValue).gameObject, false);
								if (g) {
									Component c = g.GetComponent(t);
									mine.objectReferenceValue = c ?? g.AddComponent(t);
								} else mine.objectReferenceValue = theirs.objectReferenceValue;
							} else {
								mine.objectReferenceValue = theirs.objectReferenceValue;
							}

							if (mine.serializedObject.targetObject != null)
								mine.serializedObject.ApplyModifiedProperties();
							parent.parent.BubbleRefresh();
							GUIUtility.ExitGUI();
							return;
						}
					}
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				UniMerge.Util.SetProperty(theirs, mine);
#else
					mine.serializedObject.CopyFromSerializedProperty(theirs);
#endif
					if (mine.serializedObject.targetObject != null)
						mine.serializedObject.ApplyModifiedProperties();
					same = true;
					parent.parent.BubbleRefresh();
				}, delegate { //Copy mine to theirs
					if (theirs.propertyType == SerializedPropertyType.ObjectReference) {
						if (mine.objectReferenceValue != null) {
							System.Type t = mine.objectReferenceValue.GetType();
							if (t == typeof (GameObject)) {
								GameObject g = ObjectMerge.root.GetObjectSpouse((GameObject) mine.objectReferenceValue, true);
								theirs.objectReferenceValue = g ?? mine.objectReferenceValue;
							} else if (t.IsSubclassOf(typeof (Component))) {
								GameObject g = ObjectMerge.root.GetObjectSpouse(((Component) mine.objectReferenceValue).gameObject, true);
								if (g) {
									Component c = g.GetComponent(t);
									theirs.objectReferenceValue = c ?? g.AddComponent(t);
								} else theirs.objectReferenceValue = mine.objectReferenceValue;
							} else
								theirs.objectReferenceValue = mine.objectReferenceValue;
							if (theirs.serializedObject.targetObject != null)
								theirs.serializedObject.ApplyModifiedProperties();
							parent.parent.BubbleRefresh();
							GUIUtility.ExitGUI();
							return;
						}
					}
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				UniMerge.Util.SetProperty(mine, theirs);
#else
					theirs.serializedObject.CopyFromSerializedProperty(mine);
#endif
					if (theirs.serializedObject.targetObject != null)
						theirs.serializedObject.ApplyModifiedProperties();
					same = true;
					parent.parent.BubbleRefresh();
				});
			} else GUILayout.Space(UniMergeConfig.midWidth*2);
			//Display theirs
			DrawProperty(false, width);
			ObjectMerge.EndRow();
		}
	}
	private void DrawProperty(bool isMine, float width) {
		GUILayout.BeginVertical(GUILayout.Width(ObjectMerge.colWidth + 6));
		if(GetProperty(isMine) != null) {
			UniMerge.Util.Indent(width + UniMerge.Util.TAB_SIZE * 3, delegate {
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				EditorGUILayout.PropertyField(GetProperty(isMine));
#else
				EditorGUILayout.PropertyField(GetProperty(isMine), true);
#endif
			});
		} else GUILayout.Label("");
		GUILayout.Space(10);
		GUILayout.EndVertical();
	}
}