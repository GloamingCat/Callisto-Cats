using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	public static CameraControl instance;

	public bool following = true;
	public float rotationSpeed;
	public float distanceToPlayer;
	public float height;

	void Awake() {
		instance = this;
	}

	void Update () {
		if (following) {
			transform.position = Player.instance.transform.position;
			transform.Translate (0, height, -distanceToPlayer);
			transform.RotateAround (Player.instance.transform.position, Vector3.up, 
			                        Input.GetAxis ("Camera") * rotationSpeed * Time.deltaTime);
		}
	}
}
