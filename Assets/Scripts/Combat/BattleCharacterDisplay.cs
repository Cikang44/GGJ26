using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays character sprites during battle (mirrors the real characters)
/// </summary>
public class BattleCharacterDisplay : MonoBehaviour
{
    [Header("Character")]
    [Tooltip("Is this the player or opponent?")]
    public bool isPlayer = true;
    
    [Header("Visual")]
    [Tooltip("Character sprite renderer or image")]
    public Image characterImage;
    
    [Tooltip("Idle sprite")]
    public Sprite idleSprite;
    
    [Tooltip("Singing sprite")]
    public Sprite singingSprite;
    
    [Tooltip("Miss sprite")]
    public Sprite missSprite;
    
    [Header("Animation")]
    [Tooltip("How long to show singing animation")]
    public float singDuration = 0.2f;
    
    [Tooltip("Scale bounce effect on note hit")]
    public bool enableBounce = true;
    
    [Tooltip("Bounce scale multiplier")]
    public float bounceScale = 1.1f;
    
    private float _lastSingTime;
    private Vector3 _originalScale;
    private Animator _animator; // Optional: if you want to use Animator instead

    private void Start()
    {
        _originalScale = transform.localScale;
        _animator = GetComponent<Animator>();
        
        SetIdle();
    }

    private void Update()
    {
        // Return to idle after sing duration
        if (Time.time - _lastSingTime > singDuration)
        {
            SetIdle();
        }
        
        // Smooth bounce back to original scale
        if (enableBounce)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _originalScale, Time.deltaTime * 10f);
        }
    }

    public void OnNoteSing()
    {
        _lastSingTime = Time.time;
        
        if (_animator != null)
        {
            _animator.SetTrigger("Sing");
        }
        else if (characterImage != null && singingSprite != null)
        {
            characterImage.sprite = singingSprite;
        }
        
        if (enableBounce)
        {
            transform.localScale = _originalScale * bounceScale;
        }
    }

    public void OnNoteMiss()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("Miss");
        }
        else if (characterImage != null && missSprite != null)
        {
            characterImage.sprite = missSprite;
            Invoke(nameof(SetIdle), 0.3f);
        }
    }

    private void SetIdle()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("Idle");
        }
        else if (characterImage != null && idleSprite != null)
        {
            characterImage.sprite = idleSprite;
        }
    }

    public void SetCharacterSprites(Sprite idle, Sprite sing, Sprite miss = null)
    {
        idleSprite = idle;
        singingSprite = sing;
        if (miss != null)
        {
            missSprite = miss;
        }
        
        SetIdle();
    }
}
