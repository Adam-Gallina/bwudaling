using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NametagUI : MonoBehaviour
{
    [HideInInspector] public NetworkPlayer LinkedPlayer;

    [SerializeField] protected TMPro.TMP_Text playerNameText;
    [SerializeField] protected Image playerColorImg;

    [SerializeField] protected Vector3 offsetDir = new Vector3(0, 1, 0);
    [SerializeField] protected RangeF zoomRange;
    [SerializeField] protected RangeF zoomSize;
    [SerializeField] protected RangeF zoomOffset;

    private RectTransform rt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public virtual void SetLinkedPlayer(NetworkPlayer player)
    {
        LinkedPlayer = player;

        playerNameText.text = player.displayName;
        playerColorImg.color = player.avatarColor;
    }

    protected virtual void Update()
    {
        if (LinkedPlayer.avatar && CameraController.Instance)
        {
            float t = Mathf.Clamp01(zoomRange.PercentOfRange(CameraController.Instance.ZoomLevel));

            float scale = zoomSize.PercentVal(t);
            rt.localScale = new Vector3(scale, scale, scale);

            float offset = zoomOffset.PercentVal(t);
            transform.position = Camera.main.WorldToScreenPoint(LinkedPlayer.avatar.NametagTarget.position) + offsetDir * offset;
        }
    }
}
