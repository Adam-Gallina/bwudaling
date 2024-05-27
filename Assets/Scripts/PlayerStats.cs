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
    private float statDistCount;
    [SerializeField] private float statDistUpdateAmount = 5;

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
        {
            float d = Vector3.Distance(player.avatar.transform.position, lastPlayerPos);
            currStats.DistTravelled += d;
            statDistCount += d;

            if (statDistCount >= statDistUpdateAmount)
            {
                AchievmentController.Instance.AddStat(AchievmentController.TotalDistance, (int)statDistCount);
                statDistCount -= (int)statDistCount;
            }
        }
        lastPlayerPos = player.avatar.transform.position;
    }

    private void OnSawLeftPlayer()
    {
        if (player.avatar && !player.avatar.dead)
            currStats.Dodges++;

        AchievmentController.Instance.AddStat(AchievmentController.TotalDodges, 1);
    }

    public void AddDeath()
    {
        currStats.Deaths++;

        AchievmentController.Instance.AddStat(AchievmentController.TotalDeaths, 1);
    }

    public void AddRescue()
    {
        currStats.Rescues++;

        AchievmentController.Instance.AddStat(AchievmentController.TotalRevives, 1);
    }

    public void AddAbility()
    {
        currStats.Abilities++;

        AchievmentController.Instance.AddStat(AchievmentController.TotalAbilities, 1);
    }

    public void AddHaiw()
    {
        currStats.HaiwsCollected++;

        AchievmentController.Instance.AddStat(AchievmentController.TotalHaiws, 1);
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