using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInkUI : MonoBehaviour
{
    public Image RedTeamImage;
    public Image BlueTeamImage;

    // Update is called once per frame
    void Update()
    {
        var gameState = GameInstance.Instance.PhotonManager.NetworkGameState;
        int redTeamScore = gameState.RedTeamScore;
        int blueTeamScore = gameState.BlueTeamScore;
        int totalScore = redTeamScore + blueTeamScore;

        RedTeamImage.fillAmount = redTeamScore / (totalScore+0.0000001f);
        BlueTeamImage.fillAmount = blueTeamScore / (totalScore + 0.0000001f);
    }
}
