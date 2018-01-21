using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class PaletteMaker : EditorWindow
{

	int numColors;
	Sprite example = null;
	Sprite lastExample;
	Texture2D exampleTex = null;
	Texture2D startingPalette = null;
	Texture2D lastStartingPalette = null;
	List<Color> newColorPalette;
	List<Color> greyPalette;

	// saving
	string filePath;
	bool savePaletteInStartingPaletteDir = true;
	bool saveInExampleSpriteDir = false;
	bool createNewPalette = true;
	string newPaletteName = "palette-new";
	bool createNewPaletteFolder = false;
	string newPaletteFolder = "PaletteFolderName";

	bool saved = false;
	double saveTime;
	double saveSignTime = 4;

	bool basePaletteIsReadWriteable = true;


	[MenuItem("Window/Palette Maker")]
	static void ShowWindow() {
		GetWindow<PaletteMaker> ("Palette Maker");
	}


	GUIStyle MakeColor (Color makeColor, int width, int height) {
		// background color
		Texture2D bgTex = new Texture2D(width, height);
		Color fillcolor = makeColor;
		Color[] fillColorArray =  bgTex.GetPixels();

		for(var i = 0; i < fillColorArray.Length; ++i) {
			fillColorArray[i] = fillcolor;
		}
		bgTex.SetPixels( fillColorArray );
		bgTex.Apply();

		GUIStyle bg = new GUIStyle();
		bg.normal.background = bgTex;
		return bg;
	}

	Texture2D MakeColorTexture (Color makeColor, int width, int height) {
		// background color
		Texture2D bgTex = new Texture2D(width, height);
		Color fillcolor = makeColor;
		Color[] fillColorArray =  bgTex.GetPixels();

		for(var i = 0; i < fillColorArray.Length; ++i) {
			fillColorArray[i] = fillcolor;
		}
		bgTex.SetPixels( fillColorArray );
		bgTex.Apply();

		return bgTex;
	}


	void OnEnable() {
		if (example != null)
			GetGreyPalette ();
	}



	Texture2D SpriteToTexture2D (Sprite sprite) {
		if ((int)sprite.rect.width <= 0 || (int)sprite.rect.height <= 0 || newColorPalette.Count <= 0)
			return new Texture2D (1, 1);
		Texture2D outputTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
		Color[] pixels = sprite.texture.GetPixels(
			(int)(sprite.textureRect.x - sprite.textureRectOffset.x),
			(int)(sprite.textureRect.y - sprite.textureRectOffset.y), 
			(int)sprite.rect.width, 
			(int)sprite.rect.height
		);
		ApplyColors (pixels);
		outputTexture.SetPixels( pixels );
		outputTexture.Apply();
		outputTexture.filterMode = FilterMode.Point;
		return outputTexture;
	}

	Color[] ApplyColors(Color[] pixels) {
		Texture2D lookupTexture = GeneratePaletteTexture(newColorPalette); // needs to be our generated lookup texture
		if (!lookupTexture)
			return pixels;
		int paletteSize = lookupTexture.width;
		float pixOffset = (1/paletteSize) * 0f;
		for (int i = 0; i < pixels.Length; i++) {
			float greyVal = pixels [i].r + pixOffset;
			int pixLoc = (int)Mathf.Floor(greyVal * paletteSize);
			if (pixels[i].a != 0)
				pixels[i] = lookupTexture.GetPixel(pixLoc,0);
		}
		return pixels;
	}

	Texture2D GeneratePaletteTexture(List<Color> colorList = null) {
		Texture2D palette = null;
		Color[] paletteColorArray = null;

		palette = new Texture2D(colorList.Count, 1);
		paletteColorArray =  palette.GetPixels();
		for(var i = 0; i < paletteColorArray.Length; ++i) {
			paletteColorArray[i] = colorList[i];
		}

		palette.SetPixels( paletteColorArray );
		palette.Apply();

		return palette;
	}

	void MakeColorSlots(List<Color> paletteToUse, bool forceRebuild = false) {
		if (newColorPalette != null && (newColorPalette.Count == paletteToUse.Count && !forceRebuild))
			return;

		newColorPalette = new List<Color> ();
		for (int i = 0; i < greyPalette.Count; i++) {
			if (i < paletteToUse.Count) {
				newColorPalette.Add (paletteToUse [i]);
			} else {
				newColorPalette.Add (greyPalette [i]);
			}
		}
	}

	void GetGreyPalette() {
		if (Event.current != null && Event.current.type != EventType.Layout)
			return;
		
		lastExample = example;

		greyPalette = new List<Color> ();
		Color[] pixels = example.texture.GetPixels(
			(int)(example.textureRect.x - example.textureRectOffset.x),
			(int)(example.textureRect.y - example.textureRectOffset.y), 
			(int)example.rect.width, 
			(int)example.rect.height
		);

		// build list of only unique colors found in image
		greyPalette = FindUniqueColors(pixels);
		// sort pixels by brightness 
		greyPalette = SortBasedOnValue(greyPalette);
		numColors = greyPalette.Count;


		// ceck if texture is readable
		if (startingPalette != null || lastStartingPalette != startingPalette || !basePaletteIsReadWriteable) {
			basePaletteIsReadWriteable = true;
			try {
				startingPalette.GetPixel (0, 0);
			} catch (UnityException e) {
				if (e.Message.StartsWith ("Texture '" + startingPalette.name + "' is not readable")) {
					basePaletteIsReadWriteable = false;
				}
			}

		}


		if (startingPalette == null || lastStartingPalette == startingPalette || !basePaletteIsReadWriteable) {
			MakeColorSlots (greyPalette);
		}
		else if (lastStartingPalette != startingPalette) {
			//greyPalette = new List<Color> ();
			Color[] fromPal = startingPalette.GetPixels ();
			List<Color> fromPalList = new List<Color> ();

			for (int i = 0; i < fromPal.Length; i++) {
				fromPalList.Add (fromPal [i]);
			}

			MakeColorSlots (fromPalList, true);

			lastStartingPalette = startingPalette;
		}
	}

	List<Color> FindUniqueColors(Color[] pixels) {
		List<Color> uniqueColors = new List<Color>();

		// look at every pixel
		bool unique;
		for (int i = 0; i < pixels.Length; i++) {
			unique = true;
			// compare pixel to found unique colors
			if (pixels [i].a == 0)
				continue;

			for (int c = 0; c < uniqueColors.Count; c++) {
				if (pixels [i] == uniqueColors [c]) {
					unique = false;
					break;
				}
			}
			if (unique) {
				uniqueColors.Add (pixels [i]);
			}
		}
		return uniqueColors;
	}

	List<Color> SortBasedOnValue (List<Color> colorsToSort) {
		colorsToSort.Sort( (p1,p2)=>(int)((p1.r + p1.g + p1.b) * 1000).CompareTo((int)((p2.r + p2.g + p2.b) * 1000)) );
		return colorsToSort;
	}

	void OnGUI() {
		// display example
		if (example != null) {
			if (example != lastExample)
				GetGreyPalette ();
			if (startingPalette != lastStartingPalette || !basePaletteIsReadWriteable)
				GetGreyPalette ();
			Rect exampleRect = GUILayoutUtility.GetRect (250, 250);
			GUI.DrawTexture (exampleRect, SpriteToTexture2D(example), ScaleMode.ScaleToFit, true, 0); 
		}

		//
		if (!basePaletteIsReadWriteable) {
			EditorGUILayout.HelpBox("Selected palette does not have 'Read/TextWriter Enabled'.", MessageType.Error);
		}
		else if (EditorApplication.timeSinceStartup - saveTime < saveSignTime) {
			EditorGUILayout.HelpBox("Palette Saved", MessageType.Info);
		}
		else {
			GUILayout.Space (50);
		}
		//

		GUILayout.BeginHorizontal ();
		GUILayout.Label("Example Sprite To Color");
		GUILayout.Label("Starting Pallete to use (optional)");
		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal ();
		example = (Sprite)EditorGUILayout.ObjectField( example, typeof(Sprite));
		startingPalette = (Texture2D)EditorGUILayout.ObjectField( startingPalette, typeof(Texture2D));
		GUILayout.EndHorizontal ();

		if (newColorPalette != null && newColorPalette.Count > 0) {

			for (int i = 0; i < newColorPalette.Count; i++) {
				GUILayout.BeginHorizontal ();
				Rect frameSpriteRect = GUILayoutUtility.GetRect (15, 15);
				GUILayout.Label ("replace value");
				GUI.DrawTexture (frameSpriteRect, MakeColorTexture (greyPalette [i], 15, 15), ScaleMode.ScaleToFit, true, 0); 
				newColorPalette [i] = EditorGUILayout.ColorField (newColorPalette [i]);
				GUILayout.EndHorizontal ();
			}



			createNewPalette = EditorGUILayout.Toggle ("Create new Palette", createNewPalette);

			if (createNewPalette) {
				GUILayout.BeginVertical ();

				newPaletteName = EditorGUILayout.TextField ("Palette Name", newPaletteName); 

				if (startingPalette != null) {
					GUILayout.BeginHorizontal ();
					savePaletteInStartingPaletteDir = !saveInExampleSpriteDir;
					savePaletteInStartingPaletteDir = EditorGUILayout.Toggle ("Save in starting palette dir.", savePaletteInStartingPaletteDir);
					saveInExampleSpriteDir = !savePaletteInStartingPaletteDir;
					saveInExampleSpriteDir = EditorGUILayout.Toggle ("Save in example sprite dir.", saveInExampleSpriteDir);
					GUILayout.EndHorizontal ();
				} else {
					EditorGUILayout.HelpBox ("Will save in directory of example sprite", MessageType.Info);
				}

				if (startingPalette == null)
					savePaletteInStartingPaletteDir = false;

				createNewPaletteFolder = EditorGUILayout.Toggle ("Save in New Folder", createNewPaletteFolder);

				if (createNewPaletteFolder) {
					newPaletteFolder = EditorGUILayout.TextField ("New Palette Folder Name", newPaletteFolder);
				}
				GUILayout.EndVertical ();
			}

			if (!createNewPalette) {
				EditorGUILayout.HelpBox ("Saving will save over and replace your currently loaded pallet", MessageType.Warning);
			}
			if (GUILayout.Button ("Save Palette", GUILayout.Height (25))) {
				GeneratePalette (newColorPalette);
			}
		}

	}
		
		

	void GeneratePalette(List<Color> colorList = null, Color[] colorArray = null, string specialName = "") {
		Texture2D palette = null;
		Color[] paletteColorArray = null;

		if (colorList != null && colorList.Count != 0) {
			palette = new Texture2D(colorList.Count, 1);
			paletteColorArray =  palette.GetPixels();
			for(var i = 0; i < paletteColorArray.Length; ++i) {
				paletteColorArray[i] = colorList[i];
			}
		}
		
		else if (colorArray != null && colorArray.Length != 0) {
			palette = new Texture2D(colorArray.Length, 1);
			paletteColorArray =  palette.GetPixels();
			for(var i = 0; i < paletteColorArray.Length; ++i) {
				paletteColorArray[i] = colorArray[i];
			}
		}

		palette.SetPixels( paletteColorArray );
		palette.Apply();

		string palettePath = "";

		if (GetFilePath ()) {
			SaveTextureToFile (palette, filePath);
		} else {
			EditorGUILayout.HelpBox("Missing file or directory name", MessageType.Error);
		}
		
	}

	bool GetFilePath() {
		filePath = "";

		if (startingPalette != null) {
			filePath = AssetDatabase.GetAssetPath (startingPalette.GetInstanceID ());

			if (!createNewPalette)
				savePaletteInStartingPaletteDir = true;

			if (savePaletteInStartingPaletteDir) {
				string[] newfilePath = filePath.Split ('/');
				filePath = "";

				for (int i = 0; i < newfilePath.Length; i++ ) {
					if (i + 1 < newfilePath.Length)
						filePath += newfilePath [i] + "/";
				}

				if (!createNewPalette) {
					newPaletteName = startingPalette.name;
					return true;
				}

				if (newPaletteName == "")
					return false;

				if (createNewPaletteFolder)
					filePath +=  newPaletteFolder + "/";

				return true;
			}
		}

		if ((startingPalette == null || !savePaletteInStartingPaletteDir) && example) {
			filePath = AssetDatabase.GetAssetPath (example.GetInstanceID ());

			string[] newfilePath = filePath.Split ('/');
			filePath = "";

			for (int i = 0; i < newfilePath.Length; i++ ) {
				if (i + 1 < newfilePath.Length)
					filePath += newfilePath [i] + "/";
			}

			if (newPaletteName == "")
				return false;

			if (createNewPaletteFolder)
				filePath +=  newPaletteFolder + "/";

			return true;
		}

		return false;
	}

	void SaveTextureToFile( Texture2D texture , string _filePath = "") {
		texture.filterMode = FilterMode.Point;
		string finalFilePath = _filePath;

		// check if directory exists
		if (!Directory.Exists(finalFilePath)) {
			Directory.CreateDirectory(finalFilePath);
		}

		finalFilePath += newPaletteName + ".png";

		byte[] bytes = texture.EncodeToPNG();
		FileStream stream = new FileStream(finalFilePath, FileMode.OpenOrCreate, FileAccess.Write);
		BinaryWriter writer = new BinaryWriter(stream);


		// do writing
		for (int i = 0; i < bytes.Length; i++) {
			writer.Write (bytes [i]);
		}

		writer.Close ();
		stream.Close ();

		saveTime = EditorApplication.timeSinceStartup;
		Repaint ();
	}





}