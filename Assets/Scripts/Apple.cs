using UnityEngine;

public class Apple : MonoBehaviour {

	public float rotateSpeed = 20;
	private void Update() {
		transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
	}

	public void ProcessCollision(GameObject obj) {
		if (!obj.CompareTag("Player"))
			return;
		if (StageNetwork.mode == 0) {
			StageController.instance.EatApple();
			Destroy(gameObject);
			return;
		} else {
			if (StageController.instance.IsLocalPlayer(obj)) {
				// Collides with host player.
				StageController.instance.EatApple();
			} else {
				// Collides with player ghost.
				NetworkCat netPlayer = obj.GetComponent<NetworkCat>();
				netPlayer.EatClientRpc(netPlayer.OwnerOnly());
			}
			StageNetwork.ServerDespawn(gameObject);
		}
	}

	private void OnTriggerEnter(Collider other) {
		// Server only.
		if (StageNetwork.mode != 2)
			ProcessCollision(other.gameObject);
	}

}
