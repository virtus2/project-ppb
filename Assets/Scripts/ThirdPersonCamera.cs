using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : Fusion.NetworkBehaviour
{
    private PlayerCharacter playerCharacter; 

    public void Init(PlayerCharacter playerCharacter)
    {
        this.playerCharacter = playerCharacter;
    }

    public override void Render()
    {
        // TODO: 덜덜 떨리는 문제가 있다
        if (!playerCharacter)
        {
            return;
        }
    }
}
