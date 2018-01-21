using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class SpriteGreyscaleConverter : EditorWindow
{
	bool showTextureList = true;
	public Texture2D[] multiTextures;
	public Texture2D homogenizeBase;

	bool readWriteEnabled = true;

	string paletteDirName = "palettes";
	bool putPaletteInOutputDir = true;
	bool putPaletteInOriginalDir = false;
	string outputFolderName = "grey-map textures";
	bool outputIntoParentFolder = true;

	Texture2D[] homogenizedTextures = null;

	bool otherSettings = false;

	bool colorCorrectToExample;

	bool generateColorPalette = false;
	bool reCheckPalette = false;
	bool debug;

	string colorPaletteName = "new color palette";

	string filePath;
	string parentFilePath;
	Texture2D outputTexture;

	[MenuItem("Window/Palette Converter")]
	static void ShowWindow() {
		GetWindow<SpriteGreyscaleConverter> ("Palette Conv.");
	}

	GUIStyle bg;
	GUIStyle dragAreaBg;
	GUIStyle darkBg;


	void GuiColorBlock () {
		// background color
		Texture2D bgTex = new Texture2D(1, 1);
		Color fillcolor = new Color(0.76f, 0.76f, 0.76f);
		Color[] fillColorArray =  bgTex.GetPixels();

		for(var i = 0; i < fillColorArray.Length; ++i) {
			fillColorArray[i] = fillcolor;
		}
		bgTex.SetPixels( fillColorArray );
		bgTex.Apply();

		bg = new GUIStyle();
		bg.normal.background = bgTex;

		// darker
		bgTex = new Texture2D(1, 1);
		fillcolor = new Color(0.65f, 0.65f, 0.65f);
		fillColorArray =  bgTex.GetPixels();

		for(var i = 0; i < fillColorArray.Length; ++i)
		{
			fillColorArray[i] = fillcolor;
		}
		bgTex.SetPixels( fillColorArray );
		bgTex.Apply();

		darkBg = new GUIStyle();
		darkBg.normal.background = bgTex;

		// drag area
		bgTex = new Texture2D(1, 1);
		fillcolor = new Color(0.62f, 0.68f, 0.73f);
		fillColorArray =  bgTex.GetPixels();

		for(var i = 0; i < fillColorArray.Length; ++i)
		{
			fillColorArray[i] = fillcolor;
		}
		bgTex.SetPixels( fillColorArray );
		bgTex.Apply();

		dragAreaBg = new GUIStyle();
		dragAreaBg.normal.background = bgTex;
	}

	void OnEnable() {
		GuiColorBlock ();
	}

	void OnGUI() {
		GUILayout.BeginVertical (bg, GUILayout.MaxWidth(500));

		if (multiTextures == null || multiTextures.Length == 0) {
			GuiColorBlock ();
			GUILayout.BeginVertical (dragAreaBg);
			GUILayout.Space (30);
			GUIStyle centeredStyle = GUI.skin.GetStyle ("Label");
			centeredStyle.alignment = TextAnchor.UpperCenter;
			GUILayout.Label ("drag images here", centeredStyle);
			GUILayout.Space (30);
			GUILayout.EndVertical ();
		}

		DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
		if (Event.current.type == EventType.DragExited && Event.current.mousePosition.y < 80) {
			multiTextures = new Texture2D[DragAndDrop.objectReferences.Length];
			for (int i = 0; i < DragAndDrop.objectReferences.Length; i++) {
				multiTextures [i] = DragAndDrop.objectReferences [i] as Texture2D;
			}
			homogenizedTextures = new Texture2D[0];
			if (Event.current.type != EventType.Layout) {
				return;
			}
		}

		if (multiTextures != null && multiTextures.Length > 0) {
			GUILayout.Space (10);
			showTextureList = EditorGUILayout.Foldout (showTextureList, "Textures to process");
			if (showTextureList) {
				for (int i = 0; i < multiTextures.Length; i++) {
					multiTextures [i] = (Texture2D)EditorGUILayout.ObjectField (multiTextures [i], typeof(Texture2D));
				}
			}

			GUILayout.Space (5);
			outputIntoParentFolder = EditorGUILayout.Toggle ("Output into parent folder", outputIntoParentFolder);
			if (!outputIntoParentFolder) {
				GUILayout.Space (5);
				outputFolderName = EditorGUILayout.TextField ("Output Folder Name", outputFolderName); 
			}
			GUILayout.Space (5);


			if (GUILayout.Button ("Remove All Textures", GUILayout.Height(25))) {
				multiTextures = new Texture2D[0];
			}
		}

		if (multiTextures != null && multiTextures.Length > 0) {
			readWriteEnabled = true;
			for (int i = 0; i < multiTextures.Length; i++) {
				// check if readable
				try {
					multiTextures [i].GetPixel (0, 0);
				} catch (UnityException e) {
					if (e.Message.StartsWith ("Texture '" + multiTextures [i].name + "' is not readable")) {
						readWriteEnabled = false;
					}
				}
			}
		}
		if (!readWriteEnabled) {
			EditorGUILayout.HelpBox("Please make sure 'Read/TextWriter Enabled' is checked on all your textures (or sprites)", MessageType.Error);
		}

		GUILayout.Space (20);


		generateColorPalette = EditorGUILayout.Toggle ("Generate Color Palette", generateColorPalette);

		GUIStyle leftAllign = new GUIStyle("label");
		leftAllign.alignment = TextAnchor.LowerLeft;

		GUIStyle rightAllign = new GUIStyle("toggle");
		rightAllign.alignment = TextAnchor.LowerRight;

		if (generateColorPalette) {
			GUILayout.BeginVertical (darkBg);
			GUILayout.Space (5);
				GUILayout.BeginHorizontal ();
					GUILayout.BeginVertical ();
						GUILayout.Label ("Palette Name", leftAllign);
						GUILayout.Label ("Palette Folder Name", leftAllign);
					GUILayout.EndVertical ();

					GUILayout.BeginVertical ();
						colorPaletteName = EditorGUILayout.TextField (colorPaletteName);
						paletteDirName = EditorGUILayout.TextField (paletteDirName);
					GUILayout.EndVertical ();
				GUILayout.EndHorizontal ();
				GUILayout.Space (10);
				GUILayout.BeginHorizontal ();
					GUILayout.FlexibleSpace();
					GUILayout.Label ("Put Palette Folder in Output Directory", leftAllign);
					GUILayout.Space (40);
					putPaletteInOutputDir = !putPaletteInOriginalDir;
					putPaletteInOutputDir = EditorGUILayout.Toggle (putPaletteInOutputDir, rightAllign);
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
					GUILayout.FlexibleSpace();
					GUILayout.Label ("Put Palette Folder in Original Sprite Directory", leftAllign);
					GUILayout.Space (40);
					putPaletteInOriginalDir = !putPaletteInOutputDir;
					putPaletteInOriginalDir = EditorGUILayout.Toggle (putPaletteInOriginalDir, rightAllign);
				GUILayout.EndHorizontal ();
			GUILayout.Space (5);
			GUILayout.EndVertical ();
		}

		GUILayout.Space (20);


		if (multiTextures != null && multiTextures.Length > 0 && readWriteEnabled) {
			if (GUILayout.Button ("Convert Images", GUILayout.Height (50))) {
				StartConvert ();
			}
		}

		otherSettings = EditorGUILayout.Foldout (otherSettings, "Debug and other settings");

		if (otherSettings) {
			/*
			GUILayout.BeginHorizontal ();
			colorCorrectToExample = EditorGUILayout.Toggle ("Color Correct", colorCorrectToExample);
			homogenizeBase = (Texture2D)EditorGUILayout.ObjectField("Color Match Sample", homogenizeBase, typeof(Texture2D),GUILayout.MaxHeight(40));
			GUILayout.EndHorizontal ();
			*/

			GUILayout.BeginHorizontal ();
			debug = EditorGUILayout.Toggle ("Generate Debug Palette", debug);
			GUILayout.EndHorizontal ();
		}
		/*
		GUILayout.Space (20);

		GUILayout.BeginHorizontal ();
		homogenizeBase = (Texture2D)EditorGUILayout.ObjectField("Homogenize Base", homogenizeBase, typeof(Texture2D),GUILayout.MaxHeight(40));
		if (GUILayout.Button ("Homogenize Images Based on Base")) {
			Homogenize ();
		}
		GUILayout.EndHorizontal ();
		*/

		GUILayout.Space (200);
		GUILayout.EndVertical ();
	}


	// HOMOGONIZE MANY IMAGES TO USE THE SAME CORRECT PALLET (removes artifacts)
	void Homogenize() {
		HomogenizeProcess (homogenizeBase);
	}

	void HomogenizeProcess (Texture2D texture) {
		Texture2D outputTexture = new Texture2D((int)texture.width, (int)texture.height);

		// get all the pixels
		Color[] pixels = GetPixels(texture);

		// build list of only unique colors found in image
		List<Color> goalColors = FindUniqueColors(pixels);

		// sort
		goalColors = SortBasedOnValue(goalColors);

		// use this to lock down color locations when matched correctly
		bool[] colorsMatched;
		List<Color> unMatchedColors;

		homogenizedTextures = new Texture2D[multiTextures.Length];

		// loop though my images to match
		for (int i = 0; i < multiTextures.Length; i++) {
			// dont bother if it's the same texture
			if (multiTextures [i] == texture)
				continue;
			// reset matched colors
			colorsMatched = new bool[goalColors.Count];
			unMatchedColors = new List<Color> ();

			Color[] pix = GetPixels(multiTextures[i]);
			List<Color> pixColors = FindUniqueColors(pix);
			pixColors = SortBasedOnValue(pixColors);
			while (pixColors.Count < goalColors.Count) {
				pixColors.Add (new Color (0, 0, 0, 0));
			}
			while (pixColors.Count > goalColors.Count) {
				if (pixColors [0].a == 0) {
					pixColors.RemoveAt (0);
					continue;
				}
				else if (pixColors [pixColors.Count - 1].a == 0) {
					pixColors.RemoveAt (0);
					continue;
				}
				float cDiff1 = (pixColors [0].r + pixColors [0].g + pixColors [0].b + pixColors [0].a) - (pixColors [1].r + pixColors [1].g + pixColors [1].b + pixColors [1].a);
				float cDiff2 = (pixColors [pixColors.Count-1].r + pixColors [pixColors.Count-1].g + pixColors [pixColors.Count-1].b + pixColors [pixColors.Count-1].a) - (pixColors [pixColors.Count-2].r + pixColors [pixColors.Count-2].g + pixColors [pixColors.Count-2].b + pixColors [pixColors.Count-2].a);
				if (Mathf.Abs(cDiff1) > Mathf.Abs(cDiff2)) {
					pixColors.RemoveAt (pixColors.Count - 1);
					continue;
				}
				else pixColors.RemoveAt (0); {
					continue;
				}
			}
			Color[] homPallet = new Color[goalColors.Count];
			bool[] matchedColorsFromGoalPalette = new bool[goalColors.Count];

			// find colors that already match
			for (int n = 0; n < pixColors.Count; n++) {
				if (CompareColors(pixColors[n], goalColors[n])) {
					colorsMatched [n] = true;
					matchedColorsFromGoalPalette [n] = true;
					homPallet [n] = goalColors [n];
				} else {
					unMatchedColors.Add (pixColors [n]);
				}
			}


			// itterate though unmatched colors, a few times, getting less picky each time.
			float margiOfError = 0.02f;
			for (int q = 0; q < 10; q++) {
				for (int n = 0; n < colorsMatched.Length; n++) {
					if (colorsMatched [n] == true)
						continue;
					for (int c = 0; c < goalColors.Count; c++) {
						if (CompareColors(pixColors[n], goalColors[c], margiOfError) && colorsMatched[c] == false) {
							colorsMatched [n] = true;
							matchedColorsFromGoalPalette [c] = true;
							homPallet [n] = goalColors [c];
						}
					}
				}
				margiOfError *= 2;
			}
				
			bool matchFailfound = false;
			string matched = "";
			for (int n = 0; n < colorsMatched.Length; n++) {
				if (colorsMatched [n])
					matched += "1";
				else {
					matched += "0";
					matchFailfound = true;
				}
			}
			if (matchFailfound)
				Debug.Log (multiTextures [i].name + " " + matched);

			// now swap all colors baed on new color key
			pix = RedrawPixelsWithNewColorScheme(pix, pixColors, homPallet);

			// saves colros
			//multiTextures[i].SetPixels( pix );
			//multiTextures[i].Apply();
			homogenizedTextures [i] = new Texture2D (multiTextures [i].width, multiTextures [i].height);
			homogenizedTextures [i].SetPixels ( pix );
			homogenizedTextures [i].Apply();
			homogenizedTextures [i].name = multiTextures [i].name;
		}

		for (int i = 0; i < homogenizedTextures.Length; i++) {
			if (homogenizedTextures [i] == null)
				homogenizedTextures [i] = multiTextures [i];
			Debug.Log (homogenizedTextures [i]);
		}
	}
		
	bool CompareColors(Color a, Color b, float margin = 0.02f) {
		if (Mathf.Abs (a.r - b.r) > margin)
			return false;
		if (Mathf.Abs (a.g - b.g) > margin)
			return false;
		if (Mathf.Abs (a.b - b.b) > margin)
			return false;
		if (Mathf.Abs (a.a - b.a) > margin)
			return false;
		return true;
	}

	Color[] GetPixels (Texture2D texture) {
		// get all the pixels
		Color[] pixels = texture.GetPixels(
			(int)0,
			(int)0, 
			(int)texture.width, 
			(int)texture.height
		);
		return pixels;
	}

	void StartConvert () {
		if (multiTextures == null)
			return;
		
		filePath = AssetDatabase.GetAssetPath (multiTextures[0].GetInstanceID ());

		string[] newfilePath = filePath.Split ('/');

		parentFilePath = "";
		filePath = "";

		// build directories
		for (int i = 0; i < newfilePath.Length; i++ ) {
			if (i + 2 < newfilePath.Length)
				parentFilePath += newfilePath [i] + "/";
			if (i + 1 < newfilePath.Length)
				filePath += newfilePath [i] + "/";
		}

		reCheckPalette = generateColorPalette;
		if (multiTextures.Length > 0) {
			for (int i = 0; i < multiTextures.Length; i++) {
				if (homogenizedTextures.Length > 0) {
					outputTexture = SpriteToTexture2D (homogenizedTextures [i]);
				} else {
					outputTexture = SpriteToTexture2D (multiTextures [i]);
				}


				string finalOutputDir = "";
				if (outputIntoParentFolder) {
					finalOutputDir = parentFilePath;
				} else {
					finalOutputDir = filePath + "/" + outputFolderName + "/";
				}

				SaveTextureToFile (outputTexture, multiTextures[i].name, finalOutputDir);
				generateColorPalette = false;
			}
		}
		generateColorPalette = reCheckPalette;
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
		Debug.Log ("Converted. Number of unique colors: " + uniqueColors.Count);
		return uniqueColors;
	}

	List<Color> SortBasedOnValue (List<Color> colorsToSort) {
		colorsToSort.Sort( (p1,p2)=>(int)((p1.r + p1.g + p1.b) * 1000).CompareTo((int)((p2.r + p2.g + p2.b) * 1000)) );
		return colorsToSort;
	}

	Color[] RedrawPixelsWithNewColorScheme(Color[] pixels, List<Color> originalScheme, Color[] newScheme) {
		for (int i = 0; i < pixels.Length; i++) {
			// compare this pixel to the unique pixel list
			for (int c = 0; c < originalScheme.Count; c++) {
				if (pixels [i] == originalScheme [c]) {
					pixels [i] = newScheme [c];
					break;
				}
			}
		}
		return pixels;
	}


	bool SimilarColor(Color a, Color b) {
		if (Mathf.Abs(a.r - b.r) > 0.1f)
			return false;
		if (Mathf.Abs(a.g - b.g) > 0.1f)
			return false;
		if (Mathf.Abs(a.b - b.b) > 0.1f)
			return false;
		if (Mathf.Abs(a.a - b.a) > 0.1f)
			return false;
		return true;
	}



	Texture2D SpriteToTexture2D (Texture2D texture) {
		Texture2D outputTexture = new Texture2D((int)texture.width, (int)texture.height);

		// get all the pixels
		Color[] pixels = GetPixels(texture);

		// build list of only unique colors found in image
		List<Color> uniqueColors = FindUniqueColors(pixels);

		// sort pixels by brightness 
		uniqueColors = SortBasedOnValue(uniqueColors);

		// if COLOR CORRECT is on
		if (colorCorrectToExample) {
			// use example's unique colors instead
			Color[] examplePixels = GetPixels(homogenizeBase);
			List<Color> forceColors = FindUniqueColors(examplePixels);
			bool[] colorExistsNaturally = new bool[forceColors.Count];

			// find out which colors we need already are on the sprite
			for (int i = 0; i < pixels.Length; i++) {
				bool matchfound = false;
				for (int c = 0; c < forceColors.Count; c++) {
					if (pixels[i] == forceColors[c]) {
						colorExistsNaturally [c] = true;
						matchfound = true;
						break;
					}
				}
			}

			// look at colors not found on sprite and add them somewhere
			for (int i = 0; i < colorExistsNaturally.Length; i++) {
				if (!colorExistsNaturally[i]) {
					// if this color is missing, find the first pixel that is similar and make it that missing color
					for (int p = 0; p < pixels.Length; p++) {
						if (SimilarColor(pixels[p], forceColors[i])) {
							pixels [p] = forceColors [i];
							break;
						}
					}
				}
			}

			// finally, only use the example's pallet
			uniqueColors = forceColors;
		}

		// creats a new grayscale matching pixel list
		Color[] orderedGreyPixels = new Color[uniqueColors.Count];
		float greyscaleStep = 1f / uniqueColors.Count;

		// create the correct greyscale color for each
		for (float i = 0; i < orderedGreyPixels.Length; i++) {
			float greyVal = i * greyscaleStep;
			orderedGreyPixels [(int)i] = new Color (greyVal, greyVal, greyVal, 1);
			if (uniqueColors [(int)i].a == 0)
				orderedGreyPixels [(int)i].a = 0;
		}
			
		pixels = RedrawPixelsWithNewColorScheme(pixels, uniqueColors, orderedGreyPixels);

		// make palette
		if (colorPaletteName != "" && generateColorPalette) {
			GeneratePalette (uniqueColors);
		}
		if (colorPaletteName != "" && reCheckPalette && homogenizedTextures.Length > 0 && texture == homogenizeBase) {
			Debug.Log ("drew hom-based palette");
			GeneratePalette (uniqueColors);
		}
		if (debug) {
			GeneratePalette (null, orderedGreyPixels, "debug-palette");
		}


		outputTexture.SetPixels( pixels );
		outputTexture.Apply();
		return outputTexture;
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
		// put in original sprite dir
		if (putPaletteInOriginalDir) {
			palettePath = filePath + "/" + paletteDirName + "/";
		}
		// put in folder in parent folder
		else if (!putPaletteInOriginalDir && outputIntoParentFolder) {
			palettePath = parentFilePath + "/" + paletteDirName + "/";
		}
		// In Subfolder
		else if (!putPaletteInOriginalDir && !outputIntoParentFolder) {
			palettePath = filePath + "/" + outputFolderName + "/";
			// check if directory exists
			if (!Directory.Exists(palettePath)) {
				Directory.CreateDirectory(palettePath);
			}
			palettePath += paletteDirName + "/";
		}

		string generatedPaletteName = colorPaletteName;
		if (specialName != "")
			generatedPaletteName = specialName;

		SaveTextureToFile( palette, generatedPaletteName, palettePath);
	}

	void SaveTextureToFile( Texture2D texture , string fileName, string _filePath = "") {
		string finalFilePath = _filePath;
		if (finalFilePath == "")
			finalFilePath = "" + Application.dataPath + "/";

		// check if directory exists
		if (!Directory.Exists(finalFilePath)) {
			Directory.CreateDirectory(finalFilePath);
		}

		byte[] bytes = texture.EncodeToPNG();
		FileStream stream = new FileStream(finalFilePath + fileName + ".png", FileMode.OpenOrCreate, FileAccess.Write);
		BinaryWriter writer = new BinaryWriter(stream);


		// do writing
		for (int i = 0; i < bytes.Length; i++) {
			writer.Write (bytes [i]);
		}

		writer.Close ();
		stream.Close ();
	}




	// COLOR CORRECT











}