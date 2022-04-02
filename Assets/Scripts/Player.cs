using UnityEngine;
using System.Collections;
using Unity.Netcode;

[RequireComponent(typeof(Character))]
public class Player : MonoBehaviour {

	public bool localControl = true;
	
	// Components
	private Character character;

	// Consts
	public static float bigJumpDelay = 0.01f;
	public AudioClip rollSound;
	public AudioClip jumpSound;
	public AudioClip damageSound;
	public AudioClip spitSound;

	private void Awake() {
		character = GetComponent<Character>();
    }

	private void Start() {
		StageMenu.instance.UpdateLifeText (character.lifePoints);
		StageMenu.instance.UpdateManaText (character.manaPoints);
	}

	// =========================================================================================
	//	Movement
	// =========================================================================================

	private void FixedUpdate() {
		if (!localControl || StageMenu.paused || character.damaging || character.dying)
			return;
		CheckMovement(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
	}

	public void CheckMovement(float x, float z) {
		if (character.damaging || character.dying)
			return;
		if (character.boost == Vector3.zero) {
			if (Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0) {
				character.Move(x, z);
			} else {
				character.resetMoveVector();
			}
		}
	}

	// =========================================================================================
	//	Other input
	// =========================================================================================

	private void Update() {
		if (!localControl || StageMenu.paused)
			return;
		CheckFire(Input.GetButtonDown("Fire"));
		CheckRoll(Input.GetButtonDown("Roll"));
		CheckBigJump(Input.GetButtonDown("Jump"));
	}

	public void CheckFire(bool pressed) {
		if (character.manaPoints > 0) {
			if (!character.rolling && pressed) {
				if (spitSound != null)
					AudioSource.PlayClipAtPoint(spitSound, transform.position);
				character.Fire();
				character.manaPoints--;
				StageMenu.instance.UpdateManaText(character.manaPoints);
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

	public void HealLife(int points) {
		character.lifePoints = Mathf.Min (character.lifePoints + points, Character.maxLifePoints);
		StageMenu.instance.UpdateLifeText (character.lifePoints);
	}

	public void HealMana(int points) {
		character.manaPoints = Mathf.Min (character.manaPoints + points, Character.maxManaPoints);
		StageMenu.instance.UpdateManaText (character.manaPoints);
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