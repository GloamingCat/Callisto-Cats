using UnityEngine;

[RequireComponent(typeof(Character))]
public class Enemy : MonoBehaviour {

	private Character character;
	private UnityEngine.AI.NavMeshPath path;

	public float vision = 5;

	private void Awake() {
		path = new UnityEngine.AI.NavMeshPath();
		character = GetComponent<Character>();
	}

	private Transform findTarget() {
		Player[] players = GameObject.FindObjectsOfType<Player>();
		foreach (Player player in players) {
			if (player.IsVisible(transform, vision)) {
				return player.transform;
            }
        }
		return null;
    }

	private void FixedUpdate() {
		if (character.damaging || character.dying)
			return;
		Transform target = findTarget();
		if (target == null) {
			character.resetMoveVector();
        } else {
			UnityEngine.AI.NavMesh.CalculatePath(transform.position, target.position, UnityEngine.AI.NavMesh.AllAreas, path);
			if (path.corners.Length > 1) {
				float x = path.corners[1].x - transform.position.x;
				float z = path.corners[1].z - transform.position.z;
				character.Move(x, z);
			}
		}
	}

	protected void OnControllerColliderHit(ControllerColliderHit hit) {
		if (character.dying)
			return;
		Player player = hit.gameObject.GetComponent<Player>();
		if (player != null) {
			player.Damage(10, transform.position);
			return;
		}
		NetworkPlayer netPlayer = hit.gameObject.GetComponent<NetworkPlayer>();
		if (netPlayer != null) {
			netPlayer.DamageClientRpc(10, transform.position);
        }
	}

	// =========================================================================================
	//	Callbacks
	// =========================================================================================

	protected void OnDieEnd() {
		Destroy(gameObject);
	}

}