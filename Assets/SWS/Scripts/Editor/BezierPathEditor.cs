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
    /// Custom bezier path inspector.
    /// <summary>
    [CustomEditor(typeof(BezierPathManager))]
    public class BezierPathEditor : Editor
    {
        //define reference we want to use/control
        private BezierPathManager script;
        //optional segment detail button toggle
        private bool showDetailSettings = false;
        //inspector scrollbar x/y position, modified by mouse input
        private Vector2 scrollPosDetail;


        //called whenever this inspector window is loaded 
        public void OnEnable()
        {
            //we create a reference to our script object by passing in the target
            script = (BezierPathManager)target;

            //reposition handles of the first and last point to the waypoint
            //they only have one control point so we set the other one to zero
            BezierPoint first = script.bPoints[0];
            first.cp[0].position = first.wp.position;
            BezierPoint last = script.bPoints[script.bPoints.Count - 1];
            last.cp[1].position = last.wp.position;

            //recalculate path points
            script.CalculatePath();
        }


        //adds a waypoint when clicking on the "+" button in the inspector
        private void AddWaypointAtIndex(int index)
        {
            //create a new bezier point property class
            BezierPoint point = new BezierPoint();
            //create new waypoint gameobject
            Transform wp = new GameObject("Waypoint").transform;

            //disabled because of a Unity bug that crashes the editor
            //Undo.RecordObject(script, "Add");
            //Undo.RegisterCreatedObjectUndo(wp, "Add");

            //set its position to the last one
            wp.position = script.bPoints[index].wp.position;
            //assign it to the class
            point.wp = wp;
            //assign new control points
            Transform left = new GameObject("Left").transform;
            Transform right = new GameObject("Right").transform;
            left.parent = right.parent = wp;
            left.position = wp.position + new Vector3(2, 0, 0);
            right.position = wp.position + new Vector3(-2, 0, 0);
            point.cp = new[] { left, right };
            
            //parent bezier point to the path gameobject
            wp.parent = script.transform;
            //add new detail value for the new segment
            script.segmentDetail.Insert(index + 1, script.pathDetail);
            //finally, insert this new waypoint after the one clicked
            script.bPoints.Insert(index + 1, point);
        }


        //removes a waypoint when clicking on the "-" button in the inspector
        private void RemoveWaypointAtIndex(int index)
        {
            Undo.RecordObject(script, "Remove");
            //remove corresponding detail value
            script.segmentDetail.RemoveAt(index - 1);
            //remove the point from the list
            Undo.DestroyObjectImmediate(script.bPoints[index].wp.gameObject);
            script.bPoints.RemoveAt(index);
        }


        public override void OnInspectorGUI()
        {
            //don't draw inspector fields if the path contains less than 2 points
            //(a path with less than 2 points really isn't a path)
            if (script.bPoints.Count < 2)
                return;

            //force naming scheme
            RenameWaypoints();

            //checkbox field to enable editable path properties
            EditorGUILayout.BeginHorizontal();
            script.showHandles = EditorGUILayout.Toggle("Show Handles", script.showHandles);
            EditorGUILayout.EndHorizontal();

            //checkbox field for drawing gizmo path lines
            script.drawCurved = EditorGUILayout.Toggle("Draw Smooth Lines", script.drawCurved);

            //create new color fields for editing path gizmo colors 
            script.color1 = EditorGUILayout.ColorField("Color1", script.color1);
            script.color2 = EditorGUILayout.ColorField("Color2", script.color2);
            script.color3 = EditorGUILayout.ColorField("Color3", script.color3);

            //calculate path length of all waypoints
            float pathLength = WaypointManager.GetPathLength(script.pathPoints);
            //path length label, show calculated path length
            GUILayout.Label("Path Length: " + pathLength);

            float thisDetail = script.pathDetail;
            //slider to modify the smoothing factor of the final path
            script.pathDetail = EditorGUILayout.Slider("Path Detail", script.pathDetail, 0.5f, 10);
            //toggle custom detail when modifying the whole path
            if (thisDetail != script.pathDetail)
                script.customDetail = false;

            //draw custom detail settings
            EditorGUILayout.Space();
            DetailSettings();
            EditorGUILayout.Space();

            //waypoint index header
            GUILayout.Label("Waypoints: ", EditorStyles.boldLabel);

            //loop through the waypoint array
            for (int i = 0; i < script.bPoints.Count; i++)
            {
                GUILayout.BeginHorizontal();
                //indicate each array slot with index number in front of it
                GUILayout.Label(i + ".", GUILayout.Width(20));

                //create an object field for every waypoint
                EditorGUILayout.ObjectField(script.bPoints[i].wp, typeof(Transform), true);

                //display an "Add Waypoint" button for every array row except the last one
                //on click we call AddWaypointAtIndex() to insert a new waypoint slot AFTER the selected slot
                if (i < script.bPoints.Count && GUILayout.Button("+", GUILayout.Width(30f)))
                {
                    AddWaypointAtIndex(i);
                    break;
                }

                //display an "Remove Waypoint" button for every array row except the first and last one
                //on click we call RemoveWaypointAtIndex() to delete the selected waypoint slot
                if (i > 0 && i < script.bPoints.Count - 1 && GUILayout.Button("-", GUILayout.Width(30f)))
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
                foreach (BezierPoint bp in script.bPoints)
                {
                    //define ray to cast downwards waypoint position
                    Ray ray = new Ray(bp.wp.position + new Vector3(0, 2f, 0), -Vector3.up);
                    Undo.RecordObject(bp.wp, "PlaceToGround");

                    RaycastHit hit;
                    //cast ray against ground, if it hit:
                    if (Physics.Raycast(ray, out hit, 100))
                    {
                        //position waypoint to hit point
                        bp.wp.position = hit.point;
                    }

                    //also try to raycast against 2D colliders
                    RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, -Vector2.up, 100);
                    if (hit2D)
                    {
                        bp.wp.position = new Vector3(hit2D.point.x, hit2D.point.y, bp.wp.position.z);
                    }
                }
            }

            EditorGUILayout.Space();

            //invert direction of whole path
            if (GUILayout.Button("Invert Direction"))
            {
                Undo.RecordObject(script, "Invert");

                //to reverse the whole path we need to know where the waypoints were before
                //for this purpose a new copy must be created
                BezierPoint[] waypointCache = new BezierPoint[script.bPoints.Count];
                for (int i = 0; i < waypointCache.Length; i++)
                    waypointCache[i] = script.bPoints[i];

                //reverse order based on the old list
                for (int i = 0; i < waypointCache.Length; i++)
                {
                    BezierPoint currentPoint = script.bPoints[waypointCache.Length - 1 - i];
                    script.bPoints[waypointCache.Length - 1 - i] = waypointCache[i];
                    Vector3 leftHandle = currentPoint.cp[0].position;

                    Undo.RecordObject(currentPoint.cp[0], "Invert");
                    Undo.RecordObject(currentPoint.cp[1], "Invert");

                    currentPoint.cp[0].position = currentPoint.cp[1].position;
                    currentPoint.cp[1].position = leftHandle;
                }
            }

            EditorGUILayout.Space();

            //draw object field for new waypoint object
            script.replaceObject = (GameObject)EditorGUILayout.ObjectField("Replace Object", script.replaceObject, typeof(GameObject), true);

            //replace all waypoints with the prefab
            if (GUILayout.Button("Replace Waypoints with Object"))
            {
                ReplaceWaypoints();
            }

            //recalculate on inspector changes
            if (GUI.changed)
            {
                script.CalculatePath();
                EditorUtility.SetDirty(target);
            }
        }


        private void DetailSettings()
        {
            if (showDetailSettings)
            {
                if (GUILayout.Button("Hide Detail Settings"))
                    showDetailSettings = false;

                //draw bold settings checkbox
                GUILayout.Label("Segment Detail:", EditorStyles.boldLabel);
                script.customDetail = EditorGUILayout.Toggle("Enable Custom", script.customDetail);

                EditorGUILayout.BeginHorizontal();
                //begin a scrolling view inside GUI, pass in Vector2 scroll position 
                scrollPosDetail = EditorGUILayout.BeginScrollView(scrollPosDetail, GUILayout.Height(105));

                //loop through waypoint array
                for (int i = 0; i < script.bPoints.Count - 1; i++)
                {
                    float thisDetail = script.segmentDetail[i];
                    //create a float slider for every segment detail setting
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(i + "-" + (i + 1) + ".");
                    script.segmentDetail[i] = EditorGUILayout.Slider(script.segmentDetail[i], 0.5f, 10);
                    EditorGUILayout.EndHorizontal();
                    //toggle custom detail when modifying individual segments
                    if (thisDetail != script.segmentDetail[i])
                        script.customDetail = true;
                }

                //ends the scrollview defined above
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                //if path is set but detail settings are not shown,
                //draw button to toggle showDelaySetup
                if (GUILayout.Button("Show Detail Settings"))
                    showDetailSettings = true;
            }
        }


        private void ReplaceWaypoints()
        {
            if (script.replaceObject == null)
            {
                Debug.LogWarning("You haven't specified a replace object. Cancelling.");
                return;
            }

            Undo.RecordObject(script, "Replace");

            //old waypoints to remove after replace
            List<GameObject> toRemove = new List<GameObject>();
            //loop through waypoint list
            for (int i = 0; i < script.bPoints.Count; i++)
            {
                //get current bezier point at index position
                BezierPoint point = script.bPoints[i];
                Transform curWP = point.wp;
                //instantiate new waypoint at old position
                Transform newCur = ((GameObject)Instantiate(script.replaceObject, curWP.position, Quaternion.identity)).transform;
                Undo.RegisterCreatedObjectUndo(newCur.gameObject, "Replace");
                
                //parent control points to the new bezier point
                Undo.SetTransformParent(point.cp[0], newCur, "Replace");
                Undo.SetTransformParent(point.cp[1], newCur, "Replace");
                //parent new waypoint to this path
                newCur.parent = point.wp.parent;
                
                //replace old waypoint at index
                script.bPoints[i].wp = newCur;
                //indicate to remove old waypoint
                toRemove.Add(curWP.gameObject);
            }

            //destroy old waypoints
            for (int i = 0; i < toRemove.Count; i++)
                Undo.DestroyObjectImmediate(toRemove[i]);
        }


        //renaming scheme
        private void RenameWaypoints()
        {
            for (int i = 0; i < script.bPoints.Count; i++)
                script.bPoints[i].wp.name = "Waypoint " + i;
        }


        //if this path is selected, display small info boxes above all waypoint positions
        //also display handles for the waypoints and their bezier points
        void OnSceneGUI()
        {
            //do not execute further code if we have no waypoints defined
            //(just to make sure, practically this can not occur)
            if (script.bPoints.Count == 0) return;

            //begin GUI block
            Handles.BeginGUI();
            //loop through waypoint list
            for (int i = 0; i < script.bPoints.Count; i++)
            {
                //translate waypoint vector3 position in world space into a position on the screen
                var guiPoint = HandleUtility.WorldToGUIPoint(script.bPoints[i].wp.position);
                //create rectangle with that positions and do some offset
                var rect = new Rect(guiPoint.x - 50.0f, guiPoint.y - 40, 100, 20);
                //draw box at position with current waypoint name
                GUI.Box(rect, "Waypoint: " + i);
            }
            Handles.EndGUI(); //end GUI block

            //handles
            for (int i = 0; i < script.bPoints.Count; i++)
            {
                Handles.color = script.color2;
                //get related bezier point class
                BezierPoint point = script.bPoints[i];

                //draw bezier point handles
                Vector3 wpPos = point.wp.position;
                float size = HandleUtility.GetHandleSize(wpPos) * 0.4f;
                Vector3 newPos = Handles.FreeMoveHandle(wpPos, Quaternion.identity,
                                 size, Vector3.zero, Handles.SphereCap);
                Handles.RadiusHandle(Quaternion.identity, wpPos, size / 2);

                if (wpPos != newPos)
                {
                    Undo.RecordObject(point.wp, "Move Handles");
                    point.wp.position = newPos;
                }

                if (!script.showHandles) continue;
                
                Handles.color = script.color3;
                //draw control point handles
                //left handle, all control points except first one
                if (i > 0)
                {
                    PositionOpposite(point, true,
                        Handles.FreeMoveHandle(
                        point.cp[0].position,
                        Quaternion.identity,
                        HandleUtility.GetHandleSize(point.cp[0].position) * 0.24f,
                        Vector3.zero,
                        Handles.SphereCap));

                    Undo.RecordObject(point.cp[0], "Move Control Left");
                }
                //right handle, all waypoints except last one
                if (i < script.bPoints.Count - 1)
                {
                    PositionOpposite(point, false,
                        Handles.FreeMoveHandle(
                        point.cp[1].position,
                        Quaternion.identity,
                        HandleUtility.GetHandleSize(point.cp[0].position) * 0.24f, 
                        Vector3.zero,
                        Handles.SphereCap));

                    Undo.RecordObject(point.cp[1], "Move Control Right");
                }

                //draw line between control points
                Handles.DrawLine(point.cp[0].position, point.cp[1].position);
            }

            if (GUI.changed)
                EditorUtility.SetDirty(target);

            //recalculate path points after handles
            script.CalculatePath();

            if (!script.showHandles) return;

            //draw small dots for each path point (not waypoint)
            Handles.color = script.color2;
            Vector3[] pathPoints = script.pathPoints;
            for (int i = 0; i < pathPoints.Length; i++)
                Handles.SphereCap(0, pathPoints[i], Quaternion.identity, 
                                  HandleUtility.GetHandleSize(pathPoints[i]) * 0.12f);
        }


        //repositions the opposite control point if one changes
        private void PositionOpposite(BezierPoint point, bool isLeft, Vector3 newPos)
        {
            Vector3 pos = point.wp.position;
            Vector3 toParent = pos - newPos;
            toParent.Normalize();

            if (isLeft)
            {
                //received the left handle, manipulating the right
                float magnitude = (pos - point.cp[1].position).magnitude;
                point.cp[0].position = newPos;
                point.cp[1].position = pos + toParent * magnitude;
            }
            else
            {
                //received the right handle, manipulating the left
                float magnitude = (pos - point.cp[0].position).magnitude;
                point.cp[1].position = newPos;
                point.cp[0].position = pos + toParent * magnitude;
            }
        }
    }
}