using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    private bool hovered = false;

    private RectTransform rt;
    private RectTransform prt;
    private RectTransform crt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        prt = transform.parent.GetComponent<RectTransform>();
    }

    private void Start()
    {
        crt = GameUI.Instance.GetComponent<RectTransform>();
    }

    public void HoverStart() { hovered = true; }
    public void HoverStop() { hovered = false; }

    public void UpdateCam()
    {
        if (!hovered) return;

        Vector2 mousePos = Input.mousePosition;

        Vector2 scaledMousePos = new Vector2(
            (mousePos.x - rt.position.x) / (rt.rect.size.x * crt.localScale.x),
            (mousePos.y - rt.position.y) / (rt.rect.size.y * crt.localScale.y));
        scaledMousePos *= 2; // [-.5, .5] > [-1, 1]

        Vector3 mapPos = MapCam.Instance.transform.position + new Vector3(scaledMousePos.x, 0, scaledMousePos.y) * MapCam.Cam.orthographicSize;
        mapPos.y = 0;

        CameraController.Instance.FocusOnPoint(mapPos);
    }
}
