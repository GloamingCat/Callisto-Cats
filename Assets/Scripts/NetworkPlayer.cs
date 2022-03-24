using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{

    public NetworkVariable<Vector3> positionVar = new NetworkVariable<Vector3>();
    public NetworkVariable<Quaternion> rotationVar = new NetworkVariable<Quaternion>();

    public override void OnNetworkSpawn() {
        positionVar.OnValueChanged += delegate (Vector3 prevp, Vector3 newp) {
            transform.position = newp;
        };
        if (IsOwner) {
            CameraControl.instance.target = transform;
            ResetPosition();
        } else {
            Destroy(GetComponent<Player>());
        }
    }

    public void ResetPosition() {
        if (NetworkManager.Singleton.IsServer) {
            transform.position = StageManager.InitialPosition();
            positionVar.Value = transform.position;
        } else {
            ResetPositionServerRpc();
        }
    }

    // =========================================================================================
    //	On Server
    // =========================================================================================

    [ServerRpc]
    private void ResetPositionServerRpc() {
        positionVar.Value = StageManager.InitialPosition();
    }

}
