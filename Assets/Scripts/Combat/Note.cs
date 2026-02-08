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
    private bool _isHolding; // True when sustain note is being held
    private float _sustainStartTime; // When the sustain started
    private bool _sustainComplete; // True when sustain duration is complete
    private float _lastHealthUpdateTime; // Track when we last gave health for sustain
    private float _initialSustainTailHeight; // Store original tail height
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
                float tailHeight = sustainLength * scrollSpeed * 0.1f;
                _initialSustainTailHeight = tailHeight;
                _sustainTailRect.sizeDelta = new Vector2(_sustainTailRect.sizeDelta.x, tailHeight);
            }
        }
    }

    private void FixedUpdate()
    {
        // Check if sustain is complete
        if (_isHolding && sustainLength > 0)
        {
            float holdDuration = (Time.time - _sustainStartTime) * 1000f; // Duration in milliseconds

            // Gradually decrease sustain tail visual
            if (_sustainTailRect != null && sustainTail != null && sustainTail.activeSelf)
            {
                float remainingDuration = sustainLength - holdDuration;
                float _progress = Mathf.Clamp01(remainingDuration / sustainLength);

                // Scale tail height based on remaining duration
                float currentHeight = _initialSustainTailHeight * _progress;
                _sustainTailRect.sizeDelta = new Vector2(_sustainTailRect.sizeDelta.x, currentHeight);
            }

            // Gradually give health while holding (for player notes)
            if (isPlayerNote && RapManager.Instance != null)
            {
                float timeSinceLastUpdate = Time.time - _lastHealthUpdateTime;
                if (timeSinceLastUpdate >= 0.05f) // Update every 50ms
                {
                    RapManager.Instance.OnPlayerSustainHold(timeSinceLastUpdate * 1000f); // Pass time in ms
                    _lastHealthUpdateTime = Time.time;
                }
            }

            if (holdDuration >= sustainLength)
            {
                _sustainComplete = true;
                _isHolding = false;
                Destroy(gameObject);
                return;
            }
        }

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
                RapManager.Instance.hitSound.Play();
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

        // If this is a sustain note, start holding instead of destroying
        if (sustainLength > 0)
        {
            _isHolding = true;
            _sustainStartTime = Time.time;
            _lastHealthUpdateTime = Time.time;
            // Keep the note visible at the receptor position
            if (_targetReceptor != null)
            {
                transform.position = _targetReceptor.GetPosition();
            }
        }
        else
        {
            // Regular note - destroy immediately
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called when the player releases the note (for sustain notes)
    /// </summary>
    public void Release()
    {
        if (_isHolding)
        {
            _isHolding = false;
            Destroy(gameObject);
        }
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

        // Enemy notes with sustain should also hold and show visual decrease
        if (sustainLength > 0)
        {
            _isHolding = true;
            _sustainStartTime = Time.time;
            // Keep the note visible at the receptor position
            if (_targetReceptor != null)
            {
                transform.position = _targetReceptor.GetPosition();
            }
        }
        else
        {
            Destroy(gameObject);
        }
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
}
