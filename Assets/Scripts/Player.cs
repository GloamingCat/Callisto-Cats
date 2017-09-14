using UnityEngine;
using System.Collections;

public class Player : Character {

	public static Player instance;
	public int maxLifePoints = 30;

	// Fire
	public int maxManaPoints = 20;
	public int manaPoints;
	public GameObject hadouken;

	// Big Jump
	public float bigJumpSpeed;
	public bool bigJumping;
	float bigJumpDelay = 0.01f;

	public bool rolling = false;

	protected override void Awake () {
		instance = this;
		base.Awake ();
	}

	void Start() {
		lifePoints = maxLifePoints;
		manaPoints = maxManaPoints;
		StageMenu.instance.UpdateLifeText (lifePoints);
		StageMenu.instance.UpdateManaText (manaPoints);
	}

	// =========================================================================================
	//	Movement
	// =========================================================================================
	
	protected override void UpdateMovement() {
		if (damaging)
			return;

		if (dying) {
			moveVector.x = 0;
			moveVector.z = 0;
			return;
		}

		if (boost == Vector3.zero) {

			float x = Input.GetAxis ("Horizontal");
			float z = Input.GetAxis ("Vertical");
		
			if (Mathf.Abs (x) > 0 || Mathf.Abs (z) > 0) {
				Move (x, z);
			} else {
				moveVector.x = 0;
				moveVector.z = 0;
			}
		} else {
			moveVector.x = boost.x;
			moveVector.z = boost.z;
		}
	}

	// =========================================================================================
	//	Other input
	// =========================================================================================

	void Update() {
		if (!StageMenu.paused) {
			CheckFire ();
			CheckRoll ();
			CheckBigJump ();
		}
	}

	void CheckFire() {
		if (manaPoints > 0) {
			if (!rolling && Input.GetButtonDown ("Fire")) {
				Instantiate (hadouken, transform.position, transform.rotation);
				manaPoints--;
				StageMenu.instance.UpdateManaText(manaPoints);
			}
		}
	}

	// =========================================================================================
	//	Big Jump
	// =========================================================================================

	void CheckBigJump() {
		if (dying || damaging)
			return;

		if (!bigJumping && Input.GetButtonDown("Jump")) {
			jumping = false;
			bigJumping = true;
			Jump();
			moveVector.y = bigJumpSpeed * Time.fixedDeltaTime;
		}
	}

	protected override void Jump (bool playSound) {
		base.Jump (playSound);
		OnRollEnd ();
	}

	public void OnJumpEnd() {
		landing = false;
		jumping = false;
		Invoke ("AllowBigJump", bigJumpDelay);
	}
	
	void AllowBigJump() {
		bigJumping = false;
	}

	// =========================================================================================
	//	Roll
	// =========================================================================================

	Vector3 boost = Vector3.zero;

	void CheckRoll() {
		if (dying || damaging || bigJumping)
			return;

		if (!rolling && Input.GetButtonDown("Roll")) {
			Roll();
		}
	}

	public void Roll() {
		rolling = true;
		sounds [0].Play ();
		animator.SetTrigger ("roll");
		boost = transform.forward * Time.fixedDeltaTime * moveSpeed * 2;
		OnJumpEnd ();
	}

	public void OnRollEnd() {
		rolling = false;
		if (!bigJumping) {
			boost = Vector3.zero;
		}
	}

	protected override void Land () {
		if (!rolling) {
			boost = Vector3.zero;
			base.Land ();
		} else {
			OnJumpEnd();
		}
	}

	// =========================================================================================
	//	External
	// =========================================================================================

	public void HealLife(int points) {
		lifePoints = Mathf.Min (lifePoints + points, maxLifePoints);
		StageMenu.instance.UpdateLifeText (lifePoints);
	}

	public void HealMana(int points) {
		manaPoints = Mathf.Min (manaPoints + points, maxManaPoints);
		StageMenu.instance.UpdateManaText (manaPoints);
	}

	public override void Damage(int points, Vector3 origin) {
		base.Damage (points, origin);
		StageMenu.instance.UpdateLifeText (lifePoints);
	}

	public void OnDieEnd() {
		dying = false;
		StageMenu.instance.GameOver();
	}

	protected override void DieByCliff () {
		CameraControl.instance.following = false;
		StageMenu.instance.GameOver();
		base.DieByCliff ();
	}


}
