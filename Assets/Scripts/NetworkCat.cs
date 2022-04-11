using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Cat))]
public class NetworkCat : NetworkBehaviour {
	
	// Ghost variables
	public NetworkVariable<Vector4> initVar = new NetworkVariable<Vector4>(0);
	public NetworkVariable<int> stateVar = new NetworkVariable<int>(0);
	public NetworkVariable<Vector4> moveVar = new NetworkVariable<Vector4>();

	private Cat cat;

    private void Awake() {
		if (CompareTag("Player")) {
			initVar.OnValueChanged += delegate (Vector4 oldv, Vector4 newv) {
				// Initialize player and its ghosts.
				transform.position = newv;
				MeshRenderer renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
				renderer.material = StageManager.GetMaterial((int) newv.w);
			};
		}
		stateVar.OnValueChanged += delegate (int oldv, int newv) {
			if (!IsOwner) {
				// Update state of ghosts (player or enemy).
				cat.SetState(newv);
			}
		};
		moveVar.OnValueChanged += delegate (Vector4 oldv, Vector4 newv) {
			if (!IsOwner) {
				// Update position/angle of ghosts (player or enemy).
				cat.Move((Vector3) newv - transform.position);
				Vector3 eulerAngles = transform.eulerAngles;
				eulerAngles.y = newv.w;
				transform.eulerAngles = eulerAngles;
			}
		};
		cat = GetComponent<Cat>();
	}

	public override void OnNetworkSpawn() {
		if (!CompareTag("Player"))
			return;
		if (IsOwner) {
			// Local player.
			PlayerInterface.instance.SetLocalPlayer(gameObject);
			Vector4 init = new Vector4(transform.position.x, transform.position.y,
				transform.position.z, StageManager.material);
			if (IsServer) {
				initVar.Value = init;
				gameObject.name = "Player " + OwnerClientId + " (Server)";
			} else {
				InitServerRpc(init);
				gameObject.name = "Player " + OwnerClientId;
			}
		} else {
			// Player ghost.
			gameObject.name = "Player " + OwnerClientId + " (Ghost)";
			if (IsServer) {
				// Sent to the owner the game rules.
				InitModeClientRpc(PlayerInterface.killMode, PlayerInterface.respawn, 
					PlayerInterface.timeLimit, OwnerOnly());
			}
		}
		MeshRenderer renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
		renderer.material = StageManager.GetMaterial((int)initVar.Value.w);
		PlayerInterface.instance.SetPaused(true);
	}

	public override void OnNetworkDespawn() {
		if (!PlayerInterface.instance.IsLocalPlayer(gameObject))
			return;
		StageManager.mode = 0;
		StageManager.Exit();
	}

	public ClientRpcParams OwnerOnly() {
		return OwnerOnly(OwnerClientId);
	}

	public ClientRpcParams OwnerOnly(ulong id) {
		return new ClientRpcParams {
			Send = new ClientRpcSendParams {
				TargetClientIds = new ulong[] { id }
			}
		};
	}

	// =========================================================================================
	//	Triggers
	// =========================================================================================

	public void OnMove() {
		// When local player or enemy moved. Update position of ghosts.
		Vector4 newPos = new Vector4(transform.position.x, transform.position.y,
			transform.position.z, transform.eulerAngles.y);
		if (IsServer) {
			// Send to other clients.
			moveVar.Value = newPos;
		} else {
			// Send to server.
			MoveServerRpc(newPos);
		}
    }

	public void OnStateChange(int i) {
		// When local enemy or player changes its state. Update state of ghosts.
		if (IsOwner) {
			if (IsServer)
				stateVar.Value = i;
			else
				ChangeStateServerRpc(i);
		} else {
			Debug.Log("State change on ghost: " + gameObject.name + ", state " + i);
        }
	}

    public void OnPause(bool value) {
		// When local player paused. Warn other clients.
		if (IsServer) {
			NetworkCat[] players = FindObjectsOfType<NetworkCat>();
			PauseClientRpc(value);
		} else {
			PauseServerRpc(value);
		}
	}

	// =========================================================================================
	//  Broadcast for clients
	// =========================================================================================

	[ClientRpc]
	public void InitModeClientRpc(int killMode, bool respawn, float time,
			ClientRpcParams clientRpcParams) {
		// When player is spawned, to inform the gameplay mode.
		PlayerInterface.killMode = killMode;
		PlayerInterface.respawn = respawn;
		PlayerInterface.timeLimit = time;
	}

	[ClientRpc]
	public void ShotClientRpc(int points, Vector3 origin, ulong shooterId,
			ClientRpcParams clientRpcParams) {
		// When server detects this client was shot by an opponent.
		if (PlayerInterface.instance.Damage(points, origin)) {
			IncreaseKillsServerRpc(shooterId);
		}
	}

	[ClientRpc]
	public void IncreaseKillsClientRpc(ClientRpcParams clientRpcParams) {
		// When server detects collision with an opponent's spit or an enemy.
		PlayerInterface.instance.IncreaseKills();
	}

	[ClientRpc]
	public void EatClientRpc(ClientRpcParams clientRpcParams) {
		// When server detects collision with apple.
		PlayerInterface.instance.EatApple();
	}

	[ClientRpc]
	public void PauseClientRpc(bool value) {
		// When some player resquests pause.
		// Broadcasted to all clients.
		PlayerInterface.instance.SetPaused(value);
    }

	[ClientRpc]
	public void GameOverClientRpc(bool timeout) {
		// When everybody died.
		// Broadcasted to all clients.
		PlayerInterface.instance.PartyGameOver(timeout);
    }

	// =========================================================================================
	//	Server Messages
	// =========================================================================================

	[ServerRpc]
	public void InitServerRpc(Vector4 init) {
		// This player requested initialization. Update ghosts.
		initVar.Value = init;
	}

	[ServerRpc]
	public void ExitServerRpc() {
		// This player exited the scene. Delete all ghosts.
		GetComponent<NetworkObject>().Despawn();
		Destroy(gameObject);
	}

	[ServerRpc]
	public void GameOverServerRpc() {
		// This player died. Recheck if someone is still alive.
		if (PlayerInterface.instance.gameOver)
			PlayerInterface.instance.GameOver(false);
	}

	[ServerRpc]
	public void IncreaseKillsServerRpc(ulong shooterId) {
		if (GetComponent<NetworkObject>().OwnerClientId == shooterId) {
			// Shoot by local/host player.
			PlayerInterface.instance.IncreaseKills();
		} else {
			// Shoot by ghost/remote player.
			IncreaseKillsClientRpc(OwnerOnly(shooterId));
		}
	}

	[ServerRpc]
	public void MoveServerRpc(Vector4 newPos) {
		// This player moved. Update ghost positions.
		moveVar.Value = newPos;
	}

	[ServerRpc]
	public void ChangeStateServerRpc(int id) {
		// This player changed state. Update ghost positions.
		stateVar.Value = id;
	}

	[ServerRpc]
	public void PauseServerRpc(bool value) {
		// This player requested pause.
		PauseClientRpc(value);
	}

	[ServerRpc]
	public void RespawnServerRpc() {
		// This player requested a respawn.
		cat.ResetState();
		stateVar.Value = 0;
	}

	[ServerRpc]
	public void InstantiateServerRpc(int prefabId, Vector3 position, Quaternion rotation) {
		// This player requested a new object.
		StageManager.ServerSpawn(prefabId, OwnerClientId, position, rotation);
	}

}
