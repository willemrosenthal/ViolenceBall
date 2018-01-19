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

	PlayerController pc;
	SpriteRenderer renderer;
	ControlManager cm;
	GameManager gm;

	void Start () {
		gm = GameManager.Instance;
		pc = GetComponent<PlayerController> ();
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
			if ((ball.state == "idle" || ball.state == "pass") && state != "passing" && state != "throwing") {
				ball.BallAquired (this);
				pc.hasBall = true;
				cm.currentPlayer = playerNo;
				cm.charge = 0;
			}
			// if ball is in attack mode
			if (ball.state == "throw" || ball.state == "max-throw" || ball.state == "idle-dangerous") {
				ball.Attack (this);
			}
		}   
	}

	public void PrepPass() {
		// find closest player in direction of joystick
		passTo = cm.FindPlayerInDirection(pc.focusDirection, this);
	}

	public void Pass() {
		gm.ball.Pass (passTo, ballCarryHeight);
		state = "passing";
		pc.hasBall = false;
		stateResetTimer = 0.1f;
		passTo = null;
		icon.GetComponent<SpriteRenderer> ().enabled = false;
		cm.charge = 0;
		// tell ball to bounce-pass to him.
			// calculate mid point
			// set ball on path

		// tell teammate to not move from that spot

		// gain controll once teammate has ball
	}

	public void Throw() {
		gm.ball.Throw (pc.focusDirection, cm.charge, ballCarryHeight);
		state = "throwing";
		pc.hasBall = false;
		stateResetTimer = 0.1f;
		cm.charge = 0;
	}

	void Die() {
		cm.RemovePlayer (this);
		Destroy (this.gameObject);
	}
	 
}
