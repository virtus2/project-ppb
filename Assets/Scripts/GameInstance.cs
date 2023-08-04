using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public enum GameState
{
    Main,
    Lobby,
    GameCountdown,
    GameInProgress,
    Finished,
}

public class GameInstance : MonoBehaviour
{
    // GameInstance class is a singleton that lives during runtime
    private static GameInstance instance = null;

    public static GameInstance Instance
    {
        get
        {
            if (!instance)
            {
                var obj = new GameObject(typeof(GameInstance).Name);
                instance = obj.AddComponent<GameInstance>();
            }
            return instance;
        }
    }

    public int GameRoundTime;
    public GameState CurrentGameState { get; private set; }
    public PhotonManager PhotonManager { get; private set; }
    public SoundManager SoundManager { get; private set; }
    public LobbyManager LobbyManager { get; private set; }
    public GameManager GameManager { get; private set; }

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        PhotonManager = FindObjectOfType<PhotonManager>();
        SoundManager = FindObjectOfType<SoundManager>();

        Screen.SetResolution(1600, 900, false);
    }

    public PlayerInstance GetLocalPlayerInstance()
    {
        if(PhotonManager == null)
        {
            return null;
        }

        if(PhotonManager.NetworkRunner == null)
        {
            return null;
        }

        var localPlayer = PhotonManager.NetworkRunner.LocalPlayer;
        var playerObject = PhotonManager.NetworkRunner.GetPlayerObject(localPlayer);
        if(playerObject == null)
        {
            return null;
        }
        
        var playerInstance = playerObject.GetComponent<PlayerInstance>();
        return playerInstance;
    }

    public bool ShouldPlayerControlCharacter()
    {
        if (CurrentGameState == GameState.GameCountdown)
        {
            return false;
        }
        if (CurrentGameState == GameState.Finished)
        {
            return false;
        }
        return true;
    }

    public void OnLobbySceneCreated(LobbyManager lobbyManager)
    {
        CurrentGameState = GameState.Lobby;
        LobbyManager = lobbyManager;
    }

    public void OnGameSceneCreated(GameManager gameManager)
    {
        CurrentGameState = GameState.GameCountdown;
        GameManager = gameManager;

        PhotonManager.SpawnAllPlayerCharacter_Game();
    }

    public void OnGameSceneCountdownFinished()
    {
        CurrentGameState = GameState.GameInProgress;
        SoundManager.PlayAudioSource(Constants.BGM_MainGamePlayMusic, true, 0.5f);

        if(PhotonManager.NetworkRunner.IsServer)
        {
            PhotonManager.NetworkGameState.StartGameTime();
        }
        GameManager.ShowGameTimerUI();
    }

    public void OnGameOver(int teamNum)
    {
        Debug.Log("Game Over!");
        CurrentGameState = GameState.Finished;
        SoundManager.PlaySoundEffectOneShot(Constants.SFX_Whistle);

        var playerInstance = GetLocalPlayerInstance();
        GameManager.ShowGameOverUI(playerInstance.TeamNumber == teamNum);
    }
}
