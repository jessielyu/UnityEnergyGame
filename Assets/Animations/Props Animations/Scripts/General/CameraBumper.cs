using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
class CameraBumper
{
    #region Fields

    private RaycastHit hit;
    private bool isColliderHit;

    #endregion

    #region Properties

    #region Private Properties

    private GameObject ourBumper;
    private GameObject Bumper
    {
        get
        {
            if (ourBumper == null)
                ourBumper = new GameObject("Bumper");
            return ourBumper;
        }
        set { ourBumper = value; }
    }

    private DetectionTrigger ourDetectionTrigger;
    private DetectionTrigger DetectionTrigger
    {
        get
        {
            if (ourDetectionTrigger == null)
                ourDetectionTrigger = Bumper.AddComponent(typeof(DetectionTrigger)) as DetectionTrigger;
            return ourDetectionTrigger;
        }
    }


    #endregion

    #region Public Properties

    public enum CollisionType { None, Raycast, Collider }
    [SerializeField] private CollisionType collisionType = CollisionType.Raycast; // length of bumper ray
    public CollisionType Collision
    {
        get { return collisionType; }
        set { collisionType = value; }
    }

    [SerializeField] private float distanceCheck = 2.5f; // length of bumper
    public float DistanceCheck
    {
        get { return distanceCheck; }
        set { distanceCheck = value; }
    }

    [SerializeField] private float newCameraHeight = 1.0f; // adjust camera height while bumping
    public float NewCameraHeight
    {
        get { return newCameraHeight; }
        set { newCameraHeight = value; }
    }

    [SerializeField] private Vector3 offset = Vector3.zero; // allows offset of the bumper ray from target origin
    public Vector3 Offset
    {
        get { return offset; }
        set { offset = value; }
    }

    private List<Transform> ourIgnores = new List<Transform>();
    public List<Transform> Ignores
    {
        get { return ourIgnores; }
        set { ourIgnores = value; }
    }

    private List<Type> ourIgnoreTypes = new List<Type>();
    public List<Type> IgnoreTypes
    {
        get { return ourIgnoreTypes; }
        set { ourIgnoreTypes = value; }
    }
    #endregion

    #endregion

    #region Methods

    private bool IsBumperHit(Transform argTarget, Transform argCamera)
    {
        switch (collisionType)
        {
            case CollisionType.Collider:
                Bumper.transform.position = argTarget.position + offset + (-1 * argTarget.forward);
                DetectionTrigger.Ignores = Ignores;
                DetectionTrigger.IgnoreTypes = IgnoreTypes;
                return DetectionTrigger.IsTripped;
            case CollisionType.Raycast:
                // check to see if there is anything behind the target
                Vector3 back = argTarget.transform.TransformDirection(-1 * Vector3.forward);

                // cast the bumper ray out from rear and check to see if there is anything behind
                return Physics.Raycast(argTarget.TransformPoint(offset), back, out hit, distanceCheck)
                    && hit.transform != argTarget;
            default: return false;
        }
    }

    public Vector3 UpdatePosition(Transform argTarget, Transform argCamera, Vector3 argWantedPosition, float argT)
    {
        if (IsBumperHit(argTarget, argCamera))
        {
            argWantedPosition.x = hit.point.x;
            argWantedPosition.z = hit.point.z;
            argWantedPosition.y = Mathf.Lerp(hit.point.y + newCameraHeight, argWantedPosition.y, argT);
        }
        return argWantedPosition;
    }

    #endregion
}
