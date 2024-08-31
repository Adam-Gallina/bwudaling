using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class MapController : MonoBehaviour
{
    public static MapController Instance;

    public GameController gameControllerPrefab;

    [SerializeField] private Transform[] spawnPositions = new Transform[0];

    public GameObject startWall;

    [Header("Map Rules")]
    public int safeZoneXp = 5;
    public int endZoneXp = 20;
    public int winXp = 10;
    public float hazardSpeed = 5;
    public float hazardRange = 150;
    public Vector3 mapCenter;
    public bool canControlAvatars = true;
    public int maxFriendlyReward = 3;
    public float friendlyXpMod = .2f;
    public bool resetTimerOnStart = false;

    private void OnDrawGizmos()
    {
        if (spawnPositions.Length > 0)
        {
            foreach (Transform t in spawnPositions)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(t.position, 1);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(t.position, t.position + t.forward * 2);
            }
        }
    }

    private void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex != Constants.MainMenu.buildIndex && !BwudalingNetworkManager.Instance)
            SceneManager.LoadScene(Constants.MainMenu.buildIndex);

        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public Transform GetSpawnPos(int player)
    {
        if (player < 0 || player >= spawnPositions.Length)
        {
            Debug.LogWarning($"Missing spawn point {player}");
            return null;
        }

        return spawnPositions[player];
    }

    
    public NetworkEnemySpawnPos[] GetMapEnemies()
    {
        return GetComponentsInChildren<NetworkEnemySpawnPos>();
    }
}
