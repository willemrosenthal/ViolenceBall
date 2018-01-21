using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public Vector2 pos;

	public float speed = 7;
	public Vector2 movingDir;
	public Vector2 focusDirection;
	public bool AIControlled = false;

	ControlManager cm;
	GameManager gm;
	Player player;
	PlayerAI ai;


	void Start () {
		ai = GetComponent<PlayerAI> ();
		player = GetComponent<Player> ();
		gm = GameManager.Instance;
		cm = gm.cm [player.team];
	}

	public void PlayerControlls(PlayerInputs playerInputs) {
		if (player.state == "attacked")
			return;

		AIControlled = false;

		// movement
		if (Mathf.Abs(playerInputs.LeftStick.Value.x) > 0.3f || Mathf.Abs(playerInputs.LeftStick.Value.y) > 0.3f)
			focusDirection = playerInputs.LeftStick.Value.normalized;

		// throwing
		if (player.hasBall && playerInputs.RightTrigger.IsPressed) {
			if (cm.charge < 1) {
				cm.charge += Time.deltaTime / cm.fullChargeTime;
			} if (cm.charge >= 1) {
				cm.charge = 1;
			}
		}
		if (player.hasBall && playerInputs.RightTrigger.WasReleased) {
			player.Throw ();
		}
		else if (player.hasBall && !playerInputs.RightTrigger.IsPressed) {
			cm.charge = 0;
		}

		// passing
		if (cm.TotalPlayers () > 1) {
			if (playerInputs.AButton.IsPressed && player.hasBall) {
				player.PrepPass ();
			}
			else if (playerInputs.AButton.WasReleased && player.hasBall) {
				player.Pass ();
			}
		}

		// movement
		movingDir = playerInputs.LeftStick.Value.normalized;
		SetVelocity (movingDir);
	}

	public void SetVelocity(Vector3 moveDir) {
		player.velocity = speed * moveDir;
	}

	void Update() {
		pos = (Vector2)transform.position;

		if (AIControlled)
			ai.AI ();
	}

	public void Move(Vector2 velocity) {
		transform.position = transform.position + (Vector3)velocity * Time.deltaTime;
		AIControlled = true;
	}

}
