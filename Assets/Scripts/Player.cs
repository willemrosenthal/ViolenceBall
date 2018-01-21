using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public float health = 1;
	public GameObject healthBar;

	public int team;
	public int playerNo;

	public Vector2 velocity;

	public bool hasBall;
	public Player passTo = null;
	public float ballCarryHeight = 0.7f;

	public string state = "normal";
	public float stateTimer;

	public GameObject icon;

	// state values
	float runSpeed = 7;



	PlayerAI ai;
	SpriteRenderer renderer;
	ControlManager cm;
	GameManager gm;
	PlayerController pc;
	Ball ball;

	void Start () {
		gm = GameManager.Instance;
		ball = gm.ball;
		ai = GetComponent<PlayerAI> ();
		renderer = GetComponent<SpriteRenderer> ();
		cm = gm.cm [team];
		pc = GetComponent<PlayerController> ();
	}

	void Update () {
		StateTimer ();

		if (state == "normal") {
			pc.speed = runSpeed;
		}
		else if (state == "stun") {
			pc.speed = 0;
		}
		else if (state == "throw") {
			pc.speed = 0;
		}
		else if (state == "attacked") {
			pc.speed = 0;
		}

		pc.Move (velocity); 
	}

	void StateTimer() {
		if (stateTimer > 0) {
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0)
				state = "normal";
		}
	}

	public virtual void LateUpdate () {
		renderer.sortingOrder = (int)Camera.main.WorldToScreenPoint (transform.position).y * -1;
	}

	void OnCollisionEnter2D(Collision2D col) {
		Player enemyCol = col.gameObject.GetComponent<Player> ();
		// if you collide with an enemy player
		if (enemyCol && enemyCol.team != team && !hasBall) {
			// steal ball
			if (enemyCol.hasBall) {
				enemyCol.Stun (this);
			}
		}
	}

	void OnTriggerEnter2D(Collider2D c) {
		Ball ball = c.gameObject.GetComponent<Ball> ();
		if (ball) {
			if (ball.actualPosition.z > renderer.bounds.size.y - 0.3f)
				return;
			// if ball is ok to pick up
			if (!ball.attack && ball.heldBy == null && state != "throw" && state != "stun") {
				ball.AquireBall (this);
				AquireBall ();
			}
			// if ball is in attack mode
			if (ball.attack) {
				ball.Attack (this);
			}
		}   
	}


	public void AquireBall() {
		hasBall = true;
		cm.currentPlayer = playerNo;
		cm.charge = 0;
	}

	public void PrepPass() {
		// find closest player in direction of joystick
		passTo = cm.FindPlayerInDirection(ai.focusDirection, this);
	}

	public void Pass() {
		// if pass is null, find someone to pass to
		while (passTo == null)
			PrepPass ();
		
		gm.ball.Pass (passTo, ballCarryHeight);
		state = "throw";
		stateTimer = 0.1f;
		icon.GetComponent<SpriteRenderer> ().enabled = false;
		RelinquishBall ();
	}

	public void Throw() {
		gm.ball.Throw (ai.focusDirection, cm.charge, ballCarryHeight);
		state = "throw";
		stateTimer = 0.1f;
		RelinquishBall ();
	}

	public void Stun(Player p) {
		if (hasBall) {
			state = "stun";
			stateTimer = 1f;
			RelinquishBall ();
			ball.AquireBall (p);
			p.AquireBall ();
		}
	}

	// call this anytime you stop havin the ball
	void RelinquishBall () {
		cm.charge = 0;
		hasBall = false;
		passTo = null;
	}

	public void TakeDammage(float dammage) {
		health -= dammage;
		healthBar.transform.localScale = new Vector3 (health, 1, 0);
		if (health < 0) {
			health = 0;
			Die ();
		}
	}

	void Die() {
		cm.RemovePlayer (this);
		Destroy (this.gameObject);
	}
	 
}
