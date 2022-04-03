using Unity.Netcode;
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

	private Transform FindTarget() {
		Character[] cats = FindObjectsOfType<Character>();
		foreach (Character cat in cats) {
			if (cat.CompareTag("Player") && cat.IsVisible(transform, vision)) {
				return cat.transform;
            }
        }
		return null;
    }

	private void FixedUpdate() {
		if (NetworkManager.Singleton.IsConnectedClient)
			return;
		character.UpdateMovement();
		if (character.damaging || character.dying)
			return;
		Transform target = FindTarget();
		if (target == null) {
			character.ResetMoveVector();
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
		if (NetworkManager.Singleton.IsConnectedClient || character.dying)
			return;
		if (hit.gameObject.CompareTag("Player")) {
			if (Player.instance.gameObject == hit.gameObject) {
				Player.instance.Damage(10, transform.position);
				return;
			}
			NetworkPlayer netPlayer = hit.gameObject.GetComponent<NetworkPlayer>();
			if (netPlayer != null) {
				netPlayer.DamageClientRpc(10, transform.position);
			}
		}
	}

	// =========================================================================================
	//	Callbacks
	// =========================================================================================

	protected void OnDieEnd() {
		Destroy(gameObject);
	}

}