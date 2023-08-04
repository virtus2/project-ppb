using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const int BUTTON_FIRE = 1;

    public const int BUTTON_FORWARD = 3;
    public const int BUTTON_BACKWARD = 4;
    public const int BUTTON_LEFT = 5;
    public const int BUTTON_RIGHT = 6;

    public const int BUTTON_JUMP = 7;

    public NetworkButtons Buttons;
    public float mouseX;
    public float mouseY;
    public Vector3 mousePosition;

    public bool IsUp(int button)
    {
        return Buttons.IsSet(button) == false;
    }

    public bool IsDown(int button)
    {
        return Buttons.IsSet(button);
    }
}

public class NetworkPlayerInput : Fusion.Behaviour, INetworkRunnerCallbacks
{
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var inputData = new NetworkInputData();

        if (Input.GetKey(KeyCode.W))
        {
            inputData.Buttons.Set(NetworkInputData.BUTTON_FORWARD, true);
        }

        if (Input.GetKey(KeyCode.S))
        {
            inputData.Buttons.Set(NetworkInputData.BUTTON_BACKWARD, true);
        }

        if (Input.GetKey(KeyCode.A))
        {
            inputData.Buttons.Set(NetworkInputData.BUTTON_LEFT, true);
        }

        if (Input.GetKey(KeyCode.D))
        {
            inputData.Buttons.Set(NetworkInputData.BUTTON_RIGHT, true);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            inputData.Buttons.Set(NetworkInputData.BUTTON_JUMP, true);
        }

        if (Input.GetMouseButton(0))
        {
            inputData.Buttons.Set(NetworkInputData.BUTTON_FIRE, true);
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int layerMask = ~(1 << LayerMask.NameToLayer(Constants.ArenaWallLayerName));
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, layerMask))
            {
                inputData.mousePosition.Set(hit.point.x, hit.point.y, hit.point.z);
            }
        }

        inputData.mouseX = Input.GetAxis("Mouse X");
        inputData.mouseY = Input.GetAxis("Mouse Y");

        input.Set(inputData);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { } 
}
