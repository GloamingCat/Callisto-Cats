using UnityEngine;
using System.Collections;

public class Apple : MonoBehaviour {

	public float rotateSpeed = 20;

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Player")) {
			AudioSource source = GetComponent<AudioSource>();
			if (source.clip != null) { 
				AudioSource.PlayClipAtPoint(source.clip, transform.position);
			}
			Player.instance.HealLife(2);
			Destroy(gameObject);
		}
	}

	void Update() {
		transform.Rotate (0, rotateSpeed * Time.deltaTime, 0);
	}

}
