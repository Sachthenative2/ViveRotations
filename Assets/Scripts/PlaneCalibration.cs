using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlaneCalibration : MonoBehaviour
{
    [SerializeField]
    private SteamVR_Action_Boolean trigger = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");

    [SerializeField]
    private SteamVR_Behaviour_Pose handLeft;
    [SerializeField]
    private SteamVR_Behaviour_Pose handRight;

    public Transform controller { get; protected set; }

    public List<Vector3> calibrationPoints = new List<Vector3>();
    public bool calibrated { get; protected set; }
    public Plane monitor;

    private void Awake()
    {
        StartCalibration();
    }

    private void StartCalibration()
    {
        calibrated = false;
        trigger.AddOnStateDownListener(AddCalibrationPoint, handLeft.inputSource);
        trigger.AddOnStateDownListener(AddCalibrationPoint, handRight.inputSource);
    }

    private void StopCalibration()
    {
        trigger.RemoveOnStateDownListener(AddCalibrationPoint, handLeft.inputSource);
        trigger.RemoveOnStateDownListener(AddCalibrationPoint, handRight.inputSource);
        calibrated = true;

        CreatePlane();
    }

    private void AddCalibrationPoint(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (calibrated) return;

        if (fromSource == SteamVR_Input_Sources.LeftHand)
        {
            controller = handLeft.transform;
        }
        else if (fromSource == SteamVR_Input_Sources.RightHand)
        {
            controller = handRight.transform;
        }
        else
        {
            return;
        }
        
        calibrationPoints.Add(controller.position);

        if (calibrationPoints.Count > 2)
        {
            StopCalibration();
        }
    }

    private void CreatePlane()
    {
        if (calibrationPoints.Count < 3)
        {
            Debug.LogWarning("Invalid calibration");
            return;
        }

        monitor.Set3Points(calibrationPoints[0], calibrationPoints[1], calibrationPoints[2]);
        print("Calibrated");
    }
}
