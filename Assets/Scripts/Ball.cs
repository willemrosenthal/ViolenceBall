using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PowerTools;


public class Ball : WorldObject {

	public bool attack;
	public string state;
	public int stage;
	public Player heldBy;
	public int team;

	public GameObject bloodSpirt;

	// passing
	float maxPassDistance = 10;
	float minPassSpeed = 10;
	float speedPerMeter = 1f;
	public Player passTo;
	Vector3 passDestination;
	Vector3 passBouncePoint;
	float passSpeed;

	// throw
	public float powerThrowSpeed = 30;
	public float maxThrowSpeed = 25;
	public float minThrowSpeed = 15;
	Vector3 throwDestination;

	public float dammageTime;
	public float dammage = 0.05f;
	Player attackingPlayer;
	float attackingTimer;
	float attackFrequency = 0.1f;

	public AnimationClip ballNormal;
	public AnimationClip ballDangerous;

	float colliderYOffset;
	BoxCollider2D collider;
	SpriteAnim anim;


	void Awake() {
		GameManager.Instance.ball = this;
	}

	public override void Start () {
		base.Start ();
		collider = GetComponent<BoxCollider2D> ();
		anim = GetComponent<SpriteAnim> ();
		colliderYOffset = collider.offset.y;

		state = "idle";
		actualPosition = (Vector3)(Vector2)transform.position;

		bloodSpirt.GetComponent<ParticleSystem> ().Stop ();
	}

	void Update () {
		// makes ball safe again
		if (grounded && attack)
			attack = false;
		if (!attack && !anim.IsPlaying (ballNormal))
			anim.Play (ballNormal);

		if (state == "held") {
			actualPosition.z = 0.5f;
			transform.position = heldBy.transform.position + Vector3.up * actualPosition.z;
			actualPosition = heldBy.transform.position + Vector3.forward * actualPosition.z;
		} 
		if (state == "pass") {
			if (stage == 0) {
				if (MoveToPoint (passBouncePoint, 0, passSpeed)) {
					attack = false;
					ResetAction ();
					stage++;
				}
			}
			if (stage == 1) {
				if (MoveToPoint (passDestination, 0, passSpeed * friction)) {
					ResetAction ();
					stage++;
				}
			}
			if (stage == 2) {
				state = "idle";
				noMovement = false;
				applyGravity = true;
			}
		}
		if (state == "max-throw") {
			if (MoveToPoint (throwDestination, 0, powerThrowSpeed)) {
				state = "idle-dangerous";
				noMovement = false;
				applyGravity = true;
			}
		}
		if (state == "throw") {
			noMovement = false;
			applyGravity = true;
			if (grounded) {
				state = "idle";
			}
		}
		if (state == "attacking") {
			bloodSpirt.GetComponent<ParticleSystem> ().Play ();
			noMovement = true;
			applyGravity = false;
			attackingPlayer.state = "attacked";
			dammageTime -= Time.deltaTime;

			attackingPlayer.stateResetTimer = 0.1f;

			attackingTimer -= Time.deltaTime;
			if (attackingTimer <= 0) {
				attackingTimer = attackFrequency;
				attackingPlayer.health -= dammage;
				if (attackingPlayer.health <= 0)
					dammageTime = 0;
			}

			if (dammageTime <= 0) {
				bloodSpirt.GetComponent<ParticleSystem> ().Stop ();
				state = "idle-dangerous";
				noMovement = false;
				applyGravity = true;
				attackingPlayer.state = "";
				velocity = velocity * 0.5f;
				velocity.z -= 3 + Random.value * 3;
				dammageTime = 0.4f;
			}
		}
		if (state == "idle-dangerous") {
			if (grounded) {
				state = "idle";
			}
		}

	}

	public override void LateUpdate () {
		base.LateUpdate ();
		collider.offset = new Vector2 (0, colliderYOffset - actualPosition.z);
	}

	public void BallAquired(Player player) {
		team = player.team;
		player.GetComponent<PlayerAI> ().goalInterrupt = true;
		heldBy = player;
		state = "held";
		noMovement = true;
		renderer.enabled = false;
	}
		

	public void Pass(Player destinationPlayer, float passHeight) {
		passTo = destinationPlayer;
		noMovement = true;
		applyGravity = false;
		actualPosition = heldBy.transform.position + Vector3.forward * passHeight;
		state = "pass";
		stage = 0;

		passDestination = passTo.transform.position + Vector3.forward * passTo.ballCarryHeight;
		passBouncePoint = Vector2.Lerp ((Vector2)actualPosition, (Vector2)passDestination, 0.5f);

		float passDistance = Vector2.Distance ((Vector2)actualPosition, (Vector2)passDestination);

		if (passDistance > maxPassDistance) {
			float ratio = maxPassDistance / passDistance;
			passDestination = Vector2.Lerp ((Vector2)actualPosition, (Vector2)passDestination, ratio);
			passDestination.z = passTo.ballCarryHeight;
			passBouncePoint = Vector2.Lerp ((Vector2)actualPosition, (Vector2)passDestination, 0.5f);
		}
			
		passSpeed = minPassSpeed + Vector2.Distance ((Vector2)actualPosition, (Vector2)passDestination) * speedPerMeter;

		renderer.enabled = true;
		//CalculateVelocity (passsTo.transform.position + Vector3.forward * passsTo.ballCarryHeight, true);
		ResetAction ();
		heldBy = null;

		// makes ball dangerous
		grounded = false;
		attack = true;
		anim.Play (ballDangerous);
	}

	public void Throw(Vector2 direction, float power, float passHeight) {
		actualPosition = heldBy.transform.position + Vector3.forward * passHeight;
		// max power
		if (power == 1) {
			throwDestination = (Vector2)actualPosition + direction * 100000;
			state = "max-throw";
			applyGravity = false;
			noMovement = true;
		}
		// not max
		else {
			velocity = (Vector3)direction * (Mathf.Lerp(minThrowSpeed,maxThrowSpeed,power)) + Vector3.forward * power * -4;
			state = "throw";
			applyGravity = true;
			noMovement = false;
		}

		dammageTime = (power * 0.5f + 0.5f);
		stage = 0;
		renderer.enabled = true;
		ResetAction ();
		heldBy = null;


		// makes ball dangerous
		attack = true;
		grounded = false;
		anim.Play (ballDangerous);
	}

	public void Attack(Player hurtPlayer) {
		if (state != "attacking") {
			bloodSpirt.GetComponent<ParticleSystem> ().Play ();
			state = "attacking";
			stage = 0;
			attackingTimer = 0;
			attackingPlayer = hurtPlayer;
		}
	}

	void OnDrawGizmos () {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere (passDestination, 0.25f);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere (passBouncePoint, 0.25f);
	}

}
