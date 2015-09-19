/*  This file is part of the "Simple Waypoint System" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace SWS
{
    /// <summary>
    /// Movement script: bezier curves.
    /// <summary>
    [AddComponentMenu("Simple Waypoint System/bezierMove")]
    public class bezierMove : MonoBehaviour
    {
        /// <summary>
        /// Path component to use for movement.
        /// <summary>
        public BezierPathManager pathContainer;

        /// <summary>
        /// Whether this object should start its movement at game launch.
        /// <summary>
        public bool onStart = false;

        /// <summary>
        /// Whether this object should walk to the first waypoint or spawn there.
        /// <summary>
        public bool moveToPath = false;

        /// <summary>
        /// reverse the movement direction on the path, typically used for "pingPong" behavior.
        /// <summary>
        public bool reverse = false;

        /// <summary>
        /// Waypoint index where this object should start its path. 
        /// This is the total index and includes intermediate waypoints.
        /// <summary>
        public int startPoint = 0;

        /// <summary>
        /// Current waypoint indicator on the path. 
        /// <summary>
        [HideInInspector]
        public int currentPoint = 0;

        /// <summary>
        /// Value to look ahead on the path when orientToPath is enabled (0-1).
        /// <summary>
        public float lookAhead = 0;

        /// <summary>
        /// Additional units to add on the y-axis.
        /// <summary>
        public float sizeToAdd = 0;

        /// <summary>
        /// Selection for speed-based movement or time in seconds per segment. 
        /// <summary>
        public enum TimeValue
        {
            time,
            speed
        }
        public TimeValue timeValue = TimeValue.speed;

        /// <summary>
        /// Speed or time value depending on the selected TimeValue type.
        /// <summary>
        public float speed = 5;
        //original speed when changing the tween's speed
        private float originSpeed;

        /// <summary>
        /// Custom curve when AnimationCurve has been selected as easeType.
        /// <summary>
        public AnimationCurve animEaseType;

        /// <summary>
        /// Supported movement looptypes when moving on the path. 
        /// <summary>
        public enum LoopType
        {
            none,
            loop,
            pingPong,
            yoyo
        }
        public LoopType loopType = LoopType.none;

        /// <summary>
        /// Waypoint array references of the requested path.
        /// <summary>
        [HideInInspector]
        public Vector3[] waypoints;
        //array of modified waypoint positions for the tween
        private Vector3[] wpPos;

        /// <summary>
        /// List of Unity Events invoked when reaching waypoints.
        /// <summary>
        [HideInInspector]
        public List<UnityEvent> events = new List<UnityEvent>();

        /// <summary>
        /// Animation path type, linear or curved.
        /// <summary>
        public DG.Tweening.PathType pathType = DG.Tweening.PathType.CatmullRom;

        /// <summary>
        /// Whether this object should orient itself to a different Unity axis
        /// <summary>
        public DG.Tweening.PathMode pathMode = DG.Tweening.PathMode.Full3D;

        /// <summary>
        /// Animation easetype on TimeValue type time.
        /// <summary>
        public DG.Tweening.Ease easeType = DG.Tweening.Ease.Linear;

        /// <summary>
        /// Option for locking a position axis.
        /// <summary>
        public DG.Tweening.AxisConstraint lockPosition = DG.Tweening.AxisConstraint.None;

        /// <summary>
        /// Option for locking a rotation axis with orientToPath enabled.
        /// <summary>
        public DG.Tweening.AxisConstraint lockRotation = DG.Tweening.AxisConstraint.None;

        //---DOTween animation helper variables---
        [HideInInspector]
        public Tweener tween;


        //check for automatic initialization
        void Start()
        {
            if (onStart)
                StartMove();
        }


        /// <summary>
        /// Starts movement. Can be called from other scripts to allow start delay.
        /// <summary>
        public void StartMove()
        {
            //don't continue without path container
            if (pathContainer == null)
            {
                Debug.LogWarning(gameObject.name + " has no path! Please set Path Container.");
                return;
            }

            //get array with waypoint positions
            waypoints = pathContainer.GetPathPoints();
            //cache original speed for future speed changes
            originSpeed = speed;
            
            //initialize waypoint positions
            startPoint = Mathf.Clamp(startPoint, 0, waypoints.Length - 1);
            int index = startPoint;
            if (reverse)
            {
                Array.Reverse(waypoints);
                index = waypoints.Length - 1 - index;
            }
            Initialize(index);

            Stop();
            CreateTween();
        }

        
        //initialize or update modified waypoint positions
        //fills array with original positions and adds custom height
        //check for message count and reinitialize if necessary
        private void Initialize(int startAt = 0)
        {
            if (!moveToPath) startAt = 0;
            wpPos = new Vector3[waypoints.Length - startAt];
            for (int i = 0; i < wpPos.Length; i++)
                wpPos[i] = waypoints[i + startAt] + new Vector3(0, sizeToAdd, 0);

            //message count is smaller than actual waypoint count,
            //add empty message per waypoint
            for (int i = events.Count; i <= pathContainer.bPoints.Count - 1; i++)
                events.Add(new UnityEvent());
        }


        //creates a new tween with given arguments that moves along the path
        private void CreateTween()
        {
            //prepare DOTween's parameters, you can look them up here
            //http://dotween.demigiant.com/documentation.php

            TweenParams parms = new TweenParams();
            //differ between speed or time based tweening
            if (timeValue == TimeValue.speed)
                parms.SetSpeedBased();
            if (loopType == LoopType.yoyo)
                parms.SetLoops(-1, DG.Tweening.LoopType.Yoyo);

            //apply ease type or animation curve
            if (easeType == DG.Tweening.Ease.INTERNAL_Custom)
                parms.SetEase(animEaseType);
            else
                parms.SetEase(easeType);

            if (moveToPath)
                parms.OnWaypointChange(OnWaypointReached);
            else
            {
                if (loopType == LoopType.yoyo)
                    parms.OnStepComplete(ReachedEnd);

                transform.position = wpPos[0];

                parms.OnWaypointChange(OnWaypointChange);
                parms.OnComplete(ReachedEnd);
            }

            tween = transform.DOPath(wpPos, originSpeed, pathType, pathMode, 1)
                             .SetAs(parms)
                             .SetOptions(false, lockPosition, lockRotation)
                             .SetLookAt(lookAhead);

            if (!moveToPath && startPoint > 0)
            {
                GoToWaypoint(startPoint);
                startPoint = 0;
            }
                                              
            //continue new tween with adjusted speed if it was changed before
            if (originSpeed != speed)
                ChangeSpeed(speed);
        }


        //called when moveToPath completes
        private void OnWaypointReached(int index)
        {
            if (index <= 0) return;

            Stop();
            moveToPath = false;
            Initialize();
            CreateTween();
        }


        //called at every waypoint, even intermediate ones
        private void OnWaypointChange(int index)
        {
            //find out if the waypoint is a major one
            bool matching = false;
            for (int i = 0; i < pathContainer.bPoints.Count; i++)
            {
                Vector3 wp = pathContainer.bPoints[i].wp.position;
                if (wp == waypoints[index])
                {
                    matching = true;
                    index = i;
                    break;
                }
            }

            //only call events on major waypoints
            if (!matching) return;
            currentPoint = index;

            if (events == null || events.Count - 1 < index || events[index] == null)
                return;

            events[index].Invoke();
        }


        private void ReachedEnd()
        {
            //each looptype has specific properties
            switch (loopType)
            {
                //none means the tween ends here
                case LoopType.none:
                    return;

                //in a loop we start from the beginning
                case LoopType.loop:
                    currentPoint = 0;
                    CreateTween();
                    break;

                //reversing waypoints then moving again
                case LoopType.pingPong:
                    reverse = !reverse;
                    Array.Reverse(waypoints);
                    Initialize();

                    CreateTween();
                    break;

                //indicate backwards direction
                case LoopType.yoyo:
                    reverse = !reverse;
                    break;
            }
        }


        /// <summary>
        /// Teleports to the defined waypoint index on the path.
        /// </summary>
        public void GoToWaypoint(int index)
        {
            if (tween == null)
                return;

            if (reverse) index = waypoints.Length - 1 - index;

            tween.ForceInit();
            tween.GotoWaypoint(index, true);
        }


        /// <summary>
        /// Pauses the current movement routine for a defined amount of time.
        /// <summary>
        public void Pause(float seconds = 0f)
        {
            StopCoroutine(Wait());
            if (tween != null)
                tween.Pause();

            if (seconds > 0)
                StartCoroutine(Wait(seconds));
        }


        //waiting routine
        private IEnumerator Wait(float secs = 0f)
        {
            yield return new WaitForSeconds(secs);
            Resume();
        }


        /// <summary>
        /// Resumes the currently paused movement routine.
        /// <summary>
        public void Resume()
        {
            if (tween != null)
                tween.Play();
        }


        /// <summary>
        /// Changes the current path of this object and starts movement.
        /// <summary>
        public void SetPath(BezierPathManager newPath)
        {
            //disable any running movement methods
            Stop();
            //set new path container
            pathContainer = newPath;
            //restart movement
            StartMove();
        }


        /// <summary>
        /// Disables any running movement routines.
        /// <summary>
        public void Stop()
        {
            StopAllCoroutines();

            if (tween != null)
                tween.Kill();
            tween = null;
        }


        /// <summary>
        /// Stops movement and resets to the first waypoint.
        /// <summary>
        public void ResetToStart()
        {
            currentPoint = 0;
            if (pathContainer)
            {
                transform.position = pathContainer.waypoints[currentPoint].position + new Vector3(0, sizeToAdd, 0);
            }
        }


        /// <summary>
        /// Change running tween speed by manipulating its timeScale.
        /// <summary>
        public void ChangeSpeed(float value)
        {
            //calulate new timeScale value based on original speed
            float newValue;
            if (timeValue == TimeValue.speed)
                newValue = value / originSpeed;
            else
                newValue = originSpeed / value;

            //set speed, change timeScale percentually
            speed = value;
            if (tween != null)
                tween.timeScale = newValue;
        }
    }
}