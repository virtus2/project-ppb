using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionUI : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField sessionNameInputField;

    [SerializeField]
    private Button sessionOKButton;

    [SerializeField] 
    private Button sessionBackButton;

    [SerializeField]
    private MainUI mainUI;

    [SerializeField]
    private LoadingUI loadingUI;

    private Fusion.GameMode sessionMode;


    private void Awake()
    {
        SetButtonOnClickEvents();
    }

    private void SetButtonOnClickEvents()
    {
        var photon = GameInstance.Instance.PhotonManager;
        sessionOKButton.onClick.AddListener(() =>
        {
            OnClickOKButton();
            HideUI();
            loadingUI.ShowUI();
        });
        sessionBackButton.onClick.AddListener(() =>
        {
            OnClickBackButton();
            HideUI();
            mainUI.ShowUI();
        });
    }

    private async void OnClickOKButton()
    {
        string sessionName = sessionNameInputField.text;
        if(string.IsNullOrEmpty(sessionName))
        {
            // TODO: 경고 텍스트 띄우기
            return;
        }

        var photon = GameInstance.Instance.PhotonManager;
        await photon.StartGame(sessionMode, sessionName);
    }

    private void OnClickBackButton()
    {
        HideUI();
    }

    public void ShowHostSessionUI()
    {
        gameObject.SetActive(true);
        sessionMode = Fusion.GameMode.Host;
    }

    public void ShowJoinSessionUI()
    {
        gameObject.SetActive(true);
        sessionMode = Fusion.GameMode.Client;
    }

    private void HideUI()
    {
        gameObject.SetActive(false);
    }
}
