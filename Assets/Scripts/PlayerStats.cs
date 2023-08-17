using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStatType { None, Distance, Deaths, Heals, Dodges, Abilities, Haiws, Best }
public class PlayerStats : MonoBehaviour
{
    private NetworkPlayer player;

    public PlayerStatValues currStats = new PlayerStatValues();

    //[Header("Dist")]
    private Vector3 lastPlayerPos;

    [Header("Dodges")]
    [SerializeField] private float pDodgeDist = 1.5f;
    private List<Collider> closeSaws = new List<Collider>();

    private void Awake()
    {
        player = GetComponent<NetworkPlayer>();
        currStats.updated = true;
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
            currStats.DistTravelled += Vector3.Distance(player.avatar.transform.position, lastPlayerPos);
        lastPlayerPos = player.avatar.transform.position;
    }

    private void OnSawLeftPlayer()
    {
        if (player.avatar && !player.avatar.dead)
            currStats.Dodges++;
    }

    public void AddDeath()
    {
        currStats.Deaths++;
    }

    public void AddRescue()
    {
        currStats.Rescues++;
    }

    public void AddAbility()
    {
        currStats.Abilities++;
    }

    public void AddHaiw()
    {
        currStats.HaiwsCollected++;
    }
}

[System.Serializable]
public struct PlayerStatValues
{
    private Dictionary<PlayerStatType, int> statVals;
    public Dictionary<PlayerStatType, int> StatVals
    {
        get { 
            if (statVals == null)
            {
                statVals = new Dictionary<PlayerStatType, int>
                {
                    { PlayerStatType.Distance, (int)DistTravelled },
                    { PlayerStatType.Deaths, Deaths},
                    { PlayerStatType.Heals, Rescues},
                    { PlayerStatType.Dodges, Dodges},
                    { PlayerStatType.Abilities, Abilities},
                    { PlayerStatType.Haiws, HaiwsCollected}
                };
            }
            return statVals; 
        }
    }

    [HideInInspector] public bool updated;
    public float DistTravelled;
    public int Deaths;
    public int Rescues;
    public int Dodges;
    public int Abilities;
    public int HaiwsCollected;
}