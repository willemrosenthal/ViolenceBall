using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaletteIndex : MonoBehaviour {
	[System.Serializable]
	public class PaletteList {
		public string fighterName;
		public Texture[] palettes;
	}

	public PaletteList[] palettes;

	public Texture GetPalette(string fighterName, Texture playerOnePalette) {
		int pNum = -1;
		int fighterIndex = 0;
		for (int i = 0; i < palettes.Length; i++) {
			if (fighterName.ToLower() == palettes[i].fighterName.ToLower()) {
				fighterIndex = i;
				while (pNum == -1 || palettes[i].palettes[pNum] == playerOnePalette)
					pNum = RandomPaletteNumber(palettes [i].palettes.Length);
			}
		}
		return palettes[fighterIndex].palettes[pNum];
	}

	int RandomPaletteNumber( int length ) {
		return (int)Mathf.Floor(length * 0.999f * Random.value);
	}

}
