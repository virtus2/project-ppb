using Fusion;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterController : Fusion.NetworkBehaviour
{
    public PlayerCharacter PlayerCharacter { get; private set; }
    public NetworkRigidbody NetworkRigidbody { get; private set; }
    public Transform WorldTransform { get; private set; }

    [SerializeField] private float MovementSpeed = 10f;
    [SerializeField] private float RushSpeed = 30f;
    

    private void Awake()
    {
        PlayerCharacter = GetComponent<PlayerCharacter>();
        NetworkRigidbody = GetComponent<NetworkRigidbody>();
        WorldTransform = GameInstance.Instance.transform;
    }

    public override void FixedUpdateNetwork()
    {
        if (GameInstance.Instance.ShouldPlayerControlCharacter() == false)
        {
            return;
        }

        if(PlayerCharacter.IsDead)
        {
            return;
        }
        Vector3 direction;
        bool isJumping = false;
        Vector3 velocity = NetworkRigidbody.ReadVelocity();
        if (velocity.y > 0.0001f || velocity.y < -0.0001f)
        {
            isJumping = true;
        }
        else
        {
            isJumping = false;
        }

        if (GetInput(out NetworkInputData input))
        {
            direction = default;

            if (input.IsDown(NetworkInputData.BUTTON_FORWARD))
            {
                direction += WorldTransform.forward;
            }

            if (input.IsDown(NetworkInputData.BUTTON_BACKWARD))
            {
                direction -= WorldTransform.forward;
            }

            if (input.IsDown(NetworkInputData.BUTTON_LEFT))
            {
                direction -= WorldTransform.right;
            }

            if (input.IsDown(NetworkInputData.BUTTON_RIGHT))
            {
                direction += WorldTransform.right;
            }

            direction = direction.normalized;

            if (input.IsDown(NetworkInputData.BUTTON_JUMP))
            {
                // 점프중에 또 점프 못한다.
                if (!isJumping)
                {
                    NetworkRigidbody.Rigidbody.AddForce(WorldTransform.up, ForceMode.Impulse);
                    // Debug.Log("Jump!");
                }
            }

            if(input.IsDown(NetworkInputData.BUTTON_FIRE))
            {
                Vector3 rushDir = (input.mousePosition - transform.position).normalized;
                rushDir.y = 0;
                if (Object.HasInputAuthority)
                {
                    PlayerCharacter.RPC_Rush(rushDir);
                }
                
                return;
            }

            // Debug.Log($"mouse X: {input.mouseX}, mouse Y: {input.mouseY}");
            // Debug.Log($"Original Rotation: {originalRotation}");
            // Debug.Log($"Moving Rotation: {rotation}");
            
            NetworkRigidbody.Rigidbody.AddForce(direction * MovementSpeed);
        }
    }

    public void HandleRush(Vector3 direction)
    {
        NetworkRigidbody.Rigidbody.velocity = (direction * RushSpeed);
    }


}