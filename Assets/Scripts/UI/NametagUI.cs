using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NametagUI : MonoBehaviour
{
    [HideInInspector] public NetworkPlayer LinkedPlayer;

    [SerializeField] protected TMPro.TMP_Text playerNameText;

    [SerializeField] protected Vector3 offset = new Vector3(0, 2, 0);

    private RectTransform rt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public virtual void SetLinkedPlayer(NetworkPlayer player)
    {
        LinkedPlayer = player;

        playerNameText.text = player.displayName;
    }

    protected virtual void Update()
    {
        if (LinkedPlayer.avatar)
            transform.position = Camera.main.WorldToScreenPoint(LinkedPlayer.avatar.NametagTarget.position) + offset;
    }
}
