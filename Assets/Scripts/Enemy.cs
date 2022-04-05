using UnityEngine;

[RequireComponent(typeof(Cat))]
public class Enemy : MonoBehaviour {

	private Cat cat;
	private UnityEngine.AI.NavMeshPath path;

	public float vision = 5;

	private void Awake() {
		path = new UnityEngine.AI.NavMeshPath();
		cat = GetComponent<Cat>();
	}

	private Transform FindTarget() {
		Cat[] cats = FindObjectsOfType<Cat>();
		foreach (Cat cat in cats) {
			if (cat.CompareTag("Player") && cat.IsVisible(transform, vision)) {
				return cat.transform;
            }
        }
		return null;
    }

	private void FixedUpdate() {
		// Server only.
		if (StageNetwork.mode == 2)
			return;
		cat.UpdateMovement();
		if (cat.damaging || cat.dying)
			return;
		Transform target = FindTarget();
		if (target == null) {
			cat.ResetMoveVector();
        } else {
			UnityEngine.AI.NavMesh.CalculatePath(transform.position, target.position, UnityEngine.AI.NavMesh.AllAreas, path);
			if (path.corners.Length > 1) {
				float x = path.corners[1].x - transform.position.x;
				float z = path.corners[1].z - transform.position.z;
				cat.BounceMove(x, z);
			}
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (!other.gameObject.CompareTag("Player"))
			return;
		// Collision is always recognized by the owner of the player (using ghost of enemy).
		if (StageController.instance.IsLocalPlayer(other.gameObject))
			StageController.instance.Damage(10, transform.position);
	}

	// =========================================================================================
	//	Callbacks
	// =========================================================================================

	protected void OnDieEnd() {
		// Server only (from animator).
		if (StageNetwork.mode == 0) {
			Destroy(gameObject);
        } else if (StageNetwork.mode == 1) {
			StageNetwork.ServerDespawn(gameObject);
		}
	}

}