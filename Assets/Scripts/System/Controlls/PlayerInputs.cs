using InControl;
using UnityEngine;


public class PlayerInputs : PlayerActionSet
{
	// face buttons
	public PlayerAction AButton;
	public PlayerAction BButton;
	public PlayerAction XButton;
	public PlayerAction YButton;

	// triggers
	public PlayerAction LeftTrigger;
	public PlayerAction RightTrigger;

	// special buttons
	public PlayerAction Start;

	// dpad
	public PlayerAction DpadUp;
	public PlayerAction DpadDown;
	public PlayerAction DpadRight;
	public PlayerAction DpadLeft;

	// left stick
	public PlayerAction Left;
	public PlayerAction Right;
	public PlayerAction Up;
	public PlayerAction Down;

	// right stick
	public PlayerAction LeftRS;
	public PlayerAction RightRS;
	public PlayerAction UpRS;
	public PlayerAction DownRS;

	// axis
	public PlayerTwoAxisAction Dpad;
	public PlayerTwoAxisAction RightStick;
	public PlayerTwoAxisAction LeftStick;


	public PlayerInputs()
	{
		AButton = CreatePlayerAction( "A Button" );
		BButton = CreatePlayerAction( "B Button" );
		XButton = CreatePlayerAction( "X Button" );
		YButton = CreatePlayerAction( "Y Button" );

		LeftTrigger = CreatePlayerAction( "Left Trigger" );
		RightTrigger = CreatePlayerAction( "Right Trigger" );

		Start = CreatePlayerAction( "Start" );

		Left = CreatePlayerAction( "Move Left" );
		Right = CreatePlayerAction( "Move Right" );
		Up = CreatePlayerAction( "Move Up" );
		Down = CreatePlayerAction( "Move Down" );

		LeftRS = CreatePlayerAction( "Aim Left" );
		RightRS = CreatePlayerAction( "Aim Right" );
		UpRS = CreatePlayerAction( "Aim Up" );
		DownRS = CreatePlayerAction( "Aim Down" );

		DpadUp = CreatePlayerAction( "D-pad Up" );
		DpadDown = CreatePlayerAction( "D-pad Down" );
		DpadRight = CreatePlayerAction( "D-pad Right" );
		DpadLeft = CreatePlayerAction( "D-pad Left" );

		Dpad = CreateTwoAxisPlayerAction( DpadLeft, DpadRight, DpadDown, DpadUp );
		RightStick = CreateTwoAxisPlayerAction( LeftRS, RightRS, DownRS, UpRS );
		LeftStick = CreateTwoAxisPlayerAction( Left, Right, Down, Up );
	}


