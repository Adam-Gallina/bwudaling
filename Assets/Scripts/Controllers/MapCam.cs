using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCam : MonoBehaviour
{
    public static MapCam Instance;
    public static Camera Cam;

    private void Awake()
    {
        Instance = this;
        Cam = GetComponent<Camera>();
    }
}
