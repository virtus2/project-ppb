using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyMemberText;

    [SerializeField] private TextMeshProUGUI noticeText;

    [SerializeField] private Button gameReadyButton;
    [SerializeField] private Button gameStartButton;

    [SerializeField] private Button lobbyMenuButton;

    [SerializeField] private Canvas readyUICanvas;
    [SerializeField] private GameObject readyUIPrefab;
    [SerializeField] private Dictionary<PlayerRef, GameObject> readyUIByPlayer = new Dictionary<PlayerRef, GameObject>();
    [SerializeField] private float readyUIoffsetY;
    [SerializeField] private Sprite readyUIimageX;
    [SerializeField] private Sprite readyUIimageV;

    [SerializeField] private TeamSelectionUI teamSelectionUI;

    [SerializeField] private TextMeshProUGUI pleaseReadyText;


    private void Awake()
    {
        gameReadyButton.onClick.AddListener(() => GameReady());
        gameStartButton.onClick.AddListener(() => GameStart());
        lobbyMenuButton.onClick.AddListener(() => ShowTeamSelectionUI());

        if(GameInstance.Instance.PhotonManager.NetworkRunner.IsServer)
        {
            gameReadyButton.gameObject.SetActive(false);
            gameStartButton.gameObject.SetActive(true);
        }
        else
        {
            gameReadyButton.gameObject.SetActive(true);
            gameStartButton.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        ShowTeamSelectionUI();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ShowTeamSelectionUI();
        }

        if(Input.GetKeyDown(KeyCode.F5))
        {
            if (GameInstance.Instance.PhotonManager.NetworkRunner.IsServer)
            {
                gameStartButton.onClick.Invoke();
            }
            else
            {
                gameReadyButton.onClick.Invoke();
            }
        }

        var localPlayerInstance = GameInstance.Instance.GetLocalPlayerInstance();
        if(localPlayerInstance)
        {
            bool showText = localPlayerInstance.LobbyReady == false;
            pleaseReadyText.gameObject.SetActive(showText);
        }

        UpdateReadyUI();
    }
    
    private void ShowTeamSelectionUI()
    {
        if (teamSelectionUI.isActiveAndEnabled)
        {
            teamSelectionUI.HideUI();
        }
        else
        {
            teamSelectionUI.ShowUI();
        }
    }

    private void UpdateReadyUI()
    {
        var photon = GameInstance.Instance.PhotonManager;
        var runner = photon.NetworkRunner;
        
        foreach(var player in readyUIByPlayer.Keys)
        {
            var playerObject = runner.GetPlayerObject(player);
            if(playerObject == null)
            {
                continue;
            }
            var playerInstance = playerObject.GetComponent<PlayerInstance>();
            var playerCharacter = playerInstance.PlayerCharacterObject;
            if (playerCharacter)
            {
                Vector3 UIPosition = Camera.main.WorldToScreenPoint(playerCharacter.transform.position);
                UIPosition.y += readyUIoffsetY;
                UIPosition.z = 0;
                readyUIByPlayer[player].gameObject.transform.position = UIPosition;
                if (playerInstance.LobbyReady)
                {
                    var image = readyUIByPlayer[player].GetComponent<Image>();
                    image.sprite = readyUIimageV;
                    image.color = Color.green;
                }
                else
                {
                    var image = readyUIByPlayer[player].GetComponent<Image>();
                    image.sprite = readyUIimageX;
                    image.color = Color.red;
                }
            }
        }
    }

    public void SpawnPlayerReadyUI(PlayerRef player)
    {
        var go = Instantiate(readyUIPrefab, readyUICanvas.transform);
        readyUIByPlayer.Add(player, go);
    }

    public void DespawnPlayerReadyUI(PlayerRef player)
    {
        if (readyUIByPlayer.TryGetValue(player, out var go))
        {
            Destroy(go);
            readyUIByPlayer.Remove(player);
        }
    }

    public void UpdateLobbyMemberText(int playerCount)
    {
        lobbyMemberText.text = $"{playerCount}/4";
    }

    public void ShowNoticeText(string text)
    {

    }

    private void GameReady()
    {
        var photon = GameInstance.Instance.PhotonManager;
        photon.GameReady(photon.NetworkRunner.LocalPlayer);
    }
    
    private void GameStart()
    {
        var photon = GameInstance.Instance.PhotonManager;
        photon.GameStart(photon.NetworkRunner.LocalPlayer);
    }
}
