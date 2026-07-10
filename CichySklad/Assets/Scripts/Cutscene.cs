using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>One spoken line of a cutscene: who says it (portrait key) and the text, with an optional
/// auto-advance timer. Authored inside a <see cref="Cutscene"/> asset.</summary>
[Serializable]
public class CutsceneLine
{
    [Tooltip(
        "Portrait key passed to DialogueSystem.GetPortraitSprite, e.g. 'maria', 'ochrana', 'kowal'."
    )]
    [SerializeField]
    private string _speakerKey;

    [Tooltip("Line body shown in the dialogue box.")]
    [TextArea]
    [SerializeField]
    private string _text;

    [Tooltip("Seconds before auto-advancing to the next line. 0 = wait for the player to advance.")]
    [SerializeField]
    private float _autoAdvanceSeconds;

    public string SpeakerKey => _speakerKey;
    public string Text => _text;
    public float AutoAdvanceSeconds => _autoAdvanceSeconds;

    public CutsceneLine() { }

    public CutsceneLine(string speakerKey, string text, float autoAdvanceSeconds = 0f)
    {
        _speakerKey = speakerKey;
        _text = text;
        _autoAdvanceSeconds = autoAdvanceSeconds;
    }
}

/// <summary>
/// A short text cutscene: an ordered list of <see cref="CutsceneLine"/>s played one after another by
/// a <see cref="CutscenePlayer"/>. Pure data (no logic, no choices — end choices are supplied in code
/// at play time, since their actions cannot be serialised into an asset), so designers author threads
/// as assets and reuse them (e.g. from B7 story beats).
/// </summary>
[CreateAssetMenu(fileName = "Cutscene", menuName = "CichySklad/Cutscene")]
public class Cutscene : ScriptableObject
{
    [Tooltip("Ordered lines, played top to bottom.")]
    [SerializeField]
    private CutsceneLine[] _lines = Array.Empty<CutsceneLine>();

    public IReadOnlyList<CutsceneLine> Lines => _lines;
}
