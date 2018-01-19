using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour {

	public GameObject focalPoint;

	public float verticalOffset = 3;
	public float camSmoothTime = 0.35f;
	Vector3 camSmoothPosition;

	void Awake() {
		GameManager.Instance.camera = this;
	}

	void Update () {
		SmoothCameraPosition ();
	}


	void SmoothCameraPosition () {
		Vector3 targetPosition = focalPoint.transform.position;
		targetPosition.z = transform.position.z;
		transform.position = Vector3.SmoothDamp (transform.position, targetPosition, ref camSmoothPosition, camSmoothTime);
	}
}
