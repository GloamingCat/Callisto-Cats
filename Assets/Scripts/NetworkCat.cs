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
				transform.position = newv;
				MeshRenderer renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
				renderer.material = StageNetwork.GetMaterial((int) newv.w);
			};
		}
		stateVar.OnValueChanged += delegate (int oldv, int newv) {
			if (!IsOwner) {
				//ulong ghostOwnerId = NetworkManager.Singleton.LocalClientId;
				//Debug.Log("Apply state change of player " + OwnerClientId + " on ghost of " + ghostOwnerId + ": " + newi);
				cat.SetState(newv);
			}
		};
		moveVar.OnValueChanged += delegate (Vector4 oldv, Vector4 newv) {
			if (!IsOwner) {
				Vector3 diff = newv;
				diff -= transform.position;
				cat.Move(diff);
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
				gameObject.name = "Player (Server)";
			} else {
				InitServerRpc(init);
				gameObject.name = "Player";
			}
		} else {
			// Player ghost.
			gameObject.name = "Player (Ghost)";
			if (IsServer) {
				// Sent to the owner the game rules.
				ClientRpcParams clientRpcParams = new ClientRpcParams {
					Send = new ClientRpcSendParams {
						TargetClientIds = new ulong[] { OwnerClientId }
					}
				};
				InitModeClientRpc(StageController.killMode, StageController.respawn, StageController.timeLimit,
					clientRpcParams);
			}
		}
		MeshRenderer renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
		renderer.material = StageNetwork.GetMaterial((int)initVar.Value.w);
		StageController.instance.SetPaused(true);
	}

	public override void OnNetworkDespawn() {
		if (!IsOwner)
			return;
		StageNetwork.mode = 0;
		StageNetwork.Exit();
	}

	// =========================================================================================
	//	Triggers
	// =========================================================================================

	public void OnMove() {
		Vector4 newPos = new Vector4(transform.position.x, transform.position.y,
			transform.position.z, transform.eulerAngles.y);
		if (IsServer) {
			moveVar.Value = newPos;
		} else {
			MoveServerRpc(newPos);
		}
    }

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
	public void InitModeClientRpc(int killMode, bool respawn, float time,
			ClientRpcParams clientRpcParams = default) {
		Debug.Log("Set mode: " + killMode + " " + respawn + " " + time);
		StageController.killMode = killMode;
		StageController.respawn = respawn;
		StageController.timeLimit = time;
	}

	[ClientRpc]
	public void DamageClientRpc(int points, Vector3 origin) {
		if (!IsOwner)
			return;
		StageController.instance.Damage(points, origin);
	}

	[ClientRpc]
	public void EatClientRpc() {
		if (!IsOwner)
			return;
		StageController.instance.EatApple();
	}

	[ClientRpc]
	public void PauseClientRpc(bool value) {
		if (!IsOwner)
			return;
		StageController.instance.SetPaused(value);
    }

	[ClientRpc]
	public void GameOverClientRpc(bool timeout) {
		StageController.instance.PartyGameOver(timeout);
    }

	// =========================================================================================
	//	Server Messages
	// =========================================================================================

	[ServerRpc]
	public void InitServerRpc(Vector4 init) {
		initVar.Value = init;
	}

	[ServerRpc]
	public void ExitServerRpc() {
		GetComponent<NetworkObject>().Despawn();
		Destroy(gameObject);
	}

	[ServerRpc]
	public void MoveServerRpc(Vector4 newPos) {
		moveVar.Value = newPos;
	}

	[ServerRpc]
	public void ChangeStateServerRpc(int id) {
		//Debug.Log("Warned of state change of player" + OwnerClientId + ": " + id);
		//character.SetState(id);
		stateVar.Value = id;
	}

	[ServerRpc]
	public void PauseServerRpc(bool value) {
		NetworkCat[] players = FindObjectsOfType<NetworkCat>();
		foreach (NetworkCat player in players) {
			player.PauseClientRpc(value);
		}
	}

	[ServerRpc]
	public void RespawnServerRpc() {
		cat.ResetState();
		stateVar.Value = 0;
	}

	[ServerRpc]
	public void InstantiateServerRpc(int prefabId, Vector3 position, Quaternion rotation) {
		StageNetwork.ServerSpawn(prefabId, OwnerClientId, position, rotation);
	}

}
