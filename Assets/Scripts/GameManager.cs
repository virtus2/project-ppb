using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameUI GameUI;

    [SerializeField]
    private List<GameObject> spawnPositions;
    private HashSet<Vector3> randomPositions = new HashSet<Vector3>();

    public int GameTime = 60;

    private void Awake()
    {
        SplatManagerSystem.instance.Clear();

        var gameInstance = GameInstance.Instance;
        gameInstance.OnGameSceneCreated(this);
        
        var playerInstance = gameInstance.GetLocalPlayerInstance();
        playerInstance.PlayerHUD.HideResurrectUI();


        if (!GameUI)
        {
            GameUI = FindObjectOfType<GameUI>();
        }
    }

    private void Start()
    {
        var gameInstance = GameInstance.Instance;
        var photon = gameInstance.PhotonManager;
        var Runner = photon.NetworkRunner;
        if(Runner.IsServer)
        {
            var serverPlayerInstance = GameInstance.Instance.GetLocalPlayerInstance();
        }
        var playerInstance = gameInstance.GetLocalPlayerInstance();
        playerInstance.RPC_GameSceneLoadCompleted();
    }

    public void ShowGameCountdownUI()
    {
        GameUI.ShowGameCountdownUI();
    }

    public void ShowGameTimerUI()
    {
        GameUI.ShowGameTimerUI();
    }

    public void ShowGameOverUI(bool victory)
    {
        GameUI.ShowGameOverUI(victory);
    }

    public List<Vector3> GetRandomSpawnPositions(int count)
    {
        randomPositions.Clear();
        while (randomPositions.Count < count)
        {
            var randomIndex = Random.Range(0, spawnPositions.Count);
            var randomPosition = spawnPositions[randomIndex].transform.position;
            if (randomPositions.Contains(randomPosition))
            {
                continue;
            }
            randomPositions.Add(randomPosition);
        }

        return randomPositions.ToList();
    }

    public void AddKillLog(KillLogInformation log)
    {
        GameUI.AddKillLog(log);
    }
}
