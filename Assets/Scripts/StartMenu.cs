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
        StageNetwork.mode = 0;
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
        StageNetwork.port = Int32.Parse(portInput.text);
        StageNetwork.ip = ipInput.text;
        StageNetwork.material = 0;
        SceneManager.LoadScene(1);
    }

    public void JoinRoom() {
        StageNetwork.mode = 2;
        StageNetwork.port = Int32.Parse(portInput.text);
        StageNetwork.ip = ipInput.text;
        StageNetwork.material = UnityEngine.Random.Range(2, 6);
        SceneManager.LoadScene(1);
    }


}
