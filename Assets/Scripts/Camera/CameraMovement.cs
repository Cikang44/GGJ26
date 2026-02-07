using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float cameraSpeed = 3f;
    public Vector2 playerBounds = new(6, 4);
    private Rigidbody2D _playerRigidbody;
    private Transform _cameraTransform;
    private Rigidbody2D _cameraRb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cameraTransform = GetComponent<Transform>();
        _cameraRb = GetComponent<Rigidbody2D>();
        _playerRigidbody = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerRigidbody == null || PlayerInBounds())
        {
            return;
        }
        float _distanceX = _playerRigidbody.position.x - _cameraTransform.position.x;
        float _distanceY = _playerRigidbody.position.y - _cameraTransform.position.y;

        // Fungsi posisi eksponensial tergantung jarak
        _cameraRb.linearVelocityX = cameraSpeed * _distanceX;
        _cameraRb.linearVelocityY = cameraSpeed * _distanceY;
    }

    private bool PlayerInBounds()
    {
        float xPos = _playerRigidbody.position.x - _cameraRb.position.x;
        float yPos = _playerRigidbody.position.y - _cameraRb.position.y;
        return Mathf.Abs(xPos) <= playerBounds.x / 2 && Mathf.Abs(yPos) <= playerBounds.y / 2;
    }
}
