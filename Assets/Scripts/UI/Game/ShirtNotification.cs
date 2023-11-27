using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShirtNotification : MonoBehaviour
{
    public static ShirtNotification Instance;

    [SerializeField] private float closedOffset;
    [SerializeField] private float openOffset;

    [SerializeField] private float toggleDuration;
    [SerializeField] private float openDuration;

    [SerializeField] private Image preview;

    private RectTransform rt;

    private float y;

    private void Awake()
    {
        Instance = this;

        rt = GetComponent<RectTransform>();
        y = rt.anchoredPosition.y;
        rt.anchoredPosition = new Vector2(closedOffset, y);
    }

    public void SetNotification(Sprite image)
    {
        preview.sprite = image;
        StopAllCoroutines();
        StartCoroutine(NotificationAnim());
    }

    private bool closing = false;
    public void CloseNotification()
    {
        if (closing) return;

        StopAllCoroutines();
        StartCoroutine(CloseAnim());
    }

    private IEnumerator NotificationAnim()
    {
        closing = false;

        float start = Time.time;
        while (Time.time < start + toggleDuration)
        {
            float t = (Time.time - start) / toggleDuration;
            rt.anchoredPosition = new Vector2(closedOffset + (openOffset - closedOffset) * t, y);

            yield return new WaitForEndOfFrame();
        }

        rt.anchoredPosition = new Vector2(openOffset, y);

        yield return new WaitForSeconds(openDuration);

        yield return CloseAnim();
    }

    private IEnumerator CloseAnim()
    {
        closing = true;

        float start = Time.time;
        while (Time.time < start + toggleDuration)
        {
            float t = (Time.time - start) / toggleDuration;
            rt.anchoredPosition = new Vector2(openOffset + (closedOffset - openOffset) * t, y);

            yield return new WaitForEndOfFrame();
        }

        rt.anchoredPosition = new Vector2(closedOffset, y);

        closing = false;
    }
}
