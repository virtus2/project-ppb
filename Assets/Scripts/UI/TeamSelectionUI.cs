using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectionUI : MonoBehaviour
{
    [SerializeField]
    private Button redTeamButton;

    [SerializeField]
    private Button blueTeamButton;

    [SerializeField]
    private TextMeshProUGUI redTeamPlayerText;

    [SerializeField]
    private TextMeshProUGUI blueTeamPlayerText;

    [SerializeField]
    private TextMeshProUGUI waitingPlayerText;

    StringBuilder redTeamStringBuilder= new StringBuilder();
    StringBuilder blueTeamStringBuilder = new StringBuilder();
    StringBuilder waitingPlayerStringBuilder = new StringBuilder();

    [SerializeField] private Button exitGameButton;

    private void Awake()
    {
        redTeamButton.onClick.AddListener(() => MoveTeam(1));
        blueTeamButton.onClick.AddListener(() => MoveTeam(2));
        exitGameButton.onClick.AddListener(ExitGame);
    }

    private void Update()
    {
        // 서버에서 RPC호출해서 갱신하는게 낫겠지만 일단 시간 상 이렇게 해둔다.
        var Runner = GameInstance.Instance.PhotonManager.NetworkRunner;
        int localPlayerId = Runner.LocalPlayer;
        var players = Runner.ActivePlayers;

        foreach (var player in players)
        {
            var playerInstanceObj = Runner.GetPlayerObject(player);
            if(playerInstanceObj == null)
            {
                continue;
            }
            var playerInstance = playerInstanceObj.GetComponent<PlayerInstance>();
            if (playerInstance == null)
            {
                continue;
            }
            string playerString = playerInstance.IsLocalPlayer ? $"<color=#ff0000>Player {(int)player}</color>" : $"Player {(int)player}";
            switch (playerInstance.TeamNumber)
            {
                case 0:
                    waitingPlayerStringBuilder.AppendLine(playerString);
                    break;

                case 1:
                    redTeamStringBuilder.AppendLine(playerString);
                    break;

                case 2:
                    blueTeamStringBuilder.AppendLine(playerString);
                    break;

                default:
                    break;
            }
        }

        waitingPlayerText.text = waitingPlayerStringBuilder.ToString();
        redTeamPlayerText.text = redTeamStringBuilder.ToString();
        blueTeamPlayerText.text = blueTeamStringBuilder.ToString();
        waitingPlayerStringBuilder.Clear();
        redTeamStringBuilder.Clear();
        blueTeamStringBuilder.Clear();
    }

    public void ShowUI()
    {
        gameObject.SetActive(true);
    }

    public void HideUI()
    {
        gameObject.SetActive(false);
    }
    
    public void MoveTeam(int teamNumber)
    {
        var Runner = GameInstance.Instance.PhotonManager.NetworkRunner;
        var playerInstance = Runner.GetPlayerObject(Runner.LocalPlayer).GetComponent<PlayerInstance>();
        playerInstance.RPC_MoveTeam_Lobby(teamNumber);
    }

    private void ExitLobby()
    {
        var Runner = GameInstance.Instance.PhotonManager.NetworkRunner;
        // TODO: 로비 나가기
        // SetActiveScene, Shutdown같은건 서버에서만 가능한듯
        Runner.SetActiveScene(Constants.MainSceneName);
    }

    private void ExitGame()
    {
        Application.Quit();
    }
}
