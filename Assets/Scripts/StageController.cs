using UnityEngine;
using UnityEngine.UI;

public class StageController : MonoBehaviour {

	public static StageController instance;
	private Cat player;

	public bool paused = false;
	public bool gameOver = false;

	public Text centerText;
	public Text lifeText;
	public Text manaText;
	public Text netInfoText;

	public AudioClip rollSound;
	public AudioClip jumpSound;
	public AudioClip damageSound;
	public AudioClip spitSound;
	public AudioClip eatSound;
	public AudioClip starSound;

	private CameraControl mainCamera;
	public Vector3 initialPosition = new Vector3(17.13f, 0.642f, 25);

	private void Awake() {
        instance = this;
    }

    private void Start() {
		netInfoText.text = StageNetwork.GetNetInfo();
		mainCamera = FindObjectOfType<CameraControl>();
	}

	public bool IsLocalPlayer(GameObject obj) {
		return obj == player.gameObject;
	}

	public void SetLocalPlayer(GameObject obj) {
		netInfoText.text = StageNetwork.GetNetInfo();
		player = obj.GetComponent<Cat>();
		player.transform.position = initialPosition;
		mainCamera.target = player.transform;
		UpdateLifeText(player.lifePoints);
		UpdateManaText(player.manaPoints);
	}

	// =========================================================================================
	//	Movement
	// =========================================================================================

	private void FixedUpdate() {
		if (paused || gameOver || player == null)
			return;
		player.UpdateMovement();
		CheckMovement(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
	}

	public void CheckMovement(float x, float z) {
		if (player.damaging || player.dying)
			return;
		if (player.boost == Vector3.zero) {
			if (Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0) {
				player.BounceMove(x, z);
			} else {
				player.ResetMoveVector();
			}
		}
	}

	// =========================================================================================
	//	Other input
	// =========================================================================================

	private void Update() {
		if (player == null)
			return;
		if (Input.GetButtonDown("Pause")) {
			if (StageNetwork.mode == 0) {
				SetPaused(!paused);
			} else {
				player.GetComponent<NetworkCat>().OnPause(!paused);
			}
		}
		if (paused || gameOver)
			return;
		// Shoot
		if (player.manaPoints > 0) {
			if (!player.rolling && Input.GetButtonDown("Fire")) {
				Shoot();
			}
		}
		// Roll
		if (player.dying || player.damaging || player.bigJumping)
			return;
		if (!player.rolling && Input.GetButtonDown("Roll")) {
			player.Roll();
			if (rollSound != null)
				AudioSource.PlayClipAtPoint(rollSound, transform.position);
		}
		if (Input.GetButtonDown("Jump")) {
			player.BigJump();
			if (jumpSound != null) {
				AudioSource.PlayClipAtPoint(jumpSound, transform.position);
			}
		}
	}

	// =========================================================================================
	//	External
	// =========================================================================================

	public void Damage(int points, Vector3 origin) {
		if (player.damaging || player.dying)
			return;
		player.Damage(points, origin);
		if (damageSound != null)
			AudioSource.PlayClipAtPoint(damageSound, transform.position);
		UpdateLifeText(player.lifePoints);
	}

	public void EatApple() {
		player.HealLife(2);
		if (eatSound != null)
			AudioSource.PlayClipAtPoint(eatSound, player.transform.position);
		UpdateLifeText(player.lifePoints);
	}

	public void GrabStar() {
		player.HealMana(1);
		if (starSound != null) {
			AudioSource.PlayClipAtPoint(starSound, player.transform.position);
		}
		UpdateManaText(player.manaPoints);
	}

	public void Shoot() {
		player.manaPoints--;
		StageNetwork.Spawn(0, player.transform, player.GetComponent<NetworkCat>());
		if (spitSound != null)
			AudioSource.PlayClipAtPoint(spitSound, transform.position);
		UpdateManaText(player.manaPoints);
	}

	public void RespawnPlayer() {
		player.transform.position = initialPosition;
		player.ResetState();
		ResetMenu(player.lifePoints, player.manaPoints);
		mainCamera.target = player.transform;
		StageNetwork.RespawnPlayer(player.GetComponent<NetworkCat>());
	}

	// =========================================================================================
	//	Menu
	// =========================================================================================

	public void ResetMenu(int lifePoints, int manaPoints) {
		paused = false;
		gameOver = false;
		centerText.text = "";
		UpdateLifeText(lifePoints);
		UpdateManaText(manaPoints);
	}

	public void SetPaused(bool value) {
		paused = value;
		if (paused) {
			Time.timeScale = 0;
			centerText.text = "Paused";
		} else {
			Time.timeScale = 1;
			centerText.text = "";
		}
	}

	public void GameOver() {
		mainCamera.target = null;
		if (StageNetwork.mode == 0) 
			Time.timeScale = 0;
		gameOver = true;
		centerText.text = "Game Over";
	}

	public void Exit() {
		Time.timeScale = 1;
		StageNetwork.Exit();
    }

	public void UpdateLifeText(int value) {
		lifeText.text = "Life: " + value;
	}

	public void UpdateManaText(int value) {
		manaText.text = "Mana: " + value;
	}

}
