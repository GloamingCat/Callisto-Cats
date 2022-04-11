using UnityEngine;

public class Apple : MonoBehaviour {

	public float rotateSpeed = 20;

	private void Update() {
		transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
	}

	public void ProcessCollision(GameObject obj) {
		if (!obj.CompareTag("Player"))
			return;
		if (StageManager.mode == 0) {
			PlayerInterface.instance.EatApple();
			Destroy(gameObject);
			return;
		} else {
			if (PlayerInterface.instance.IsLocalPlayer(obj)) {
				// Collides with host player.
				PlayerInterface.instance.EatApple();
			} else {
				// Collides with player ghost.
				NetworkCat netPlayer = obj.GetComponent<NetworkCat>();
				netPlayer.EatClientRpc(netPlayer.OwnerOnly());
			}
			StageManager.ServerDespawn(gameObject);
		}
	}

	private void OnTriggerEnter(Collider other) {
		// Server only.
		if (StageManager.mode != 2)
			ProcessCollision(other.gameObject);
	}

}
