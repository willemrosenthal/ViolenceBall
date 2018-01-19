using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour {

	public int team;
	public int currentPlayer;

	public ChargeMeter meter;
	public float charge;
	public float fullChargeTime = 3;

	List<Player> players;
	Ball ball;

	GameManager gm;
	PlayerInputs playerInputs;

	void OnEnable() {
		playerInputs = PlayerInputs.CreateWithDefaultBindings();
	}

	void Awake() {
		gm = GameManager.Instance;
		gm.cm [team] = this;
	}

	void Start() {
		BuildPlayerList ();
		ball = FindObjectOfType<Ball> ();
	}

	void Update () {
		if (ball.state == "idle" || ball.heldBy != players [currentPlayer])
			GetClosestPlayer ();
		players [currentPlayer].GetComponent<PlayerController> ().PlayerControlls (playerInputs);
	}
	 
	void GetClosestPlayer() {
		float dist = Vector2.Distance (players [currentPlayer].transform.position, ball.actualPosition);
		for (int i = 0; i < players.Count; i++) {
			if (dist > Vector2.Distance (players [i].transform.position, ball.actualPosition)) {
				dist = Vector2.Distance (players [i].transform.position, ball.actualPosition);
				currentPlayer = i;
			}
		}
	}

	void BuildPlayerList () {
		players = new List<Player> ();
		Player[] playerArray = (Player[]) GameObject.FindObjectsOfType (typeof(Player));

		for (int i = 0; i < playerArray.Length; i++) {
			if (playerArray[i].team == team) {
				players.Add (playerArray [i]);
				playerArray [i].playerNo = i;
			}
		}
	}

	float angleRange = 30;
	public Player FindPlayerInDirection(Vector2 dir, Player origin) {
		int closest = 0;
		// player with ball position
		Vector2 originPoint = origin.transform.position;
		// desired throw direction
		float goalDegree = Vector2.SignedAngle (Vector2.up, dir);

		if (players [closest] == origin)
			closest++;
			
		float lastBestDegree = Vector2.SignedAngle (Vector2.up, new Vector2(players[closest].transform.position.x - originPoint.x, players[closest].transform.position.y - originPoint.y).normalized);
		float compareDegree;

		for (int i = 0; i < players.Count; i++) {
			if (players [i] == origin)
				continue;
			
			compareDegree = Vector2.SignedAngle (Vector2.up, new Vector2(players[i].transform.position.x - originPoint.x, players[i].transform.position.y - originPoint.y).normalized);
			float angleA = Mathf.Abs (compareDegree - goalDegree);
			float angleB = Mathf.Abs (lastBestDegree - goalDegree);

			if (angleA < angleB || angleA < angleRange) {
				float distanceA = Vector2.Distance (players [i].transform.position, origin.transform.position);
				float distanceB = Vector2.Distance (players [closest].transform.position, origin.transform.position);

				if ((angleA < angleRange && distanceB < distanceA)) {
					
				}
				else {
					lastBestDegree = compareDegree;
					closest = i;
				}
			}
		}

		return players[closest];
	}

	public void RemovePlayer(Player playerToRemove) {
		bool removed = false;
		for (int i = 0; i < players.Count; i++) {
			if (players [i] == playerToRemove) {
				players.RemoveAt (i);
				removed = true;
			}
			if (removed && i < players.Count)
				players [i].playerNo--;
		}
		if (currentPlayer >= players.Count)
			currentPlayer = 0;
	}

}
