using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {

    public string typeName = "Enemy";
    public int prefabId = 1;
    public float interval = 0;
    private GameObject[] spots;
    private GameObject[] instances;

    private void Awake() {
        if (StageManager.mode == 2)
            Destroy(this);
    }

    private void Start() {
        spots = GameObject.FindGameObjectsWithTag(typeName + "Spot");
        if (StageManager.mode == 0)
            SpawnInitialEnemies();
        else
            GetComponent<NetworkManager>().OnServerStarted += SpawnInitialEnemies;
    }

    private void SpawnInitialEnemies() {
        foreach (GameObject spot in spots) {
            StageManager.Spawn(prefabId, spot.transform, null);
            spot.transform.Translate(0, 20, 0);
        }
        instances = GameObject.FindGameObjectsWithTag(typeName);
    }

    private void Update() {
        if (instances == null)
            return;
        foreach (GameObject instance in instances) {
            if (instance == null) {
                Invoke("CreateEnemy", NextEnemyTime);
                break;
            }
        }
    }

    private void CreateEnemy() {
        StageManager.Spawn(prefabId, spots[Random.Range(0, spots.Length)].transform, null);
        instances = GameObject.FindGameObjectsWithTag(typeName);
    }

    private float NextEnemyTime {
        get {
            return interval + (Random.value - 0.5f) * interval;
        }
    }

}
