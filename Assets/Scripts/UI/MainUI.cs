using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [SerializeField] 
    private Button hostButton;
    
    [SerializeField] 
    private Button joinButton;

    [SerializeField]
    private Button QuitButton;

    [SerializeField]
    private SessionUI sessionUI;

    
    private void Awake()
    {
        SetButtonOnClickEvents();
    }

    private void SetButtonOnClickEvents()
    {
        var photon = GameInstance.Instance.PhotonManager;
        hostButton.onClick.AddListener(() =>
        {
            sessionUI.ShowHostSessionUI();
            HideUI();
        });
        joinButton.onClick.AddListener(() =>
        {
            sessionUI.ShowJoinSessionUI();
            HideUI();
        });
        QuitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    public void ShowUI()
    {
        gameObject.SetActive(true);
    }

    private void HideUI()
    {
        gameObject.SetActive(false);
    }
}
