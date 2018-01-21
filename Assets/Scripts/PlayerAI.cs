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
	public bool goalInterrupt;
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
	ControlManager cmEnemy;
	Player player;
	Ball ball;
	AICalculations aiCalc;


	void Start () {
		player = GetComponent<Player> ();
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

	void AI () {
		// reset movement direction
		movingDir = Vector2.zero;

		// freeze if being attacked
		if (player.state == "attacked")
			return;

		// if you have the ball
		if (hasBall) {
			AIHasBall ();
		}
		// if you don't have the ball
		else {
			AINoBall ();
		}

		movingDir = movingDir.normalized;

		// move towards goal
		if (!GoalAchieved ()) {
			movingDir = GetDirection (goal);
		}

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

	// get enemyGoalDirecton
	public float EnemyGoalDir () {
		if (player.team == 0) {
			return 1;
		}
		return -1;
	}

	// get direction of point
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



	bool IsNearGoalX() {
		return Mathf.Abs (pos.x - aiCalc.goals [player.team].x) < 7;
	}
	bool IsNearGoalY() {
		return Mathf.Abs (pos.y) < 6;
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
		if (Mathf.Abs (pos.x - aiCalc.goals [player.team].x) < 7)
			zone = "attack";
		else if (TeamSide(pos, 1))
			zone = "defence";
		else zone = "center";
	}


	string zone;
	void AIHasBall() {
		// if read to make a new descition
		if (GoalAchieved() || goalInterrupt) {
			// find out where on the field you are
			GetZone();
			Debug.Log (zone);
			Time.timeScale = 0.5f;

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

			goalInterrupt = false;
		}
	}

	void AINoBall () {
		// if read to make a new descition
		if (GoalAchieved() || goalInterrupt) {
			goal = home;
			goalInterrupt = false;
		}

		if (cm.IsClosetPlayer(player)) {
			goal = (Vector2)ball.transform.position;
			goalInterrupt = true;
		}
	}



}
