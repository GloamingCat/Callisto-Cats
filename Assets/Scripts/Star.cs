using UnityEngine;
using System.Collections;

public class Star : MonoBehaviour {

	public float rotateSpeed = 200;
	public float lifeTime = 60f;

	void Start() {
		Destroy (gameObject, lifeTime);
		GetComponent<Rigidbody> ().velocity = Vector3.down;
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Player")) {
			AudioSource source = GetComponent<AudioSource>();
			if (source.clip != null) {
				AudioSource.PlayClipAtPoint(source.clip, transform.position);
			}
			Player.instance.HealMana(1);
			Destroy(gameObject);
		}
	}
	
	void Update() {
		transform.Rotate (0, 0, rotateSpeed * Time.deltaTime);
	}

}
