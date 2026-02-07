using UnityEngine;

public class EnemySimpleMovement : MonoBehaviour
{
    [Tooltip("Movement speed of the enemy")] 
    public float moveSpeed = 2f;
    private Vector2 _direction = Vector2.left;
    public float rightBoundary = 5f;
    public float leftBoundary = -5f;

    public float idleTimer = 2f;

    private Animator _animator;
    private Rigidbody2D _rigidbody2D;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {   
        if (PlayerMovement.isInControl == false) return;

        if (idleTimer > 0)
        {
            idleTimer -= Time.deltaTime;
            _rigidbody2D.linearVelocity = new Vector2(0, _rigidbody2D.linearVelocity.y);
            _animator.SetFloat("Direction", 0);
            return;
        }

        _rigidbody2D.linearVelocity = _direction * moveSpeed;
        // if (Physics2D.Raycast(transform.position, _direction, 0.1f))
        // {
        //     _direction = -_direction;
        //     Vector3 scale = transform.localScale;
        //     scale.x *= -1;
        //     transform.localScale = scale;
        //     idleTimer = 2f;
        //     _animator.SetFloat("Direction", _direction.x);
        // }
        
        if (transform.position.x > rightBoundary && _direction == Vector2.right)
        {
            _direction = Vector2.left;
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
            idleTimer = 2f;
            _animator.SetFloat("Direction", _direction.x);
        }
        else if (transform.position.x < leftBoundary && _direction == Vector2.left)
        {
            _direction = Vector2.right;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
            idleTimer = 2f;
            _animator.SetFloat("Direction", _direction.x);
        }
    }
}