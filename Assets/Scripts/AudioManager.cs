using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource clipSource;

    [SerializeField]
    private AudioClip matchAudio;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMatch()
    {
        clipSource.PlayOneShot(matchAudio);
        clipSource.pitch *= 1.1f;
    }

    public void ResetPitch()
    {
        clipSource.pitch = 1;
    }
}
