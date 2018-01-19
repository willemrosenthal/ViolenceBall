using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PixelPerfectSprite : MonoBehaviour {

	public bool runContinuously = true;
	public bool debug;

	// calculation vectors
	Vector2 spriteMin;
	Vector2 minToPos;
	Vector2 pixelPerfectMin = Vector2.zero;
	Vector2 pixelPerfectOffset;
	Vector2 pixelPerfectPosition;

	GameObject grid;

	float gamePixelHeight;
	float gamePixelWidth;
	float pixPerUnit = 32;

	GameManager gm = null;
	SpriteRenderer renderer = null;

	public bool localPos;

	void Awake () {
		gm = GameManager.Instance;
		renderer = GetComponent<SpriteRenderer> ();

		if (gm != null)
			pixPerUnit = gm.pixPerUnit;
		gamePixelHeight = Camera.main.GetComponent<PixelPerfectCamera> ().gamePixelHeight;
		gamePixelWidth = gamePixelHeight * Camera.main.aspect;
	}

	void Update () {
		if (runContinuously || !Application.isPlaying) {
			Vector3 pos = transform.position;
			if (localPos)
				pos = transform.localPosition;

			if (renderer == null)
				renderer = GetComponent<SpriteRenderer> ();
			if (renderer.sprite == null)
				return;
				
			spriteMin = (Vector2)renderer.sprite.bounds.min + (Vector2)pos;

			minToPos = (Vector2)pos - (Vector2)renderer.sprite.bounds.min;

			pixelPerfectMin.x = Mathf.Floor (spriteMin.x * pixPerUnit + 0.001f) / pixPerUnit;
			pixelPerfectMin.y = Mathf.Floor (spriteMin.y * pixPerUnit + 0.001f) / pixPerUnit;

			pixelPerfectOffset = spriteMin - pixelPerfectMin;
			if (!localPos)
				transform.position = (Vector2)renderer.sprite.bounds.min - pixelPerfectOffset + minToPos;
			else transform.localPosition = (Vector2)renderer.sprite.bounds.min - pixelPerfectOffset + minToPos;
		}
	}

	bool OddNumber(float n) {
		return (n % 2 == 1);
	}

	void MakeGrid() {
		grid = new GameObject ();
		grid.AddComponent<Grid> ();
		grid.GetComponent<Grid> ().cellSize = new Vector3 (1 / pixPerUnit, 1 / pixPerUnit, 1 / pixPerUnit);
	}

	void OnDrawGizmosSelected() {
		if (!debug && grid) {
			Destroy (grid);
		}

		if (debug) {
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere (spriteMin, 0.01f);
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere (pixelPerfectMin, 0.01f);


			Gizmos.color = Color.black;
			Gizmos.DrawWireSphere (transform.position, 0.01f);
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere ((Vector2)renderer.sprite.bounds.min - pixelPerfectOffset + minToPos, 0.01f);

			if (!grid) {
				MakeGrid ();
			}
			grid.transform.position = Vector3.zero;
		}
	}
}
