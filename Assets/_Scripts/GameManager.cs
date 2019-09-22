using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;
    public GameObject SphereGrid;
    private Transform sphere;
    public float dragSpeed = 5;             //Spped of dragging action
    public static float TileRadius = 1.0f;  //Find neighbours in Tile
    public float computerWaitTime = 0.5f;
    public float aiRotSpeed = 5;
    public float maxFoV, minFoV;
    public float perspectiveZoomSpeed = 5;
    public float flickThreshold = 0.5f;

    //UI
    public Transform HUD;
    /// <summary>
    /// Direct parent of winner and loser scores
    /// </summary>
    public Transform endMenu;
    public Transform turnIndicator;
    public Transform powerIndicator;
    public Transform scoreIndicator;
    public Sprite rangeUpImage;
    public Sprite invertImage;
    public Sprite skipImage;
    public GameObject powerParticles;
    public AudioClip powerTapSound;

    //Game Logic
    public float difficulty = 0.2f;
    private int playerTurn;
    private List<Player> players = new List<Player>();
    private HashSet<Tile> freeTiles = new HashSet<Tile>();
    int numHexTiles = 0;
    /// <summary>
    /// number of instances of changeneighbour currently running
    /// </summary>
    private int changeNeighbourCount = 0;
    private new Camera camera;
    private float zoomedRotSpeed;
    private float initialFoV;
    private Vector3 flickSpeed;

    //Game States
    [HideInInspector]
    bool SPGame;
    public static bool isPaused = true;
    private bool pauseClicks = true;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
    }

    private void Start()
    {
        camera = Camera.main;
        InputManager.instance.ClickedEvent += OnClick;
        InputManager.instance.DraggingEvent += OnDrag;
        InputManager.instance.DragEndEvent += OnDragEnd;
        InputManager.instance.PinchEvent += OnPinch;
    }

    // Use this for initialization
    public void Initialise (bool isSP)
    {
        players.Clear();

        SPGame = isSP;
        players.Add(new Player(0, GameData.colorChoices[GameData.playerColorIndex[0]]));
        players.Add(new Player(1, GameData.colorChoices[GameData.playerColorIndex[1]], isSP));

        playerTurn = Random.value > 0.5f ? 1 : 0;

        HUD.gameObject.SetActive(true);
        sphere = Instantiate(SphereGrid).transform;

        List<Tile> pentaTiles = new List<Tile>();

        Tile[] tiles = FindObjectsOfType<Tile>();

        freeTiles.Clear();

        if (tiles == null)
        {
            return;
        }

        foreach (Tile tile in tiles)
        {
            tile.Initialise();
            freeTiles.Add(tile);
            //Keep penta Tiles separate
            if(tile.type == TileType.Pentagon)
            {
                tile.SetTapSound(powerTapSound);
                pentaTiles.Add(tile);
            }
        }

        numHexTiles = freeTiles.Count - pentaTiles.Count;

        //Get a pentagon
        RaycastHit hit;
        
        //Get opposite pentagon by raycasting along normal
        Physics.Raycast(pentaTiles[0].transform.position, -pentaTiles[0].GetNormal() * 20, out hit);

        Tile p2Tile = hit.transform.gameObject.GetComponent<Tile>();

        //Bring first player to front
        if(playerTurn == 0)
            sphere.Rotate(Vector3.Cross(pentaTiles[0].GetNormal(), Vector3.back), Vector3.Angle(pentaTiles[0].GetNormal(), Vector3.back), Space.World);
        else
        {
            sphere.Rotate(Vector3.Cross(p2Tile.GetNormal(), Vector3.back), Vector3.Angle(p2Tile.GetNormal(), Vector3.back), Space.World);
        }
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
        int numInverts = 2, numSkips = 5, numRange = 3;

        while(pentaTiles.Count > 0)
        {
            //Get a random Tile
            Tile tile = pentaTiles[Random.Range(0, pentaTiles.Count)];

            //Instantiate icon sprite over the tile
            GameObject powerIcon = new GameObject("Icon");
            powerIcon.transform.position = tile.transform.position + tile.GetNormal() * 0.01f;
            powerIcon.transform.rotation = Quaternion.LookRotation(tile.GetNormal(), Vector3.up);
            powerIcon.transform.localScale *= 0.2f;
            powerIcon.transform.parent = tile.transform;
            SpriteRenderer sr = powerIcon.AddComponent<SpriteRenderer>();

            if (numInverts > 0)
            {
                tile.type = TileType.Invert;
                sr.sprite = invertImage;
                numInverts--;
            }
            else if(numSkips > 0)
            {
                tile.type = TileType.Skip;
                sr.sprite = skipImage;
                numSkips--;
            }
            else if(numRange > 0)
            {
                tile.type = TileType.RangeUp;
                sr.sprite = rangeUpImage;
                numRange--;
            }
            else
            {
                break;
            }

            //Attach particles
            Instantiate(powerParticles, tile.transform.position, Quaternion.LookRotation(tile.GetNormal(), Vector3.up), tile.transform);

            pentaTiles.Remove(tile);
        }

        isPaused = false;
        pauseClicks = false;

        //Make the first move, if player 1 if AI
        if(players[playerTurn].IsAI())
        {
            StartCoroutine(PlayTile());
        }

        zoomedRotSpeed = dragSpeed;
        initialFoV = camera.fieldOfView;

        //Initialise turn and score indicators
        turnIndicator.GetComponentInChildren<Image>().color = players[playerTurn].GetColor();
        turnIndicator.GetComponentInChildren<TextMeshProUGUI>().SetText((playerTurn + 1).ToString());

        scoreIndicator.GetChild(0).GetChild(0).GetComponent<Image>().color = players[0].GetColor();
        scoreIndicator.GetChild(0).GetChild(1).GetComponent<Image>().color = players[1].GetColor();
        scoreIndicator.GetChild(0).GetComponent<Image>().color = players[1].GetColor();
        scoreIndicator.GetChild(1).GetComponent<Image>().color = players[0].GetColor();
        scoreIndicator.GetChild(1).GetComponent<Image>().fillAmount = 0.5f;

        endMenu.gameObject.SetActive(false);
    }

    public void PauseGame()
    {
        isPaused = !isPaused;
        //pauseClicks = !pauseClicks;
        MenuScript.instance.TogglePauseMenu(isPaused);
    }

    private Tile GetClickedTile(Vector3 mousePosition)
    {
        Ray ray = camera.ScreenPointToRay(mousePosition);
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

            //Free tiles are assigned for first time, so don't need to remove from other player lists
            freeTiles.Remove(clickedTile);

            //Give tile to a player
            //remove power
            //Temp comment for refactoring
            //Power behaviour
            //wait
            //changeturn

            //Give Power
            if (clickedTile.type == TileType.RangeUp)
            {
                clickedTile.ChangeTo(players[playerTurn]);

                players[playerTurn].AddTile(clickedTile);

                clickedTile.RemovePowerEffect();
                players[playerTurn].SetDepth(2);
                yield return new WaitForSecondsRealtime(Tile.transitionTime);

                if (players[playerTurn].ExtraTurn())
                {
                    players[playerTurn].ExtraTurn(false);

                    //Play second move for AI
                    if (players[playerTurn].IsAI())
                        StartCoroutine(PlayTile());
                }
                else
                {
                    StartCoroutine(ChangePlayerTurn());
                }
            }
            else if (clickedTile.type == TileType.Invert)
            {
                clickedTile.ChangeTo(players[playerTurn]);

                players[playerTurn].AddTile(clickedTile);

                clickedTile.RemovePowerEffect();
                HashSet<Tile> p1Tiles = new HashSet<Tile>(players[playerTurn].GetTiles());
                HashSet<Tile> p2Tiles = new HashSet<Tile>(players[playerTurn^1].GetTiles());
                players[playerTurn].GetTiles().Clear();
                yield return new WaitForSecondsRealtime(Tile.transitionTime);
                foreach (var tile in p1Tiles)
                {
                    if (tile != clickedTile && tile.type == TileType.Hexagon )
                    {
                        tile.ChangeTo(players[playerTurn ^ 1]); //XOR to flip 1 and 0
                        players[playerTurn ^ 1].AddTile(tile);
                    }
                    else
                    {
                        players[playerTurn].AddTile(tile);
                    }
                }

                foreach (var tile in p2Tiles)
                {
                    if (tile.type == TileType.Hexagon)
                    {
                        tile.ChangeTo(players[playerTurn]);
                        players[playerTurn].AddTile(tile);
                    }
                    else
                    {
                        players[playerTurn ^ 1].AddTile(tile);
                    }
                }

                players[playerTurn ^ 1].GetTiles().IntersectWith(p1Tiles);
                yield return new WaitForSecondsRealtime(Tile.transitionTime);

                if (players[playerTurn].ExtraTurn())
                {
                    players[playerTurn].ExtraTurn(false);

                    //Play second move for AI
                    if (players[playerTurn].IsAI())
                        StartCoroutine(PlayTile());
                }
                else
                {
                    StartCoroutine(ChangePlayerTurn());
                }
            }
            else if(clickedTile.type == TileType.Skip)
            {
                clickedTile.ChangeTo(null);
                clickedTile.RemovePowerEffect();
                //Change player turn
                players[playerTurn].ExtraTurn(true);
                playerTurn ^= 1;
                yield return new WaitForSecondsRealtime(Tile.transitionTime);
                StartCoroutine(ChangePlayerTurn());
            }
            else
            {
                //HexTile was clicked
                numHexTiles--;

                clickedTile.ChangeTo(players[playerTurn]);

                players[playerTurn].AddTile(clickedTile);

                //Changes player turn counter at end of coroutine
                yield return StartCoroutine(ChangeNeighbours(clickedTile, players[playerTurn].Depth()));

                //If depth >1, reduce by 1
                if (players[playerTurn].Depth() > 1)
                {
                    players[playerTurn].SetDepth(players[playerTurn].Depth() - 1);
                }

                if(players[playerTurn].ExtraTurn())
                {
                    players[playerTurn].ExtraTurn(false);

                    //Play second move for AI
                    if (players[playerTurn].IsAI())
                        StartCoroutine(PlayTile());
                }
                else
                {
                    StartCoroutine(ChangePlayerTurn());
                }
            }

            pauseClicks = false;
        }

        if (numHexTiles < 1)
        {
            StartCoroutine(EndGame());
        }
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSecondsRealtime(2 * Tile.transitionTime);

        Transform uiCanvas = HUD.parent;
        //Show final score
        for (int i = 1; i < uiCanvas.childCount; i++)
            uiCanvas.GetChild(i).gameObject.SetActive(false);

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

        endMenu.gameObject.SetActive(true);
        uiCanvas.GetChild(0).gameObject.SetActive(true);

    }

    private IEnumerator ChangeNeighbours(Tile tile, int depth)
    {
        changeNeighbourCount++;

        yield return new WaitForSecondsRealtime(Tile.transitionTime);

        foreach (Tile neighbour in tile.neighbours)
        {
            //Do not run for pentagons or free tiles
            if (neighbour.type != TileType.Hexagon || neighbour.IsFree())
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
        while (changeNeighbourCount > 0)
        {
            yield return null;
        }
    }

    private IEnumerator ChangePlayerTurn()
    {
        StartCoroutine(ChangeScoreUI());
        yield return StartCoroutine(ChangeTurnUI());

        //For next move, check again, if player if AI
        if (players[playerTurn].IsAI())
        {
            StartCoroutine(PlayTile());
        }
    }

    private IEnumerator ChangeScoreUI()
    {
        //Update the score indicator
        float initialFillAmount = scoreIndicator.GetChild(1).GetComponent<Image>().fillAmount;
        float targetFillAmount = (float)players[0].GetScore() / (float)(players[0].GetScore() + players[1].GetScore());

        float dt = 0;
        while (scoreIndicator.GetChild(1).GetComponent<Image>().fillAmount != targetFillAmount)
        {
            if (isPaused)
                yield return null;

            scoreIndicator.GetChild(1).GetComponent<Image>().fillAmount = Mathf.Lerp(initialFillAmount, targetFillAmount, dt / 0.2f);
            dt += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ChangeTurnUI()
    {
        //rotate turn indicator to mid
        Quaternion startRot = turnIndicator.transform.GetChild(0).rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 90, 0);
        float dt = 0;

        while (turnIndicator.transform.GetChild(0).rotation != endRot)
        {
            if (isPaused)
                yield return null;

            turnIndicator.transform.GetChild(0).rotation = Quaternion.Lerp(startRot, endRot, dt / 0.1f);
            dt += Time.deltaTime;
            yield return null;
        }

        //change turn & color
        playerTurn ^= 1;
        turnIndicator.GetComponentInChildren<Image>().color = players[playerTurn].GetColor();

        if (players[playerTurn].Depth() > 1)
        {
            powerIndicator.GetChild(2).GetComponent<Image>().sprite = rangeUpImage;
            powerIndicator.GetChild(1).GetComponent<Image>().color = players[playerTurn].GetColor();
            powerIndicator.gameObject.SetActive(true);
        }
        else if(players[playerTurn].ExtraTurn())
        {
            powerIndicator.GetChild(2).GetComponent<Image>().sprite = skipImage;
            powerIndicator.GetChild(1).GetComponent<Image>().color = players[playerTurn].GetColor();
            powerIndicator.gameObject.SetActive(true);
        }
        else
            powerIndicator.gameObject.SetActive(false);

        //Change text
        turnIndicator.GetComponentInChildren<TextMeshProUGUI>().SetText((playerTurn + 1).ToString());
        //rotate rest of the way
        startRot = turnIndicator.GetChild(0).rotation;
        endRot = startRot * Quaternion.Euler(0, 90, 0);
        dt = 0;
        while (turnIndicator.GetChild(0).rotation != endRot)
        {
            if (isPaused)
                yield return null;

            turnIndicator.GetChild(0).rotation = Quaternion.Lerp(startRot, endRot, dt / 0.1f);
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
            StartCoroutine(MakeMove(clickedTile));
        }
    }

    //Rotate sphere in world space on drag
    public void OnDrag(Vector3 start, Vector3 diff)
    {
        flickSpeed = Vector3.zero;
        if (!(isPaused || pauseClicks))
        {
            Vector3 axis = Vector3.Cross(diff, Vector3.forward).normalized;

            float angle = Vector3.Angle(start - diff, start);

            sphere.Rotate(axis * angle * dragSpeed, Space.World);
        }
    }

    public void OnDragEnd(Vector3 speed)
    {
        if(!pauseClicks  && speed.magnitude > flickThreshold)
        {
            flickSpeed = speed.normalized * dragSpeed;

            StartCoroutine(KeepRotating());
        }
    }

    public void OnPinch(float diff)
    {
        //Change the field of view based on the change in distance between the touches.
        camera.fieldOfView += diff * perspectiveZoomSpeed;

        // Clamp the field of view to make sure it's between 0 and 180.
        camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, minFoV, maxFoV);

        zoomedRotSpeed = dragSpeed + dragSpeed * (initialFoV - camera.fieldOfView) / (maxFoV - minFoV);
    }

    private IEnumerator KeepRotating()
    {
        while (flickSpeed.magnitude > 0)
        {
            if (isPaused)
                yield return null;

            sphere.Rotate(Vector3.Cross(flickSpeed, Vector3.forward), Space.World);
            Vector3 newSpeed = flickSpeed - flickSpeed * Time.deltaTime;
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
        pauseClicks = true;
        while (Vector3.Angle(t.GetNormal(), Vector3.back) > 20)
        {
            if (isPaused)
                yield return null;

            float step = aiRotSpeed * Time.deltaTime;
            sphere.Rotate(Vector3.Cross(t.GetNormal(), Vector3.back), step, Space.World);

            yield return null;
        }

        if (!isPaused)
            pauseClicks = false;
    }

    public void RestartGame()
    {
        ClearScene();
        MenuScript.instance.TogglePauseMenu(false);
        StartCoroutine(Reset());
    }

    public void ClearScene()
    {
        var icons = FindObjectsOfType<SpriteRenderer>();
        foreach (var icon in icons)
            Destroy(icon.gameObject);
        StopAllCoroutines();
        foreach (Transform child in HUD.parent)
            child.gameObject.SetActive(false);
        Destroy(sphere.gameObject);
    }

    private IEnumerator Reset()
    {
        while (sphere != null)
        {
            yield return null;
        }

        Initialise(SPGame);
    }
}