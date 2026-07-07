using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

/// <summary>
/// Drives an inspection: walks the NPC along a waypoint route, plays enter/leave/catch cues, and
/// accumulates risk from any contraband spotted along the way. Cardinal-facing maths are delegated
/// to the pure <see cref="InspectionPath"/>; risk is read/written through an injected
/// <see cref="RiskManager"/>.
/// </summary>
public class InspectionSystem : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Risk source this inspection reads and adds to. Required.")]
    [SerializeField]
    private RiskManager _riskManager;

    [Header("NPC Movement")]
    [Tooltip("Transform of the walking NPC. Required.")]
    [FormerlySerializedAs("npc")]
    [SerializeField]
    private Transform _npc;

    [Tooltip("NPC movement speed in world units per second.")]
    [FormerlySerializedAs("moveSpeed")]
    [SerializeField]
    private float _moveSpeed = 2f;

    [Tooltip("Ordered route the NPC walks during an inspection. Must have at least one point.")]
    [FormerlySerializedAs("waypoints")]
    [SerializeField]
    private List<Transform> _waypoints;

    [Header("Animation & Audio")]
    [Tooltip("Animator playing the inspection enter/leave transitions. Required.")]
    [FormerlySerializedAs("inspectionAnimator")]
    [SerializeField]
    private Animator _inspectionAnimator;

    [Tooltip("Animator for the walking officer (MoveX/MoveY/isWalking).")]
    [FormerlySerializedAs("inspectionOfficerAnimator")]
    [SerializeField]
    private Animator _inspectionOfficerAnimator;

    [Tooltip("Sounds indexed as [0]=enter, [1]=leave, [2]=catch. Required (>= 3 clips).")]
    [FormerlySerializedAs("inspectionSounds")]
    [SerializeField]
    private List<AudioClip> _inspectionSounds;

    private Npc _npcScript;
    private AudioSource _inspectionAudioSource;
    private int _currentWaypoint;
    private bool _inspecting;
    private bool _isCatching;
    private Vector2 _facing;
    private Vector2 _lastPosition;

    public bool IsCatching => _isCatching;

    private void Awake()
    {
        Assert.IsNotNull(
            _riskManager,
            $"[{nameof(InspectionSystem)}] RiskManager unassigned on {name}!"
        );
        Assert.IsNotNull(_npc, $"[{nameof(InspectionSystem)}] NPC transform unassigned on {name}!");
        Assert.IsNotNull(
            _inspectionAnimator,
            $"[{nameof(InspectionSystem)}] Inspection animator unassigned on {name}!"
        );

        _npcScript = _npc.GetComponent<Npc>();
        Assert.IsNotNull(
            _npcScript,
            $"[{nameof(InspectionSystem)}] NPC transform has no Npc component on {name}!"
        );
        _inspectionAudioSource = _inspectionAnimator.GetComponent<AudioSource>();
    }

    private void OnValidate()
    {
        if (_moveSpeed < 0f)
            _moveSpeed = 0f;
    }

    private void Update()
    {
        if (_inspecting && !_isCatching)
            MoveNpc();

        if (_inspecting || _isCatching)
            UpdateWalkingState();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            StartInspection();
    }

    public void StartInspection()
    {
        if (_waypoints.Count == 0)
        {
            Debug.LogWarning($"[{nameof(InspectionSystem)}] No waypoints set on {name}.");
            return;
        }

        _npc.gameObject.SetActive(true);
        _currentWaypoint = 0;
        StartCoroutine(StartInspectionRoutine());
    }

    /// <summary>Called from contraband triggers while the NPC is inspecting.</summary>
    public void DetectSensitiveItem(int itemRisk)
    {
        _riskManager.AddRisk(itemRisk);
        if (_isCatching)
            return;

        if (_riskManager.CurrentRisk >= 90f)
        {
            _isCatching = true;
            GameEvents.OchranaBribe(); // resolved by EndCatching() once the bribe is handled
        }
    }

    /// <summary>Ends the "caught" bribe standoff — invoked by the event handler after resolution.</summary>
    public void EndCatching() => _isCatching = false;

    private IEnumerator StartInspectionRoutine()
    {
        DisplayInspectionEnter();

        // Wait until the animator actually enters, then leaves, the enter state.
        yield return new WaitUntil(() =>
            _inspectionAnimator.GetCurrentAnimatorStateInfo(0).IsName("OchranaInspectionEnter")
        );
        yield return new WaitWhile(() =>
            _inspectionAnimator.GetCurrentAnimatorStateInfo(0).IsName("OchranaInspectionEnter")
        );

        PlayInspectionSound(InspectionCue.Enter);
        _inspecting = true;
    }

    private void MoveNpc()
    {
        if (_riskManager.CurrentRisk >= 99.5f)
        {
            GameEvents.Arrest();
            return;
        }

        Transform target = _waypoints[_currentWaypoint];
        Vector2 delta = (Vector2)target.position - (Vector2)_npc.position;
        _facing = InspectionPath.CardinalFacing(delta);

        _npc.position = Vector2.MoveTowards(
            _npc.position,
            target.position,
            _moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(_npc.position, target.position) < 0.1f)
        {
            _currentWaypoint++;
            if (_currentWaypoint >= _waypoints.Count)
                EndInspection();
        }
    }

    private void UpdateWalkingState()
    {
        bool isWalking = Vector2.Distance(_npc.position, _lastPosition) > 0.0001f;
        _lastPosition = _npc.position;

        if (isWalking && !_npcScript.IsPlayingSound)
            _npcScript.PlaySound();
        else if (!isWalking && _npcScript.IsPlayingSound)
            _npcScript.StopSound();

        if (_inspectionOfficerAnimator)
        {
            _inspectionOfficerAnimator.SetFloat("MoveX", _facing.x);
            _inspectionOfficerAnimator.SetFloat("MoveY", _facing.y);
            _inspectionOfficerAnimator.SetBool("isWalking", isWalking);
        }
    }

    private void EndInspection()
    {
        _inspecting = false;
        DisplayInspectionLeave();

        if (_riskManager.CurrentRisk > 99f)
            PlayInspectionSound(InspectionCue.Catch);

        _npc.gameObject.SetActive(false);
    }

    private void DisplayInspectionEnter()
    {
        if (_inspectionAnimator)
            _inspectionAnimator.SetTrigger("Enter");
    }

    private void DisplayInspectionLeave()
    {
        if (_inspectionAnimator)
            _inspectionAnimator.SetTrigger("Leave");
        PlayInspectionSound(InspectionCue.Leave);
    }

    private void PlayInspectionSound(InspectionCue cue)
    {
        if (_inspectionAudioSource == null || _inspectionSounds == null)
            return;

        int index = (int)cue;
        if (index >= 0 && index < _inspectionSounds.Count)
            _inspectionAudioSource.PlayOneShot(_inspectionSounds[index]);
    }

    private enum InspectionCue
    {
        Enter = 0,
        Leave = 1,
        Catch = 2,
    }
}
