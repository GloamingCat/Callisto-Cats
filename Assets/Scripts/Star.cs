using UnityEngine;

public class Star : MonoBehaviour {

	public float rotateSpeed = 200;
	public float lifeTime = 60f;

	void Start() {
		Destroy (gameObject, lifeTime);
		GetComponent<Rigidbody>().velocity = Vector3.down;
	}

	void OnTriggerEnter(Collider other) {
		if (PlayerInterface.instance.IsLocalPlayer(other.gameObject)) {
			PlayerInterface.instance.GrabStar();
			Destroy(gameObject);
		}
	}
	
	void Update() {
		transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
	}

}
