using ParrelSync;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager instance = null;

    private NetworkManager networkManager;
    private UNetTransport networkTransport;

    public static int mode = 0;
    public static int color = 0;
    public static string ip;
    public static int port;

    public bool testingNetwork = true;

    private void Awake() {
        instance = this;
        networkManager = GetComponent<NetworkManager>();
        networkTransport = GetComponent<UNetTransport>();
    }

    private void Start() {
        if (testingNetwork) {
            port = 7777;
            ip = "127.0.0.1";
            if (ClonesManager.IsClone()) {        
                string customArgument = ClonesManager.GetArgument();
                Debug.Log("Clone arg: " + customArgument);
                color = 3;
                mode = 2;
            } else {
                color = 2;
                mode = 1;
            }
        }
        if (mode == 0) {
            // Offline
            CameraControl.instance.target = Instantiate(networkManager.NetworkConfig.PlayerPrefab).transform;
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

    public static Vector3 InitialPosition() {
        return new Vector3(17.13f, 0.642f, 25);
    }

    public static Vector3 RandomPosition() {
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }

    // =========================================================================================
    //	Control Panel
    // =========================================================================================

    void OnGUI() {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (networkManager.IsHost || networkManager.IsServer) {
            GUILayout.Label("Hosting address " + ip + ":" + port);
            if (!networkManager.IsHost)
                GUILayout.Label("Dedicated server");
        } else if (networkManager.IsClient) {
            GUILayout.Label("Client to address " + ip + ":" + port);
            if (!networkManager.IsConnectedClient)
                GUILayout.Label("Connecting...");
        } else {
            GUILayout.Label("Offline.");
        }
        if (mode > 0 && !networkManager.IsClient && !networkManager.IsServer) {
            if (GUILayout.Button(networkManager.IsServer ? "Reset Position" : "Request Position Reset")) {
                if (networkManager.IsServer && !networkManager.IsClient) {
                    // Dedicated server
                    foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                        networkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<NetworkPlayer>().ResetPlayer();
                } else {
                    // Player
                    var playerObject = networkManager.SpawnManager.GetLocalPlayerObject();
                    var player = playerObject.GetComponent<NetworkPlayer>();
                    player.ResetPlayer();
                }
            }
        }
        GUILayout.EndArea();
    }

}
