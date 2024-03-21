using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeybindType
{
    left, right, up, down,
    fire, altfire,
    boost, special1, special2, special3,
    dance1,
    resetCamOffset, resetCamZoom
}
public class InputController : MonoBehaviour
{
    public static InputController Instance { get; private set; }

    public Key left = new Key("Input_Left", KeyCode.A);
    public Key right = new Key("Input_Right", KeyCode.D);
    public Key up = new Key("Input_Up", KeyCode.W);
    public Key down = new Key("Input_Down", KeyCode.S);

    public Key fire = new Key("Input_Fire", KeyCode.Mouse0);
    public Key altfire = new Key("Input_AltFire", KeyCode.Mouse1);

    public Key boost = new Key("Input_Boost", KeyCode.LeftShift);

    public Key special1 = new Key("Input_Special1", KeyCode.Q, KeyCode.Z);
    public Key special2 = new Key("Input_Special2", KeyCode.E, KeyCode.X);
    public Key special3 = new Key("Input_Special3", KeyCode.F, KeyCode.C);

    public Key dance1 = new Key("Input_Dance1", KeyCode.Alpha1);

    public Key resetCamOffset = new Key("Input_ResetCam", KeyCode.Space);

    public Key resetCamZoom = new Key("Input_ResetZoom", KeyCode.LeftControl);

    [HideInInspector] public bool UsingTargetKeys = true;

    public static List<Key> keys { get; } = new List<Key>();

    private void Awake()
    {
        Instance = this;
    }

    public void LoadKeybinds()
    {
        foreach (Key k in keys)
            k.LoadKeybinds();
    }

    private void Update()
    {
        foreach (Key k in keys)
        {
            k.UpdateKey();
        }
    }

    public Key GetKey(KeybindType key)
    {
        switch (key)
        {
            case KeybindType.left: return left;
            case KeybindType.right: return right;
            case KeybindType.up: return up;
            case KeybindType.down: return down;
            case KeybindType.fire: return fire;
            case KeybindType.altfire: return altfire;
            case KeybindType.boost: return boost;
            case KeybindType.special1: return special1;
            case KeybindType.special2: return special2;
            case KeybindType.special3: return special3;
            case KeybindType.dance1: return dance1;
            case KeybindType.resetCamOffset: return resetCamOffset;
            case KeybindType.resetCamZoom: return resetCamZoom;
            default:
                Debug.LogError("Don't know what " + key + " corresponds to");
                return null;
        }
    }
}

public class Key
{
    private string prefName;
    public KeyCode targetKey { get; private set; }
    public KeyCode alternateKey { get; private set; } = KeyCode.None;
    public bool down { get; private set; }
    public bool held { get; private set; }
    public bool up { get; private set; }

    public bool primaryPressed { get; private set; }
    public bool alternatePressed { get; private set; }

    public Key(string prefName, KeyCode targetKey)
    {
        this.prefName = prefName;
        this.targetKey = targetKey;
        InputController.keys.Add(this);
    }

    public Key(string prefName, KeyCode targetKey, KeyCode alternateKey)
    {
        this.prefName = prefName;
        this.targetKey = targetKey;
        this.alternateKey = alternateKey;
        InputController.keys.Add(this);
    }

    public void LoadKeybinds()
    {
        targetKey = (KeyCode)PlayerPrefs.GetInt(prefName, (int)targetKey);
        alternateKey = (KeyCode)PlayerPrefs.GetInt(prefName + "_alt", (int)alternateKey);
    }

    public void SetTargetKey(KeyCode targetKey)
    {
        this.targetKey = targetKey;
        PlayerPrefs.SetInt(prefName, (int)targetKey);
    }
    public void SetAlternateKey(KeyCode alternateKey)
    {
        this.alternateKey = alternateKey;
        PlayerPrefs.SetInt(prefName + "_alt", (int)alternateKey);
    }

    public void UpdateKey()
    {
        down = Input.GetKeyDown(targetKey) || Input.GetKeyDown(alternateKey);
        held = Input.GetKey(targetKey) || Input.GetKey(alternateKey);
        up = Input.GetKeyUp(targetKey) || Input.GetKeyUp(alternateKey);

        primaryPressed = targetKey != KeyCode.None && (Input.GetKeyDown(targetKey) || Input.GetKey(targetKey) || Input.GetKeyUp(targetKey));
        alternatePressed = targetKey != KeyCode.None && (Input.GetKeyDown(alternateKey) || Input.GetKey(alternateKey) || Input.GetKeyUp(alternateKey));
    }

    public string GetKey()
    {
        switch (targetKey)
        {
            case KeyCode.Mouse2: return "M2";
            case KeyCode.Mouse3: return "M3";
            case KeyCode.Mouse4: return "M4";
            case KeyCode.Mouse5: return "M5";
            case KeyCode.Mouse6: return "M6";
            default: return targetKey.ToString();
        }
    }

    public string GetAltKey()
    {
        switch (alternateKey)
        {
            case KeyCode.Mouse2: return "M2";
            case KeyCode.Mouse3: return "M3";
            case KeyCode.Mouse4: return "M4";
            case KeyCode.Mouse5: return "M5";
            case KeyCode.Mouse6: return "M6";
            default: return alternateKey.ToString();
        }
    }

    public static implicit operator bool(Key obj)
    {
        return obj.down || obj.held || obj.up;
    }
}