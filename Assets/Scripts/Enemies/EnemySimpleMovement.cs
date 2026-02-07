using UnityEngine;

public class EnemySimpleMovement : MonoBehaviour
{
    [Tooltip("Movement speed of the enemy")] 
    public float moveSpeed = 2f;
    private Vector2 _direction = Vector2.left;

    private void Update()
    { 
        transform.Translate(_direction * moveSpeed * Time.deltaTime);
        if (Physics2D.Raycast(transform.position, _direction, 0.1f))
        {
            _direction = -_direction;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
}