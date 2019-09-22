using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuScript : MonoBehaviour {
    public static MenuScript instance;

    public Transform modeSelect;
    public Sprite audioImage;
    public Sprite muteImage;
    public Transform audioButton;
    public Transform mainMenu;
    public Transform pauseMenu;
    private GameObject pauseMenuBackground;
    private GameObject title;

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

        title = mainMenu.parent.parent.GetComponentInChildren<TextMeshProUGUI>().gameObject;
        pauseMenuBackground = pauseMenu.parent.parent.GetComponentInChildren<Image>().gameObject;
        pauseMenuBackground.SetActive(false);
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
        //Deactivate mainmenuUI, title, mode Select
        mainMenu.gameObject.SetActive(false);
        title.SetActive(false);
        modeSelect.gameObject.SetActive(false);
        //Activate pausemenuUI, background
        pauseMenu.gameObject.SetActive(true);
        pauseMenuBackground.SetActive(true);
        //Deactivate parent containing all menus(MenuSystem)
        pauseMenu.parent.parent.gameObject.SetActive(false);

        AudioManager.instance.PlayGameMusic();

        //Start game
        GameManager.instance.Initialise(singlePlayer);

        AdManager.instance.ShowAd(true);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void GoToMenu()
    {
        //Hide Ad
        AdManager.instance.ShowAd(false);

        //Destoy game Data (Reset Game, but don't call Initialise again)
        GameManager.instance.ClearScene();

        GameManager.instance.endMenu.gameObject.SetActive(false);

        //Activate mainmenuUI, title
        mainMenu.gameObject.SetActive(true);
        title.SetActive(true);
        
        //Dectivate pausemenuUI, background
        pauseMenu.gameObject.SetActive(false);
        pauseMenuBackground.SetActive(false);
        AudioManager.instance.PlayMenuMusic();

        //Activate parent containing all menus(MenuSystem)
        pauseMenu.parent.parent.gameObject.SetActive(true);
    }

    public void TogglePauseMenu(bool isPaused)
    {
        pauseMenu.parent.parent.gameObject.SetActive(isPaused);
    }

    public void ToggleModes()
    {
        modeSelect.gameObject.SetActive(!modeSelect.gameObject.activeSelf);
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
