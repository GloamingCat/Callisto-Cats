using Unity.Netcode;
using UnityEngine;

public class SpawnSpot : MonoBehaviour {

    public GameObject prefab;
    public float interval = 0;
    public float height = 20;
    public bool mvpOnly = false;
    private GameObject instance;

    private void Start() {
        if (StageManager.mode == 2 || mvpOnly && PlayerInterface.killMode == 1) {
            Destroy(gameObject);
        } else {
            Spawn();
            transform.Translate(0, height, 0);
        }
    }

    private void Spawn() {
        instance = Instantiate(prefab, transform.position, transform.rotation);
        AutoRespawn autoRespawn = instance.AddComponent<AutoRespawn>();
        autoRespawn.spot = this;
        if (StageManager.mode == 1)
            instance.GetComponent<NetworkObject>().Spawn();
    }

    public void TriggerRespawn() {
        Invoke("Spawn", NextSpawnTime);
    }

    private float NextSpawnTime {
        get {
            return interval + (Random.value - 0.5f) * interval;
        }
    }

}
