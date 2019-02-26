using UnityEngine;

public class AudioManager : MonoBehaviour {

    public static AudioManager instance;

    public AudioClip menuMusicClip;
    public AudioClip[] gameMusicClips;
    private AudioSource audioSource;

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
    }

    // Use this for initialization
    void Start () {

        audioSource = GetComponent<AudioSource>();
        if (!audioSource.playOnAwake && GameData.musicOn)
        {
            if (MenuScript.instance.pauseMenu.gameObject.activeSelf)
                audioSource.clip = gameMusicClips[Random.Range(0, gameMusicClips.Length)];
            else
                audioSource.clip = menuMusicClip;
            audioSource.Play();
        }
	}
	
	// Update is called once per frame
	void Update () {
		if(!audioSource.isPlaying && GameData.musicOn)
        {
            if (MenuScript.instance.pauseMenu.gameObject.activeSelf)
                audioSource.clip = gameMusicClips[Random.Range(0, gameMusicClips.Length)];
            else
                audioSource.clip = menuMusicClip;
            audioSource.Play();
        }
	}

    public void PlayMenuMusic()
    {
        if (MenuScript.instance.mainMenu.gameObject.activeSelf)
        {
            audioSource.clip = menuMusicClip; ;
            audioSource.Play();
        }
    }

    public void PlayGameMusic()
    {
        if (MenuScript.instance.pauseMenu.gameObject.activeSelf)
            audioSource.clip = gameMusicClips[Random.Range(0, gameMusicClips.Length)];
    }

    public void ToggleMusic()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
        else
            audioSource.Play();
    }
}
