#if UNITY_EDITOR
using ParrelSync;
#endif
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageNetwork : MonoBehaviour {

    private static StageNetwork instance;

    // Network
    private static NetworkManager networkManager;
    private static UNetTransport networkTransport;

    // Initial Menu Params (network)
    public static int mode = -1;
    public static int material = 0;
    public static string ip = "127.0.0.1";
    public static int port = 7777;

    // Stage Specific
    public GameObject[] prefabs;
    public Material[] materials;

    private void Awake() {
        instance = this;
        networkManager = GetComponent<NetworkManager>();
        networkTransport = GetComponent<UNetTransport>();
#if UNITY_EDITOR
        if (mode == -1) {
            if (ClonesManager.IsClone()) {
                string customArgument = ClonesManager.GetArgument();
                if (customArgument.Length > 0)
                    Debug.Log("Clone arg: " + customArgument);
                material = 3;
                mode = 2;
            } else {
                material = 2;
                mode = 1;
            }
        }
#endif
    }

    private void Start() {
        if (mode == 0) {
            // Offline
            GameObject obj = Instantiate(networkManager.NetworkConfig.PlayerPrefab);
            StageController.instance.SetLocalPlayer(obj);
        } else if (mode == 1) {
            // Host
            networkTransport.ServerListenPort = port;
            networkManager.StartHost();
        } else if (mode == 2) {
            // Client
            networkTransport.ConnectPort = port;
            networkTransport.ConnectAddress = ip;
            networkManager.StartClient();
        }
    }

    public static string GetNetInfo() {
        if (mode == 1) {
            return "Hosting to port: " + port;
        } else if (mode == 2) {
            if (networkManager.IsConnectedClient)
                return "Client to address " + ip + ":" + port;
            else
                return "Connecting to address " + ip + ":" + port + "...";
        }
        return "";
    }

    public static GameObject FindOwner(GameObject obj) {
        if (mode == 0)
            return GameObject.FindGameObjectWithTag("Player");
        ulong ownerId = obj.GetComponent<NetworkObject>().OwnerClientId;
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
            if (player.GetComponent<NetworkObject>().OwnerClientId == ownerId) {
                return player;
            }
        }
        return null;
    }

    public static Material GetMaterial(int i) {
        return instance.materials[i];
    }

    // =========================================================================================
    //  Spawn
    // =========================================================================================

    public static void Exit() {
        if (mode == 0) {
            Destroy(NetworkManager.Singleton.gameObject);
            SceneManager.LoadScene(0);
        } else if (mode == 1) {
            GameObject[] cats = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject cat in cats) {
                cat.GetComponent<NetworkObject>().Despawn();
                Destroy(cat);
            }
        } else {
            mode = 0;
            NetworkManager.Singleton.Shutdown();
        }
    }

    public static void Spawn(int id, Transform transform, NetworkCat owner) {
        if (mode == 0) {
            Instantiate(instance.prefabs[id], transform.position, transform.rotation);
        } else if (mode == 1) {
            GameObject obj = Instantiate(instance.prefabs[id], transform.position, transform.rotation);
            obj.GetComponent<NetworkObject>().Spawn();
        } else {
            owner.InstantiateServerRpc(id, transform.position, transform.rotation);
        }
    }

    public static void ServerSpawn(int prefabId, ulong ownerId, Vector3 position, Quaternion rotation) {
        GameObject inst = Instantiate(instance.prefabs[prefabId], position, rotation);
        inst.GetComponent<NetworkObject>().Spawn();
        inst.GetComponent<NetworkObject>().ChangeOwnership(ownerId);
    }

    public static void RespawnPlayer(NetworkCat player) {
        if (mode == 1) {
             player.stateVar.Value = 0;
        } else if (mode == 2) {
            player.RespawnServerRpc();
        }
    }

    public static void ServerDespawn(GameObject obj) {
        obj.GetComponent<NetworkObject>().Despawn();
        Destroy(obj);
    }

}
