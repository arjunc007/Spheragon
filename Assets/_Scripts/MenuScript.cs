using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuScript : MonoBehaviour {
    public static MenuScript instance;

    public Sprite audioImage;
    public Sprite muteImage;
    public Transform audioButton;
    public Transform mainMenu;
    public Transform pauseMenu;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        GameData.LoadPlayerPrefs();
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
        //Deactivate mainmenuUI, title
        mainMenu.gameObject.SetActive(false);
        mainMenu.parent.parent.GetComponentInChildren<TextMeshProUGUI>().gameObject.SetActive(false);
        //Activate pausemenuUI, background
        pauseMenu.gameObject.SetActive(true);
        pauseMenu.parent.parent.GetComponentInChildren<Image>().gameObject.SetActive(true);
        //Deactivate parent containing all menus(MenuSystem)
        pauseMenu.parent.parent.gameObject.SetActive(false);

        //Start game
        GameManager.instance.Initialise(singlePlayer);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void GoToMenu()
    {
    }

    public void TogglePauseMenu(bool isPaused)
    {
        pauseMenu.parent.parent.gameObject.SetActive(isPaused);
    }

    public void ToggleAudio()
    {
        GameData.musicOn = !GameData.musicOn;

        if (GameData.musicOn)
            audioButton.GetComponent<Image>().sprite = audioImage;
        else
            audioButton.GetComponent<Image>().sprite = muteImage;

        AudioManager.instance.ToggleMusic();

        PlayerPrefs.SetInt("Music", GameData.musicOn ? 1 : 0);
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
