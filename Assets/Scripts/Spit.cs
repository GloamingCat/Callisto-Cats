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
		owner = StageManager.FindOwner(gameObject);
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
			other.gameObject.GetComponent<Character>().Damage(10, transform.position);
			Destroy (gameObject);
		} else if (other.CompareTag("Player")) {
			if (StageManager.pvp) {
				if (other.gameObject != owner) {
					if (Player.instance.gameObject == other.gameObject) {
						// Server
						Player.instance.Damage(10, transform.position);
                    } else {
						// Remote client
						other.gameObject.GetComponent<NetworkPlayer>().DamageClientRpc(10, transform.position);
					}
					Destroy(gameObject);
				}
			}
		} else if (!other.CompareTag("Apple") && !other.CompareTag("Star")) {
			Destroy(gameObject);
		}
	}

}
