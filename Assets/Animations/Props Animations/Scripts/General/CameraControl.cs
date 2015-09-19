using UnityEngine;
using System;

public enum TargetEnum { Height, Distance, PanX, PanY, Pivot }
public enum MouseCodeEnum { None, ScrollWheel, X, Y }

[Serializable]
public class CameraControl
{
    #region Properties 

    [SerializeField] private TargetEnum target = TargetEnum.Distance;
    public TargetEnum Target
    {
        get { return target; }
        set { target = value; }
    }

    [SerializeField] private float stepSize = 1.0f;
    public float StepSize
    {
        get { return this.stepSize; }
        set { this.stepSize = value; }
    }

    #region Input Controls

    [SerializeField] private MouseCodeEnum mouseCode = MouseCodeEnum.None;
    public MouseCodeEnum MouseCode
    {
        get { return mouseCode; }
        set { mouseCode = value; }
    }

    [SerializeField] private KeyCode keyCode = KeyCode.None;
    public KeyCode KeyCode
    {
        get { return keyCode; }
        set { keyCode = value; }
    }

    #endregion

    #region Input Checks

    public bool IsPressed
    {
        get { return Input.GetKey(keyCode); }
    }

    public float Value
    {
        get
        {
            // Check Axis
            float value = 0;
            switch (mouseCode)
            {
                case MouseCodeEnum.ScrollWheel: value = -Input.GetAxis("Mouse ScrollWheel") * StepSize * 100 * Time.deltaTime; break;
                case MouseCodeEnum.X: value = Input.GetAxis("Mouse X") * StepSize * 100 * Time.deltaTime; break;
                case MouseCodeEnum.Y: value = Input.GetAxis("Mouse Y") * StepSize * 100 * Time.deltaTime; break;
            }

            // Check Button Buttons
            if( value == 0 && IsPressed)
                value = StepSize * Time.deltaTime;

            return value;
        }
    }

    #endregion

    #endregion

    #region Constructor

    public CameraControl()
    {
        stepSize = 1;
    }

    public CameraControl(TargetEnum argTarget, KeyCode argKeyCode, float argStepSize)
    {
        target = argTarget;
        keyCode = argKeyCode;
        stepSize = argStepSize;
    }

    public CameraControl(TargetEnum argTarget, MouseCodeEnum argMouseCode, float argStepSize)
    {
        target = argTarget;
        mouseCode = argMouseCode;
        stepSize = argStepSize;
    }

    public CameraControl(TargetEnum argTarget, KeyCode argKeyCode, MouseCodeEnum argMouseCode, float argStepSize)
    {
        target = argTarget;
        keyCode = argKeyCode;
        mouseCode = argMouseCode;
        stepSize = argStepSize;
    }

    #endregion


}
