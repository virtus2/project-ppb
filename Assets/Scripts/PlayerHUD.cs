using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private CooldownTimeUI cooldownTimeUI;
    [SerializeField] private ResurrectGaugeUI resurrectGaugeUI;
    [SerializeField] private ArrowUI arrowUI;

    [SerializeField] private float arrowUIOffsetY = 3f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if(GameInstance.Instance.CurrentGameState == GameState.Main)
        {
            arrowUI.gameObject.SetActive(false);
            return;
        }

        var localPlayerInstance = GameInstance.Instance.GetLocalPlayerInstance();
        if (localPlayerInstance == null)
        {
            arrowUI.gameObject.SetActive(false);
            return;
        }

        var obj = localPlayerInstance.PlayerCharacterObject;
        if(obj == null)
        {
            arrowUI.gameObject.SetActive(false);
            return;
        }

        arrowUI.gameObject.SetActive(true);
        Vector3 UIPosition = Camera.main.WorldToScreenPoint(obj.transform.position);
        UIPosition.y += arrowUIOffsetY;
        UIPosition.z = 0;
        arrowUI.gameObject.transform.position = UIPosition;
    }

    public void StartCooldown()
    {
        cooldownTimeUI.StartCooldown();
    }

    public void ShowCooldownTimUI(float cooldownTime)
    {
        cooldownTimeUI.ShowUI(cooldownTime);
    }

    public void ShowResurrectUI(float resurrectTime)
    {
        resurrectGaugeUI.ShowUI(resurrectTime);
    }

    public void HideResurrectUI()
    {
        resurrectGaugeUI.gameObject.SetActive(false);
    }
}
