using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class Player : MonoBehaviour {

	private Character character;
	
	// Fire
	public int maxManaPoints = 20;
	public int manaPoints;
	public GameObject hadouken;

	// Roll
	public AudioClip rollSound;
	public AudioClip jumpSound;
	public AudioClip damageSound;
	public AudioClip spitSound;

	public float bigJumpDelay = 0.01f;

	private void Awake() {
		character = GetComponent<Character>();
    }

	private void Start() {
		manaPoints = maxManaPoints;
		StageMenu.instance.UpdateLifeText (character.lifePoints);
		StageMenu.instance.UpdateManaText (manaPoints);
	}

	// =========================================================================================
	//	Movement
	// =========================================================================================
	
	private void FixedUpdate() {
		if (character.damaging || character.dying)
			return;
		if (character.boost == Vector3.zero) {
			float x = Input.GetAxis("Horizontal");
			float z = Input.GetAxis("Vertical");
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
		if (!StageMenu.paused) {
			CheckFire();
			CheckRoll();
			CheckBigJump();
		}
	}

	private void CheckFire() {
		if (manaPoints > 0) {
			if (!character.rolling && Input.GetButtonDown ("Fire")) {
				if (spitSound != null)
					AudioSource.PlayClipAtPoint(spitSound, transform.position);
				Instantiate(hadouken, transform.position, transform.rotation);
				manaPoints--;
				StageMenu.instance.UpdateManaText(manaPoints);
			}
		}
	}

	private void CheckBigJump() {
		if (character.dying || character.damaging || character.bigJumping)
			return;
		if (Input.GetButtonDown("Jump")) {
			character.BigJump();
			if (jumpSound != null) {
				AudioSource.PlayClipAtPoint(jumpSound, transform.position);
			}
		}
	}

	private void CheckRoll() {
		if (character.dying || character.damaging || character.bigJumping)
			return;
		if (!character.rolling && Input.GetButtonDown("Roll")) {
			character.Roll();
			if (rollSound != null)
				AudioSource.PlayClipAtPoint(rollSound, transform.position);
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

	public bool isVisible(Transform other, float vision) {
		if (character.dying)
			return false;
		float distance = (transform.position - other.position).magnitude;
		return distance < vision;
	}

	public void HealLife(int points) {
		character.lifePoints = Mathf.Min (character.lifePoints + points, character.maxLifePoints);
		StageMenu.instance.UpdateLifeText (character.lifePoints);
	}

	public void HealMana(int points) {
		manaPoints = Mathf.Min (manaPoints + points, maxManaPoints);
		StageMenu.instance.UpdateManaText (manaPoints);
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
