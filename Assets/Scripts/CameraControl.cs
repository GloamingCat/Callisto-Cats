using UnityEngine;

public class CameraControl : MonoBehaviour {

	public Transform target;
	public float rotationSpeed;
	public float distanceToPlayer;
	public float height;

	void Update () {
		if (target != null) {
			transform.position = target.position;
			transform.Translate (0, height, -distanceToPlayer);
			transform.RotateAround (target.position, Vector3.up, 
			                        Input.GetAxis("Camera") * rotationSpeed * Time.deltaTime);
		}
	}

}
