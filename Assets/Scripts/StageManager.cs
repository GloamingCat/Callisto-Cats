using ParrelSync;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;

public class StageManager : NetworkBehaviour {

    // Network
    private NetworkManager networkManager;
    private UNetTransport networkTransport;

    // Initial Menu Params
    public static int mode = -1;
    public static bool pvp = true;
    public static int material = 0;
    public static string ip = "127.0.0.1";
    public static int port = 7777;

    private void Awake() {
        networkManager = GetComponent<NetworkManager>();
        networkTransport = GetComponent<UNetTransport>();
    }

    private void Start() {
        // Testing network
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
        if (mode == 0) {
            // Offline
            Player.instance = Instantiate(networkManager.NetworkConfig.PlayerPrefab).GetComponent<Player>();
            CameraControl.instance.target = Player.instance.transform;
            StageMenu.instance.netInfoText.text = "";
        } else if (mode == 1) {
            // Host
            networkTransport.ServerListenPort = port;
            networkManager.StartHost();
            StageMenu.instance.exitText.text = "Shutdown";
            StageMenu.instance.netInfoText.text = "Hosting address " + ip + ":" + port + "\n" +
                (networkManager.IsHost ? "ID " + networkManager.LocalClientId : "Dedicated server.");
        } else if (mode == 2) {
            // Client
            networkTransport.ConnectPort = port;
            networkTransport.ConnectAddress = ip;
            networkManager.StartClient();
            StageMenu.instance.netInfoText.text = "Client to address " + ip + ":" + port + "\n" +
                (networkManager.IsConnectedClient ? "ID " + networkManager.LocalClientId : "Connecting...");
        }
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

}
