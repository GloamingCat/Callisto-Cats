using Unity.Netcode;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public static Spawner instance;

    public GameObject[] prefabs;
    public Vector3 initialPosition = new Vector3(17.13f, 0.642f, 25);

    private void Awake() {
        instance = this;
    }

    public void ServerSpawn(int prefabId, ulong ownerId, Vector3 position, Quaternion rotation) {
        GameObject inst = Instantiate(prefabs[prefabId], position, rotation);
        inst.GetComponent<NetworkObject>().Spawn();
        inst.GetComponent<NetworkObject>().ChangeOwnership(ownerId);
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
        Player.instance.transform.position = initialPosition;
        Player.instance.Respawn();
        if (NetworkManager.Singleton.IsClient) {
            if (NetworkManager.Singleton.IsServer) {
                Player.instance.GetComponent<NetworkPlayer>().stateVar.Value = 0;
            } else {
                Player.instance.GetComponent<NetworkPlayer>().RespawnServerRpc();
            }
        }
    }

}
