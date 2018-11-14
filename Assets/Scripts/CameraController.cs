using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	
	public float zoomSpeed;
	public float maxZoom = -5.0f;
	public float minZoom = -15.0f;
	private float zoomTo;
	void Update() {
		if (Input.GetButton ("Vertical")) {
			if (Input.GetKey (KeyCode.UpArrow)) {
				zoomTo = transform.position.z + zoomSpeed;
			} else if (Input.GetKey (KeyCode.DownArrow)) {
				zoomTo = transform.position.z - zoomSpeed;
			}
			if (zoomTo > maxZoom)
				zoomTo = maxZoom;
			if (zoomTo < minZoom)
				zoomTo = minZoom;
			transform.Translate (0.0f, 0.0f, zoomTo);
		}
	}
}
