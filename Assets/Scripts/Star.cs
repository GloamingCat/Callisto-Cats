using UnityEngine;
using System.Collections;

public class Star : MonoBehaviour {

	public float rotateSpeed = 200;
	public float lifeTime = 60f;
	public AudioClip grabSound;

	void Start() {
		Destroy (gameObject, lifeTime);
		GetComponent<Rigidbody>().velocity = Vector3.down;
	}

	void OnTriggerEnter(Collider other) {
		Player player = other.gameObject.GetComponent<Player>();
		if (player != null) {
			if (grabSound != null) {
				AudioSource.PlayClipAtPoint(grabSound, transform.position);
			}
			player.HealMana(1);
			Destroy(gameObject);
		}
	}
	
	void Update() {
		transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
	}

}
