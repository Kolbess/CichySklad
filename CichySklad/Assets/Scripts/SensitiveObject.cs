using UnityEngine;

public class SensitiveItem : MonoBehaviour
{
    public int riskValue = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("NPC"))
        {
            collision.GetComponent<Npc>().GetInspectionSystem().DetectSensitiveItem(riskValue);
            //if risk is too high trigger ochrana bribe
        }
    }
}