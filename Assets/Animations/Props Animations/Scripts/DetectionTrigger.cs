using UnityEngine;
using System;
using System.Collections.Generic;

[AddComponentMenu("Triggers/DetectionTrigger")]
public class DetectionTrigger : MonoBehaviour
{
    #region Properties

    #region Private Properties

    private GameObject ourGameObject;
    private GameObject GameObject
    {
        get
        {
            if (ourGameObject == null)
                ourGameObject = gameObject;
            return ourGameObject;
        }
    }

    protected Collider ourCollider;
    protected Collider Collider
    {
        get
        {
            if (ourCollider == null)
            {
                ourCollider = GetCollider();
                ourCollider.isTrigger = true;
            }
            return ourCollider;
        }
    }

    #endregion

    #region Public Properties

    public enum ColliderEnumType { Box, Capsule, Sphere, Wheel, Mesh }
    [SerializeField] private ColliderEnumType colliderType = ColliderEnumType.Sphere;
    public ColliderEnumType ColliderType
    {
        get { return colliderType; }
        set { colliderType = value; }
    }

    private Dictionary<int, Transform> ourColliders = new Dictionary<int, Transform>();
    public Dictionary<int, Transform> Colliders
    {
        get { return ourColliders; }
        set { ourColliders = value; }
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

    #region Unity Methods

    public void Awake()
    {
        if (Collider) ;
    }

    void OnTriggerEnter(Collider argCollider)
    {
        Debug.Log(argCollider.transform.GetInstanceID() + " " + argCollider.name);
        ourColliders.Add(argCollider.transform.GetInstanceID(), argCollider.transform);
    }

    void OnTriggerExit(Collider argCollider)
    {
        ourColliders.Remove(argCollider.transform.GetInstanceID());
    }

    void OnColliderEnter(Collision argCollider)
    {
        Debug.Log(argCollider.transform.GetInstanceID() + " " + argCollider.transform.name);
        ourColliders.Add(argCollider.transform.GetInstanceID(), argCollider.transform);
    }

    void OnColliderExit(Collision argCollider)
    {
        ourColliders.Remove(argCollider.transform.GetInstanceID());
    }

    #endregion

    #region Private Methods

    private Collider GetCollider()
    {
        Collider myCollider = null;
        switch (colliderType)
        {
            case ColliderEnumType.Box:
                myCollider = GetComponent(typeof(BoxCollider)) as BoxCollider;
                if (myCollider == null)
                    myCollider = GameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;
                break;
            case ColliderEnumType.Capsule:
                myCollider = GetComponent(typeof(CapsuleCollider)) as CapsuleCollider;
                if (myCollider == null)
                    myCollider = GameObject.AddComponent(typeof(CapsuleCollider)) as CapsuleCollider;
                break;
            case ColliderEnumType.Sphere:
                myCollider = GetComponent(typeof(SphereCollider)) as SphereCollider;
                if (myCollider == null)
                    myCollider = GameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
                break;
            case ColliderEnumType.Wheel:
                myCollider = GetComponent(typeof(WheelCollider)) as WheelCollider;
                if (myCollider == null)
                    myCollider = GameObject.AddComponent(typeof(WheelCollider)) as WheelCollider;
                break;
            case ColliderEnumType.Mesh:
                myCollider = GetComponent(typeof(MeshCollider)) as MeshCollider;
                if (myCollider == null)
                    myCollider = GameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
                break;
        }
        if (myCollider == null)
            throw new Exception("Trigger Item Has No Collider");

        return myCollider;
    }

    #endregion


    public bool IsTripped
    {
        get
        {
            if (ourColliders.Count == 0)
                return false;
            else
            {
                bool isTripped = false;
                foreach (Transform t in ourColliders.Values)
                    if (!Ignores.Contains(t))
                        isTripped = true;
                return isTripped;
            }
        }
    }

}

