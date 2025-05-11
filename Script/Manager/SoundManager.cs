using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : SceneSingleton<SoundManager>
{
    public AudioClip[] audioClips; // 재생할 여러 AudioClip 배열
    private AudioSource audioSource;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlaySound(int index)
    {
        if (audioClips != null && index >= 0 && index < audioClips.Length)
        {
            audioSource.PlayOneShot(audioClips[index]);
        }
    }

    public void PlaySound_RandomMarinDead() // Soldier 타입 병사 사운드
    {
        if (audioClips != null) 
        {
            int[] specificIndices = { 7, 8 };
            int randomIndex = specificIndices[UnityEngine.Random.Range(0, specificIndices.Length)];
            AudioClip randomClip = audioClips[randomIndex];
            audioSource.PlayOneShot(randomClip);
        }
        else
        {
            Debug.Log("error! check audio system");
        }
    }
    
    public void PlaySound_RandomIndex(int[] specificIndices) // Soldier 타입 병사 사운드
    {
        if (audioClips != null) 
        {
            int randomIndex = specificIndices[UnityEngine.Random.Range(0, specificIndices.Length)];
            if (randomIndex >= 0 && randomIndex < audioClips.Length)
            {
                AudioClip randomClip = audioClips[randomIndex];
                audioSource.PlayOneShot(randomClip);
            }
            else
            {
                Debug.Log("error! randomIndex is out of audioClips bounds");
            }
        }
        else
        {
            Debug.Log("error! check audio system");
        }
    }
}
