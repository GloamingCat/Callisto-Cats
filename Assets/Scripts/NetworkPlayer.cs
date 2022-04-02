using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{

	public NetworkVariable<int> colorVar = new NetworkVariable<int>(0);

    private void Awake() {
		colorVar.OnValueChanged += delegate (int oldm, int newm) {
			MeshRenderer renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
			renderer.material = StageManager.instance.materials[newm];
		};
	}

	public override void OnNetworkSpawn() {
		if (IsOwner) {
			CameraControl.instance.target = transform;
			if (IsClient) {
				ResetPlayerServerRpc(StageManager.material);
			} else {
				colorVar.Value = StageManager.material;
				transform.position = StageManager.InitialPosition();
			}
		} else {
			Destroy(GetComponent<Player>());
			Destroy(GetComponent<CharacterController>());
		}
		MeshRenderer renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
		renderer.material = StageManager.instance.materials[colorVar.Value];
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

	// =========================================================================================
	//	On Client
	// =========================================================================================

	[ClientRpc]
	public void DamageClientRpc(int point, Vector3 origin) {
		if (!IsOwner)
			return;
		GetComponent<Player>().Damage(10, transform.position);
	}

	// =========================================================================================
	//	On Server
	// =========================================================================================

	[ServerRpc]
	private void ResetPlayerServerRpc(int color) {
		colorVar.Value = color;
		transform.position = StageManager.InitialPosition();
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

}
