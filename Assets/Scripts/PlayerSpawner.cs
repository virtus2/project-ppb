using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef playerInstancePrefab;
    [SerializeField] private NetworkPrefabRef playerCharacterPrefab;
    private Dictionary<PlayerRef, NetworkObject> spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();
    private Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    [Networked, Capacity(2)]
    NetworkArray<PlayerRef> RedTeamPlayers => default;

    [Networked, Capacity(2)]
    NetworkArray<PlayerRef> BlueTeamPlayers => default;


    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            NetworkObject networkPlayerObject = runner.Spawn(playerInstancePrefab, Constants.Vector3Zero, Quaternion.identity, player);
            spawnedPlayers.Add(player, networkPlayerObject);

            var playerInstance = networkPlayerObject.GetComponent<PlayerInstance>();
            if(!playerInstance)
            {
                Debug.LogWarning($"{player.PlayerId}'s instance is null!");
            }
            runner.SetPlayerObject(player, networkPlayerObject);
        }
        Debug.Log($"{player.PlayerId} is joined");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (spawnedPlayers.TryGetValue(player, out NetworkObject networkObject))
        {
            var playerInstance = networkObject.GetComponent<PlayerInstance>();
            runner.Despawn(playerInstance.PlayerCharacterObject);
            runner.Despawn(networkObject);
            spawnedPlayers.Remove(player);
            spawnedCharacters.Remove(player);

            GameInstance.Instance.LobbyManager.DespawnPlayerReadyUI(player);
        }
    }

    public void Server_KillPlayer(NetworkRunner runner, PlayerRef victim)
    {
        if(spawnedPlayers.TryGetValue(victim, out NetworkObject networkObject))
        {
            var victimPlayerInstance = runner.GetPlayerObject(victim).GetComponent<PlayerInstance>();
            StartCoroutine(Server_KillPlayerObject(runner, victimPlayerInstance.PlayerCharacterObject));
            victimPlayerInstance.Server_OnPlayerCharacterDied(victim);
            victimPlayerInstance.RPC_ShowResurrectGauge(victim);
            spawnedCharacters.Remove(victim);

            GameInstance.Instance.LobbyManager.DespawnPlayerReadyUI(victim);
        }
        else
        {
            // error?
        }
    }

    private IEnumerator Server_KillPlayerObject(NetworkRunner runner, NetworkObject obj)
    {
        yield return new WaitForSeconds(1f);
        runner.Despawn(obj);
    }

    public void TrySpawnPlayerCharacter_Lobby(NetworkRunner runner, PlayerRef player)
    {
        StartCoroutine(SpawnPlayerCharacter_Lobby(runner, player));
    }

    public void SpawnPlayerCharacter_Game(NetworkRunner runner, PlayerRef player, Vector3 position)
    {
        // 이미 플레이어 인스턴스가 있으므로 대기할 필요가 없다.
        if(runner.IsServer)
        {
            // TODO: 스폰 포지션을 정해야한다.
            //Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.DefaultPlayers) * 3, 1, 0);
            NetworkObject networkPlayerObject = runner.Spawn(playerCharacterPrefab, position, Quaternion.identity, player);
            spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    private IEnumerator SpawnPlayerCharacter_Lobby(NetworkRunner runner, PlayerRef player)
    {
        // 로비씬이 아직 준비되지 않았으면 대기한다.
        while (!GameInstance.Instance.LobbyManager)
        {
            yield return null;
        }
        // 플레이어 인스턴스가 만들어질때까지 기다린다.
        while(!runner.GetPlayerObject(player))
        {
            yield return null;
        }

        if (runner.IsServer)
        {
            Debug.Log($"Spawn Player {player.PlayerId}'s Character");
            // TODO: 스폰 포지션을 정해야한다.
            //Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.DefaultPlayers) * 3, 1, 0);
            NetworkObject networkPlayerObject = runner.Spawn(playerCharacterPrefab, Constants.Vector3Zero, Quaternion.identity, player);
            spawnedCharacters.Add(player, networkPlayerObject);
        }
        GameInstance.Instance.LobbyManager.SpawnPlayerReadyUI(player);
    }

    public void OnPlayerCharacterDespawned(PlayerRef player)
    {
        if (spawnedCharacters.ContainsKey(player))
        {
            spawnedCharacters.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}
