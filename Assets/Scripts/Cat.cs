using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Cat : MonoBehaviour {

	// Components
	private Animator animator;
	private CharacterController controller;
	private NetworkCat netCat;

	// Constants
	private static readonly string[] stateAnimations = new string[] { "Idle", "Jump", "Land", "Roll", "Die", "Deaded" };
	public static int maxLifePoints = 30;
	public float gravity = 15f;
	public float damageTime = 2f;
	public float damageSpeed = 2f;
	public float moveSpeed = 5f;
	public float boostSpeed = 10f;
	public float jumpSpeed = 3f;
	public float bigJumpSpeed = 7f;
	public float bigJumpDelay = 0.01f;

	// State
	public int lifePoints;
	private Vector3 moveVector;
	public bool jumping { get; private set; }
	public bool landing { get; private set; }
	public bool damaging { get; private set; }
	public bool invincible { get; private set; }
	public bool dying { get; private set; }
	public bool dead { get; private set; }
	public bool rolling { get; private set; }

	// Big Jump
	public Vector3 boost { get; private set; }
	public bool bigJumping { get; private set; }
	

	// Fire
	public static int maxManaPoints = 20;
	public int manaPoints;

	// Score
	public int killPoints = 0;
	public int diePoints = 0;

	private void Awake() {
		controller = GetComponent<CharacterController>();
		controller.detectCollisions = false;
		animator = GetComponent<Animator>();
		netCat = GetComponent<NetworkCat>();
		ResetState();
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
			if (jumping && !landing && !dying && moveVector.y < 0) { 
				// If was jumping
				Land();
			}
		} else { // if on air
			if (transform.position.y < 0) {
				DieByCliff();
			} else {
				moveVector.y -= gravity * Time.fixedDeltaTime;
			}
		}
		UpdatePlatform();
	}

	// =========================================================================================
	//	State
	// =========================================================================================

	public void ResetState() {
		lifePoints = maxLifePoints;
		manaPoints = maxManaPoints;
		moveVector = Vector3.zero;
		boost = Vector3.zero;
		jumping = false;
		damaging = false;
		invincible = false;
		dying = false;
		dead = false;
		landing = false;
		rolling = false;
		animator.Play("Idle", 0, 0);
	}

	public void SetState(int i) {
		// Idle, Jump, Land, Roll, Die, Deaded, Damage
		if (i == 0) {
			ResetState();
			return;
		}
		if (i < 6)
			animator.Play(stateAnimations[i], 0, 0);
		if (i == 1) {
			jumping = true;
			landing = false;
		} else if (i == 2) {
			jumping = true;
			landing = true;
		} else if (i == 3) {
			rolling = true;
		} else if (i == 4) {
			jumping = false;
			dying = true;
        } else if (i == 5) {
			dying = false;
			dead = true;
        } else if (i == 6) {
			damaging = true;
			invincible = true;
		} else if (i == 7) {
			invincible = false;
		}
	}

	public bool IsVisible(Transform other, float vision) {
		if (dying || dead || damaging || invincible)
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

	// =========================================================================================
	//	Movement
	// =========================================================================================

	public void BounceMove(float dx, float dz) {
		Vector3 transformedVector = AdjustedVector (dx, dz);
		SetDirection (transformedVector.x, transformedVector.z);
		if (!jumping)
			Jump();
	}

	private Vector3 AdjustedVector(float x, float z) {
		Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
		forward.y = 0;
		forward.Normalize();
		Vector3 right  = new Vector3(forward.z, 0, -forward.x);
		Vector3 vector = (x * right + z * forward).normalized;
		return vector;
	}
	
	public void SetDirection(float dx, float dz) {
		moveVector.x = dx * moveSpeed;
		moveVector.z = dz * moveSpeed;
		transform.rotation = Quaternion.LookRotation(moveVector);
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
	}

	public void Move(Vector3 pos) {
		controller.Move(pos);
    }

	public void ResetMoveVector() {
		moveVector.x = 0;
		moveVector.z = 0;
	}

	// =========================================================================================
	//	Jump / Roll
	// =========================================================================================

	public void Jump() {
		if (!dying) {
			SetState(1);
			if (netCat)
				netCat.OnStateChange(1);
			moveVector.y = jumpSpeed;
		}
		OnRollEnd();
	}

	public void Land() {
		if (!rolling) {
			boost = Vector3.zero;
			if (!dying) {
				SetState(2);
				if (netCat)
					netCat.OnStateChange(2);
			}
		} else {
			OnJumpEnd();
		}
	}

	public void DieByCliff() {
		Destroy (gameObject, 1);
		if (netCat)
			netCat.OnStateChange(5);
	}

	public void Roll() {
		SetState(3);
		if (netCat)
			netCat.OnStateChange(3);
		boost = transform.forward * boostSpeed;
		OnJumpEnd();
	}

	public void BigJump() {
		bigJumping = true;
		Jump();
		moveVector.y = bigJumpSpeed;
	}

	public void EndBigJump() {
		bigJumping = false;
	} 

	// =========================================================================================
	//	Damage
	// =========================================================================================

	public void Damage(int points, Vector3 origin) {
		if (damaging || dying || invincible)
			return;
		Vector3 direction = (origin - transform.position).normalized;
		if ((direction - Vector3.down).magnitude < 0.01f) {
			int r = Random.Range(0, 4);
			direction.x += r >= 2 ? -1 : 1;
			direction.z += r % 2 == 0 ? -1 : 1;
		}
		SetDirection (direction.x, direction.z);
		SetState(6);
		if (netCat)
			netCat.OnStateChange(6);
		moveVector.x *= -damageSpeed;
		moveVector.z *= -damageSpeed;
		lifePoints = Mathf.Max(0, lifePoints - points);
		invincible = true;
		Invoke("EndInvincibility", damageTime);
		Jump();
	}

	public void Die() {
		if (!dying) {
			SetState(4);
			if (netCat)
				netCat.OnStateChange(4);
			animator.Play("Die");
		}
	}

	public void EndInvincibility() {
		invincible = false;
		if (netCat)
			netCat.OnStateChange(7);
	}

	// =========================================================================================
	//	Callbacks
	// =========================================================================================

	protected void OnJumpEnd() {
		landing = false;
		jumping = false;
		if (damaging) {
			damaging = false;
			if (lifePoints <= 0) {
				lifePoints = 0;
				Die();
			} else if (netCat)
				netCat.OnStateChange(0);
		}// else if (netCat)
			//netCat.OnStateChange(0);
		Invoke("EndBigJump", bigJumpDelay);
	}

	protected void OnRollEnd() {
		rolling = false;
		if (!bigJumping) {
			boost = Vector3.zero;
		}
	}

	public void OnDieEnd() {
		if (PlayerInterface.instance.IsLocalPlayer(gameObject)) {
			SetState(5);
			if (netCat)
				netCat.OnStateChange(5);
			PlayerInterface.instance.GameOver();
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
	
	private void UpdatePlatform() {

		if (activePlatform != null) {
			Vector3 newGlobalPlatformPoint = activePlatform.TransformPoint(activeLocalPlatformPoint);
			Vector3 moveDistance = (newGlobalPlatformPoint - activeGlobalPlatformPoint);
			controller.Move(moveDistance * Time.fixedDeltaTime);

			Quaternion newGlobalPlatformRotation = activePlatform.rotation * activeLocalPlatformRotation;
			Quaternion rotationDiff = newGlobalPlatformRotation * Quaternion.Inverse(activeGlobalPlatformRotation);

			rotationDiff = Quaternion.FromToRotation(rotationDiff * transform.up, transform.up) * rotationDiff;
			transform.rotation = rotationDiff * transform.rotation;

			if (moveDistance != Vector3.zero && netCat != null)
				netCat.OnMove();
		}
		activePlatform = null;

		controller.Move(moveVector * Time.fixedDeltaTime);

		if (activePlatform != null) {
			activeGlobalPlatformPoint = transform.position;
			activeLocalPlatformPoint = activePlatform.InverseTransformPoint (transform.position);
			activeGlobalPlatformRotation = transform.rotation;
			activeLocalPlatformRotation = Quaternion.Inverse(activePlatform.rotation) * transform.rotation; 
		}

		if (moveVector != Vector3.zero && netCat != null) {
			netCat.OnMove();
		}

	}
	private void OnControllerColliderHit(ControllerColliderHit hit) {
		if (hit.moveDirection.y < -0.9f && hit.normal.y > 0.5f) {
			activePlatform = hit.collider.transform;
		}
	}

}