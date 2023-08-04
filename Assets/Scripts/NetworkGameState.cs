using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGameState : Fusion.NetworkBehaviour
{
    [Networked]
    public int RedTeamScore { get; set; } = 0;
    [Networked]
    public int BlueTeamScore { get; set; } = 0;
    [Networked]
    public int GameSceneJoinedPlayerCount { get; set; } = 0;

    [Networked]
    public TickTimer GameTimer { get; set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void Spawned()
    {
        GameInstance.Instance.PhotonManager.NetworkGameState = this;
    }

    public override void FixedUpdateNetwork()
    {
        if(Runner.IsServer)
        {
            var score = SplatManagerSystem.instance.scores;
            float totalScore = score.x + score.y + score.z + score.w;
            float redTeamRatio = score.y / totalScore;
            float blueTeamRatio = score.w / totalScore;
            RedTeamScore = (int)(redTeamRatio * 512);
            BlueTeamScore = (int)(blueTeamRatio * 512);
            // TODO: 게임 종료
            if (GameTimer.Expired(Runner))
            {
                GameTimer = TickTimer.None;
                RPC_GameOver();
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_GameOver()
    {
        int winTeamNum = 0;
        if(RedTeamScore > BlueTeamScore)
        {
            winTeamNum = 1;
        }
        if(BlueTeamScore > RedTeamScore)
        {
            winTeamNum = 2;
        }
        GameInstance.Instance.OnGameOver(winTeamNum);
    }


    public void Clear()
    {
        // TODO: 게임 다시 시작하면 서버가 초기화해줘야한다.
        RedTeamScore = 0;
        BlueTeamScore = 0;
        GameSceneJoinedPlayerCount = 0;
        GameTimer = TickTimer.None;
    }

    public void StartGameTime()
    {
        // TODO: 테스트용으로 시간을 짧게했다.
        GameTimer = TickTimer.CreateFromSeconds(Runner, GameInstance.Instance.GameRoundTime);
    }

}
