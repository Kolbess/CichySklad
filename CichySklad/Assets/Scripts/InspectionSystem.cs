using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectionSystem : MonoBehaviour
{
    [Header("NPC Settings")]
    [SerializeField] private Transform npc; // NPC GameObject
    private Npc npcScript;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private List<Transform> waypoints; // punkty trasy NPC
    [SerializeField] private Animator inspectionAnimator;
    [SerializeField] private Animator inspectionOfficerAnimator;
    private AudioSource _inspectionAnimatorAudioSource;
    [SerializeField] private List<AudioClip> inspectionSounds;

    private int currentWaypoint = 0;
    private bool inspecting = false;
    public bool isCatching = false;
    private float moveX;
    private float moveY;

    private void Start()
    {
        npcScript = npc.GetComponent<Npc>();
        _inspectionAnimatorAudioSource = inspectionAnimator.GetComponent<AudioSource>();
    }

    private void PlayInspectionSound(string sound)
    {
        switch (sound)
        {
            case "enter":
                _inspectionAnimatorAudioSource.PlayOneShot(inspectionSounds[0]);
                break;
            case "leave":
                _inspectionAnimatorAudioSource.PlayOneShot(inspectionSounds[1]);
                break;
            case "catch":
                _inspectionAnimatorAudioSource.PlayOneShot(inspectionSounds[2]);
                break;
            default:
                _inspectionAnimatorAudioSource.PlayOneShot(inspectionSounds[0]);
                break;
        }
    }

    private void Update()
    {
        //Debug.Log("Inspecting: " + inspecting);
        if (inspecting && !isCatching)
        {
            MoveNpc();
        }

        if (inspecting || isCatching)
        {
            UpdateWalkingState();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartInspection();
            Debug.Log("Inspection started!");
        }
    }
    
    private IEnumerator StartInspectionRoutine()
    {
        DisplayInspectionEnter();

        // 1. Czekaj aż ANIMATOR faktycznie wejdzie w stan
        yield return new WaitUntil(() =>
            inspectionAnimator.GetCurrentAnimatorStateInfo(0).IsName("OchranaInspectionEnter")
        );

        // 2. Czekaj aż ANIMATOR opuści ten stan (co oznacza koniec animacji)
        yield return new WaitWhile(() =>
            inspectionAnimator.GetCurrentAnimatorStateInfo(0).IsName("OchranaInspectionEnter")
        );
        PlayInspectionSound("enter");
        inspecting = true;
    }

    public void StartInspection()
    {
        if (waypoints.Count == 0 || npc == null)
        {
            Debug.LogWarning("Waypoints or NPC not set!");
            return;
        }
        
        npc.gameObject.SetActive(true);
        currentWaypoint = 0;
        
        StartCoroutine(StartInspectionRoutine());
    }


    private Vector2 lastPosition;

    private void MoveNpc()
    {
        if (RiskManager.Instance.CurrentRisk >= 99.5f)
        {
            EventSystem.Arrest();
            return;
        }

        Transform target = waypoints[currentWaypoint];
        Vector2 rawDir = (target.position - npc.position);
        Vector2 direction = rawDir.normalized;

        npc.position = Vector2.MoveTowards(npc.position, target.position, moveSpeed * Time.deltaTime);

        // --- TWOJA ORYGINALNA LOGIKA ---
        if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            moveY = direction.y > 0 ? 1f : -1f;
            moveX = 0f;
        }
        else
        {
            moveX = direction.x > 0 ? 1f : -1f;
            moveY = 0f;
        }

        // waypoint arrival
        if (Vector2.Distance(npc.position, target.position) < 0.1f)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Count)
            {
                EndInspection();
            }
        }
    }


    
    private void UpdateWalkingState()
    {
        // ✔️ real walking detection (działa nawet gdy MoveNpc NIE jest wywoływany)
        bool isWalking = (Vector2.Distance(npc.position, lastPosition) > 0.0001f);
        lastPosition = npc.position;

        // --- SOUND ---
        if (isWalking)
        {
            if (!npcScript.IsPlayingSound())
                npcScript.PlaySound();
        }
        else
        {
            if (npcScript.IsPlayingSound())
                npcScript.StopSound();
        }

        // --- ANIM ---
        if (inspectionOfficerAnimator)
        {
            inspectionOfficerAnimator.SetFloat("MoveX", moveX);
            inspectionOfficerAnimator.SetFloat("MoveY", moveY);
            inspectionOfficerAnimator.SetBool("isWalking", isWalking);
        }
    }




    private void EndInspection()
    {
        inspecting = false;
        DisplayInspectionLeave();

        if (RiskManager.Instance.CurrentRisk > 99)
        {
            DisplayInspectionCatch();
        }
        
        npc.gameObject.SetActive(false);
    }

    private void DisplayInspectionEnter()
    {
        if (inspectionAnimator)
            inspectionAnimator.SetTrigger("Enter");
    }

    private void DisplayInspectionCatch()//Add Arrest to an event (print arrester for now but make it actaully print it)
    {
        PlayInspectionSound("catch");
       // if (inspectionAnimator)
       //     inspectionAnimator.SetTrigger("Catch");
    }

    private void DisplayInspectionLeave()
    {
        if (inspectionAnimator)
            inspectionAnimator.SetTrigger("Leave");
        PlayInspectionSound("leave");
    }

    // Wywoływane z NPC colliderem przy obiektach Sensitive
    public void DetectSensitiveItem(int itemRisk)
    {
        RiskManager.Instance.AddRisk(itemRisk);
        if (isCatching) return;
        if (RiskManager.Instance.CurrentRisk >= 90)
        {
            isCatching = true;
            EventSystem.OchranaBribe();//set isCatching to false after event
        }
        Debug.Log($"Sensitive item detected!");
    }
}
