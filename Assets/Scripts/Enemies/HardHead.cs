using UnityEngine;

public class HardHead : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null && playerRb.linearVelocityY < 0)
            {
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocityX, 5f);

                HealthBehaviour health = playerRb.GetComponent<HealthBehaviour>();
                if (health != null)
                {
                    health.TakeDamage(1);
                }
            }
        }
    }
}