using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    public Collider2D detectorCollider;
    public bool isGrounded = false;
    private int _groundCount = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            isGrounded = true;
            _groundCount++;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            _groundCount--;
            isGrounded = _groundCount > 0;
        }
    }
}