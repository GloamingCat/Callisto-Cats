using UnityEngine;
using UnityEngine.UI;

public class DisableOnWebGL : MonoBehaviour {

#if UNITY_WEBGL
    void Awake() {
        Button button = GetComponent<Button>();
        if (button)
            button.interactable = false;
        else
            Destroy(gameObject);
    }
#endif

}