using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Shape
{
    Pentagon,
    Hexagon
};

public class GameManager : MonoBehaviour {

    class Player
    {
        Color color;

        public Player(Color c)
        {
            color = c;
        }

        public Color GetColor()
        {
            return color;
        }
    }

    public Transform sphere;
    public float dragSpeed = 5;             //Spped of dragging action
    public static float TileRadius = 0.8f;  //Find neighbours in Tile
    public GameObject pauseMenu;

    //Input Controls
    private Vector3 initialMousePosition;
    private Vector3 finalMousePosition;

    //Game Logic
    private int playerTurn = 0;
    private List<Player> players = new List<Player>();
    private List<Tile> freeTiles = new List<Tile>();

    //Game States
    [HideInInspector]
    public static bool isPaused;

    // Use this for initialization
    void Start () {
        InputManager.instance.ClickedEvent += OnClick;
        InputManager.instance.DraggingEvent += OnDrag;
        InputManager.instance.DragEndEvent += OnDragEnd;

        players.Add(new Player(Color.red));
        players.Add(new Player(Color.blue));

        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles)
        {
            freeTiles.Add(tile);
        }

        isPaused = false;
    }
	
	// Update is called once per frame
	void Update () {
       
        ////Implementing Zooming in and out
        //if (Input.mouseScrollDelta.y != 0)
        //{
        //    ZoomCamera(Input.mouseScrollDelta.y);
        //}
    }

    public void PauseGame()
    {
        isPaused = !isPaused;

        if(isPaused)
        {
            pauseMenu.SetActive(true);
        }
        else
        {
            pauseMenu.SetActive(false);
        }
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
        if (!isPaused)
        {
            Debug.LogFormat("Clicked at {0}", pos);
            //Do click actions
            Tile clickedTile = GetClickedTile(Input.mousePosition);
            Debug.Log(clickedTile);

            if (clickedTile != null && clickedTile.IsFree())
            {
                clickedTile.ChangeTo(playerTurn, players[playerTurn].GetColor());
                freeTiles.Remove(clickedTile);
                playerTurn = ++playerTurn > players.Count - 1 ? 0 : playerTurn;
            }

            if (freeTiles.Count < 1)
            {
                Debug.Log("Game Over");
            }
        }
    }

    //Rotate sphere in world space on drag
    public void OnDrag(Vector3 diff)
    {
        if (!isPaused)
        {
            Vector3 axis = Vector3.Cross(diff, Vector3.forward).normalized;

            sphere.Rotate(axis * dragSpeed, Space.World);
        }
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
