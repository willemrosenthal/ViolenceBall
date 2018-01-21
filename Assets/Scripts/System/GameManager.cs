using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class GameManager : MonoBehaviour {

	// SINGLETON
	public static GameManager Instance { get; private set; }


	public float pixPerUnit = 32;
	public float pxSize;
	public Vector2 screenSize = new Vector2(400,240);

	public Bounds arenaWalls;
	public Bounds goalArea;

	public float gravity = 12;

	public float fps = 60;

	public int[] score;


	public Ball ball;
	public GameCamera camera;
	public ControlManager[] cm;
	public AICalculations aiCalc;

	void Awake () {
		cm = new ControlManager[2];
		aiCalc = GetComponent<AICalculations> ();

		QualitySettings.vSyncCount = 1;
		Application.targetFrameRate = (int)fps;

		// singleton setup
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else {
			Destroy (gameObject);
		}
		pxSize = 1f / pixPerUnit;

		//Time.timeScale = 0.15f;
	}

	void Start() {
		score = new int[2];
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube (arenaWalls.center, arenaWalls.size);

		float goalSize = (goalArea.size.x - arenaWalls.size.x) * 0.5f;
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube (arenaWalls.center + Vector3.right * (arenaWalls.size.x * 0.5f + goalSize * 0.5f), new Vector3(goalSize, goalArea.size.y));
		Gizmos.DrawWireCube (arenaWalls.center - Vector3.right * (arenaWalls.size.x * 0.5f + goalSize * 0.5f), new Vector3(goalSize, goalArea.size.y));
	}

}
