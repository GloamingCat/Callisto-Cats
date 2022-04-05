using UnityEngine;
using Unity.Netcode;

public class Spit : MonoBehaviour {

	public float speed = 10;
	public float lifeTime = 15.0f;
	private MeshRenderer meshRenderer;
	private GameObject owner;

	private void Awake() {
		meshRenderer = GetComponent<MeshRenderer>();
	}

	private void Start () {
		// Change color according to shooter.
		owner = StageNetwork.FindOwner(gameObject);
		gameObject.name = "Spit (" + owner.name + ")";
		meshRenderer.material = owner.transform.GetChild(0).GetComponent<MeshRenderer>().material;
		GetComponent<Rigidbody>().velocity = transform.forward * speed;
		// Server sets the time limit.
		if (StageNetwork.mode != 2) {
			Invoke("Despawn", lifeTime);
		}
	}

	public void ProcessCollision(GameObject obj) {
		if (obj.CompareTag("Enemy")) {
			// Collided with an enemy.
			obj.GetComponent<Cat>().Damage(10, transform.position);
			Despawn();
		} else if (obj.CompareTag("Player")) {
			// Collided with a player.
			if (StageController.killMode < 2 && obj != owner) {
				Debug.Log(gameObject.name + " collided with " + obj.name);
				// Collided with an opponent player.
				if (StageController.instance.IsLocalPlayer(obj)) {
					// Host player.
					StageController.instance.Damage(10, transform.position);
				} else {
					// Ghost player.
					NetworkCat netCat = obj.GetComponent<NetworkCat>();
					netCat.DamageClientRpc(10, transform.position, netCat.OwnerOnly());
				}
				Despawn();
			}
		} else if (!obj.CompareTag("Apple") && !obj.CompareTag("Star")) {
			// Collided with something else.
			Despawn();
		}
	}

	private void OnTriggerStay(Collider other) {
		// Server only.
		if (StageNetwork.mode != 2)
			ProcessCollision(other.gameObject);
	}

	private void Despawn() {
		// Server only.
		if (StageNetwork.mode == 0)
			Destroy(gameObject);
		else 
			StageNetwork.ServerDespawn(gameObject);
	}

}
