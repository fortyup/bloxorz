using UnityEngine;

// Ce script doit être ajouté sur la tuile goal. Il déclenche la victoire quand
// un GameObject joueur (tag "Player" ou possédant PlayerMovement) entre.
[RequireComponent(typeof(Collider))]
public class GoalTrigger : MonoBehaviour
{
    private bool triggered = false;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        // Accept either objects tagged "Player" or containing PlayerMovement
        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
        {
            triggered = true;
            if (WinLoseManager.I != null)
                WinLoseManager.I.TriggerWin();
            else
                Debug.LogWarning("WinLoseManager not found in scene. Add a WinLoseManager to handle win/lose.");
        }
    }
}
