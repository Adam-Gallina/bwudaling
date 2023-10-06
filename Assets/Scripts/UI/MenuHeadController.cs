using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHeadController : MonoBehaviour
{
    public static MenuHeadController Instance;

    [SerializeField] private Transform[] headRotOrigins = new Transform[0];
    [SerializeField] private Vector3 mouseOffset;

    [SerializeField] private float toggleOffset;

    private Camera cam;

    private void Awake()
    {
        Instance = this;

        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (headRotOrigins.Length > 0)
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition) + mouseOffset;
            foreach (Transform t in headRotOrigins)
            {
                t.forward = mousePos - t.position;
            }
        }
    }

    public void HoverHead(int head)
    {
        transform.GetChild(head).GetComponent<Animator>().SetBool("Up", true);
    }

    public void UnhoverHead(int head)
    {
        transform.GetChild(head).GetComponent<Animator>().SetBool("Up", false);
    }

    public void SelectHead(int head, Action callback)
    {
        transform.GetChild(head).GetComponent<Animator>().SetTrigger("Smack");
        HideHeads(head, callback);
    }

    public void HideHeads(int start, Action callback)
    {
        StartCoroutine(DoHideHeads(start, true, callback));
    }

    public void ShowHeads(int start)
    {
        StartCoroutine(DoHideHeads(start, false, null));
    }

    private IEnumerator DoHideHeads(int start, bool hide, Action callback)
    {
        transform.GetChild(start).GetComponent<Animator>().SetBool("Hide", hide);
        float o = toggleOffset;
        if (hide) o *= 2;
        yield return new WaitForSeconds(o);

        for (int i = 1; i < 4; i++)
        {
            if (start - i < 0 && start + i >= transform.childCount)
                break;

            if (start + i < transform.childCount)
                transform.GetChild(start + i).GetComponent<Animator>().SetBool("Hide", hide);
            if (start - i >= 0)
                transform.GetChild(start - i).GetComponent<Animator>().SetBool("Hide", hide);

            yield return new WaitForSeconds(toggleOffset);
        }

        callback?.Invoke();
    }
}
