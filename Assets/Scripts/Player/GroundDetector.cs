using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    public Collider2D detectorCollider;
    public bool isGrounded = false;
    private int _groundCount = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") || collision.CompareTag("Moving Platform"))
        {
            isGrounded = true;
            _groundCount++;
            if (collision.CompareTag("Moving Platform"))
            {
                transform.SetParent(collision.transform, true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") || collision.CompareTag("Moving Platform"))
        {
            _groundCount--;
            isGrounded = _groundCount > 0;
            if (collision.CompareTag("Moving Platform"))
            {
                transform.SetParent(null, true);
            }
        }
    }
}