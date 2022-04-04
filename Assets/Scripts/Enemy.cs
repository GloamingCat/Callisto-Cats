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

	protected void OnControllerColliderHit(ControllerColliderHit hit) {
		if (!hit.gameObject.CompareTag("Player"))
			return;
		// Collision is always recognized by the owner of the player (using ghost of enemy).
		if (StageNetwork.mode == 0) {
			// Offline.
			StageController.instance.Damage(10, transform.position);
		} else if (StageController.instance.IsLocalPlayer(hit.gameObject)) {
			if (StageNetwork.mode == 1) {
				// When local player is server.
				StageController.instance.Damage(10, transform.position);
			} else {
				// When local player is client.
				NetworkCat netPlayer = hit.gameObject.GetComponent<NetworkCat>();
				netPlayer.DamageClientRpc(10, transform.position);
			}
		} else {
			// Nothing happens when it collides with other ghosts.
        }
	}

	// =========================================================================================
	//	Callbacks
	// =========================================================================================

	protected void OnDieEnd() {
		// Server only (from animator).
		StageNetwork.Despawn(gameObject);
	}

}