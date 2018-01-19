using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public float health = 1;
	public GameObject healthBar;

	public int team;
	public int playerNo;

	public LayerMask mask;

	public Player passTo = null;
	public float ballCarryHeight = 0.7f;

	public string state;
	float stateResetTimer;

	public GameObject icon;

	PlayerAI pc;
	SpriteRenderer renderer;
	ControlManager cm;
	GameManager gm;

	void Start () {
		gm = GameManager.Instance;
		pc = GetComponent<PlayerAI> ();
		renderer = GetComponent<SpriteRenderer> ();
		cm = gm.cm [team];
	}

	void Update () {
		StateTimer ();

		if (passTo != null) {
			icon.GetComponent<SpriteRenderer> ().enabled = true;
			icon.transform.position = passTo.transform.position;
		}
		if (health < 0) {
			health = 0;
			Die ();
		}
		healthBar.transform.localScale = new Vector3 (health, 1, 0);
	}

	void StateTimer() {
		if (stateResetTimer > 0) {
			stateResetTimer -= Time.deltaTime;
			if (stateResetTimer <= 0)
				state = "";
		}
	}

	public virtual void LateUpdate () {
		renderer.sortingOrder = (int)Camera.main.WorldToScreenPoint (transform.position).y * -1;
	}

	void OnTriggerEnter2D(Collider2D c) {
		Ball ball = c.gameObject.GetComponent<Ball> ();
		if (ball) {
			if (ball.actualPosition.z > renderer.bounds.size.y - 0.3f)
				return;
			// if ball is ok to pick up
			if (!ball.attack && state != "passing" && state != "throwing") {
				ball.BallAquired (this);
				pc.hasBall = true;
				cm.currentPlayer = playerNo;
				cm.charge = 0;
			}
			// if ball is in attack mode
			if (ball.attack) {
				ball.Attack (this);
			}
		}   
	}

	public void PrepPass() {
		// find closest player in direction of joystick
		passTo = cm.FindPlayerInDirection(pc.focusDirection, this);
	}

	public void Pass() {
		// if pass is null, find someone to pass to
		while (passTo == null)
			PrepPass ();
		
		gm.ball.Pass (passTo, ballCarryHeight);
		state = "passing";
		stateResetTimer = 0.1f;
		icon.GetComponent<SpriteRenderer> ().enabled = false;
		RelinquishBall ();
	}

	public void Throw() {
		gm.ball.Throw (pc.focusDirection, cm.charge, ballCarryHeight);
		state = "throwing";
		stateResetTimer = 0.1f;
		RelinquishBall ();
	}

	// call this anytime you stop havin the ball
	void RelinquishBall () {
		cm.charge = 0;
		pc.hasBall = false;
		passTo = null;
	}

	void Die() {
		cm.RemovePlayer (this);
		Destroy (this.gameObject);
	}
	 
}
