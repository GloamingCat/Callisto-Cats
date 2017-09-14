using UnityEngine;
using System.Collections;

public class Spit : MonoBehaviour {

	public float speed = 10;
	public float lifeTime = 15.0f;
	
	void Start () {
		AudioSource source = GetComponent<AudioSource> ();
		if (source.clip != null) {
			source.Play ();
		}
		GetComponent<Rigidbody> ().velocity = transform.forward * speed;
		Destroy (gameObject, lifeTime);
	}

	void OnTriggerEnter(Collider other) {
		if (other.CompareTag ("Enemy")) {
			other.gameObject.GetComponent<Enemy>().Damage(10, transform.position);
			Destroy (gameObject);
		} else {
			if (!other.CompareTag ("Player") && !other.CompareTag("Apple")) {
				Destroy(gameObject);
			}
		}
	}

}
