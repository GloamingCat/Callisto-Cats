using UnityEngine;
using Unity.Netcode;

public class Apple : MonoBehaviour {

	public float rotateSpeed = 20;

	void OnTriggerEnter(Collider other) {
		if (NetworkManager.Singleton.IsClient) {
			if (NetworkManager.Singleton.IsServer) {
				NetworkPlayer netPlayer = other.gameObject.GetComponent<NetworkPlayer>();
				if (netPlayer == null) 
					return;
				netPlayer.EatClientRpc();
				Destroy(gameObject);
			}
		} else {
			if (Player.instance.gameObject == other.gameObject) {
				// Local player
				Player.instance.EatApple();
				Destroy(gameObject);
				return;
			}
		}
	}

	void Update() {
		transform.Rotate (0, rotateSpeed * Time.deltaTime, 0);
	}

}
