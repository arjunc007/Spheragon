using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum Shape
{
    Pentagon,
    Hexagon
};

public class GameManager : MonoBehaviour {

    public Transform sphere;
    public float dragSpeed = 5;             //Spped of dragging action
    public static float TileRadius = 0.8f;  //Find neighbours in Tile
    public float computerWaitTime = 0.5f;
    public float aiRotSpeed = 5;
    public float maxFoV, minFoV;
    public float perspectiveZoomSpeed = 5;
    public float flickDamping = 0.7f;

    //UI
    public GameObject pauseMenu;
    public Text[] scoreText;
    public TextMeshProUGUI scoreSeparator;

    //Game Logic
    public float difficulty = 0.2f;
    private int playerTurn = 0;
    private List<Player> players = new List<Player>();
    private HashSet<Tile> freeTiles = new HashSet<Tile>();
    private int changeNeighbourCount = 0;
    private float zoomedRotSpeed;
    private float initialFoV;
    private Vector3 flickSpeed;

    //Game States
    [HideInInspector]
    public static bool isPaused;
    private bool pauseClicks;

    // Use this for initialization
    void Start () {
        InputManager.instance.ClickedEvent += OnClick;
        InputManager.instance.DraggingEvent += OnDrag;
        InputManager.instance.DragEndEvent += OnDragEnd;
        InputManager.instance.PinchEvent += OnPinch;

        Color p1Color = Color.red;
        Color p2Color = Color.blue;

        bool randomiseP1 = Random.value > 0.5f;
        
        players.Add(new Player(0, p1Color, GameData.isSP && randomiseP1));
        players.Add(new Player(1, p2Color, GameData.isSP && !randomiseP1));

        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles)
        {
            freeTiles.Add(tile);
        }

        isPaused = false;
        pauseClicks = false;

        //Update Score colors
        scoreSeparator.colorGradient = scoreSeparator.colorGradient = new VertexGradient(p1Color, p2Color, p1Color, p2Color);
        for (int i = 0; i < players.Count; i++)
        {
            scoreText[i].color = players[i].GetColor();
        }

        //Make the first move, if player 1 if AI
        if(players[playerTurn].IsAI())
        {
            StartCoroutine(PlayTile());
        }

