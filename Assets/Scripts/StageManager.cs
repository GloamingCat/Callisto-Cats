using ParrelSync;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;

public class StageManager : NetworkBehaviour {

    public static StageManager instance = null;

    // Network
    public Material[] materials;
    private NetworkManager networkManager;
    private UNetTransport networkTransport;
    public static int mode = 0;
    public static bool pvp = true;
    public static int material = 0;
    public static string ip;
    public static int port;
    public bool testingNetwork = true;

    public Vector3 initialPosition = new Vector3(17.13f, 0.642f, 25);

    private void Awake() {
        instance = this;
        networkManager = GetComponent<NetworkManager>();
        networkTransport = GetComponent<UNetTransport>();
    }

    private void Start() {
        // Network
        if (testingNetwork) {
            port = 7777;
            ip = "127.0.0.1";
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

    public GameObject FindOwner(GameObject obj) {
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
            GUILayout.Label("Client to address " + ip + ":" + port + ", ID " + networkManager.LocalClientId);
            if (!networkManager.IsConnectedClient)
                GUILayout.Label("Connecting...");
        } else {
            GUILayout.Label("Offline.");
        }
        if (mode > 0 && !networkManager.IsClient && !networkManager.IsServer) {
            if (GUILayout.Button(networkManager.IsServer ? "Reset Position" : "Request Position Reset")) {
                if (networkManager.IsServer && !networkManager.IsClient) {
                    // Dedicated server
                    foreach (ulong uid in networkManager.ConnectedClientsIds) {
                        var playerObject = networkManager.SpawnManager.GetPlayerNetworkObject(uid);
                        playerObject.transform.position = initialPosition;
                    }
                } else {
                    // Host Player
                    var playerObject = networkManager.SpawnManager.GetLocalPlayerObject();
                    playerObject.transform.position = initialPosition;
                }
            }
        }
        GUILayout.EndArea();
    }

}
