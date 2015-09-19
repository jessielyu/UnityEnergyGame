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

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using FoldoutDraw = ObjectMerge.EmptyVoid;

namespace UniMerge {
	public static class Util {
		public static float TAB_SIZE = 15;

		public static void Foldout(ref bool open, string title, FoldoutDraw draw) { 
			Foldout(ref open, new GUIContent(title), draw, null);
		}
		public static void Foldout(ref bool open, GUIContent content, FoldoutDraw draw) { 
			Foldout(ref open, content, draw, null);
		}
		public static void Foldout(ref bool open, string title, FoldoutDraw draw, FoldoutDraw moreLabel) {
			Foldout(ref open, new GUIContent(title), draw, moreLabel);
		}
		public static void Foldout(ref bool open, GUIContent content, FoldoutDraw draw, FoldoutDraw moreLabel) {
			GUILayout.BeginHorizontal();
			open = EditorGUILayout.Foldout(open, content);
			if(moreLabel != null)
				moreLabel.Invoke();
			GUILayout.EndHorizontal();
			if(open) {
				Indent(draw);
			}
		}
		public static void Indent(FoldoutDraw draw) { Indent(TAB_SIZE, draw); }
		public static void Indent(float width, FoldoutDraw draw) {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width(width));
			GUILayout.Label("");
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical();
			draw.Invoke();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}
		public static void Center(FoldoutDraw element) {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			element();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		//Not used but a nice little gem :)
		public static void ClearLog() {
			Assembly assembly = Assembly.GetAssembly(typeof(SceneView));

			System.Type type = assembly.GetType("UnityEditorInternal.LogEntries");
			MethodInfo method = type.GetMethod("Clear");
			method.Invoke(new object(), null);
		}
		public static void PingButton(string content, Object obj) {
			if(GUILayout.Button(content)) EditorGUIUtility.PingObject(obj);
		}
		//Looks like Unity already has one of these... doesn't work the way I want though
		public static IEnumerable<bool> PropEqual(SerializedProperty mine, SerializedProperty theirs, GameObject mineParent, GameObject theirsParent) {
			if(mine != null && theirs != null) {
				if(mine.propertyType == theirs.propertyType) {
					switch(mine.propertyType) {
						case SerializedPropertyType.AnimationCurve:
							if(mine.animationCurveValue.keys.Length == theirs.animationCurveValue.keys.Length) {
								for(int i = 0; i < mine.animationCurveValue.keys.Length; i++) {
									if(mine.animationCurveValue.keys[i].inTangent != theirs.animationCurveValue.keys[i].inTangent) {
										yield return false;
										yield break;
									}
									if(mine.animationCurveValue.keys[i].outTangent != theirs.animationCurveValue.keys[i].outTangent){
										yield return false;
										yield break;
									}
									if(mine.animationCurveValue.keys[i].time != theirs.animationCurveValue.keys[i].time){
										yield return false; 
										yield break;
									}
									if(mine.animationCurveValue.keys[i].value != theirs.animationCurveValue.keys[i].value){
										yield return false;
										yield break;
									}
								}
							} else {
								yield return false;
								yield break;
							}
							yield return true;
							yield break;
						case SerializedPropertyType.ArraySize:
							//Haven't handled this one... not sure it will come up
							//Debug.LogWarning("Got ArraySize type");
							break;
						case SerializedPropertyType.Boolean:
							yield return mine.boolValue == theirs.boolValue;
							yield break;
#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
						case SerializedPropertyType.Bounds:
							yield return mine.boundsValue == theirs.boundsValue;
							yield break;
#endif
						case SerializedPropertyType.Character:
							//TODO: Character comparison
							Debug.LogWarning("Got Character type. Need to add comparison function");
							break;
						case SerializedPropertyType.Color:
							yield return   mine.colorValue == theirs.colorValue;yield break;
						case SerializedPropertyType.Enum:
							yield return   mine.enumValueIndex == theirs.enumValueIndex;yield break;
						case SerializedPropertyType.Float:
							yield return   mine.floatValue == theirs.floatValue;yield break;
						case SerializedPropertyType.Generic:
							//Override equivalence for some types that will never be equal
							switch(mine.type) {
								case "NetworkViewID":
									yield return true;
									yield break;
								case "GUIStyle":	//Having trouble with this one
									yield return true;
									yield break;
							}
#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
						bool equal;
						if(mine.isArray) {
							if(mine.arraySize == theirs.arraySize) {
								for(int i = 0; i < mine.arraySize; i++) {
									equal = false;
									foreach(bool e in PropEqual(mine.GetArrayElementAtIndex(i), theirs.GetArrayElementAtIndex(i), mineParent, theirsParent)) {
										yield return e;
										equal = e;
									}
									if(!equal)
										yield break;
								}
								yield return true;
								yield break;
							}
							yield return false;
							yield break;
						}
						equal = false;
						foreach(var e in CheckChildren(mine, theirs, mineParent, theirsParent))
							equal = e;
						if (!equal)
							yield break;
#endif
							break;
#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
						case SerializedPropertyType.Gradient:
							Debug.LogWarning("Got Gradient type");
							break;
#endif
						case SerializedPropertyType.Integer:
							yield return  mine.intValue == theirs.intValue;
							yield break;
						case SerializedPropertyType.LayerMask:
#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
							yield return  mine.intValue == theirs.intValue;
#else						//TODO: fix layer mask compare
							yield return true;
#endif
							yield break;
						case SerializedPropertyType.ObjectReference:
							if(mine.objectReferenceValue && theirs.objectReferenceValue) {
								System.Type t = mine.objectReferenceValue.GetType();
								//If the property is a gameObject or component, compare equivalence by comparing names, and whether they're both children of the same parent (from different sides)
								if(t == typeof(GameObject)) {
									if(theirs.objectReferenceValue.GetType() != t) {
										Debug.LogWarning("EH? two properties of different types?");
										yield return false;
										yield break;
									}
									if(ChildOfParent((GameObject)mine.objectReferenceValue, mineParent)
										&& ChildOfParent((GameObject)theirs.objectReferenceValue, theirsParent)) {
										yield return mine.objectReferenceValue.name == theirs.objectReferenceValue.name;
										yield break;
									}
								}
								if(t.IsSubclassOf(typeof(Component))) {
									if(theirs.objectReferenceValue.GetType() != t) {
										Debug.LogWarning("EH? two properties of different types?");
										yield return false;
										yield break;
									}
									if(ChildOfParent(((Component)mine.objectReferenceValue).gameObject, mineParent)
										&& ChildOfParent(((Component)theirs.objectReferenceValue).gameObject, theirsParent)) {
										yield return mine.objectReferenceValue.name == theirs.objectReferenceValue.name;
										yield break;
									}
								}
							}
							yield return mine.objectReferenceValue == theirs.objectReferenceValue;
							yield break;
						case SerializedPropertyType.Rect:
							 yield return mine.rectValue == theirs.rectValue;
							 yield break;
						case SerializedPropertyType.String:
							 yield return mine.stringValue == theirs.stringValue;
							 yield break;
						case SerializedPropertyType.Vector2:
							 yield return mine.vector2Value == theirs.vector2Value;
							 yield break;
						case SerializedPropertyType.Vector3:
							 yield return mine.vector3Value == theirs.vector3Value;
							 yield break;
#if UNITY_4_5 || UNITY_4_5_0 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1
						case SerializedPropertyType.Quaternion:
							yield return mine.quaternionValue.Equals(theirs.quaternionValue);
							yield break;
#elif !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
						case (SerializedPropertyType)16:
							yield return mine.quaternionValue.Equals(theirs.quaternionValue);
							yield break;
#endif
							default:
							Debug.LogWarning("Unknown SeralizedPropertyType encountered: " + mine.propertyType);
							break;
					}
				} else Debug.LogWarning("Not same type?");
			}
			yield return true;
		}

		private static IEnumerable<bool> CheckChildren(SerializedProperty mine, SerializedProperty theirs, GameObject mineParent, GameObject theirsParent) {
			bool enter = true;
			if(mine == null && theirs == null)
				yield break;
			if(mine == null || theirs == null) {
				yield return  false;
				yield break;
			}
			SerializedProperty mTmp = mine.Copy();
			SerializedProperty tTmp = theirs.Copy();
			//I'm really not sure what's going on here. This seems to work differently for different types of properties
			while(mTmp.Next(enter)) {
				tTmp.Next(enter);
				enter = false;					//Maybe we want this?
				if(mTmp.depth != mine.depth)	//Once we're back in the same depth, we've gone too far
					break;
				bool equal = false;
				foreach (bool e in PropEqual(mTmp, tTmp, mineParent, theirsParent)) {
					yield return e;
					equal = e;
				}
				if(!equal)
					yield break;
			}
			yield return true;
		}
		//U3: Had to define out some types of components. Default case will warn me about this
		//Looks like this is already implemented...
		public static void SetProperty(SerializedProperty from, SerializedProperty to) {
			switch(from.propertyType) {
				case SerializedPropertyType.AnimationCurve:
					to.animationCurveValue = from.animationCurveValue;
					break;
				case SerializedPropertyType.ArraySize:
					Debug.LogWarning("Got ArraySize type");
					break;
				case SerializedPropertyType.Boolean:
					to.boolValue = from.boolValue;
					break;
				#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
				case SerializedPropertyType.Bounds:
					to.boundsValue = from.boundsValue;
					break;
				#endif
				case SerializedPropertyType.Character:
					Debug.LogWarning("Got Character type");
					break;
				case SerializedPropertyType.Color:
					to.colorValue = from.colorValue;
					break;
				case SerializedPropertyType.Enum:
					to.enumValueIndex = from.enumValueIndex;
					break;
				case SerializedPropertyType.Float:
					to.floatValue = from.floatValue;
					break;
				case SerializedPropertyType.Generic:
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
					Debug.LogWarning("How to copy generic properties in Unity 3?");
#else
					to.serializedObject.CopyFromSerializedProperty(from);
					to.serializedObject.ApplyModifiedProperties();
#endif
					break;
				#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
				case SerializedPropertyType.Gradient:
					Debug.LogWarning("Got Gradient type");
					break;
				#endif
				case SerializedPropertyType.Integer:
					to.intValue = from.intValue;
					break;
				case SerializedPropertyType.LayerMask:
					to.intValue = from.intValue;
					break;
				case SerializedPropertyType.ObjectReference:
					to.objectReferenceValue = from.objectReferenceValue;
					break;
				case SerializedPropertyType.Rect:
					to.rectValue = from.rectValue;
					break;
				case SerializedPropertyType.String:
					to.stringValue = from.stringValue;
					break;
				case SerializedPropertyType.Vector2:
					to.vector2Value = from.vector2Value;
					break;
				case SerializedPropertyType.Vector3:
					to.vector3Value = from.vector3Value;
					break;
#if UNITY_4_5 || UNITY_4_5_0 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1
				case SerializedPropertyType.Quaternion:
					to.quaternionValue = from.quaternionValue;
					break;
#elif !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
				case (SerializedPropertyType)16:			
					to.quaternionValue = from.quaternionValue;
					break;
#endif
				default:
					Debug.LogWarning("Unknown SeralizedPropertyType encountered: " + to.propertyType);
					break;
			}
		}
		public static bool ChildOfParent(GameObject obj, GameObject parent) {
			while(true) {
				if(obj == parent)
					return true;
				if(obj.transform.parent == null)
					break;
				obj = obj.transform.parent.gameObject;
			}
			return false;
		}
		public static void GameObjectToList(GameObject obj, List<GameObject> list) {
			list.Add(obj);
			foreach(Transform t in obj.transform)
				GameObjectToList(t.gameObject, list);
		}
		/// <summary>
		/// Determine whether the object is part of a prefab, and if it is the root of the prefab tree
		/// </summary>
		/// <param name="g"></param>
		/// <returns></returns>
		public static bool IsPrefabParent(GameObject g) {
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
			return EditorUtility.FindPrefabRoot(g) == g;
#else
			return PrefabUtility.FindPrefabRoot(g) == g;
#endif
		}
	}
}