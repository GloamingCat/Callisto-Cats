using Unity.Netcode;
using UnityEngine;

public class SpawnSpot : MonoBehaviour {

    public GameObject prefab;
    public float interval = 0;
    public float height = 20;
    private GameObject instance;

    private void Start() {
        if (StageManager.mode == 1) {
            NetworkManager.Singleton.OnServerStarted += delegate {
                Spawn();
                transform.Translate(0, height, 0);
            };
        } else if (StageManager.mode == 0) {
            Spawn();
            transform.Translate(0, height, 0);
        } else {
            Destroy(gameObject);
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
