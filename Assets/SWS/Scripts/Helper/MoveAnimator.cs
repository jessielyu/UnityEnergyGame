/*  This file is part of the "Simple Waypoint System" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace SWS
{
    /// <summary>
    /// Mecanim motion animator for movement scripts.
    /// Passes speed and direction to the Mecanim controller. 
    /// <summary>
    public class MoveAnimator : MonoBehaviour
    {
        /// <summary>
        /// Selection type for the movement script attached to this object.
        /// <summary>
        public enum MovementType
        {
            splineMove,
            bezierMove,
            navMove
        }
        public MovementType mType = MovementType.splineMove;

        //movement script references
        private splineMove hMove;
        private bezierMove bMove;
        private NavMeshAgent nAgent;
        //Mecanim animator reference
        private Animator animator;
        //cached y-rotation on tweens
        private float lastRotY;


        //getting component references
        void Start()
        {
            animator = GetComponentInChildren<Animator>();

            switch (mType)
            {
                case MovementType.splineMove:
                    hMove = GetComponent<splineMove>();
                    break;
                case MovementType.bezierMove:
                    bMove = GetComponent<bezierMove>();
                    break;
                case MovementType.navMove:
                    nAgent = GetComponent<NavMeshAgent>();
                    break;
            }
        }


        //method override for root motion on the animator
        void OnAnimatorMove()
        {
            //init variables
            float speed = 0f;
            float angle = 0f;

            //calculate variables based on movement script:
            //for splineMove and bezierMove, speed and rotation are regulated by the tween.
            //here we just get the tween's speed and calculate the rotation difference to the last frame.
            //navmesh agents have their own type of movement which has to be calculated separately.
            switch (mType)
            {
                case MovementType.splineMove:
                    speed = hMove.tween == null || !hMove.tween.IsPlaying() ? 0f : hMove.speed;
                    angle = (transform.eulerAngles.y - lastRotY) * 10;
                    lastRotY = transform.eulerAngles.y;
                    break;
                case MovementType.bezierMove:
                    speed = bMove.tween == null || !bMove.tween.IsPlaying() ? 0f : bMove.speed;
                    angle = (transform.eulerAngles.y - lastRotY) * 10;
                    lastRotY = transform.eulerAngles.y;
                    break;
                case MovementType.navMove:
                    speed = nAgent.velocity.magnitude;
                    Vector3 velocity = Quaternion.Inverse(transform.rotation) * nAgent.desiredVelocity;
                    angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;
                    break;
            }

            //push variables to the animator with some optional damping
            animator.SetFloat("Speed", speed);
            animator.SetFloat("Direction", angle, 0.15f, Time.deltaTime);
        }
    }
}