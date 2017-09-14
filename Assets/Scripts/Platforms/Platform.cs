using UnityEngine;
using System.Collections;

public abstract class Platform : MonoBehaviour {

	protected Rigidbody rb;
	public float speed = 1;

	protected virtual void Start() {
		rb = GetComponent<Rigidbody> ();
		StartCoroutine (ContinousPath ());
	}
	
	IEnumerator ContinousPath() {
		while (true) {
			IEnumerator routine = Path();
			if (routine == null)
				break;
			yield return StartCoroutine(routine);
		}
	}

	protected abstract IEnumerator Path();
}
