using UnityEngine;


public class Parallax : MonoBehaviour
{
    [Range(0f, 1f)]
    [Tooltip("0 = background is static, 1 = moves with camera (no parallax)")]
    public float parallaxFactor = 0.5f;

    private Camera _camera;
    private Transform _cameraTransform;
    private SpriteRenderer _spriteRenderer;
    
    private Vector3 _startPosition;
    private Vector2 _backgroundSize;
    private Vector2 _cameraSize;
    private Vector2 _travelLimit;

    void Start()
    {
        _camera = Camera.main;
        _cameraTransform = _camera.transform;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _startPosition = transform.position;
        
        CalculateBounds();
    }

    void CalculateBounds()
    {
        // Get background size
        if (_spriteRenderer != null && _spriteRenderer.sprite != null)
        {
            _backgroundSize = _spriteRenderer.bounds.size;
        }
        
        // Get camera viewport size in world units
        _cameraSize.y = _camera.orthographicSize * 2f;
        _cameraSize.x = _cameraSize.y * _camera.aspect;
        
        // Calculate maximum travel distance before revealing edges
        // This is half the difference between background and camera size
        _travelLimit.x = Mathf.Max(0f, (_backgroundSize.x - _cameraSize.x) * 0.5f);
        _travelLimit.y = Mathf.Max(0f, (_backgroundSize.y - _cameraSize.y) * 0.5f);
    }

    void LateUpdate()
    {
        if (_camera == null || _cameraTransform == null)
            return;

        Vector3 targetPosition = transform.position;

        // Handle X axis
        if (_travelLimit.x > 0f)
        {
            // Apply parallax within bounds
            float cameraDeltaX = _cameraTransform.position.x - _startPosition.x;
            float parallaxOffsetX = cameraDeltaX * (1f - parallaxFactor);
            float clampedOffsetX = Mathf.Clamp(parallaxOffsetX, -_travelLimit.x, _travelLimit.x);
            
            targetPosition.x = _startPosition.x + clampedOffsetX;
            
            // If we hit the limit, update start position to follow camera
            if (Mathf.Abs(parallaxOffsetX) > _travelLimit.x)
            {
                float excessMovement = (parallaxOffsetX > 0) 
                    ? parallaxOffsetX - _travelLimit.x 
                    : parallaxOffsetX + _travelLimit.x;
                _startPosition.x += excessMovement;
            }
        }
        else
        {
            // Background too small, follow camera completely
            targetPosition.x = _cameraTransform.position.x;
        }

        // Handle Y axis
        if (_travelLimit.y > 0f)
        {
            // Apply parallax within bounds
            float cameraDeltaY = _cameraTransform.position.y - _startPosition.y;
            float parallaxOffsetY = cameraDeltaY * (1f - parallaxFactor);
            float clampedOffsetY = Mathf.Clamp(parallaxOffsetY, -_travelLimit.y, _travelLimit.y);
            
            targetPosition.y = _startPosition.y + clampedOffsetY;
            
            // If we hit the limit, update start position to follow camera
            if (Mathf.Abs(parallaxOffsetY) > _travelLimit.y)
            {
                float excessMovement = (parallaxOffsetY > 0) 
                    ? parallaxOffsetY - _travelLimit.y 
                    : parallaxOffsetY + _travelLimit.y;
                _startPosition.y += excessMovement;
            }
        }
        else
        {
            // Background too small, follow camera completely
            targetPosition.y = _cameraTransform.position.y;
        }

        transform.position = targetPosition;
    }

    void OnDrawGizmosSelected()
    {
        if (_camera == null) _camera = Camera.main;
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Draw background bounds
        Gizmos.color = Color.green;
        if (_spriteRenderer != null)
        {
            Gizmos.DrawWireCube(transform.position, _spriteRenderer.bounds.size);
        }
        
        // Draw safe area (where background can move)
        Gizmos.color = Color.yellow;
        Vector2 safeArea = new Vector2(_travelLimit.x * 2f, _travelLimit.y * 2f);
        Gizmos.DrawWireCube(_startPosition, safeArea);
    }
}