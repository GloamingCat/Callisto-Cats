using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class StartMenu : MonoBehaviour
{

    public InputField portInput;
    public InputField ipInput;


    // =========================================================================================
    //	General
    // =========================================================================================

    public void PlaySingle() {
        StageManager.mode = 0;
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
        StageManager.port = Int32.Parse(portInput.text);
        StageManager.ip = ipInput.text;
        SceneManager.LoadScene(1);
    }

    public void JoinRoom() {
        StageManager.mode = 2;
        StageManager.port = Int32.Parse(portInput.text);
        StageManager.ip = ipInput.text;
        SceneManager.LoadScene(1);
    }


}
