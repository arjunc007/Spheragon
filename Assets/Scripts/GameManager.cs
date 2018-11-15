using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Shape
{
    Pentagon,
    Hexagon
};

public class GameManager : MonoBehaviour {

    //TileGeneration
    public static float TileRadius = 0.8f;

    //Camera
    private float cameraSpeed = 5.0f;

    //Input Controls
    private Vector3 initialMousePosition;
    private Vector3 finalMousePosition;
    public bool dragging = false;

    // Use this for initialization
    void Start () {
        InputManager.instance.ClickedEvent += OnClick;
        InputManager.instance.DraggingEvent += OnDrag;
        InputManager.instance.DragEndEvent += OnDragEnd;
	}
	
	// Update is called once per frame
	void Update () {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    initialMousePosition = Input.mousePosition;
        //    return;
        //}

        //if (Input.GetMouseButtonUp(0))
        //{
        //    finalMousePosition = Input.mousePosition;
        //    if (initialMousePosition == finalMousePosition)
        //    {
        //        //Do click actions
        //        //Tile clickedTile = GetClickedTile(Input.mousePosition);

        //        //if (clickedTile != null && clickedTile.owner == 0)
        //        //{
        //        //    if (playerTurn == 1)
        //        //    {
        //        //        clickedTile.ChangeColor(playerTurn);
        //        //        uiManager.SetTurnText(playerTurn);
        //        //    }
        //        //    gridManager.freeTiles.Remove(clickedTile);
        //        //    playerTurn = playerTurn == 1 ? ++playerTurn : --playerTurn;
        //        //}
        //        return;
        //    }
        //}
        //else
        //{
        //    //Do camera things
        //    MoveCamera(finalMousePosition - initialMousePosition);
        //}

        ////Implementing Zooming in and out
        //if (Input.mouseScrollDelta.y != 0)
        //{
        //    ZoomCamera(Input.mouseScrollDelta.y);
        //}
    }

    public void OnClick(Vector3 pos)
    {
        Debug.LogFormat("Clicked at {0}", pos);
    }

    public void OnDrag(Vector3 diff)
    {
        Debug.LogFormat("Dragging: {0}", diff);
    }

    public void OnDragEnd(Vector3 speed)
    {
        Debug.LogFormat("Ended drag with speed {0}", speed.magnitude);
    }

    private void MoveCamera(Vector3 direction)
    {
        //Remove hardcoding
        cameraSpeed = Mathf.Min(direction.magnitude, 20);
        Vector3 temp = Camera.main.transform.position;
        //Debug.Log ();
        temp += Camera.main.ScreenToViewportPoint(-direction) * cameraSpeed * Time.deltaTime;

        //temp.x = temp.x > edgeDistance.z ? edgeDistance.z : temp.x < edgeDistance.x ? edgeDistance.x : temp.x;
        //temp.y = temp.y > edgeDistance.y ? edgeDistance.y : temp.y < edgeDistance.w ? edgeDistance.w : temp.y;

        Camera.main.transform.position = temp;
    }

    private void ZoomCamera(float zoom)
    {
        float temp = Camera.main.orthographicSize;

        temp += zoom;
        //Liable to change
        temp = temp > 8.2f ? 8.2f : temp < 5f ? 5f : temp;

        Camera.main.orthographicSize = temp;
    }
}
