using System;
using TMPro;
using UnityEngine;

public class StageController : MonoBehaviour {

	public static StageController instance;
	private Cat player;
	private CameraControl mainCamera;
	public Vector3 initialPosition = new Vector3(17.13f, 0.642f, 25);

	// State
	public bool paused = false;
	public bool gameOver = false;
	public float startTime = 0;

	// Initial Menu Params (gameplay)
	public static int killMode = 0;
	public static float timeLimit = 300;
	public static bool respawn = true;

	// Menu
	public TextMeshProUGUI centerText;
	public TextMeshProUGUI lifeText;
	public TextMeshProUGUI manaText;
	public TextMeshProUGUI netInfoText;
	public TextMeshProUGUI countdownText;
	public GameObject respawnButton;

	// Audio
	public AudioClip rollSound;
	public AudioClip jumpSound;
	public AudioClip damageSound;
	public AudioClip spitSound;
	public AudioClip eatSound;
	public AudioClip starSound;

	private void Awake() {
        instance = this;
    }

    private void Start() {
		mainCamera = FindObjectOfType<CameraControl>();
		startTime = Time.time;
		countdownText.text = "";
		respawnButton.SetActive(false);
		netInfoText.text = StageNetwork.GetNetInfo();
	}

	public bool IsLocalPlayer(GameObject obj) {
		return obj == player.gameObject;
	}

	public void SetLocalPlayer(GameObject obj) {
		player = obj.GetComponent<Cat>();
		player.transform.position = initialPosition;
		mainCamera.target = player.transform;
		UpdateLifeText(player.lifePoints);
		UpdateManaText(player.manaPoints);
		netInfoText.text = StageNetwork.GetNetInfo();
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
		if (!gameOver && Input.GetButtonDown("Pause")) {
			if (StageNetwork.mode == 0) {
				SetPaused(!paused);
			} else {
				player.GetComponent<NetworkCat>().OnPause(!paused);
			}
		}
		if (!paused && timeLimit >= 0) {
			UpdateCountdown();
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

	private void UpdateCountdown() {
		timeLimit -= (Time.time - startTime);
		startTime = Time.time;
		if (timeLimit <= 0) {
			timeLimit = 0;
			GameOver(true);
        }
		var ts = TimeSpan.FromSeconds(timeLimit);
		countdownText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
	}

	public void SetPaused(bool value) {
		paused = value;
		if (paused) {
			Time.timeScale = 0;
			centerText.text = "PAUSED";
			if (timeLimit >= 0)
				timeLimit -= Time.time - startTime;
		} else {
			Time.timeScale = 1;
			centerText.text = "";
			if (timeLimit >= 0)
				startTime = Time.time;
			netInfoText.text = StageNetwork.GetNetInfo();
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
			AudioSource.PlayClipAtPoint(spitSound, player.transform.position);
		UpdateManaText(player.manaPoints);
	}

	public void RespawnPlayer() {
		if (!paused)
			Time.timeScale = 1;
		respawnButton.SetActive(false);
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

	public void GameOver(bool timeout = false) {
		mainCamera.target = null;
		gameOver = true;
		if (timeout) {
			centerText.text = "TIMEOUT";
			respawnButton.SetActive(false);
			if (StageNetwork.mode == 0) {
				Time.timeScale = 0;
			} else if (StageNetwork.mode == 1) {
				player.GetComponent<NetworkCat>().GameOverClientRpc(true);
			}
		} else {
			centerText.text = "YOU DEADED";
			if (respawn) {
				respawnButton.SetActive(true);
			} else {
				foreach (Cat cat in FindObjectsOfType<Cat>()) {
					if (!cat.dead && cat.gameObject.CompareTag("Player")) {
						return;
					}
				}
				if (StageNetwork.mode == 1) {
					player.GetComponent<NetworkCat>().GameOverClientRpc(false);
				} else {
					player.GetComponent<NetworkCat>().GameOverServerRpc();
				}
			}
         }
	}

	public void PartyGameOver(bool timeout = false) {
		mainCamera.target = null;
		gameOver = true;
		Time.timeScale = 0;
		centerText.text = timeout ? "TIMEOUT" : "EVERYBODY DEADED";
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
