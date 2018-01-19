using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PixelPerfectCamera : MonoBehaviour {

	public bool pixelPerfectSize = true;
	public float gamePixelHeight = 224;
	public float gamePixelWidth = 384;

	public bool pixelPerfectPosition = true;
	public float pixPerUnit = 32;
	Vector2 pixelPerfectPos;

	GameManager gm;
	Camera cam;

	void Awake () {
		gm = GameManager.Instance;
		cam = GetComponent<Camera> ();
		if (gm) {
			pixPerUnit = gm.pixPerUnit;
		}
	}

	void Start() {
		if (pixelPerfectSize)
			cam.orthographicSize = ((float)gamePixelHeight / pixPerUnit) * 0.5f;
	}

	void Update() {
		if (!pixelPerfectPosition)
			return;

		pixelPerfectPos = (Vector2)transform.position;
		pixelPerfectPos.x = Mathf.Floor (pixelPerfectPos.x * pixPerUnit + 0.001f) / pixPerUnit;
		pixelPerfectPos.y = Mathf.Floor (pixelPerfectPos.y * pixPerUnit + 0.001f) / pixPerUnit;

		transform.position = (Vector3)pixelPerfectPos + Vector3.forward * transform.position.z;
	}

}
