using System.Collections;
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

    public void ToggleAudio()
    {
        GameData.musicOn = !GameData.musicOn;

        if (GameData.musicOn)
            audioButton.GetComponent<Image>().sprite = audioImage;
        else
            audioButton.GetComponent<Image>().sprite = muteImage;
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

    public void SlideOut_Left(RectTransform obj)
    {
        StopAllCoroutines();
        StartCoroutine(SlideOut(obj));
    }

    private IEnumerator SlideOut(RectTransform obj)
    {
        RectOffset padding = obj.GetComponent<HorizontalLayoutGroup>().padding;
        int initialPadding = padding.left;
        int finalPadding = initialPadding > 0 ? 0 : 250;
        float t = 0;

        while (padding.left != finalPadding)
            {
                t += Time.deltaTime / 0.1f;
                padding.left = (int) Mathf.Lerp(initialPadding, finalPadding, t);
                LayoutRebuilder.ForceRebuildLayoutImmediate(obj);
                yield return null;
            }
    }
}
