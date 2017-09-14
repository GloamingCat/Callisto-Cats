using UnityEngine;

public class Enemy : Character {

	public float vision = 5;
	NavMeshPath path;

	protected override void Awake() {
		base.Awake ();
		path = new NavMeshPath();
		lifePoints = 30;
	}

	protected override void UpdateMovement() {
		if (damaging)
			return;
		
		if (dying) {
			moveVector.x = 0;
			moveVector.z = 0;
			return;
		}

		float distance = (Player.instance.transform.position - transform.position).magnitude;
		if (!Player.instance.dying && distance < vision && !dying) {
			NavMesh.CalculatePath(transform.position, Player.instance.transform.position, NavMesh.AllAreas, path);
			if (path.corners.Length > 1) {
				float x = path.corners[1].x - transform.position.x;
				float z = path.corners[1].z - transform.position.z;
				Move (x, z);
			}
		} else {
			moveVector.x = 0;
			moveVector.z = 0;
		}
	}

	public void OnDieEnd() {
		Destroy (gameObject);
	}

	public void OnJumpEnd() {
		landing = false;
		jumping = false;
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (!dying && hit.gameObject.CompareTag ("Player")) {
			Player.instance.Damage(10, transform.position);
		}
	}

}