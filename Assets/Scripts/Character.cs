using UnityEngine;

public class Character : MonoBehaviour {

	// Components
	private Animator animator;
	private CharacterController controller;

	// Constants
	public static int maxLifePoints = 30;
	public static float gravity = 0.25f;
	public static float damageTime = 0.5f;
	public float moveSpeed = 5;
	public float jumpSpeed = 2.5f;

	// State
	public int lifePoints;
	private Vector3 moveVector;
	public bool jumping { get; private set; }
	public bool landing { get; private set; }
	public bool damaging { get; private set; }
	public bool dying { get; private set; }
	public bool rolling { get; private set; }

	// Big Jump
	public Vector3 boost { get; private set; }
	public bool bigJumping { get; private set; }
	public float bigJumpSpeed = 6;

	// Fire
	public static int maxManaPoints = 20;
	public int manaPoints;

	private void Awake() {
		controller = GetComponent<CharacterController> ();
		animator = GetComponent<Animator> ();
		moveVector = Vector3.zero;
		boost = Vector3.zero;
		jumping = false;
		damaging = false;
		dying = false;
		landing = false;
		rolling = false;
		lifePoints = maxLifePoints;
		manaPoints = maxManaPoints;
	}

    public void UpdateMovement() {
		if (!damaging) {
			if (dying) {
				ResetMoveVector();
			} else if (boost != Vector3.zero) {
                moveVector.x = boost.x;
                moveVector.z = boost.z;
            }
		}
		if (controller.isGrounded) {
			if (jumping && !landing && !dying && moveVector.y < 0) { // if was jumping
				Land();
			}
		} else { // if on air
			if (transform.position.y < 0) {
				DieByCliff();
			} else {
				moveVector.y -= gravity * Time.deltaTime;
			}
		}
		UpdatePlatform();
	}

	public bool IsVisible(Transform other, float vision) {
		if (dying)
			return false;
		float distance = (transform.position - other.position).magnitude;
		return distance < vision;
	}

	public void HealLife(int points) {
		lifePoints = Mathf.Min(lifePoints + points, maxLifePoints);
	}

	public void HealMana(int points) {
		manaPoints = Mathf.Min(manaPoints + points, maxManaPoints);
	}

	public void ResetState(Vector3 position) {
		transform.position = position;
		lifePoints = maxLifePoints;
		manaPoints = maxManaPoints;
		animator.Play("Idle");
	}

	// =========================================================================================
	//	Movement
	// =========================================================================================

	public virtual void Move(float dx, float dz) {
		Vector3 transformedVector = AdjustedVector (dx, dz);
		SetDirection (transformedVector.x, transformedVector.z);
		if (!jumping)
			Jump();
	}

	private Vector3 AdjustedVector(float x, float z) {
		Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
		forward.y = 0;
		forward.Normalize ();
		Vector3 right  = new Vector3(forward.z, 0, -forward.x);
		Vector3 vector = (x * right + z * forward).normalized;
		return vector;
	}
	
