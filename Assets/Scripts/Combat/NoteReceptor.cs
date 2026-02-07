using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The stationary arrow at the top of the screen that notes move towards
/// </summary>
public class NoteReceptor : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Direction this receptor represents")]
    public RapManager.NoteDirection direction;
    
    [Tooltip("Key to press for this receptor")]
    public KeyCode inputKey;
    
    [Tooltip("Is this for the player or opponent?")]
    public bool isPlayerReceptor = true;
    
    [Header("Visual Feedback")]
    [Tooltip("Normal state sprite/image")]
    public Image arrowImage;
    
    [Tooltip("Color when idle")]
    public Color idleColor = Color.white;
    
    [Tooltip("Color when pressed")]
    public Color pressedColor = Color.cyan;
    
    [Tooltip("Color when note is hit")]
    public Color hitColor = Color.green;
    
    [Header("Glow Effect (Optional)")]
    [Tooltip("Optional glow image that appears on press")]
    public GameObject glowEffect;
    
    private float _pressedTime;
    private float _hitTime;
    private const float PRESSED_DURATION = 0.1f;
    private const float HIT_DURATION = 0.15f;

    private void Start()
    {
        if (arrowImage != null)
        {
            arrowImage.color = idleColor;
        }
        
        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (!isPlayerReceptor) return;
        
        // Check for input
        if (Input.GetKeyDown(inputKey))
        {
            OnPress();
        }
        
        if (Input.GetKeyUp(inputKey))
        {
            OnRelease();
        }
        
        // Update visual states
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (arrowImage == null) return;
        
        // Hit feedback takes priority
        if (Time.time - _hitTime < HIT_DURATION)
        {
            float t = (Time.time - _hitTime) / HIT_DURATION;
            arrowImage.color = Color.Lerp(hitColor, idleColor, t);
        }
        // Then pressed feedback
        else if (Time.time - _pressedTime < PRESSED_DURATION)
        {
            arrowImage.color = pressedColor;
        }
        else
        {
            arrowImage.color = idleColor;
        }
    }

    public void OnPress()
    {
        _pressedTime = Time.time;
        
        if (glowEffect != null)
        {
            glowEffect.SetActive(true);
        }
    }

    public void OnRelease()
    {
        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
        }
    }

    public void ShowHitFeedback()
    {
        _hitTime = Time.time;
        
        if (glowEffect != null)
        {
            glowEffect.SetActive(true);
            Invoke(nameof(HideGlow), HIT_DURATION);
        }
    }

    private void HideGlow()
    {
        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
