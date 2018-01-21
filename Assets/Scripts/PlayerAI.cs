using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAI : MonoBehaviour {

	public Vector2 focusDirection;

	public string position;
	public string ability;

	public Vector2 home;

	public Vector2 pos;
	public Vector2 goal;
	public bool goalInterrupt;
	public bool passTo;

	public bool aiControlled = true;

	GameManager gm;
	ControlManager cm;
	ControlManager cmEnemy;
	Player player;
	Ball ball;
	AICalculations aiCalc;

	PlayerController pc;

	void Start () {
		player = GetComponent<Player> ();
		pc = GetComponent<PlayerController> ();
		gm = GameManager.Instance;
		cm = gm.cm [player.team];
		ball = gm.ball;
		aiCalc = gm.aiCalc;

		int enemyteam = 0;
		if (player.team == 0)
			enemyteam = 1;
		cmEnemy = gm.cm [enemyteam];

		home = (Vector2)transform.position;
		goal = home;
	}

	float maxMoveStep = 5;

	public void AI () {
		// reset movement direction
		pc.movingDir = Vector2.zero;

		// if you have the ball
		if (player.hasBall) {
			HasBall ();
		}
		// if you don't have the ball
		else {
			NoBall ();
		}

		pc.movingDir = pc.movingDir.normalized;

		// move towards goal
		if (!GoalAchieved ()) {
			pc.movingDir = GetDirection (goal);
		}

		/*
		// dont move when ball is being passed to you
		if (ball.passTo == this.player && ball.heldBy == null)
			movingDir = Vector2.zero;

		// if nobody has the ball and you are closest
		if (cm.IsClosetPlayer (this.player) && ball.heldBy == null) {
			AdvanceOnBall ();
			
			// run from ball if it's attacking
			if (ball.attack) {
				if (Vector2.Distance (ball.transform.position, pos) < 3)
					movingDir *= -1;
				if (Vector2.Distance (ball.transform.position, pos) < 4)
					movingDir = Vector2.zero; 
			}

				
		}
		*/
		pc.SetVelocity (pc.movingDir);

	}

	public void AdvanceOnBall() {
		pc.movingDir = TowardsBall ();
	}



	void Move(Vector2 v) {
		transform.position = transform.position + (Vector3)v;
	}


	// AI HELPER EQUATIONS
	// is location on your team's side of the center
	public bool GoalAchieved(float margin = 0.3f) {
		return Vector2.Distance (transform.position, goal) < margin;
	}

	public Vector2 TowardsBall() {
		return new Vector2 (ball.transform.position.x - pc.pos.x, ball.transform.position.y - pc.pos.y ).normalized;
	}

	public bool TeamSide(Vector3 loc,  float margin = 0) {
		if (player.team == 0 && loc.x <= -margin || player.team == 1 && loc.x >= margin)
			return true;
		return false;
	}

	// get enemyGoalDirecton
	public float EnemyGoalDir () {
		if (player.team == 0) {
			return 1;
		}
		return -1;
	}

	// get direction of point
	Vector2 GetDirection(Vector2 point) {
		return new Vector2 (point.x - pc.pos.x, point.y - pc.pos.y ).normalized;
	}
		

	bool IsNearGoalY() {
		return Mathf.Abs (pc.pos.y) < 6;
	}

	void AttackOrPass() {
		focusDirection = GetDirection (aiCalc.goals [player.team]);
		// should add in a check to see if a player is open rather than the random value
		// pass
		if (Random.value < 0.5f) {
			player.PrepPass ();
			player.Pass ();
		}
		// attack
		else {
			Player enemyPlayer = cmEnemy.FindPlayerInDirection(focusDirection, this.player);
			if (aiCalc.ClearShot (player, enemyPlayer.transform.position, 1, true)) {
				focusDirection = GetDirection (enemyPlayer.transform.position);
				cm.charge = 1;
				player.Throw ();
			}
			goalInterrupt = true;
		}
	}

	void Shoot() {
		focusDirection = GetDirection (aiCalc.goals [player.team]);
		cm.charge = 1;
		player.Throw ();
	}

	void GetZone() {
		if (Mathf.Abs (pc.pos.x - aiCalc.goals [player.team].x) < 7)
			zone = "attack";
		else if (TeamSide(pc.pos, 1))
			zone = "defence";
		else zone = "center";
	}


	string zone;
	void HasBall() {
		// if read to make a new descition
		if (GoalAchieved() || goalInterrupt) {
			// find out where on the field you are
			GetZone();


			if (zone == "attack") {
				if (IsNearGoalY()) {
					Shoot ();
				}
				else {
					AttackOrPass ();
				}
			}

			if (zone == "center") {
				// if you can move forwad, do it
				if (aiCalc.ClearMovementPath(player, Vector3.right * EnemyGoalDir(), 1)) {
					goal = pos + Vector2.right * EnemyGoalDir () * 2;
				}
				else {
					AttackOrPass ();
				}
			}

			if (zone == "defence") {
				AttackOrPass ();
			}

			goalInterrupt = true;
		}
	}

	void NoBall () {
		// if you leave the play area
		if (!gm.InPlayArea(pc.pos)) {
			goal = home;
		}

		// if in the play area, you are close and enemy has the ball
		if (cm.IsClosetPlayer(player) && ball.heldBy != null && ball.team != player.team) {
			goal = (Vector2)ball.transform.position;
			goalInterrupt = false;
		}

		// if enemy team has ball and you are NOT the closest player
		if (!cm.IsClosetPlayer (player) && ball.heldBy != null && ball.team != player.team) {
			goal = home;
		}

		// if nobody has the ball, go for it
		if (ball.heldBy == null && gm.InPlayArea(pc.pos)) {
			goal = (Vector2)ball.transform.position;
			goalInterrupt = true;
		}
	}



}
