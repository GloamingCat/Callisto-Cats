using UnityEngine;
using UnityEngine.UI;

public class StageMenu : MonoBehaviour {

	public bool paused = false;
	public bool gameOver = false;

	public Text centerText;
	public Text lifeText;
	public Text manaText;
	public Text netInfoText;

    private void Start() {
		netInfoText.text = StageManager.GetNetInfo();
	}

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
		if (StageManager.mode == 0) 
			Time.timeScale = 0;
		gameOver = true;
		centerText.text = "Game Over";
	}

	public void Exit() {
		Time.timeScale = 1;
		StageManager.Exit();
    }

	public void UpdateLifeText(int value) {
		lifeText.text = "Life: " + value;
	}

	public void UpdateManaText(int value) {
		manaText.text = "Mana: " + value;
	}

}
