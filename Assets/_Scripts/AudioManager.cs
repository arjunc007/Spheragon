using UnityEngine;

public class AudioManager : MonoBehaviour {

    public AudioClip[] audioClips;
    private new AudioSource audio;

	// Use this for initialization
	void Start () {

        audio = GetComponent<AudioSource>();

        if (!audio.playOnAwake)
        {
            audio.clip = audioClips[Random.Range(0, audioClips.Length)];
            audio.Play();
        }
	}
	
	// Update is called once per frame
	void Update () {
		if(!audio.isPlaying)
        {
            audio.clip = audioClips[Random.Range(0, audioClips.Length)];
            audio.Play();
        }
	}
}