        zoomedRotSpeed = dragSpeed;
        initialFoV = Camera.main.fieldOfView;
    }

    public void PauseGame()
    {
        isPaused = !isPaused;
        pauseClicks = !pauseClicks;

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

    private void MakeMove(Tile clickedTile)
    {
        if (clickedTile != null && clickedTile.IsFree())
        {
            pauseClicks = true;
            clickedTile.ChangeTo(players[playerTurn]);
            players[playerTurn].AddTile(clickedTile);
            //Free tiles are assigned for first time, so don't need to remove from other player lists
            freeTiles.Remove(clickedTile);

            StartCoroutine(ChangeNeighbours(clickedTile, players[playerTurn].Depth()));
        }
    }

    private void EndGame()
    {
        //Show final score
        //Restart
        //Home
        //
    }

    private IEnumerator ChangeNeighbours(Tile tile, int depth)
    {
        changeNeighbourCount++;

        //Update Score
        for (int i = 0; i < players.Count; i++)
        {
            scoreText[i].text = players[i].GetScore().ToString();
        }

        yield return new WaitForSeconds(Tile.transitionTime);

        foreach (Tile neighbour in tile.neighbours)
        {
            //Check if neighbour is empty or owned
            if (neighbour.IsFree() || neighbour.CheckOwner(playerTurn))
                continue;
            else
            {
                //Neighbor is occupied by other player
                neighbour.ChangeTo(players[playerTurn]);

                players[playerTurn].AddTile(neighbour);

                //Remove from everyone else
                foreach (var player in players)
                {
                    if (player.GetID() != playerTurn)
                        player.RemoveTile(neighbour);
                }

                //Do for all neighbors of changed tile
                if (--depth > 0)
                {
                    StartCoroutine(ChangeNeighbours(neighbour, depth));
                }
                else
                {
                    //Update Score
                    for (int i = 0; i < players.Count; i++)
                    {
                        scoreText[i].text = players[i].GetScore().ToString();
                    }
                }
            }
        }

        changeNeighbourCount--;

        //Increment player turn counter because turn has ended and all neighbours visited
        if (changeNeighbourCount == 0)
        {
            pauseClicks = false;
            playerTurn = ++playerTurn > players.Count - 1 ? 0 : playerTurn;

            //For next move, check again, if player if AI
            if (players[playerTurn].IsAI())
            {
                StartCoroutine(PlayTile());
            }
        }
    }

    public void OnClick(Vector3 pos)
    {
        flickSpeed = Vector3.zero;
        if (!pauseClicks)
        {
            //Do click actions
            Tile clickedTile = GetClickedTile(Input.mousePosition);
            //Debug.Log(clickedTile);
            MakeMove(clickedTile);

            if (freeTiles.Count < 1)
            {
                EndGame();
            }
        }
    }

    //Rotate sphere in world space on drag
    public void OnDrag(Vector3 diff)
    {
        flickSpeed = Vector3.zero;
        if (!isPaused)
        {
            Vector3 axis = Vector3.Cross(diff, Vector3.forward).normalized;

            sphere.Rotate(axis * zoomedRotSpeed, Space.World);
        }
    }

    public void OnDragEnd(Vector3 speed)
    {
        //Debug.LogFormat("Ended drag with speed {0}", speed.magnitude);
        flickSpeed = speed * dragSpeed;
        StartCoroutine(KeepRotating());
    }

    public void OnPinch(float diff)
    {
        Camera camera = Camera.main;
        //Change the field of view based on the change in distance between the touches.
        camera.fieldOfView += diff * perspectiveZoomSpeed;

        // Clamp the field of view to make sure it's between 0 and 180.
        camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, minFoV, maxFoV);

        zoomedRotSpeed = dragSpeed + dragSpeed * (initialFoV - camera.fieldOfView) / (maxFoV - minFoV);
    }

    private IEnumerator KeepRotating()
    {
        while(flickSpeed.magnitude > 0)
        {
            sphere.Rotate(Vector3.Cross(flickSpeed, Vector3.back), Space.World);
            flickSpeed *= flickDamping;
            yield return null;
        }
    }

    private IEnumerator PlayTile()
    {

        if (freeTiles.Count == 0)
            yield break;

        List<Tile> playableTiles = new List<Tile>(freeTiles);

        Tile tileToPlay = playableTiles[0];

        //Get tile with max blue neighbors
        playableTiles.Sort((a, b) => a.GetDiffNeighbours(playerTurn).CompareTo(b.GetDiffNeighbours(playerTurn)));
        foreach (Tile tile in playableTiles)
        {
            if (tile.GetDiffNeighbours(playerTurn) > tileToPlay.GetDiffNeighbours(playerTurn))
                tileToPlay = tile;
        }

        //Remove this one from possibilities
        playableTiles.Remove(tileToPlay);

        //Either select the best choice, or pick a random tile from the rest
        float predictor = Random.Range(0.0f, 1.0f);
        if (playableTiles.Count > 1 && predictor > difficulty)
        {
            tileToPlay = playableTiles[Random.Range(0, playableTiles.Count)];
        }

        yield return new WaitForSeconds(computerWaitTime);

        //Check if tile in view, then rotate if not.
        yield return StartCoroutine(BringTileToFront(tileToPlay));

        //Get the tile by some logic above, and then call this function
        MakeMove(tileToPlay);
        
        //uiManager.SetTurnText(playerTurn);
    }

    IEnumerator BringTileToFront(Tile t)
    {
        while (Vector3.Angle(t.GetNormal(), Vector3.back) > 20)
        {
            float step = aiRotSpeed * Time.deltaTime;
            sphere.Rotate(Vector3.Cross(t.GetNormal(), Vector3.back), step, Space.World);

            yield return null;
        }
    }
}