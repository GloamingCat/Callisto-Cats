using UnityEngine;

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

	private StageMenu menu;
	private CameraControl mainCamera;
	private Spawner spawner;

	private void Start() {
		instance = this;
		character = GetComponent<Character>();
		mainCamera = FindObjectOfType<CameraControl>();
		mainCamera.target = transform;
		menu = FindObjectOfType<StageMenu>();
		menu.UpdateLifeText (character.lifePoints);
		menu.UpdateManaText (character.manaPoints);
		spawner = FindObjectOfType<Spawner>();
	}

    // =========================================================================================
    //	Movement
    // =========================================================================================

    private void FixedUpdate() {
		if (menu.paused || menu.gameOver)
			return;
		character.UpdateMovement();
		CheckMovement(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
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

	private void Update() {
		if (Input.GetButtonDown("Pause")) {
			bool value = !menu.paused;
			if (StageManager.mode == 0) {
				SetPaused(value);
			} else {
				BroadcastMessage("OnPause", value, SendMessageOptions.DontRequireReceiver);
			}
		}
		if (menu.paused || menu.gameOver)
			return;
		// Shoot
		if (character.manaPoints > 0) {
			if (!character.rolling && Input.GetButtonDown("Fire")) {
				Shoot();
			}
		}
		// Roll
		if (character.dying || character.damaging || character.bigJumping)
			return;
		if (!character.rolling && Input.GetButtonDown("Roll")) {
			character.Roll();
			if (rollSound != null)
				AudioSource.PlayClipAtPoint(rollSound, transform.position);
		}
		if (Input.GetButtonDown("Jump")) {
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
		menu.UpdateLifeText(character.lifePoints);
	}

	public void EatApple() {
		character.HealLife(2);
		if (eatSound != null)
			AudioSource.PlayClipAtPoint(eatSound, transform.position);
		menu.UpdateLifeText(character.lifePoints);
	}

	public void GrabStar() {
		character.HealMana(1);
		if (starSound != null) {
			AudioSource.PlayClipAtPoint(starSound, transform.position);
		}
		menu.UpdateManaText(character.manaPoints);
	}

	public void Shoot() {
		character.manaPoints--;
		spawner.Spawn(0, transform);
		if (spitSound != null)
			AudioSource.PlayClipAtPoint(spitSound, transform.position);
		menu.UpdateManaText(character.manaPoints);
	}

	public void Respawn() {
		character.ResetState();
		menu.ResetMenu(character.lifePoints, character.manaPoints);
		mainCamera.target = transform;
	}

	public void SetPaused(bool value) {
		menu.SetPaused(value);
    }

	// =========================================================================================
	//	Callbacks
	// =========================================================================================

	protected void OnDieEnd() {
		mainCamera.target = null;
		menu.GameOver();
	}

	protected void OnJumpEnd() {
		character.Invoke("AllowBigJump", bigJumpDelay);
	}

}