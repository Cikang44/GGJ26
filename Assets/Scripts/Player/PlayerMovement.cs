using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GroundDetector))]
public class PlayerMovement : MonoBehaviour
{
    [Min(0)] public float speed = 3;
    [Min(0)] public float jumpHeight = 3;
    [Min(0)] public float boostSpeed = 10;
    [Min(0)] public float boostHeight = 3;
    private Rigidbody2D _playerRb;
    private GroundDetector _groundDetector;
    private bool _isJumpOnCooldown = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _groundDetector = GetComponent<GroundDetector>();
    }

    // Update is called once per frame
    void Update()
    {
        float direction = 0;
        if (Input.GetKey(KeyCode.A)) direction -= 1;
        if (Input.GetKey(KeyCode.D)) direction += 1;
        Move(direction);

        if (Input.GetKeyDown(KeyCode.W)) Jump();
    }

    private void Move(float direction)
    {
        _playerRb.linearVelocityX = direction * speed;
    }

    private void Jump()
    {
        if (_groundDetector.isGrounded && !_isJumpOnCooldown)
        {
            if (_playerRb.linearVelocityY < 0) _playerRb.linearVelocityY = 0;
            _playerRb.linearVelocityY += Mathf.Sqrt(2 * Physics2D.gravity.magnitude * jumpHeight);
            StartCoroutine(JumpCooldown());
        }
    }

    private static readonly WaitForSeconds _waitForSeconds0_1 = new(0.1f);
    private IEnumerator JumpCooldown()
    {
        _isJumpOnCooldown = true;
        yield return _waitForSeconds0_1;
        _isJumpOnCooldown = false;
    }

    public void Boost()
    {
        StartCoroutine(BoostCoroutine());
    }
    private IEnumerator BoostCoroutine()
    {
        _isJumpOnCooldown = true;
        float timePassed = 0f;
        while (timePassed < boostHeight / boostSpeed)
        {
            yield return null;
            timePassed += Time.deltaTime;
            _playerRb.linearVelocityY = boostSpeed;
        }
        _playerRb.linearVelocityY = boostSpeed / 2;
        _isJumpOnCooldown = false;
    }
}
