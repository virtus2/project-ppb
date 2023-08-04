using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInstance : NetworkBehaviour
{
    [Networked]
    public string PlayerName { get; set; }

    [Networked]
    public PlayerRef PlayerRef { get; private set; }

    [Networked]
    public int TeamNumber { get; set; }

    [Networked]
    public bool LobbyReady { get; private set; } = false;

    public bool IsLocalPlayer;

    public int PlayerId;

    public NetworkObject PlayerCharacterObject { get; private set; }
    public PlayerCharacter PlayerCharacterRef { get; private set; }
    public PlayerHUD PlayerHUD { get; private set; }

    [Networked] public bool IsDead { get; set; } = false;
    [Networked] public TickTimer ResurrectionTimer { get; set; }
    [SerializeField] private float ResurrectionWaitingTime = 5f;


    private void Awake()
    {
        DontDestroyOnLoad(this);
        PlayerHUD = FindObjectOfType<PlayerHUD>();
    }

    public override void Spawned()
    {
        if(Object.HasInputAuthority)
        {
            IsLocalPlayer = true;

            if (Object.HasStateAuthority)
            {
                // 호스트는 항상 준비상태
                LobbyReady = true;
                // 호스트가 생성되면 게임 스테이트도 같이 생성
                GameInstance.Instance.PhotonManager.SpawnNetworkGameState(Object.StateAuthority);
            }
        }
        PlayerRef = Object.InputAuthority;
        PlayerId = PlayerRef;
        TeamNumber = 0;

        TrySpawnPlayerCharacter_Lobby();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        GameInstance.Instance.LobbyManager.UpdatePlayerCount(Runner.ActivePlayers.Count());
    }

    public override void FixedUpdateNetwork()
    {
        var photon = GameInstance.Instance.PhotonManager;
        if(GameInstance.Instance.CurrentGameState == GameState.Lobby)
        {
            if (ResurrectionTimer.Expired(Runner) && IsDead && Runner.IsServer)
            {
                IsDead = false;
                ResurrectionTimer = TickTimer.None;

                photon.TrySpawnPlayerCharacter_Lobby(this);
            }
        }
        if(GameInstance.Instance.CurrentGameState == GameState.GameInProgress)
        {
            if (ResurrectionTimer.Expired(Runner) && IsDead && Runner.IsServer)
            {
                IsDead = false;
                ResurrectionTimer = TickTimer.None;

                var position = GameInstance.Instance.GameManager.GetRandomSpawnPositions(1);
                photon.SpawnPlayerCharacter_Game(this, position[0]);
            }
        }
    }

    public void SetPlayerCharacter(NetworkObject playerCharacterObject)
    {
        this.PlayerCharacterObject = playerCharacterObject;
        PlayerCharacterRef = playerCharacterObject.GetComponent<PlayerCharacter>();
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_MoveTeam_Lobby(int teamNumber)
    {
        if(teamNumber != 0)
        {
            int teamPlayerCount = GameInstance.Instance.PhotonManager.GetTeamPlayerCount(teamNumber);
            if(teamPlayerCount >= 2)
            {
                return;
            }    
        }
        
        TeamNumber = teamNumber;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_GameReady_Lobby(PlayerRef player)
    {
        LobbyReady = !LobbyReady;
    }

    public void Server_GameStart_Lobby()
    {
        var photon = GameInstance.Instance.PhotonManager;
        var runner = photon.NetworkRunner;
        var players = runner.ActivePlayers;

        foreach (var player in players)
        {
            var playerInstance = runner.GetPlayerObject(player).GetComponent<PlayerInstance>();
            if (playerInstance.TeamNumber == 0)
            {
                return;
            }

            if (playerInstance.LobbyReady == false)
            {
                return;
            }
        }
        
        int redTeamPlayerCount = photon.GetTeamPlayerCount(1);
        int blueTeamPlayerCount = photon.GetTeamPlayerCount(2);
        if (redTeamPlayerCount <= 0 || blueTeamPlayerCount <= 0)
        {
            return;
        }

        foreach(var player in players)
        {
            var playerInstance = runner.GetPlayerObject(player).GetComponent<PlayerInstance>();
            // TODO: 따로 함수로 분리?
            playerInstance.ResurrectionTimer = TickTimer.None;
            playerInstance.IsDead = false;
            playerInstance.LobbyReady = false;
        }

        runner.SetActiveScene(Constants.GameSceneName);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ShowResurrectGauge(PlayerRef deadPlayer)
    {
        // Debug.Log($"this:{PlayerId}, dead:{deadPlayer}");
        if (Runner.LocalPlayer == deadPlayer)
        {
            PlayerHUD.ShowResurrectUI(ResurrectionWaitingTime);
        }
    }

    public void Server_OnPlayerCharacterDied(PlayerRef victim)
    {
        IsDead = true;
        PlayerCharacterObject = null;
        PlayerCharacterRef = null;
        ResurrectionTimer = TickTimer.CreateFromSeconds(Runner, ResurrectionWaitingTime);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_StartGameCountdown()
    {
        // Debug.Log("Start Game!!!");
        GameInstance.Instance.GameManager.ShowGameCountdownUI();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_GameSceneLoadCompleted()
    {
        // On Sever
        GameInstance.Instance.PhotonManager.NetworkGameState.GameSceneJoinedPlayerCount++;
        if(GameInstance.Instance.PhotonManager.NetworkGameState.GameSceneJoinedPlayerCount >= Runner.ActivePlayers.Count())
        {
            RPC_StartGameCountdown();
        }
    }

    public void TrySpawnPlayerCharacter_Lobby()
    {
        GameInstance.Instance.PhotonManager.TrySpawnPlayerCharacter_Lobby(this);
    }
}
