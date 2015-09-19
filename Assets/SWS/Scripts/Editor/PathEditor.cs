/*  This file is part of the "Simple Waypoint System" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace SWS
{
    /// <summary>
    /// Custom path inspector.
    /// <summary>
    [CustomEditor(typeof(PathManager))]
    public class PathEditor : Editor
    {
        //define Serialized Objects we want to use/control
        //this will be our serialized reference to the inspected script
        private SerializedObject m_Object;
        //serialized waypoint array
        private SerializedProperty m_Waypoint;
        //serialized waypoint array count
        private SerializedProperty m_WaypointsCount;
        //serialized path gizmo property booleans
        private SerializedProperty m_Check1;
        //serialized scene view gizmo colors
        private SerializedProperty m_Color1;
        private SerializedProperty m_Color2;
        //serialized replace object gameobject
        private SerializedProperty m_WaypointPref;

        //waypoint array size, define path to know where to lookup for this variable
        //(we expect an array, so it's "name_of_array.data_type.size")
        private static string wpArraySize = "waypoints.Array.size";
        //.data gives us the data of the array,
        //we replace this {0} token with an index we want to get
        private static string wpArrayData = "waypoints.Array.data[{0}]";


        //called whenever this inspector window is loaded 
        public void OnEnable()
        {
            //we create a reference to our script object by passing in the target
            m_Object = new SerializedObject(target);

            //from this object, we pull out the properties we want to use
            //these are just the names of our variables in the manager
            m_Check1 = m_Object.FindProperty("drawCurved");
            m_Color1 = m_Object.FindProperty("color1");
            m_Color2 = m_Object.FindProperty("color2");
            m_WaypointPref = m_Object.FindProperty("replaceObject");

            //set serialized waypoint array count by passing in the path to our array size
            m_WaypointsCount = m_Object.FindProperty(wpArraySize);
        }


        private Transform[] GetWaypointArray()
        {
            //get array count from serialized property and store its int value into var arrayCount
            var arrayCount = m_Object.FindProperty(wpArraySize).intValue;
            //create new waypoint transform array with size of arrayCount
            var transformArray = new Transform[arrayCount];
            //loop over waypoints
            for (var i = 0; i < arrayCount; i++)
            {
                //for each one use "FindProperty" to get the associated object reference
                //of waypoints array, string.Format replaces {0} token with index i
                //and store the object reference value as type of transform in transformArray[i]
                transformArray[i] = m_Object.FindProperty(string.Format(wpArrayData, i)).objectReferenceValue as Transform;
            }
            //finally return that array copy for modification purposes
            return transformArray;
        }


        //similiar to GetWaypointArray(), find serialized property which belongs to index
        //and set this value to parameter transform "waypoint" directly
        private void SetWaypoint(int index, Transform waypoint)
        {
            m_Object.FindProperty(string.Format(wpArrayData, index)).objectReferenceValue = waypoint;
        }


        //similiar to SetWaypoint(), this will find the waypoint from array at index position
        //and returns it instead of modifying
        private Transform GetWaypointAtIndex(int index)
        {
            return m_Object.FindProperty(string.Format(wpArrayData, index)).objectReferenceValue as Transform;
        }


        //get the corresponding waypoint and destroy the whole gameobject in editor
        private void RemoveWaypointAtIndex(int index)
        {
            Undo.DestroyObjectImmediate(GetWaypointAtIndex(index).gameObject);

            //iterate over the array, starting at index,
            //and replace it with the next one
            for (int i = index; i < m_WaypointsCount.intValue - 1; i++)
                SetWaypoint(i, GetWaypointAtIndex(i + 1));

            //decrement array count by 1
            m_WaypointsCount.intValue--;
        }


        private void AddWaypointAtIndex(int index)
        {
            //increment array count so the waypoint array is one unit larger
            m_WaypointsCount.intValue++;

            //backwards loop through array:
            //since we're adding a new waypoint for example in the middle of the array,
            //we need to push all existing waypoints after that selected waypoint
            //1 slot upwards to have one free slot in the middle. So:
            //we're doing exactly that and start looping at the end downwards to the selected slot
            for (int i = m_WaypointsCount.intValue - 1; i > index; i--)
                SetWaypoint(i, GetWaypointAtIndex(i - 1));

            //create new waypoint gameobject
            GameObject wp = new GameObject("Waypoint");
            Undo.RegisterCreatedObjectUndo(wp, "Created WP");

            //set its position to the last one
            wp.transform.position = GetWaypointAtIndex(index).position;
            //parent it to the path gameobject
            wp.transform.parent = GetWaypointAtIndex(index).parent;
            //finally, set this new waypoint after the one clicked in waypoints array
            SetWaypoint(index + 1, wp.transform);
        }


        //called whenever the inspector gui gets rendered
        public override void OnInspectorGUI()
        {
            //don't draw inspector fields if the path contains less than 2 points
            //(a path with less than 2 points really isn't a path)
            if (m_WaypointsCount.intValue < 2)
                return;

            //this pulls the relative variables from unity runtime and stores them in the object
            m_Object.Update();

            //create new checkboxes for path gizmo property 
            m_Check1.boolValue = EditorGUILayout.Toggle("Draw Smooth Lines", m_Check1.boolValue);

            //create new property fields for editing waypoint gizmo colors 
            EditorGUILayout.PropertyField(m_Color1);
            EditorGUILayout.PropertyField(m_Color2);

            //get waypoint array
            var waypoints = GetWaypointArray();

            //force naming scheme
            RenameWaypoints();

            //calculate path length of all waypoints
            Vector3[] wpPositions = new Vector3[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++)
                wpPositions[i] = waypoints[i].position;
            float pathLength = WaypointManager.GetPathLength(wpPositions);
            //path length label, show calculated path length
            GUILayout.Label("Path Length: " + pathLength);

            //waypoint index header
            GUILayout.Label("Waypoints: ", EditorStyles.boldLabel);

            //loop through the waypoint array
            for (int i = 0; i < waypoints.Length; i++)
            {
                GUILayout.BeginHorizontal();
                //indicate each array slot with index number in front of it
                GUILayout.Label(i + ".", GUILayout.Width(20));
                //create an object field for every waypoint
                EditorGUILayout.ObjectField(waypoints[i], typeof(Transform), true);

                //display an "Add Waypoint" button for every array row except the last one
                if (i < waypoints.Length && GUILayout.Button("+", GUILayout.Width(30f)))
                {
                    AddWaypointAtIndex(i);
                    break;
                }

                //display an "Remove Waypoint" button for every array row except the first and last one
                if (i > 0 && i < waypoints.Length - 1 && GUILayout.Button("-", GUILayout.Width(30f)))
                {
                    RemoveWaypointAtIndex(i);
                    break;
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            //button to move all waypoints down to the ground
            if (GUILayout.Button("Place to Ground"))
            {
                //for each waypoint of this path
                foreach (Transform trans in waypoints)
                {
                    //define ray to cast downwards waypoint position
                    Ray ray = new Ray(trans.position + new Vector3(0, 2f, 0), -Vector3.up);
                    Undo.RecordObject(trans, "");

                    RaycastHit hit;
                    //cast ray against ground, if it hit:
                    if (Physics.Raycast(ray, out hit, 100))
                    {
                        //position y values of waypoint to hit point
                        trans.position = hit.point;
                    }

                    //also try to raycast against 2D colliders
                    RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, -Vector2.up, 100);
                    if (hit2D)
                    {
                        trans.position = new Vector3(hit2D.point.x, hit2D.point.y, trans.position.z);
                    }
                }
            }

            EditorGUILayout.Space();

            //invert direction of whole path
            if (GUILayout.Button("Invert Direction"))
            {
                Undo.RecordObjects(waypoints, "");

                //to reverse the whole path we need to know where the waypoints were before
                //for this purpose a new copy must be created
                Vector3[] waypointCopy = new Vector3[waypoints.Length];
                for (int i = 0; i < waypoints.Length; i++)
                    waypointCopy[i] = waypoints[i].position;

                //looping over the array in reversed order
                for (int i = 0; i < waypoints.Length; i++)
                    waypoints[i].position = waypointCopy[waypointCopy.Length - 1 - i];
            }

            EditorGUILayout.Space();

            //draw object field for waypoint prefab
            EditorGUILayout.PropertyField(m_WaypointPref);

            //replace all waypoints with the prefab
            if (GUILayout.Button("Replace Waypoints with Object"))
            {
                if (m_WaypointPref == null)
                {
                    Debug.LogWarning("No replace object set. Cancelling.");
                    return;
                }

                ReplaceWaypoints();
            }

            //we push our modified variables back to our serialized object
            m_Object.ApplyModifiedProperties();
        }


        private void ReplaceWaypoints()
        {
            //get prefab object and path transform
            var waypointPrefab = m_WaypointPref.objectReferenceValue as GameObject;
            var path = GetWaypointAtIndex(0).parent;

            if (waypointPrefab == null)
            {
                Debug.LogWarning("You haven't specified a replace object. Cancelling.");
                return;
            }

            //loop through waypoint array of this path
            for (int i = 0; i < m_WaypointsCount.intValue; i++)
            {
                //get current waypoint at index position
                Transform curWP = GetWaypointAtIndex(i);
                //instantiate new waypoint at old position
                Transform newCur = ((GameObject)Instantiate(waypointPrefab, curWP.position, Quaternion.identity)).transform;
                Undo.RegisterCreatedObjectUndo(newCur.gameObject, "New");

                //parent new waypoint to this path
                newCur.parent = path;
                //replace old waypoint at index
                SetWaypoint(i, newCur);

                //destroy old waypoint object
                Undo.DestroyObjectImmediate(curWP.gameObject);
            }
        }


        //renaming scheme
        private void RenameWaypoints()
        {
            Transform[] waypointArray = GetWaypointArray();
            for (int i = 0; i < waypointArray.Length; i++)
                waypointArray[i].name = "Waypoint " + i;
        }


        //if this path is selected, display small info boxes above all waypoint positions
        //also display handles for the waypoints
        void OnSceneGUI()
        {
            //again, get waypoint array
            var waypoints = GetWaypointArray();
            //do not execute further code if we have no waypoints defined
            //(just to make sure, practically this can not occur)
            if (waypoints.Length == 0) return;

            //loop through waypoint array
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (!waypoints[i]) return;

                //begin 2D GUI block
                Handles.BeginGUI();
                //translate waypoint vector3 position in world space into a position on the screen
                var guiPoint = HandleUtility.WorldToGUIPoint(waypoints[i].position);
                //create rectangle with that positions and do some offset
                var rect = new Rect(guiPoint.x - 50.0f, guiPoint.y - 40, 100, 20);
                //draw box at position with current waypoint name
                GUI.Box(rect, "Waypoint: " + i);
                Handles.EndGUI(); //end GUI block

                //draw handles per waypoint
                Handles.color = m_Color2.colorValue;
                Vector3 wpPos = waypoints[i].position;
                float size = HandleUtility.GetHandleSize(wpPos) * 0.4f;
                Vector3 newPos = Handles.FreeMoveHandle(wpPos, Quaternion.identity,
                                 size, Vector3.zero, Handles.SphereCap);
                Handles.RadiusHandle(Quaternion.identity, wpPos, size / 2);

                if (wpPos != newPos)
                {
                    Undo.RecordObject(waypoints[i], "Move Handles");
                    waypoints[i].position = newPos;
                }
            }
        }
    }
}