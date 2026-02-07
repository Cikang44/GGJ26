using UnityEngine;
using Yarn.Unity;

public class DialoguePositionCommands : MonoBehaviour
{
    private static DialoguePositionCommands _instance;
    public static DialoguePositionCommands Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DialoguePositionCommands>();
                if (_instance == null)
                {
                    Debug.LogError("DialoguePositionCommands: No instance found in scene!");
                }
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }
    
    [Header("Dialog UI References")]
    [Tooltip("The RectTransform of the dialogue panel to move")]
    public RectTransform dialoguePanel;
    
    [Tooltip("Optional: Canvas for screen-space calculations")]
    public Canvas dialogueCanvas;
    
    [Header("Animation Settings")]
    [Tooltip("Time to move the dialogue panel")]
    public float moveDuration = 0.3f;
    
    [Tooltip("Should the movement be smooth?")]
    public bool smoothMovement = true;
    
    [Header("Offset Settings")]
    [Tooltip("Vertical offset above character (in world units)")]
    public float characterOffset = 2f;
    
    [Tooltip("Optional padding from screen edges")]
    public Vector2 screenPadding = new Vector2(50f, 50f);
    
    // Private state
    private Vector3 _targetPosition;
    private bool _isMoving;
    private float _moveStartTime;
    private Vector3 _startPosition;
    private Vector3 _defaultPosition;
    private bool _hasDefaultPosition;
    
    private void Start()
    {
        // Store initial position as default
        if (dialoguePanel != null)
        {
            _defaultPosition = dialoguePanel.position;
            _hasDefaultPosition = true;
        }
    }
    
    private void Update()
    {
        if (_isMoving && dialoguePanel != null)
        {
            float elapsed = Time.time - _moveStartTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            
            if (smoothMovement)
            {
                // Smooth easing
                t = Mathf.SmoothStep(0f, 1f, t);
            }
            
            dialoguePanel.position = Vector3.Lerp(_startPosition, _targetPosition, t);
            
            if (t >= 1f)
            {
                _isMoving = false;
            }
        }
    }
    
    /// <summary>
    /// Yarn Command: Move dialog to a character's position
    /// Usage: <<move_dialog_to_character PlayerName>>
    /// </summary>
    [YarnCommand("move_dialog_to_character")]
    public static void MoveDialogToCharacter(string characterName)
    {
        Instance?.MoveDialogToCharacterInstance(characterName);
    }
    
    private void MoveDialogToCharacterInstance(string characterName)
    {
        if (dialoguePanel == null)
        {
            Debug.LogWarning("DialoguePositionCommands: dialoguePanel not assigned!");
            return;
        }
        
        // Find character by name (searches for GameObject with matching name or tag)
        GameObject character = GameObject.Find(characterName);
        
        // If not found by name, try finding by tag
        if (character == null)
        {
            character = GameObject.FindGameObjectWithTag(characterName);
        }
        
        if (character == null)
        {
            Debug.LogWarning($"DialoguePositionCommands: Character '{characterName}' not found!");
            return;
        }
        
        // Get world position and add offset
        Vector3 worldPos = character.transform.position + Vector3.up * characterOffset;
        
        // Convert to screen position
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("DialoguePositionCommands: No main camera found!");
            return;
        }
        
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        
        // Clamp to screen bounds with padding
        screenPos.x = Mathf.Clamp(screenPos.x, screenPadding.x, Screen.width - screenPadding.x);
        screenPos.y = Mathf.Clamp(screenPos.y, screenPadding.y, Screen.height - screenPadding.y);
        
        // Set target position
        MoveDialogToScreenPosition(screenPos);
        
        Debug.Log($"DialoguePositionCommands: Moving dialog to character '{characterName}' at {screenPos}");
    }
    
    /// <summary>
    /// Yarn Command: Move dialog to a specific screen position
    /// Usage: <<move_dialog_to_position 100 200>>
    /// </summary>
    [YarnCommand("move_dialog_to_position")]
    public static void MoveDialogToPosition(float x, float y)
    {
        Instance?.MoveDialogToPositionInstance(x, y);
    }
    
    private void MoveDialogToPositionInstance(float x, float y)
    {
        if (dialoguePanel == null)
        {
            Debug.LogWarning("DialoguePositionCommands: dialoguePanel not assigned!");
            return;
        }
        
        Vector3 screenPos = new Vector3(x, y, 0);
        MoveDialogToScreenPosition(screenPos);
        
        Debug.Log($"DialoguePositionCommands: Moving dialog to position ({x}, {y})");
    }
    
    /// <summary>
    /// Yarn Command: Move dialog to a world position
    /// Usage: <<move_dialog_to_world_position 5.0 3.0 0.0>>
    /// </summary>
    [YarnCommand("move_dialog_to_world_position")]
    public static void MoveDialogToWorldPosition(float x, float y, float z)
    {
        Instance?.MoveDialogToWorldPositionInstance(x, y, z);
    }
    
    private void MoveDialogToWorldPositionInstance(float x, float y, float z)
    {
        if (dialoguePanel == null)
        {
            Debug.LogWarning("DialoguePositionCommands: dialoguePanel not assigned!");
            return;
        }
        
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("DialoguePositionCommands: No main camera found!");
            return;
        }
        
        Vector3 worldPos = new Vector3(x, y, z);
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        
        MoveDialogToScreenPosition(screenPos);
        
        Debug.Log($"DialoguePositionCommands: Moving dialog to world position ({x}, {y}, {z})");
    }
    
    /// <summary>
    /// Yarn Command: Reset dialog to its default position
    /// Usage: <<reset_dialog_position>>
    /// </summary>
    [YarnCommand("reset_dialog_position")]
    public static void ResetDialogPosition()
    {
        Instance?.ResetDialogPositionInstance();
    }
    
    private void ResetDialogPositionInstance()
    {
        if (dialoguePanel == null)
        {
            Debug.LogWarning("DialoguePositionCommands: dialoguePanel not assigned!");
            return;
        }
        
        if (!_hasDefaultPosition)
        {
            Debug.LogWarning("DialoguePositionCommands: No default position stored!");
            return;
        }
        
        MoveDialogToScreenPosition(_defaultPosition);
        Debug.Log("DialoguePositionCommands: Resetting dialog to default position");
    }
    
    private void MoveDialogToScreenPosition(Vector3 screenPosition)
    {
        if (moveDuration <= 0f)
        {
            // Instant move
            dialoguePanel.position = new Vector3(screenPosition.x, screenPosition.y, dialoguePanel.position.z);
            _isMoving = false;
        }
        else
        {
            // Animated move
            _startPosition = dialoguePanel.position;
            _targetPosition = screenPosition;
            _moveStartTime = Time.time;
            _isMoving = true;
        }
    }
    
    [YarnCommand("set_dialog_default_position")]
    public static void SetDialogDefaultPosition()
    {
        Instance?.SetDialogDefaultPositionInstance();
    }
    
    private void SetDialogDefaultPositionInstance()
    {
        if (dialoguePanel != null)
        {
            _defaultPosition = dialoguePanel.position;
            _hasDefaultPosition = true;
            Debug.Log("DialoguePositionCommands: Set default position to current position");
        }
    }
    
    [YarnCommand("move_dialog_relative")]
    public static void MoveDialogRelative(float deltaX, float deltaY)
    {
        Instance?.MoveDialogRelativeInstance(deltaX, deltaY);
    }
    
    private void MoveDialogRelativeInstance(float deltaX, float deltaY)
    {
        if (dialoguePanel == null)
        {
            Debug.LogWarning("DialoguePositionCommands: dialoguePanel not assigned!");
            return;
        }
        
        Vector3 newPosition = dialoguePanel.position + new Vector3(deltaX, deltaY, 0);
        MoveDialogToScreenPosition(newPosition);
        
        Debug.Log($"DialoguePositionCommands: Moving dialog relative by ({deltaX}, {deltaY})");
    }
}
