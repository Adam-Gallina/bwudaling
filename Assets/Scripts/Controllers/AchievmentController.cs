using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievmentController : MonoBehaviour
{
    public static AchievmentController Instance { get; private set; }

    public static string DefaultShirtId = "bw";

    //[SerializeField] private string[] startingShirtIds = new string[] { DefaultShirtId };

    [SerializeField] private ShirtData[] shirts = new ShirtData[0];
    public static Dictionary<string, ShirtData> Shirts = new Dictionary<string, ShirtData>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (ShirtData s in shirts)
        {
            if (Shirts.ContainsKey(s.id))
                Debug.LogError($"Duplicate shirt id {s.id}, skipping {s.name}");
            else
                Shirts.Add(s.id, s);
        }
    }

    public string[] GetUnlockedShirts()
    {
        List<string> ids = new List<string>();
        foreach (string s in Shirts.Keys)
            ids.Add(s);

        return ids.ToArray();
    }
}

[System.Serializable]
public struct ShirtData
{
    public string name;
    public string id;
    public Material mat;
}