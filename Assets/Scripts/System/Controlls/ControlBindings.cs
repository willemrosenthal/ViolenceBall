using InControl;
using UnityEngine;


public class ControlBindings : MonoBehaviour {

	int actionNo = 0;
	int inputTurn = 0;
	int inputStage = 0;
	  
	PlayerInputs playerInputs; 
	//public PlayerInputs f.playerInputs = null;
	string saveData;

	void OnEnable() {
		playerInputs = PlayerInputs.CreateWithDefaultBindings();
	}

	public void SetupBindings (ref PlayerInputs inputs) {
		//playerInputs = PlayerInputs.CreateWithDefaultBindings();
		//playerInputs = inputs;
		LoadBindings();
	}


	void SaveBindings() {
		saveData = playerInputs.Save();
		PlayerPrefs.SetString( "ControllerBindings", saveData );
		PlayerPrefs.Save();
	}


	void LoadBindings() {
		if (PlayerPrefs.HasKey( "ControllerBindings" )) {
			saveData = PlayerPrefs.GetString( "ControllerBindings" );
			playerInputs.Load( saveData );
		}
	}

	AvailibleButtons availibleButtons = new AvailibleButtons();
	public struct AvailibleButtons {
		public string[] buttons;
		public bool[] takenButtons;
		public void SetButtons(int buttonNo, string name) {
			if (buttons == null) {
				buttons = new string[4];
				takenButtons = new bool[4];
			}
			buttons[buttonNo] = name;
		}
	}

	void Update() {

		if (inputTurn == 0) {
			LoadBindings();
			playerInputs.Device = playerInputs.Device;
			UnsetBindableButtons();
			inputTurn++;
		}

		// listen for A button
		else if (inputTurn == 1) {
			ListenForinput ("A Button");
		}

		// listen for B button
		else if (inputTurn == 2) {
			ListenForinput ("B Button");
		}

		// listen for X button
		else if (inputTurn == 3) {
			ListenForinput ("X Button");
		}

		// listen for Y button
		else if (inputTurn == 4) {
			ListenForinput ("Y Button");
		}

		// done //
		else if (inputTurn ==  5) {
			Debug.Log("inputs done being set!");
			inputTurn++;
		}

		// press up to start again, press down to exit
		else if (inputTurn ==  6) {
			if (playerInputs.DpadDown.IsPressed) {
				Debug.Log ("close and save");
				SaveAndClose ();
			}
			else if (playerInputs.DpadUp.IsPressed) {
				Debug.Log ("lets try again");
				inputTurn = 0;
				inputStage = 0;
			}
		}
	}

	void SaveAndClose() {
		SaveBindings ();
		playerInputs.Save ();
		//playerInputs.Destroy();
		Destroy (this); // should be this.gameObject
	}

	void ListenForinput(string buttonName) {
		// find the button we want to set
		actionNo = GetInput (buttonName);
		var action = playerInputs.Actions [actionNo];

		// get and clear action
		if (inputStage == 0) {
			Debug.Log ("listening for " + buttonName);
			if (action.Bindings.Count > 0) {
				action.ListenForBindingReplacing (action.Bindings [0]);
			} else {
				action.ListenForBinding ();
			}
			inputStage++;
		}

		if (inputStage == 1) {
			// if we aren't listening anymore (meaning a new button is set) then go to the next
			if ( !action.IsListeningForBinding ) {
				Debug.Log (playerInputs.Actions [actionNo].Name + " = " + playerInputs.Actions [actionNo].Bindings [0].Name);
				inputStage = 0;
				inputTurn++;
			} 
		}
	}
		
	// gets the action for the button name input
	int GetInput (string buttonName) {
		int actNo = 0; 
		// go though actions and find the requested action
		for (var i = 0; i < playerInputs.Actions.Count; i++) {
			if (playerInputs.Actions [i].Name == buttonName)
				actNo = i;
		}
		return  actNo;
	}

	void UnsetBindableButtons() {
		actionNo = GetInput ("A Button");
		var action = playerInputs.Actions [actionNo];
		if (action.Bindings.Count > 0)
			action.ClearBindings ();

		actionNo = GetInput ("B Button");
		action = playerInputs.Actions [actionNo];
		if (action.Bindings.Count > 0)
			action.ClearBindings ();

		actionNo = GetInput ("X Button");
		action = playerInputs.Actions [actionNo];
		if (action.Bindings.Count > 0)
			action.ClearBindings ();

		actionNo = GetInput ("Y Button");
		action = playerInputs.Actions [actionNo];
		if (action.Bindings.Count > 0)
			action.ClearBindings ();
	}
}