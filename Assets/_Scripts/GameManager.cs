using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Shape
{
    Pentagon,
    Hexagon
};

public class GameManager : MonoBehaviour {

    public Transform sphere;
    public float dragSpeed = 5;
    //TileGeneration
    public static float TileRadius = 0.8f;

    //Input Controls
    private Vector3 initialMousePosition;
    private Vector3 finalMousePosition;
    public bool dragging = false;

    //Game Variables
    private int playerTurn = 1;

    // Use this for initialization
    void Start () {
        InputManager.instance.ClickedEvent += OnClick;
        InputManager.instance.DraggingEvent += OnDrag;
        InputManager.instance.DragEndEvent += OnDragEnd;
        
        Color P1Color = Color.red;
        Color P2Color = Color.blue;
    }
	
	// Update is called once per frame
	void Update () {
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

        ////Implementing Zooming in and out
        //if (Input.mouseScrollDelta.y != 0)
        //{
        //    ZoomCamera(Input.mouseScrollDelta.y);
        //}
    }

    private Tile GetClickedTile(Vector3 mousePosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.GetComponent<Tile>();
        }

        return null;
    }

    public void OnClick(Vector3 pos)
    {
        Debug.LogFormat("Clicked at {0}", pos);
        //Do click actions
        Tile clickedTile = GetClickedTile(Input.mousePosition);
        Debug.Log(clickedTile);
        
        if (clickedTile != null && clickedTile.IsFree())
        {
            Color color;
            if (playerTurn == 1)
            {
                color = Color.red;
                //        uiManager.SetTurnText(playerTurn);
            }
            else
                color = Color.blue;

            clickedTile.ChangeColor(color);
            clickedTile.SetOwner(playerTurn);
            //    gridManager.freeTiles.Remove(clickedTile);
            playerTurn = ++playerTurn > 2 ? 1 : playerTurn;
        }
    }

    //Rotate sphere in world space on drag
    public void OnDrag(Vector3 diff)
    {
        Vector3 axis = Vector3.Cross(diff, Vector3.forward).normalized;

        sphere.Rotate(axis * dragSpeed, Space.World);
    }

    public void OnDragEnd(Vector3 speed)
    {
        Debug.LogFormat("Ended drag with speed {0}", speed.magnitude);
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
