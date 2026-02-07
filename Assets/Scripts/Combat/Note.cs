using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Individual note that moves towards a receptor
/// </summary>
public class Note : MonoBehaviour
{
    [Header("Note Data")]
    public float hitTime; // When this note should be hit (in milliseconds)
    public RapManager.NoteDirection direction;
    public float sustainLength; // Hold duration in milliseconds
    public bool isPlayerNote;
    
    [Header("References")]
    public Image noteImage;
    public GameObject sustainTail; // Visual for hold notes
    
    [Header("Movement")]
    [Tooltip("Speed multiplier for note movement")]
    public float scrollSpeed = 1f;
    
    private NoteReceptor _targetReceptor;
    private Vector3 _startPosition;
    private bool _hasBeenHit;
    private bool _hasMissed;
    private float _songStartTime;
    private float _travelTime; // Time it takes for note to reach receptor
    private RectTransform _rectTransform;
    private RectTransform _sustainTailRect;

    public bool HasBeenHit => _hasBeenHit;
    public bool HasMissed => _hasMissed;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (sustainTail != null)
        {
            _sustainTailRect = sustainTail.GetComponent<RectTransform>();
        }
    }

    public void Initialize(RapManager.Note noteData, NoteReceptor receptor, float scrollSpeed, Vector3 startPos, float songStartTime, float travelTime)
    {
        this.hitTime = noteData.time;
        this.direction = noteData.GetDirection();
        this.sustainLength = noteData.sustainLength;
        this.isPlayerNote = noteData.IsPlayerNote();
        this.scrollSpeed = scrollSpeed;
        
        _targetReceptor = receptor;
        _startPosition = startPos;
        _songStartTime = songStartTime;
        _travelTime = travelTime;
        
        transform.position = startPos;
        
        // Setup sustain tail if this is a hold note
        if (sustainTail != null)
        {
            sustainTail.SetActive(sustainLength > 0);
            if (sustainLength > 0 && _sustainTailRect != null)
            {
                // Scale tail based on sustain length
                float tailHeight = sustainLength * scrollSpeed * 0.001f;
                _sustainTailRect.sizeDelta = new Vector2(_sustainTailRect.sizeDelta.x, tailHeight);
            }
        }
        
        // Set color based on direction
        if (noteImage != null)
        {
            noteImage.color = GetColorForDirection(direction);
        }
    }

    private void FixedUpdate()
    {
        if (_hasBeenHit || _targetReceptor == null) return;
        
        // Calculate position based on absolute song time
        float currentSongTime = (Time.time - _songStartTime) * 1000f; // Song time in milliseconds
        float timeUntilHit = hitTime - currentSongTime; // How much time until this note should be hit
        float progress = 1f - (timeUntilHit / _travelTime); // Progress from 0 to 1
        
        // Clamp progress to prevent overshooting
        progress = Mathf.Clamp01(progress);
        
        transform.position = Vector3.Lerp(_startPosition, _targetReceptor.GetPosition(), progress);
        
        // Check if note has passed the receptor
        if (currentSongTime > hitTime + 200f && !_hasMissed) // 200ms grace period
        {
            if (isPlayerNote)
            {
                Miss(); // Player missed
            }
            else
            {
                AutoHit(); // Enemy note auto-hits
            }
        }
    }

    public void Hit()
    {
        if (_hasBeenHit || _hasMissed) return;
        
        _hasBeenHit = true;
        
        // Visual feedback
        if (_targetReceptor != null)
        {
            _targetReceptor.ShowHitFeedback();
        }
        
        // Destroy or hide note
        Destroy(gameObject);
    }

    public void AutoHit()
    {
        if (_hasBeenHit || _hasMissed) return;
        
        _hasBeenHit = true;
        
        // Visual feedback for opponent
        if (_targetReceptor != null)
        {
            _targetReceptor.ShowHitFeedback();
        }
        
        // Just destroy note, don't affect health
        Destroy(gameObject);
    }
    
    public void Miss()
    {
        if (_hasBeenHit || _hasMissed) return;
        
        _hasMissed = true;
        
        // Notify RapManager that player missed
        if (RapManager.Instance != null)
        {
            RapManager.Instance.OnPlayerNoteMiss();
        }
        
        // Fade out and destroy
        if (noteImage != null)
        {
            Color c = noteImage.color;
            c.a = 0.3f;
            noteImage.color = c;
        }
        
        Destroy(gameObject);
    }

    public float GetDistanceToReceptor()
    {
        if (_targetReceptor == null) return float.MaxValue;
        return Vector3.Distance(transform.position, _targetReceptor.GetPosition());
    }

    private Color GetColorForDirection(RapManager.NoteDirection dir)
    {
        switch (dir)
        {
            case RapManager.NoteDirection.Left:
                return new Color(0.8f, 0.2f, 0.8f); // Purple
            case RapManager.NoteDirection.Down:
                return new Color(0.2f, 0.8f, 0.8f); // Cyan
            case RapManager.NoteDirection.Up:
                return new Color(0.2f, 0.8f, 0.2f); // Green
            case RapManager.NoteDirection.Right:
                return new Color(0.8f, 0.2f, 0.2f); // Red
            default:
                return Color.white;
        }
    }
}
