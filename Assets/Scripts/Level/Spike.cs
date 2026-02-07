using UnityEngine;

public class Spike : MonoBehaviour
{
    public bool canLockPosition = true;
    public Vector2 lockPositionOffset;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameObject player = collision.gameObject;
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

            if (!playerMovement.isOnBoost)
            {
                if (canLockPosition)
                {
                    Vector3 lockPosition = transform.position;
                    lockPosition.x += lockPositionOffset.x;
                    lockPosition.y += lockPositionOffset.y;
                    playerMovement.Boost(lockPosition);
                }
                else
                {
                    playerMovement.Boost(player.transform.position);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 lockPosition = transform.position;
        lockPosition.x += lockPositionOffset.x;
        lockPosition.y += lockPositionOffset.y;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(lockPosition, 0.1f);
    }
}