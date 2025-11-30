using System;
using UnityEngine;

public class Npc : MonoBehaviour
{
    [SerializeField] private InspectionSystem inspectionSystem;
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public InspectionSystem GetInspectionSystem() => inspectionSystem;

    public void PlaySound(AudioClip clip = null)
    {
        if (clip) _audioSource.PlayOneShot(clip);
        else _audioSource.Play();
    }
    
    public void StopSound() => _audioSource.Stop();
    
    public bool IsPlayingSound() => _audioSource.isPlaying;
}
