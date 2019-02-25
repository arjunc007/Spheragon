using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {

    public string gameScene = "Game";
    public string menuScene = "Main Menu";
    public Sprite audioImage;
    public Sprite muteImage;
    public Transform audioButton;
    public GameObject loadingScreen;


    private void Awake()
    {
        //GameData.LoadPlayerPrefs();
    }

    private void Start()
    {
        if (GameData.musicOn)
            audioButton.GetComponent<Image>().sprite = audioImage;
        else
            audioButton.GetComponent<Image>().sprite = muteImage;
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            OnBackButton();
        }
    }

    public void StartGame(bool singlePlayer)
    {
        //Deactivate mainmenuUI
        //Activate pausemenuUI
        GameManager.instance.Initialise(singlePlayer);
            //Make icosahedron visible

    }

    private IEnumerator LoadScene(string scene)
    {
        GameObject ls = Instantiate(loadingScreen);

        yield return SceneManager.LoadSceneAsync(scene);
        DontDestroyOnLoad(gameObject);

        yield return SceneManager.UnloadSceneAsync(scene);
        Destroy(ls);
        Destroy(gameObject);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void GoToMenu()
    {
        StartCoroutine(LoadScene(menuScene));
    }

    public void ToggleAudio()
    {
        GameData.musicOn = !GameData.musicOn;

        if (GameData.musicOn)
            audioButton.GetComponent<Image>().sprite = audioImage;
        else
            audioButton.GetComponent<Image>().sprite = muteImage;

        AudioManager.instance.ToggleMusic();
    }

    public void PlayTapSound()
    {
        if (GameData.musicOn)
            GetComponent<AudioSource>().Play();
    }

    public void OnBackButton()
    {
        if(FindObjectOfType<GameManager>() == null)
        {
            Quit();
        }
        else
        {
            if (GameManager.isPaused)
                GoToMenu();
            else
                GameManager.instance.PauseGame();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        GameData.SavePlayerPrefs();
    }

    private void OnApplicationQuit()
    {
        GameData.SavePlayerPrefs();
    }
}
