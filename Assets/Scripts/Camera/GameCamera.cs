using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour {

	public GameObject focalPoint;

	public float verticalOffset = 3;
	public float camSmoothTime = 0.35f;
	Vector3 camSmoothPosition;

	Bounds camRect;
	PixelPerfectCamera pixelPerfectCamera;
	GameManager gm;

	void Awake() {
		gm = GameManager.Instance;
		gm.camera = this;
		pixelPerfectCamera = GetComponent<PixelPerfectCamera> ();
		if (pixelPerfectCamera) {
			camRect = new Bounds (Vector2.zero, new Vector2 (pixelPerfectCamera.gamePixelWidth * gm.pxSize, pixelPerfectCamera.gamePixelHeight * gm.pxSize));
		}

	}

	void Update () {
		SmoothCameraPosition ();
	}


	void SmoothCameraPosition () {
		Vector3 targetPosition = focalPoint.transform.position;
		targetPosition.z = transform.position.z;
		transform.position = Vector3.SmoothDamp (transform.position, targetPosition, ref camSmoothPosition, camSmoothTime);
	}

	public bool OnCamera(Vector2 point, float margin = 0) {
		playerPt = point;
		Rect cameraRect = new Rect ((Vector2)transform.position - Vector2.right * (camRect.size.x * 0.5f + margin * -0.5f) - Vector2.up * (camRect.size.y * 0.5f + margin * -0.5f), camRect.size);
		cameraRect.size = new Vector2 (cameraRect.size.x - margin, cameraRect.size.y - margin);
		camRectangle = cameraRect;
		return cameraRect.Contains (point);
	}

	Vector2 playerPt;
	Rect camRectangle;
	void OnDrawGizmos () {
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube (camRectangle.center, camRectangle.size);

		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere (playerPt, 0.2f);
	}
}
