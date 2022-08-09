using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using Valve.VR;

public class MouseEmulator : MonoBehaviour
{
    [SerializeField]
    private SteamVR_Action_Boolean leftClick = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");

    public PlaneCalibration planeCalibration;
    public Transform cursor3D;

    private Vector2 cursorPos;

    [SerializeField]
    private float smoothing = .01f;
    [SerializeField]
    private float timeToDrag = .5f;
    private IEnumerator dragCoroutine;
    private bool canMove = true;

    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, UIntPtr dwExtraInfo);
    private const uint MOUSE_LEFTDOWN = 0x02;
    private const uint MOUSE_LEFTUP = 0x04;
    private const uint MOUSE_RIGHTDOWN = 0x08;
    private const uint MOUSE_RIGHTUP = 0x10;

    private void Awake()
    {
        leftClick.AddOnStateDownListener(LeftClickDown, SteamVR_Input_Sources.Any);
        leftClick.AddOnStateUpListener(LeftClickUp, SteamVR_Input_Sources.Any);
    }

    private void Update()
    {
        if (planeCalibration.calibrated)
        {
            Ray ray = new Ray(planeCalibration.controller.position, planeCalibration.controller.forward);
            float enter = 0f;
            if (planeCalibration.monitor.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                cursor3D.position = hitPoint;

                if (canMove)
                {
                    Vector2 newPos = Vector2.zero;
                    Vector3 pointX = GetClosestPointOnFiniteLine(hitPoint, planeCalibration.calibrationPoints[0], planeCalibration.calibrationPoints[1]);
                    float percentageX = Vector3.Distance(planeCalibration.calibrationPoints[0], pointX) / Vector3.Distance(planeCalibration.calibrationPoints[0], planeCalibration.calibrationPoints[1]);
                    newPos.x = percentageX * Screen.currentResolution.width;

                    Vector3 pointY = GetClosestPointOnFiniteLine(hitPoint, planeCalibration.calibrationPoints[1], planeCalibration.calibrationPoints[2]);
                    float percentageY = Vector3.Distance(planeCalibration.calibrationPoints[1], pointY) / Vector3.Distance(planeCalibration.calibrationPoints[1], planeCalibration.calibrationPoints[2]);
                    newPos.y = percentageY * Screen.currentResolution.height;

                    cursorPos = Vector2.Lerp(cursorPos, newPos, smoothing);
                }

                MoveCursor(cursorPos);
            }
        }
    }

    // mouse button mapping /////////////////////////////////////////////////////////////////
    private void LeftClickDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        MouseClick(MOUSE_LEFTDOWN);
        WaitForDrag();
    }

    private void LeftClickUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        MouseClick(MOUSE_LEFTUP);
    }

    // helper functions ////////////////////////////////////////////////////////////////////////////////////////////////
    private Vector3 GetClosestPointOnFiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
    {
        Vector3 line_direction = line_end - line_start;
        float line_length = line_direction.magnitude;
        line_direction.Normalize();
        float project_length = Mathf.Clamp(Vector3.Dot(point - line_start, line_direction), 0f, line_length);
        return line_start + line_direction * project_length;
    }

    // cursor functions ////////////////////////////////////////////////////////////////////
    private void MoveCursor(Vector2 pos)
    {
        SetCursorPos((int)pos.x, (int)pos.y);
    }

    private void MouseClick(Vector2 pos, uint clickType)
    {
        mouse_event(clickType, (uint)pos.x, (uint)pos.y, 0, (UIntPtr)0);
    }

    private void MouseClick(uint clickType)
    {
        if (!planeCalibration.calibrated) return;
        MouseClick(cursorPos, clickType);
    }

    private void WaitForDrag()
    {
        if (dragCoroutine != null)
        {
            StopCoroutine(dragCoroutine);
            dragCoroutine = null;
        }
        dragCoroutine = DragCoroutine();
        StartCoroutine(dragCoroutine);
    }

    private IEnumerator DragCoroutine()
    {
        canMove = false;
        float endTime = Time.time + timeToDrag;

        while (true)
        {
            if (Time.time > endTime)
            {
                canMove = true;
            }
            yield return null;
        }
    }
}
