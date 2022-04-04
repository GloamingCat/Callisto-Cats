using UnityEngine;
using Unity.Netcode;

public class Apple : MonoBehaviour {

	public float rotateSpeed = 20;

	void OnTriggerEnter(Collider other) {
		if (NetworkManager.Singleton.IsClient) {
			if (NetworkManager.Singleton.IsServer) {
				NetworkCat netPlayer = other.gameObject.GetComponent<NetworkCat>();
				if (netPlayer == null) 
					return;
				netPlayer.EatClientRpc();
				Destroy(gameObject);
			}
		} else {
			if (StageController.instance.IsLocalPlayer(other.gameObject)) {
				// Local player
				StageController.instance.EatApple();
				Destroy(gameObject);
				return;
			}
		}
	}

	void Update() {
		transform.Rotate (0, rotateSpeed * Time.deltaTime, 0);
	}

}
