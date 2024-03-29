﻿using UnityEngine;

public class StarSpawner : MonoBehaviour {

	public float interval = 10.0f;
	public float height = 20;
	public GameObject starPrefab;

	private TerrainData terrain;

	// Use this for initialization
	void Start () {
		terrain = GetComponent<Terrain>().terrainData;
		Invoke ("CreateStar", NextStarTime);
	}

	// =========================================================================================
	//	Stars
	// =========================================================================================

	void CreateStar() {
		Instantiate (starPrefab, NextStarPlace, starPrefab.transform.rotation);
		Invoke ("CreateStar", NextStarTime);
	}

	float NextStarTime {
		get {
			return interval + (Random.value - 0.5f) * interval;
		}
	}

	Vector3 NextStarPlace {
		get {
			float x = transform.position.x + Random.value * terrain.size.x;
			float y = transform.position.y + height;
			float z = transform.position.z + Random.value * terrain.size.z;
			return new Vector3(x, y ,z);
		}
	}

}
