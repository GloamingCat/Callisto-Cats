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

	private void OnTriggerEnter(Collider other) {
		// Server only.
		if (StageNetwork.mode == 2)
			return;
		if (other.CompareTag("Enemy")) {
			// Collided with an enemy.
			other.gameObject.GetComponent<Cat>().Damage(10, transform.position);
			Despawn();
		} else if (other.CompareTag("Player")) {
			// Collided with a player.
			if (StageController.killMode < 2 && other.gameObject != owner) {
				Debug.Log(gameObject.name + " collided with " + other.gameObject.name);
				// Collided with an opponent player.
				if (StageController.instance.IsLocalPlayer(other.gameObject)) {
					// Host player.
					StageController.instance.Damage(10, transform.position);
                } else {
					// Ghost player.
					NetworkCat netCat = other.gameObject.GetComponent<NetworkCat>();
					netCat.DamageClientRpc(10, transform.position, netCat.OwnerOnly());
				}
				Despawn();
			}
		} else if (!other.CompareTag("Apple") && !other.CompareTag("Star")) {
			// Collided with something else.
			Despawn();
		}
	}

	private void Despawn() {
		// Server only.
		if (StageNetwork.mode == 0)
			Destroy(gameObject);
		else 
			StageNetwork.ServerDespawn(gameObject);
	}

}
