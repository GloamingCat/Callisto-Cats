using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StageMenu : MonoBehaviour {

	public static StageMenu instance;
	public static bool paused = false;
	public static bool gameOver = false;

	public Text pauseText;
	public Text gameOverText;
	public Text lifeText;
	public Text manaText;

	public GameObject restartButton;

	void Awake() {
		instance = this;
		paused = false;
		gameOver = false;
	}

	void Update() {
		if (Input.GetButtonDown ("Pause")) {
			if (paused) {
				paused = false;
				Time.timeScale = 1;
				pauseText.text = "";
			} else {
				Time.timeScale = 0;
				paused = true;
				pauseText.text = "Paused";
			}
		}
	}

	public void GameOver() {
		Time.timeScale = 0;
		gameOver = true;
		gameOverText.text = "Game Over";
		restartButton.SetActive (true);
	}

	public void UpdateLifeText(int value) {
		lifeText.text = "Life: " + value;
	}

	public void UpdateManaText(int value) {
		manaText.text = "Mana: " + value;
	}

	public void RestartScene() {
		Time.timeScale = 1;
		//string sceneName = UnityEngine.SceneManagement.SceneManager.
		UnityEngine.SceneManagement.SceneManager.LoadScene (Application.loadedLevel);
	}

}
