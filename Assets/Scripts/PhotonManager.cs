using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkRunner NetworkRunner { get; private set; }
    public NetworkGameState NetworkGameState { get; set; }
    private NetworkSceneManagerDefault networkSceneManager;
    private NetworkPlayerInput playerInput;
    private PlayerSpawner playerSpawner;

    [SerializeField] 
    private NetworkPrefabRef NetworkGameStatePrefab;

    public int GetTeamPlayerCount(int teamNumber)
    {
        int count = 0;
        var players = NetworkRunner.ActivePlayers;
        foreach(var player in players)
        {
            if(NetworkRunner.GetPlayerObject(player).GetComponent<PlayerInstance>().TeamNumber == teamNumber)
            {
                count++;
            }
        }
        return count;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        NetworkRunner = GetComponent<NetworkRunner>();
        NetworkRunner.ProvideInput = true;

        networkSceneManager = GetComponent<NetworkSceneManagerDefault>();
        playerInput = GetComponent<NetworkPlayerInput>();
        playerSpawner = GetComponent<PlayerSpawner>();  
    }

    public async Task StartGame(GameMode gameMode, string sessionName)
    {
        var gameInstance = GameInstance.Instance;

        var task = NetworkRunner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = sessionName,
            SceneManager = networkSceneManager
        });

        while (!task.IsCompleted)
        {
            await Task.Yield();
        }

        var result = task.Result;
        if (result.Ok)
        {
            Debug.Log("Hosting Game OK!");
            NetworkRunner.SetActiveScene(Constants.LobbySceneName);
        }
        else
        {
            // TODO: Disconnect UI
        }
    }

    public void SpawnNetworkGameState(PlayerRef inputAuthorityPlayerRef)
    {
        var go = NetworkRunner.Spawn(NetworkGameStatePrefab, inputAuthority: inputAuthorityPlayerRef);
        NetworkGameState = go.GetComponent<NetworkGameState>();
    }

    public void SpawnAllPlayerCharacter_Game()
    {
        var gameManager = GameInstance.Instance.GameManager;
        var spawnPositions = gameManager.GetRandomSpawnPositions(4);
        var players = NetworkRunner.ActivePlayers;
        int index = 0;
        foreach (var player in players)
        {
            var playerInstance = NetworkRunner.GetPlayerObject(player).GetComponent<PlayerInstance>();
            SpawnPlayerCharacter_Game(playerInstance, spawnPositions[index++]);
        }
    }

    public void SpawnPlayerCharacter_Game(PlayerInstance playerInstance, Vector3 position)
    {
        playerSpawner.SpawnPlayerCharacter_Game(NetworkRunner, playerInstance.PlayerRef, position);
    }

    public void TrySpawnPlayerCharacter_Lobby(PlayerInstance playerInstance)
    {
        playerSpawner.TrySpawnPlayerCharacter_Lobby(NetworkRunner, playerInstance.PlayerRef);
    }

    public void Server_KillPlayer(PlayerRef victim)
    {
        playerSpawner.Server_KillPlayer(NetworkRunner, victim);
    }

    public void GameReady(PlayerRef player)
    {
        var playerInstance = NetworkRunner.GetPlayerObject(player).GetComponent<PlayerInstance>();
        playerInstance.RPC_GameReady_Lobby(player);
    }

    public void GameStart(PlayerRef player)
    {
        var playerInstance = NetworkRunner.GetPlayerObject(player).GetComponent<PlayerInstance>();
        playerInstance.Server_GameStart_Lobby();
    }

    public void OnPlayerCharacterDespawned(PlayerRef player)
    {
        playerSpawner.OnPlayerCharacterDespawned(player);
    }


    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) 
    {
        Debug.Log("SessionListUpdated:\n" + string.Join("\n", sessionList.Select(s => $"{s.Name} [{s.PlayerCount}/{s.MaxPlayers}]")));
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}
