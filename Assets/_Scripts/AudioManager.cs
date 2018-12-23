using UnityEngine;

public class AudioManager : MonoBehaviour {

    public AudioClip[] audioClips;
    private AudioSource audioSource;

	// Use this for initialization
	void Start () {

        audioSource = GetComponent<AudioSource>();

        if (!audioSource.playOnAwake)
        {
            audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
            audioSource.Play();
        }
	}
	
	// Update is called once per frame
	void Update () {
		if(!audioSource.isPlaying)
        {
            audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
            audioSource.Play();
        }
	}
}
