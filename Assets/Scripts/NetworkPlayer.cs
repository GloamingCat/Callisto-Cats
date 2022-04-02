using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{

	public GameObject[] prefabs;

	public NetworkVariable<int> matVar = new NetworkVariable<int>(0);

    private void Awake() {
		matVar.OnValueChanged += delegate (int oldm, int newm) {
			MeshRenderer renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
			renderer.material = StageManager.instance.materials[newm];
		};
	}

	public override void OnNetworkSpawn() {
		if (IsOwner) {
			CameraControl.instance.target = transform;
			Player.instance = GetComponent<Player>();
			if (IsClient) {
				ResetPlayerServerRpc(StageManager.material);
				gameObject.name = "Player";
			} else {
				matVar.Value = StageManager.material;
				transform.position = StageManager.instance.initialPosition;
				gameObject.name = "Player (Server)";
			}
		} else {
			Destroy(GetComponent<Player>());
			gameObject.name = "Player (Ghost)";
		}
		MeshRenderer renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
		renderer.material = StageManager.instance.materials[matVar.Value];
	}

	// =========================================================================================
	//	Triggers
	// =========================================================================================

	public void OnLand() {
		LandServerRpc();
	}

	public void OnRoll() {
		RollServerRpc();
	}

	public void OnJump() {
		JumpServerRpc();
	}
	public void OnDie() {
		DieServerRpc();
	}

	public void OnShoot() {
		if (StageManager.mode == 2) {
			InstantiateServerRpc(0, transform.position, transform.rotation);
		} else {
			GameObject obj = Instantiate(prefabs[0], transform.position, transform.rotation);
			if (StageManager.mode == 1) {
				obj.GetComponent<NetworkObject>().Spawn();
			}
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
	private void ResetPlayerServerRpc(int color) {
		matVar.Value = color;
		transform.position = StageManager.instance.initialPosition;
	}

	[ServerRpc]
	private void LandServerRpc() {
		GetComponent<Animator>().SetTrigger("land");
    }

	[ServerRpc]
	private void RollServerRpc() {
		GetComponent<Animator>().SetTrigger("roll");
	}

	[ServerRpc]
	private void JumpServerRpc() {
		GetComponent<Animator>().SetTrigger("jump");
	}

	[ServerRpc]
	private void DieServerRpc() {
		GetComponent<Animator>().SetTrigger("die");
	}

	[ServerRpc]
	private void InstantiateServerRpc(int prefabId, Vector3 position, Quaternion rotation) {
		GameObject inst = Instantiate(prefabs[prefabId], position, rotation);
		inst.GetComponent<NetworkObject>().Spawn();
		inst.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);
	}

}
