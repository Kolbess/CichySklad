/// <summary>
/// Discrete escalation bands the continuous risk value falls into.
/// Ordered from safest (<see cref="Low"/>) to most dangerous (<see cref="Critical"/>).
/// </summary>
public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical,
}
