using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class StageMenu : MonoBehaviour {

	public static StageMenu instance;
	public static bool paused = false;
	public static bool gameOver = false;

	public Text centerText;
	public Text lifeText;
	public Text manaText;
	public Text netInfoText;
	public Text exitText;

	private void Awake() {
		instance = this;
		paused = false;
		gameOver = false;
	}

	private void Update() {
		if (StageManager.mode != 0) {
        }
		if (gameOver)
			return;
		if (Input.GetButtonDown ("Pause")) {
			if (StageManager.mode == 0) {
				SetPaused(!paused);
			} else {
				Player.instance.GetComponent<NetworkPlayer>().PauseServerRpc(!paused);
			}
		}
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
		if (StageManager.mode == 0) 
			Time.timeScale = 0;
		gameOver = true;
		centerText.text = "Game Over";
	}

	public void Exit() {
		Time.timeScale = 1;
		if (StageManager.mode == 0) {
			Destroy(NetworkManager.Singleton.gameObject);
			SceneManager.LoadScene(0);
		} else if (StageManager.mode == 1) {
			GameObject[] cats = GameObject.FindGameObjectsWithTag("Player");
			foreach (GameObject cat in cats) {
				cat.GetComponent<NetworkObject>().Despawn();
				Destroy(cat);
			}
		} else {
			StageManager.mode = 0;
			NetworkManager.Singleton.Shutdown();
		}
	}

	public void UpdateLifeText(int value) {
		lifeText.text = "Life: " + value;
	}

	public void UpdateManaText(int value) {
		manaText.text = "Mana: " + value;
	}

}
