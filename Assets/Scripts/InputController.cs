using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public static InputController Instance { get; private set; }

    public Key left = "A";
    public Key right = "D";
    public Key up = "W";
    public Key down = "S";

    public Key fire = KeyCode.Mouse0;
    public Key altfire = KeyCode.Mouse1;

    public Key boost = KeyCode.LeftShift;

    public Key special1 = new Key("Q", "Z");
    public Key special2 = new Key("E", "X");
    public Key special3 = new Key("F", "C");

    public Key dance1 = new Key(KeyCode.Alpha1);

    public Key pause = KeyCode.Escape;

    [HideInInspector] public bool UsingTargetKeys = true;

    public static List<Key> keys { get; } = new List<Key>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        foreach (Key k in keys)
        {
            k.UpdateKey();
        }
    }
}

public class Key
{
    private KeyCode targetKey;
    private KeyCode alternateKey;
    public bool down { get; private set; }
    public bool held { get; private set; }
    public bool up { get; private set; }

    public Key(KeyCode targetKey)
    {
        this.targetKey = targetKey;
        InputController.keys.Add(this);
    }

    public Key(string targetKey, string altKey)
    {
        this.targetKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), targetKey);
        this.alternateKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), altKey);
        InputController.keys.Add(this);
    }

    public Key(KeyCode targetKey, KeyCode alternateKey)
    {
        this.targetKey = targetKey;
        this.alternateKey = alternateKey;
    }

    public void UpdateKey()
    {
        down = Input.GetKeyDown(targetKey) || Input.GetKeyDown(alternateKey);
        held = Input.GetKey(targetKey) || Input.GetKey(alternateKey);
        up = Input.GetKeyUp(targetKey) || Input.GetKeyUp(alternateKey);

        if (alternateKey != KeyCode.None && (down || held || up))
            InputController.Instance.UsingTargetKeys = Input.GetKey(targetKey) || Input.GetKeyUp(targetKey);
    }

    public string GetKey()
    {
        return targetKey.ToString();
    }

    public string GetAltKey()
    {
        return alternateKey.ToString();
    }

    public static implicit operator bool(Key obj)
    {
        return obj.down || obj.held || obj.up;
    }

    public static implicit operator Key(string key)
    {
        KeyCode newKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), key);
        return new Key(newKey);
    }

    public static implicit operator Key(KeyCode key)
    {
        return new Key(key);
    }
}