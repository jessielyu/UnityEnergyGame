using UnityEngine;
using System;
using System.Collections.Generic;

//This script was taken directly from here: http://wiki.unity3d.com/index.php/SmoothFollowAdvanced 
//so the cred goes to Daniel P. Rossi for it and the associated scripts (CameraBumper.cs, CameraControl.cs, LimitedFloat.cs, DetectionTrigger.cs)

[AddComponentMenu("Camera-Control/SmoothCameraAdvanced")]
class SmoothCameraAdvanced : MonoBehaviour
{
    #region Private Properties

    private static SmoothCameraAdvanced instance = null;
    public static SmoothCameraAdvanced Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(SmoothCameraAdvanced)) as SmoothCameraAdvanced;
            if (instance == null && Camera.main != null)
                instance = Camera.main.gameObject.AddComponent(typeof(SmoothCameraAdvanced)) as SmoothCameraAdvanced;
            return instance;
        }
    }

    // Unity property "transform" calls GetComponent(Transform), so we store the value
    private Transform ourCameraTransform;
    private static Transform CameraTransform
    {
        get
        {
            if (Instance.ourCameraTransform == null)
            {
                Instance.ourCameraTransform = Instance.transform;
                Bumper.Ignores.Add(Instance.ourCameraTransform);
            }
            return Instance.ourCameraTransform;
        }
    }

    #endregion

    #region Public Properties

    [SerializeField] private Transform target = null;
    public static Transform Target
    {
        set
        {
            // Remove Prior Target
            Bumper.Ignores.Remove(Instance.target);

            Instance.target = value;

            // Add New Target
            Bumper.Ignores.Add(Instance.target);
        }
        get { return Instance.target; }
    }

    [SerializeField] public CameraBumper bumper;
    public static CameraBumper Bumper
    {
        get { return Instance.bumper; }
        set { Instance.bumper = value; }
    }

    [SerializeField] List<CameraControl> controls = new List<CameraControl>();
    public static List<CameraControl> Controls
    {
        get { return Instance.controls; }
        set { Instance.controls = value; }
    }


    #region Offset Properties

    [SerializeField] private Vector3 lookAtOffset; // allows offsetting of camera lookAt, very useful for low bumper heights
    public static Vector3 LookAtOffset
    {
        get { return Instance.lookAtOffset; }
        set { Instance.lookAtOffset = value; }
    }

    private Vector3 runtimeOffset = Vector3.zero;
    private static Vector3 RuntimeOffset
    {
        set { PanX.Current = value.x; }
        get { return new Vector3(PanX.Current, PanY.Current,0); }
    }

    #endregion

    #region MovementType Properties

    public enum MovementType { Instant, LinearInterpolation, SphericalLinearInterpolation }

    [SerializeField] private MovementType rotationType = MovementType.SphericalLinearInterpolation;
    private static MovementType RotationType
    {
        get { return Instance.rotationType; }
        set { Instance.rotationType = value; }
    }

    [SerializeField] private MovementType translationType = MovementType.SphericalLinearInterpolation;
    public static MovementType TranslationType
    {
        get { return Instance.translationType; }
        set { Instance.translationType = value; }
    }

    #endregion

    #region Position Properties

    [SerializeField] private LimitedFloat distance = new LimitedFloat(3.0f, 1, 10);
    public static LimitedFloat Distance
    {
        get { return Instance.distance; }
        set { Instance.distance = value; }
    }

    [SerializeField] private LimitedFloat height = new LimitedFloat(1.0f, 1, 5);
    public static LimitedFloat Height
    {
        get { return Instance.height; }
        set { Instance.height = value; }
    }

    [SerializeField] private LimitedFloat panX = new LimitedFloat(0.0f, -1, 1);
    public static LimitedFloat PanX
    {
        get { return Instance.panX; }
        set { Instance.panX = value; }
    }

    [SerializeField] private LimitedFloat panY = new LimitedFloat(0.0f, 0, 2);
    public static LimitedFloat PanY
    {
        get { return Instance.panY; }
        set { Instance.panY = value; }
    }

    #endregion

    #region Damping Properties

    [SerializeField] private float damping = 5.0f;
    public static float Damping
    {
        get { return Instance.damping; }
        set { Instance.damping = value; }
    }

    [SerializeField] private float rotationDamping = 10.0f;
    public static float RotationDamping
    {
        get { return Instance.rotationDamping; }
        set { Instance.rotationDamping = value; }
    }

    #endregion

    #endregion

    #region Unity Methods

    void Reset()
    {
        Controls.Add(new CameraControl(TargetEnum.Distance, MouseCodeEnum.ScrollWheel, 2));
        Controls.Add(new CameraControl(TargetEnum.Height, MouseCodeEnum.ScrollWheel, 1));
        Controls.Add(new CameraControl(TargetEnum.PanY, MouseCodeEnum.ScrollWheel, 0.5f));
        Controls.Add(new CameraControl(TargetEnum.PanX, KeyCode.LeftArrow, -1));
        Controls.Add(new CameraControl(TargetEnum.PanX, KeyCode.RightArrow, 1));
    }

    private void Awake()
    {
        if (target)
            FocusOn(target);
    }

    public void Update()
    {
        UpdateControls();

        UpdateCameraPosition();
    }

    #endregion

    #region Private Methods

    private void UpdateControls()
    {
        // Update Controls
        
        foreach (CameraControl control in controls)
        {
            // Handle Buttons
            if(control.Value != 0)
                switch (control.Target)
                {
                    case TargetEnum.Distance: Distance += control.Value; break;
                    case TargetEnum.Height: Height += control.Value; break;
                    case TargetEnum.PanX: PanX += control.Value; break;
                    case TargetEnum.PanY: PanY += control.Value; break;
                    //case TargetEnum.Pivot: Pivot += control.Value; break;
                }
        }
    }

    private void UpdateCameraPosition()
    {
        Vector3 wantedPosition = target.TransformPoint(PanX.Current, height.Current, -distance.Current);

        // Only Update When Changes are Present
        if (wantedPosition != CameraTransform.position)
        {
            wantedPosition = Bumper.UpdatePosition(target, CameraTransform, wantedPosition, Time.deltaTime * damping);

            switch(translationType)
            {
                case MovementType.Instant: CameraTransform.position = wantedPosition; break;
                case MovementType.LinearInterpolation:
                    CameraTransform.position = Vector3.Lerp(CameraTransform.position, wantedPosition, Time.deltaTime * damping);
                    break;
                case MovementType.SphericalLinearInterpolation:
                    CameraTransform.position = Vector3.Slerp(CameraTransform.position, wantedPosition, Time.deltaTime * damping);
                    break;
            }

            /// Rotate Camera
            Vector3 lookPosition = target.TransformPoint(lookAtOffset + RuntimeOffset);
            Quaternion wantedRotation = Quaternion.LookRotation(lookPosition - CameraTransform.position, target.up);

            switch (rotationType)
            {
                case MovementType.Instant:
                    CameraTransform.rotation = Quaternion.LookRotation(lookPosition - CameraTransform.position, target.up);
                    break;
                case MovementType.LinearInterpolation:
                    CameraTransform.rotation = Quaternion.Lerp(CameraTransform.rotation, wantedRotation, Time.deltaTime * rotationDamping);
                    break;
                case MovementType.SphericalLinearInterpolation:
                    CameraTransform.rotation = Quaternion.Slerp(CameraTransform.rotation, wantedRotation, Time.deltaTime * rotationDamping);
                    break;
            }
        }
    }

    #endregion

    #region Public Methods

    public static void FocusOn(Transform argTarget)
    {
        Target = argTarget;

        CameraTransform.parent = Target;

		PropsUnityObject Object = Target.GetComponent(typeof(PropsUnityObject)) as PropsUnityObject;
        if (Object)
        {
            float myYOffSet = Object.Renderer.bounds.size.y;
            LookAtOffset = Vector3.up * myYOffSet;
            Height.Current = myYOffSet + .5f;
            Distance.Current = myYOffSet + 1;
            PanY.Current = 0;
            PanX.Current = 0;
        }
    }

    public static void ResetPosition()
    {
		PropsUnityObject Object = Target.GetComponent(typeof(PropsUnityObject)) as PropsUnityObject;
        float myYOffSet = Object.Renderer.bounds.size.y;
        LookAtOffset = Vector3.up * myYOffSet;
        PanX.Current = 0;
    }

    #endregion
}

