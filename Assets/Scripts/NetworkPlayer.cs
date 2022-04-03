using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour {

	public Material[] materials;
	
	public NetworkVariable<int> matVar = new NetworkVariable<int>(0);
	public NetworkVariable<int> stateVar = new NetworkVariable<int>(0);

	private Character character;
	private Player player = null;

    private void Awake() {
		character = GetComponent<Character>();
		matVar.OnValueChanged += delegate (int oldi, int newi) {
			MeshRenderer renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
			renderer.material = materials[newi];
		};
		stateVar.OnValueChanged += delegate (int oldi, int newi) {
			if (!IsOwner) {
				ulong ghostOwnerId = NetworkManager.Singleton.LocalClientId;
				//Debug.Log("Apply state change of player " + OwnerClientId + " on ghost of " + ghostOwnerId + ": " + newi);
				character.SetState(newi);
			}
		};
	}

	public override void OnNetworkSpawn() {
		if (IsOwner) {
			player = GetComponent<Player>();
			if (IsServer) {
				matVar.Value = StageManager.material;
				gameObject.name = "Player (Server)";
			} else {
				InitServerRpc(StageManager.material);
				gameObject.name = "Player";
			}
			if (!IsServer)
				PauseServerRpc(false);
		} else {
			Destroy(GetComponent<Player>());
			gameObject.name = "Player (Ghost)";
		}
		MeshRenderer renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
		renderer.material = materials[matVar.Value];
	}

	public override void OnNetworkDespawn() {
		if (!IsOwner)
			return;
		StageManager.mode = 0;
		StageManager.Exit();
	}

	// =========================================================================================
	//	Triggers
	// =========================================================================================

	public void OnStateChange(int i) {
		if (IsOwner) {
			//Debug.Log("Warn server of new state of player " + OwnerClientId + ": " + i);
			ChangeStateServerRpc(i);
		}
	}

    public void OnPause(bool pause) {
		PauseServerRpc(pause);
	}

    // =========================================================================================
    //  Broadcast for clients
    // =========================================================================================

    [ClientRpc]
	public void DamageClientRpc(int points, Vector3 origin) {
		if (!IsOwner)
			return;
		player.Damage(points, origin);
	}

	[ClientRpc]
	public void EatClientRpc() {
		if (!IsOwner)
			return;
		player.EatApple();
	}

	[ClientRpc]
	public void PauseClientRpc(bool value) {
		if (!IsOwner)
			return;
		player.SetPaused(value);
    }

	// =========================================================================================
	//	Server Messages
	// =========================================================================================

	[ServerRpc]
	public void InitServerRpc(int mat) {
		matVar.Value = mat;
	}

	[ServerRpc]
	public void ExitServerRpc() {
		GetComponent<NetworkObject>().Despawn();
		Destroy(gameObject);
	}

	[ServerRpc]
	public void ChangeStateServerRpc(int id) {
		//Debug.Log("Warned of state change of player" + OwnerClientId + ": " + id);
		character.SetState(id);
		stateVar.Value = id;
	}

	[ServerRpc]
	public void PauseServerRpc(bool value) {
		NetworkPlayer[] players = FindObjectsOfType<NetworkPlayer>();
		foreach (NetworkPlayer player in players) {
			player.PauseClientRpc(value);
		}
	}

	[ServerRpc]
	public void RespawnServerRpc() {
		character.ResetState();
		stateVar.Value = 0;
	}

	[ServerRpc]
	public void InstantiateServerRpc(int prefabId, Vector3 position, Quaternion rotation) {
		Spawner.instance.ServerSpawn(prefabId, OwnerClientId, position, rotation);
	}

}
