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
        StageNetwork.mode = 0;
        StageController.killMode = 2;
        SceneManager.LoadScene(1);
    }

    public void Exit() {
        Application.Quit();
    }

    // =========================================================================================
    //	Network
    // =========================================================================================

    public void HostRoom() {
        StageNetwork.mode = 1;
        StageNetwork.port = Int32.Parse(listenPortInput.text);
        StageNetwork.ip = ipInput.text;
        StageNetwork.material = 0;
        StageController.killMode = modeDropdown.value;
        StageController.timeLimit = Single.Parse(timeInput.text);
        SceneManager.LoadScene(1);
    }

    public void JoinRoom() {
        StageNetwork.mode = 2;
        StageNetwork.port = Int32.Parse(portInput.text);
        StageNetwork.ip = ipInput.text;
        StageNetwork.material = 2 + colorDropdown.value;
        SceneManager.LoadScene(1);
    }

}
