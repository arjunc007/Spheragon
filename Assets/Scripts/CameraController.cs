using UnityEngine;

public class CameraController : MonoBehaviour {

    public Transform target = null;
    public float dragSpeed = 5;
	public float zoomSpeed;
	public float maxZoom = -5.0f;
	public float minZoom = -15.0f;
	private float zoomTo;

    private void Start()
    {
    }

    void Update() {
        //Get axis from inputManager
        //transform.RotateAround(target.transform.position, new Vector3(0, 1, 0), 0.5f);
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

    public void OnDrag(Vector3 diff)
    {
        Debug.LogFormat("Dragging in direction {0}", diff);
        Vector3 axis = Vector3.Cross(diff, -transform.forward).normalized;
        Debug.Log(axis);
        transform.RotateAround(target.transform.position, axis, dragSpeed);
        transform.LookAt(target);
    }

    public void OnDragEnd(Vector3 speed)
    {
        Debug.LogFormat("Ended drag with speed {0}", speed.magnitude);
    }
}
