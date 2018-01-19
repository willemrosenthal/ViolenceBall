using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {

	public int team;
	BoxCollider2D collider;
	GameManager gm;

	void Start () {
		gm = GameManager.Instance;
		collider = GetComponent<BoxCollider2D> ();
	}

	void OnTriggerEnter2D(Collider2D c) {
		Ball ball = c.gameObject.GetComponent<Ball> ();
		if (ball) {
			gm.score [team]++;
		}   
	}

	void OnDrawGizmos() {
		if (collider == null)
			collider = GetComponent<BoxCollider2D> ();
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube (collider.bounds.center, collider.bounds.size);

		Gizmos.color = Color.magenta;
		Gizmos.DrawWireCube (collider.bounds.center, collider.bounds.size * 0.99f);
	}
}
