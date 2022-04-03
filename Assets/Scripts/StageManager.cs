using ParrelSync;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour {

    // Network
    private static NetworkManager networkManager;
    private static UNetTransport networkTransport;

    // Initial Menu Params
    public static int mode = -1;
    public static bool pvp = true;
    public static int material = 0;
    public static string ip = "127.0.0.1";
    public static int port = 7777;

    private void Awake() {
        networkManager = GetComponent<NetworkManager>();
        networkTransport = GetComponent<UNetTransport>();
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
    }

    private void Start() {
        if (mode == 0) {
            // Offline
            Instantiate(networkManager.NetworkConfig.PlayerPrefab).GetComponent<Player>();
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
            return "Hosting address " + ip + ":" + port;
        } else if (mode == 2) {
            if (networkManager.IsConnectedClient)
                return "Client to address " + ip + ":" + port;
            else
                return "Conecting to address " + ip + ":" + port;
        }
        return "";
    }

    public static GameObject FindOwner(GameObject obj) {
        if (mode == 0)
            return Player.instance.gameObject;
        ulong ownerId = obj.GetComponent<NetworkObject>().OwnerClientId;
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
            if (player.GetComponent<NetworkObject>().OwnerClientId == ownerId) {
                return player;
            }
        }
        return null;
    }

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

}
