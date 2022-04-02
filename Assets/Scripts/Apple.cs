using UnityEngine;
using Unity.Netcode;

public class Apple : MonoBehaviour {

	public float rotateSpeed = 20;

	void OnTriggerEnter(Collider other) {
		if (StageManager.mode == 2)
			return;
		if (Player.instance.gameObject == other.gameObject) {
			// Local player
			Player.instance.EatApple();
			Destroy(gameObject);
			return;
		}
		NetworkPlayer netPlayer = other.gameObject.GetComponent<NetworkPlayer>();
		if (netPlayer != null) {
			// Remote player
			netPlayer.EatClientRpc();
			Destroy(gameObject);
		}
	}

	void Update() {
		transform.Rotate (0, rotateSpeed * Time.deltaTime, 0);
	}

}
