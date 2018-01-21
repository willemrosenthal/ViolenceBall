using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PaletteSwap : MonoBehaviour
{
	public Texture PaletteTexture;
	Texture lastLookupTexture;
	float valOffset = 0;
	float colorSepVal;
	float lastVal;

	SpriteRenderer renderer;

	void Awake() {
		renderer = GetComponent<SpriteRenderer> ();
	}

	void Start() {
		colorSepVal = 1f/PaletteTexture.width;
		valOffset = colorSepVal * 0.5f;
		lastVal = valOffset;
		renderer.sharedMaterial.SetFloat("_ColSepOffset", valOffset);
		renderer.sharedMaterial.SetTexture("_PaletteTex", PaletteTexture);
		lastLookupTexture = PaletteTexture;
	}

	void Update() {
		if (!Application.isPlaying) {
			if (lastVal != valOffset) {
				lastVal = valOffset;
				renderer.sharedMaterial.SetFloat ("_ColSepOffset", valOffset);
			}
			if (lastLookupTexture != PaletteTexture) {
				renderer.sharedMaterial.SetTexture ("_PaletteTex", PaletteTexture);
				lastLookupTexture = PaletteTexture;
			}
		}
	}

	public void UpdatePaletteTexture() {
		renderer = GetComponent<SpriteRenderer> ();
		renderer.material = Instantiate (renderer.material);
		renderer.sharedMaterial.SetTexture("_PaletteTex", PaletteTexture);
		lastLookupTexture = PaletteTexture;
	}


}