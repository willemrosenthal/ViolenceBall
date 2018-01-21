using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICalculations : MonoBehaviour {

	public List<List<Player>> players;
	public List<Vector2> goals;

	void Awake () {
		// get teams
		players = new List<List<Player>> ();
		players.Add (new List<Player> ());
		players.Add (new List<Player> ());
	}

	void Start() {
		// get gaol locations
		Goal[] goalArray = (Goal[]) GameObject.FindObjectsOfType (typeof(Goal));
		goals = new List<Vector2> ();

		if (goalArray [0].transform.position.x < 0) {
			goals.Add (goalArray [1].transform.position);
			goals.Add (goalArray [0].transform.position);
		}
		else if (goalArray [1].transform.position.x < 0) {
			goals.Add (goalArray [0].transform.position);
			goals.Add (goalArray [1].transform.position);
		}
	}
	
	public Vector2 AdvanceOnGoal(int team, Vector2 pos, float distanceStep) {
		//float dir = GetGoalDir(team);
		//Vector2 newPos = pos + Vector2.right * distanceStep * dir;
		// get the ball


		// step 1: can you advance to center row without interception
		// step 2: can you advance to a shoulder without interception
		// step 3: will advancing to that area put you near a teammate
			// if so, pass to them instead of advancing
			// if not, advance

		// alt:
		// step 4: if you are close to the goal
			// try to find a shot opening
			// try to pass to annother open player
			// try to attack a nearby player
		return Vector2.zero;
	}

	float GetGoalDir(int team) {
		if (team == 0)
			return 1;
		return -1;
	}

	public int GetEnemyTeam(int team) {
		if (team == 0)
			return 1;
		return 0;
	}


	// are there any enemies between you and your shot destination
	public bool ClearShot(Player p, Vector2 destination, float radius = 1, bool checkFriendly = false) {
		int enemyTeam = GetEnemyTeam (p.team);
		if (checkFriendly)
			enemyTeam = p.team;
		float distance = Vector2.Distance ((Vector2)p.transform.position, destination);
		float totalChecks = Mathf.Ceil(distance / radius);
		Vector2 dir = new Vector2 (destination.x - p.transform.position.x, destination.y - p.transform.position.y);
		dir = dir.normalized;

		bool noEnemy = true;

		for (int n = 0; n < totalChecks; n++) {
			for (int i = 0; i < players[enemyTeam].Count; i++) {
				moveCheckArea = (Vector2)p.transform.position + dir * n;
				if (Vector2.Distance (players [enemyTeam][i].transform.position, (Vector2)p.transform.position + dir * n) <= radius)
					noEnemy = false;
			}
		}

		return noEnemy;
	}
		
	// clear movement path
	public bool ClearMovementPath(Player p, Vector2 dir, float radius = 1) {
		int enemyTeam = GetEnemyTeam (p.team);
		Vector2 destinationPoint = (Vector2)p.transform.position + dir * radius * 2;
		bool noEnemies = true;

		moveCheckArea = destinationPoint;

		for (int i = 0; i < players[enemyTeam].Count; i++) {
			if (Vector2.Distance (players [enemyTeam][i].transform.position, destinationPoint) <= radius)
				noEnemies = false;
		}
		return noEnemies;
	}

	Vector2 moveCheckArea;
	void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere (moveCheckArea, 1);
	}
}
