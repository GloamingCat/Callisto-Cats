using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{

	public Material[] materials;
	private static string[] animations = new string[] { "Idle", "Jump", "Land", "Roll", "Die", "Deaded" };

	public NetworkVariable<int> matVar = new NetworkVariable<int>(0);
	public NetworkVariable<int> animVar = new NetworkVariable<int>(0);
	public NetworkVariable<bool> pauseVar = new NetworkVariable<bool>(false);

    private void Awake() {
		matVar.OnValueChanged += delegate (int oldi, int newi) {
			MeshRenderer renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
			renderer.material = materials[newi];
		};
		animVar.OnValueChanged += delegate (int oldi, int newi) {
			if (!IsOwner)
				GetComponent<Animator>().Play(animations[newi], 0, 0);
		};
		pauseVar.OnValueChanged += delegate (bool oldi, bool newi) {
			StageMenu.instance.SetPaused(newi);
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

    public void OnPause(bool value) {
		if (IsServer) {
			StageMenu.instance.SetPaused(value);
			pauseVar.Value = value;
		} else {
			Player.instance.GetComponent<NetworkPlayer>().PauseServerRpc(value);
		}
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
		pauseVar.Value = value;
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
