using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAI : MonoBehaviour {

	public bool hasBall;
	public Vector2 focusDirection;

	public string position;
	public string ability;

	public Vector2 home;

	public Vector2 pos;
	public Vector2 goal;
	bool goalInterrupt;
	public bool passTo;

	public bool aiControlled = true;

	float speed = 7;
	float jetSpeed = 14;

	Vector2 velocity;

	bool jetBoost = false;
	Vector2 jetSteer;
	float smoothJetVelocity;

	Vector2 movingDir;

	GameManager gm;
	ControlManager cm;
	Player player;
	Ball ball;


	void Start () {
		player = GetComponent<Player> ();
		gm = GameManager.Instance;
		cm = gm.cm [player.team];
		ball = gm.ball;

		home = (Vector2)transform.position;
	}


	void AI () {
		movingDir = Vector2.zero;

		if (player.state == "attacked")
			return;

		// offence, get ball and move forward with other offensive players. -- if ball is in offence area, keep pressing on, if ball is in def area, try to stay open for a pass near the middle
		if (position == "forward") {

			// if you reached your destination, make a new goal
			if (GoalAchieved() || goalInterrupt) {
				// choose where to go next

				// if ball is towards enemy's goal
				if (!TeamSide (ball.transform.position, 4)) {
					// move in a straight line towards ball
					goal = new Vector2 (ball.transform.position.x, home.y) + Vector2.right * 4 * EnemyGoalDir ();
				}
				// if ball is on our side of the courte, retreat
				else {
					goal = home;
				}
				goalInterrupt = false;
			}

		}

		// move towards goal
		if (!GoalAchieved ()) {
			movingDir = GetDirection (goal);
		}


		// dont move when ball is being passed to you
		if (ball.passTo == this.player && ball.heldBy == null)
			movingDir = Vector2.zero;

		// if nobody has the ball and you are closest
		if ( cm.IsClosetPlayer(this.player) && ball.heldBy == null )
			AdvanceOnBall ();

		velocity = movingDir * speed;
	}

	public void AdvanceOnBall() {
		movingDir = TowardsBall ();
		velocity = speed * (Vector3)movingDir;
	}

	public void PlayerControlls(PlayerInputs playerInputs) {
		if (player.state == "attacked")
			return;

		pos = (Vector2)transform.position;
		aiControlled = false;

		// movement
		if (Mathf.Abs(playerInputs.LeftStick.Value.x) > 0.3f || Mathf.Abs(playerInputs.LeftStick.Value.y) > 0.3f)
			focusDirection = playerInputs.LeftStick.Value.normalized;

		// throwing
		if (hasBall && playerInputs.RightTrigger.IsPressed) {
			if (cm.charge < 1) {
				cm.charge += Time.deltaTime / cm.fullChargeTime;
			} if (cm.charge >= 1) {
				cm.charge = 1;
			}
		}
		if (hasBall && playerInputs.RightTrigger.WasReleased) {
			player.Throw ();
		}
		else if (hasBall && !playerInputs.RightTrigger.IsPressed) {
			cm.charge = 0;
		}

		// passing
		if (cm.TotalPlayers () > 1) {
			if (playerInputs.AButton.IsPressed && hasBall) {
				player.PrepPass ();
			}
			else if (playerInputs.AButton.WasReleased && hasBall) {
				player.Pass ();
			}
		}

		// movement
		movingDir = playerInputs.LeftStick.Value.normalized;
		velocity = speed * (Vector3)movingDir;

		// Player Abilitites
		if (ability == "jet boost")
			JetBoost (playerInputs);
	}

	void Update () {
		pos = (Vector2)transform.position;

		if (aiControlled)
			AI ();
		
		Move (velocity * Time.deltaTime);
		velocity = Vector3.zero;

		// reset player controlled val
		aiControlled = true;
	}


	void Move(Vector2 v) {
		transform.position = transform.position + (Vector3)v;
	}

	// player specific abilitites
	void JetBoost(PlayerInputs playerInputs) {
		if (playerInputs.XButton.IsPressed) {
			velocity = jetSpeed * (Vector3)movingDir;

			// steering
			float cDegree = Vector2.SignedAngle (Vector2.up, movingDir);
			float degree = Vector2.SignedAngle (Vector2.up, playerInputs.LeftStick.Value.normalized);

			cDegree = Mathf.SmoothDampAngle (cDegree, degree, ref smoothJetVelocity, 0.75f);

			movingDir = (Vector2)(Quaternion.Euler (0, 0, cDegree) * Vector2.up);
		}
	}

	// AI HELPER EQUATIONS
	// is location on your team's side of the center
	public bool GoalAchieved(float margin = 0.3f) {
		return Vector2.Distance (transform.position, goal) < margin;
	}

	public Vector2 TowardsBall() {
		return new Vector2 (ball.transform.position.x - pos.x, ball.transform.position.y - pos.y ).normalized;
	}

	public bool TeamSide(Vector3 loc,  float margin = 0) {
		if (player.team == 0 && loc.x <= -margin || player.team == 1 && loc.x >= margin)
			return true;
		return false;
	}
	// is location on your team's side of the home position
	public bool TeamSideFromHome(Vector3 loc) {
		if (player.team == 0 && loc.x <= home.x || player.team == 1 && loc.x >= home.x)
			return true;
		return false;
	}
	// ball ahead
	public bool BallAhead() {
		if (ball.transform.position.x > pos.x && player.team == 0)
			return true;
		else if (ball.transform.position.x < pos.x && player.team == 1)
			return true;
		return false;
	}
	// ball within distance
	public bool BallWithinDistance(float dist) {
		return (Vector2.Distance (ball.transform.position, pos) <= dist);
	}
	// get enemyGoalDirecton
	public float EnemyGoalDir () {
		if (player.team == 0) {
			return 1;
		}
		return -1;
	}

	Vector2 GetDirection(Vector2 point) {
		return new Vector2 (point.x - pos.x, point.y - pos.y ).normalized;
	}

	// POSITION LIST
	public string[] positionList = new string[]
	{
		"forward",
		"striker",
		"defence"
	};
	// ABILITY LIST
	public string[] abilityList = new string[]
	{
		"none",
		"jet bootst"
	};



}
