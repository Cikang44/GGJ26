using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Enemy behavior that can enter battle mode when player is nearby
/// </summary>
[RequireComponent(typeof(HealthBehaviour))]
public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [Tooltip("Enemy name displayed in battle")]
    public string enemyName = "Enemy";
    
    [Tooltip("Difficulty level affects note patterns")]
    [Range(1, 5)] public int difficultyLevel = 1;
    
    [Header("Battle Settings")]
    [Tooltip("FNF Chart JSON file for this enemy")]
    public TextAsset battleChartAsset;
    
    [Tooltip("Character sprite for battle UI")]
    public Sprite enemyBattleSprite;
    
    [Tooltip("Detection radius for visual indicator")]
    [Min(0f)] public float detectionRadius = 5f;
    
    [Header("Visual Feedback")]
    [Tooltip("Visual indicator when player is nearby (optional)")]
    public GameObject detectionIndicator;
    
    [Header("Events")]
    public UnityEvent OnBattleStart;
    public UnityEvent OnBattleEnd;
    public UnityEvent OnDefeat;
    
    private HealthBehaviour _healthBehaviour;
    private Transform _playerTransform;
    private bool _isInBattle = false;
    private bool _isDefeated = false;
    
    public bool IsDefeated => _isDefeated;
    public bool IsInBattle => _isInBattle;

    private void Start()
    {
        _healthBehaviour = GetComponent<HealthBehaviour>();
        
        // Subscribe to death event
        if (_healthBehaviour != null)
        {
            _healthBehaviour.OnDeath.AddListener(HandleDefeat);
        }
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
        
        if (detectionIndicator != null)
        {
            detectionIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        if (_isDefeated || _isInBattle) return;
        
        CheckPlayerProximity();
    }

    private void CheckPlayerProximity()
    {
        if (_playerTransform == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
        bool playerIsNearby = distanceToPlayer <= detectionRadius;
        
        if (detectionIndicator != null)
        {
            detectionIndicator.SetActive(playerIsNearby);
        }
    }

    public void EnterBattle()
    {
        _isInBattle = true;
        OnBattleStart?.Invoke();
        
        if (detectionIndicator != null)
        {
            detectionIndicator.SetActive(false);
        }
    }

    public void ExitBattle(bool wasDefeated)
    {
        _isInBattle = false;
        OnBattleEnd?.Invoke();
        
        if (wasDefeated)
        {
            HandleDefeat();
        }
    }

    private void HandleDefeat()
    {
        _isDefeated = true;
        OnDefeat?.Invoke();
        
        // Optional: disable collider or hide enemy
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = false;
        }
    }

    // Visualize detection radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
