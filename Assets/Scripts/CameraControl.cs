using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	public static CameraControl instance;

	public Transform target;
	public float rotationSpeed;
	public float distanceToPlayer;
	public float height;

	void Awake() {
		instance = this;
	}

	void Update () {
		if (target != null) {
			transform.position = target.position;
			transform.Translate (0, height, -distanceToPlayer);
			transform.RotateAround (target.position, Vector3.up, 
			                        Input.GetAxis ("Camera") * rotationSpeed * Time.deltaTime);
		}
	}
}
