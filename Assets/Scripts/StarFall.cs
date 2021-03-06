﻿using UnityEngine;
using System.Collections;

public class StarFall : MonoBehaviour {

	public float interval = 10.0f;
	public GameObject starPrefab;
	TerrainData terrain;

	// Use this for initialization
	void Start () {
		GameObject terrainObj = GameObject.FindGameObjectWithTag ("Floor");
		terrain = terrainObj.GetComponent<Terrain> ().terrainData;
		Invoke ("CreateStar", NextStarTime);
	}
	
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
			float x = Random.value * terrain.size.x;
			float y = transform.position.y;
			float z = Random.value * terrain.size.z;
			return new Vector3(x, y ,z);
		}
	}
}
