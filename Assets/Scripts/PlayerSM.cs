using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSM : StateMachiene {


	void Start () {
		FirstState (ReturnToPosition);
	}
	

	void Update () {
		Execute ();
	}


	private class ReturnToPosition: State {
		public override void Enter () {
		}

		public override void Execute () {
		}

		public override void Exit () {
		}
	}
}
