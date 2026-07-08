using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Shared plumbing for the PlayMode suite. Every runtime component hard-asserts its required
/// references in <c>Awake</c>, so the helpers here build each GameObject <em>inactive</em>, inject
/// the private serialized fields by reflection, then activate it — guaranteeing the wiring is in
/// place before <c>Awake</c> runs. Each test owns a throwaway scene so spawned prefabs and static
/// <c>GameEvents</c> subscriptions (unsubscribed in the components' <c>OnDisable</c>) are torn down.
/// </summary>
public abstract class PlayModeTestBase
{
    private static int _sceneCounter;

    private Scene _testScene;

    // ResourceManager wiring exposed so tests can inspect the driven UI directly.
    protected TextMeshProUGUI RmPaperText;
    protected TextMeshProUGUI RmInkText;
    protected TextMeshProUGUI RmLeafletsText;
    protected TextMeshProUGUI RmMoneyText;
    protected Slider RmTrustSlider;

    [UnitySetUp]
    public IEnumerator BaseSetUp()
    {
        _testScene = SceneManager.CreateScene($"PlayTest_{GetType().Name}_{_sceneCounter++}");
        SceneManager.SetActiveScene(_testScene);
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator BaseTearDown()
    {
        if (_testScene.IsValid() && _testScene.isLoaded)
            yield return SceneManager.UnloadSceneAsync(_testScene);
    }

    // =====================================================================
    // Low-level construction helpers
    // =====================================================================

    /// <summary>Creates an inactive GameObject and adds <typeparamref name="T"/> so its
    /// <c>Awake</c> is deferred until the caller has injected fields and re-activates it.</summary>
    protected static T AddInactive<T>(out GameObject go, string name = null)
        where T : Component
    {
        go = new GameObject(name ?? typeof(T).Name);
        go.SetActive(false);
        return go.AddComponent<T>();
    }

    protected static void Activate(Component component) => component.gameObject.SetActive(true);

    /// <summary>Assigns a private/serialized field by name, walking base types.</summary>
    protected static void SetField(object target, string fieldName, object value)
    {
        System.Type type = target.GetType();
        FieldInfo field = null;
        while (type != null && field == null)
        {
            field = type.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );
            type = type.BaseType;
        }

        Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
        field.SetValue(target, value);
    }

    protected static GameObject NewGo(string name = "GO") => new GameObject(name);

    // UI elements are left parentless (no Canvas) on purpose: the tests only read/write logical
    // state (text string, slider value, image color), never force a canvas mesh rebuild.
    protected static TextMeshProUGUI NewText() => NewGo("Text").AddComponent<TextMeshProUGUI>();

    protected static Slider NewSlider() => NewGo("Slider").AddComponent<Slider>();

    protected static Image NewImage() => NewGo("Image").AddComponent<Image>();

    protected static Sprite NewSprite()
    {
        var texture = new Texture2D(4, 4);
        return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
    }

    /// <summary>A choice-button prefab shaped like the one <see cref="DialogueSystem"/> expects:
    /// a <see cref="Button"/> with a child <see cref="TextMeshProUGUI"/> label.</summary>
    protected static GameObject NewChoiceButtonPrefab()
    {
        var root = NewGo("ChoiceButton");
        root.AddComponent<Button>();
        var label = NewGo("Label");
        label.transform.SetParent(root.transform);
        label.AddComponent<TextMeshProUGUI>();
        return root;
    }

    // =====================================================================
    // Component rigs
    // =====================================================================

    protected static RiskManager BuildRiskManager()
    {
        RiskManager risk = AddInactive<RiskManager>(out _, "RiskManager");
        Activate(risk);
        return risk;
    }

    /// <summary>Fully wires a <see cref="ResourceManager"/>. Prefabs/warnings are empty stand-ins;
    /// the driven counter texts and trust slider are stored on the <c>Rm*</c> fields for asserts.</summary>
    protected ResourceManager BuildResourceManager()
    {
        ResourceManager rm = AddInactive<ResourceManager>(out _, "ResourceManager");

        RmPaperText = NewText();
        RmInkText = NewText();
        RmLeafletsText = NewText();
        RmMoneyText = NewText();
        RmTrustSlider = NewSlider();

        SetField(rm, "_paperText", RmPaperText);
        SetField(rm, "_inkText", RmInkText);
        SetField(rm, "_leafletsText", RmLeafletsText);
        SetField(rm, "_moneyText", RmMoneyText);
        SetField(rm, "_trustSlider", RmTrustSlider);

        SetField(rm, "_noPaperWarning", NewGo("noPaper"));
        SetField(rm, "_noInkWarning", NewGo("noInk"));
        SetField(rm, "_noLeafletsWarning", NewGo("noLeaflets"));
        SetField(rm, "_noMoneyWarning", NewGo("noMoney"));

        SetField(rm, "_paperPrefab", NewGo("paperPrefab"));
        SetField(rm, "_inkPrefab", NewGo("inkPrefab"));
        SetField(rm, "_leafletsPrefab", NewGo("leafletsPrefab"));
        SetField(rm, "_moneyPrefab", NewGo("moneyPrefab"));
        SetField(rm, "_packagePrefab", NewGo("packagePrefab"));
        SetField(rm, "_resourceSpawnPoint", NewGo("spawnPoint").transform);

        Activate(rm);
        return rm;
    }

    /// <summary>Wires a <see cref="DialogueSystem"/> and hands back the box root and choices
    /// container so tests can observe visibility and spawned choice buttons.</summary>
    protected DialogueSystem BuildDialogue(out GameObject box, out Transform choicesContainer)
    {
        DialogueSystem dialogue = AddInactive<DialogueSystem>(out _, "DialogueSystem");

        box = NewGo("DialogueBox");
        choicesContainer = NewGo("Choices").transform;

        SetField(dialogue, "_dialogueBox", box);
        SetField(dialogue, "_dialogueText", NewText());
        SetField(dialogue, "_dialogueImage", NewImage());
        SetField(dialogue, "_choiceButtonPrefab", NewChoiceButtonPrefab());
        SetField(dialogue, "_choicesContainer", choicesContainer);
        SetField(dialogue, "_defaultPortrait", NewSprite());

        Activate(dialogue);
        return dialogue;
    }

    /// <summary>Wires an <see cref="InspectionSystem"/> plus its mutually-referencing
    /// <see cref="Npc"/> against an existing <paramref name="risk"/> source.</summary>
    protected InspectionSystem BuildInspection(RiskManager risk, out Npc npc)
    {
        InspectionSystem inspection = AddInactive<InspectionSystem>(
            out var inspectionGo,
            "Inspection"
        );
        npc = AddInactive<Npc>(out var npcGo, "Npc");

        var animatorGo = NewGo("InspectionAnimator");
        Animator animator = animatorGo.AddComponent<Animator>();

        SetField(inspection, "_riskManager", risk);
        SetField(inspection, "_npc", npcGo.transform);
        SetField(inspection, "_inspectionAnimator", animator);
        SetField(inspection, "_waypoints", new List<Transform>());
        SetField(inspection, "_inspectionSounds", new List<AudioClip>());

        SetField(npc, "_inspectionSystem", inspection);

        inspectionGo.SetActive(true);
        npcGo.SetActive(true);
        return inspection;
    }
}
