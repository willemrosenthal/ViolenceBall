using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(PlayerAI))]
public class PlayerInspector : Editor {

	int selectedPosition = 0;
	int selectedAbility = 0;

	public PlayerAI ai;

	void OnEnable() {
		//ai = (PlayerAI)target;
	}

	public override void OnInspectorGUI () {
		/*
		if (Application.isEditor && !Application.isPlaying) {
			selectedPosition = EditorGUILayout.Popup ("Positions:", selectedPosition, ai.positionList, EditorStyles.popup);
			ai.position = ai.positionList [selectedPosition];

			selectedAbility = EditorGUILayout.Popup ("Abilities:", selectedAbility, ai.abilityList, EditorStyles.popup);
			ai.ability = ai.abilityList [selectedAbility];
		
			GUILayout.Space (15);
			}

			DrawDefaultInspector ();
		}
		*/
	}
}