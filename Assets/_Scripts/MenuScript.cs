using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour {

    public string gameScene = "Game";
    public string menuScene = "Main Menu";
    public GameObject loadingScreen;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            OnBackButton();
        }
    }

    public void SelectMode()
    {

    }

    public void StartGame(bool singlePlayer)
    {
        GameData.isSP = singlePlayer;
        StartCoroutine(LoadScene(gameScene));
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
}
