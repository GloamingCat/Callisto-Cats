using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{

    public NetworkVariable<int> colorVar = new NetworkVariable<int>();

    public override void OnNetworkSpawn() {
        colorVar.OnValueChanged += delegate (int prevc, int newc) {
            GetComponent<Character>().SetColor(newc);
        };
        if (IsOwner) {
            CameraControl.instance.target = transform;
            ResetPlayer();
        } else {
            Destroy(GetComponent<Player>());
        }
    }

    public void ResetPlayer() {
        if (NetworkManager.Singleton.IsServer) {
            transform.position = StageManager.InitialPosition();
            colorVar.Value = StageManager.color;
        } else {
            ResetPositionServerRpc(StageManager.color);
        }
    }


    // =========================================================================================
    //	Other input
    // =========================================================================================

    public void FixedUpdate() {
        
    }

    // =========================================================================================
    //	On Server
    // =========================================================================================

    [ServerRpc]
    private void ResetPositionServerRpc(int color) {
        colorVar.Value = color;
    }





}
