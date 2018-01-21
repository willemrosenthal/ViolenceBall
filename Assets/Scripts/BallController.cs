using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour {

	public Vector3 actualPosition;
	public Vector3 velocity;
	public bool grounded;
	public bool attack;

	public bool applyGravity;
	public float gravityScale = 1;

	public float friction = 0.75f;

	public GameObject shadow;

	public bool noMovement;

	public GameManager gm;
	public SpriteRenderer renderer;

	public virtual void Start() {
		gm = GameManager.Instance;
		renderer = GetComponent<SpriteRenderer> ();
	}

	public virtual void LateUpdate () {
		if (!noMovement)
			Move (velocity * Time.deltaTime);
		renderer.sortingOrder = (int)Camera.main.WorldToScreenPoint (transform.position).y * -1;

		// convert actal position to position
		transform.position = new Vector3 (actualPosition.x, actualPosition.y + actualPosition.z, 0);
		if (shadow)
			shadow.transform.position = new Vector3 (actualPosition.x, actualPosition.y, 0);
	}

	void Move(Vector3 moveAmmount) {
		if (actualPosition.z > 0)
			grounded = false;
		
		if (applyGravity) {
			ApplyGravity ();

			actualPosition.z -= moveAmmount.z;

			if (actualPosition.z < 0) {
				velocity.z *= -1;
				actualPosition.z = 0;
				velocity *= friction; // drag
				attack = false;
			}

			GroundedCheck (ref moveAmmount);
		}


		ArenaWalls (ref moveAmmount);


		actualPosition += (Vector3)(Vector2)moveAmmount;
	}

	void ApplyGravity() {
		velocity.z += gm.gravity * Time.deltaTime * gravityScale;
	}

	void GroundedCheck(ref Vector3 moveAmmount) {
		if (Mathf.Abs(actualPosition.z) < 0.05f && velocity.z < 0.3f && velocity.z > -0.3f) {
			velocity.z = 0;
			actualPosition.z = 0;
			moveAmmount.z = 0;
		}
		if (actualPosition.z == 0) {
			grounded = true;
			velocity *= friction; // drag 
		}
	}

	bool ArenaWalls(ref Vector3 moveAmmount) {
		bool hit = false;
		bool goal = false;
		Vector2 newPos = (Vector2)actualPosition + (Vector2)moveAmmount;
		// check if in goal
		if ((newPos.x < gm.arenaWalls.size.x * -0.5f && moveAmmount.x < 0) || (newPos.x > gm.arenaWalls.size.x * 0.5f && moveAmmount.x > 0)) {
			// if inside goal
			if ((newPos.y > gm.goalArea.size.y * -0.5f) && (newPos.y < gm.goalArea.size.y * 0.5f)) {
				goal = true;
			}
			// inside goal
			if ((newPos.y < gm.goalArea.size.y * -0.5f) || (newPos.y > gm.goalArea.size.y * 0.5f)) {
				moveAmmount.y *= -1;
				velocity.y *= -1;
				hit = true;
			}
			if ((newPos.x < gm.goalArea.size.x * -0.5f && moveAmmount.x < 0) || (newPos.x > gm.goalArea.size.x * 0.5f && moveAmmount.x > 0)) {
				moveAmmount.x *= -1;
				velocity.x *= -1;
				hit = true;
			}
		}

		if (!goal) {
			if ((newPos.x < gm.arenaWalls.size.x * -0.5f && moveAmmount.x < 0) || (newPos.x > gm.arenaWalls.size.x * 0.5f && moveAmmount.x > 0)) {
				moveAmmount.x *= -1;
				velocity.x *= -1;
				hit = true;
			}

			if ((newPos.y < gm.arenaWalls.size.y * -0.5f && moveAmmount.y < 0) || (newPos.y > gm.arenaWalls.size.y * 0.5f && moveAmmount.y > 0)) {
				moveAmmount.y *= -1;
				velocity.y *= -1;
				hit = true;
			}
		}
		return hit;
	}

	// actions
	public void ResetAction () {
		actionSet = false;
	}

	bool actionSet;
	float totalTime;
	float actionTimer;
	Vector3 moveToPointDestination;
	Vector3 moveToPointStart;
	public bool MoveToPoint(Vector3 destination, float time, float speed = 0) {
		if (!actionSet) {
			actionSet = true;
			moveToPointDestination = destination;
			moveToPointStart = actualPosition;
			actionTimer = 0;
			totalTime = time;
			if (speed != 0) {
				float d = Vector3.Distance (actualPosition, destination);
				totalTime = d / speed;
			}
		}
		actionTimer += Time.deltaTime;
		float percentComplete = actionTimer / totalTime;
		Vector3 newPos = Vector3.Lerp (moveToPointStart, moveToPointDestination, percentComplete);



		if (percentComplete < 1) {
			velocity = (newPos - actualPosition) / Time.deltaTime;
			velocity.z *= -1;
		}

		Vector3 moveAmmount = velocity * Time.deltaTime;
		if (ArenaWalls(ref moveAmmount)) {
			percentComplete = 1;
		} else {
			actualPosition = newPos;
		}


		if (percentComplete >= 1) {
			return true;
		}
		return false;
	}
		
}
