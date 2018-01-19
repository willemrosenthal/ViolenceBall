using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour {

	public int team = 0;
	public int numSpacing = 1;
	public Sprite[] numbers;
	GameObject[] score;
	SpriteRenderer[] numRenderer;
	int[] currentScore;

	GameManager gm;

	void Start () {
		gm = GameManager.Instance;
		Setup ();
	}

	void Setup() {
		currentScore = new int[2];
		score = new GameObject[2];
		numRenderer = new SpriteRenderer[2];

		// number 1
		score [0] = new GameObject ();
		score [0].transform.parent = transform;
		score [0].transform.localPosition = new Vector3 ();
		score [0].AddComponent<SpriteRenderer> ();
		numRenderer [0] = score [0].GetComponent<SpriteRenderer> ();
		numRenderer [0].sortingLayerName = "HUD";

		// number 2
		score [1] = new GameObject ();
		score [1].transform.parent = transform;
		score [1].transform.localPosition = new Vector3 () + Vector3.right * gm.pxSize * (numSpacing + numbers [0].rect.width);
		score [1].AddComponent<SpriteRenderer> ();
		numRenderer [1] = score [1].GetComponent<SpriteRenderer> ();
		numRenderer [1].sortingLayerName = "HUD";

		UpdateNumbers ();
	}

	void Update () {
		if (currentScore[team] != gm.score[team])
			UpdateNumbers ();
	}

	void UpdateNumbers() {
		float tens = Mathf.Floor((float)gm.score [team] * 0.1f);
		float ones = (float)gm.score [team] - (tens * 10);
		numRenderer [0].sprite = numbers [(int)tens];
		numRenderer [1].sprite = numbers [(int)ones];
		currentScore[team] = gm.score[team];
	}
}