	public static PlayerInputs CreateWithDefaultBindings()
	{
		var playerActions = new PlayerInputs();

		// How to set up mutually exclusive keyboard bindings with a modifier key.
		// playerActions.Back.AddDefaultBinding( Key.Shift, Key.Tab );
		// playerActions.Next.AddDefaultBinding( KeyCombo.With( Key.Tab ).AndNot( Key.Shift ) );

		playerActions.AButton.AddDefaultBinding( Key.V );
		playerActions.AButton.AddDefaultBinding( InputControlType.Action1 );

		playerActions.BButton.AddDefaultBinding( Key.C );
		playerActions.BButton.AddDefaultBinding( InputControlType.Action2 );

		playerActions.XButton.AddDefaultBinding( Key.Z );
		playerActions.XButton.AddDefaultBinding( InputControlType.Action3 );

		playerActions.YButton.AddDefaultBinding( Key.X );
		playerActions.YButton.AddDefaultBinding( InputControlType.Action4 );

		playerActions.LeftTrigger.AddDefaultBinding( Key.A );
		playerActions.LeftTrigger.AddDefaultBinding( InputControlType.LeftTrigger );

		playerActions.RightTrigger.AddDefaultBinding( Key.S );
		playerActions.RightTrigger.AddDefaultBinding( InputControlType.RightTrigger );

		playerActions.Start.AddDefaultBinding( Key.Return );
		playerActions.Start.AddDefaultBinding( InputControlType.Command );

		playerActions.Up.AddDefaultBinding( Key.UpArrow );
		playerActions.Down.AddDefaultBinding( Key.DownArrow );
		playerActions.Left.AddDefaultBinding( Key.LeftArrow );
		playerActions.Right.AddDefaultBinding( Key.RightArrow );

		playerActions.DpadUp.AddDefaultBinding( Key.UpArrow );
		playerActions.DpadDown.AddDefaultBinding( Key.DownArrow );
		playerActions.DpadRight.AddDefaultBinding( Key.RightArrow );
		playerActions.DpadLeft.AddDefaultBinding( Key.LeftArrow );


		playerActions.DpadUp.AddDefaultBinding( InputControlType.DPadUp );
		playerActions.DpadDown.AddDefaultBinding( InputControlType.DPadDown );
		playerActions.DpadRight.AddDefaultBinding( InputControlType.DPadRight );
		playerActions.DpadLeft.AddDefaultBinding( InputControlType.DPadLeft );

		playerActions.Left.AddDefaultBinding( InputControlType.LeftStickLeft );
		playerActions.Right.AddDefaultBinding( InputControlType.LeftStickRight );
		playerActions.Up.AddDefaultBinding( InputControlType.LeftStickUp );
		playerActions.Down.AddDefaultBinding( InputControlType.LeftStickDown );

		playerActions.LeftRS.AddDefaultBinding( InputControlType.RightStickLeft );
		playerActions.RightRS.AddDefaultBinding( InputControlType.RightStickRight );
		playerActions.UpRS.AddDefaultBinding( InputControlType.RightStickUp );
		playerActions.DownRS.AddDefaultBinding( InputControlType.RightStickDown );

		/*
		playerActions.Up.AddDefaultBinding( Mouse.PositiveY );
		playerActions.Down.AddDefaultBinding( Mouse.NegativeY );
		playerActions.Left.AddDefaultBinding( Mouse.NegativeX );
		playerActions.Right.AddDefaultBinding( Mouse.PositiveX );
		*/

		playerActions.ListenOptions.IncludeUnknownControllers = true;
		//playerActions.ListenOptions.MaxAllowedBindingsPerType = 1;
		//playerActions.ListenOptions.AllowDuplicateBindingsPerSet = true;
		playerActions.ListenOptions.UnsetDuplicateBindingsOnSet = false;
		playerActions.ListenOptions.MaxAllowedBindings = 1;
		//playerActions.ListenOptions.IncludeMouseButtons = true;
		//playerActions.ListenOptions.IncludeModifiersAsFirstClassKeys = true;
		//playerActions.ListenOptions.IncludeMouseButtons = true;
		//playerActions.ListenOptions.IncludeMouseScrollWheel = true;

		playerActions.ListenOptions.OnBindingFound = ( action, binding ) => {
			if (binding == new KeyBindingSource( Key.Escape ))
			{
				action.StopListeningForBinding();
				return false;
			}
			return true;
		};

		playerActions.ListenOptions.OnBindingAdded += ( action, binding ) => {
			Debug.Log( "Binding added... " + binding.DeviceName + ": " + binding.Name );
		};

		playerActions.ListenOptions.OnBindingRejected += ( action, binding, reason ) => {
			Debug.Log( "Binding rejected... " + reason );
		};

		return playerActions;
	}

	/*
	public class BindingListenOptions
	{
		/// Include controllers when listening for new bindings.
		public bool IncludeControllers = true;

		/// Include unknown controllers when listening for new bindings.
		public bool IncludeUnknownControllers = false;

		/// Include non-standard controls on controllers when listening for new bindings.
		public bool IncludeNonStandardControls = true;

		/// Include mouse buttons when listening for new bindings.
		public bool IncludeMouseButtons = false;

		/// Include keyboard keys when listening for new bindings.
		public bool IncludeKeys = true;

		/// Treat modifiers (Shift, Alt, Control, etc.) as first class keys instead of modifiers.
		public bool IncludeModifiersAsFirstClassKeys = false;

		/// The maximum number of bindings allowed for the action. 
		/// If a new binding is detected and would cause this number to be exceeded, 
		/// enough bindings are removed to make room before adding the new binding.
		/// When zero (default), no limit is applied.
		public uint MaxAllowedBindings = 0;

		/// The maximum number of bindings of a given type allowed for the action. 
		/// If a new binding is detected and would cause this number to be exceeded, 
		/// enough bindings are removed to make room before adding the new binding.
		/// When zero (default), no limit is applied.
		/// When nonzero, this setting overrides MaxAllowedBindings.
		public uint MaxAllowedBindingsPerType = 0;

		/// Allow bindings that are already bound to any other action in the set.
		public bool AllowDuplicateBindingsPerSet = false;

		/// If an existing duplicate binding exists, remove it before adding the new one.
		/// When <code>true</code>, the value of AllowDuplicateBindingsPerSet is irrelevant.
		public bool UnsetDuplicateBindingsOnSet = false;

		/// If not <code>null</code>, and this binding is on the listening action, this binding
		/// will be replace by the newly found binding.
		public BindingSource ReplaceBinding = null;

		/// This function is called when a binding is found but before it is added.
		/// If this function returns false, then the binding is ignored
		/// and listening for new bindings will continue.
		/// If set to null (default), it will not be called.
		public Func<PlayerAction, BindingSource, bool> OnBindingFound = null;

		/// This action is called after a binding is added.
		/// If set to null (default), it will not be called.
		public Action<PlayerAction, BindingSource> OnBindingAdded = null;

		/// This action is called after a binding is found, but rejected along with 
		/// the reason (BindingSourceRejectionType) why it was rejected.
		/// If set to null (default), it will not be called.
		public Action<PlayerAction, BindingSource, BindingSourceRejectionType> OnBindingRejected = null;
	}
	*/
}

