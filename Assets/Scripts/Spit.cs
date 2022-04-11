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
		owner = StageManager.FindOwner(gameObject);
		gameObject.name = "Spit (" + owner.name + ")";
		meshRenderer.material = owner.transform.GetChild(0).GetComponent<MeshRenderer>().material;
		GetComponent<Rigidbody>().velocity = transform.forward * speed;
		if (StageManager.mode == 2) {
			// Ghost
			Destroy(this);
		} else {
			// Server sets the time limit.
			Invoke("Despawn", lifeTime);
		}
	}

	private void OnTriggerStay(Collider other) {
		// Server only.
		if (other.CompareTag("Enemy")) {
			// Collided with an enemy.
			OnEnemyCollision(other.GetComponent<Cat>());
			Despawn();
		} else if (other.CompareTag("Player")) {
			// Collided with a player.
			if (PlayerInterface.killMode < 2 && other.gameObject != owner) {
				// Collided with an opponent player.
				OnPlayerCollision(other.GetComponent<Cat>());
				Despawn();
			}
		} else if (!other.CompareTag("Apple") && !other.CompareTag("Star")) {
			// Collided with something else.
			Despawn();
		}
	}

	private void OnEnemyCollision(Cat enemy) {
		enemy.Damage(10, transform.position);
		if (enemy.lifePoints == 0) {
			if (PlayerInterface.instance.IsLocalPlayer(owner)) {
				// Shot by host/local player.
				PlayerInterface.instance.IncreaseKills(1);
			} else {
				// Shot by ghost/remote player.
				NetworkCat netCat = owner.GetComponent<NetworkCat>();
				netCat.IncreaseKillsClientRpc(1, netCat.OwnerOnly());
			}
		}
	}

	private void OnPlayerCollision(Cat opponent) {
		if (PlayerInterface.instance.IsLocalPlayer(opponent.gameObject)) {
			// Collided with local/host player.
			PlayerInterface.instance.Damage(10, transform.position);
			if (opponent.lifePoints == 0) {
				// Shot by ghost/remote player.
				NetworkCat netCat = owner.GetComponent<NetworkCat>();
				netCat.IncreaseKillsClientRpc(2, netCat.OwnerOnly());
			}
		} else {
			// Collided with player ghost.
			ulong ownerId = owner.GetComponent<NetworkObject>().OwnerClientId;
			NetworkCat opponentCat = opponent.GetComponent<NetworkCat>();
			opponentCat.ShotClientRpc(10, transform.position, ownerId, opponentCat.OwnerOnly());
		}
	}

	private void Despawn() {
		// Server only.
		if (StageManager.mode == 0)
			Destroy(gameObject);
		else 
			StageManager.ServerDespawn(gameObject);
	}

}
