using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PowerTools;

public class ChargeMeter : MonoBehaviour {

	public int team = 0;

	// bar positioning
	public float yPos = 30;
	public float centerGap = 60;

	// objects
	public GameObject bar;
	public float barPixelSize = 88;
	public AnimationClip barNormal;
	public AnimationClip barFlash;

	float facing = 1;

	bool setupComplete = false;

	GameManager gm;
	ControlManager cm;

	void Start()  {
		gm = GameManager.Instance;
		cm = gm.cm [team];
		transform.parent = Camera.main.transform;
	}

	void SetUp() {
		if (team == 1) {
			facing = 1;
		} else {
			facing = -1;
		}

		transform.localScale = new Vector3 (facing, 1);

		transform.localPosition = new Vector3 (centerGap * facing * gm.pxSize, (gm.screenSize.y * gm.pxSize * 0.5f) - yPos * gm.pxSize, 1);

		float pxDistanceFromFull = Mathf.Ceil((1) * barPixelSize) * gm.pxSize;
		bar.transform.localPosition = new Vector2 (pxDistanceFromFull, 0);

		setupComplete = true;
	}

	void Update () {
		if (team != -1 && !setupComplete) {
			SetUp ();
		}
		if (cm == null)
			return;

		float pxDistanceFromFull = Mathf.Ceil((1-cm.charge) * barPixelSize) * gm.pxSize;
		bar.transform.localPosition = new Vector2 (pxDistanceFromFull, 0);

		if (cm.charge == 1 && !bar.GetComponent<SpriteAnim> ().IsPlaying(barFlash))
			bar.GetComponent<SpriteAnim> ().Play (barFlash);
		if (cm.charge != 1 && !bar.GetComponent<SpriteAnim> ().IsPlaying(barNormal))
			bar.GetComponent<SpriteAnim> ().Play (barNormal);
	}
}
