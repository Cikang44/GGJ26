using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GroundDetector))]
public class PlayerMovement : MonoBehaviour
{
    public static bool isInControl = true;

    [Min(0)] public float speed = 3;
    [Min(0)] public float jumpHeight = 3;
    [Min(0)] public float boostSpeed = 10;
    [Min(0)] public float boostHeight = 3;
    [Min(0)] public float boostDelay = 1;
    private Rigidbody2D _playerRb;
    private GroundDetector _groundDetector;
    private Animator _animator;
    private bool _isJumpOnCooldown = false;
    public ParticleSystem boostParticleSystem;
    [HideInInspector] public bool isOnBoost = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _groundDetector = GetComponent<GroundDetector>();
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInControl) return;
        int direction = 0;
        if (Input.GetKey(KeyCode.A)) direction -= 1;
        if (Input.GetKey(KeyCode.D)) direction += 1;
        Move(direction);

        if (Input.GetKeyDown(KeyCode.W)) Jump();

        _animator.SetInteger("Direction", direction);
        _animator.SetBool("Grounded", _groundDetector.isGrounded);
    }

    private void Move(int direction)
    {
        _playerRb.linearVelocityX = direction * speed;
    }

    private void Jump()
    {
        if (_groundDetector.isGrounded && !_isJumpOnCooldown && !isOnBoost)
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

    public void Boost(Vector3 startPosition)
    {
        StartCoroutine(BoostCoroutine(startPosition));
    }
    private IEnumerator BoostCoroutine(Vector3 startPosition)
    {
        isOnBoost = true;
        transform.position = startPosition;
        _playerRb.constraints |= RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;

        yield return new WaitForSeconds(boostDelay);

        _playerRb.constraints ^= RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;

        boostParticleSystem.Play();
        float timePassed = 0f;
        while (timePassed < boostHeight / boostSpeed)
        {
            yield return null;
            timePassed += Time.deltaTime;
            _playerRb.linearVelocityY = boostSpeed;
        }
        _playerRb.linearVelocityY = boostSpeed / 2;
        isOnBoost = false;
        boostParticleSystem.Stop();
    }
}
