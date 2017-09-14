using UnityEngine;
using System.Collections;

public class OrbitalPlatform : Platform {

	float angSpeed;

	protected override IEnumerator Path () {
		angSpeed = speed / transform.localPosition.magnitude;
		return null;
	}

	public void Update() {
		rb.angularVelocity = new Vector3(0, angSpeed, 0);
	}

}