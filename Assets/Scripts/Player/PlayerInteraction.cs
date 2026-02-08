using UnityEngine;

/// <summary>
/// Handles player interaction with enemies and initiating battles
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Key to press to initiate battle with nearby enemy")]
    public KeyCode interactKey = KeyCode.E;

    [Tooltip("Detection radius for finding enemies")]
    [Min(0.1f)] public float detectionRadius = 3f;

    [Tooltip("Layer mask for enemy detection")]
    public LayerMask enemyLayer;

    [Header("UI (Optional)")]
    [Tooltip("UI indicator when near enemy (optional)")]
    public GameObject interactionPrompt;

    private Enemy _nearbyEnemy;
    private bool _canInteract = false;

    private void Update()
    {
        CheckForNearbyEnemies();

        if (_canInteract && Input.GetKeyDown(interactKey))
        {
            InitiateBattle();
        }

        UpdateInteractionPrompt();
    }

    private void CheckForNearbyEnemies()
    {
        // Find all colliders within detection radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);    

        Enemy closestEnemy = null;
        float closestDistance = float.MaxValue;

        _canInteract = false;
        // Find the closest enemy
        foreach (Collider2D collider in hitColliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();

            if (enemy != null)
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                    _canInteract = true;
                }
            }

            if (collider.TryGetComponent<Interactable>(out var interactable))
            {
                if (interactable.isInteractable)
                {
                    _canInteract = true;
                }
            }
        }

        _nearbyEnemy = closestEnemy;
    }

    private void InitiateBattle()
    {
        if (_nearbyEnemy != null && RapManager.Instance != null)
        {
            RapManager.Instance.StartBattle(_nearbyEnemy);
        }
    }

    private void UpdateInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(_canInteract);
        }
    }

    // Visualize detection radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
