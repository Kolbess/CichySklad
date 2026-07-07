using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

/// <summary>
/// The inspecting NPC's footstep audio and its link back to the <see cref="InspectionSystem"/>
/// that drives it. A thin wrapper so the inspection controller can play/stop sound and query
/// playback without reaching into an AudioSource directly.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Npc : MonoBehaviour
{
    [Tooltip("Inspection controller that owns this NPC's route and state. Required.")]
    [FormerlySerializedAs("inspectionSystem")]
    [SerializeField]
    private InspectionSystem _inspectionSystem;

    private AudioSource _audioSource;

    public InspectionSystem InspectionSystem => _inspectionSystem;

    public bool IsPlayingSound => _audioSource.isPlaying;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        Assert.IsNotNull(
            _inspectionSystem,
            $"[{nameof(Npc)}] InspectionSystem unassigned on {name}!"
        );
    }

    public void PlaySound(AudioClip clip = null)
    {
        if (clip)
            _audioSource.PlayOneShot(clip);
        else
            _audioSource.Play();
    }

    public void StopSound() => _audioSource.Stop();
}
