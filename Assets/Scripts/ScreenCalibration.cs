using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Glass;
using System.Runtime.InteropServices;


public class ScreenCalibration : MonoBehaviour
{

    public SteamVR_Action_Boolean click = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");
    private List<Vector3> calibrationPoints = new List<Vector3>();
    public List<SteamVR_Behaviour_Pose> controllers = new List<SteamVR_Behaviour_Pose>();
    private Plane screen = new Plane();
    private bool calibrated = false;
    public Transform cursor;
    public Transform plane;
    public SteamVR_Behaviour_Pose currentController;
    public RectTransform cursorContainer;
    public RectTransform cursor2D;
    public float smoothing = 0.1f;
    public Transform cameraRig;
    public float sensitivity = 1f;
    public float sensitivityZoom = 10f;
    private Vector2 lastCursorPos;
    private bool canMoveCamera = false;
    private IEnumerator velocityCoroutine;
    private Quaternion lastRotation;
    public float velocityMultiplier = 10f;
    private float lastDistance;
    private Vector2 cursorPos;
    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, UIntPtr dwExtraInfo);
    private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
    private const uint MOUSEEVENTF_LEFTUP = 0x04;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
    private const uint MOUSEEVENTF_RIGHTUP = 0x10;


    void Start()
    {

        foreach (SteamVR_Behaviour_Pose controller in controllers)
        {
            click.AddOnStateDownListener(CalibrationClick, controller.inputSource);
        }
        print(Screen.currentResolution);
        SetCursorPos(Screen.currentResolution.width, Screen.currentResolution.height);


    }

    void CalibrationClick(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (calibrated)
        {
            return;
        }
        if (calibrationPoints.Count < 4)
        {
            foreach (SteamVR_Behaviour_Pose controller in controllers)
            {
                if (controller.inputSource == fromSource)
                {
                    calibrationPoints.Add(controller.transform.localPosition);
                    currentController = controller;
                    break;
                }
            }

        }
        if (calibrationPoints.Count > 3)
        {
            SetScreen();
            
            plane.localPosition = (calibrationPoints[0] + calibrationPoints[2]) / 2;
            Vector3 local0 = plane.InverseTransformPoint(cameraRig.TransformPoint(calibrationPoints[0]));
            Vector3 local1 = plane.InverseTransformPoint(cameraRig.TransformPoint(calibrationPoints[1]));
            Vector3 local2 = plane.InverseTransformPoint(cameraRig.TransformPoint(calibrationPoints[2]));
            plane.localScale = new Vector3(local1.x - local0.x, local1.y - local2.y, 1f);
            Vector3 direction = Vector3.forward;
            if (screen.GetSide(calibrationPoints[3]))
            {
                direction *= -1;
            }
            plane.rotation = Quaternion.FromToRotation(direction, screen.normal);
            print("Calibrated");
            //calibrationPoints = new List<Vector3>();
            calibrated = true;
            /*foreach (SteamVR_Behaviour_Pose controller in controllers)
            {
                click.RemoveOnStateDownListener(CalibrationClick, controller.inputSource);
                
            }*/

        }
    }

    void SetScreen()
    {
        screen.Set3Points(cameraRig.TransformPoint(calibrationPoints[0]), cameraRig.TransformPoint(calibrationPoints[1]), cameraRig.TransformPoint(calibrationPoints[2]));
    }
    /*void CameraClick()
    {
        cameraRig.eulerAngles += new Vector3(0, (cursor2D.anchoredPosition - lastCursorPos).x * sensitivity, (cursor2D.anchoredPosition - lastCursorPos).y * sensitivity);
        float currentDistance = Vector3.Distance(plane.localPosition, currentController.transform.localPosition);
        float newScale = (currentDistance - lastDistance) * sensitivityZoom;
        cameraRig.localScale -= new Vector3(newScale, newScale, newScale);
        lastDistance = currentDistance;
    }
    
    void StartVelocity()
    {
        if (velocityCoroutine != null)
        {
            StopCoroutine(velocityCoroutine);
        }
        velocityCoroutine = DoVelocity(2f);
        StartCoroutine(velocityCoroutine);
    }
    IEnumerator DoVelocity(float duration)
    {
        float startTime = Time.time;
        Quaternion startRot = lastRotation;
        Quaternion endRot = cameraRig.rotation;
        while (true)
        {
            float percentage = (Time.time - startTime) / duration;
            if (percentage > 1f) percentage = 1f;
            // float easing = Easings.Ease(percentage, Easings.Easing.Expo, Easings.EasingDirection.Out, false);
            // print(easing);
            print(percentage);
            // cameraRig.rotation *= (startRot * Quaternion.Inverse(endRot)) * (1f - percentage);
            if (percentage > 1f) break;
            yield return null;
            
        }
        
    }
    */
    Vector3 GetClosestPointOnFiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
    {
        Vector3 line_direction = line_end - line_start;
        float line_length = line_direction.magnitude;
        line_direction.Normalize();
        float project_length = Mathf.Clamp(Vector3.Dot(point - line_start, line_direction), 0f, line_length);
        return line_start + line_direction * project_length;
    }
    private void MoveCursor(Vector2 pos)
    {
        SetCursorPos((int) pos.x, (int)pos.y);

    }
    private void Update()
    {
        if (calibrated)
        {
            SetScreen();
            Ray ray = new Ray(currentController.transform.position, currentController.transform.forward);
            float enter = 0f;
            if (screen.Raycast(ray, out enter))
            {

                lastCursorPos = cursor2D.anchoredPosition;
                Vector3 hitPoint = ray.GetPoint(enter);
                cursor.position = hitPoint;
                Vector3 closestX = GetClosestPointOnFiniteLine(hitPoint, calibrationPoints[0], calibrationPoints[1]);
                float percentageX = Vector3.Distance(closestX, calibrationPoints[0])/Vector3.Distance(calibrationPoints[0], calibrationPoints[1]);
                Vector3 closestY = GetClosestPointOnFiniteLine(hitPoint, calibrationPoints[1], calibrationPoints[2]);
                float percentageY = Vector3.Distance(closestY, calibrationPoints[1]) / Vector3.Distance(calibrationPoints[1], calibrationPoints[2]);
                cursorPos = new Vector2((int)(percentageX * Screen.currentResolution.width), (int)(percentageY * Screen.currentResolution.height));
                MoveCursor(cursorPos);
               
                //cursor2D.anchoredPosition = Vector2.Lerp(cursor2D.anchoredPosition, whatever, smoothing);

            }
            foreach (SteamVR_Behaviour_Pose controller in controllers)
            {

                click.AddOnStateDownListener(LeftClickDown, SteamVR_Input_Sources.Any);
                click.AddOnStateUpListener(LeftClickUp, SteamVR_Input_Sources.Any);

            }
            
            /*if (click.GetState(currentController.inputSource))
            {
                SendMouseDown(cursorPos);
            }
            else
            {
                SendMouseUp(cursorPos);
            }*/
            /*if (click.GetState(currentController.inputSource))
            {
                if (canMoveCamera)
                {
                    if (velocityCoroutine != null) StopCoroutine(velocityCoroutine);
                    CameraClick();
                }
                canMoveCamera = true;
            }
            else
            {
                if (canMoveCamera)
                {
                    StartVelocity();
                }
                canMoveCamera = false;
            }*/
            ///lastRotation = cameraRig.rotation;
        }
    }
    void SendMouseRightclick(Vector2 p)
    {
        mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint)p.x, (uint)p.y, 0, (UIntPtr)0);
    }

    /*void SendMouseDoubleClick(Vector2 p)
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, p.x, p.Y, 0, 0);

        Thread.Sleep(150);

        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, p.x, p.Y, 0, 0);
    }

    void SendMouseRightDoubleClick(Vector2 p)
    {
        mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, p.x, p.Y, 0, 0);

        Thread.Sleep(150);

        mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, p.x, p.Y, 0, 0);
    }*/

    private void LeftClickDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        SendMouseDown(cursorPos);
    }

    private void LeftClickUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        SendMouseUp(cursorPos);
    }

    void SendMouseDown(Vector2 pos)
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)pos.x, (uint)pos.y, 0, (UIntPtr)0);
    }

    void SendMouseUp(Vector2 pos)
    {
        mouse_event(MOUSEEVENTF_LEFTUP, (uint)pos.x, (uint)pos.y, 0, (UIntPtr)0);
    }
}
