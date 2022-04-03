using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{

	public Material[] materials;
	private static string[] animations = new string[] { "Idle", "Jump", "Land", "Roll", "Die", "Deaded" };

	public NetworkVariable<int> matVar = new NetworkVariable<int>(0);
	public NetworkVariable<int> animVar = new NetworkVariable<int>(0);

    private void Awake() {
		matVar.OnValueChanged += delegate (int oldi, int newi) {
			MeshRenderer renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
			renderer.material = materials[newi];
		};
		animVar.OnValueChanged += delegate (int oldi, int newi) {
			if (!IsOwner)
				GetComponent<Animator>().Play(animations[newi], 0, 0);
		};
	}

	public override void OnNetworkSpawn() {
		if (IsOwner) {
			CameraControl.instance.target = transform;
			Player.instance = GetComponent<Player>();
			if (IsServer) {
				matVar.Value = StageManager.material;
				transform.position = Spawner.instance.initialPosition;
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
		StageMenu.instance.Exit();
	}

	// =========================================================================================
	//	Triggers
	// =========================================================================================

	public void OnJump() {
		AnimationServerRpc(1);
	}

	public void OnLand() {
		AnimationServerRpc(2);
	}

	public void OnRoll() {
		AnimationServerRpc(3);
	}

	public void OnDie() {
		AnimationServerRpc(4);
	}

	public void OnDieEnd() {
		AnimationServerRpc(5);
    }

    // =========================================================================================
    //	On Client
    // =========================================================================================

    [ClientRpc]
	public void DamageClientRpc(int points, Vector3 origin) {
		if (!IsOwner)
			return;
		GetComponent<Player>().Damage(points, origin);
	}

	[ClientRpc]
	public void EatClientRpc() {
		if (!IsOwner)
			return;
		GetComponent<Player>().EatApple();
	}

	[ClientRpc]
	public void PauseClientRpc(bool value) {
		StageMenu.instance.SetPaused(value);
    }

	// =========================================================================================
	//	On Server
	// =========================================================================================

	[ServerRpc]
	public void InitServerRpc(int mat) {
		matVar.Value = mat;
		transform.position = Spawner.instance.initialPosition;
	}

	[ServerRpc]
	public void ExitServerRpc() {
		GetComponent<NetworkObject>().Despawn();
		Destroy(gameObject);
	}

	[ServerRpc]
	public void AnimationServerRpc(int id) {
		GetComponent<Animator>().Play(animations[id], 0, 0);
		animVar.Value = id;
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
		GetComponent<Animator>().Play(animations[0], 0, 0);
		animVar.Value = 0;
	}

	[ServerRpc]
	public void InstantiateServerRpc(int prefabId, Vector3 position, Quaternion rotation) {
		GameObject inst = Instantiate(Spawner.instance.prefabs[prefabId], position, rotation);
		inst.GetComponent<NetworkObject>().Spawn();
		inst.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);
	}

}
