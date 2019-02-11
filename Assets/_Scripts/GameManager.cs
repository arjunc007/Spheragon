using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;
    public Transform sphere;
    public float dragSpeed = 5;             //Spped of dragging action
    public static float TileRadius = 1.0f;  //Find neighbours in Tile
    public float computerWaitTime = 0.5f;
    public float aiRotSpeed = 5;
    public float maxFoV, minFoV;
    public float perspectiveZoomSpeed = 5;
    public float flickDeceleration = 5;
    private float flickDamping;

    //UI
    public GameObject pauseMenu;
    public GameObject turnIndicator;
    public GameObject rangeUpIcon;
    public GameObject invertIcon;
    public GameObject skipIcon;

    //Game Logic
    public float difficulty = 0.2f;
    private int playerTurn = 0;
    private List<Player> players = new List<Player>();
    private HashSet<Tile> freeTiles = new HashSet<Tile>();
    /// <summary>
    /// number of instances of changeneighbour currently running
    /// </summary>
    private int changeNeighbourCount = 0;
    private float zoomedRotSpeed;
    private float initialFoV;
    private Vector3 flickSpeed;

    //Game States
    [HideInInspector]
    public static bool isPaused;
    private bool pauseClicks;
    private bool turnUI;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
    }

    // Use this for initialization
    void Start () {
        InputManager.instance.ClickedEvent += OnClick;
        InputManager.instance.DraggingEvent += OnDrag;
        InputManager.instance.DragEndEvent += OnDragEnd;
        InputManager.instance.PinchEvent += OnPinch;

        //Abstract out to enable choie of colors
        Color p1Color = Color.red;
        Color p2Color = Color.blue;

        bool randomiseP1 = Random.value > 0.5f;
        
        players.Add(new Player(0, GameData.playerColor[0], GameData.isSP && randomiseP1));
        players.Add(new Player(1, GameData.playerColor[1], GameData.isSP && !randomiseP1));

        List<Tile> pentaTiles = new List<Tile>();

        Tile[] tiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in tiles)
        {
            tile.Initialise();
            freeTiles.Add(tile);
            //Keep penta Tiles separate
            if(tile.type == TileType.Pentagon)
            {
                pentaTiles.Add(tile);
            }
        }

        //Get a pentagon
        RaycastHit hit;
        
        //Get opposite pentagon by raycasting along normal
        Physics.Raycast(pentaTiles[0].transform.position, -pentaTiles[0].GetNormal() * 20, out hit);

        Tile p2Tile = hit.transform.gameObject.GetComponent<Tile>();

        //Bring first player to front
        sphere.Rotate(Vector3.Cross(pentaTiles[0].GetNormal(), Vector3.back), Vector3.Angle(pentaTiles[0].GetNormal(), Vector3.back), Space.World);

        //Initialise both by making them startTiles
        pentaTiles[0].SetOwner(players[0]);
        freeTiles.Remove(pentaTiles[0]);
        players[0].AddTile(pentaTiles[0]);
        pentaTiles.RemoveAt(0);

        p2Tile.SetOwner(players[1]);
        freeTiles.Remove(p2Tile);
        players[1].AddTile(p2Tile);
        pentaTiles.Remove(p2Tile);

        //Assign the rest of the tiles powers
        foreach (Tile tile in pentaTiles)
        {
            tile.type = (TileType)Random.Range(3, 6);

            //Instantiate icon sprite over the tile
            if(tile.type == TileType.Invert)
            {
                Instantiate(invertIcon, tile.transform.position + tile.GetNormal() * 0.01f, Quaternion.LookRotation(tile.GetNormal(), Vector3.up), tile.transform);
            }
            else if(tile.type == TileType.RangeUp)
            {
                Instantiate(rangeUpIcon, tile.transform.position + tile.GetNormal() * 0.01f, Quaternion.LookRotation(tile.GetNormal(), Vector3.up), tile.transform);
            }
            else if(tile.type == TileType.Skip)
            {
                Instantiate(skipIcon, tile.transform.position + tile.GetNormal() * 0.01f, Quaternion.LookRotation(tile.GetNormal(), Vector3.up), tile.transform);
            }
        }

        isPaused = false;
        pauseClicks = false;

        //Make the first move, if player 1 if AI
        if(players[playerTurn].IsAI())
        {
            StartCoroutine(PlayTile());
        }

        zoomedRotSpeed = dragSpeed;
        initialFoV = Camera.main.fieldOfView;

        turnIndicator.GetComponentInChildren<Image>().color = players[playerTurn].GetColor();
        turnIndicator.GetComponentInChildren<TextMeshProUGUI>().SetText((playerTurn + 1).ToString());
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

    private IEnumerator MakeMove(Tile clickedTile)
    {
        if (clickedTile != null && clickedTile.IsFree())
        {
            pauseClicks = true;
            clickedTile.ChangeTo(players[playerTurn]);

            players[playerTurn].AddTile(clickedTile);
            //Free tiles are assigned for first time, so don't need to remove from other player lists
            freeTiles.Remove(clickedTile);

            //Give Power
            if (clickedTile.type == TileType.RangeUp)
            {
                players[playerTurn].SetDepth(2);
                yield return new WaitForSecondsRealtime(Tile.transitionTime);
                StartCoroutine(ChangePlayerTurn());
            }
            else if (clickedTile.type == TileType.Invert)
            {
                HashSet<Tile> p1Tiles = new HashSet<Tile>(players[playerTurn].GetTiles());
                HashSet<Tile> p2Tiles = new HashSet<Tile>(players[playerTurn^1].GetTiles());
                players[playerTurn].GetTiles().Clear();
                yield return new WaitForSecondsRealtime(Tile.transitionTime);
                foreach (var tile in p1Tiles)
                {
                    tile.ChangeTo(players[playerTurn ^ 1]); //XOR to flip 1 and 0
                    players[playerTurn ^ 1].AddTile(tile);
                }

                foreach (var tile in p2Tiles)
                {
                    tile.ChangeTo(players[playerTurn]);
                    players[playerTurn].AddTile(tile);
                }

                players[playerTurn ^ 1].GetTiles().IntersectWith(p1Tiles);
                yield return new WaitForSecondsRealtime(Tile.transitionTime);
                StartCoroutine(ChangePlayerTurn());
            }
            else if(clickedTile.type == TileType.Skip)
            {
                ///Add animation and stuff

                //Change player turn
                playerTurn ^= 1;
                yield return new WaitForSecondsRealtime(Tile.transitionTime);
                StartCoroutine(ChangePlayerTurn());
            }
            else if (players[playerTurn].Depth() > 1)
            {
                //Changes player turn counter at end of coroutine
                StartCoroutine(ChangeNeighbours(clickedTile, players[playerTurn].Depth()));
                //If depth >1, reduce by 1
                players[playerTurn].SetDepth(players[playerTurn].Depth() - 1);
            }
            else
            {
                //Changes player turn counter at end of coroutine
                StartCoroutine(ChangeNeighbours(clickedTile, players[playerTurn].Depth()));
            }
        }
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSecondsRealtime(2 * Tile.transitionTime);

        Transform uiCanvas = pauseMenu.transform.parent;
        //Show final score
        for (int i = 1; i < uiCanvas.childCount; i++)
            uiCanvas.GetChild(i).gameObject.SetActive(false);

        Transform endMenu = uiCanvas.GetChild(0);
        if (players[0].GetScore() > players[1].GetScore())
        {
            endMenu.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = players[0].GetScore().ToString();
            endMenu.GetChild(0).GetComponent<Image>().color = players[0].GetColor();
            endMenu.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = players[1].GetScore().ToString();
            endMenu.GetChild(1).GetComponent<Image>().color = players[1].GetColor();
        }
        else
        {
            endMenu.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = players[1].GetScore().ToString();
            endMenu.GetChild(0).GetComponent<Image>().color = players[1].GetColor();
            endMenu.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = players[0].GetScore().ToString();
            endMenu.GetChild(1).GetComponent<Image>().color = players[0].GetColor();
        }

        uiCanvas.GetChild(0).gameObject.SetActive(true);

    }

    private IEnumerator ChangeNeighbours(Tile tile, int depth)
    {
        changeNeighbourCount++;

        yield return new WaitForSecondsRealtime(Tile.transitionTime);

        foreach (Tile neighbour in tile.neighbours)
        {
            //Do not run for pentagons
            if (neighbour.type != TileType.Hexagon)
                continue;

            //Check if neighbour is empty or owned
            if (neighbour.CheckOwner(playerTurn ^ 1))
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
            }

            //Do for all neighbors of changed tile
            if (depth > 1)
            {
                StartCoroutine(ChangeNeighbours(neighbour, depth - 1));
            }
        }

        changeNeighbourCount--;

        //Increment player turn counter because turn has ended and all neighbours visited
        if (changeNeighbourCount == 0)
        {
            StartCoroutine(ChangePlayerTurn());
        }
    }

    private IEnumerator ChangePlayerTurn()
    {
        pauseClicks = false;
        
        yield return StartCoroutine(ChangeTurnUI());

        //For next move, check again, if player if AI
        if (players[playerTurn].IsAI())
        {
            StartCoroutine(PlayTile());
        }
    }

    private IEnumerator ChangeTurnUI()
    {
        //rotate to mid
        Quaternion startRot = turnIndicator.transform.GetChild(0).rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 90, 0);
        float dt = 0;

        while (turnIndicator.transform.GetChild(0).rotation != endRot)
        {
            turnIndicator.transform.GetChild(0).rotation = Quaternion.Lerp(startRot, endRot, dt / 0.1f);
            dt += Time.deltaTime;
            yield return null;
        }

        //change turn & color
        playerTurn ^= 1;
        turnIndicator.GetComponentInChildren<Image>().color = players[playerTurn].GetColor();

        //Change text
        turnIndicator.GetComponentInChildren<TextMeshProUGUI>().SetText((playerTurn + 1).ToString());
        //rotate rest of the way
        startRot = turnIndicator.transform.GetChild(0).rotation;
        endRot = startRot * Quaternion.Euler(0, 90, 0);
        dt = 0;
        while (turnIndicator.transform.GetChild(0).rotation != endRot)
        {
            turnIndicator.transform.GetChild(0).rotation = Quaternion.Lerp(startRot, endRot, dt / 0.1f);
            dt += Time.deltaTime;
            yield return null;
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
            StartCoroutine(MakeMove(clickedTile));

            if (freeTiles.Count < 1)
            {
                StartCoroutine(EndGame());
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
        if(speed.magnitude > 500)
        {
            flickSpeed = speed * dragSpeed;
            StartCoroutine(KeepRotating());
        }
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
            flickDamping = flickSpeed.magnitude * -flickDeceleration;
            Debug.Log(flickSpeed.magnitude);
            sphere.Rotate(Vector3.Cross(flickSpeed, Vector3.back), Space.World);
            Vector3 newSpeed = flickSpeed - flickSpeed.normalized * Time.deltaTime * flickDamping;
            if (Vector3.Dot(newSpeed, flickSpeed) > 0)
                flickSpeed = newSpeed;
            else
                flickSpeed.Set(0, 0, 0);
            yield return null;
        }
    }

    private IEnumerator PlayTile()
    {
        //Disable user clicks while AI running
        pauseClicks = true;

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
        StartCoroutine(MakeMove(tileToPlay));
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

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}