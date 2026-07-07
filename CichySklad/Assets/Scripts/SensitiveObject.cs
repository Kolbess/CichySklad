using UnityEngine;

/// <summary>
/// A contraband item. When it overlaps the inspecting NPC's trigger it reports its risk value to
/// that NPC's <see cref="InspectionSystem"/>, nudging the player closer to being caught.
/// </summary>
public class SensitiveItem : MonoBehaviour
{
    [Tooltip("Risk contributed when this item is spotted by the NPC during an inspection.")]
    [SerializeField]
    private int _riskValue = 1;

    public int RiskValue => _riskValue;

    private void OnValidate()
    {
        if (_riskValue < 0)
            _riskValue = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("NPC"))
            return;

        if (collision.TryGetComponent(out Npc npc) && npc.InspectionSystem != null)
            npc.InspectionSystem.DetectSensitiveItem(_riskValue);
    }
}
