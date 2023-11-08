using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    private int targetImportance = 0;
    private Transform target;

    [Header("Tracking")]
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    [SerializeField] private float moveSpeed;

    [Header("Panning")]
    [SerializeField] private float maxCursorPercent = 0.1f;
    private float maxCursorDist;
    [SerializeField] private float maxPanSpeed = 2;
    private Vector3 panOffset;
    private Vector3 targetPanOffset;
    [SerializeField] private float panSmoothingSpeed = 10;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed;
    private float zoomLevel;
    public float ZoomLevel { get { return zoomLevel; } }

    private void Awake()
    {
        Instance = this;

        maxCursorDist = Camera.main.pixelWidth * maxCursorPercent;
    }

    private void Update()
    {
        if (panOffset != targetPanOffset)
        {
            if (panSmoothingSpeed > 0)
                panOffset = Vector3.MoveTowards(panOffset, targetPanOffset, panSmoothingSpeed * Time.deltaTime);
            else
                panOffset = targetPanOffset;
        }

        if (!target)
            return;

        CheckInput();

        if (Input.GetKeyDown(KeyCode.Space))
            panOffset = targetPanOffset = Vector3.zero;

        zoomLevel -= Input.mouseScrollDelta.y * zoomSpeed;
        if (Input.GetKeyDown(KeyCode.LeftControl))
            zoomLevel = 0;

        transform.position = target.position + panOffset - Camera.main.transform.forward * zoomLevel;
    }

    private void CheckInput()
    {
        if (IsPointerOverUIObject())
            return;

        if (Input.mousePosition.x < 0 || Input.mousePosition.x > Camera.main.pixelWidth ||
            Input.mousePosition.y < 0 || Input.mousePosition.y > Camera.main.pixelHeight)
            return;

        float xt = 0, yt = 0;
        if (Input.mousePosition.x <= maxCursorDist && Input.mousePosition.x >= 0)
            xt = -(maxCursorDist - Input.mousePosition.x) / maxCursorDist;
        else if (Input.mousePosition.x >= Camera.main.pixelWidth - maxCursorDist && Input.mousePosition.x <= Camera.main.pixelWidth)
            xt = (maxCursorDist - (Camera.main.pixelWidth - Input.mousePosition.x)) / maxCursorDist;

        if (Input.mousePosition.y <= maxCursorDist && Input.mousePosition.y >= 0)
            yt = -(maxCursorDist - Input.mousePosition.y) / maxCursorDist;
        else if (Input.mousePosition.y >= Camera.main.pixelHeight - maxCursorDist && Input.mousePosition.y <= Camera.main.pixelHeight)
            yt = (maxCursorDist - (Camera.main.pixelHeight - Input.mousePosition.y)) / maxCursorDist;

        panOffset += new Vector3(xt * maxPanSpeed, 0, yt * maxPanSpeed) * Time.deltaTime;
        targetPanOffset += new Vector3(xt * maxPanSpeed, 0, yt * maxPanSpeed) * Time.deltaTime;

    }

    public void FocusOnPoint(Vector3 point)
    {
        targetPanOffset = point - target.position;
    }

    public void SetTarget(Transform t, int importance = 0)
    {
        if (targetImportance > importance)
            return;

        target = t;
        targetImportance = importance;
        panOffset = Vector3.zero;
    }

    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        foreach (RaycastResult r in results)
        {
            if (r.gameObject.layer == 5)
                return true;
        }
        return false;
    }

    public void SetZoom(float zoom)
    {
        zoomLevel = zoom;
    }
}
