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
				renderer.material = StageNetwork.GetMaterial((int) newv.w);
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
			StageController.instance.SetLocalPlayer(gameObject);
			Vector4 init = new Vector4(transform.position.x, transform.position.y,
				transform.position.z, StageNetwork.material);
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
				InitModeClientRpc(StageController.killMode, StageController.respawn, 
					StageController.timeLimit, OwnerOnly());
			}
		}
		MeshRenderer renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
		renderer.material = StageNetwork.GetMaterial((int)initVar.Value.w);
		StageController.instance.SetPaused(true);
	}

	public override void OnNetworkDespawn() {
		if (!StageController.instance.IsLocalPlayer(gameObject))
			return;
		StageNetwork.mode = 0;
		StageNetwork.Exit();
	}

	public ClientRpcParams OwnerOnly() {
		return new ClientRpcParams {
			Send = new ClientRpcSendParams {
				TargetClientIds = new ulong[] { OwnerClientId }
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
		StageController.killMode = killMode;
		StageController.respawn = respawn;
		StageController.timeLimit = time;
	}

	[ClientRpc]
	public void DamageClientRpc(int points, Vector3 origin,
			ClientRpcParams clientRpcParams) {
		// When server detects collision with an opponent's spit or an enemy.
		StageController.instance.Damage(points, origin);
	}

	[ClientRpc]
	public void EatClientRpc(ClientRpcParams clientRpcParams) {
		// When server detects collision with apple.
		StageController.instance.EatApple();
	}

	[ClientRpc]
	public void PauseClientRpc(bool value) {
		// When some player resquests pause.
		// Broadcasted to all clients.
		StageController.instance.SetPaused(value);
    }

	[ClientRpc]
	public void GameOverClientRpc(bool timeout) {
		// When everybody died.
		// Broadcasted to all clients.
		StageController.instance.PartyGameOver(timeout);
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
		if (StageController.instance.gameOver)
			StageController.instance.GameOver(false);
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
		StageNetwork.ServerSpawn(prefabId, OwnerClientId, position, rotation);
	}

}
