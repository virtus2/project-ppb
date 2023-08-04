using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GameCountdownUI gameCountdownUI;
    [SerializeField] private GameTimerUI gameTimerUI;
    [SerializeField] private GameInkUI gameInkUI;
    [SerializeField] private GameResultUI gameResultUI;
    [SerializeField] private GameKillLogUI gameKillLogUI;

    public void ShowGameCountdownUI()
    {
        gameCountdownUI.ShowUI();
    }

    public void ShowGameTimerUI()
    {
        gameTimerUI.ShowUI();
    }

    public void ShowGameOverUI(bool victory)
    {
        gameResultUI.ShowUI(victory);
    }

    public void AddKillLog(KillLogInformation log)
    {
        gameKillLogUI.AddKillLog(log);
    }
}
