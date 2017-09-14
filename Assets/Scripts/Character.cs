using UnityEngine;

public abstract class Character : MonoBehaviour {
	
	public static float gravity = 0.25f;
	public static float damageTime = 0.5f;

	public float moveSpeed;
	public float jumpSpeed;
	
	protected Animator animator;
	protected CharacterController controller;
	
	public Vector3 moveVector = Vector3.zero;
	public bool jumping;
	public bool landing;
	public bool damaging;
	public bool dying;
	
	public int lifePoints;

	protected AudioSource[] sounds;

	protected virtual void Awake() {
		controller = GetComponent<CharacterController> ();
		animator = GetComponent<Animator> ();
		sounds = GetComponents<AudioSource> ();
		jumping = false;
		damaging = false;
		dying = false;
		landing = false;
	}

	protected virtual void FixedUpdate() {
		UpdateMovement();
		UpdateJump();
		UpdatePlatform();
	}

	// =========================================================================================
	//	Movement
	// =========================================================================================

	protected abstract void UpdateMovement ();

	protected virtual void Move(float dx, float dz) {
		Vector3 transformedVector = AdjustedVector (dx, dz);
		SetDirection (transformedVector.x, transformedVector.z);
		if (!jumping)
			Jump();
	}

	protected Vector3 AdjustedVector(float x, float z) {
		Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
		forward.y = 0;
		forward.Normalize ();
		Vector3 right  = new Vector3(forward.z, 0, -forward.x);
		Vector3 vector = (x * right + z * forward).normalized;
		return vector;
	}
	
	protected void SetDirection(float dx, float dz) {
		moveVector.x = dx * moveSpeed * Time.deltaTime;
		moveVector.z = dz * moveSpeed * Time.deltaTime;
		transform.rotation = Quaternion.LookRotation (moveVector);
		transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);
	}

	// =========================================================================================
	//	Jump
	// =========================================================================================

	protected virtual void UpdateJump() {
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
	}

	protected virtual void Jump(bool playSound = true) {
		if (!dying) {
			landing = false;
			jumping = true;
			if (playSound)
				sounds [0].Play ();
			animator.SetTrigger ("jump");
			moveVector.y = jumpSpeed * Time.deltaTime;
		}
	}

	protected virtual void Land() {
		if (!dying) {
			animator.SetTrigger ("land");
			landing = true;
		}
	}

	protected virtual void DieByCliff() {
		Destroy (gameObject, 1);
	}

	// =========================================================================================
	//	External
	// =========================================================================================

	public virtual void Damage(int points, Vector3 origin) {
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

			sounds [1].Play ();
			lifePoints = Mathf.Max(0, lifePoints - points);
			damaging = true;

			jumping = false;
			Jump (false);
			Invoke("OnDamageEnd", damageTime);
		}
	}

	protected virtual void OnDamageEnd() {
		if (lifePoints <= 0) {
			lifePoints = 0;
			Die();
		}
		damaging = false;
	}

	public virtual void Die() {
		if (!dying) {
			dying = true;
			animator.SetTrigger ("die");
		}
	}
	
	// =========================================================================================
	//	Platform
	// =========================================================================================

	Transform activePlatform;
	Vector3 activeLocalPlatformPoint;
	Vector3 activeGlobalPlatformPoint;

	Quaternion activeLocalPlatformRotation;
	Quaternion activeGlobalPlatformRotation;
	
	void UpdatePlatform () {

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

		controller.Move (moveVector);

		if (activePlatform != null) {
			activeGlobalPlatformPoint = transform.position;
			activeLocalPlatformPoint = activePlatform.InverseTransformPoint (transform.position);
			activeGlobalPlatformRotation = transform.rotation;
			activeLocalPlatformRotation = Quaternion.Inverse(activePlatform.rotation) * transform.rotation; 
		}
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit) {
		if (hit.moveDirection.y < -0.9f && hit.normal.y > 0.5f) {
			activePlatform = hit.collider.transform;  
		}
	}


}