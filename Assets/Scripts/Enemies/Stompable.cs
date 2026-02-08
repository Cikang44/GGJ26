using UnityEngine;

public class Stompable : MonoBehaviour
{
    [SerializeField] private float bounceForce = 10f;
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            
            // Check if player is above the enemy and falling down
            bool isPlayerAbove = collision.transform.position.y > transform.position.y + 0.5f;
            bool isPlayerFalling = playerRb != null && playerRb.linearVelocityY < 0;
            
            if (isPlayerAbove && isPlayerFalling)
            {
                // Enemy dies, player bounces
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocityX, bounceForce);
                Destroy(gameObject);
            }
            else
            {
                // Player takes damage from side or bottom collision
                HealthBehaviour playerHealth = collision.gameObject.GetComponent<HealthBehaviour>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(1);
                }
            }
        }
    }
}