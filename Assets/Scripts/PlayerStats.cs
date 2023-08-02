using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private NetworkPlayer player;

    //[Header("Dist")]
    private Vector3 lastPlayerPos;

    [Header("Dodges")]
    [SerializeField] private float pDodgeDist = 1.5f;
    private List<Collider> closeSaws = new List<Collider>();

    private void Awake()
    {
        player = GetComponent<NetworkPlayer>();
    }

    private void Update()
    {
        if (!player.hasAuthority) return;
        if (!player.avatar) return;


        Collider[] colls = Physics.OverlapSphere(player.avatar.transform.position, pDodgeDist, 1 << Constants.HazardLayer);
        bool[] checkSaws = new bool[closeSaws.Count];
        foreach (Collider coll in colls)
        {
            if (!closeSaws.Contains(coll))
                closeSaws.Add(coll);
            else
                checkSaws[closeSaws.IndexOf(coll)] = true;
        }
        for (int i = checkSaws.Length - 1; i >= 0; i--)
        {
            if (!checkSaws[i])
            {
                closeSaws.RemoveAt(i);
                OnSawLeftPlayer();
            }
        }

        if (lastPlayerPos != Vector3.zero)
            player.currPlayerStats.DistTravelled += Vector3.Distance(player.avatar.transform.position, lastPlayerPos);
        lastPlayerPos = player.avatar.transform.position;
    }

    private void OnSawLeftPlayer()
    {
        if (player.avatar && !player.avatar.dead)
            player.currPlayerStats.Dodges++;
    }

    public void AddDeath()
    {
        player.currPlayerStats.Deaths++;
    }

    public void AddRescue()
    {
        player.currPlayerStats.Rescues++;
    }

    public void AddAbility()
    {
        player.currPlayerStats.Abilities++;
    }

    public void AddHaiw()
    {
        player.currPlayerStats.HaiwsCollected++;
    }
}

[System.Serializable]
public struct PlayerStatValues
{
    public float DistTravelled;
    public int Deaths;
    public int Rescues;
    public int Dodges;
    public int Abilities;
    public int HaiwsCollected;
}