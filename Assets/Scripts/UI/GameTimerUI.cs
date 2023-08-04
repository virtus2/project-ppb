using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimerUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI timerText;

    private int time;

    public void ShowUI()
    {
        gameObject.SetActive(true);
    }

    private void Update()
    {
        float? gameTime = GameInstance.Instance.PhotonManager.NetworkGameState.GameTimer.RemainingTime(GameInstance.Instance.PhotonManager.NetworkRunner);
        if(gameTime.HasValue)
        {
            float t = gameTime.Value;
            time = (int)gameTime;
            timerText.text = $"{time / 60}:{time % 60}";
        }
    }
}
