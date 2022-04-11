using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class StartMenu : MonoBehaviour {

    // Host
    public InputField listenPortInput;
    public InputField timeInput;
    public Dropdown modeDropdown;
    public Toggle respawn;

    // Join
    public InputField ipInput;
    public InputField portInput;
    public Dropdown colorDropdown;

    // =========================================================================================
    //	General
    // =========================================================================================

    public void PlaySingle() {
        Time.timeScale = 1;
        StageManager.mode = 0;
        PlayerInterface.killMode = 2;
        PlayerInterface.timeLimit = -1;
        SceneManager.LoadScene(1);
    }

    public void Exit() {
        Application.Quit();
    }

    // =========================================================================================
    //	Network
    // =========================================================================================

    public void HostRoom() {
        StageManager.mode = 1;
        StageManager.port = Int32.Parse(listenPortInput.text);
        StageManager.ip = ipInput.text;
        StageManager.material = 0;
        PlayerInterface.killMode = modeDropdown.value;
        PlayerInterface.timeLimit = Single.Parse(timeInput.text);
        SceneManager.LoadScene(1);
    }

    public void JoinRoom() {
        StageManager.mode = 2;
        StageManager.port = Int32.Parse(portInput.text);
        StageManager.ip = ipInput.text;
        StageManager.material = 2 + colorDropdown.value;
        SceneManager.LoadScene(1);
    }

}
