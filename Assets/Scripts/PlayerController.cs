using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public bool hasBall;

	public Vector2 focusDirection;

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


	void Start () {
		player = GetComponent<Player> ();
		gm = GameManager.Instance;
		cm = gm.cm [player.team];
	}

	public void PlayerControlls(PlayerInputs playerInputs) {
		if (player.state == "attacked")
			return;
		
		// CHARGE SHOT
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



		if (playerInputs.XButton.IsPressed) {
			jetBoost = true;
		} else {
			jetBoost = false;
		}

		// movement
		if (Mathf.Abs(playerInputs.LeftStick.Value.x) > 0.3f || Mathf.Abs(playerInputs.LeftStick.Value.y) > 0.3f)
			focusDirection = playerInputs.LeftStick.Value.normalized;

		movingDir = playerInputs.LeftStick.Value.normalized;
		velocity = speed * (Vector3)movingDir;

		/*
		if (jetBoost) {
			velocity = jetSpeed * (Vector3)movingDir;

			// steering
			float cDegree = Vector2.SignedAngle (Vector2.up, movingDir);
			float degree = Vector2.SignedAngle (Vector2.up, playerInputs.LeftStick.Value.normalized);

			cDegree = Mathf.SmoothDampAngle (cDegree, degree, ref smoothJetVelocity, 0.75f);

			movingDir = (Vector2)(Quaternion.Euler(0,0,cDegree) * Vector2.up);
		}
		*/

		if (playerInputs.AButton.IsPressed && hasBall) {
			player.PrepPass ();
		}
		if (playerInputs.AButton.WasReleased && hasBall) {
			player.Pass ();
		}
	}

	void Update () {
		Move (velocity * Time.deltaTime);
		velocity = Vector3.zero;
	}


	void Move(Vector2 v) {
		transform.position = transform.position + (Vector3)v;
	}
}
