using UnityEngine;
using System.Collections;
using Unity.Netcode;

[RequireComponent(typeof(Character))]
public class Player : MonoBehaviour {
	
	// Singleton
	public static Player instance = null;
	
	// Components
	private Character character;

	// Consts
	public static float bigJumpDelay = 0.01f;
	public AudioClip rollSound;
	public AudioClip jumpSound;
	public AudioClip damageSound;
	public AudioClip spitSound;
	public AudioClip eatSound;
	public AudioClip starSound;

	private void Start() {
		character = GetComponent<Character>();
		StageMenu.instance.UpdateLifeText (character.lifePoints);
		StageMenu.instance.UpdateManaText (character.manaPoints);
	}

    // =========================================================================================
    //	Movement
    // =========================================================================================

    private void FixedUpdate() {
		if (StageMenu.paused)
			return;
		character.UpdateMovement();
		CheckMovement(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
	}

	private void Update() {
		if (StageMenu.paused)
			return;
		CheckShoot(Input.GetButtonDown("Fire"));
		CheckRoll(Input.GetButtonDown("Roll"));
		CheckBigJump(Input.GetButtonDown("Jump"));
	}

	public void CheckMovement(float x, float z) {
		if (character.damaging || character.dying)
			return;
		if (character.boost == Vector3.zero) {
			if (Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0) {
				character.Move(x, z);
			} else {
				character.ResetMoveVector();
			}
		}
	}

	// =========================================================================================
	//	Other input
	// =========================================================================================

	public void CheckShoot(bool pressed) {
		if (character.manaPoints > 0) {
			if (!character.rolling && pressed) {
				Shoot();
			}
		}
	}

	public void CheckRoll(bool pressed) {
		if (character.dying || character.damaging || character.bigJumping)
			return;
		if (!character.rolling && pressed) {
			character.Roll();
			if (rollSound != null)
				AudioSource.PlayClipAtPoint(rollSound, transform.position);
		}
	}

	public void CheckBigJump(bool pressed) {
		if (character.dying || character.damaging || character.bigJumping)
			return;
		if (pressed) {
			character.BigJump();
			if (jumpSound != null) {
				AudioSource.PlayClipAtPoint(jumpSound, transform.position);
			}
		}
	}

	// =========================================================================================
	//	External
	// =========================================================================================

	public void Damage(int points, Vector3 origin) {
		character.Damage(points, origin);
		if (damageSound != null)
			AudioSource.PlayClipAtPoint(damageSound, transform.position);
		StageMenu.instance.UpdateLifeText(character.lifePoints);
	}

	public void EatApple() {
		character.HealLife(2);
		if (eatSound != null)
			AudioSource.PlayClipAtPoint(eatSound, transform.position);
		StageMenu.instance.UpdateLifeText(character.lifePoints);
	}

	public void GrabStar() {
		character.HealMana(1);
		if (starSound != null) {
			AudioSource.PlayClipAtPoint(starSound, transform.position);
		}
		StageMenu.instance.UpdateManaText(character.manaPoints);
	}

	public void Shoot() {
		character.manaPoints--;
		BroadcastMessage("OnShoot");
		if (spitSound != null)
			AudioSource.PlayClipAtPoint(spitSound, transform.position);
		StageMenu.instance.UpdateManaText(character.manaPoints);
	}

	// =========================================================================================
	//	Callbacks
	// =========================================================================================

	protected void OnDieEnd() {
		CameraControl.instance.target = null;
		StageMenu.instance.GameOver();
	}

	protected void OnJumpEnd() {
		character.Invoke("AllowBigJump", bigJumpDelay);
	}

}