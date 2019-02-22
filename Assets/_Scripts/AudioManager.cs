using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour {

    public AudioClip[] musicClips;
    private AudioSource audioSource;

    // Use this for initialization
    void Start () {

        audioSource = GetComponent<AudioSource>();
        if (!audioSource.playOnAwake && GameData.musicOn)
        {
            audioSource.clip = musicClips[Random.Range(0, musicClips.Length)];
            audioSource.Play();
        }
	}
	
	// Update is called once per frame
	void Update () {
		if(!audioSource.isPlaying && GameData.musicOn)
        {
            audioSource.clip = musicClips[Random.Range(0, musicClips.Length)];
            audioSource.Play();
        }
	}

    public void ToggleMusic()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
        else
            audioSource.Play();
    }
}
