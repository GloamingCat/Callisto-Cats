using UnityEngine;
using System.Collections;

public class UpDownPlatform : Platform {

	public float distance;
	public float idleTime;

	protected override IEnumerator Path () {
		yield return new WaitForSeconds (idleTime);
		rb.velocity = Vector3.up;
		yield return new WaitForSeconds (distance / speed);
		rb.velocity = Vector3.zero;
		yield return new WaitForSeconds (idleTime);
		rb.velocity = Vector3.down;
		yield return new WaitForSeconds (distance / speed);
		rb.velocity = Vector3.zero;
	}
}
