using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    private bool hovered = false;

    private RectTransform rt;
    private RectTransform prt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        prt = transform.parent.GetComponent<RectTransform>();
    }

    public void HoverStart() { hovered = true; }
    public void HoverStop() { hovered = false; }

    public void UpdateCam()
    {
        if (!hovered) return;

        Vector2 mousePos = Input.mousePosition;

        Vector2 scaledMousePos = new Vector2(
            (mousePos.x - rt.position.x) / prt.sizeDelta.x,
            (mousePos.y - rt.position.y) / prt.sizeDelta.y);

        Vector3 mapPos = MapCam.Instance.transform.position + new Vector3(scaledMousePos.x, 0, scaledMousePos.y) * MapCam.Instance.size;
        mapPos.y = 0;

        CameraController.Instance.FocusOnPoint(mapPos);
    }
}
