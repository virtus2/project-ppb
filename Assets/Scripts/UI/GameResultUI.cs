using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultUI : MonoBehaviour
{
    [SerializeField]
    private Color VictoryTextColor;
    
    [SerializeField]
    private Color LoseTextColor;

    [SerializeField]
    private TextMeshProUGUI resultText;

    [SerializeField]
    private TextMeshProUGUI InkReservationPercentText;

    [SerializeField]
    private Image RedTeamImage;

    [SerializeField]
    private Image BlueTeamImage;

    private void Awake()
    {

    }

    public void ShowUI(bool victory)
    {
        gameObject.SetActive(true);

        var gameState = GameInstance.Instance.PhotonManager.NetworkGameState;
        int redTeamScore = gameState.RedTeamScore;
        int blueTeamScore = gameState.BlueTeamScore;
        int totalScore = redTeamScore + blueTeamScore;

        RedTeamImage.fillAmount = redTeamScore / (totalScore + 0.0000001f);
        BlueTeamImage.fillAmount = blueTeamScore / (totalScore + 0.0000001f);
            
        bool redTeamWin = redTeamScore > blueTeamScore;

        if (victory)
        {
            resultText.text = Constants.VictoryText;
            resultText.color = VictoryTextColor;
            if (redTeamWin)
            {
                InkReservationPercentText.text = $"{(redTeamScore / (totalScore + 0.0000001f)) * 100f:F1}% 점유";
            }
            else
            {
                InkReservationPercentText.text = $"{(blueTeamScore / (totalScore + 0.0000001f)) * 100f:F1}% 점유";
            }
        }
        else
        {
            resultText.text = Constants.LoseText;
            resultText.color = LoseTextColor;

            if (redTeamWin)
            {
                InkReservationPercentText.text = $"{(blueTeamScore / (totalScore + 0.0000001f))*100f:F1}% 점유";
            }
            else
            {
                InkReservationPercentText.text = $"{(redTeamScore / (totalScore + 0.0000001f)) * 100f:F1}% 점유";
            }
        }
    }
}
