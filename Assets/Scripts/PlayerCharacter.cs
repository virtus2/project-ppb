using Cinemachine;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacter : Fusion.NetworkBehaviour
{
    public PlayerInstance PlayerInstance { get; private set; }
    public PlayerCharacterController PlayerCharacterController { get; private set; }

    public int PlayerId;

    [Networked] public bool IsDead { get; set; } = false;
    [SerializeField] private float DiedFlyingSpeed = 300f;
    [Networked] public TickTimer resurrectionTimer { get; set; }
    [SerializeField] private float resurrectionWaitingTime = 5f;

    [Header("잉크칠 관련 변수")]
    private float poopCooldownTime = 1f;
    [SerializeField] private float poopScale = 3f;
    [SerializeField] private float poopScaleMultiplierOnRushCollision = 3f;
    [Networked] public TickTimer poopTimer { get; set; }
    int splatsX = 1;
    int splatsY = 1;

    [Header("돌진 관련 변수")]
    [SerializeField] private float rushCooldownTime = 4f;
    [SerializeField, Tooltip("돌진 후 이 시간동안 닿는 플레이어를 죽임")] private float rushDurationTime = 1f;
    [Networked] public bool IsRushing { get; set; } = false;
    [Networked] public TickTimer rushTimer { get; set; }
    [Networked] public TickTimer rushDurationTimer { get; set; }
    [SerializeField] public ParticleSystem[] rushOnObjectEffect;
    [SerializeField] public ParticleSystem rushOnCharacterEffect;
    [SerializeField] public ParticleSystem rushOnRushingCharacterEffect;

    [SerializeField] private GameObject teamCircleUIPrefab;
    [SerializeField] private GameObject teamCircleUI;


    public float RushRemainingCooldownTime => rushTimer.RemainingTime(Runner).Value;


    public Vector3 PlayerPosition => PlayerCharacterController.NetworkRigidbody.ReadPosition();
    

    private void Awake()
    {
        PlayerCharacterController = GetComponent<PlayerCharacterController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(Object == null)
        {
            return;
        }

        if (Runner.IsServer == false)
        {
            return;
        }

        foreach (var contact in collision.contacts)
        {
            var other = contact.otherCollider.gameObject;
            bool contactWithPlayer = other.CompareTag(Constants.PlayerTagName);
            if(IsRushing)
            {
                if(contactWithPlayer)
                {
                    var otherPlayer = other.GetComponent<PlayerCharacter>();
                    RPC_PoopOnRushCollision();
                    if (otherPlayer.IsRushing)
                    {
                        // 둘다 돌진 중에 부딪혔다면?
                        RPC_CollideWithRushingCharacter(contact.point);
                    }
                    else
                    {
                        Debug.Log($"{PlayerId} 와 {otherPlayer.PlayerId}가 부딪힘");
                        Server_KillPlayer(PlayerId, otherPlayer.PlayerId);
                        RPC_CollideWithCharacter(contact.point);
                    }
                }
                else
                {
                    if(other.CompareTag(Constants.ArenaWallTagName))
                    {
                        RPC_CollideWithObject(contact.point);
                    }
                }
            }
        }
    }

    public override void Spawned()
    {
        IsDead = false;
        splatsX = SplatManagerSystem.instance.splatsX;
        splatsY = SplatManagerSystem.instance.splatsY;
        PlayerId = Object.InputAuthority;
        var networkObject = Runner.GetPlayerObject(Object.InputAuthority);
        PlayerInstance = networkObject.GetComponent<PlayerInstance>();
        PlayerInstance.SetPlayerCharacter(Object);
        Debug.Log($"PlayerCharacter Spawned {Object.InputAuthority}");
        if(GameInstance.Instance.CurrentGameState == GameState.Lobby)
        {
            GameInstance.Instance.LobbyManager.UpdatePlayerCount(Runner.ActivePlayers.Count());
        }
        
        if (Object.HasInputAuthority)
        {
            PlayerInstance.PlayerHUD.ShowCooldownTimUI(rushCooldownTime);
            PlayerCharacterController.Object.AssignInputAuthority(PlayerId);
        }

        teamCircleUI = Instantiate(teamCircleUIPrefab);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        var photon = GameInstance.Instance.PhotonManager;
        photon.OnPlayerCharacterDespawned(PlayerId);

        Destroy(teamCircleUI);
    }

    public override void Render()
    {
        if(Object.HasInputAuthority)
        {
            Vector3 velocity = PlayerCharacterController.NetworkRigidbody.ReadVelocity().normalized;
            Debug.DrawRay(transform.position, velocity * 3f, Color.red, 0.2f);
        }
        teamCircleUI.transform.position = transform.position;
    }

    public override void FixedUpdateNetwork()
    {
        if (GameInstance.Instance.CurrentGameState == GameState.GameCountdown)
        {
            return;
        }
        if (poopTimer.ExpiredOrNotRunning(Runner))
        {
            splatsX = SplatManagerSystem.instance.splatsX;
            splatsY = SplatManagerSystem.instance.splatsY;
            if (Object.HasInputAuthority)
            {
                RPC_Poop();
            }
            poopTimer = TickTimer.CreateFromSeconds(Runner, poopCooldownTime);
        }

        if(rushDurationTimer.ExpiredOrNotRunning(Runner))
        {
            IsRushing = false;
        }

        var playerInstance = GameInstance.Instance.PhotonManager.NetworkRunner.GetPlayerObject(Object.InputAuthority).GetComponent<PlayerInstance>();
        Image circleImage = teamCircleUI.GetComponentInChildren<Image>();
        if(playerInstance.TeamNumber == 0)
        {
            circleImage.color = Color.white;
        }
        else if(playerInstance.TeamNumber == 1)
        {
            circleImage.color = Color.red;
        }
        else
        {
            circleImage.color = Color.blue;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_Rush(Vector3 direction)
    {
        // TODO: RPC가 여러번 호출되는 문제가 있다.
        if(rushTimer.ExpiredOrNotRunning(Runner))
        {
            if(Runner.IsServer)
            {
                // Debug.Log("Rush!");
                IsRushing = true;
                PlayerCharacterController.HandleRush(direction);
                rushTimer = TickTimer.CreateFromSeconds(Runner, rushCooldownTime);
                rushDurationTimer = TickTimer.CreateFromSeconds(Runner, rushDurationTime);
            }
            if (Object.HasInputAuthority)
            {
                PlayerInstance.PlayerHUD.StartCooldown();
            }
        }
        else
        {
            if(Object.HasInputAuthority)
            {
                // Debug.LogWarning($"It's not time yet! {rushTimer.RemainingTime(Runner)}");
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_Poop()
    {
        // Debug.Log("RPC_Poop");
        Ray ray = new Ray(PlayerPosition, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10000))
        {
            Vector3 leftVec = Vector3.Cross(hit.normal, Vector3.up);
            float randScale = Random.Range(0.1f, 0.2f);

            GameObject newSplatObject = new GameObject();
            newSplatObject.transform.position = hit.point;
            if (leftVec.magnitude > 0.001f)
            {
                newSplatObject.transform.rotation = Quaternion.LookRotation(leftVec, hit.normal);
            }
            newSplatObject.transform.RotateAround(hit.point, hit.normal, Random.Range(-180, 180));
            newSplatObject.transform.localScale = new Vector3(randScale, randScale * 0.5f, randScale) * poopScale;

            Splat newSplat;
            newSplat.splatMatrix = newSplatObject.transform.worldToLocalMatrix;
            newSplat.channelMask = GetSplatChannelMask(); // new Vector4(1,0,0,0);

            float splatscaleX = 1.0f / splatsX;
            float splatscaleY = 1.0f / splatsY;
            float splatsBiasX = Mathf.Floor(Random.Range(0, splatsX * 0.99f)) / splatsX;
            float splatsBiasY = Mathf.Floor(Random.Range(0, splatsY * 0.99f)) / splatsY;

            newSplat.scaleBias = new Vector4(splatscaleX, splatscaleY, splatsBiasX, splatsBiasY);

            SplatManagerSystem.instance.AddSplat(newSplat);

            GameObject.Destroy(newSplatObject);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PoopOnRushCollision()
    {
        // Debug.Log("RPC_PoopOnRushCollision");
        Ray ray = new Ray(PlayerPosition, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10000))
        {
            Vector3 leftVec = Vector3.Cross(hit.normal, Vector3.up);
            float randScale = Random.Range(0.1f, 0.2f);

            GameObject newSplatObject = new GameObject();
            newSplatObject.transform.position = hit.point;
            if (leftVec.magnitude > 0.001f)
            {
                newSplatObject.transform.rotation = Quaternion.LookRotation(leftVec, hit.normal);
            }
            newSplatObject.transform.RotateAround(hit.point, hit.normal, Random.Range(-180, 180));
            newSplatObject.transform.localScale = new Vector3(randScale, randScale * 0.5f, randScale) * poopScale * poopScaleMultiplierOnRushCollision;

            Splat newSplat;
            newSplat.splatMatrix = newSplatObject.transform.worldToLocalMatrix;
            newSplat.channelMask = GetSplatChannelMask(); // new Vector4(1,0,0,0);

            float splatscaleX = 1.0f / splatsX;
            float splatscaleY = 1.0f / splatsY;
            float splatsBiasX = Mathf.Floor(Random.Range(0, splatsX * 0.99f)) / splatsX;
            float splatsBiasY = Mathf.Floor(Random.Range(0, splatsY * 0.99f)) / splatsY;

            newSplat.scaleBias = new Vector4(splatscaleX, splatscaleY, splatsBiasX, splatsBiasY);

            SplatManagerSystem.instance.AddSplat(newSplat);

            GameObject.Destroy(newSplatObject);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_CollideWithObject(Vector3 position)
    {
        Instantiate(rushOnObjectEffect[0], position, Quaternion.identity);
        Instantiate(rushOnObjectEffect[1], position, Quaternion.identity);
        GameInstance.Instance.SoundManager.PlaySoundEffectAtPoint(Constants.SFX_RushOnObject, position);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_CollideWithCharacter(Vector3 position)
    {
        Instantiate(rushOnCharacterEffect, position, Quaternion.identity);
        GameInstance.Instance.SoundManager.PlaySoundEffectAtPoint(Constants.SFX_RushOnCharacter, position);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_CollideWithRushingCharacter(Vector3 position)
    {
        Instantiate(rushOnRushingCharacterEffect, position, Quaternion.identity);
        GameInstance.Instance.SoundManager.PlaySoundEffectAtPoint(Constants.SFX_RushOnRushingCharacter, position);
    }

    private void Server_KillPlayer(PlayerRef killer, PlayerRef victim)
    {
        var victimPlayerInstance = Runner.GetPlayerObject(victim).GetComponent<PlayerInstance>();
        var victimPlayerCharacter = victimPlayerInstance.PlayerCharacterRef;
        victimPlayerCharacter.IsDead = true;

        var killerPlayerInstance = Runner.GetPlayerObject(killer).GetComponent<PlayerInstance>();
        var killerPlayerCharacter = killerPlayerInstance.PlayerCharacterRef;

        Vector3 flyDirection = (killerPlayerCharacter.PlayerCharacterController.NetworkRigidbody.Rigidbody.velocity.normalized + Vector3.up).normalized;
        Vector3 characterToCameraDirection = (Camera.main.transform.position - victimPlayerCharacter.PlayerCharacterController.NetworkRigidbody.transform.position).normalized;
        Vector3 resultDirection = (flyDirection + characterToCameraDirection).normalized;
        
        victimPlayerCharacter.PlayerCharacterController.NetworkRigidbody.Rigidbody.AddForce(resultDirection * DiedFlyingSpeed, ForceMode.Impulse);
        GameInstance.Instance.PhotonManager.Server_KillPlayer(victim);
        if(GameInstance.Instance.CurrentGameState == GameState.GameInProgress)
        {
            RPC_AddKillLog(killerPlayerInstance, victimPlayerInstance);
        }
        

        // victimPlayerInstance.ResurrectPlayerObject();
        // Runner.Despawn(victimPlayerObj);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AddKillLog(PlayerInstance killer, PlayerInstance victim)
    {
        GameInstance.Instance.GameManager.AddKillLog(new KillLogInformation(killer, victim));
    }

    private Vector4 GetSplatChannelMask()
    {
        switch(PlayerInstance.TeamNumber)
        {
            case 1:
                return new Vector4(0, 1, 0, 0);
            case 2:
                return new Vector4(0, 0, 0, 1);
            default:
                return new Vector4(0, 0, 0, 0);
        }
    }
}
