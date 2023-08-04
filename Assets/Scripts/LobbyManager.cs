using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private LobbyUI lobbyUI;

    private void Awake()
    {
        var gameInstance = GameInstance.Instance;
        gameInstance.OnLobbySceneCreated(this);

        if (!lobbyUI)
        {
            lobbyUI = FindObjectOfType<LobbyUI>();
        }
    }

    public void SpawnPlayerReadyUI(PlayerRef player)
    {
        lobbyUI.SpawnPlayerReadyUI(player);
    }

    public void DespawnPlayerReadyUI(PlayerRef player)
    {
        lobbyUI.DespawnPlayerReadyUI(player);
    }

    public void UpdatePlayerCount(int playerCount)
    {
        lobbyUI.UpdateLobbyMemberText(playerCount);
    }
}