	public void SetDirection(float dx, float dz) {
		moveVector.x = dx * moveSpeed * Time.deltaTime;
		moveVector.z = dz * moveSpeed * Time.deltaTime;
		transform.rotation = Quaternion.LookRotation (moveVector);
		transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);
	}

	public void ResetMoveVector() {
		moveVector.x = 0;
		moveVector.z = 0;
	}

	// =========================================================================================
	//	Jump
	// =========================================================================================

	public void Jump() {
		if (!dying) {
			landing = false;
			jumping = true;
			animator.Play("Jump", 0, 0);
			BroadcastMessage("OnJump", SendMessageOptions.DontRequireReceiver);
			moveVector.y = jumpSpeed * Time.deltaTime;
		}
		OnRollEnd();
	}

	public void Land() {
		if (!rolling) {
			boost = Vector3.zero;
			if (!dying) {
				animator.Play("Land", 0, 0);
				BroadcastMessage("OnLand", SendMessageOptions.DontRequireReceiver);
				landing = true;
			}
		} else {
			OnJumpEnd();
		}
	}

	public void DieByCliff() {
		Destroy (gameObject, 1);
		BroadcastMessage("OnDieEnd", SendMessageOptions.DontRequireReceiver);
	}

	// =========================================================================================
	//	Roll
	// =========================================================================================

	public void Roll() {
		rolling = true;
		animator.Play("Roll", 0, 0);
		BroadcastMessage("OnRoll", SendMessageOptions.DontRequireReceiver);
		boost = transform.forward * Time.fixedDeltaTime * moveSpeed * 2;
		OnJumpEnd();
	}

	public void BigJump() {
		bigJumping = true;
		Jump();
		moveVector.y = bigJumpSpeed * Time.fixedDeltaTime;
	}

	public void AllowBigJump() {
		bigJumping = false;
	} 

	// =========================================================================================
	//	Damage
	// =========================================================================================

	public void Damage(int points, Vector3 origin) {
		if (!damaging && !dying) {
			Vector3 direction = (origin - transform.position).normalized;
			if ((direction - Vector3.down).magnitude < 0.1f) {
				int r = Random.Range(0, 4);
				direction.x += r >= 2 ? -1 : 1;
				direction.z += r % 2 == 0 ? -1 : 1;
			}
			SetDirection (direction.x, direction.z);
			moveVector.x *= -2;
			moveVector.z *= -2;
			lifePoints = Mathf.Max(0, lifePoints - points);
			damaging = true;
			jumping = false;
			Jump();
			Invoke("OnDamageEnd", damageTime);
		}
	}

	public void Die() {
		if (!dying) {
			dying = true;
			BroadcastMessage("OnDie", SendMessageOptions.DontRequireReceiver);
			animator.Play("Die");
		}
	}

	// =========================================================================================
	//	Callbacks
	// =========================================================================================

	protected void OnJumpEnd() {
		landing = false;
		jumping = false;
	}

	protected void OnRollEnd() {
		rolling = false;
		if (!bigJumping) {
			boost = Vector3.zero;
		}
	}

	protected void OnDamageEnd() {
		if (lifePoints <= 0) {
			lifePoints = 0;
			Die();
		}
		damaging = false;
	}

	protected void OnDieEnd() {
		dying = false;
	}

	// =========================================================================================
	//	Platform
	// =========================================================================================

	Transform activePlatform;
	Vector3 activeLocalPlatformPoint;
	Vector3 activeGlobalPlatformPoint;

	Quaternion activeLocalPlatformRotation;
	Quaternion activeGlobalPlatformRotation;
	
	private void UpdatePlatform() {

		if (activePlatform != null) {
			Vector3 newGlobalPlatformPoint = activePlatform.TransformPoint(activeLocalPlatformPoint);
			Vector3 moveDistance = (newGlobalPlatformPoint - activeGlobalPlatformPoint);
			if (moveDistance != Vector3.zero)
				controller.Move(moveDistance);

			Quaternion newGlobalPlatformRotation = activePlatform.rotation * activeLocalPlatformRotation;
			Quaternion rotationDiff = newGlobalPlatformRotation * Quaternion.Inverse(activeGlobalPlatformRotation);

			rotationDiff = Quaternion.FromToRotation(rotationDiff * transform.up, transform.up) * rotationDiff;
			transform.rotation = rotationDiff * transform.rotation;
		}
		activePlatform = null;

		controller.Move(moveVector);

		if (activePlatform != null) {
			activeGlobalPlatformPoint = transform.position;
			activeLocalPlatformPoint = activePlatform.InverseTransformPoint (transform.position);
			activeGlobalPlatformRotation = transform.rotation;
			activeLocalPlatformRotation = Quaternion.Inverse(activePlatform.rotation) * transform.rotation; 
		}
	}
	
	protected void OnControllerColliderHit (ControllerColliderHit hit) {
		if (hit.moveDirection.y < -0.9f && hit.normal.y > 0.5f) {
			activePlatform = hit.collider.transform;  
		}
	}

}