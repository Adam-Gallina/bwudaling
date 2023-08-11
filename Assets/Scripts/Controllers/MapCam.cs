using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCam : MonoBehaviour
{
    public static MapCam Instance;
    public static Camera Cam;
    public float size = 75;

    private void Awake()
    {
        Instance = this;
        Cam = GetComponent<Camera>();
    }
}
