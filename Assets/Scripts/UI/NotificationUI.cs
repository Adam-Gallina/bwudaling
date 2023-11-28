using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationUI : MonoBehaviour
{
    public static NotificationUI Instance;

    [SerializeField] private float notificationDuration = 5;

    private List<string> currText = new List<string>();
    private List<float> textTime = new List<float>();

    [SerializeField] private GameObject textObj;
    [SerializeField] private TMPro.TMP_Text text;
    [SerializeField] private TMPro.TMP_Text textBkgd;

    private void Awake()
    {
        Instance = this;

        UpdateText();
    }

    private void Update()
    {
        if (textTime.Count > 0 && Time.time > textTime[0])
        {
            currText.RemoveAt(0);
            textTime.RemoveAt(0);
            UpdateText();
        }
    }

    public void AddNotification(string msg)
    {
        currText.Add(msg);
        textTime.Add(Time.time + notificationDuration);
        UpdateText();
    }

    private void UpdateText()
    {
        textObj.SetActive(textTime.Count > 0);
        string t = "";

        foreach (string s in currText)
            t += s + "\n";

        text.text = t;
        textBkgd.text = t;
    }
}
