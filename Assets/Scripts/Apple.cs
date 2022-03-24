using UnityEngine;
using System.Collections;

public class Apple : MonoBehaviour {

	public float rotateSpeed = 20;
	public AudioClip eatSound;

	void OnTriggerEnter(Collider other) {
		Player player = other.gameObject.GetComponent<Player>();
		if (player != null) {
			if (eatSound != null) { 
				AudioSource.PlayClipAtPoint(eatSound, transform.position);
			}
			player.HealLife(2);
			Destroy(gameObject);
		}
	}

	void Update() {
		transform.Rotate (0, rotateSpeed * Time.deltaTime, 0);
	}

}
