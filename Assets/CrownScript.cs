using UnityEngine;

public class CrownScript : MonoBehaviour {

    public void PlayImpact(AudioClip audio)
    {
        AudioSource.PlayClipAtPoint(audio, Vector3.zero);
    }
}
