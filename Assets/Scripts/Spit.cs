using UnityEngine;
using System.Collections;
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
		owner = StageNetwork.FindOwner(gameObject);
		meshRenderer.material = owner.transform.GetChild(0).GetComponent<MeshRenderer>().material;
		if (NetworkManager.Singleton.IsConnectedClient)
			return;
		GetComponent<Rigidbody> ().velocity = transform.forward * speed;
		Destroy (gameObject, lifeTime);
	}

	private void OnTriggerEnter(Collider other) {
		if (NetworkManager.Singleton.IsConnectedClient)
			return;
		if (other.CompareTag("Enemy")) {
			other.gameObject.GetComponent<Cat>().Damage(10, transform.position);
			Destroy (gameObject);
		} else if (other.CompareTag("Player")) {
			if (StageNetwork.pvp) {
				if (other.gameObject != owner) {
					if (StageController.instance.IsLocalPlayer(other.gameObject)) {
						// Server
						StageController.instance.Damage(10, transform.position);
                    } else {
						// Remote client
						other.gameObject.GetComponent<NetworkCat>().DamageClientRpc(10, transform.position);
					}
					Destroy(gameObject);
				}
			}
		} else if (!other.CompareTag("Apple") && !other.CompareTag("Star")) {
			Destroy(gameObject);
		}
	}

}
