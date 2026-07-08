using NUnit.Framework;
using UnityEngine;

/// <summary>PlayMode coverage for <see cref="InteractableObject"/>: enabling/disabling toggles its
/// renderer and colliders, and the <c>IsHidden</c> setter routes through that same visibility flip.</summary>
public class InteractableObjectPlayTests : PlayModeTestBase
{
    private InteractableObject BuildInteractable(out SpriteRenderer renderer)
    {
        InteractableObject interactable = AddInactive<InteractableObject>(
            out var go,
            "Interactable"
        );
        go.AddComponent<BoxCollider2D>(); // DisableObject requires at least one collider
        Activate(interactable);
        renderer = interactable.GetComponent<SpriteRenderer>();
        return interactable;
    }

    [Test]
    public void DisableObject_HidesRenderer()
    {
        InteractableObject interactable = BuildInteractable(out SpriteRenderer renderer);

        bool disabled = interactable.DisableObject();

        Assert.IsTrue(disabled);
        Assert.IsFalse(renderer.enabled);
    }

    [Test]
    public void EnableObject_ShowsRenderer()
    {
        InteractableObject interactable = BuildInteractable(out SpriteRenderer renderer);
        interactable.DisableObject();

        bool enabled = interactable.EnableObject();

        Assert.IsTrue(enabled);
        Assert.IsTrue(renderer.enabled);
    }

    [Test]
    public void IsHidden_TogglesVisibilityAndState()
    {
        InteractableObject interactable = BuildInteractable(out SpriteRenderer renderer);

        interactable.IsHidden = true;
        Assert.IsTrue(interactable.IsHidden);
        Assert.IsFalse(renderer.enabled);

        interactable.IsHidden = false;
        Assert.IsFalse(interactable.IsHidden);
        Assert.IsTrue(renderer.enabled);
    }
}
