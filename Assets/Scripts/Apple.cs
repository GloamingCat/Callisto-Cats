using UnityEngine;

public class Apple : MonoBehaviour {

	public float rotateSpeed = 20;

	void OnTriggerEnter(Collider other) {
		// Server only.
		if (StageNetwork.mode == 2)
			return;
		if (!other.gameObject.CompareTag("Player"))
			return;
		if (StageNetwork.mode == 0) {
			StageController.instance.EatApple();
			Destroy(gameObject);
		} else {
			if (StageController.instance.IsLocalPlayer(other.gameObject)) {
				// Collides with host player.
				StageController.instance.EatApple();
				StageNetwork.Despawn(gameObject);
			} else {
				// Collides with ghost player.
				NetworkCat netPlayer = other.gameObject.GetComponent<NetworkCat>();
				netPlayer.EatClientRpc();
				StageNetwork.Despawn(gameObject);
			}
		}
	}

	void Update() {
		transform.Rotate (0, rotateSpeed * Time.deltaTime, 0);
	}

}
