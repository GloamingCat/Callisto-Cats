using UnityEngine;

public class Apple : MonoBehaviour {

	public float rotateSpeed = 20;
	private void Update() {
		transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
	}

	public void ProcessCollision(GameObject obj) {
		if (!obj.CompareTag("Player"))
			return;
		if (StageController.instance.IsLocalPlayer(obj)) {
			// Collides with host player.
			StageController.instance.EatApple();
			if (StageNetwork.mode == 0)
				Destroy(gameObject);
			else
				StageNetwork.ServerDespawn(gameObject);
		} else {
			// Collides with ghost player.
			NetworkCat netPlayer = obj.GetComponent<NetworkCat>();
			netPlayer.EatClientRpc(netPlayer.OwnerOnly());
			StageNetwork.ServerDespawn(gameObject);
		}
	}

	private void OnTriggerEnter(Collider other) {
		// Server only.
		if (StageNetwork.mode == 2)
			return;
		ProcessCollision(other.gameObject);
	}

}
