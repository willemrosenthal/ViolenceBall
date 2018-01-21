using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachiene : MonoBehaviour {

	// current state
	public State cState;
	// previous state
	public State pState;
	// global state
	public State gState;

	//public StateMachiene() {}

	public void FirstState(State newState) {
		// set new state
		cState = newState;

		// call enter
		cState.Enter ();
	}
	
	public void ChangeState(State newState) {
		Debug.Assert (cState && newState);

		// record previous state
		pState = cState;

		// call exit
		cState.Exit ();

		// set new state
		cState = newState;

		// call enter
		cState.Enter ();
	}

	public void RevertToPreviousState() {
		ChangeState (pState);
	}

	public void Execute () {
		if (gState)
			gState.Execute ();
		if (cState)
			gState.Execute ();
	}

	public bool InState(State stateCheck) {
		return (stateCheck == cState);
	}
}
