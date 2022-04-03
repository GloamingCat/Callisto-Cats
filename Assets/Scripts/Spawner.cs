using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public static Spawner instance = null;

    public GameObject[] prefabs;
    public Vector3 initialPosition = new Vector3(17.13f, 0.642f, 25);

    public void Awake() {
        instance = this;
    }

    public void Spawn(int id, Transform transform) {
        if (NetworkManager.Singleton.IsClient) {
            if (NetworkManager.Singleton.IsServer) {
                GameObject obj = Instantiate(prefabs[id], transform.position, transform.rotation);
                obj.GetComponent<NetworkObject>().Spawn();
            } else {
                Player.instance.GetComponent<NetworkPlayer>().InstantiateServerRpc(id, transform.position, transform.rotation);
            }
        } else {
            Instantiate(prefabs[id], transform.position, transform.rotation);
        }
    }

    public void RespawnPlayer() {
        if (NetworkManager.Singleton.IsClient) {
            if (NetworkManager.Singleton.IsServer) {
                Player.instance.GetComponent<Character>().ResetState(initialPosition);
                Player.instance.GetComponent<NetworkPlayer>().animVar.Value = 0;
            } else {
                Player.instance.GetComponent<NetworkPlayer>().RespawnServerRpc();
            }
        } else {
            Player.instance.GetComponent<Character>().ResetState(initialPosition);
        }
    }

}
