using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStretch : MonoBehaviour {

	// 240 x 400

	Camera cam;
	GameCamera gameCamera;
	public float height = 1f;
	public float width = 1f;
	public Vector2 ratio = new Vector2();
	// having this on can mess up the resolution when combined with the pixel perfect script
	public bool updateEveryFrame = false;

	// Use this for initialization
	void Awake () 
	{
		cam = GetComponent<Camera>();
		gameCamera = GetComponent<GameCamera> ();
		// manually set ratio
		if (ratio.x != 0 && ratio.y != 0) {
			width = ((ratio.y / ratio.x) / 0.5625f);
		}
		SetRatio ();
	}


	void Update () {
		if (updateEveryFrame)
			SetRatio ();
	}

	void SetRatio() {
		//stretch view//
		cam.ResetProjectionMatrix();
		var m = cam.projectionMatrix;

		m.m11*=height;
		m.m00*=width;
		cam.projectionMatrix = m;
		//if (gameCamera.enabled)
		//	gameCamera.RecalculateCameraAfterStretch ();
	}
}